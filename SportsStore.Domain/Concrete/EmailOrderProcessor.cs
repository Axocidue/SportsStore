using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportsStore.Domain.Entities;
using SportsStore.Domain.Abstract;
using System.Net.Mail;
using System.Net;

namespace SportsStore.Domain.Concrete
{
    public class EmailSettings
    {
        public string MailToAddress = "alex.guan@sonymobile.com";
        public string MailFromAddress = "alexander.guan@live.com";
        public bool UseSsl = true;
        public string Username = "alexander.guan@live.com";
        public string Password = "1234567890";
        public string ServerName = "d.smtp.live.com";
        public int ServerPort = 587;
        public bool WriteAsFile = false;
        public string FileLocation = @"C:\Users\strategyadmin\Documents\Visual Studio 2013\Projects\SportsStore\Email.Files\";
    }
    public class EmailOrderProcessor : IOrderProcessor
    {
        private EmailSettings emailSettings;

        public EmailOrderProcessor(EmailSettings settings)
        {
            emailSettings = settings;
        }

        public void ProcessOrder(Cart cart, ShippingDetails shippingDetails)
        {
            using (var smtpClient =
                new SmtpClient
                {
                    EnableSsl = emailSettings.UseSsl,
                    Host = emailSettings.ServerName,
                    Port = emailSettings.ServerPort,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password)
                }
                )
            {
                if(emailSettings.WriteAsFile)
                {
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtpClient.PickupDirectoryLocation = emailSettings.FileLocation;
                    smtpClient.EnableSsl = false;
                }

                var sb = new StringBuilder()
                    .AppendLine("A new order has been submitted.")
                    .AppendLine("---")
                    .AppendLine("Items: ");

                foreach(var line in cart.Lines)
                {
                    var subTotal = line.Product.Price * line.Quantity;
                    sb.AppendFormat("{0} x {1} (subtotal: {2:c}", line.Product.Price, line.Quantity, subTotal);
                    sb.AppendLine();
                }

                sb.AppendFormat("Total Order Value: {0:c}", cart.ComputeTotalValue());
                sb.AppendLine();

                sb.AppendLine("---")
                    .AppendLine("Ship to:")
                    .AppendLine(shippingDetails.Name)
                    .AppendLine(shippingDetails.Line1)
                    .AppendLine(shippingDetails.Line2 ?? "")
                    .AppendLine(shippingDetails.Line3 ?? "")
                    .AppendLine(shippingDetails.City)
                    .AppendLine(shippingDetails.State ?? "")
                    .AppendLine(shippingDetails.Country)
                    .AppendLine(shippingDetails.Zip)
                    .AppendLine("---")
                    .AppendFormat("Gift wrap: {0}", shippingDetails.GiftWrap ? "Yes" : "No");

                var mailMessage = new MailMessage(emailSettings.MailFromAddress, emailSettings.MailToAddress, "New order submitted", sb.ToString());

                if (emailSettings.WriteAsFile) mailMessage.BodyEncoding = Encoding.ASCII;

                smtpClient.Send(mailMessage);


            }
        }
    }
}
