using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace StockWise
{
    public partial class PurchasePage : UserControl
    {
        private IMongoCollection<BsonDocument> _savedProductsCollection;
        private IMongoCollection<BsonDocument> _storeCollection;
        private DataGridView dataGridViewProducts;
        private Button btnPurchase;

        public PurchasePage()
        {
            InitializeComponent();
            InitializeDatabaseConnection();
            InitializeControls();
            LoadProducts();
        }

        private void InitializeDatabaseConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("users");
            _savedProductsCollection = database.GetCollection<BsonDocument>("savedProducts");
            _storeCollection = database.GetCollection<BsonDocument>("store");
        }

        private void InitializeControls()
        {
            // DataGridView
            dataGridViewProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };

            dataGridViewProducts.Columns.Add("ProductName", "Product Name");
            dataGridViewProducts.Columns.Add("Price", "Price (₺)");
            var quantityColumn = new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                HeaderText = "Quantity",
                ValueType = typeof(int)
            };
            dataGridViewProducts.Columns.Add(quantityColumn);

            // Purchase Button
            btnPurchase = new Button
            {
                Text = "Purchase",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = System.Drawing.Color.DodgerBlue,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold)
            };

            btnPurchase.Click += BtnPurchase_Click;

            // Add controls to the form
            this.Controls.Add(dataGridViewProducts);
            this.Controls.Add(btnPurchase);
        }

        private void LoadProducts()
        {
            var products = _savedProductsCollection.Find(new BsonDocument()).ToList();

            foreach (var product in products)
            {
                dataGridViewProducts.Rows.Add(
                    product.GetValue("productName").AsString,
                    product.GetValue("price").ToDouble(),
                    1 // Default quantity
                );
            }
        }

        private void BtnPurchase_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to purchase.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRow = dataGridViewProducts.SelectedRows[0];
            string productName = selectedRow.Cells["ProductName"].Value.ToString();
            double price = Convert.ToDouble(selectedRow.Cells["Price"].Value);
            int stock = Convert.ToInt32(selectedRow.Cells["Quantity"].Value); // Quantity olarak geçiyor, fakat stock anlamında kullanılıyor.

            // Kullanıcıdan mağaza seçmesini iste
            var storeNames = _storeCollection.Find(new BsonDocument()).ToList().Select(s => s["storeName"].AsString).ToList();

            using (var storeSelectionForm = new StoreSelectionForm(storeNames))
            {
                if (storeSelectionForm.ShowDialog() == DialogResult.OK)
                {
                    string selectedStore = storeSelectionForm.SelectedStore;

                    // Ürünü mağazaya ekle
                    var storeFilter = Builders<BsonDocument>.Filter.Eq("storeName", selectedStore);
                    var update = Builders<BsonDocument>.Update.Push("products", new BsonDocument
                    {
                        { "productId", Guid.NewGuid().ToString() }, // Her ürün için benzersiz bir ID oluştur
                        { "productName", productName },
                        { "price", price },
                        { "stock", stock }, // Yeni stok bilgisi
                        { "category", "Purchased" }, // Kategori purchased olarak ayarlanıyor
                        { "status", "Processing" }, // Yeni durum statüsü
                        { "purchaseDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                    });

                    _storeCollection.UpdateOne(storeFilter, update);

                    MessageBox.Show($"Product '{productName}' has been purchased and added to store '{selectedStore}'.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }

    public class StoreSelectionForm : Form
    {
        private ComboBox comboBoxStores;
        private Button btnConfirm;
        public string SelectedStore { get; private set; }

        public StoreSelectionForm(List<string> storeNames)
        {
            Text = "Select Store";
            Width = 400;
            Height = 200;

            comboBoxStores = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboBoxStores.Items.AddRange(storeNames.ToArray());
            if (comboBoxStores.Items.Count > 0)
                comboBoxStores.SelectedIndex = 0;

            btnConfirm = new Button
            {
                Text = "Confirm",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = System.Drawing.Color.DodgerBlue,
                ForeColor = System.Drawing.Color.White
            };

            btnConfirm.Click += BtnConfirm_Click;

            Controls.Add(comboBoxStores);
            Controls.Add(btnConfirm);
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            SelectedStore = comboBoxStores.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(SelectedStore))
            {
                MessageBox.Show("Please select a store.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
