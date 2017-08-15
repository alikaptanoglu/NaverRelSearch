using BrowserCollection;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Naver.SearchAd;
using RestSharp;
using System.Runtime.CompilerServices;
using Managed.Adb;
using SharpAdbClient;

namespace Winner
{
    
    public partial class Main : Form
    {
        private Browser Browser;
        private  LogManager LogManager;

        private Thread naverAdThread;
        private Thread browserThread;
        private Thread keyWordThread;

        private SQLite sqlLite;

        private SearchAdApi restApi = new SearchAdApi();

        public static int HEADER_SLOT = 0;
        public static int HEADER_BROWSER = 1;
        public static int HEADER_AGENT = 2;
        public static int HEADER_CATEGORY = 3;
        public static int HEADER_SEARCH = 4;
        public static int HEADER_NXSEARCH = 5;
        public static int HEADER_TO_COUNT = 6;
        public static int HEADER_CURR_COUNT = 7;
        public static int HEADER_VIEW = 8;
        public static int HEADER_INIT_RANK = 9;
        public static int HEADER_CURR_RANK = 10;
        public static int HEADER_CREATED_AT = 11;
       
        bool isLoginSuccess = false;


        public Main()
        {
            InitializeComponent();
        }

        // 폼 초기화
        private void Form1_Load(object sender, EventArgs e)
        {
            // string ss =  LicenseUtils.Generate();
            //MobileUtils.EnAbleAirPlainMode();
            //MobileUtils.DisAbleAirPlainMode();       

         
            if (Properties.Settings.Default.USE_LOGIN)
            {
                var loginForm = new LoginForm();
                DialogResult result = loginForm.ShowDialog();

                if (!(result == DialogResult.OK))
                {
                    this.Close();
                    return;
                }             
            }

            // 데이터베이스 초기화
            SetUpDataBase();

            // 일반적 내용 초기화
            CommonInit();

            // 주기적인작업 설정
            SetIntervalWork();

            // 로그 초기화
            LogManager = new LogManager();
            LogManager.SetUp(textBox3);

            // 브라우저 초기화
            Browser = new ChromeBrowser();
            Browser.SetUp( this);
            Browser.SetLogManager( LogManager);

            // Naver 조회수 쓰레드 모듈 시작
            //naverAdThread = new Thread(new ThreadStart( NaverAdThread));
            //naverAdThread.Start();

            keyWordThread = new Thread(new ThreadStart(backGroundKeywordWork));
            keyWordThread.Start();
        }

        private void SetUpDataBase()
        {
            sqlLite = SQLite.GetInstance();
            sqlLite.CreateSQLiteDB();
            sqlLite.CreateTable();
            //sqlLite.InsertDataToTable();
            //sqlLite.ReadAllDataFromTable();
        }

        private void SetIntervalWork()
        {
            timer1.Interval = 1000;
            timer1.Enabled = true;
            timer1.Tick += new EventHandler(timer1_Tick);
        }

        
        public void ChangeRowColor(DataGridViewRow row, Color color)
        {            
            row.DefaultCellStyle.BackColor = color;                      
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel3.Text = DateTime.Now.ToString();
        }

        public void NaverAdThread()
        {
            var baseUrl = Properties.Settings.Default.BASE_URL;
            var apiKey = Properties.Settings.Default.API_KEY;
            var secretKey = Properties.Settings.Default.SECRET_KEY;
            var managerCustomerId = long.Parse(Properties.Settings.Default.CUSTOMER_ID);
            var rest = new SearchAdApi();

            while (true)
            {               
                if (dataGridView1.Rows.Count == 0)
                {
                    Thread.Sleep(5000);
                    continue;
                }

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if ( dataGridView1.Rows[i].Cells[ Main.HEADER_SLOT].Value == null)
                    {
                        continue;
                    }

                    try
                    {
                        var request = new RestRequest("/keywordstool", Method.GET);
                        request.AddQueryParameter("hintKeywords", (string)dataGridView1.Rows[i].Cells[HEADER_SEARCH].Value);
                        request.AddQueryParameter("showDetail", "1");
                        List<RelKwdStat> relKwdStats = rest.Execute<RelKwdStat>(request, "keywordList");

                        if (relKwdStats != null)
                        {
                            RelKwdStat relKwdStat = relKwdStats.First();

                            // 조회수
                            dataGridView1.Rows[i].Cells[Main.HEADER_VIEW].Value = relKwdStat.monthlyPcQcCnt + "/" + relKwdStat.monthlyMobileQcCnt;
                        }
                                                
                        Thread.Sleep(50);
                    }
                    catch(Exception e)
                    {
                        // 쓰레드가 동작 중인 상태에서 Row가 삭제될 경우 익셉션 발생.
                    }
                }                
            }        
        }

        internal void SetSearchRank(DataGridViewRow row ,int rank)
        {
            LogManager.AppendLog("랭크 계산 작업을 수행합니다.");
            string rankStr = rank == -1 ? "*" : rank.ToString();

            if (row.Cells[Main.HEADER_INIT_RANK].Value.Equals("*"))
            {
                row.Cells[Main.HEADER_INIT_RANK].Value = rankStr;
            }
            else
            {
                row.Cells[Main.HEADER_CURR_RANK].Value = rankStr;
            }
        }

        private void CommonInit()
        {
            //  시스템시간표시
            toolStripStatusLabel3.Text = DateTime.Now.ToString();

            // 아이피 레이블 초기화
            setExternalAddress( CommonUtils.GetExternalIPAddress());
            
            // 테이블 초기화
            SetupDataGridVIew();

            // 로직설정아이템 초기화
            InitLogic();

            // 슬롯 초기화
            List<Slot> slots = sqlLite.SelectAllSlots();
            AddDataGridRows(slots);

        }

        // 로직 콤보박스 초기화
        private void InitLogic()
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Items.Add(new ComboItem("MANUAL", "<직접입력>"));

            List<Logic> logics = sqlLite.SelectAllLogics();
            for (int i = 0; i < logics.Count; i++)
            {
                comboBox1.Items.Add(new ComboItem(logics[i].id, logics[i].name));
            }
            
            comboBox1.SelectedIndex = 0;
        }

        // 아이피 뷰 설정
        delegate void SetExtrnalAddressCallback(String address);
        public void setExternalAddress( string address)
        {

            SetExtrnalAddressCallback callback = new SetExtrnalAddressCallback(setExternalAddress);

            if (label4.InvokeRequired)
            {
                label4.Invoke(callback, address);
            }
            else
            {
                label4.Text = String.Format("{0}", address);
            }

            
        }

        // 테이블 모든 로우 반환
        public DataGridViewRowCollection getTableRows()
        {
            return dataGridView1.Rows;            
        }

        // 테이블 헤더 설정
        private void SetupDataGridVIew()
        {
            //this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AllowUserToAddRows = false;
            Controls.Add(dataGridView1);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.YellowGreen;            

            string[] headers = new string[] { "슬롯번호", "로직명", "목표횟수", "현재횟수", "순위", "메모", "등록일"};
            int[] widths = new int[] { 120, 200, 80, 80, 80, -1, 150};            

            dataGridView1.ColumnCount = headers.Length;

            for ( int i = 0; i < headers.Length; i++)
            {
                dataGridView1.Columns[i].Name =  headers[i];

                //if (!headers[i].Equals("목표횟수"))
                //{
                //    dataGridView1.Columns[i].ReadOnly = true;                    
                //}

                if (widths[i] == -1)
                {
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                else
                {
                    dataGridView1.Columns[i].Width = widths[i];
                }
                
            }                                 
        }

        // 현재횟수 +1
        public  void PlusCurrentCount(DataGridViewRow row)
        {
            row.Cells[Main.HEADER_CURR_COUNT].Value = (int.Parse((string)row.Cells[Main.HEADER_CURR_COUNT].Value) + 1).ToString();
        }
      
        //  작업 실행
        private void StartStopWorkBtn(object sender, EventArgs e)
        {
            if (browserThread == null || !browserThread.IsAlive)
            {
                if (!(dataGridView1.Rows.Count == 0) && isRemainTask())
                {
                    browserThread = new Thread(new ThreadStart(Browser.run));
                    browserThread.Start();
                    toolStripStatusLabel2.Text = "정상적으로 실행중입니다.";
                    EnableWorkBtn(false, "실행 중");
                }
                else
                {
                    MessageBox.Show("남아있는 작업이 없습니다. 슬롯을 추가해 주세요.", "경고");
                }
                
            }
            else
            {
                
                browserThread.Abort();
                toolStripStatusLabel2.Text = "정지";
                
            }                        
        }

        // 작업버튼 활성화 여부
        delegate void EnableWorkBtnCallBack(bool b, string t);
        public void EnableWorkBtn(bool doWork, string text)
        {
            //EnableWorkBtnCallBack Callback = new EnableWorkBtnCallBack(EnableWorkBtn);

            //if (button2.InvokeRequired)
            //{
            //    button2.Invoke(Callback, doWork, text);
            //}
            //else
            //{
            //    button2.Text = text;
            //    button2.Enabled = doWork;
            //}
            
        }

        // 작업이 남았는지 확인
        public bool isRemainTask()
        {

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {                

                    DataGridViewRow row = dataGridView1.Rows[i];                

                    if (((string)row.Cells[ HEADER_SEARCH].Value).Length == 0)
                    {
                        continue;
                    }

                    if (int.Parse((string)row.Cells[ HEADER_TO_COUNT].Value) > int.Parse((string)row.Cells[ HEADER_CURR_COUNT].Value))
                    {
                        return true;
                    }
                }
          
            return false;

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
      

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        //  Row 추가
        private void button4_Click(object sender, EventArgs e)
        {
           // if (textBox1.Text.Length == 0 || textBox2.Text.Length == 0)
            //{
           //     using (new CenterWinDialog(this))
           //     {
          //          MessageBox.Show("검색어와 노출어는 모두 입력하여야 합니다.", "경고");
          //      }
          //      return;
//}

            Slot slot = new Slot();
            slot.OID = DateUtils.GetCurrentTimeStamp().ToString();            
           // slot.category = comboBox2.Text;
           // slot.search = textBox1.Text;
            //slot.nxSearch = textBox2.Text;
            slot.toCount = "1";
            slot.currCount = "0";            
            slot.createdAt = DateTime.Now.ToString();
            //slot.agent = comboBox3.Text;
            //slot.browser = comboBox4.Text;

            AddDataGridRow(slot);

            sqlLite.InsertSlot(slot);
            LogManager.AppendLog("새로운 슬롯[{0}]이 추가되었습니다.", slot.OID);
        }
        
        private void AddDataGridRows(List<Slot> slots)
        {
            for( int i = 0; i < slots.Count; i++)
            {
                AddDataGridRow(slots.ElementAt(i));
            }
        }

        private void AddDataGridRow(Slot slot)
        {            
            dataGridView1.Rows.Add(CommonUtils.MakeArray(slot));
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

   
        private void Column1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column1_KeyPress);
            if (dataGridView1.CurrentCell.ColumnIndex == 3) //Desired Column
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column1_KeyPress);
                }
            }
        }

        // 환경설정 
        private void ClickConfiguration(object sender, EventArgs e)
        {
            Config MdiChild = new Config();            
            MdiChild.ShowDialog();
        }

        // NaverAd 쓰레드 종료
        private void CloseFormEvent(object sender, FormClosedEventArgs e)
        {
            
           
                // 데이터 베이스 쓰기 
                //SaveDataBase();

                // 쓰래드 종료
                AbortThread();
          
            
            
        
        }

        private void SaveDataBase()
        {
            DataGridViewRowCollection rows = dataGridView1.Rows;
            List<Slot> Slots = new List<Slot>();

            for( int i = 0; i < rows.Count; i++)
            {                                
                DataGridViewRow row = rows[i];
                Slot slot = new Slot();
                slot.OID =  (string) row.Cells[ HEADER_SLOT].Value;                 
                slot.toCount =  (string)row.Cells[HEADER_TO_COUNT].Value;
                slot.currCount = (string)row.Cells[HEADER_CURR_COUNT].Value;                
                slot.createdAt = (string)row.Cells[HEADER_CREATED_AT].Value;                

                Slots.Add(slot);
            }

            sqlLite.DeleteAllTables("Slot");
            sqlLite.InsertAllSlots(Slots);
        }

        private void AbortThread()
        {
            if (naverAdThread != null)
            {
                naverAdThread.Abort();
            }

            if (browserThread != null)
            {
                browserThread.Abort();
            }

            if (keyWordThread != null)
            {
                keyWordThread.Abort();
            }
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click_1(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel3_Click(object sender, EventArgs e)
        {

        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SlotAddForm MdiChild = new SlotAddForm();
            MdiChild.ShowDialog();
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        // 백그라운드 순위 검색
        private Queue<string> queue = new Queue<string>();
        private void backGroundKeywordWork()
        {
            while (true)
            {
                if (queue.Count == 0)
                {
                    continue;
                }
                
                string keyWord = queue.Dequeue();                

                var request = new RestRequest("/keywordstool", Method.GET);
                request.AddQueryParameter("hintKeywords", keyWord);
                request.AddQueryParameter("showDetail", "1");
                List<RelKwdStat> relKwdStats = restApi.Execute<RelKwdStat>(request, "keywordList");

                if (relKwdStats != null)
                {
                    RelKwdStat relKwdStat = relKwdStats.First();

                    // PC
                    this.Invoke(new Action(delegate ()
                    {
                        label5.Text = relKwdStat.monthlyPcQcCnt;

                        // Mb
                        label6.Text = relKwdStat.monthlyMobileQcCnt;

                        // To
                        label7.Text = (long.Parse(relKwdStat.monthlyPcQcCnt) + long.Parse(relKwdStat.monthlyMobileQcCnt)).ToString();
                    }));                                    
                }
            }
        }


        // 키워드 순위 검색
        private void pictureBox1_Click_1(object sender, EventArgs e)
        {         
            string keyword = textBox1.Text;
            queue.Enqueue(keyword);
        }

        private void NaverKeywordSearch(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


        // 순위조회
        private void SearchRanking(object sender, EventArgs e)
        {
            Ranking MdiChild = new Ranking();
            MdiChild.ShowDialog();
        }

        // 슬롯 추가
        private void pictureBox6_Click(object sender, EventArgs e)
        {
            ComboItem item = comboBox1.SelectedItem as ComboItem;
            ;

            if (item.Key.Equals("MANUAL"))
            {
                SlotAddForm MdiChild = new SlotAddForm();
                MdiChild.ShowDialog();
            }
            else
            {                            
                Slot slot = new Slot();
                slot.OID = DateUtils.GetCurrentTimeStamp().ToString();
                slot.logicName = comboBox1.Text;
                slot.rank = "*";
                slot.toCount = "1";
                slot.currCount = "0";
                //slot.description = "";
                slot.createdAt = DateTime.Now.ToString();
                AddDataGridRow(slot);
            }            
        }

        // 슬롯 삭제
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                string SlotNumber = (string)row.Cells[HEADER_SLOT].Value;
                if (SlotNumber == null || SlotNumber.Length == 0)
                {
                    continue;
                }

                dataGridView1.Rows.Remove(row);
                LogManager.AppendLog("슬롯[{0}]이 삭제되었습니다.", SlotNumber);
            }
        }

        // 슬롯 수정
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("선택된 슬롯이 없습니다.");
            }
            else if (dataGridView1.SelectedRows.Count > 1)
            {
                MessageBox.Show("수정은 하나의 슬롯만 선택하여 할 수 있습니다.");
            }
            else
            {
                DataGridViewRow row = dataGridView1.SelectedRows[0];
                string SlotNumber = (string)row.Cells[HEADER_SLOT].Value;

                SlotAddForm MdiChild = new SlotAddForm(SlotNumber);
                MdiChild.ShowDialog();
            }
        }
    }
}

