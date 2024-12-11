using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;

namespace StockWise
{
    public partial class SalesAnalyticsPage : UserControl
    {
        public SalesAnalyticsPage()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
        }

        private async void SalesAnalyticsPage_Load(object sender, EventArgs e)
        {
            // Başlangıçta varsayılan bir arama kelimesiyle veri yükleyebilirsiniz.
            await LoadProductData("ayakkabı");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // TextBox'a girilen arama kelimesini al
            string searchQuery = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(searchQuery))
            {
                MessageBox.Show("Lütfen bir arama kelimesi girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // API isteğini çalıştır
            await LoadProductData(searchQuery);
        }

        private async Task LoadProductData(string searchQuery)
        {
            string apiUrl = "https://public.trendyol.com/discovery-web-searchgw-service/v2/api/infinite-scroll/sr";
            string fullUrl = $"{apiUrl}?q={Uri.EscapeDataString(searchQuery)}&userGenderId=1";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(fullUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);

                    var products = jsonResponse.result.products;
                    UpdateChart(products);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri çekerken bir hata oluştu: {ex.Message}");
            }
        }

        private void UpdateChart(dynamic products)
        {
            chart1.Series.Clear();
            chart1.Titles.Clear();

            Series series = new Series("Ürün Fiyatları")
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true
            };

            foreach (var product in products)
            {
                string productName = product.name.ToString();
                decimal productPrice = product.price.originalPrice;
                series.Points.AddXY(productName, productPrice);
            }

            chart1.Series.Add(series);

            chart1.ChartAreas[0].AxisX.Title = "Ürünler";
            chart1.ChartAreas[0].AxisY.Title = "Fiyatlar (₺)";
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chart1.Titles.Add("Ürün Fiyat Analizi");
            series.Color = System.Drawing.Color.CornflowerBlue;

            chart1.Dock = DockStyle.Fill;
        }
    }
}
