using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winner
{
    public class LogManager
    {     
        private TextBox textBox;
        
        // 로그출력
        public void AppendLog(string log, params object[] list)
        {
           textBox.BeginInvoke(new Action(() =>
            {
                if (list.Length > 0)
                {
                    textBox.AppendText("[" + DateTime.Now.ToString() + "] " + string.Format(log, list));
                }
                else
                {
                    textBox.AppendText("[" + DateTime.Now.ToString() + "] " + log);
                }
                
                textBox.AppendText(Environment.NewLine);
                textBox.SelectionStart = textBox.Text.Length;
                textBox.ScrollToCaret();
            }));
            
        }

        public void SetUp( TextBox textBox)
        {
            this.textBox = textBox;
        }
    }
}
