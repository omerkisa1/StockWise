using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraBars.Navigation;  // DevExpress AccordionControl için gerekli

namespace StockWise
{
    public partial class mainPage : Form
    {
        public mainPage()
        {
            InitializeComponent();
            SetupAccordionControl();  // AccordionControl'u oluşturuyoruz
        }

        // Hamburger menü için AccordionControl ayarları
        private void SetupAccordionControl()
        {
            AccordionControl accordionControl = new AccordionControl();
            accordionControl.Dock = DockStyle.Left;  // Sol tarafa yerleştiriyoruz
            accordionControl.Width = 200;  // Menü genişliği

            // Ürün Yönetimi
            AccordionControlElement productManagement = new AccordionControlElement
            {
                Text = "Ürün Yönetimi",
                Style = ElementStyle.Item
            };
            productManagement.Click += ProductManagement_Click;

            // Kategoriler
            AccordionControlElement categories = new AccordionControlElement
            {
                Text = "Kategoriler",
                Style = ElementStyle.Item
            };
            categories.Click += Categories_Click;

            // Stok Yönetimi
            AccordionControlElement stockManagement = new AccordionControlElement
            {
                Text = "Stok Yönetimi",
                Style = ElementStyle.Item
            };
            stockManagement.Click += StockManagement_Click;

            // Siparişler
            AccordionControlElement orders = new AccordionControlElement
            {
                Text = "Siparişler",
                Style = ElementStyle.Item
            };
            orders.Click += Orders_Click;

            // Satış Analizleri
            AccordionControlElement salesAnalytics = new AccordionControlElement
            {
                Text = "Satış Analizleri",
                Style = ElementStyle.Item
            };
            salesAnalytics.Click += SalesAnalytics_Click;

            // Geri Bildirimler
            AccordionControlElement feedbacks = new AccordionControlElement
            {
                Text = "Geri Bildirimler",
                Style = ElementStyle.Item
            };
            feedbacks.Click += Feedbacks_Click;

            // Accordion Control'e elementleri ekleyin
            accordionControl.Elements.AddRange(new AccordionControlElement[]
            {
                productManagement, categories, stockManagement, orders, salesAnalytics, feedbacks
            });

            // Form'a accordionControl ekleyin
            this.Controls.Add(accordionControl);
        }

        // PanelContainer ile sayfa içeriğini değiştirme
        private void LoadContentToPanel(UserControl content)
        {
            // Mevcut içeriği temizle
            panelContainer.Controls.Clear();
            // Yeni içeriği ekle
            content.Dock = DockStyle.Fill;
            panelContainer.Controls.Add(content);
        }

        // Ürün Yönetimi seçeneğine tıklandığında yapılacaklar
        private void ProductManagement_Click(object sender, EventArgs e)
        {
            // Ürün Yönetimi sayfasını yükleyin
            LoadContentToPanel(new ProductManagementPage());  // ProductManagementPage, bir UserControl olmalı
        }

        // Kategoriler seçeneğine tıklandığında yapılacaklar
        private void Categories_Click(object sender, EventArgs e)
        {
            // Kategoriler sayfasını yükleyin
            LoadContentToPanel(new CategoriesPage());  // CategoriesPage, bir UserControl olmalı
        }

        // Stok Yönetimi seçeneğine tıklandığında yapılacaklar
        private void StockManagement_Click(object sender, EventArgs e)
        {
            // Stok Yönetimi sayfasını yükleyin
            LoadContentToPanel(new StockManagementPage());  // StockManagementPage, bir UserControl olmalı
        }

        private void Orders_Click(object sender, EventArgs e)
        {
            // Siparişler sayfasını yükleyin
            LoadContentToPanel(new OrdersPage());  // OrdersPage, bir UserControl olmalı
        }

        private void SalesAnalytics_Click(object sender, EventArgs e)
        {
            // Satış Analizleri sayfasını yükleyin
            LoadContentToPanel(new SalesAnalyticsPage());  // SalesAnalyticsPage, bir UserControl olmalı
        }

        private void Feedbacks_Click(object sender, EventArgs e)
        {
            // Geri Bildirimler sayfasını yükleyin
            LoadContentToPanel(new FeedbacksPage());  // FeedbacksPage, bir UserControl olmalı
        }
    }
}
