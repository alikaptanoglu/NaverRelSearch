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
    public partial class LoginForm : Form
    {
        SQLite sqlite;

        public LoginForm()
        {
            InitializeComponent();
            sqlite = SQLite.GetInstance();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            textBox2.PasswordChar = '*';            
        }

      
        // 로그인 클릭
        private void Login_Click(object sender, EventArgs e)
        {
            string account = textBox1.Text;
            string password = textBox2.Text;

            if (sqlite.ExistAccount(account, password))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("아이디 혹은 패스워드가 일치하지 않습니다.");
            }
        }
    }
}
