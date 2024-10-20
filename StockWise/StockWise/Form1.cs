using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockWise
{
    public partial class LoginPage : Form
    {
        public LoginPage()
        {
            InitializeComponent();
            InitializeRegisterLabel();
        }
        private void InitializeRegisterLabel()
        {
            registerLabel.Text = "Haven't you registered?";
            registerLabel.ForeColor = System.Drawing.Color.Blue; 
            registerLabel.Cursor = Cursors.Hand; 
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void registerLabel_Click(object sender, EventArgs e)
        {
            
            RegisterPage registerForm = new RegisterPage();
            registerForm.Show(); 
            this.Hide(); 
        }

    }
}
