using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EmailModels
{
    public class EmailMessage
    {
        public List<MailboxAddress> To { get; set; }

        public string Subject { get; set; }

        public string HtmlPath { get; set; }

        public List<(string placeHolder, string actualValue)> Values { get; set; }


        public EmailMessage(IEnumerable<EmailAddress> to, string subject, string htmlTemplate, 
            List<(string placeHolder, string actualValue)> values)
        {
            To = new List<MailboxAddress>();

            To.AddRange(to.Select(ea => new MailboxAddress(ea.DisplayName, ea.Address)));
            Subject = subject;
            HtmlPath = htmlTemplate;
            Values = values;
        }
    }
}
