using System;
using System.Net.Mail;
using System.Text;
using Nostradabus.Configuration;

namespace Nostradabus.BusinessComponents.Common
{
    /// <summary>
    /// Helper class for sending emails
    /// </summary>
    public class EmailComponent
    {
        private static readonly string SiteEmailAddress = ConfigurationManager.Instance.EmailSettings.SiteEmailAddress;
        /// <summary>
        /// If this value is set to false, the mail will not be sent. This is for unit testing purposes. 
        /// </summary>
        public static bool IsMailEnabled
        {
            get { return ConfigurationManager.Instance.EmailSettings.Enabled; }
        }
		
		//public static void SendContactUs(string subject, string msg)
		//{
		//    MailMessage mailMessage = CreateContactUsMailMessage(subject, msg);

		//    SendMessage(mailMessage);
		//}

		//public static void SendNewPasswordMail(string to, string newPassword)
		//{
		//    MailMessage mailMessage = CreateNewPasswordMailMessage(to, newPassword);

		//    SendMessage(mailMessage);
		//}
		
    	#region Private Methods

        private static void SendMessage(MailMessage mailMessage)
        {
            // if mail is enabled creates mailmessage object and send mail
            if (IsMailEnabled)
            {
                try
                {
                    new SmtpClient().Send(mailMessage);
                }
                catch (SmtpException)
                {
                    // TODO: see to do what with exception 
                    throw;
                }
            }
        }

		//private static MailMessage CreateContactUsMailMessage(string subject, string msg)
		//{

		//    MailAddress mailAddressFrom = new MailAddress(SecurityManager.Instance.LoggedUser.Email, Email.DisplayNameFrom);
		//    MailAddress mailAddressTo = new MailAddress(SiteEmailAddress);

		//    MailMessage mailMessage = new MailMessage(mailAddressFrom, mailAddressTo);

		//    mailMessage.Subject = subject;
		//    mailMessage.Body = msg;

		//    return mailMessage;
		//}

		//private static MailMessage CreateNewPasswordMailMessage(string to, string newPassword)
		//{
		//    MailAddress mailAddressFrom = new MailAddress(SiteEmailAddress, Email.DisplayNameFrom);
		//    MailAddress mailAddressTo = new MailAddress(to);
            
		//    MailMessage mailMessage = new MailMessage(mailAddressFrom,mailAddressTo);
            
		//    mailMessage.Subject = Email.ForgotPasswordSubject;
		//    mailMessage.Body = string.Format(Email.ForgotPasswordBody, newPassword);

		//    return mailMessage;
		//}

        #endregion
    }
}