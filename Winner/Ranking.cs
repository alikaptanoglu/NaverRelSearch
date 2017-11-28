using Naver.SearchAd;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using RestSharp.Extensions.MonoHttp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winner
{
    public partial class Ranking : Form
    {
        IWebDriver driver;
        Thread backGroundSearchRankingThread;
        SQLite sqlite;

        private int HEADER_KEYWORD = 0;
        private int HEADER_SUB_KEYWORD = 1;
        private int HEADER_RANKING = 2;
        private int HEADER_UPDATE_DATE = 3;
        private int HEADER_IS_MORE = 4;

        public Ranking()
        {
            InitializeComponent();
            Init();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        private void Init()
        {            
            CommonInit();            
            InitDataLoad();
            
        }

        /// <summary>
        /// 초기데이터 로드
        /// </summary>
        private void InitDataLoad()
        {
            // 그리드 데이터 로드            
            List<RankingModel> rankings = sqlite.SelectAllRanking();
            AddDataGridRows( rankings);
        }

        /// <summary>
        /// 공통초기화
        /// </summary>
        private void CommonInit()
        {
            // 그리드 설정
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);

            // SQL 설정
            sqlite = SQLite.GetInstance();
        }

        private void AddDataGridRows(List<RankingModel> rankings)
        {
            for (int i = 0; i < rankings.Count; i++)
            {
                AddDataGridRow(rankings.ElementAt(i));
            }
        }

        private void AddDataGridRow(RankingModel ranking)
        {            
            dataGridView1.Rows.Add( CommonUtils.MakeArray(ranking));
        }

        // 검색 백그라운드 쓰레드
        public void SearchRankingBackGround()
        {
                try
                {

                pictureBox3.Invoke(new Action(delegate ()
                {
                    pictureBox3.BackgroundImage = global::Winner.Properties.Resources.BB;                                        
                }));

                var service = PhantomJSDriverService.CreateDefaultService(); 
                service.SslProtocol = "any"; //"any" also works
                service.HideCommandPromptWindow = true;                
                driver = new PhantomJSDriver(service);

                //ChromeOptions cOptions = new ChromeOptions();
                //cOptions.AddArguments("disable-infobars");
                //cOptions.AddArguments("--js-flags=--expose-gc");
                //cOptions.AddArguments("--enable-precise-memory-info");
                //cOptions.AddArguments("--disable-popup-blocking");
                //cOptions.AddArguments("--disable-default-apps");
                //cOptions.AddArguments("--headless");                        

                //// 서비스 초기화
                //ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
                //chromeDriverService.HideCommandPromptWindow = true;

                //driver = new ChromeDriver(chromeDriverService, cOptions);


                DataGridViewRowCollection rows = dataGridView1.Rows;

                    for (int i = 0; i < rows.Count; i++)
                    {
                        DataGridViewRow row = rows[i];
                        DataGridViewCellCollection cells = row.Cells;
                        string keyword = (string)cells[HEADER_KEYWORD].Value;
                        string subKeyword = (string)cells[HEADER_SUB_KEYWORD].Value;

                        driver.Navigate().GoToUrl("https://search.naver.com/search.naver?where=nexearch&sm=top_hty&fbm=1&ie=utf8&query=" + HttpUtility.UrlEncode(keyword));
                                                
                        IWebElement unfold = CssSelector.FindElement(driver, ".unfold");
                        bool isRanked = false;

                        if (unfold != null)
                        {
                            if (unfold.Displayed)
                            {
                                unfold.Click();
                            }
                            IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.CssSelector(".lst_relate a"));
                            int currentWidth = 0;
                            int line = 0;


                            for (int j = 0; j < elements.Count; j++)
                            {
                                IWebElement element = elements.ElementAt(j);
                                IWebElement li = element.FindElement(By.XPath("./.."));
                                string WIDTH = li.GetCssValue("width");
                            
                                int cusorWIDTH = (int)double.Parse(WIDTH.Substring(0, WIDTH.IndexOf("px"))) + 13;

                                currentWidth += cusorWIDTH;

                                if (currentWidth > 460)
                                {
                                    currentWidth = cusorWIDTH;
                                    line++;
                                }

                                if (element.Text.Equals(subKeyword))
                                {
                                    isRanked = true;

                                    dataGridView1.Invoke(new Action(delegate ()
                                    {
                                        row.Cells[HEADER_RANKING].Value = element.GetAttribute("data-idx");
                                        row.Cells[HEADER_UPDATE_DATE].Value = DateTime.Now.ToString("yyyy-MM-dd hh:mm");
                                        if (line >= 2)
                                        {
                                            row.Cells[HEADER_IS_MORE].Value = "N";
                                        }
                                        else
                                        {
                                            row.Cells[HEADER_IS_MORE].Value = "Y";
                                        }
                                    }));

                                    currentWidth = 0;
                                    break;
                                }
                            }                          
                        }

                        if (!isRanked)
                        {
                            row.Cells[HEADER_RANKING].Value = "*";
                            row.Cells[HEADER_UPDATE_DATE].Value = DateTime.Now.ToString("yyyy-MM-dd hh:mm");
                            row.Cells[HEADER_IS_MORE].Value = "N/A";
                        }                        
                    }
                }
                catch (Exception e)
                {                   
                    
                }
                finally
                {
                    PhantomJSClose();
                }
            
        }

        private void PhantomJSClose()
        {
            pictureBox3.Invoke(new Action(delegate ()
            {
                pictureBox3.BackgroundImage = global::Winner.Properties.Resources.AA;
            }));

            if (driver != null)
            {
                try
                {
                    driver.Close();
                    driver.Quit();
                }
                catch
                {

                }
                finally
                {
                    driver = null;
                    backGroundSearchRankingThread = null;
                }                              
            }
        }

        /// <summary>
        /// 검색
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchRanking(object sender, EventArgs e)
        {
            if (backGroundSearchRankingThread != null)
            {
                backGroundSearchRankingThread.Abort();                          
            }
            else
            {
                backGroundSearchRankingThread = new Thread(new ThreadStart(SearchRankingBackGround));
                backGroundSearchRankingThread.Start();
            }            
        }

        /// <summary>
        /// 키워드 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddKeyWord(object sender, EventArgs e)
        {
            AddKeyWord();
        }

        private void AddKeyWord()
        {
            string keyword = textBox2.Text;
            string subKeyword = textBox1.Text;

            if (keyword.Trim().Length == 0 || subKeyword.Trim().Length == 0)
            {
                MessageBox.Show("키워드 및 서브키워드는 반드시 모두 입력해야 합니다.", "경고");
                return;
            }


            string[] row = { keyword, subKeyword, "", "" };
            dataGridView1.Rows.Add(row);
        }

        // 키워드 삭제
        private void RemoveKeyWord(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {               
                dataGridView1.Rows.Remove(row);                
            }
        }
        /// <summary>
        /// 컬럼 드래그엔 드랍
        /// </summary>        
        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;
        

        private void dataGridView1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag.
                if (dragBoxFromMouseDown != Rectangle.Empty &&
                    !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {

                    // Proceed with the drag and drop, passing in the list item.                    
                    DragDropEffects dropEffect = dataGridView1.DoDragDrop(
                    dataGridView1.Rows[rowIndexFromMouseDown],
                    DragDropEffects.Move);
                }
            }
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            // Get the index of the item the mouse is below.
            rowIndexFromMouseDown = dataGridView1.HitTest(e.X, e.Y).RowIndex;
            if (rowIndexFromMouseDown != -1)
            {
                // Remember the point where the mouse down occurred. 
                // The DragSize indicates the size that the mouse can move 
                // before a drag event should be started.                
                Size dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                               e.Y - (dragSize.Height / 2)),
                                    dragSize);
            }
            else
                // Reset the rectangle if the mouse is not over an item in the ListBox.
                dragBoxFromMouseDown = Rectangle.Empty;
        }

        private void dataGridView1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            // The mouse locations are relative to the screen, so they must be 
            // converted to client coordinates.
            Point clientPoint = dataGridView1.PointToClient(new Point(e.X, e.Y));

            // Get the row index of the item the mouse is below. 
            rowIndexOfItemUnderMouseToDrop =
                dataGridView1.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            // If the drag operation was a move then remove and insert the row.
            if (e.Effect == DragDropEffects.Move)
            {
                DataGridViewRow rowToMove = e.Data.GetData(
                    typeof(DataGridViewRow)) as DataGridViewRow;
                dataGridView1.Rows.RemoveAt(rowIndexFromMouseDown);
                dataGridView1.Rows.Insert(rowIndexOfItemUnderMouseToDrop, rowToMove);

            }
        }
        /// <summary>
        ///  폼 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Unload(object sender, FormClosingEventArgs e)
        {
            // 그리드 저장
            DataGridViewRowCollection rows = dataGridView1.Rows;
            List<RankingModel> models = new List<RankingModel>();
            for (int i = 0; i < rows.Count; i++)
            {
                DataGridViewRow row = rows[i];
                RankingModel model = new RankingModel();
                model.keyword = (string)row.Cells[HEADER_KEYWORD].Value;
                model.subKeyword = (string)row.Cells[HEADER_SUB_KEYWORD].Value;
                model.rank = (string)row.Cells[HEADER_RANKING].Value;
                model.checkedAt = (string)row.Cells[HEADER_UPDATE_DATE].Value;
                model.more = (string)row.Cells[HEADER_IS_MORE].Value;

                models.Add(model);
            }

            sqlite.DeleteAllTables(RankingModel.TableName);
            sqlite.InsertAllRankings( models);
        }

        private void AddKeyWord(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                AddKeyWord();
            }                                       
        }
    }
}
