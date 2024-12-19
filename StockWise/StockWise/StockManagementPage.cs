using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MongoDB.Bson;
using MongoDB.Driver;

namespace StockWise
{
    public partial class StockManagementPage : UserControl
    {
        private IMongoCollection<BsonDocument> _productCollection;

        private TextBox textBoxSearch;
        private TextBox textBoxProductName;
        private NumericUpDown numericUpDownMinPrice;
        private NumericUpDown numericUpDownMaxPrice;
        private NumericUpDown numericUpDownPrice;
        private NumericUpDown numericUpDownStock;
        private ComboBox comboBoxCategory;
        private DataGridView dataGridViewProducts;
        private Button buttonSearch;
        private Button buttonAdd;
        private Button buttonUpdate;
        private Button buttonDelete;

        public StockManagementPage()
        {
            InitializeComponent();
            InitializeControls();
            InitializeDataGridView();
            this.Load += new EventHandler(StockManagementPage_Load);
        }

        private void StockManagementPage_Load(object sender, EventArgs e)
        {
            InitializeDatabaseConnection();
            LoadProducts();
        }

        private void InitializeControls()
        {
            // Initialize Labels
            var labelSearch = new Label { Text = "Search Product Name:", Location = new Point(20, 5), AutoSize = true };
            var labelMinPrice = new Label { Text = "Min Price:", Location = new Point(240, 5), AutoSize = true };
            var labelMaxPrice = new Label { Text = "Max Price:", Location = new Point(360, 5), AutoSize = true };
            var labelCategory = new Label { Text = "Category:", Location = new Point(480, 5), AutoSize = true };
            var labelProductName = new Label { Text = "Product Name:", Location = new Point(20, 45), AutoSize = true };
            var labelPrice = new Label { Text = "Price:", Location = new Point(240, 45), AutoSize = true };
            var labelStock = new Label { Text = "Stock:", Location = new Point(360, 45), AutoSize = true };

            // Initialize TextBoxes
            textBoxSearch = new TextBox { Location = new Point(20, 20), Width = 200 };
            textBoxProductName = new TextBox { Location = new Point(20, 60), Width = 200 };

            // Initialize NumericUpDowns
            numericUpDownMinPrice = new NumericUpDown { Location = new Point(240, 20), Width = 100, Minimum = 0, Maximum = 10000 };
            numericUpDownMaxPrice = new NumericUpDown { Location = new Point(360, 20), Width = 100, Minimum = 0, Maximum = 10000 };
            numericUpDownPrice = new NumericUpDown { Location = new Point(240, 60), Width = 100, Minimum = 0, Maximum = 10000 };
            numericUpDownStock = new NumericUpDown { Location = new Point(360, 60), Width = 100, Minimum = 0, Maximum = 1000 };

            // Initialize ComboBox
            comboBoxCategory = new ComboBox { Location = new Point(480, 20), Width = 150 };
            comboBoxCategory.Items.AddRange(new string[] { "Giyim", "Aksesuar", "Ayakkabı", "Dış Giyim" });

            // Initialize Buttons
            buttonSearch = new Button { Text = "Search", Location = new Point(650, 20), Width = 100 };
            buttonSearch.Click += buttonSearch_Click;

            buttonAdd = new Button { Text = "Add", Location = new Point(650, 60), Width = 100 };
            buttonAdd.Click += buttonAdd_Click;

            buttonUpdate = new Button { Text = "Update", Location = new Point(770, 60), Width = 100 };
            buttonUpdate.Click += buttonUpdate_Click;

            buttonDelete = new Button { Text = "Delete", Location = new Point(890, 60), Width = 100 };
            buttonDelete.Click += buttonDelete_Click;

            // Initialize DataGridView
            dataGridViewProducts = new DataGridView { Location = new Point(20, 120), Width = 950, Height = 400, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };

            // Add controls to the UserControl
            Controls.Add(labelSearch);
            Controls.Add(labelMinPrice);
            Controls.Add(labelMaxPrice);
            Controls.Add(labelCategory);
            Controls.Add(labelProductName);
            Controls.Add(labelPrice);
            Controls.Add(labelStock);

            Controls.Add(textBoxSearch);
            Controls.Add(textBoxProductName);
            Controls.Add(numericUpDownMinPrice);
            Controls.Add(numericUpDownMaxPrice);
            Controls.Add(numericUpDownPrice);
            Controls.Add(numericUpDownStock);
            Controls.Add(comboBoxCategory);
            Controls.Add(buttonSearch);
            Controls.Add(buttonAdd);
            Controls.Add(buttonUpdate);
            Controls.Add(buttonDelete);
            Controls.Add(dataGridViewProducts);

            // Set responsive layout
            this.AutoScroll = true;
        }

        private void InitializeDatabaseConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017"); // MongoDB bağlantı adresi
            var database = client.GetDatabase("users"); // Veritabanı adı
            _productCollection = database.GetCollection<BsonDocument>("usersStocks"); // Koleksiyon adı
        }

        private void InitializeDataGridView()
        {
            dataGridViewProducts.Columns.Clear();
            dataGridViewProducts.Columns.Add("ColumnId", "ID");
            dataGridViewProducts.Columns.Add("ColumnName", "Product Name");
            dataGridViewProducts.Columns.Add("ColumnPrice", "Price");
            dataGridViewProducts.Columns.Add("ColumnStock", "Stock");
            dataGridViewProducts.Columns.Add("ColumnCategory", "Category");

            dataGridViewProducts.Columns["ColumnId"].Visible = false; // ID kolonunu gizle

            // Apply custom styles
            dataGridViewProducts.BackgroundColor = Color.White;
            dataGridViewProducts.DefaultCellStyle.BackColor = Color.White;
            dataGridViewProducts.DefaultCellStyle.ForeColor = Color.Black;
            dataGridViewProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
            dataGridViewProducts.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewProducts.GridColor = Color.Black;
            dataGridViewProducts.RowHeadersVisible = false;
            dataGridViewProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadProducts()
        {
            var products = _productCollection.Find(new BsonDocument()).ToList();
            PopulateGridView(products);
        }

        private void PopulateGridView(List<BsonDocument> products)
        {
            dataGridViewProducts.Rows.Clear();
            foreach (var product in products)
            {
                dataGridViewProducts.Rows.Add(
                    product["_id"].ToString(),
                    product["productName"].ToString(),
                    product["price"].ToDouble(),
                    product["stock"].ToInt32(),
                    product["category"].ToString()
                );
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filters = new List<FilterDefinition<BsonDocument>>();

            if (!string.IsNullOrEmpty(textBoxSearch.Text))
            {
                filters.Add(filterBuilder.Regex("productName", new BsonRegularExpression(textBoxSearch.Text, "i")));
            }

            if (numericUpDownMinPrice.Value > 0 || numericUpDownMaxPrice.Value > 0)
            {
                filters.Add(filterBuilder.Gte("price", (double)numericUpDownMinPrice.Value));
                filters.Add(filterBuilder.Lte("price", (double)numericUpDownMaxPrice.Value));
            }

            if (comboBoxCategory.SelectedItem != null)
            {
                filters.Add(filterBuilder.Eq("category", comboBoxCategory.SelectedItem.ToString()));
            }

            var filter = filters.Count > 0 ? filterBuilder.And(filters) : new BsonDocument();
            var products = _productCollection.Find(filter).ToList();
            PopulateGridView(products);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var newProduct = new BsonDocument
            {
                { "productName", textBoxProductName.Text },
                { "price", (double)numericUpDownPrice.Value },
                { "stock", (int)numericUpDownStock.Value },
                { "category", comboBoxCategory.Text }
            };

            _productCollection.InsertOne(newProduct);
            LoadProducts();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridViewProducts.SelectedRows[0];
                var productId = selectedRow.Cells["ColumnId"].Value.ToString();

                var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(productId));
                _productCollection.DeleteOne(filter);
                LoadProducts();
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridViewProducts.SelectedRows[0];
                var productId = selectedRow.Cells["ColumnId"].Value.ToString();

                var update = Builders<BsonDocument>.Update
                    .Set("productName", textBoxProductName.Text)
                    .Set("price", (double)numericUpDownPrice.Value)
                    .Set("stock", (int)numericUpDownStock.Value)
                    .Set("category", comboBoxCategory.Text);

                var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(productId));
                _productCollection.UpdateOne(filter, update);
                LoadProducts();
            }
        }
    }
}
