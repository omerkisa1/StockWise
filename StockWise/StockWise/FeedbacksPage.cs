using System;
using System.Net.Mail;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.DXErrorProvider;

namespace StockWise
{
    public partial class FeedbacksPage : UserControl
    {
        private TextEdit textEditName;
        private TextEdit textEditEmail;
        private MemoEdit memoEditMessage;
        private SimpleButton buttonSubmit;
        private DXValidationProvider validationProvider;

        public FeedbacksPage()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
         
            validationProvider = new DXValidationProvider();

 
            textEditName = new TextEdit
            {
                Location = new System.Drawing.Point(20, 20),
                Width = 300,
                Properties = { NullText = "Your Name" }
            };
            textEditName.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            Controls.Add(textEditName);
            AddValidationRule(textEditName, "Name cannot be empty.");

      
            textEditEmail = new TextEdit
            {
                Location = new System.Drawing.Point(20, 60),
                Width = 300,
                Properties = { NullText = "Your Email" } 
            };
            textEditEmail.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            Controls.Add(textEditEmail);
            AddValidationRule(textEditEmail, "Email cannot be empty.");

     
            memoEditMessage = new MemoEdit
            {
                Location = new System.Drawing.Point(20, 100),
                Width = 500,
                Height = 150,
                Properties = { NullText = "Your Message" } 
            };
            memoEditMessage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(memoEditMessage);
            AddValidationRule(memoEditMessage, "Message cannot be empty.");

            buttonSubmit = new SimpleButton
            {
                Text = "Submit",
                Location = new System.Drawing.Point(20, 270),
                Width = 100
            };
            buttonSubmit.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            buttonSubmit.Click += ButtonSubmit_Click;
            Controls.Add(buttonSubmit);


            this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.AutoScroll = true;
        }

        private void AddValidationRule(BaseEdit control, string errorText)
        {
            var validationRule = new ConditionValidationRule
            {
                ConditionOperator = ConditionOperator.IsNotBlank,
                ErrorText = errorText
            };
            validationProvider.SetValidationRule(control, validationRule);
        }

        private void ButtonSubmit_Click(object sender, EventArgs e)
        {
            if (!validationProvider.Validate())
            {
                XtraMessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("your_email@example.com"), // here too
                    Subject = "Feedback from " + textEditName.Text,
                    Body = $"Name: {textEditName.Text}\nEmail: {textEditEmail.Text}\nMessage:\n{memoEditMessage.Text}"
                };
                mail.To.Add("your_email@example.com");  // dont forget to setup ur mail here

                var smtpClient = new SmtpClient("smtp.example.com") 
                {
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential("your_email@example.com", "your_password"),
                    EnableSsl = true
                };
                smtpClient.Send(mail);

                XtraMessageBox.Show("Your feedback has been sent successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                
                textEditName.Text = string.Empty;
                textEditEmail.Text = string.Empty;
                memoEditMessage.Text = string.Empty;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("An error occurred while sending your feedback: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void FeedbacksPage_Load(object sender, EventArgs e)
        {

        }
    }
}
