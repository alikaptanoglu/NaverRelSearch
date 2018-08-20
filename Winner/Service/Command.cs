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

using System.Threading;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Remote;

namespace Winner
{

    public class Command
    {
        private List<LogicItem> items;
        private Dictionary<string, string> inputMap;
        private IWebDriver driver;
        private LogManager logManager;        
        private WebDriverWait WAIT;

        private PropertyManager PROP;

        public Command(List<LogicItem> items, Dictionary<string, string> inputMap)
        {
            this.items = items;
            this.inputMap = inputMap;
            this.PROP = new PropertyManager();
            
            SelectUpAgent(inputMap[LogicInput.CONST_AGNET]);
        }

        public void doCommand()
        {
            BeforeCommand();
            StartBrowser();
            doProcess();
            CloseBrowser();
        }

        // 슬롯작업 시작 전
        private void BeforeCommand()
        {
            string[] waitTime = inputMap[LogicInput.CONST_SLOT_WAIT_TIME].Split(CommonUtils.delimiterChars);
            logManager.AppendLog("[슬롯대기시간][{0}/{1}]", waitTime[0], waitTime[1]);
            Stay(waitTime[0], waitTime[1]);
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
            if (type.Equals( Agent.AGENT_TYPE_PC) || type.Equals(Agent.AGENT_TYPE_MOBILE))
            {
                PROP.AGENT.SetType( type);                
            }            
            else
            {
                PROP.AGENT.SetRandomAgent();                
            }
        }


        private void ExecuteCommand(string action, string value)
        {
            string[] v = value.Split(CommonUtils.delimiterSlash);

            DefaultStay();

            logManager.AppendLog(string.Format("[{0}][{1}]", action, ObjectUtils.isNull(value, "랜덤")));
                        
            switch (action)
            {
                case "키워드":
                    {                        
                        if ( PROP.LOCATION.GetLocation() == Location.HOME)
                        {                                                      
                            bool success = SetSearchKeyword("#query", v[0], -1);

                            if (!success)
                            {
                                MoveHome();
                                SetSearchKeyword("#query", v[0], -1);
                            }

                            if (PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC)
                            {
                                ClickSearchButton("search_btn");
                            }
                            else
                            {
                                ClickSearchButton("query");
                            }


                            PROP.LOCATION.SetLocation(Location.SEARCH);                            

                        }
                        else if (PROP.LOCATION.GetLocation() == Location.SEARCH)
                        {
                            bool success = false;
                            string selector = null;
                            int direction = 0;

                            if (v[1].Equals("상단"))
                            {
                                selector = "#nx_query";
                                direction = -1;
                                success = SetSearchKeyword(selector, v[0], direction);
                            }
                            else
                            {
                                selector = "#nx_query_btm";
                                direction = 1;
                                success = SetSearchKeyword(selector, v[0], direction);
                            }

                            if (!success)
                            {
                                MoveHome();
                                SetSearchKeyword(selector, v[0], direction);
                            }

                            if (PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC)
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
                        // 카테고리
                        PROP.LOCATION.CATEGORY = PROP.LOCATION.CATEGORY.GetCategoryByName(v[0]);

                        bool success = MoveCategory();

                        if (!success)
                        {
                            MoveHome();
                            MoveCategory();
                        }
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

                                if (PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC)
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
                case "홈":
                    {
                        MoveHome();
                    }
                    break;

            }

            DefaultStay();
        }

        // 홈 이동
        private void MoveHome()
        {
            IWebElement element = CssSelector.FindElement(driver, ".naver_logo, .logo_naver, .link_naver, .h_logo");      
            
            if (element == null)
            {
                if ( PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC)
                {
                    driver.Navigate().GoToUrl(Url.CONST_DEFAULT_PC_START_URL);
                }
                else
                {
                    driver.Navigate().GoToUrl(Url.CONST_DEFAULT_MOBILE_START_URL);
                }                
            }
            else
            {                
                element.Click();
            }

            PROP.LOCATION.SetLocation( Location.HOME);                        
        }


        // 종료처리
        internal void Finish()
        {
            if ( driver != null)
            {

                try
                {
                    driver.Close();
                    driver.Quit();
                }
                catch (Exception ex)
                {
                    if (ex is InvalidOperationException)
                    {
                        driver.Quit();
                    }
                }
            }
        }

        // 게시물 조회
        private bool PostViewBy(string url, bool isSelf)
        {
            Start:

            IWebElement element = null;
            IReadOnlyCollection<IWebElement> elements = null;          

            try
            { 
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
                    if (PROP.LOCATION.CATEGORY.code == Category.BLOG)
                    {
                        elements = PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".sh_blog_title")) : driver.FindElements(By.CssSelector(".lst_total .bx"));
                    }
                    else if (PROP.LOCATION.CATEGORY.code == Category.NEWS)
                    {
                        elements = PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector("._sp_each_title")) : driver.FindElements(By.CssSelector(".list_news .bx"));
                    }
                    else if(PROP.LOCATION.CATEGORY.code == Category.CAFE)
                    {
                        elements = PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".sh_cafe_title")) : driver.FindElements(By.CssSelector(".lst_total .bx"));
                    }
                    else if(PROP.LOCATION.CATEGORY.code == Category.IMAGE)
                    {
                        elements = PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".thumb")) : driver.FindElements(By.CssSelector(".photo_grid a"));
                    }
                    else if(PROP.LOCATION.CATEGORY.code == Category.KNOWLEDGE)
                    {
                        elements = PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".question a")) : driver.FindElements(By.CssSelector(".lst_total a"));
                    }
                    else if(PROP.LOCATION.CATEGORY.code == Category.MOVIE)
                    {
                        elements = PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC ? driver.FindElements(By.CssSelector(".video_info a")) : driver.FindElements(By.CssSelector(".video_list .info_area a"));
                    }
                    else if(PROP.LOCATION.CATEGORY.code == Category.SEARCH)
                    {
                        if ( PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC)                        
                        {
                            elements = driver.FindElements(By.CssSelector(".question a, .sh_blog_title, ._sp_each_title, .sh_cafe_title, .sh_booktext_title, lst_img a, .video_thum"));
                        }
                        else
                        {
                            elements = driver.FindElements(By.CssSelector(".list_video .bx, .photo_grid a, .list_news .bx, .sp_ntotal .bx"));
                        }
                    }
                    else
                    {
                        elements = driver.FindElements(By.CssSelector("a"));
                    }
                }

            }
            catch (UnhandledAlertException f)
            {
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept();
                goto Start;
            }


            if (elements.Count == 0)
            {                
                HistoryPrev();
                goto Start;
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

       

        private bool MoveCategory()
        {            
            int index = 0;
            IWebElement element = null;            
            //index = convertStringToTabIndex(category);

            if (PROP.LOCATION.GetLocation() == Location.HOME)
            {
                if (PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC)
                {
                    //string className = TabIndexToMainTabClassName(index);                   
                    element = CssSelector.FindElement(driver, string.Format(".{0}", PROP.LOCATION.CATEGORY.mainClass));

                    if (element == null)
                    {
                        return false;
                    }

                    try
                    {
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
                    //FIX 모바일 쪽은 나중에 수정                    
                    element = driver.FindElement(By.CssSelector(string.Format("li a[href*='{0}']", PROP.LOCATION.CATEGORY.m_menu)));
                    doScroll(1000, -200, 1);
                    element.Click();
                }
            }

            if (PROP.LOCATION.GetLocation() == Location.SEARCH)
            {
                if (PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC)
                {                    
                    element = CssSelector.FindElement(driver, string.Format(".lnb_menu li.lnb{0}", PROP.LOCATION.CATEGORY.index));

                    if (element == null)
                    {
                        return false;
                    }

                    try
                    {
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
                    string value = PROP.LOCATION.CATEGORY.m_menu;
                    element = driver.FindElement(By.CssSelector(string.Format("li a[href*='{0}']", PROP.LOCATION.CATEGORY.m_menu)));
                    doScroll(1000, -200, 1);
                    element.Click();
                }
            }

            PROP.LOCATION.CATEGORY.SetCategory( PROP.LOCATION.CATEGORY.name);
            return true;
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

        private void Stay(String min, String max)
        {
            Stay(int.Parse(min), int.Parse(max));
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
                IWebElement element = null;

                try
                {
                    IWebElement screenFrame = expandRootElement(driver.FindElement(By.Id("screenFrame")));
                    element = expandRootElement(screenFrame.FindElement(By.Id("mainFrame")));
                    IReadOnlyCollection<IWebElement> elements = element.FindElements(By.TagName("a"));
                }
                catch (Exception e)
                {
                    element = expandRootElement(driver.FindElement(By.Id("mainFrame")));
                    IReadOnlyCollection<IWebElement> elements = element.FindElements(By.TagName("a"));
                }

                jse.ExecuteScript(string.Format("window.scrollBy( {0},{1})", 0, y), element);
               // driver.FindElement(By.CssSelector("a")).SendKeys(OpenQA.Selenium.Keys.Down);
                Thread.Sleep(interval);
            }
        }

        //Returns webelement
        public IWebElement expandRootElement(IWebElement element)
        {
            var jse = (IJavaScriptExecutor)driver;
            
            IWebElement ele = (IWebElement)jse.ExecuteScript("return arguments[0].shadowRoot", element);
            return ele;
        }

        private bool SetSearchKeyword(string selector, string keyword, int direction)
        {
            //IWebElement e = driver.FindElement(By.CssSelector("#query"));
            //((IJavaScriptExecutor)driver).ExecuteScript( "arguments[0].scrollIntoView();", e);

            IWebElement element = FindElementByScroll( selector, direction);

            if (element == null)
            {
                return false;
            }

            element.Clear();

            if (keyword.Length == 0)
            {
                keyword = CommonService.GetRandomKeyword();
            }
            
            char[] charArray = keyword.ToCharArray();
            foreach (char c in charArray)
            {                                
                element.SendKeys(c.ToString());
                Thread.Sleep(400);
            }

            return true;
            

            //char[] charArray = keyword.ToCharArray();
            //foreach (char c in charArray)
            //{
            //    HanGulUtils.HANGUL_INFO info = HanGulUtils.DevideJaso(c);

            //    if (info.isHangul.Equals("H"))
            //    {
            //        char[] charArray2 = info.chars;
            //        foreach (char c2 in charArray2)
            //        {
            //            element.SendKeys(c2.ToString());

            //        }
            //    }
            //    else
            //    {
            //        element.SendKeys(c.ToString());

            //    }

            //}

        }

        

        private IWebElement FindElementByScroll(string selector, int direction)
        {
            IWebElement element = null;
            int MAX_COUNT = 10;
            int index = 0;

            while (element == null &&  index < MAX_COUNT)
            {
                try
                {
                    
                    element = driver.FindElement(By.CssSelector(selector));
                }
                catch (Exception e)
                {
                    doScroll(1, 100 * direction, 1);
                    index++;
                }
            }

            return element;
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
            string initPage = PROP.AGENT.GetType() == Agent.AGENT_TYPE_PC ? Url.CONST_DEFAULT_PC_START_URL : Url.CONST_DEFAULT_MOBILE_START_URL;
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
                        cOptions.AddArguments("enable-popup-blocking");
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
                        
                        var options = new InternetExplorerOptions
                        {
                            IgnoreZoomLevel = true
                        };

                        driver = new InternetExplorerDriver(driverService, options);
                        //driver.Manage().Timeouts().PageLoad =  TimeSpan.FromSeconds(10);
                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                    }
                    break;
                case "PhantomJS":
                    {
                       // var service = PhantomJSDriverService.CreateDefaultService();
                       // service.SslProtocol = "tlsv1"; //"any" also works
                        //service.HideCommandPromptWindow = true;                                                       
                        //driver = new PhantomJSDriver( service);
                    }
                    break;
                case "랜덤":
                    {
                        string[] browsers = { "Chrome", "FireFox", "IE" };
                        SelectBrowser(browsers[CommonUtils.GetRandomValue(0, browsers.Length )]);
                    }
                    break;
            }

            WAIT = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }
     
        internal void SetLogManager(LogManager logManager)
        {
            this.logManager = logManager;
        }
    }
}
