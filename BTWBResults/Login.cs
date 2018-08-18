using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTWBResults
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            this.textBoxUserName.Text = Settings1.Default.username;
            this.textBoxPassword.Text = Settings1.Default.pass;
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            SetData();
        }

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SetData();

        }

        private void SetData()
        {
            Settings1.Default.username = this.textBoxUserName.Text;
            Settings1.Default.pass = this.textBoxPassword.Text;
            Settings1.Default.Save();
            this.Close();
        }
    }
}
