using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSIXProjectArtifactory
{
    public partial class UserInputForm : Form
    {
        private string customMessage;
        private TextBox textBox1;

        public UserInputForm()
        {
            textBox1 = new TextBox();
            this.Controls.Add(textBox1);
        }

        public string get_CustomMessage()
        {
            return customMessage;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            customMessage = textBox1.Text;
            this.Dispose();
        }
    }
}
