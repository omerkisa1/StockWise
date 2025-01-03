using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.WinForms;
using System.Collections.Generic;
using System.Drawing;
using MongoDB.Bson;
using MongoDB.Driver;

namespace StockWise
{
    public partial class SalesAnalyticsPage : UserControl
    {
        private LiveCharts.WinForms.CartesianChart cartesianChart;
        private Label lblSummary; // Sağdaki Summary alanı
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnSaveProducts;
        private Panel headerPanel;
        private Panel chartPanel;
        private Panel summaryPanel;

        private IMongoCollection<BsonDocument> _savedProductsCollection;

        public SalesAnalyticsPage()
        {
            InitializeComponent();
            InitializeDatabaseConnection();
            this.Dock = DockStyle.Fill;
            InitializeControls();
        }

        private void InitializeDatabaseConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("users");
            _savedProductsCollection = database.GetCollection<BsonDocument>("savedProducts");
        }

        private void InitializeControls()
        {
            // Header Panel (Search Section)
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.LightGray,
                Padding = new Padding(10)
            };

            txtSearch = new TextBox
            {
                Width = 300,
                Font = new Font("Arial", 12)
            };

            txtSearch.Text = "Enter product keyword...";
            txtSearch.ForeColor = Color.Gray;

            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == "Enter product keyword...")
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };

            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "Enter product keyword...";
                    txtSearch.ForeColor = Color.Gray;
                }
            };

            btnSearch = new Button
            {
                Text = "Search",
                Width = 100,
                Height = 30,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Margin = new Padding(10, 0, 0, 0),
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White
            };
            btnSearch.Click += async (s, e) => await SearchAndLoadDataAsync();

            btnSaveProducts = new Button
            {
                Text = "Save Products",
                Width = 150,
                Height = 30,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Margin = new Padding(10, 0, 0, 0),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Enabled = false
            };
            btnSaveProducts.Click += async (s, e) => await SaveProductsToDatabaseAsync();

            headerPanel.Controls.Add(txtSearch);
            headerPanel.Controls.Add(btnSearch);
            headerPanel.Controls.Add(btnSaveProducts);

            txtSearch.Location = new Point(10, 20);
            btnSearch.Location = new Point(320, 20);
            btnSaveProducts.Location = new Point(450, 20);

            // Chart Panel
            chartPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            cartesianChart = new LiveCharts.WinForms.CartesianChart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            chartPanel.Controls.Add(cartesianChart);

            // Summary Panel
            summaryPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 250,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle // Daha şık bir görünüm için kenarlık
            };

            lblSummary = new Label
            {
                Text = "Summary: \n- Total Products: \n- Average Price: \n- Total Price: ",
                Font = new Font("Arial", 12),
                ForeColor = Color.Black,
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 150,
                TextAlign = ContentAlignment.TopLeft
            };

            summaryPanel.Controls.Add(lblSummary);

            // Add panels to the main page
            this.Controls.Add(chartPanel);
            this.Controls.Add(summaryPanel);
            this.Controls.Add(headerPanel);
        }

        private async Task SearchAndLoadDataAsync()
        {
            string query = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(query) || query == "Enter product keyword...")
            {
                MessageBox.Show("Please enter a product keyword.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string apiUrl = "https://public.trendyol.com/discovery-web-searchgw-service/v2/api/infinite-scroll/sr";
            string fullUrl = $"{apiUrl}?q={Uri.EscapeDataString(query)}&userGenderId=1";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(fullUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                    var products = jsonResponse.result?.products;

                    if (products == null || products.Count == 0)
                    {
                        MessageBox.Show("No products found for the given keyword.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearChart();
                        return;
                    }

                    UpdateChart(products);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SaveProductsToDatabaseAsync()
        {
            if (cartesianChart.Series.Count == 0)
            {
                MessageBox.Show("No products available to save.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var productNames = cartesianChart.AxisX[0].Labels;
                var productPrices = (LiveCharts.ChartValues<double>)cartesianChart.Series[0].Values;

                var productsToSave = new List<BsonDocument>();

                for (int i = 0; i < productNames.Count; i++)
                {
                    var productDoc = new BsonDocument
                    {
                        { "productName", productNames[i] },
                        { "price", productPrices[i] }
                    };
                    productsToSave.Add(productDoc);
                }

                await _savedProductsCollection.InsertManyAsync(productsToSave);

                MessageBox.Show("Products saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnSaveProducts.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateChart(dynamic products)
        {
            ClearChart();

            var productNames = new List<string>();
            var productPrices = new List<double>();

            double totalPrice = 0;
            foreach (var product in products)
            {
                string name = product.name.ToString();
                double price = (double)product.price.originalPrice;

                productNames.Add(name);
                productPrices.Add(price);

                totalPrice += price;
            }

            cartesianChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Price",
                    Values = new LiveCharts.ChartValues<double>(productPrices)
                }
            };

            cartesianChart.AxisX.Add(new Axis
            {
                Title = "Products",
                Labels = productNames,
                Separator = new Separator { Step = 1, IsEnabled = false }
            });

            cartesianChart.AxisY.Add(new Axis
            {
                Title = "Price (₺)",
                LabelFormatter = value => value.ToString("C")
            });

            lblSummary.Text = $"Summary:\n- Total Products: {productNames.Count}\n- Average Price: {totalPrice / productNames.Count:C}\n- Total Price: {totalPrice:C}";
            btnSaveProducts.Enabled = true;
        }

        private void ClearChart()
        {
            cartesianChart.Series.Clear();
            cartesianChart.AxisX.Clear();
            cartesianChart.AxisY.Clear();
            lblSummary.Text = "Summary: \n- Total Products: \n- Average Price: \n- Total Price: ";
            btnSaveProducts.Enabled = false;
        }
                private void SalesAnalyticsPage_Load(object sender, EventArgs e)
        {

        }

        public void button1_Click_1(object sender, EventArgs e)
        {

        }

        public void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
