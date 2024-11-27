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
        }

        private async void SalesAnalyticsPage_Load(object sender, EventArgs e)
        {
            // Form yüklendiğinde veri çek ve grafiği güncelle
            await LoadProductData();
        }

        private async Task LoadProductData()
        {
            // Trendyol API URL'si ve parametreleri
            string apiUrl = "https://apigw.trendyol.com/discovery-web-websfxproductgroups-santral/api/v2/product-groups";
            string productGroupIds = "113869021,110655146,603682766"; // Örnek Ürün Grupları
            string fullUrl = $"{apiUrl}?productGroupIds={productGroupIds}&channelId=1";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // API'den veri çek
                    HttpResponseMessage response = await client.GetAsync(fullUrl);
                    response.EnsureSuccessStatusCode();

                    // JSON yanıtını al ve çözümle
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                    var products = jsonResponse.productGroups;

                    // Grafiği güncelle
                    UpdateChart(products);
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya mesaj göster
                MessageBox.Show($"Veri çekerken bir hata oluştu: {ex.Message}");
            }
        }

        private void UpdateChart(dynamic products)
        {
            // Chart kontrolünü temizle
            chart1.Series.Clear();
            chart1.Titles.Clear();

            // Yeni bir seri oluştur
            Series series = new Series("Ürün Fiyatları")
            {
                ChartType = SeriesChartType.Column, // Sütun grafiği
                IsValueShownAsLabel = true // Değerleri göster
            };

            // API'den gelen ürün verilerini ekle
            foreach (var product in products)
            {
                string productName = product.name.ToString();
                decimal productPrice = product.price.originalPrice;

                series.Points.AddXY(productName, productPrice);
            }

            // Chart kontrolüne seriyi ekle
            chart1.Series.Add(series);

            // X ve Y eksenlerini etiketle
            chart1.ChartAreas[0].AxisX.Title = "Ürünler";
            chart1.ChartAreas[0].AxisY.Title = "Fiyatlar (₺)";

            // X eksenindeki etiketlerin düzenlenmesi
            chart1.ChartAreas[0].AxisX.Interval = 1; // Her ürünü göster
            chart1.ChartAreas[0].AxisX.LabelStyle.Angle = -45; // Etiketleri döndür

            // Grafik başlığı ekle
            chart1.Titles.Add("Ürün Fiyat Analizi");

            // Grafiğin renk ayarları
            series.Color = System.Drawing.Color.CornflowerBlue; // Sütun rengini ayarla
        }
    }
}
