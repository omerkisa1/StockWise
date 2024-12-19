using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Bson;
using MongoDB.Driver;

namespace StockWise
{
    public partial class LoginPage : Form
    {
        public LoginPage()
        {
            InitializeComponent();
            InitializeRegisterLabel();
        }


        private IMongoCollection<BsonDocument> GetUserCollection()
        {
            try
            {
                var client = new MongoClient("mongodb://localhost:27017");
                var database = client.GetDatabase("users");
                return database.GetCollection<BsonDocument>("userInfos");
            }
            catch (Exception ex)
            {
                MessageBox.Show("MongoDB bağlantı hatası: " + ex.Message);
                return null;
            }
        }

        private void InitializeRegisterLabel()
        {
            registerLabel.Text = "Haven't you registered?";
            registerLabel.ForeColor = System.Drawing.Color.Blue;
            registerLabel.Cursor = Cursors.Hand;
        }

        private void registerLabel_Click(object sender, EventArgs e)
        {
            RegisterPage registerForm = new RegisterPage();
            registerForm.Show();
            this.Hide();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text.Trim();

            var userCollection = GetUserCollection();

            if (userCollection == null)
            {
                MessageBox.Show("Kullanıcı veritabanına bağlanılamadı.");
                return;
            }

            var filter = Builders<BsonDocument>.Filter.Eq("username", username);
            var userResult = userCollection.Find(filter).FirstOrDefault();

            if (userResult != null)
            {
                string storedPassword = userResult["password"].AsString;

                if (storedPassword == password)
                {
                    // Giriş başarılı, MainPage formunu açıyoruz
                    //MessageBox.Show("Giriş başarılı!"); -> just hide this for now

                    mainPage mainForm = new mainPage();  // mainPage formu
                    mainForm.Show();  // Yeni formu göster
                    this.Hide();  // Mevcut Login formunu gizle (veya Close() ile tamamen kapatabilirsin)
                }
                else
                {
                    MessageBox.Show("Kullanıcı adı veya şifre yanlış.");
                }
            }
            else
            {
                MessageBox.Show("Kullanıcı bulunamadı.");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void LoginPage_Load(object sender, EventArgs e)
        {

        }
    }
}
