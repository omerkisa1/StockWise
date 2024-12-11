using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using LiveCharts;
using LiveCharts.WinForms;
using LiveCharts.Wpf;

namespace StockWise
{
    public partial class SalesAnalyticsPage : UserControl
    {
        private LiveCharts.WinForms.CartesianChart cartesianChart;

        public SalesAnalyticsPage()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            // LiveCharts için CartesianChart oluşturma
            cartesianChart = new LiveCharts.WinForms.CartesianChart
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(cartesianChart);

            // TextBox placeholder için başlangıç ayarı
            textBox1.Text = "Arama kelimesi girin...";
            textBox1.ForeColor = System.Drawing.Color.Gray;

            // TextBox olaylarını bağlama
            textBox1.Enter += TextBox1_Enter;
            textBox1.Leave += TextBox1_Leave;
        }

        private async void SalesAnalyticsPage_Load(object sender, EventArgs e)
        {
            // Uygulama ilk açıldığında varsayılan bir arama kelimesiyle veri yükleniyor.
            await LoadProductData("ayakkabı");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string searchQuery = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(searchQuery) || searchQuery == "Arama kelimesi girin...")
            {
                MessageBox.Show("Lütfen bir arama kelimesi girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

                    // JSON verisini deserialize et
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                    var products = jsonResponse.result?.products;

                    // Ürün kontrolü
                    if (products == null || products.Count == 0)
                    {
                        MessageBox.Show("Aradığınız kelimeye ait ürün bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        cartesianChart.Series.Clear();
                        return;
                    }

                    UpdateChart(products);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri çekerken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateChart(dynamic products)
        {
            cartesianChart.Series.Clear();
            cartesianChart.AxisX.Clear();
            cartesianChart.AxisY.Clear();

            var seriesCollection = new SeriesCollection();
            var productNames = new string[products.Count];
            var productPrices = new double[products.Count];

            int index = 0;
            foreach (var product in products)
            {
                string productName = product.name.ToString();
                decimal productPrice = product.price.originalPrice;

                productNames[index] = productName;
                productPrices[index] = (double)productPrice;
                index++;
            }

            seriesCollection.Add(new ColumnSeries
            {
                Title = "Fiyat",
                Values = new ChartValues<double>(productPrices)
            });

            // Grafiği güncelle
            cartesianChart.Series = seriesCollection;
            cartesianChart.AxisX.Add(new Axis
            {
                Title = "Ürünler",
                Labels = productNames,
                Separator = new Separator { Step = 1 }
            });

            cartesianChart.AxisY.Add(new Axis
            {
                Title = "Fiyat (₺)"
            });
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        // Placeholder için manuel olaylar
        private void TextBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Arama kelimesi girin...")
            {
                textBox1.Text = "";
                textBox1.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void TextBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Arama kelimesi girin...";
                textBox1.ForeColor = System.Drawing.Color.Gray;
            }
        }
    }
}
