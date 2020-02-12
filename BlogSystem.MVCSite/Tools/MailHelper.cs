using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace BlogSystem.MVCSite.Tools
{
    public class MailHelper
    {
        public static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>  
        /// 发送邮件程序调用方法 SendMail("abc@126.com", "某某人", "cba@126.com", "你好", "我测试下邮件", "邮箱登录名", "邮箱密码", "smtp.126.com", true,);  
        /// </summary>  
        /// <param name="from">发送人邮件地址</param>  
        /// <param name="fromname">发送人显示名称</param>  
        /// <param name="to">发送给谁（邮件地址）</param>  
        /// <param name="subject">标题</param>  
        /// <param name="body">内容</param>  
        /// <param name="username">邮件登录名</param>  
        /// <param name="password">邮件密码</param>  
        /// <param name="server">邮件服务器 smtp服务器地址</param>  
        /// <param   name= "IsHtml "> 是否是HTML格式的邮件 </param>  
        /// <returns>send ok</returns>  
        public static bool SendMail(string from, string fromname, string to, string subject, string body, string server, string username, string password, bool IsHtml)
        {
            SmtpClient smtp = new SmtpClient(server);
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            //指定发送方式  
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            //发件人身份验证,否则163   发不了  
            smtp.UseDefaultCredentials = true;
            //指定登录名和密码  
            smtp.Credentials = new System.Net.NetworkCredential(username, password);
            //超时时间  
            smtp.Timeout = 10000;

            //邮件发送类  
            MailMessage mail = new MailMessage();
            //是谁发送的邮件  
            mail.From = new MailAddress(from, fromname);
            //发送给谁  
            mail.To.Add(to);
            //标题  
            mail.Subject = subject;
            //内容编码  
            mail.BodyEncoding = Encoding.Default;
            //发送优先级  
            mail.Priority = MailPriority.High;
            //邮件内容  
            mail.Body = body;
            //是否HTML形式发送  
            mail.IsBodyHtml = IsHtml;
            //邮件服务器和端口  

            try
            {
                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "发送邮件失败");
                return false;
            }
            finally
            {
                mail.Dispose();
            }
        }

        //读取指定URL地址的HTML，用来以后发送网页用  
        public static string ScreenScrapeHtml(string url)
        {
            //读取stream并且对于中文页面防止乱码  
            StreamReader reader = new StreamReader(System.Net.WebRequest.Create(url).GetResponse().GetResponseStream(), System.Text.Encoding.UTF8);
            string str = reader.ReadToEnd();
            reader.Close();
            return str;
        }

        //发送plaintxt  
        public static bool SendText(string from, string fromname, string to, string subject, string body, string server, string username, string password)
        {
            return SendMail(from, fromname, to, subject, body, server, username, password, false);
        }

        //发送HTML内容  
        public static bool SendHtml(string from, string fromname, string to, string subject, string body, string server, string username, string password)
        {
            return SendMail(from, fromname, to, subject, body, server, username, password, true);
        }

        //发送制定网页  
        public static bool SendWebUrl(string from, string fromname, string to, string subject, string server, string username, string password, string url)
        {
            //发送制定网页  
            return SendHtml(from, fromname, to, subject, ScreenScrapeHtml(url), server, username, password);

        }
        //默认发送格式  
        public static bool SendEmailDefault(string ToEmail, string token)
        {
            StringBuilder MailContent = new StringBuilder();
            MailContent.Append("亲爱的用户：<br/>");
            MailContent.Append("    您好！你于");
            MailContent.Append(DateTime.Now.ToString("yyyy-MM-dd HH:MM:ss"));
            MailContent.Append("申请找回密码。<br/>");
            MailContent.Append("　　　为了安全起见，请用户点击以下链接重设个人密码：<br/><br/>");
            string url = ConfigurationManager.AppSettings["Url"].ToString() + "/Home/ResetPassword?Token=" + token;
            MailContent.Append("<a href='" + url + "'>" + url + "</a><br/><br/>");
            MailContent.Append(" (如果无法点击该URL链接地址，请将它复制并粘帖到浏览器的地址输入框，然后单击回车即可。)");
            return SendHtml(ConfigurationManager.AppSettings["EmailName"].ToString(), "博客管理中心", ToEmail, "找回密码", MailContent.ToString(), ConfigurationManager.AppSettings["EmailService"].ToString(), ConfigurationManager.AppSettings["EmailName"].ToString(), ConfigurationManager.AppSettings["EmailPass"].ToString()); //这是从webconfig中自己配置的。
        }
    }
}
// webconfig配置信息
//  < add key = "EmailName" value = "××××@163.com" />               
//    < add key = "EmailPass" value = "××××" />                  
//        < add key = "EmailService" value = "smtp.163.com" />
//说明： 这里面的"EmailService"得与你自己设置邮箱的smtp/POP3/...服务要相同， 大部分是根据@后面的进行配置。我是用163邮箱配置的。 可以根据自己需要自己配置。



//  后台调用的方法
//public ActionResult SendEmail(string EmailName)
//           {
//               EmailName = Helper.FI_DesTools.DesDecrypt(EmailName);
//               if (!Regex.IsMatch(EmailName, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
//               {
//                   return Content("0");
//               }
//               string f_username = "";
//               string f_pass = "";
//               string f_times = Helper.FI_DesTools.DesEncrypt(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//               List<user> list = (from a in users where a.emailaddress == EmailName select a).ToList();
//               if (list.Count > 0)
//               {
//                   f_username = Helper.FI_DesTools.DesEncrypt(list[0].×××);
//                   f_pass = Helper.FI_DesTools.DesEncrypt(list[0].×××);

//                   bool flag = Helper.MailService.SendEmailDefault(EmailName, “×××”,“×××”, “×××”);  //这里面的参数根据自己需求自己定，最好进行加密
//                   if (flag)
//                   {
//                       return Content("true");
//                   }
//                   else
//                   {
//                       return Content("false");
//                   }
//               }
//               else
//               {
//                   return Content("false");
//               }
//           }