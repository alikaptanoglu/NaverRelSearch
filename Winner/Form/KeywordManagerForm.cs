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
    public partial class KeywordManagerForm : Form
    {
        SQLite SQL;

        private static int HEADER_KEYWORD = 0;

        public KeywordManagerForm()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            CommonInit();
            InitDataLoad();
        }

        private void InitDataLoad()
        {
            // 그리드 데이터 로드            
            List<Keyword> keywords = SQL.SelectAllKeyword();
            AddDataGridRows( keywords);
        }

        private void CommonInit()
        {
            // 그리드 설정
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);

            // SQL 설정
            SQL = SQLite.GetInstance();
        }

        private void AddKeyWord(object sender, EventArgs e)
        {
            AddKeyWord();
        }

        // 키워드 삭제
        private void RemoveKeyWord(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                dataGridView1.Rows.Remove(row);
            }
        }

        // 키워드 추가
        private void AddKeyWord()
        {
            string keyword = textBox2.Text;            

            if (keyword.Trim().Length == 0 )
            {
                MessageBox.Show("키워드 반드시 입력해야 합니다.", "경고");
                return;
            }

            string[] row = { keyword };
            dataGridView1.Rows.Add(row);
        }


        // 그리드 저장
        private void Unload(object sender, FormClosingEventArgs e)
        {            
            DataGridViewRowCollection rows = dataGridView1.Rows;
            List<Keyword> models = new List<Keyword>();
            for (int i = 0; i < rows.Count; i++)
            {
                DataGridViewRow row = rows[i];
                Keyword model = new Keyword();
                model.keyword = (string)row.Cells[HEADER_KEYWORD].Value;                
                models.Add(model);
            }

            SQL.DeleteAllTables(Keyword.TableName);
            SQL.InsertAllKeyword( models);
        }

        private void AddDataGridRows(List<Keyword> keywords)
        {
            for (int i = 0; i < keywords.Count; i++)
            {
                AddDataGridRow(keywords.ElementAt(i));
            }
        }

        private void AddDataGridRow(Keyword keyword)
        {
            dataGridView1.Rows.Add(CommonUtils.MakeArray(keyword));
        }
    }

   
}
