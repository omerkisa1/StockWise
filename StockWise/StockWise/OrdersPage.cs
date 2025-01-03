using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Data;
using System.Windows.Forms;
using System.Linq;


namespace StockWise
{
    public partial class OrdersPage : UserControl
    {
        // MongoDB koleksiyon bağlantısı
        private IMongoCollection<BsonDocument> _storeCollection;

        // GridControl ve GridView tanımları
        private GridControl gridControlOrders;
        private GridView gridViewOrders;

        public OrdersPage()
        {
            InitializeComponent();
            InitializeDatabaseConnection();
            InitializeGridControl();
            this.Load += OrdersPage_Load; // Load event bağlanıyor
        }

        private void OrdersPage_Load(object sender, EventArgs e)
        {
            LoadData(); // Sayfa yüklendiğinde veriler yüklenecek
        }

        private void InitializeDatabaseConnection()
        {
            // MongoDB bağlantısı
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("users");
            _storeCollection = database.GetCollection<BsonDocument>("store");
        }

        private void InitializeGridControl()
        {
            // GridControl ve GridView oluştur
            gridControlOrders = new GridControl();
            gridViewOrders = new GridView(gridControlOrders);

            // GridControl ayarları
            gridControlOrders.Dock = DockStyle.Fill;
            gridControlOrders.MainView = gridViewOrders;
            this.Controls.Add(gridControlOrders);

            // GridView ayarları
            gridViewOrders.OptionsView.ShowGroupPanel = false;
            gridViewOrders.OptionsBehavior.Editable = false;

            // GridView sütunları ekle
            gridViewOrders.Columns.AddVisible("StoreName", "Store Name");
            gridViewOrders.Columns.AddVisible("ProductName", "Product Name");
            gridViewOrders.Columns.AddVisible("Price", "Price");
            gridViewOrders.Columns.AddVisible("Stock", "Stock");
            gridViewOrders.Columns.AddVisible("Category", "Category");
            gridViewOrders.Columns.AddVisible("Status", "Status");
        }

        private void LoadData()
        {
            try
            {
                // MongoDB'den verileri çek
                var stores = _storeCollection.Find(new BsonDocument()).ToList();

                // GridControl'e bağlanacak veri kaynağı
                var ordersData = stores.SelectMany(store =>
                {
                    var storeName = store.GetValue("storeName", "Unknown").AsString;
                    var products = store.GetValue("products", new BsonArray()).AsBsonArray;

                    return products.Select(product =>
                    {
                        var productDoc = product.AsBsonDocument;

                        return new
                        {
                            StoreName = storeName,
                            ProductName = productDoc.GetValue("productName", "").AsString,
                            Price = productDoc.GetValue("price", 0).ToDouble(),
                            Stock = productDoc.GetValue("stock", 0).ToInt32(),
                            Category = productDoc.GetValue("category", "").AsString,
                            Status = productDoc.GetValue("status", "").AsString
                        };
                    });
                }).ToList();

                // GridControl'e verileri bağla
                gridControlOrders.DataSource = ordersData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
