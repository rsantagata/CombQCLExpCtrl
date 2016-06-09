using System;
using System.Net.Mail;
using SharedCode;

namespace EDMPlotter
{
    public class EvernoteDispatcher
    {
        ExperimentControl controller;
        public EvernoteDispatcher(ExperimentControl c)
        {
            controller = c;
        }
        public void SendToEvernote(DataSet[] d)
        {
            try
            {
                /*
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("***@gmail.com");
                mail.To.Add("****@****");
                mail.Subject = "Test Mail";
                mail.Body = "This is for testing SMTP mail from GMAIL";

                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential("", "");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                controller.ToConsole("Mail Sent");*/
            }
            catch (Exception ex)
            {
                controller.ToConsole(ex.ToString());
            }
        }

    }
}




