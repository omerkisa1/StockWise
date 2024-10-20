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
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void registerLabel_Click(object sender, EventArgs e)
        {
            registerLabel.Text = "Haven't you registered?";
            registerLabel.ForeColor = System.Drawing.Color.Blue; // Mavi renk, tıklanabilir izlenimi verir
            registerLabel.Cursor = Cursors.Hand; // El simgesi, tıklanabilir olduğunu gösterir

            // Register formunu göster
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();
            this.Hide(); // Login formunu gizlemek isterseniz

        }
    }
}
