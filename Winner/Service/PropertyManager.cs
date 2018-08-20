using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winner;

namespace Winner
{
    public class PropertyManager
    {        
        public Location LOCATION = new Location();        
        public Agent AGENT = new Agent();
       
    }

    }

    public class Agent
    {
        public static string AGENT_TYPE_PC = "PC"; // 검색화면
        public static string AGENT_TYPE_MOBILE = "모바일"; // 검색화면

        public static string[] agents = { AGENT_TYPE_PC, AGENT_TYPE_MOBILE };

        private string type;
        private int length;

        public int GetLength()
        {
            return length;
        }

        public string GetType()
        {
            return type;
        }

        public void SetType(string type)
        {
            this.type = type;
            this.length = agents.Length;
        }

        public void SetRandomAgent()
        {
            type = agents[CommonUtils.GetRandomValue(0, agents.Length - 1)];
        }
    }

    public class Url
    {
        public static string CONST_DEFAULT_PC_START_URL = "https://www.naver.com/";
        public static string CONST_DEFAULT_MOBILE_START_URL = "https://m.naver.com/";
    }

    public class Category
    {
        public static string NONE = "N";
        public static string SEARCH = "S";
        public static string IMAGE = "I";
        public static string BLOG = "B";
        public static string MOVIE = "MV";
        public static string KNOWLEDGE = "K";
        public static string CAFE = "C";
        public static string NEWS = "NE";
        public static string MAP = "MA";


        // PC 메뉴 인덱스
        public int index { get; set; }
        // 이름
        public string name { get; set; }
        // 모바일 메뉴
        public string m_menu { get; set; }
        // 메인메뉴의 클래스
        public string mainClass { get; set; }
        // 코드
        public string code { get; set; }

       private Dictionary<string, string> mDefaultCategory = new Dictionary<string, string>();

        public Category() : this(-1, NONE)
        {
            
        }
          
        public Category(int index, string name) 
        {
            this.index = index;
            this.name = name;

            initDefaultCategory();
        }

        private string[] categoryNameArray = { "통합검색", "이미지", "블로그", "동영상", "지식IN", "카페", "뉴스", "지도" };

        private void initDefaultCategory()
        {            
            mDefaultCategory.Add("없음", string.Format("{0},{1},{2},{3},{4}", "-1", "없음", "", "", NONE));            
            mDefaultCategory.Add(categoryNameArray[0], string.Format("{0},{1},{2},{3},{4}", "0", categoryNameArray[0], "where=m", "mn_blog", SEARCH));
            mDefaultCategory.Add(categoryNameArray[1], string.Format("{0},{1},{2},{3},{4}", "2", categoryNameArray[1], "where=m_image", "mn_blog", IMAGE));
            mDefaultCategory.Add(categoryNameArray[2], string.Format("{0},{1},{2},{3},{4}", "3", categoryNameArray[2], "where=m_blog", "mn_blog", BLOG));
            mDefaultCategory.Add(categoryNameArray[3], string.Format("{0},{1},{2},{3},{4}", "1", categoryNameArray[3], "where=m_video", "mn_kin", MOVIE));
            mDefaultCategory.Add(categoryNameArray[4], string.Format("{0},{1},{2},{3},{4}", "5", categoryNameArray[4], "where=m_kin", "mn_kin", KNOWLEDGE));
            mDefaultCategory.Add(categoryNameArray[5], string.Format("{0},{1},{2},{3},{4}", "6", categoryNameArray[5], "where=m_cafe", "mn_cafe", CAFE));
            mDefaultCategory.Add(categoryNameArray[6], string.Format("{0},{1},{2},{3},{4}", "4", categoryNameArray[6], "where=m_news", "mn_news", NEWS));
            mDefaultCategory.Add(categoryNameArray[7], string.Format("{0},{1},{2},{3},{4}", "13", categoryNameArray[7], "where=m_map", "mn_news", MAP));
        }

        public string GetRandomCategoryName()
        {
            return categoryNameArray[CommonUtils.GetRandomValue(0, categoryNameArray.Length - 1)];
        }

        public Dictionary<string, string> GetDefaultCategory()
        {           
            return mDefaultCategory;
        }

        public Category GetCategoryByName(string name)
        {            
            if ( name.Equals("랜덤"))
            {
                name = GetRandomCategoryName();
            }
           
            SetCategory(name);

            return this;
        }
   
        public Category GetCategory() {        
            return this;
        }

        internal void SetCategory(string category)
        {          
            string[] c = mDefaultCategory[ category].Split( CommonUtils.delimiterComma);
            index = int.Parse( c[0]);
            name = c[1];
            m_menu = c[2];
            mainClass = c[3];
            code = c[4];
        }
    }

    public class Location
    {    
        public Category CATEGORY = new Category();

        public static string HOME = "H";
        public static string CAFE_HOME = "HC";
        public static string KNOWLEDGE_HOME = "HK";
        public static string NEWS_HOME = "HN";
        public static string MAP_HOME = "HM";
        public static string BLOG_HOME = "HB";

        public static string SEARCH = "S";
        public static string SEARCH_IMAGE = "SI";
        public static string SEARCH_BLOG = "SB";
        public static string SEARCH_MOVIE = "SM";
        public static string SEARCH_KNOWLEDGE = "SK";
        public static string SEARCH_CAFE = "SC";
        public static string SEARCH_NEWS = "SN";
        public static string SEARCH_MAP = "SMP";

        public static string ETC = "E";

        private string location = HOME;
    
        public void SetLocation( string location)
        {
            if (location.Equals(HOME))
            {
                CATEGORY.SetCategory("없음");
            }

            this.location = location;
        }

    public string GetLocation()
    {
        return location;
    }
}


