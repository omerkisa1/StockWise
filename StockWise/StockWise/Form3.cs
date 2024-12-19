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
            AddAccordionElement("Ürün Yönetimi", "D:\\product-development.png", new ProductManagementPage());
            //AddAccordionElement("Kategoriler", "D:\\category.png", new CategoriesPage()); this page deprecated
            AddAccordionElement("Stok Yönetimi", "D:\\inventory-management.png", new StockManagementPage());
            AddAccordionElement("Siparişler", "D:\\package-tracking.png", new OrdersPage());
            AddAccordionElement("Satış Analizleri", "D:\\sales.png", new SalesAnalyticsPage());
            AddAccordionElement("Geri Bildirimler", "D:\\feedback.png", new FeedbacksPage());

            // AccordionControl formun kontrol listesine ekleniyor
            this.Controls.Add(accordionControl);
        }

        private void AddAccordionElement(string text, string imagePath, UserControl page)
        {
            // Tek bir menü öğesi ekleme işlemi
            AccordionControlElement element = new AccordionControlElement
            {
                Text = text,
                Style = ElementStyle.Item,
                ImageOptions =
                {
                    Image = ResizeImage(Image.FromFile(imagePath), new Size(24, 24))
                }
            };

            // Menü öğesine tıklama olayı
            element.Click += (s, e) => ChangePageAndHighlight(element, page);

            // AccordionControl öğesine ekleniyor
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
