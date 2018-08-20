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
    
    public partial class Prompt : Form
    {
        private Action<string> callback;
        public Prompt( string promptInstructions, Action<string> action)
        {
            InitializeComponent();
            label1.Text = promptInstructions;            
            callback = action;
        }
        
        private void BtnSubmitText_Click(object sender, EventArgs e)
        {                        
            callback(textBox1.Text);
            Close();
        }   
    }
}
