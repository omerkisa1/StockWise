using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StockWise
{
    public partial class ProductManagement : UserControl
    {
        public ProductManagement()
        {
            InitializeComponent();
            InitializeGridControl();  
        }

      
        private void InitializeGridControl()
        {
        
            GridView gridView = new GridView(mygridControl);

            mygridControl.MainView = gridView;
            mygridControl.Dock = DockStyle.Fill; 

           
            gridView.Columns.AddVisible("ProductName", "Ürün Adı");
            gridView.Columns.AddVisible("Price", "Fiyat");
            gridView.Columns.AddVisible("Stock", "Stok Durumu");
            gridView.Columns.AddVisible("Category", "Kategori");
        }

        
        private void LoadProductsFromDatabase()
        {
            var productList = GetProductListFromDatabase(); 
            if (productList.Count > 0)
            {
                mygridControl.DataSource = productList;
            }
            else
            {
                MessageBox.Show("Veri çekilemedi veya veri yok.");
            }
        }


        private List<Product> GetProductListFromDatabase()
        {
            try
            {
    
                var client = new MongoClient("mongodb://localhost:27017");
                var database = client.GetDatabase("users"); 
                var collection = database.GetCollection<BsonDocument>("usersStocks"); 

          
                var documents = collection.Find(new BsonDocument()).ToList();

           
                List<Product> productList = new List<Product>();

                foreach (var doc in documents)
                {
                    var product = new Product
                    {
                        ProductName = doc.Contains("productName") ? doc["productName"].AsString : "Bilinmiyor",
                        Price = doc.Contains("price") ? doc["price"].ToDecimal() : 0,
                        Stock = doc.Contains("stock") ? doc["stock"].ToInt32() : 0,
                        Category = doc.Contains("category") ? doc["category"].AsString : "Bilinmiyor"
                    };
                    productList.Add(product);
                }

                return productList; 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürünler yüklenirken bir hata oluştu: " + ex.Message);
                return new List<Product>(); 
            }
        }


        public class Product
        {
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Category { get; set; }
        }

        private void ProductManagement_Load(object sender, EventArgs e)
        {
           
        }

        private void gridControl1_Click(object sender, EventArgs e)
        {
    
        }
        private void gridControl1_Click_1(object sender, EventArgs e)
        {
   
        }


        private void button1_Click(object sender, EventArgs e) 
        {
            LoadProductsFromDatabase(); 
        }
    }
}
