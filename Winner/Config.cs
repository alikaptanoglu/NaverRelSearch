using Naver.SearchAd;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winner
{
    public partial class Config : Form
    {
        SQLite sqlite;

        // 현재 로직 아이디
        string currentLogicId; 
        delegate void MousePostionCheckerCallBack(object sender, System.Timers.ElapsedEventArgs e);
        
        public Config()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            sqlite = SQLite.GetInstance();

            // 공통초기화
            CommonInit();
            
            // 초기 설정 세팅
            SetInitConfiguration();
        }

        private void CommonInit()
        {
            // 그리드초기화
            InitDataGridView();        

            // 로직 세팅 처리
            InitLogic();

            // 로직입력값 초기화
            InitLogicInput(currentLogicId);

            // 로직아이템 초기화
            InitLogicItem(currentLogicId);
        }

        class ComboItem
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

            // 로직 콤보박스 초기화
            private void InitLogic()
        {
            List<Logic> logics = sqlite.SelectAllLogics();

            for (int i = 0; i < logics.Count; i++)
            {
                comboBox1.Items.Add(new ComboItem(logics[i].id, logics[i].name));
            }
            comboBox1.SelectedIndex = 0;
            currentLogicId = logics[0].id;        
        }

        // 로직아이템 초기화
        private void InitLogicItem(string id)
        {
            List<LogicItem> logicItem = sqlite.SelectAllLogicItemsByLogicId(id);
            AddDataGridRow(logicItem);            
        }

        // 로직입력값 초기화
        private void InitLogicInput(string id)
        {
            List<LogicInput> logicInputs = sqlite.SelectAllLogicInputsByLogicId( id);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            foreach (LogicInput item in logicInputs)
            {
                dictionary.Add(item.key, item.value);
            }

            // 에이전트 초기화
            string AGENT = dictionary[LogicInput.CONST_AGNET];
            comboBox2.SelectedItem = AGENT;            

            // 브라우저 초기화
            string BROWSER = dictionary[LogicInput.CONST_BROWSER];
            comboBox3.SelectedItem = BROWSER;

            // 중복아이피 허용횟수
            string DUPLICATE_ADDRESS = dictionary[LogicInput.CONST_DUPLICATE_ADDRESS];
            textBox1.Text = DUPLICATE_ADDRESS;

            // 키워드 초기화
            string[] tokenKeyword  = dictionary[LogicInput.CONST_KEYWORD].Split(CommonUtils.delimiterChars);
            string keyword = tokenKeyword[0];
            string postion = tokenKeyword[1];
            textBox2.Text = keyword;
            comboBox4.SelectedItem = postion;

            // 체류시간
            string[] tokenStay = dictionary[LogicInput.CONST_STAY].Split(CommonUtils.delimiterChars);
            string start = tokenStay[0];
            string end = tokenStay[1];
            textBox3.Text = start;
            textBox4.Text = end;

            // 스크롤
            string[] tokenScroll = dictionary[LogicInput.CONST_SCROLL].Split(CommonUtils.delimiterChars);
            textBox8.Text = tokenScroll[0];
            textBox7.Text = tokenScroll[1];
            textBox10.Text = tokenScroll[2];
            textBox9.Text = tokenScroll[3];
            textBox12.Text = tokenScroll[4];
            textBox11.Text = tokenScroll[5];

            // 게시글조회
            string POST_VIEW = dictionary[LogicInput.CONST_POST_VIEW];
            textBox5.Text = POST_VIEW;

            // 카테고리 이동
            string CATEGORY = dictionary[LogicInput.CONST_CATEGORY_MOVE];
            comboBox5.SelectedItem = CATEGORY;
        }

        // 로직 저장 처리
        private void SaveLogic(object sender, EventArgs e)
        {            
            // 로직입력값 저장
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add(LogicInput.CONST_KEYWORD, CommonUtils.MakeDelimeterItem( textBox2.Text, comboBox4.Text));
            dictionary.Add(LogicInput.CONST_STAY, CommonUtils.MakeDelimeterItem(textBox3.Text, textBox4.Text));
            dictionary.Add(LogicInput.CONST_SCROLL, CommonUtils.MakeDelimeterItem(textBox8.Text, textBox7.Text, textBox10.Text, textBox9.Text, textBox12.Text, textBox11.Text));
            dictionary.Add(LogicInput.CONST_POST_VIEW, textBox5.Text);            
            dictionary.Add(LogicInput.CONST_CATEGORY_MOVE, comboBox5.Text);
            dictionary.Add(LogicInput.CONST_AGNET, comboBox2.Text);
            dictionary.Add(LogicInput.CONST_BROWSER, comboBox3.Text);
            dictionary.Add(LogicInput.CONST_DUPLICATE_ADDRESS, textBox1.Text);

            List<LogicInput> logicInputs = LogicInput.ConvertMapToObject( dictionary, currentLogicId);

            sqlite.DeleteLogicInputByLogicId(currentLogicId);
            sqlite.InsertAllLogicInpts( logicInputs);


            // 로직 아이템 저장
            DataGridViewRowCollection rows = dataGridView1.Rows;
            List<LogicItem> LogicItems = new List<LogicItem>();

            for (int i = 0; i < rows.Count; i++)
            {
                DataGridViewRow row = rows[i];
                LogicItem item = new LogicItem();
                item.logicId = currentLogicId;
                item.sequence = row.Cells[LogicItem.HEADER_SEQUENCE].Value.ToString();
                item.action = (string)row.Cells[LogicItem.HEADER_ACTION].Value;
                item.value = (string)row.Cells[LogicItem.HEADER_VALUE].Value;
                LogicItems.Add( item);
            }

            sqlite.DeleteAllLogicItems( currentLogicId);
            sqlite.InsertAllLogicItems( LogicItems);
        }

        private void InitDataGridView()
        {
            dataGridView1.AllowUserToAddRows = false;
            //Controls.Add(dataGridView1);
            //dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            //dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.YellowGreen;
        }



        private void SetInitConfiguration()
        {
            List<Configuration> configs = sqlite.SelectAllConfigurationByOwner(Configuration.Default);

            for (int i = 0; i < configs.Count; i++)
            {
                Configuration config = configs.ElementAt(i);
                SetConfig(config.key, config.value);
            }
        }

        private void SetConfig(string key, string value)
        {

            switch (key)
            {
                //    case Configuration.COMMON_SEARCH_STAY :
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        textBox1.Text = values[0];
                //        textBox2.Text = values[1];
                //    } break;
                //    case Configuration.COMMON_INIT_PAGE_STAY:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        textBox4.Text = values[0];
                //        textBox3.Text = values[1];
                //    }
                //    break;
                //    case Configuration.COMMON_MAX_PAGE:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        textBox19.Text = values[0];
                //        textBox18.Text = values[1];
                //    }
                //    break;
                //    case Configuration.PAGE_VIEW_ANOHTERPAGE_USE:
                //    {
                //        checkBox1.Checked = bool.Parse( value);
                //    }
                //    break;
                //    case Configuration.PAGE_VIEW_ANOHTERPAGE:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        textBox8.Text = values[0];
                //        textBox7.Text = values[1];
                //    }
                //    break;
                //    case Configuration.PAGE_VIEW_ANOHTERPAGE_STAY:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        textBox10.Text = values[0];
                //        textBox9.Text = values[1];
                //    }
                //    break;
                //    case Configuration.PAGE_STAY_USE:
                //    {
                //        checkBox2.Checked = bool.Parse(value);
                //    }
                //    break;
                //    case Configuration.PAGE_STAY:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        textBox6.Text = values[0];
                //        textBox5.Text = values[1];
                //    }
                //    break;
                //    case Configuration.COMMON_PRE_SEARCH_STAY:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        textBox12.Text = values[0];
                //        textBox11.Text = values[1];
                //    }
                //    break;
                //    case Configuration.COMMON_PRE_HISTORY_MOVE_STAY:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        textBox17.Text = values[0];
                //        textBox16.Text = values[1];
                //    }
                //    break;
                //    case Configuration.COMMON_POST_SEARCH_SCROLL_USE:
                //    {
                //            checkBox4.Checked = bool.Parse(value);
                //    }
                //    break;
                //    case Configuration.COMMON_POST_SEARCH_SCROLL:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_LIST_CHARS);

                //        for (int i = 0; i < values.Length; i++)
                //        {
                //            string[] items = values[i].Split(Configuration.DELIMITER_CHARS);

                //            if (i == 0)
                //            {
                //                textBox14.Text = items[0];
                //                textBox27.Text = items[1];
                //            }
                //            else if (i == 1)
                //            {
                //                textBox28.Text = items[0];
                //                textBox13.Text = items[1];
                //            }
                //            else if (i == 2)
                //            {
                //                textBox15.Text = items[0];
                //                textBox29.Text = items[1];
                //            }

                //        }                                      
                //    }
                //    break;              
                //    case Configuration.IP_CAHNGE_STAY:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        textBox21.Text = values[0];
                //        textBox20.Text = values[1];
                //    }
                //    break;
                //    case Configuration.COOKIE_DELETE_POST_STAY:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        textBox23.Text = values[0];
                //        textBox22.Text = values[1];
                //    }
                //    break;
                //    case Configuration.START_PAGE_STAY:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        textBox26.Text = values[0];
                //        textBox25.Text = values[1];
                //    }
                //    break;
                //    case Configuration.IP_AGENT_TYPE:
                //    {
                //        CheackRaidoBox( value);                    
                //    }
                //    break;
                //    case Configuration.START_PAGE_LIST:
                //    {
                //        string[] values = value.Split(Configuration.DELIMITER_CHARS);
                //        foreach (string item in values)
                //        {
                //                listBox1.Items.Add( item);
                //        }
                //    }
                //    break;

            }
        }

        private void CheackRaidoBox(string value)
        {
            //switch (value)
            //{
            //    case "Proxy": { radioButton1.Checked = true; ipCurrentRaidoText = "Proxy"; } break;
            //    case "Tethering": { radioButton3.Checked = true; ipCurrentRaidoText = "Tethering"; } break;

            //}
        }



        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        // 폼이 닫히기전에 모든 데이터 저장 처리
        private void SaveConfig(object sender, FormClosingEventArgs e)
        {
            //Dictionary<string, string> Configs = new Dictionary<string, string>();

            //// 필수 기본설정              
            //Configs.Add(Configuration.COMMON_SEARCH_STAY, textBox1.Text + "," + textBox2.Text); // 검색, 카테고리 클릭 후 체류시간
            //Configs.Add(Configuration.COMMON_INIT_PAGE_STAY, textBox4.Text + "," + textBox3.Text); // 최초 방문페이지 체류시간(초)
            //Configs.Add(Configuration.COMMON_MAX_PAGE, textBox19.Text + "," + textBox18.Text); // 최대 검색할 페이지
            //Configs.Add(Configuration.COMMON_PRE_HISTORY_MOVE_STAY, textBox17.Text + "," + textBox16.Text); // 이전 히스토리 이동 후 체류시간

            //// 최초 방문페이지 작업설정
            //Configs.Add(Configuration.PAGE_VIEW_ANOHTERPAGE_USE, checkBox1.Checked.ToString()); // 방문페이지 내 다른 포스팅 보기 사용여부
            //Configs.Add(Configuration.PAGE_VIEW_ANOHTERPAGE, textBox8.Text + "," + textBox7.Text); // 방문페이지 내 다른 포스팅 보기
            //Configs.Add(Configuration.PAGE_VIEW_ANOHTERPAGE_STAY, textBox10.Text + "," + textBox9.Text); // 체류시간
            //Configs.Add(Configuration.PAGE_STAY_USE, checkBox2.Checked.ToString()); // 방문페이지 체류시간 사용여부
            //Configs.Add(Configuration.PAGE_STAY, textBox6.Text + "," + textBox5.Text); // 방문페이지 체류시간

            ////검색 설정
            //Configs.Add(Configuration.COMMON_PRE_SEARCH_STAY, textBox12.Text + "," + textBox11.Text); // 검색전 체류시간(초)
            //Configs.Add(Configuration.COMMON_POST_SEARCH_SCROLL_USE, checkBox4.Checked.ToString()); // 스크롤 사용
            //StringBuilder sb = new StringBuilder();
            //  sb.Append(textBox14.Text).Append(",").Append(textBox27.Text).Append("|")
            //    .Append(textBox28.Text).Append(",").Append(textBox13.Text).Append("|")
            //    .Append(textBox15.Text).Append(",").Append(textBox29.Text);
            //Configs.Add(Configuration.COMMON_POST_SEARCH_SCROLL, sb.ToString()); // 스크롤 처리

            ////아이피설정            
            //Configs.Add(Configuration.IP_AGENT_TYPE, ipCurrentRaidoText); // IP 변경 에이전트            
            //Configs.Add(Configuration.IP_CAHNGE_STAY, textBox21.Text + "," + textBox20.Text); // 아이피 변경 후 체류 시간
            //Configs.Add(Configuration.COOKIE_DELETE_POST_STAY, textBox23.Text + "," + textBox22.Text); // 쿠키 삭재 후 체류시간

            //// 시작페이지 설정
            //ListBox.ObjectCollection items = listBox1.Items;
            //sb = new StringBuilder();
            //for (int i = 0; i < items.Count; i++)
            //{
            //    sb.Append(items[i].ToString());
            //    if (i != items.Count - 1)
            //    {
            //        sb.Append(",");
            //    }
            //}

            //Configs.Add(Configuration.START_PAGE_LIST, sb.ToString()); // 시작페이지 리스트                               
            //Configs.Add(Configuration.START_PAGE_STAY, textBox26.Text + "," + textBox25.Text); // 시작페이지 리스트




            //List<Configuration> configs = Configuration.ConvertMapToObject(Configs);

            //sqlite.DeleteAllConfigurationByOwner(Configuration.Default);
            //sqlite.InsetAllConfiguration(configs);
        }

        private void RadioButtonClick(object sender, EventArgs e)
        {
            //switch (((RadioButton)sender).Text)
            //{
            //    case "프록시아이피": { ipCurrentRaidoText = "Proxy"; } break;
            //    case "모바일테더링": { ipCurrentRaidoText = "Tethering"; } break;

            //    case "IE": { browserCurrentRaidoText = "IE"; } break;
            //    case "Chrome": { browserCurrentRaidoText = "Chrome"; } break;
            //    case "FireFox": { browserCurrentRaidoText = "FireFox"; } break;
            //    case "랜덤": { browserCurrentRaidoText = "BrowserRandom"; } break;
            //}
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {

        }

        // 시작페이지 추가
        private void AddStartPage(object sender, EventArgs e)
        {

            //if (listBox1.Items.Contains(textBox24.Text))
            //{
            //    MessageBox.Show("이미 추가되어 있는 항목입니다.");
            //}
            //else if (textBox24.Text.Trim().Length == 0)
            //{
            //    MessageBox.Show("공백은 추가 할 수 없습니다.");
            //}
            //else
            //{
            //    listBox1.Items.Add(textBox24.Text);
            //    textBox24.Text = "";
            //}

        }

        // 시작페이지 삭제
        private void DeleteStartPage(object sender, EventArgs e)
        {
            //ListBox.SelectedObjectCollection items = listBox1.SelectedItems;

            //for (int i = items.Count - 1; i >= 0; i--)
            //{                
            //    listBox1.Items.Remove(items[i]);
            //}
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {

        }

        // 로우즈 추가
        private void AddDataGridRow(List<LogicItem> items)
        {
            foreach (LogicItem item in items)
            {
                AddDataGridRow(item);
            }            
        }

        // 로우 추가
        private void AddDataGridRow(LogicItem item)
        {
            dataGridView1.Rows.Add(CommonUtils.MakeArray( item));
        }

        // 액션 추가
        private void AddAction(object sender, EventArgs e)
        {
            PictureBox o = (PictureBox)sender;
            LogicItem item = new LogicItem();
            string action = null;
            string value = null;

            switch (o.Name)
            {
                    case "ActionKeyword":
                    {
                        action = "키워드";
                        value = textBox2.Text + "/" + comboBox4.Text;
                        if (!IsVaildate(action, value))
                        {
                            MessageBox.Show("값이 비어 있는 액션은 추가 할 수 없습니다.");
                            return;
                        };
                    }
                    break;
                    case "ActionStay":
                    {
                        action = "체류";
                        value = textBox3.Text + "/"+ textBox4.Text;
                    }
                    break;
                    case "ActionScroll":
                    {
                        action = "스크롤";
                        value = textBox8.Text + "/" + textBox7.Text  + "/" + textBox10.Text + "/" + textBox9.Text + "/" + textBox12.Text + "/" + textBox11.Text;
                    }
                    break;
                    case "ActionView":
                    {
                        action = "게시글조회";
                        value = textBox5.Text;
                    }
                    break;
                    case "ActionHistoryPrev":
                    {
                        action = "히스토리";
                        value = "Prev";
                    }
                    break;
                    case "ActionHistoryNext":
                    {
                        action = "히스토리";
                        value = "Next";
                    }
                    break;
                    case "ActionMoveCategory":
                    {
                        action = "카테고리 이동";
                        value = comboBox5.Text;
                    }
                    break;
            }
                       
            item.action = action;
            item.value = value;

            AddDataGridRow(item);
            AutoSequence();
        }

        private bool IsVaildate(string action, string value)
        {
            if (ObjectUtils.isNull(action) || ObjectUtils.isNull(value))
            {                
                return false;
            }

            return true;
        }

        // 자동 시퀀스 처리
        private void AutoSequence()
        {
            int cellnum = 0;
            int rownum = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                cellnum = cellnum + 1;
                dataGridView1.Rows[rownum].Cells[0].Value = cellnum;
                rownum = rownum + 1;
            }
        }

        // 로직 변경
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboItem item = comboBox1.SelectedItem as ComboItem;
            currentLogicId = item.Key;
            // 로직입력값 초기화
            InitLogicInput(currentLogicId);
            // 로직아이템 초기화
            InitLogicItem(currentLogicId);
        }
    }
}

