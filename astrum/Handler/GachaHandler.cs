﻿using Astrum.Http;
using Astrum.Json.Gacha;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrum.Handler
{
    class GachaHandler
    {
        private AstrumClient _client = null;

        public GachaHandler(AstrumClient client)
        {
            _client = client;
        }

        public bool Start()
        {
            initGachaList();
            _client.ViewModel.History = "";

            return _client.ViewModel.IsGachaEnable;
        }


        private void initGachaList()
        {
            var gachaList = new List<GachaInfo>();
            var stockMap = new Dictionary<string, int>();


            initGachaType(gachaList, stockMap, "normal", null);
            initGachaType(gachaList, stockMap, "raid", null);
            initGachaType(gachaList, stockMap, "platinum", "other");

            _client.ViewModel.GachaList = gachaList;
        }
        
        private void initGachaType(List<GachaInfo> gachaList, Dictionary<string, int> stockMap, string type, string subType)
        {
            var gacha = GachaListInfo(type, subType);
            stockMap["coin"] = gacha.stock.coin;
            stockMap["gacha"] = gacha.stock.gacha;

            if (gacha.stock.ticket != null)
            {
                foreach (var key in gacha.stock.ticket.Keys)
                {
                    stockMap[key] = gacha.stock.ticket[key];
                }
            }

            foreach (var item in gacha.list)
            {
                var key = "ticket".Equals(item.price.type) ? item.price._id : item.price.type;

                if (!"coin".Equals(key))
                {
                    item.stock = stockMap[key];
                    gachaList.Add(item);
                }
            }
        }
        
        private GachaList GachaListInfo(string type,string subType)
        {
            var url = "http://astrum.amebagames.com/_/gacha?type=" + type;

            if(subType != null)
            {
                url += "&subType=" + subType;
            }
            
            var result = _client.GetXHR(url);
            return JsonConvert.DeserializeObject<GachaList>(result);
        }

        public void Run(string gachaId, bool sequence)
        {
            var result = GachaResult(gachaId, sequence);

            InfoPrinter.PrintGachaResult(result, _client.ViewModel);
            InfoUpdater.UpdateGachaResult(result, _client.ViewModel);

            initGachaList();
        }
        
        private GachaResult GachaResult(string _id, bool sequence)
        {
            var values = new Dictionary<string, object>
                {
                   { "_id", _id }
                };

            if (sequence)
            {
                values.Add("sequence", true);
            }

            var result = _client.PostXHR("http://astrum.amebagames.com/_/gacha", values);
            return JsonConvert.DeserializeObject<GachaResult>(result);
        }
    }
}
