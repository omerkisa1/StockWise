using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors;
using MongoDB.Bson;
using MongoDB.Driver;

namespace StockWise
{
    public partial class ProductManagementPage : UserControl
    {
        private GridControl gridControlStores;
        private GridView gridViewStores;
        private GridControl gridControlProducts;
        private GridView gridViewProducts;
        private SimpleButton buttonRefresh;
        private SimpleButton buttonAddStore;
        private ComboBoxEdit filterComboBox;

        private IMongoCollection<BsonDocument> _productCollection;

        public ProductManagementPage()
        {
            InitializeComponent();
            InitializeControls();
            InitializeDatabaseConnection();
            LoadStores();
        }

        private void InitializeDatabaseConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("users");
            _productCollection = database.GetCollection<BsonDocument>("store");
        }

        private void InitializeControls()
        {
            // Stores GridControl Initialization
            gridControlStores = new GridControl
            {
                Dock = DockStyle.Top,
                Height = 200
            };

            gridViewStores = new GridView(gridControlStores)
            {
                OptionsBehavior = { Editable = false },
                OptionsView = { ShowGroupPanel = false }
            };

            gridControlStores.MainView = gridViewStores;
            gridViewStores.Columns.AddVisible("storeId", "Store ID");
            gridViewStores.Columns.AddVisible("storeName", "Store Name");

            gridViewStores.FocusedRowChanged += GridViewStores_FocusedRowChanged;

            Controls.Add(gridControlStores);

            // Buttons Panel
            var buttonsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80
            };

            // Refresh Button
            buttonRefresh = new SimpleButton
            {
                Text = "Refresh",
                Location = new System.Drawing.Point(20, 10),
                Width = 100
            };
            buttonRefresh.Click += ButtonRefresh_Click;
            buttonsPanel.Controls.Add(buttonRefresh);

            // Add Store Button
            buttonAddStore = new SimpleButton
            {
                Text = "Add Store",
                Location = new System.Drawing.Point(140, 10),
                Width = 100
            };
            buttonAddStore.Click += ButtonAddStore_Click;
            buttonsPanel.Controls.Add(buttonAddStore);

            // Filter ComboBox
            filterComboBox = new ComboBoxEdit
            {
                Location = new System.Drawing.Point(260, 10),
                Width = 150,
                Properties = { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor }
            };
            filterComboBox.Properties.Items.AddRange(new string[] { "All", "Sold", "Ordered", "Unknown" });
            filterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;
            buttonsPanel.Controls.Add(filterComboBox);

            Controls.Add(buttonsPanel);

            // Products GridControl Initialization
            gridControlProducts = new GridControl
            {
                Dock = DockStyle.Fill
            };

            gridViewProducts = new GridView(gridControlProducts)
            {
                OptionsBehavior = { Editable = false },
                OptionsView = { ShowGroupPanel = false }
            };

            gridControlProducts.MainView = gridViewProducts;
            gridViewProducts.Columns.AddVisible("productId", "Product ID");
            gridViewProducts.Columns.AddVisible("productName", "Product Name");
            gridViewProducts.Columns.AddVisible("price", "Price");
            gridViewProducts.Columns.AddVisible("stock", "Stock");
            gridViewProducts.Columns.AddVisible("category", "Category");
            gridViewProducts.Columns.AddVisible("status", "Status"); // New Status Column

            Controls.Add(gridControlProducts);

            // Set Form Properties
            this.Dock = DockStyle.Fill;
            this.AutoScroll = true;
        }

        private void LoadStores()
        {
            var stores = GetStoresFromDatabase();
            gridControlStores.DataSource = stores;
        }

        private void LoadProducts(string storeId)
        {
            var products = GetProductsFromDatabase(storeId);
            gridControlProducts.DataSource = products;
        }

        private void LoadAllProducts()
        {
            var allProducts = GetAllProductsFromDatabase();
            gridControlProducts.DataSource = allProducts;
        }

        private List<Store> GetStoresFromDatabase()
        {
            try
            {
                var documents = _productCollection.Find(new BsonDocument()).ToList();

                List<Store> storeList = new List<Store>();
                foreach (var doc in documents)
                {
                    var store = new Store
                    {
                        StoreId = doc.GetValue("storeId", new BsonString("Unknown")).AsString,
                        StoreName = doc.GetValue("storeName", new BsonString("Unknown")).AsString
                    };
                    storeList.Add(store);
                }

                return storeList;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Error loading stores: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<Store>();
            }
        }

        private List<Product> GetProductsFromDatabase(string storeId)
        {
            try
            {
                var storeDoc = _productCollection.Find(Builders<BsonDocument>.Filter.Eq("storeId", storeId)).FirstOrDefault();

                if (storeDoc != null && storeDoc.Contains("products") && storeDoc["products"].IsBsonArray)
                {
                    var productsArray = storeDoc["products"].AsBsonArray;
                    List<Product> productList = new List<Product>();

                    foreach (var productValue in productsArray)
                    {
                        if (productValue.IsBsonDocument)
                        {
                            var productDoc = productValue.AsBsonDocument;
                            var product = new Product
                            {
                                ProductId = productDoc.GetValue("productId", new BsonString("Unknown")).AsString,
                                ProductName = productDoc.GetValue("productName", new BsonString("Unknown")).AsString,
                                Price = productDoc.GetValue("price", new BsonDouble(0)).ToDecimal(),
                                Stock = productDoc.GetValue("stock", new BsonInt32(0)).ToInt32(),
                                Category = productDoc.GetValue("category", new BsonString("Unknown")).AsString,
                                Status = productDoc.GetValue("status", new BsonString("Unknown")).AsString
                            };
                            productList.Add(product);
                        }
                    }

                    return productList;
                }

                return new List<Product>();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Error loading products: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<Product>();
            }
        }

        private List<Product> GetAllProductsFromDatabase()
        {
            try
            {
                var documents = _productCollection.Find(new BsonDocument()).ToList();
                List<Product> allProducts = new List<Product>();

                foreach (var doc in documents)
                {
                    if (doc.Contains("products") && doc["products"].IsBsonArray)
                    {
                        var productsArray = doc["products"].AsBsonArray;
                        foreach (var productValue in productsArray)
                        {
                            if (productValue.IsBsonDocument)
                            {
                                var productDoc = productValue.AsBsonDocument;
                                var product = new Product
                                {
                                    ProductId = productDoc.GetValue("productId", new BsonString("Unknown")).AsString,
                                    ProductName = productDoc.GetValue("productName", new BsonString("Unknown")).AsString,
                                    Price = productDoc.GetValue("price", new BsonDouble(0)).ToDecimal(),
                                    Stock = productDoc.GetValue("stock", new BsonInt32(0)).ToInt32(),
                                    Category = productDoc.GetValue("category", new BsonString("Unknown")).AsString,
                                    Status = productDoc.GetValue("status", new BsonString("Unknown")).AsString,
                                    StoreName = doc.GetValue("storeName", new BsonString("Unknown")).AsString
                                };
                                allProducts.Add(product);
                            }
                        }
                    }
                }

                return allProducts;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Error loading all products: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<Product>();
            }
        }

        private void FilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedFilter = filterComboBox.SelectedItem.ToString();

            if (selectedFilter == "All")
            {
                LoadAllProducts();
            }
            else
            {
                var filteredProducts = GetAllProductsFromDatabase().Where(p => p.Status == selectedFilter).ToList();
                gridControlProducts.DataSource = filteredProducts;
            }
        }

        private void GridViewStores_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (gridViewStores.GetFocusedRow() is Store selectedStore)
            {
                LoadProducts(selectedStore.StoreId);
            }
        }

        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            LoadAllProducts();
        }

        private void ButtonAddStore_Click(object sender, EventArgs e)
        {
            string storeName = XtraInputBox.Show("Enter Store Name:", "Add Store", "");

            if (string.IsNullOrEmpty(storeName))
            {
                XtraMessageBox.Show("Store name cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var newStore = new BsonDocument
                {
                    { "storeId", Guid.NewGuid().ToString() },
                    { "storeName", storeName },
                    { "products", new BsonArray() } // Initialize with empty products
                };

                _productCollection.InsertOne(newStore);
                LoadStores();
                XtraMessageBox.Show("Store added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Error adding store: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class Store
        {
            public string StoreId { get; set; }
            public string StoreName { get; set; }
        }

        public class Product
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Category { get; set; }
            public string Status { get; set; } // New Status Field
            public string StoreName { get; set; } // Store name for All Products View
        }
        private void ProductManagement_Load(object sender, EventArgs e)
        {

        }
    }
}
