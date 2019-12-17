using System;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
namespace SmeLms
{
    class Util
    {
        public static string Encrypt(string input)
        {
            byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes("sblw-3hn8-sqoy10");
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string input)
        {

            byte[] inputArray = Convert.FromBase64String(input);
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes("sblw-3hn8-sqoy10");
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
        public static string PesudoRefNo()
        {
            string refn = "T";
            for (int i = 1; i <= 10; i++)
            {
                refn += new Random().Next(0, 9);
            }
            return refn;
        }
        public static void SendMail(string content, string subject, params string[] recipients)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("noreply@smelms.com");
                foreach (string r in recipients)
                {

                    message.Bcc.Add(new MailAddress(r));

                }

                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = content;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("smeupturnlms@gmail.com", "gpclbvidhudsfrit");

                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static string GenToken()
        {
            string id = "";
            string token = "";
            do
            {
                for (int i = 1; i <= 4; i++)
                {
                    id += new Random().Next(0, 9);
                }
                token = Encrypt(id);
            }
            while (token.Contains("/"));
            return token;
        }
    }
}