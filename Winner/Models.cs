using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace Naver.SearchAd
{
    public class ComboItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public ComboItem(string key, string value)
        {
            Key = key; Value = value;
        }
        public override string ToString()
        {
            return Value;
        }
    }

    // 순서 중요함 필드순서 변경하면 안됨
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

    public class Logic
    {
        
        public string id { get; set; }
        public string name { get; set; }
    
        public static string Column = "Id, Name";
        public static string TableName = "Logic";

        internal static List<Logic> MakeResultSet(SQLiteDataReader reader)
        {
            List<Logic> logics = new List<Logic>();

            while (reader.Read())
            {
                Logic logic = new Logic();
                logic.id = (string)reader["Id"];
                logic.name = (string)reader["Name"];                
                logics.Add( logic);
            }

            return logics;
        }
    }

    public class LogicInput
    {        
        public string key { get; set; }
        public string value { get; set; }
        public string logicId { get; set; }    

        public const string CONST_KEYWORD = "Logic.Input.Keyword";
        public const string CONST_STAY = "Logic.Input.Stay";
        public const string CONST_SCROLL = "Logic.Input.Scroll";
        public const string CONST_POST_VIEW = "Logic.Input.Post.View";
        public const string CONST_HISTORY_PREV = "Logic.Input.History.Prev";
        public const string CONST_HISTORY_NEXT = "Logic.Input.History.Next";
        public const string CONST_CATEGORY_MOVE = "Logic.Input.Category.Move";
        public const string CONST_AGNET = "Logic.Input.Agent";
        public const string CONST_BROWSER = "Logic.Input.Browser";
        public const string CONST_DUPLICATE_ADDRESS = "Logic.Input.Duplicate.Address";

        public static string TableName = "LogicInput";
        public static object Column = "Key, Value, LogicId";
        public static string Values = "'{0}','{1}','{2}'";

        internal static List<LogicInput> MakeResultSet(SQLiteDataReader reader)
        {
            List<LogicInput> logicInputs = new List<LogicInput>();

            while (reader.Read())
            {
                LogicInput logicInput = new LogicInput();
                logicInput.key = (string)reader["Key"];
                logicInput.value = (string)reader["Value"];
                logicInput.logicId = (string)reader["LogicId"];                

                logicInputs.Add(logicInput);
            }

            return logicInputs;
        }

        public static List<LogicInput> ConvertMapToObject(Dictionary<string, string> Inputs, string logicId)
        {
            List<LogicInput> LogicInputs = new List<LogicInput>();

            List<string> list = Inputs.Keys.ToList();
            for (var i = 0; i < list.Count; i++)
            {
                string key = list.ElementAt(i);
                string value = Inputs[key];

                LogicInput logicInput = new LogicInput();
                logicInput.logicId = logicId;
                logicInput.key = key;
                logicInput.value = value;

                LogicInputs.Add(logicInput);
            }

            return LogicInputs;
        }


    }

    public class LogicItem
    {                
        public string sequence { get; set; }
        public string action { get; set; }
        public string value { get; set; }
        public string logicId { get; set; }
        
        public static int HEADER_SEQUENCE = 0;
        public static int HEADER_ACTION = 1;
        public static int HEADER_VALUE = 2;

        public static object TableName = "LogicItem";
        public static object Column = "Sequence, Action, Value, LogicId";
        public static string Values = "'{0}','{1}','{2}','{3}'";

        internal static List<LogicItem> MakeResultSet(SQLiteDataReader reader)
        {
            List<LogicItem> logicItems = new List<LogicItem>();

            while (reader.Read())
            {
                LogicItem logicItem = new LogicItem();
                logicItem.sequence = (string)reader["Sequence"];
                logicItem.action = (string)reader["Action"];
                logicItem.value = (string)reader["Value"];
                logicItem.logicId = (string)reader["LogicId"];

                logicItems.Add(logicItem);
            }

            return logicItems;
        }
    }

    public class RankingModel
    {
        public string keyword { get; set; }
        public string subKeyword { get; set; }
        public string rank { get; set; }
        public string checkedAt { get; set; }
        public string more { get; set; }


        
        public static string TableName = "Ranking";
        public static string Column = "Keyword, SubKeyword, Rank, CheckedAt, More";
        public static string Values = "'{0}','{1}','{2}','{3}','{4}'";

        public static List<RankingModel> MakeResultSet(SQLiteDataReader reader)
        {
            List<RankingModel> Rankings = new List<RankingModel>();

            while (reader.Read())
            {
                RankingModel rankingModel = new RankingModel();
                rankingModel.keyword = (string)reader["Keyword"];
                rankingModel.subKeyword = (string)reader["SubKeyword"];
                rankingModel.rank = (string)reader["Rank"];
                rankingModel.checkedAt = (string)reader["CheckedAt"];
                rankingModel.more = (string)reader["More"];

                Rankings.Add( rankingModel);

            }

            return Rankings;

        }
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
        public string logicName { get; set; }        
        public string toCount { get; set; }
        public string currCount { get; set; }
        public string rank { get; set; }
        public string description { get; set; }
        public string createdAt { get; set; }

        public static string Values = "'{0}','{1}','{2}','{3}','{4}','{5}','{6}'";
        public static string Column = "OID, LogicName, CreatedAt, ToCount, CurrCount, Rank, Description";

        public static int AGENT_TYPE_PC = 1;
        public static int AGENT_TYPE_MOBILE = 2;

    }






}
