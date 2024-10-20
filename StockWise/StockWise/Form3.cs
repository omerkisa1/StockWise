using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            SetupAccordionControl();  
        }

    
        private void SetupAccordionControl()
        {
            accordionControl = new AccordionControl();
            accordionControl.Dock = DockStyle.Left;  
            accordionControl.Width = 250;  

            // will check this colors later
            accordionControl.Appearance.Item.Normal.BackColor = Color.LightGray; 
            accordionControl.Appearance.Item.Normal.ForeColor = Color.Black; 
            accordionControl.Appearance.Item.Hovered.BackColor = Color.LightBlue;  
            accordionControl.Appearance.Item.Pressed.BackColor = Color.LightSeaGreen; 


            AccordionControlElement productManagement = new AccordionControlElement
            {
                Text = "Ürün Yönetimi",
                Style = ElementStyle.Item,
                ImageOptions = {
                    Image = ResizeImage(Image.FromFile("D:\\product-development.png"), new Size(24, 24))
                }
            };
            productManagement.Click += (s, e) => ChangePageAndHighlight(productManagement, new ProductManagement());

           
            AccordionControlElement categories = new AccordionControlElement
            {
                Text = "Kategoriler",
                Style = ElementStyle.Item,
                ImageOptions = {
                    Image = ResizeImage(Image.FromFile("D:\\category.png"), new Size(24, 24)) 
                }
            };
            categories.Click += (s, e) => ChangePageAndHighlight(categories, new CategoriesPage());


            AccordionControlElement stockManagement = new AccordionControlElement
            {
                Text = "Stok Yönetimi",
                Style = ElementStyle.Item,
                ImageOptions = {
                    Image = ResizeImage(Image.FromFile("D:\\inventory-management.png"), new Size(24, 24))  
                }
            };
            stockManagement.Click += (s, e) => ChangePageAndHighlight(stockManagement, new StockManagementPage());

   
            AccordionControlElement orders = new AccordionControlElement
            {
                Text = "Siparişler",
                Style = ElementStyle.Item,
                ImageOptions = {
                    Image = ResizeImage(Image.FromFile("D:\\package-tracking.png"), new Size(24, 24)) 
                }
            };
            orders.Click += (s, e) => ChangePageAndHighlight(orders, new OrdersPage());

            
            AccordionControlElement salesAnalytics = new AccordionControlElement
            {
                Text = "Satış Analizleri",
                Style = ElementStyle.Item,
                ImageOptions = {
                    Image = ResizeImage(Image.FromFile("D:\\sales.png"), new Size(24, 24))  
                }
            };
            salesAnalytics.Click += (s, e) => ChangePageAndHighlight(salesAnalytics, new SalesAnalyticsPage());

         
            AccordionControlElement feedbacks = new AccordionControlElement
            {
                Text = "Geri Bildirimler",
                Style = ElementStyle.Item,
                ImageOptions = {
                    Image = ResizeImage(Image.FromFile("D:\\feedback.png"), new Size(24, 24))  
                }
            };
            feedbacks.Click += (s, e) => ChangePageAndHighlight(feedbacks, new FeedbacksPage());

         
            accordionControl.Elements.AddRange(new AccordionControlElement[]
            {
                productManagement, categories, stockManagement, orders, salesAnalytics, feedbacks
            });

      
            this.Controls.Add(accordionControl);
        }

 
        private void LoadContentToPanel(UserControl content)
        {
  
            panelContainer.Controls.Clear();
            
            content.Dock = DockStyle.Fill;
            panelContainer.Controls.Add(content);
        }

        
        private void ChangePageAndHighlight(AccordionControlElement element, UserControl content)
        {
            
            selectedElement = element;

            
            foreach (AccordionControlElement elem in accordionControl.Elements)
            {
                elem.Appearance.Normal.BackColor = Color.LightGray;  
            }

            
            element.Appearance.Normal.BackColor = Color.LightGray;  
            LoadContentToPanel(content);  
        }

        // not sure do I need this resize thing. I will let this here and check it later.
        private Image ResizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }
    }
}
