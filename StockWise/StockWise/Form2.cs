using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Bson;
using MongoDB.Driver;

namespace StockWise
{      // main note here : make sure the async working of the db connections
    public partial class RegisterPage : Form
    {
        public RegisterPage()
        {
            InitializeComponent();
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

        private void registerButton_Click_1(object sender, EventArgs e)
        {
            string email = mailBox.Text.Trim();
            string username = usernameBox.Text.Trim();
            string password = passwordBox.Text.Trim();
            string name = nameBox.Text.Trim();
            string surname = surnameBox.Text.Trim();
            string company = companyBox.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(surname))
            {
                MessageBox.Show("Lütfen tüm gerekli alanları doldurun.");
                return;
            }

            var userCollection = GetUserCollection();

            if (userCollection == null)
            {
                MessageBox.Show("Kullanıcı veritabanına bağlanılamadı.");
                return;
            }

            var newUser = new BsonDocument
            {
                { "email", email },
                { "username", username },
                { "password", password },
                { "name", name },
                { "surname", surname },
                { "company", company }
            };

            try
            {

                userCollection.InsertOne(newUser);
                MessageBox.Show("Kayıt başarılı!");

                // do not forget to add here more extension to page redirection
                LoginPage loginForm = new LoginPage();
                loginForm.Show(); 
                this.Hide();  
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt sırasında bir hata oluştu: " + ex.Message);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void nameLabel_Click(object sender, EventArgs e)
        {
        }
    }
}
