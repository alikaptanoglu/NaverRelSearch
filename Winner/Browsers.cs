using Naver.SearchAd;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Winner;

namespace BrowserCollection
{

        public interface Browser
        {
            void run();
            void SetUp( Main form1);
            void SetLogManager(LogManager logManager);            
         }

        public class Selectors
        {
            // 속성으로 조회
            public static By SelectorByAttributeValue(string p_strAttributeName, string p_strAttributeValue)
            {
                return (By.XPath(String.Format("//*[@{0} = '{1}']",
                                               p_strAttributeName,
                                               p_strAttributeValue)));
            }
        }

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

        public class ChromeBrowser : Browser
        {        
            private const string CONST_DEFAULT_PC_START_URL = "https://www.naver.com/";
            private const string CONST_DEFAULT_MOBILE_START_URL = "https://m.naver.com/";
        
            private Main form;
            private ChromeOptions cOptions;
            private ChromeDriverService chromeDriverService;
            private IWebDriver driver;
            private LogManager logManager;
            private SQLite sqlite;
            private int agentType;

            int[] tabArray = new int[] { (int)NaverTab.Cafe, (int)NaverTab.Knowledge, (int)NaverTab.Movie, (int)NaverTab.Blog, (int)NaverTab.Image, (int)NaverTab.Search, (int)NaverTab.News };


            delegate void StartBrowserCallback();

            public ChromeBrowser()
            {
            // 옵션 초기화
                cOptions = new ChromeOptions();
                cOptions.AddArguments("disable-infobars");
                cOptions.AddArguments("--js-flags=--expose-gc");
                cOptions.AddArguments("--enable-precise-memory-info");
                cOptions.AddArguments("--disable-popup-blocking");                                 
                cOptions.AddArguments("--disable-default-apps");
                //cOptions.AddArguments("--headless");

            // 서비스 초기화
                chromeDriverService = ChromeDriverService.CreateDefaultService();
                chromeDriverService.HideCommandPromptWindow = true;
            

                // SQLite
                sqlite = SQLite.GetInstance();
            }

            

            void Browser.run()
            {                
                while (true)
                {               
                DataGridViewRowCollection rows = form.getTableRows();                
                DataGridViewRow row = rows[CommonUtils.GetRandomValue(0, rows.Count)];                
                DataGridViewCellCollection cells = row.Cells;

                SetAgentType(getCellValue(cells, Main.HEADER_AGENT));

                if (!form.isRemainTask())
                {
                    break;
                }

                if (int.Parse(getCellValue(cells, Main.HEADER_TO_COUNT)) == int.Parse(getCellValue(cells, Main.HEADER_CURR_COUNT)))
                {
                    continue;
                }

                List<Configuration> Configs = sqlite.SelectAllConfigurationByOwner(getCellValue(cells, Main.HEADER_SLOT));
                if (Configs == null || Configs.Count == 0)
                {
                    Configs = sqlite.SelectAllConfigurationByOwner(Configuration.Default);
                }

                Dictionary<string, string> dictionary = Configuration.ConvertObjectToMap(Configs);

                string searchText = getCellValue(cells, Main.HEADER_SEARCH);
                string nxSearchText = getCellValue(cells, Main.HEADER_NXSEARCH);

                try
                {
                    logManager.AppendLog("//========슬롯[{0}]의 작업을 시작합니다.========//", getCellValue(cells, Main.HEADER_SLOT));

                    form.BeginInvoke(new Action(() => { form.ChangeRowColor(row, Color.LawnGreen); }));

                    // 브라우저 시작
                    StartBrowser( dictionary, getCellValue(cells, Main.HEADER_BROWSER));                    

                    // 검색어 입력
                    SetSearchText("#query", searchText, false);

                    // 검색전 체류
                    Stay("1차 검색 전", dictionary[Configuration.COMMON_PRE_SEARCH_STAY]);

                    // 1차 검색
                    if (agentType == Slot.AGENT_TYPE_PC)
                    {
                        ClickSearchBtn("search_btn");
                    }
                    else
                    {
                        ClickSearchBtn("query");
                    }

                    // 랭크 처리
                    form.SetSearchRank(row, FindSearchRank(nxSearchText));

                    // 검색 후 체류
                    Stay("1차 검색 후", dictionary[Configuration.COMMON_SEARCH_STAY]);

                    // 스크롤 처리           
                    ScrollBy(dictionary);

                    // 카테고리 이동            
                    int index = MoveTab( getCellValue(cells, Main.HEADER_CATEGORY));

                    // 카테고리 이동 후 체류
                    Stay("카테고리 이동 후", dictionary[Configuration.COMMON_SEARCH_STAY]);

                    // 2차 검색어 입력
                    SetSearchText("#nx_query", nxSearchText, true);

                    // 검색전 체류
                    Stay("2차 검색 전", dictionary[Configuration.COMMON_PRE_SEARCH_STAY]);

                    // 2차 검색
                    if (agentType == Slot.AGENT_TYPE_PC)
                    {
                        ClickSearchBtn("bt_search");
                    }
                    else
                    {
                        ClickSearchBtn("nx_query");
                    }
             
                    // 검색 후 체류
                    Stay("2차 검색 후", dictionary[Configuration.COMMON_SEARCH_STAY]);

                    int MaxPageCount = CommonUtils.GetRandomValue(dictionary[Configuration.COMMON_MAX_PAGE]);
                    for (int i = 0; i < MaxPageCount; i++)
                    {
                        // 스크롤 처리           
                        ScrollBy(dictionary);

                        // 게시물 조회
                        bool isViewSuccess = PostViewBy(index, null, 1500, true);

                        // 게시물 조회 성공
                        if (isViewSuccess)
                        {
                            // 스크롤 처리           
                            ScrollBy(dictionary);

                            // 최초방문페이지 체류
                            Stay("최초 페이지 방문 후", dictionary[Configuration.COMMON_INIT_PAGE_STAY]);
                        }                  

                        // 이전 히스토리 이동
                        HistoryBack();

                        // 이전 히스토리 이동 후 체류 시간
                        Stay("히스토리 이전으로 이동 후", dictionary[Configuration.COMMON_PRE_HISTORY_MOVE_STAY]);
                    }

                    DeleteCookie();

                    // 쿠키삭제 후 체류
                    Stay("쿠키 삭제 후", dictionary[Configuration.COOKIE_DELETE_POST_STAY]);

                    // 카운트 증가
                    ComplateProcess(row);
                    
                    //브라우저 종료
                    CloseBrowser();

                    // 아이피 변경
                    ChangeIP(dictionary);
             
                    logManager.AppendLog("//========슬롯[{0}]의 작업을 종료합니다.========//", getCellValue(cells, Main.HEADER_SLOT));
                }
                catch (Exception e)
                {                    
                    logManager.AppendLog("Nested Exception : {0}", e.Message);

                    // 작업 종료
                    if (e is ThreadAbortException)
                    {
                        form.EnableWorkBtn(false, "정지 중");
                        ClosingProcess(row);
                        logManager.AppendLog("정상적으로 작업이 중지 되었습니다.");
                        form.EnableWorkBtn(true, "작업 실행");
                    }
                    else
                    {
                        ClosingProcess(row);
                        logManager.AppendLog("[EXCEPTION] 브라우저가 강제적으로 종료되었거나 알 수 없는 오류가 발생 하였습니다.");
                    }
                    
                    break;
                }

                form.EnableWorkBtn(true, "작업 실행");
            }            
        }

        private void ClickMobileS()
        {
            driver.FindElement(By.CssSelector("")).SendKeys(OpenQA.Selenium.Keys.Enter);
        }

        // 에이전트 설정
        private void SetAgentType(string text)
        {
            switch ( text)
            {
                case "랜덤":
                {
                    int value = CommonUtils.GetRandomValue(1, 10);
                    agentType = value <= 7 ? Slot.AGENT_TYPE_MOBILE : Slot.AGENT_TYPE_PC;
                }
                break;
                case "PC":
                {
                    agentType = Slot.AGENT_TYPE_PC;
                }
                break;
                case "모바일":
                {
                    agentType = Slot.AGENT_TYPE_MOBILE;
                }
                break;
            }
        }

        private void ComplateProcess( DataGridViewRow row)
        {
            form.PlusCurrentCount(row);
            form.BeginInvoke(new Action(() => { form.ChangeRowColor(row, Color.White); }));
        }

        private void ClosingProcess(DataGridViewRow row)
        {               
            CloseBrowser();
            form.BeginInvoke(new Action(() => { form.ChangeRowColor(row, Color.White); }));
            form.EnableWorkBtn(true, "작업 실행");            
        }

        // 아이피 변경
        private void ChangeIP(Dictionary<string, string> dictionary)
        {
            logManager.AppendLog("아이피 를 변경중입니다.");
            switch (dictionary[Configuration.IP_AGENT_TYPE])
            {
                case "Proxy":
                {
                    // 프록시는 구현 예정
                } break;
                case "Tethering":
                {                        
                    MobileUtils.EnAbleAirPlainMode();
                    MobileUtils.DisAbleAirPlainMode();
                } break;
            }            
            logManager.AppendLog("아이피 변경이 완료되었습니다.");

            // 아이피 변경 후 대기 시간
            Stay("아이피 변경 후", dictionary[Configuration.IP_CAHNGE_STAY]);

            string externalIP = CommonUtils.GetExternalIPAddress();
            logManager.AppendLog(string.Format( "변경된 아이피 ==> {0}",externalIP));

            form.setExternalAddress( CommonUtils.GetExternalIPAddress());
        }

        //  브라우저 종료
        private void CloseBrowser()
        {
            
            if (driver != null)
            {
                try
                {
                    driver.Close();
                    driver.Quit();
                }
                catch( Exception ex)
                {
                    if (ex is InvalidOperationException || ex is NoSuchWindowException)
                    {
                        CommonUtils.ProcessKillByName("chromedriver");
                    }                    
                }                
            }
            else
            {
                CommonUtils.ProcessKillByName("chromedriver");             
            }
        }

        // 쿠키 삭제
        private void DeleteCookie()
        {
            logManager.AppendLog("쿠키를 삭제합니다.");
            driver.Manage().Cookies.DeleteAllCookies();
        }

        // 검색 버튼 클릭
        private void ClickSearchBtn( string element)
        {
            
            switch (element)
            {
                case "search_btn": { driver.FindElement(By.Id( element)).Click(); } break;
                case "bt_search": { driver.FindElement(By.ClassName( element)).Click(); } break;
                case "query": {  driver.FindElement(By.Id(element)).SendKeys(OpenQA.Selenium.Keys.Enter); } break;
                case "nx_query": { driver.FindElement(By.Id(element)).SendKeys(OpenQA.Selenium.Keys.Enter); } break;
            }
         
        }

        private void StartBrowser(Dictionary<string, string> dictionary, string browser )
        {                        
            logManager.AppendLog("브라우저를 실행합니다.");

            var service = PhantomJSDriverService.CreateDefaultService();
            service.SslProtocol = "tlsv1"; //"any" also works
                                           //service.HideCommandPromptWindow = true;
                                           // driver = new PhantomJSDriver(service);
            
            selectBrowser( browser);
            
            // 타임아웃 5분
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(999999);


            string startPageList = dictionary[Configuration.START_PAGE_LIST];
            string initPage = agentType == Slot.AGENT_TYPE_PC ? CONST_DEFAULT_PC_START_URL : CONST_DEFAULT_MOBILE_START_URL;
            if (startPageList != null && startPageList.Length > 0)
            {
                string[] arrPageList = startPageList.Split(Configuration.DELIMITER_CHARS);

                if (arrPageList != null && arrPageList.Length > 0)
                {
                    initPage = arrPageList[CommonUtils.GetRandomValue(0, arrPageList.Length)];
                }
            }
                               
            // 버튼 활성화            
            form.EnableWorkBtn(true, "작업 정지");

            driver.Navigate().GoToUrl(initPage);
            Stay("시작페이지에서",dictionary[Configuration.START_PAGE_STAY]);
        }

        private void selectBrowser( string browser)
        {
            switch ( browser)
            {
                case "Chrome":
                {
                        driver = new ChromeDriver(chromeDriverService, cOptions);
                } break;
                case "FireFox":
                {
                        var driverService = FirefoxDriverService.CreateDefaultService();
                        driverService.HideCommandPromptWindow = true;
                        driver = new FirefoxDriver( driverService);
                } break;
                case "IE":
                {
                        var driverService = InternetExplorerDriverService.CreateDefaultService();
                        driverService.HideCommandPromptWindow = true;
                        driver = new InternetExplorerDriver( driverService);
                } break;
                case "랜덤":
                {
                    string[] browsers = { "Chrome", "FireFox", "IE" };                        
                    selectBrowser(browsers[CommonUtils.GetRandomValue(0, browsers.Length - 1)]);
                }
                break;
            }
        }

        private void Stay(string preText,string value)
        {            
            string[] values = value.Split( Configuration.DELIMITER_CHARS);
            int RandomValue = CommonUtils.GetRandomValue(values[0], values[1]);
            logManager.AppendLog("{0} 약 {1}초 체류합니다.", preText, RandomValue);
            Thread.Sleep( RandomValue * 1000);            
        }

        // 셀의 값을 반환
        private string getCellValue(DataGridViewCellCollection Cells, int index)
        {
            return (string)Cells[ index].Value;
        }

        private void SetSearchText(string element, object searchText, bool isDoRemoveText)
        {
            logManager.AppendLog("검색어 {0}을 입력합니다.", searchText);
            var query = driver.FindElement(By.CssSelector(element));
            if (isDoRemoveText)
            {
                query.Clear();
            }
            query.SendKeys((string) searchText);
        }

        private int FindSearchRank( string searchText)
        {
            IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.CssSelector(".lst_relate a"));

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements.ElementAt(i).Text.Equals(searchText))
                {
                    return i + 1;
                }                
            }

            return -1;
        }

        // 이전 히스토리로 이동
        private void HistoryBack()
        {           
            var jse = (IJavaScriptExecutor)driver;
            jse.ExecuteScript("window.history.go(-1)", "");
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
                selectIndex = new Random().Next(0, tabArray.Length - 1);
            }
            
            return selectIndex;
        }

        private T RandomEnum<T>()
        {
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(new Random().Next(0, values.Length));
        }

        // 카테고리 이동
        private int MoveTab( string category)
        {
            int index = 0;
            IWebElement element = null;
            index = convertStringToTabIndex(category);

            if (agentType == Slot.AGENT_TYPE_PC)
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
                string value = convertStringToTabHref( index);
                element = driver.FindElement(By.CssSelector( string.Format("li a[href*='{0}']", value)));
                ExcuteScrollAction(1000, -200, 1);
                logManager.AppendLog("{0} 카테고리로 이동합니다.", category);
                element.Click();
            }

            return index;
        }

        private string convertStringToTabHref(int index)
        {
            string href = null;
            string[] allCategory = { "where=m", "where=m_image", "where=m_blog", "where=m_video", "where=m_kin", "where=m_cafe", "where=m_news" };

            if (index == (int) NaverTab.Search)
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

        // 게시물 조회
        private bool PostViewBy(int index,string url, int stay, bool isSelf)
        {
            IWebElement element = null;
            IReadOnlyCollection<IWebElement> elements = null;
            if (isSelf)
            {
                var jse = (IJavaScriptExecutor)driver;
                jse.ExecuteScript("var array = document.getElementsByTagName('a'); for (var i = 0; i < array.length; i++) { array[i].setAttribute('target', '_self')}", "");                
            }
            
            if (url != null)
            {
                element = driver.FindElement(Selectors.SelectorByAttributeValue("href", url));
            }
            else
            {
                elements = null;

                if (index == (int)NaverTab.Blog)
                {
                    elements = agentType == Slot.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".sh_blog_title")) : driver.FindElements(By.CssSelector(".lst_total .bx"));                                        
                }
                else if (index == (int)NaverTab.News)
                {                    
                    elements = agentType == Slot.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector("._sp_each_title")) : driver.FindElements(By.CssSelector(".list_news .bx"));
                }
                else if (index == (int)NaverTab.Cafe)
                {
                    elements = agentType == Slot.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".sh_cafe_title")) : driver.FindElements(By.CssSelector(".lst_total .bx"));                    
                }
                else if (index == (int)NaverTab.Image)
                {
                    elements = agentType == Slot.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".thumb")) : driver.FindElements(By.CssSelector(".photo_grid a"));                            
                }
                else if (index == (int)NaverTab.Knowledge)
                {
                    elements = agentType == Slot.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".question a")) : driver.FindElements(By.CssSelector(".lst_total a"));                    
                }
                else if (index == (int)NaverTab.Movie)
                {
                    elements = agentType == Slot.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".video_info a")) : driver.FindElements(By.CssSelector(".video_list .info_area a"));                                        
                }
                else if (index == (int)NaverTab.Search)
                {
                    if (agentType == Slot.AGENT_TYPE_PC)
                    {
                        elements = driver.FindElements(By.CssSelector(".question a, .sh_blog_title, ._sp_each_title, .sh_cafe_title, .sh_booktext_title, lst_img a, .video_thum"));
                    }
                    else
                    {
                        elements = driver.FindElements(By.CssSelector(".list_video .bx, .photo_grid a, .list_news .bx, .sp_ntotal .bx"));                        
                    }                    
                }                         
            }

            logManager.AppendLog("페이지에 방문합니다.");

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
                    if (e is WebDriverException)
                    {
                        logManager.AppendLog("페이지가 타임아웃 되었습니다.");
                        return false;
                    }
                    else if (e is InvalidOperationException)
                    {
                        continue;
                    }                    
                }                           
            }
            
        }

        // 스크롤 Down
        //private void ScrollBy( ChromeDriver driver, int y, int count, int interval)
        private void ScrollBy( Dictionary<string, string> dictionary)
        {
            if (bool.Parse(dictionary[Configuration.COMMON_POST_SEARCH_SCROLL_USE]))
            {
                string[] configs = dictionary[Configuration.COMMON_POST_SEARCH_SCROLL].Split(Configuration.DELIMITER_LIST_CHARS);
                string[] intervals = configs[0].Split(Configuration.DELIMITER_CHARS);
                string[] ys = configs[1].Split(Configuration.DELIMITER_CHARS);
                string[] counts = configs[2].Split(Configuration.DELIMITER_CHARS);

                int interval = CommonUtils.GetRandomValue(intervals[0], intervals[1]) * 1000;
                int y = CommonUtils.GetRandomValue(ys[0], ys[1]);
                int count = CommonUtils.GetRandomValue(counts[0], counts[1]);

                ExcuteScrollAction(interval, y, count);
            }
        }

        private void ExcuteScrollAction(int interval, int y, int count)
        {
            var jse = (IJavaScriptExecutor)driver;
            logManager.AppendLog("{0}초 간격으로 {1}Pixel만큼 {2}회 스크롤링 합니다.", interval / 1000, y, count);
            for (int i = 0; i < count; i++)
            {
                jse.ExecuteScript(string.Format("window.scrollBy( {0},{1})", 0, y), "");
                Thread.Sleep(interval);
            }
        }

        // 폼 설정
        public void SetUp(Main form1)
        {
            this.form = form1;
        }

        // 로그 관리자 설정
        public void SetLogManager(LogManager logManager)
        {
            this.logManager = logManager;
        }
    }
 }

