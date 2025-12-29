using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace alimentBAll
{
    public partial class FormPassword : Form
    {
        public enum PasswordResult
        {
            Allow,
            Stop
        }

        public PasswordResult Result { get; private set; }

        public FormPassword()
        {
            InitializeComponent();
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text == "123")
            {
                Properties.Settings.Default.IsActivated = true;
                Properties.Settings.Default.Save();
                this.Close();
            }
            else if (txtPassword.Text == "1234")
            {
                Properties.Settings.Default.IsBlocked = true;
                Properties.Settings.Default.Save();
                this.Close();
            }
            else
            {
                MessageBox.Show("كلمة السر غير صحيحة", "خطأ");
            }
        }
    }

}
