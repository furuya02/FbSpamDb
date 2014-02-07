using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mail;
using MailMessage = System.Net.Mail.MailMessage;

namespace FbSpamDb.Models{
    public class Mail{
        public static bool SendMe(string from, string subject, string body){
            try{
                var smtp = new SmtpClient();
                smtp.Host = "XXXXX";
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("XXXXX", "XXXXX");
                smtp.EnableSsl = true;
                var msg = new MailMessage(){Subject = subject, Body = body, From = new MailAddress("XXXXX", from)};
                msg.To.Add("XXXXX");
                smtp.Send(msg);
                smtp.Dispose();
                return true;
            } catch (Exception e){
                return false;
            }
        }
    }
}

    

