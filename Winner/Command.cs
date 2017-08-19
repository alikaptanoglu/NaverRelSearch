using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Naver.SearchAd;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.PhantomJS;
using System.Threading;

namespace Winner
{
    public class Command
    {
        enum NaverTab
        {
            Search = 0,
            Image = 2,
            Blog = 3,
            Movie = 1,
            Knowledge = 5,
            Cafe = 6,
            News = 4
        };        

        private List<LogicItem> items;
        private Dictionary<string, string> inputMap;
        private IWebDriver driver;
        private LogManager logManager;
        private int agentType;

        private static int LOCATION_HOME = 0; // 메인화면
        private static int LOCATION_SEARCH = 1; // 검색화면

        private static int AGENT_TYPE_PC = 10; // 검색화면
        private static int AGENT_TYPE_MOBILE = 20; // 검색화면

        private const string CONST_DEFAULT_PC_START_URL = "https://www.naver.com/";
        private const string CONST_DEFAULT_MOBILE_START_URL = "https://m.naver.com/";

        private int WHERE;
        private int CATEGORY;

        public static string COMMAND_KEYWORD = "키워드";

        public Command(List<LogicItem> items, Dictionary<string, string> inputMap)
        {
            this.items = items;
            this.inputMap = inputMap;

            SelectUpAgent(inputMap[LogicInput.CONST_AGNET]);
        }


        public void doCommand()
        {
            StartBrowser();
            doProcess();
            CloseBrowser();
        }

        private void doProcess()
        {
            foreach (LogicItem item in items)
            {
                ExecuteCommand(item.action, item.value);
            }                                    
        }

        private void SelectUpAgent(string type)
        {
            if (type.Equals("PC"))
            {
                agentType = AGENT_TYPE_PC;
            }
            else if (type.Equals("모바일"))
            {
                agentType = AGENT_TYPE_MOBILE;
            }
            else
            {
                string[] agent = { "PC", "모바일" };
                SelectUpAgent(agent[CommonUtils.GetRandomValue(0, agent.Length - 1)]);
            }
        }


        private void ExecuteCommand(string action, string value)
        {
            string[] v = value.Split(CommonUtils.delimiterSlash);

            DefaultStay();

            switch (action)
            {
                case "키워드":
                    {
                        SetSearchKeyword(v[0], v[1]);

                        if (WHERE == LOCATION_HOME)
                        {
                            if (agentType == AGENT_TYPE_PC)
                            {
                                ClickSearchButton("search_btn");
                            }
                            else
                            {
                                ClickSearchButton("query");
                            }
                            
                        }
                        else if (WHERE == LOCATION_SEARCH)
                        {
                            if (agentType == AGENT_TYPE_PC)
                            {
                                ClickSearchButton("bt_search");
                            }
                            else
                            {
                                ClickSearchButton("nx_query");
                            }
                                
                        }
                    } break;
                case "체류":
                    {
                        Stay(int.Parse(v[0]), int.Parse(v[1]));
                    }
                    break;
                case "히스토리":
                    {

                        if (v[0].Equals("Prev"))
                        {
                            HistoryPrev();
                        }
                        else
                        {
                            HistoryNext();
                        }

                    }
                    break;
                case "스크롤":
                    {
                        int interval = CommonUtils.GetRandomValue(v[0], v[1]) * 1000;
                        int y = CommonUtils.GetRandomValue(v[2], v[3]);
                        int count = CommonUtils.GetRandomValue(v[4], v[5]);
                        doScroll(interval, y, count);
                    }
                    break;
                case "카테고리":
                    {
                        MoveCategory(v[0]);
                    }
                    break;
                case "게시글조회":
                    {
                        bool isSuccess = false;
                        int index = 0;
                        int MAX_SEARCH_PAGE = 10;

                        while (!isSuccess)
                        {
                            isSuccess = PostViewBy(value.Trim(), true);

                            if (isSuccess || index == MAX_SEARCH_PAGE)
                            {
                                break;                                
                            }
                            else
                            {
                                doScroll(100, 200, 20);
                                if (agentType == AGENT_TYPE_PC)
                                {
                                    driver.FindElement(By.CssSelector(".next")).Click();
                                }
                                else
                                {
                                    driver.FindElement(By.CssSelector(".btn_next")).Click();
                                }                                
                            }

                            index++;
                        }                        
                    }
                    break;

            }

            DefaultStay();
        }

        // 게시물 조회
        private bool PostViewBy(string url, bool isSelf)
        {
            IWebElement element = null;
            IReadOnlyCollection<IWebElement> elements = null;

            if (isSelf)
            {
                var jse = (IJavaScriptExecutor)driver;
                jse.ExecuteScript("var array = document.getElementsByTagName('a'); for (var i = 0; i < array.length; i++) { array[i].setAttribute('target', '_self')}", "");
            }

            elements = null;

            if (url != null && url.Length > 0)
            {
                elements = driver.FindElements(By.CssSelector(string.Format("[href*='{0}']", url)));
                if (elements.Count == 0)
                {
                    return false;
                }
            }
            else
            { 
                if (CATEGORY == (int)NaverTab.Blog)
                {
                    elements = agentType == AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".sh_blog_title")) : driver.FindElements(By.CssSelector(".lst_total .bx"));
                }
                else if (CATEGORY == (int)NaverTab.News)
                {
                    elements = agentType == AGENT_TYPE_PC ? driver.FindElements(By.CssSelector("._sp_each_title")) : driver.FindElements(By.CssSelector(".list_news .bx"));
                }
                else if (CATEGORY == (int)NaverTab.Cafe)
                {
                    elements = agentType == AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".sh_cafe_title")) : driver.FindElements(By.CssSelector(".lst_total .bx"));
                }
                else if (CATEGORY == (int)NaverTab.Image)
                {
                    elements = agentType == AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".thumb")) : driver.FindElements(By.CssSelector(".photo_grid a"));
                }
                else if (CATEGORY == (int)NaverTab.Knowledge)
                {
                    elements = agentType == AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".question a")) : driver.FindElements(By.CssSelector(".lst_total a"));
                }
                else if (CATEGORY == (int)NaverTab.Movie)
                {
                    elements = agentType == AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".video_info a")) : driver.FindElements(By.CssSelector(".video_list .info_area a"));
                }
                else if (CATEGORY == (int)NaverTab.Search)
                {
                    if (agentType == AGENT_TYPE_PC)
                    {
                        elements = driver.FindElements(By.CssSelector(".question a, .sh_blog_title, ._sp_each_title, .sh_cafe_title, .sh_booktext_title, lst_img a, .video_thum"));
                    }
                    else
                    {
                        elements = driver.FindElements(By.CssSelector(".list_video .bx, .photo_grid a, .list_news .bx, .sp_ntotal .bx"));
                    }
                }
            }

            
            while (true)
            {
                try
                {
                    Random random = new Random();
                    int selectIndex = random.Next(0, elements.Count - 1);
                    element = elements.ElementAt(selectIndex);
                    element.Click();
                    return true;
                }
                catch (Exception e)
                {                   
                    if (e is InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
        }

        // Text를 통해서 카테고리의 인덱스를 조회
        private int convertStringToTabIndex(string value)
        {
            int selectIndex = -1;

            if (value.Equals("통합검색"))
            {
                selectIndex = (int)NaverTab.Search;
            }
            else if (value.Equals("이미지"))
            {
                selectIndex = (int)NaverTab.Image;
            }
            else if (value.Equals("블로그"))
            {
                selectIndex = (int)NaverTab.Blog;
            }
            else if (value.Equals("동영상"))
            {
                selectIndex = (int)NaverTab.Movie;
            }
            else if (value.Equals("지식IN"))
            {
                selectIndex = (int)NaverTab.Knowledge;
            }
            else if (value.Equals("카페"))
            {
                selectIndex = (int)NaverTab.Cafe;
            }
            else if (value.Equals("뉴스"))
            {
                selectIndex = (int)NaverTab.News;
            }
            else if (value.Equals("랜덤"))
            {
                int[] tabArray = new int[] { (int)NaverTab.Cafe, (int)NaverTab.Knowledge, (int)NaverTab.Movie, (int)NaverTab.Blog, (int)NaverTab.Image, (int)NaverTab.Search, (int)NaverTab.News };
                convertStringToTabIndex( tabArray[CommonUtils.GetRandomValue(0, tabArray.Length - 1)].ToString());                
            }

            return selectIndex;
        }

        private string convertStringToTabHref(int index)
        {
            string href = null;
            string[] allCategory = { "where=m", "where=m_image", "where=m_blog", "where=m_video", "where=m_kin", "where=m_cafe", "where=m_news" };

            if (index == (int)NaverTab.Search)
            {
                return "where=m";
            }
            else if (index == (int)NaverTab.Image)
            {
                return "where=m_image";
            }
            else if (index == (int)NaverTab.Blog)
            {
                return "where=m_blog";
            }
            else if (index == (int)NaverTab.Movie)
            {
                return "where=m_video";
            }
            else if (index == (int)NaverTab.Knowledge)
            {
                return "where=m_kin";
            }
            else if (index == (int)NaverTab.Cafe)
            {
                return "where=m_cafe";
            }
            else if (index == (int)NaverTab.News)
            {
                return "where=m_news";
            }

            return href;
        }

        private void MoveCategory( string category)
        {
            int index = 0;
            IWebElement element = null;
            index = convertStringToTabIndex(category);

            if (agentType == AGENT_TYPE_PC)
            {
                element = driver.FindElement(By.CssSelector(string.Format(".lnb_menu li.lnb{0}", index)));

                try
                {
                    logManager.AppendLog("{0} 카테고리로 이동합니다.", category);
                    element.Click();
                }
                catch (Exception e)
                {
                    driver.FindElement(By.XPath("//*[@id='_nx_lnb_more']/a")).Click();
                    Thread.Sleep(300);
                    element.Click();
                }
            }
            else
            {
                string value = convertStringToTabHref(index);
                element = driver.FindElement(By.CssSelector(string.Format("li a[href*='{0}']", value)));
                doScroll(1000, -200, 1);
                logManager.AppendLog("{0} 카테고리로 이동합니다.", category);
                element.Click();
            }

            CATEGORY = index;
        }

        // 이전 히스토리로 이동
        private void HistoryPrev()
        {
            var jse = (IJavaScriptExecutor)driver;
            jse.ExecuteScript("window.history.go(-1)", "");
        }

        // 이전 히스토리로 이동
        private void HistoryNext()
        {
            var jse = (IJavaScriptExecutor)driver;
            jse.ExecuteScript("window.history.go(1)", "");
        }

        private void DefaultStay()
        {
            Stay(0, 1);
        }

        private void Stay( int min, int max)
        {            
            int RandomValue = CommonUtils.GetRandomValue( min, max);            
            Thread.Sleep(RandomValue * 1000);
        }

 
        private void doScroll(int interval, int y, int count)
        {
            var jse = (IJavaScriptExecutor)driver;            
            for (int i = 0; i < count; i++)
            {
                jse.ExecuteScript(string.Format("window.scrollBy( {0},{1})", 0, y), "");
                Thread.Sleep(interval);
            }
        }

        private void SetSearchKeyword( string keyword, string position)
        {                                    
            IWebElement element = null;
            
            if (position.Equals("상단"))
            {
                element =  driver.FindElement(By.CssSelector( "#query"));
            }
            else
            {
                element = driver.FindElement(By.CssSelector("#nx_query_btm"));
            }

            element.Clear();
            element.SendKeys( keyword);
        }



        // 검색 버튼 클릭
        private void ClickSearchButton(string element)
        {
            switch (element)
            {
                case "search_btn": { driver.FindElement(By.Id(element)).Click(); } break;
                case "bt_search": { driver.FindElement(By.ClassName(element)).Click(); } break;
                case "query": { driver.FindElement(By.Id(element)).SendKeys(OpenQA.Selenium.Keys.Enter); } break;
                case "nx_query": { driver.FindElement(By.Id(element)).SendKeys(OpenQA.Selenium.Keys.Enter); } break;
            }
        }


        private void CloseBrowser()
        {
           // throw new NotImplementedException();
        }

        private void StartBrowser()
        {
            SelectBrowser(inputMap[LogicInput.CONST_BROWSER]);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(999999);
            string initPage = agentType == AGENT_TYPE_PC ? CONST_DEFAULT_PC_START_URL : CONST_DEFAULT_MOBILE_START_URL;
            driver.Navigate().GoToUrl( initPage);            
        }

        private void SelectBrowser(string browser)
        {
            switch (browser)
            {
                case "Chrome":
                    {
                        ChromeOptions cOptions = new ChromeOptions();
                        cOptions.AddArguments("disable-infobars");
                        cOptions.AddArguments("--js-flags=--expose-gc");
                        cOptions.AddArguments("--enable-precise-memory-info");
                        cOptions.AddArguments("--disable-popup-blocking");
                        cOptions.AddArguments("--disable-default-apps");
                        //cOptions.AddArguments("--headless");

                        // 서비스 초기화
                        ChromeDriverService  chromeDriverService = ChromeDriverService.CreateDefaultService();
                        chromeDriverService.HideCommandPromptWindow = true;

                        driver = new ChromeDriver(chromeDriverService, cOptions);
                    }
                    break;
                case "FireFox":
                    {
                        var driverService = FirefoxDriverService.CreateDefaultService();
                        driverService.HideCommandPromptWindow = true;
                        driver = new FirefoxDriver(driverService);
                    }
                    break;
                case "IE":
                    {
                        var driverService = InternetExplorerDriverService.CreateDefaultService();
                        driverService.HideCommandPromptWindow = true;
                        driver = new InternetExplorerDriver(driverService);
                    }
                    break;
                case "PhantomJS":
                    {
                        var service = PhantomJSDriverService.CreateDefaultService();
                        service.SslProtocol = "tlsv1"; //"any" also works
                        service.HideCommandPromptWindow = true;                                                       
                        driver = new PhantomJSDriver( service);
                    }
                    break;
                case "랜덤":
                    {
                        string[] browsers = { "Chrome", "FireFox", "IE" };
                        SelectBrowser(browsers[CommonUtils.GetRandomValue(0, browsers.Length - 1)]);
                    }
                    break;
            }
        }
     
        internal void SetLogManager(LogManager logManager)
        {
            this.logManager = logManager;
        }
    }
}
