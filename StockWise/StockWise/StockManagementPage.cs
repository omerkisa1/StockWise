using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MongoDB.Bson;
using MongoDB.Driver;

namespace StockWise
{
    public partial class StockManagementPage : UserControl
    {
        // Mongo koleksiyonlarımız
        private IMongoCollection<BsonDocument> _storeCollection;

        // Kontroller
        private TableLayoutPanel mainTable;       // Tüm sayfayı düzenleyen TableLayoutPanel
        private TableLayoutPanel topTable;        // Üst kısım (Arama + Eklemeler)
        private DataGridView dataGridViewProducts;

        // 1) Üst satırda: ProdID, ProductName, Price, Stock, Category, Store, Add, Update, Delete, vb.
        private Label lblProdId, lblProductName, lblPrice, lblStock, lblCategory, lblStore;
        private TextBox textBoxProductId, textBoxProductName;
        private NumericUpDown numericPrice, numericStock;
        private ComboBox comboBoxCategory, comboBoxStore;
        private Button buttonAdd, buttonUpdate, buttonDelete, buttonRefresh;

        // 2) İkinci satırda: Search Product Name, Min Price, Max Price, Category, Store, Search
        private Label lblSearchProduct, lblMinPrice, lblMaxPrice, lblSearchCategory, lblSearchStore;
        private TextBox textBoxSearchName;
        private NumericUpDown numericMinPrice, numericMaxPrice;
        private ComboBox comboBoxSearchCategory, comboBoxSearchStore;
        private Button buttonSearch;

        // 3) "Kategori Ekle" butonu
        private Button buttonAddCategory;

        // Listemizdeki kategoriler (DB'den de çekebilirsiniz)
        private List<string> categoryList = new List<string> { "Giyim", "Aksesuar", "Ayakkabı", "Dış Giyim", "Elektronik" };

        public StockManagementPage()
        {
            InitializeComponent();
            InitializeDatabaseConnection();
            InitializeLayout();
            InitializeDataGridView();

            this.Load += StockManagementPage_Load;
        }

        private void StockManagementPage_Load(object sender, EventArgs e)
        {
            LoadStores();      // Mağaza isimlerini DB'den çek
            LoadCategories();  // Kategori listesi (burada sabit, ama DB'den de gelebilir)
            LoadProducts();    // Ürünleri listele
        }

        private void InitializeDatabaseConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("users");
            _storeCollection = database.GetCollection<BsonDocument>("store");
        }

        /// <summary>
        /// Tüm yerleşimi, TableLayoutPanel kullanarak düzenliyoruz.
        /// </summary>
        private void InitializeLayout()
        {
            // Ana TableLayoutPanel: 3 satır
            // 1) Üst satır (form alanları, ekleme/güncelleme/silme)
            // 2) İkinci satır (arama alanları)
            // 3) Üçüncü satır (DataGridView) => doldur
            mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            // Yükseklik payları
            mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Üst satır
            mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // İkinci satır
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // DataGrid fill

            Controls.Add(mainTable);

            // -------------- 1) Üst satır (Ekle/Güncelle/Sil) --------------
            topTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 11, // ProdID, ProductName, Price, Stock, Category (+Add Category btn), Store, (Add,Update,Delete,Refresh)
                AutoSize = true
            };
            // Sütun genişliklerini orantısal ya da AutoSize yapabilirsiniz
            for (int i = 0; i < topTable.ColumnCount; i++)
            {
                topTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }
            mainTable.Controls.Add(topTable, 0, 0); // 0.satır

            // Prod ID
            lblProdId = new Label { Text = "Prod. ID:", AutoSize = true };
            textBoxProductId = new TextBox { Width = 50 };
            topTable.Controls.Add(lblProdId, 0, 0);
            topTable.Controls.Add(textBoxProductId, 0, 1);

            // Product Name
            lblProductName = new Label { Text = "Product Name:", AutoSize = true };
            textBoxProductName = new TextBox { Width = 100 };
            topTable.Controls.Add(lblProductName, 1, 0);
            topTable.Controls.Add(textBoxProductName, 1, 1);

            // Price
            lblPrice = new Label { Text = "Price:", AutoSize = true };
            numericPrice = new NumericUpDown { Width = 60, DecimalPlaces = 2, Minimum = 0, Maximum = 999999 };
            topTable.Controls.Add(lblPrice, 2, 0);
            topTable.Controls.Add(numericPrice, 2, 1);

            // Stock
            lblStock = new Label { Text = "Stock:", AutoSize = true };
            numericStock = new NumericUpDown { Width = 60, Minimum = 0, Maximum = 999999 };
            topTable.Controls.Add(lblStock, 3, 0);
            topTable.Controls.Add(numericStock, 3, 1);

            // Category
            lblCategory = new Label { Text = "Category:", AutoSize = true };
            comboBoxCategory = new ComboBox { Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
            topTable.Controls.Add(lblCategory, 4, 0);
            topTable.Controls.Add(comboBoxCategory, 4, 1);

            // Kategori Ekle butonu
            buttonAddCategory = new Button { Text = "Add Cat", Width = 60 };
            buttonAddCategory.Click += buttonAddCategory_Click; 
            topTable.Controls.Add(buttonAddCategory, 5, 1);

            // Store
            lblStore = new Label { Text = "Store:", AutoSize = true };
            comboBoxStore = new ComboBox { Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
            topTable.Controls.Add(lblStore, 6, 0);
            topTable.Controls.Add(comboBoxStore, 6, 1);

            // Butonlar
            buttonAdd = new Button { Text = "Add", Width = 60 };
            buttonAdd.Click += buttonAdd_Click;
            topTable.Controls.Add(buttonAdd, 7, 1);

            buttonUpdate = new Button { Text = "Update", Width = 60 };
            buttonUpdate.Click += buttonUpdate_Click;
            topTable.Controls.Add(buttonUpdate, 8, 1);

            buttonDelete = new Button { Text = "Delete", Width = 60 };
            buttonDelete.Click += buttonDelete_Click;
            topTable.Controls.Add(buttonDelete, 9, 1);

            buttonRefresh = new Button { Text = "Refresh", Width = 60 };
            buttonRefresh.Click += buttonRefresh_Click;
            topTable.Controls.Add(buttonRefresh, 10, 1);

            // -------------- 2) İkinci satır (Arama) --------------
            var searchTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 9,
                AutoSize = true
            };
            for (int i = 0; i < searchTable.ColumnCount; i++)
            {
                searchTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }
            mainTable.Controls.Add(searchTable, 0, 1); // 1.satır

            lblSearchProduct = new Label { Text = "Search Product Name:", AutoSize = true };
            textBoxSearchName = new TextBox { Width = 120 };
            searchTable.Controls.Add(lblSearchProduct, 0, 0);
            searchTable.Controls.Add(textBoxSearchName, 0, 1);

            lblMinPrice = new Label { Text = "Min Price:", AutoSize = true };
            numericMinPrice = new NumericUpDown { Width = 60, DecimalPlaces = 2, Minimum = 0, Maximum = 999999 };
            searchTable.Controls.Add(lblMinPrice, 1, 0);
            searchTable.Controls.Add(numericMinPrice, 1, 1);

            lblMaxPrice = new Label { Text = "Max Price:", AutoSize = true };
            numericMaxPrice = new NumericUpDown { Width = 60, DecimalPlaces = 2, Minimum = 0, Maximum = 999999 };
            searchTable.Controls.Add(lblMaxPrice, 2, 0);
            searchTable.Controls.Add(numericMaxPrice, 2, 1);

            lblSearchCategory = new Label { Text = "Category:", AutoSize = true };
            comboBoxSearchCategory = new ComboBox { Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
            searchTable.Controls.Add(lblSearchCategory, 3, 0);
            searchTable.Controls.Add(comboBoxSearchCategory, 3, 1);

            lblSearchStore = new Label { Text = "Store:", AutoSize = true };
            comboBoxSearchStore = new ComboBox { Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
            searchTable.Controls.Add(lblSearchStore, 4, 0);
            searchTable.Controls.Add(comboBoxSearchStore, 4, 1);

            buttonSearch = new Button { Text = "Search", Width = 60 };
            buttonSearch.Click += buttonSearch_Click;
            searchTable.Controls.Add(buttonSearch, 5, 1);

            // -------------- 3) Üçüncü satır: DataGridView --------------
            dataGridViewProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dataGridViewProducts.DoubleClick += DataGridViewProducts_DoubleClick;
            mainTable.Controls.Add(dataGridViewProducts, 0, 2);
        }

        private void InitializeDataGridView()
        {
            dataGridViewProducts.Columns.Clear();

            // Daha açıklayıcı sütun başlıkları ve ToolTip'ler
            var colStore = new DataGridViewTextBoxColumn
            {
                Name = "ColumnStoreName",
                HeaderText = "Mağaza",
                ToolTipText = "Ürünün bulunduğu mağaza adı."
            };
            dataGridViewProducts.Columns.Add(colStore);

            var colProdId = new DataGridViewTextBoxColumn
            {
                Name = "ColumnProductId",
                HeaderText = "Ürün ID",
                ToolTipText = "Ürünü tanımlayan ID (metin veya rakam)."
            };
            dataGridViewProducts.Columns.Add(colProdId);

            var colName = new DataGridViewTextBoxColumn
            {
                Name = "ColumnName",
                HeaderText = "Ürün Adı",
                ToolTipText = "Ürünün ismi."
            };
            dataGridViewProducts.Columns.Add(colName);

            var colPrice = new DataGridViewTextBoxColumn
            {
                Name = "ColumnPrice",
                HeaderText = "Fiyat",
                ToolTipText = "Ürünün birim fiyatı."
            };
            dataGridViewProducts.Columns.Add(colPrice);

            var colStock = new DataGridViewTextBoxColumn
            {
                Name = "ColumnStock",
                HeaderText = "Stok",
                ToolTipText = "Ürünün stok adedi."
            };
            dataGridViewProducts.Columns.Add(colStock);

            var colCategory = new DataGridViewTextBoxColumn
            {
                Name = "ColumnCategory",
                HeaderText = "Kategori",
                ToolTipText = "Ürünün hangi kategoriye ait olduğu."
            };
            dataGridViewProducts.Columns.Add(colCategory);

            // Otomatik sığdırma
            dataGridViewProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            dataGridViewProducts.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewProducts.GridColor = Color.DarkGray;
            dataGridViewProducts.RowHeadersVisible = false;
        }

        /// <summary>
        /// Basit bir InputDialog açıp yeni kategoriyi ekliyoruz.
        /// </summary>
        private void buttonAddCategory_Click(object sender, EventArgs e)
        {
            string newCat = Prompt.ShowDialog("Yeni Kategori Ekle", "Kategori Adı:");
            if (!string.IsNullOrEmpty(newCat))
            {
                // Tekrarlanıyorsa ekleme
                if (!categoryList.Contains(newCat))
                {
                    categoryList.Add(newCat);
                    // DB'ye de kaydetmek isterseniz buraya ekleyebilirsiniz
                }
                LoadCategories(); // Tekrar yükle
            }
        }

        /// <summary>
        /// Kategori listesini (categoryList) combobox'lara doldurur.
        /// </summary>
        private void LoadCategories()
        {
            comboBoxCategory.Items.Clear();
            comboBoxCategory.Items.AddRange(categoryList.ToArray());
            if (comboBoxCategory.Items.Count > 0) comboBoxCategory.SelectedIndex = 0;

            comboBoxSearchCategory.Items.Clear();
            comboBoxSearchCategory.Items.Add(""); // Tümü
            comboBoxSearchCategory.Items.AddRange(categoryList.ToArray());
            comboBoxSearchCategory.SelectedIndex = 0;
        }

        private void LoadStores()
        {
            // storeName alanlarını distinct çekebiliriz
            var storeNames = _storeCollection.Distinct<string>("storeName", new BsonDocument()).ToList();

            comboBoxStore.Items.Clear();
            comboBoxSearchStore.Items.Clear();

            // Arama combobox
            comboBoxSearchStore.Items.Add(""); // Tümü
            foreach (var s in storeNames)
            {
                comboBoxSearchStore.Items.Add(s);
            }
            comboBoxSearchStore.SelectedIndex = 0;

            // Alt ekleme/güncelleme
            foreach (var s in storeNames)
            {
                comboBoxStore.Items.Add(s);
            }
            if (comboBoxStore.Items.Count > 0) comboBoxStore.SelectedIndex = 0;
        }

        // -----------------------------------------
        // Aşağıdaki kısımlar, embedding modelini kullanan
        // ($unwind, $match, $push, $pull, vb.) metotlar olabilir.
        // Burada sadece iskeleti gösteriyoruz; "LoadProducts()" vs. implementasyonunuzla aynı kalabilir.
        // -----------------------------------------

        private void LoadProducts()
        {
            // Burada store koleksiyonundaki ürünleri (products dizisi) $unwind ile açıp tablaya doldurun.
            // Basit örnek:
            var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$unwind", "$products"),
                new BsonDocument("$project", new BsonDocument
                {
                    {"storeName", "$storeName"},
                    {"productId", "$products.productId"},
                    {"productName", "$products.productName"},
                    {"price", "$products.price"},
                    {"stock", "$products.stock"},
                    {"category", "$products.category"}
                })
            };
            var results = _storeCollection.Aggregate<BsonDocument>(pipeline).ToList();
            PopulateGridView(results);
        }

        private void PopulateGridView(List<BsonDocument> docs)
        {
            dataGridViewProducts.Rows.Clear();
            foreach (var doc in docs)
            {
                dataGridViewProducts.Rows.Add(
                    doc.GetValue("storeName", "").ToString(),
                    doc.GetValue("productId", "").ToString(),
                    doc.GetValue("productName", "").ToString(),
                    doc.GetValue("price", 0).ToDouble(),
                    doc.GetValue("stock", 0).ToInt32(),
                    doc.GetValue("category", "").ToString()
                );
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            // Arama pipeline
            var matchConditions = new List<BsonDocument>();

            // Arama - Ürün adı
            if (!string.IsNullOrEmpty(textBoxSearchName.Text))
            {
                matchConditions.Add(new BsonDocument("products.productName",
                    new BsonDocument("$regex", textBoxSearchName.Text).Add("$options", "i")));
            }
            // Min/Max Fiyat
            if (numericMinPrice.Value > 0)
            {
                matchConditions.Add(new BsonDocument("products.price", new BsonDocument("$gte", (double)numericMinPrice.Value)));
            }
            if (numericMaxPrice.Value > 0)
            {
                matchConditions.Add(new BsonDocument("products.price", new BsonDocument("$lte", (double)numericMaxPrice.Value)));
            }
            // Kategori
            if (!string.IsNullOrEmpty(comboBoxSearchCategory.Text))
            {
                matchConditions.Add(new BsonDocument("products.category", comboBoxSearchCategory.Text));
            }
            // Store
            if (!string.IsNullOrEmpty(comboBoxSearchStore.Text))
            {
                matchConditions.Add(new BsonDocument("storeName", comboBoxSearchStore.Text));
            }

            var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$unwind", "$products")
            };
            if (matchConditions.Count > 0)
            {
                var andArray = new BsonArray();
                foreach (var c in matchConditions) andArray.Add(c);
                pipeline.Add(new BsonDocument("$match", new BsonDocument("$and", andArray)));
            }
            pipeline.Add(new BsonDocument("$project", new BsonDocument
            {
                {"storeName", "$storeName"},
                {"productId", "$products.productId"},
                {"productName", "$products.productName"},
                {"price", "$products.price"},
                {"stock", "$products.stock"},
                {"category", "$products.category"}
            }));

            var results = _storeCollection.Aggregate<BsonDocument>(pipeline).ToList();
            PopulateGridView(results);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // Seçili mağaza adını bul
            var storeName = comboBoxStore.Text;
            if (string.IsNullOrEmpty(storeName)) return;

            // Eklenecek subdocument
            var productDoc = new BsonDocument
            {
                {"productId", textBoxProductId.Text},
                {"productName", textBoxProductName.Text},
                {"price", (double)numericPrice.Value},
                {"stock", (int)numericStock.Value},
                {"category", comboBoxCategory.Text}
            };

            // Mağazayı bul ve push
            var filter = Builders<BsonDocument>.Filter.Eq("storeName", storeName);
            var update = Builders<BsonDocument>.Update.Push("products", productDoc);
            _storeCollection.UpdateOne(filter, update);

            LoadProducts();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count == 0) return;
            var row = dataGridViewProducts.SelectedRows[0];

            // Hangi mağaza + hangi productID
            var storeName = row.Cells["ColumnStoreName"].Value?.ToString();
            var prodId = row.Cells["ColumnProductId"].Value?.ToString();
            if (string.IsNullOrEmpty(storeName) || string.IsNullOrEmpty(prodId)) return;

            // array filter
            var filter = Builders<BsonDocument>.Filter.Eq("storeName", storeName);
            var arrayFilter = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("elem.productId", prodId))
            };
            var options = new UpdateOptions { ArrayFilters = arrayFilter };

            var update = Builders<BsonDocument>.Update
                .Set("products.$[elem].productName", textBoxProductName.Text)
                .Set("products.$[elem].price", (double)numericPrice.Value)
                .Set("products.$[elem].stock", (int)numericStock.Value)
                .Set("products.$[elem].category", comboBoxCategory.Text);

            _storeCollection.UpdateOne(filter, update, options);

            LoadProducts();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count == 0) return;
            var row = dataGridViewProducts.SelectedRows[0];

            var storeName = row.Cells["ColumnStoreName"].Value?.ToString();
            var prodId = row.Cells["ColumnProductId"].Value?.ToString();
            if (string.IsNullOrEmpty(storeName) || string.IsNullOrEmpty(prodId)) return;

            var filter = Builders<BsonDocument>.Filter.Eq("storeName", storeName);
            var update = Builders<BsonDocument>.Update.Pull("products", new BsonDocument("productId", prodId));
            _storeCollection.UpdateOne(filter, update);

            LoadProducts();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void DataGridViewProducts_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count == 0) return;
            var row = dataGridViewProducts.SelectedRows[0];

            textBoxProductId.Text   = row.Cells["ColumnProductId"].Value?.ToString();
            textBoxProductName.Text = row.Cells["ColumnName"].Value?.ToString();
            numericPrice.Value      = ConvertToDecimal(row.Cells["ColumnPrice"].Value);
            numericStock.Value      = ConvertToDecimal(row.Cells["ColumnStock"].Value);
            comboBoxCategory.Text   = row.Cells["ColumnCategory"].Value?.ToString();
            comboBoxStore.Text      = row.Cells["ColumnStoreName"].Value?.ToString();
        }

        private decimal ConvertToDecimal(object val)
        {
            if (val == null) return 0;
            decimal.TryParse(val.ToString(), out var result);
            return result;
        }
    }

    /// <summary>
    /// Basit bir input dialog açmak için helper sınıf.
    /// Kullanıcı bir metin girip OK der, string olarak döner.
    /// </summary>
    public static class Prompt
    {
        public static string ShowDialog(string title, string prompt)
        {
            Form promptForm = new Form()
            {
                Width = 300,
                Height = 150,
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                MinimizeBox = false,
                MaximizeBox = false
            };
            Label textLabel = new Label() { Left = 10, Top = 15, Text = prompt, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 10, Top = 40, Width = 260 };
            Button confirmation = new Button() { Text = "OK", Left = 200, Width = 70, Top = 70, DialogResult = DialogResult.OK };

            confirmation.Click += (sender, e) => { promptForm.Close(); };
            promptForm.Controls.Add(textLabel);
            promptForm.Controls.Add(textBox);
            promptForm.Controls.Add(confirmation);

            promptForm.AcceptButton = confirmation;
            var result = promptForm.ShowDialog();
            return (result == DialogResult.OK) ? textBox.Text : "";
        }
    }
}
