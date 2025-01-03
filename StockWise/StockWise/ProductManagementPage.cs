using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using MongoDB.Bson;
using MongoDB.Driver;

namespace StockWise
{
    public partial class ProductManagementPage : XtraUserControl
    {
        private IMongoCollection<BsonDocument> _storeCollection;

        private GridControl gridControlStores;
        private GridView gridViewStores;

        private TextEdit textEditStoreName;
        private SimpleButton buttonAddStore;
        private SimpleButton buttonRefreshStores;

        private ComboBoxEdit comboBoxStore1;
        private ComboBoxEdit comboBoxStore2;
        private ComboBoxEdit comboBoxCompareCategory;
        private SimpleButton buttonCompare;
        private GridControl gridControlComparison;
        private GridView gridViewComparison;

        public ProductManagementPage()
        {
            InitializeComponent();
            InitializeDatabaseConnection();
            InitializeControls();
            LoadStores();
        }

        private void InitializeDatabaseConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("users");
            _storeCollection = database.GetCollection<BsonDocument>("store");
        }

        private void InitializeControls()
        {
            // Split Container for Layout
            var splitContainer = new DevExpress.XtraEditors.SplitContainerControl
            {
                Dock = DockStyle.Fill,
                Horizontal = false,
                SplitterPosition = 300
            };
            Controls.Add(splitContainer);

            // Left-Right Layout for Comparison
            var comparisonSplitContainer = new SplitContainerControl
            {
                Dock = DockStyle.Fill,
                Horizontal = true,
                SplitterPosition = 400
            };
            splitContainer.Panel2.Controls.Add(comparisonSplitContainer);

            // Upper Panel
            var upperPanel = new PanelControl { Dock = DockStyle.Fill };
            splitContainer.Panel1.Controls.Add(upperPanel);

            var labelStoreName = new LabelControl { Text = "Store Name:", Location = new Point(20, 20) };
            textEditStoreName = new TextEdit { Location = new Point(100, 15), Width = 200 };

            buttonAddStore = new SimpleButton { Text = "Add Store", Location = new Point(320, 15), Width = 100 };
            buttonAddStore.Click += ButtonAddStore_Click;

            buttonRefreshStores = new SimpleButton { Text = "Refresh", Location = new Point(440, 15), Width = 100 };
            buttonRefreshStores.Click += ButtonRefreshStores_Click;

            gridControlStores = new GridControl { Dock = DockStyle.Bottom, Height = 200 };
            gridViewStores = new GridView(gridControlStores)
            {
                OptionsBehavior = { Editable = false },
                OptionsView = { ShowGroupPanel = false }
            };
            gridControlStores.MainView = gridViewStores;
            gridViewStores.Columns.AddVisible("StoreId", "Store ID");
            gridViewStores.Columns.AddVisible("StoreName", "Store Name");

            upperPanel.Controls.Add(labelStoreName);
            upperPanel.Controls.Add(textEditStoreName);
            upperPanel.Controls.Add(buttonAddStore);
            upperPanel.Controls.Add(buttonRefreshStores);
            upperPanel.Controls.Add(gridControlStores);

            // Left Panel for Selection
            var selectionPanel = new PanelControl { Dock = DockStyle.Fill };
            comparisonSplitContainer.Panel1.Controls.Add(selectionPanel);

            var labelStore1 = new LabelControl { Text = "Select Store 1:", Location = new Point(20, 20) };
            comboBoxStore1 = new ComboBoxEdit { Location = new Point(120, 15), Width = 200 };

            var labelStore2 = new LabelControl { Text = "Select Store 2:", Location = new Point(20, 60) };
            comboBoxStore2 = new ComboBoxEdit { Location = new Point(120, 55), Width = 200 };

            var labelCategory = new LabelControl { Text = "Category:", Location = new Point(20, 100) };
            comboBoxCompareCategory = new ComboBoxEdit { Location = new Point(120, 95), Width = 200 };

            buttonCompare = new SimpleButton { Text = "Compare", Location = new Point(120, 140), Width = 100 };
            buttonCompare.Click += ButtonCompare_Click;

            selectionPanel.Controls.Add(labelStore1);
            selectionPanel.Controls.Add(comboBoxStore1);
            selectionPanel.Controls.Add(labelStore2);
            selectionPanel.Controls.Add(comboBoxStore2);
            selectionPanel.Controls.Add(labelCategory);
            selectionPanel.Controls.Add(comboBoxCompareCategory);
            selectionPanel.Controls.Add(buttonCompare);

            // Right Panel for Results
            var resultsPanel = new PanelControl { Dock = DockStyle.Fill };
            comparisonSplitContainer.Panel2.Controls.Add(resultsPanel);

            gridControlComparison = new GridControl { Dock = DockStyle.Fill };
            gridViewComparison = new GridView(gridControlComparison)
            {
                OptionsBehavior = { Editable = false },
                OptionsView = { ShowGroupPanel = false }
            };
            gridControlComparison.MainView = gridViewComparison;
            gridViewComparison.Columns.AddVisible("ProductName", "Product Name");
            gridViewComparison.Columns.AddVisible("Store1Price", "Store 1 Price");
            gridViewComparison.Columns.AddVisible("Store1Stock", "Store 1 Stock");
            gridViewComparison.Columns.AddVisible("Store2Price", "Store 2 Price");
            gridViewComparison.Columns.AddVisible("Store2Stock", "Store 2 Stock");

            resultsPanel.Controls.Add(gridControlComparison);
        }

        private void LoadStores()
        {
            var stores = _storeCollection.Find(new BsonDocument()).ToList();
            var storeList = new List<Store>();

            comboBoxStore1.Properties.Items.Clear();
            comboBoxStore2.Properties.Items.Clear();

            foreach (var store in stores)
            {
                var storeObj = new Store
                {
                    StoreId = store.GetValue("storeId", new BsonString("Unknown")).AsString,
                    StoreName = store.GetValue("storeName", new BsonString("Unknown")).AsString
                };
                storeList.Add(storeObj);
                comboBoxStore1.Properties.Items.Add(storeObj.StoreName);
                comboBoxStore2.Properties.Items.Add(storeObj.StoreName);
            }

            gridControlStores.DataSource = storeList;

            // Load categories dynamically
            LoadCategories();
        }

        private void LoadCategories()
        {
            var stores = _storeCollection.Find(new BsonDocument()).ToList();
            var categories = new HashSet<string>();

            foreach (var store in stores)
            {
                var products = store.GetValue("products", new BsonArray()).AsBsonArray;
                foreach (var product in products)
                {
                    if (product.AsBsonDocument.Contains("category"))
                    {
                        categories.Add(product["category"].AsString);
                    }
                }
            }

            comboBoxCompareCategory.Properties.Items.Clear();
            comboBoxCompareCategory.Properties.Items.AddRange(categories.ToArray());
        }

        private void ButtonAddStore_Click(object sender, EventArgs e)
        {
            var storeName = textEditStoreName.Text;

            if (string.IsNullOrWhiteSpace(storeName))
            {
                XtraMessageBox.Show("Store name cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var newStore = new BsonDocument
            {
                { "storeId", Guid.NewGuid().ToString() },
                { "storeName", storeName },
                { "products", new BsonArray() }
            };

            _storeCollection.InsertOne(newStore);
            textEditStoreName.Text = string.Empty;
            LoadStores();
            XtraMessageBox.Show("Store added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ButtonRefreshStores_Click(object sender, EventArgs e)
        {
            LoadStores();
        }

        private void ButtonCompare_Click(object sender, EventArgs e)
        {
            var store1Name = comboBoxStore1.EditValue?.ToString();
            var store2Name = comboBoxStore2.EditValue?.ToString();
            var category = comboBoxCompareCategory.EditValue?.ToString();

            if (string.IsNullOrWhiteSpace(store1Name) || string.IsNullOrWhiteSpace(store2Name) || string.IsNullOrWhiteSpace(category))
            {
                XtraMessageBox.Show("Please select both stores and a category.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var store1 = _storeCollection.Find(Builders<BsonDocument>.Filter.Eq("storeName", store1Name)).FirstOrDefault();
            var store2 = _storeCollection.Find(Builders<BsonDocument>.Filter.Eq("storeName", store2Name)).FirstOrDefault();

            if (store1 == null || store2 == null)
            {
                XtraMessageBox.Show("One or both stores could not be found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var store1Products = store1["products"].AsBsonArray.Where(p => p.AsBsonDocument.Contains("category") && p["category"].AsString == category).ToList();

            var store2Products = store2["products"].AsBsonArray.Where(p => p.AsBsonDocument.Contains("category") && p["category"].AsString == category).ToList();


            var comparisonData = new List<dynamic>();

            foreach (var product1 in store1Products)
            {
                var productName = product1["productName"].AsString;
                var store1Price = product1["price"].ToDouble();
                var store1Stock = product1["stock"].ToInt32();

                var matchingProduct = store2Products.FirstOrDefault(p => p["productName"].AsString == productName);

                if (matchingProduct != null)
                {
                    var store2Price = matchingProduct["price"].ToDouble();
                    var store2Stock = matchingProduct["stock"].ToInt32();

                    comparisonData.Add(new
                    {
                        ProductName = productName,
                        Store1Price = store1Price,
                        Store1Stock = store1Stock,
                        Store2Price = store2Price,
                        Store2Stock = store2Stock
                    });
                }
            }

            gridControlComparison.DataSource = comparisonData;
        }

        public class Store
        {
            public string StoreId { get; set; }
            public string StoreName { get; set; }
        }
        private void ProductManagement_Load(object sender, EventArgs e)
        {

        }
    }
}
