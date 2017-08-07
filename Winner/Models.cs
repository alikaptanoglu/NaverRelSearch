using System;
using System.Collections.Generic;
using System.Linq;

namespace Naver.SearchAd
{
    public class RelKwdStat
    {
        public string relKeyword { get; set; }
        public string monthlyPcQcCnt { get; set; }
        public string monthlyMobileQcCnt { get; set; }
        public string monthlyAvePcClkCnt { get; set; }
        public string monthlyAveMobileClkCnt { get; set; }
        public string monthlyAvePcCtr { get; set; }
        public string monthlyAveMobileCtr { get; set; }
        public string plAvgDepth { get; set; }
        public string compIdx { get; set; }        
    }

    public class Configuration
    {
        public string key { get; set; }
        public string value { get; set; }
        public string description { get; set; }
        public string owner { get; set; }
        

        public static string Default = "Default";
        public static char[] DELIMITER_CHARS =  { ',' };
        public static char[] DELIMITER_LIST_CHARS = { '|' };

        /// <summary>
        /// 필수설정
        /// </summary>        
        public const string COMMON_SEARCH_STAY = "Common.Search.Stay";
        public const string COMMON_INIT_PAGE_STAY = "Common.Init.Page.Stay";
        public const string COMMON_MAX_PAGE = "Common.Max.Page";
        public const string COMMON_PRE_HISTORY_MOVE_STAY = "Common.Pre.History.Move.Stay";

        /// <summary>
        /// 페이지 설정
        /// </summary>
        public const string PAGE_VIEW_ANOHTERPAGE_USE = "Page.View.AnOhterPage.Use";
        public const string PAGE_VIEW_ANOHTERPAGE = "Page.View.AnOhterPage";
        public const string PAGE_VIEW_ANOHTERPAGE_STAY = "Page.View.AnOhterPage.Stay";
        public const string PAGE_STAY_USE = "Page.Stay.Use";
        public const string PAGE_STAY = "Page.Stay";

        /// <summary>
        /// 검색설정
        /// </summary>
        public const string COMMON_POST_SEARCH_SCROLL_USE = "Common.Post.Search.Scroll.Use";
        public const string COMMON_POST_SEARCH_SCROLL  = "Common.Post.Search.Scroll";
        public const string COMMON_PRE_SEARCH_STAY = "Common.Pre.Search.Stay";

        /// <summary>
        /// 아이피설정
        /// </summary>        
        public const string IP_AGENT_TYPE = "IP_Agent_Type";
        public const string IP_PROXY_POSITION = "IP_Proxy_Position";
        public const string IP_CAHNGE_STAY = "IP_Change_Stay";
        public const string COOKIE_DELETE_POST_STAY = "Cookie_Delete_Post_Stay";

        /// <summary>
        /// 기타설정
        /// </summary>        
        public const string START_PAGE_LIST = "Start_Page_List";
        public const string START_PAGE_STAY = "Start_Page_Stay";


        public static string Values = "'{0}','{1}','{2}','{3}'";
        public static string Column = "Key, Value, Description, Owner";

        public static Dictionary<string, string> ConvertObjectToMap(List<Configuration> configs)
        {
            return configs.ToDictionary(( config) => config.key, (config) => config.value);            
        }

        public static List<Configuration> ConvertMapToObject(Dictionary<string, string> Configs)
        {
            List<Configuration> configs = new List<Configuration>();

            List<string> list = Configs.Keys.ToList();
            for (var i = 0; i < list.Count; i++)
            {
                string key = list.ElementAt(i);
                string value = Configs[key];

                Configuration config = new Configuration();
                config.owner = Configuration.Default;
                config.key = key;
                config.value = value;

                configs.Add(config);
            }

            return configs;
        }
    }
    public class Slot
    {        
        public string OID { get; set; }
        public string browser { get; set; }
        public string agent { get; set; }
        public string category { get; set; }
        public string search { get; set; }
        public string nxSearch { get; set; }
        public string toCount { get; set; }
        public string currCount { get; set; }
        public string View { get; set; }
        public string initRank { get; set; }
        public string currRank { get; set; }
        public string createdAt { get; set; }

        public static string Values = "'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}', '{10}', '{11}'";
        public static string Column = "OID, Category, Search, NxSearch, ToCount, CurrCount, View, InitRank, CurrRank, CreatedAt, Agent, Browser";

        public static int AGENT_TYPE_PC = 1;
        public static int AGENT_TYPE_MOBILE = 2;

    }






}
