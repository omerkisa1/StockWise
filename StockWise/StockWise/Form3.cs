using System;
using System.Windows.Forms;
using DevExpress.XtraBars.Navigation;
using System.Drawing;

namespace StockWise
{
    public partial class mainPage : Form
    {
        AccordionControl accordionControl;
        AccordionControlElement selectedElement;

        public mainPage()
        {
            InitializeComponent();
            SetupAccordionControl(); // AccordionControl yapılandırması
            this.WindowState = FormWindowState.Maximized;
        }

        private void SetupAccordionControl()
        {
            // AccordionControl oluşturuluyor
            accordionControl = new AccordionControl
            {
                Dock = DockStyle.Left, // Sol tarafa yerleştiriliyor
                Width = 250 // Genişlik ayarı
            };

            // AccordionControl görsel ayarları
            accordionControl.Appearance.Item.Normal.BackColor = Color.LightGray;
            accordionControl.Appearance.Item.Normal.ForeColor = Color.Black;
            accordionControl.Appearance.Item.Hovered.BackColor = Color.LightBlue;
            accordionControl.Appearance.Item.Pressed.BackColor = Color.LightSeaGreen;

            // Menü öğeleri oluşturuluyor
            AddAccordionElement("Ürün Yönetimi", Properties.Resources.productDevelopment, new ProductManagementPage());
            AddAccordionElement("Stok Yönetimi", Properties.Resources.inventory_management, new StockManagementPage());
            AddAccordionElement("Siparişler", Properties.Resources.package_tracking, new OrdersPage());
            AddAccordionElement("Satış Analizleri", Properties.Resources.sales, new SalesAnalyticsPage());
            AddAccordionElement("Satın Alım", Properties.Resources.shopping_cart_6012938, new PurchasePage());
            AddAccordionElement("Geri Bildirimler", Properties.Resources.feedback, new FeedbacksPage());


            // AccordionControl formun kontrol listesine ekleniyor
            this.Controls.Add(accordionControl);
        }

        private void AddAccordionElement(string text, Image image, UserControl page)
        {
            AccordionControlElement element = new AccordionControlElement
            {
                Text = text,
                Style = ElementStyle.Item,
                ImageOptions =
        {
            Image = ResizeImage(image, new Size(24, 24))
        }
            };

            element.Click += (s, e) => ChangePageAndHighlight(element, page);
            accordionControl.Elements.Add(element);
        }


        private void ChangePageAndHighlight(AccordionControlElement element, UserControl content)
        {
            // Seçili öğeyi vurgula
            selectedElement = element;

            foreach (AccordionControlElement elem in accordionControl.Elements)
            {
                elem.Appearance.Normal.BackColor = Color.LightGray;
            }

            element.Appearance.Normal.BackColor = Color.LightSeaGreen;

            // Seçili sayfayı yükle
            LoadContentToPanel(content);
        }

        private void LoadContentToPanel(UserControl content)
        {
            // panelContainer temizleniyor ve yeni içerik yükleniyor
            panelContainer.Controls.Clear();
            content.Dock = DockStyle.Fill;
            panelContainer.Controls.Add(content);
        }

        private Image ResizeImage(Image imgToResize, Size size)
        {
            // Resim boyutlandırma işlemi
            return (Image)(new Bitmap(imgToResize, size));
        }

        private void panelContainer_Paint(object sender, PaintEventArgs e)
        {
            // İsteğe bağlı: Panel üzerine özel çizimler yapılabilir
        }
    }
}
