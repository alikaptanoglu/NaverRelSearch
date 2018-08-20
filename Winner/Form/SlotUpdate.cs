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
using Winner.Service;

namespace Winner
{
    public partial class SlotUpdate : Form
    {
        
        SQLite sqlite;

        // 현재 로직 아이디
        string currentLogicId;
        private string slotId;

        delegate void MousePostionCheckerCallBack(object sender, System.Timers.ElapsedEventArgs e);

        public SlotUpdate()
        {
            InitializeComponent();
        }

        public SlotUpdate(string slotId)
        {
            this.slotId = slotId;
            InitializeComponent();
        }


        private void SlotUpdate_Load(object sender, EventArgs e)
        {
            sqlite = SQLite.GetInstance();

            Slot slot = sqlite.SelectSlotBySlotId(slotId);

            currentLogicId = slot.logicId;

            // 테이블 헤더 설정
            SetupDataGridVIew();

            // 공통초기화
            CommonInit();
            
            // 로직입력값 초기화
            InitLogicInput(currentLogicId);

            // 로직아이템 초기화
            InitLogicItem(currentLogicId);
        }

        // 테이블 헤더 설정
        private void SetupDataGridVIew()
        {
            //this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // dataGridView1.AllowUserToAddRows = false;
            //Controls.Add(dataGridView1);
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.YellowGreen;

            string[] headers = new string[] { "순번", "액션", "값" };
            int[] widths = new int[] { -1, 160, 160 };

            dataGridView1.ColumnCount = headers.Length;

            for (int i = 0; i < headers.Length; i++)
            {
                dataGridView1.Columns[i].Name = headers[i];

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

        private void CommonInit()
        {
            // 콤보박스초기화
            comboBox1.Enabled = false;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox4.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox5.DropDownStyle = ComboBoxStyle.DropDownList;
            

            // 그리드초기화
            InitDataGridView();

            // 로직 세팅 처리
            InitLogic();

            // 로직입력값 초기화
            //InitLogicInput(currentLogicId);

            // 로직아이템 초기화
            //InitLogicItem(currentLogicId);
        }

        // 로직 콤보박스 초기화
        private void InitLogic()
        {
            comboBox1.Items.Clear();

            Logic logic = sqlite.SelectLogicById(currentLogicId);
                     
            comboBox1.Items.Add(new ComboItem(logic.id, logic.name));
            
            comboBox1.SelectedIndex = 0;
            currentLogicId = logic.id;
        }

        // 로직아이템 초기화
        private void InitLogicItem(string id)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();
            List<LogicItem> logicItem = sqlite.SelectAllLogicItemsByLogicId(id);
            AddDataGridRow(logicItem);
        }

        // 로직입력값 초기화
        private void InitLogicInput(string id)
        {
            List<LogicInput> logicInputs = sqlite.SelectAllLogicInputsByLogicId(id);
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
            string[] tokenKeyword = dictionary[LogicInput.CONST_KEYWORD].Split(CommonUtils.delimiterChars);
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

            // 슬롯 대기시간
            string[] SLOT_WAIT_TIME = dictionary[LogicInput.CONST_SLOT_WAIT_TIME].Split(CommonUtils.delimiterChars);
            textBox6.Text = SLOT_WAIT_TIME[0];
            textBox13.Text = SLOT_WAIT_TIME[1];
        }

        // 로직 저장 처리
        private void SaveLogic(object sender, EventArgs e)
        {
            ExecuteSave(currentLogicId);
        }

        private void ExecuteSave(string logicId)
        {
            // 로직입력값 저장
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add(LogicInput.CONST_KEYWORD, CommonUtils.MakeDelimeterItem(textBox2.Text, comboBox4.Text));
            dictionary.Add(LogicInput.CONST_STAY, CommonUtils.MakeDelimeterItem(textBox3.Text, textBox4.Text));
            dictionary.Add(LogicInput.CONST_SCROLL, CommonUtils.MakeDelimeterItem(textBox8.Text, textBox7.Text, textBox10.Text, textBox9.Text, textBox12.Text, textBox11.Text));
            dictionary.Add(LogicInput.CONST_POST_VIEW, textBox5.Text);
            dictionary.Add(LogicInput.CONST_CATEGORY_MOVE, comboBox5.Text);
            dictionary.Add(LogicInput.CONST_AGNET, comboBox2.Text);
            dictionary.Add(LogicInput.CONST_BROWSER, comboBox3.Text);
            dictionary.Add(LogicInput.CONST_DUPLICATE_ADDRESS, textBox1.Text);
            dictionary.Add(LogicInput.CONST_SLOT_WAIT_TIME, CommonUtils.MakeDelimeterItem(textBox6.Text, textBox13.Text));

            List<LogicInput> logicInputs = LogicInput.ConvertMapToObject(dictionary, logicId);

            sqlite.DeleteLogicInputByLogicId(logicId);
            sqlite.InsertAllLogicInpts(logicInputs);


            // 로직 아이템 저장
            DataGridViewRowCollection rows = dataGridView1.Rows;
            List<LogicItem> LogicItems = new List<LogicItem>();

            for (int i = 0; i < rows.Count; i++)
            {
                DataGridViewRow row = rows[i];
                LogicItem item = new LogicItem();
                item.logicId = logicId;
                item.sequence = row.Cells[LogicItem.HEADER_SEQUENCE].Value.ToString();
                item.action = (string)row.Cells[LogicItem.HEADER_ACTION].Value;
                item.value = (string)row.Cells[LogicItem.HEADER_VALUE].Value;
                LogicItems.Add(item);
            }

            sqlite.DeleteLogicItemByLogicId(logicId);
            sqlite.InsertAllLogicItems(LogicItems);

            Close();
        }

        private void InitDataGridView()
        {
            dataGridView1.AllowUserToAddRows = false;
            //Controls.Add(dataGridView1);
            //dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            //dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.YellowGreen;
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

        private void propertyGrid1_Click(object sender, EventArgs e)
        {

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
            dataGridView1.Rows.Add(CommonUtils.MakeArray(item));
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
                        value = textBox3.Text + "/" + textBox4.Text;
                    }
                    break;
                case "ActionScroll":
                    {
                        action = "스크롤";
                        value = textBox8.Text + "/" + textBox7.Text + "/" + textBox10.Text + "/" + textBox9.Text + "/" + textBox12.Text + "/" + textBox11.Text;
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
                        action = "카테고리";
                        value = comboBox5.Text;
                    }
                    break;
                case "ActionMoveHome":
                    {
                        action = "홈";
                        value = "Move";
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

     
        // 로직아이템 선택삭제
        private void button3_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in this.dataGridView1.SelectedRows)
            {
                dataGridView1.Rows.RemoveAt(row.Index);
            }
            AutoSequence();
        }

        // 로직아이템 삭제
        private void button1_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
            }
        }

        // 다른이름으로 로직 저장
        private void SaveAsLogic(object sender, EventArgs e)
        {
            var t = new Prompt("새로운 로직명을 입력하세요.", SaveAsLogicCallback);
            t.ShowDialog();
        }

        // 다른이름으로 로직 저장 콜백
        private void SaveAsLogicCallback(string asLogicName)
        {
            if (asLogicName.Trim().Length == 0)
            {
                MessageBox.Show("로직명은 빈값일 수 없습니다.");
                return;
            }

            List<Logic> logics = sqlite.SelectAllLogics();
            foreach (Logic item in logics)
            {
                if (item.name.Equals(asLogicName))
                {
                    MessageBox.Show("이미 같은 이름의 로직이 존재합니다.");
                    return;
                }
            }

            string logicId = DateUtils.GetCurrentTimeStamp().ToString();

            // 로직저장
            Logic logic = new Logic();
            logic.id = logicId;
            logic.name = asLogicName;
            logic.type = Logic.CONST_TYPE_CONFIG;
            logic.createdAt = DateTime.Now.ToString();
            sqlite.InsertLogic(logic);

            // 로직 인풋, 아이템 저장
            ExecuteSave(logicId);

            // 로직 초기화
            InitLogic();

            // 새로만든 로직 선택
            // comboBox1.SelectedItem = asLogicName;
        }

        // 로직 삭제
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (currentLogicId == "DEFAULT")
            {
                MessageBox.Show("기본로직은 삭제 할 수 없습니다.");
            }
            else
            {
                sqlite.DeleteLogicInputByLogicId(currentLogicId);
                sqlite.DeleteLogicItemByLogicId(currentLogicId);
                sqlite.DeleteLogicByLogicId(currentLogicId);

                // 로직 초기화
                InitLogic();
            }
        }

        // 로직 리셋 ( DB데이터로 원상복구)
        private void pictureBox9_Click(object sender, EventArgs e)
        {
            // 로직입력값 초기화
            InitLogicInput(currentLogicId);

            // 로직아이템 초기화
            InitLogicItem(currentLogicId);
        }


        // 그리드 행 업
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            UIService.DataGridViewRowMoveUp(dataGridView1);
            UIService.AutoSequence(dataGridView1);
        }

        // 그리드 행 아래로
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            UIService.DataGridViewRowMoveDown(dataGridView1);
            UIService.AutoSequence(dataGridView1);
        }

        // 그리드 로우 최상단으로 이동
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            UIService.DataGridViewRowMoveTop(dataGridView1);
            UIService.AutoSequence(dataGridView1);
        }

        // 그리드 로우 최하단으로 이동
        private void pictureBox6_Click(object sender, EventArgs e)
        {
            UIService.DataGridViewRowMoveBottom(dataGridView1);
            UIService.AutoSequence(dataGridView1);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
