using System;
using System.IO;
using System.Net.Mail;

namespace Velocloud2Connectwise
{
    public static class Helpers
    {
        public static void ExportObjectToCsv<T>(T ObjectToExport, string fileName)
        {
            string csv = ServiceStack.Text.CsvSerializer.SerializeToString<T>(ObjectToExport);
            StreamWriter writer = new StreamWriter(fileName);
            try
            {
                writer.Write(csv);
            }
            catch (FileNotFoundException ex)
            {
                throw ex;
            }
            finally
            {
                writer.Close();
            }
        }
        public static bool SendEmail(string smtpServer, string to, string from, string subject, string body, string cc = null, string attachments = null, bool isBodyHtml = false)
        {
            using (MailMessage mail = new MailMessage())
            using (SmtpClient client = new SmtpClient())
            {
                string[] emails = to.Split(',');

                if (emails.Length > 0 && !String.IsNullOrEmpty(emails[0]))
                {
                    client.Port = 25;
                    client.Timeout = 600000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = true;
                    client.Host = smtpServer;

                    //Assigns email info
                    mail.Subject = subject;
                    mail.IsBodyHtml = isBodyHtml;
                    mail.Body = body;
                    mail.From = new MailAddress(from);

                    //Adds emails
                    foreach (string email in emails)
                    {
                        if (!String.IsNullOrEmpty(email))
                        {
                            mail.To.Add(new MailAddress(email));
                        }
                    }

                    //Adds attachments
                    if (!string.IsNullOrEmpty(attachments))
                    {
                        string[] files = attachments.Split(',');

                        if (files.Length > 0 && !String.IsNullOrEmpty(files[0]))
                        {
                            foreach (string file in files)
                            {
                                if (!String.IsNullOrEmpty(file))
                                {
                                    mail.Attachments.Add(new Attachment(file));
                                }
                            }
                        }
                    }

                    //Adds CC emails
                    if (!string.IsNullOrEmpty(cc))
                    {
                        string[] ccEmails = cc.Split(',');

                        foreach (string email in ccEmails)
                        {
                            if (!String.IsNullOrEmpty(email))
                            {
                                mail.CC.Add(new MailAddress(email));
                            }
                        }
                    }

                    client.Send(mail);
                }
            }

            return true;
        }

    }
}
