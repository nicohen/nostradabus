using System;
using System.Text;
using System.Threading;
using System.Web;
using log4net;
using log4net.Config;

namespace Nostradabus.Common.Logger
{
    /// <summary>
    /// Helper for Loggin.
    /// </summary>
    /// <remarks>Use Log4Net Implementation.</remarks>
    public class LoggingManager
    {
        private static readonly ILog loggerInfo = LogManager.GetLogger("Logger.Info");
        private static readonly ILog loggerDebug = LogManager.GetLogger("Logger.Debug");
        private static readonly ILog loggerError = LogManager.GetLogger("Logger.Error");
        private static readonly ILog loggerWarning = LogManager.GetLogger("Logger.Warning");
        private static readonly ILog loggerFatal = LogManager.GetLogger("Logger.Fatal");


        /// <summary>
        /// Initializes the <see cref="LoggingManager"/> class.
        /// </summary>
        static LoggingManager()
        {
            XmlConfigurator.Configure();
        }

        /// <summary>
        /// Gets the current locale id from the CurrentUICulture
        /// </summary>
        /// <returns></returns>
        private static string GetCurrentLocaleId
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name.Replace("-", "_"); }
        }

        private static void SetMDCsToLog4net()
        {
				MDC.Set("url", HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.Url.ToString() : null);
				MDC.Set("localeId", GetCurrentLocaleId);
				MDC.Set("ip", HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.UserHostAddress : null);
				MDC.Set("requestVerb", HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.RequestType : null);
				MDC.Set("userId", (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null) ? HttpContext.Current.User.Identity.Name : null);
				MDC.Set("uniqueidentifier", Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Gets the information to log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private static string GetInformationToLog(string message, Exception exception)
        {
            var infoToLog = new StringBuilder();

            infoToLog.AppendFormat("<<<<<<------------- {0}", Environment.NewLine);

            infoToLog.Append(!string.IsNullOrEmpty(message) ? message : string.Empty);
            infoToLog.Append(Environment.NewLine);
            infoToLog.AppendFormat("Url: {0}",
								   HttpContext.Current != null && HttpContext.Current.Request != null
                                       ? HttpContext.Current.Request.Url.ToString()
                                       : string.Empty);
            if (exception != null)
            {
                infoToLog.Append(Environment.NewLine);
                infoToLog.AppendFormat("Exception: {0}", GetFullMessage(exception));
                infoToLog.Append(Environment.NewLine);
                infoToLog.AppendFormat("StackTrace: {0}", exception.StackTrace);
                infoToLog.Append(Environment.NewLine);
                if(exception.InnerException != null)
                    infoToLog.AppendFormat("InnerStackTrace: {0}{1}", exception.InnerException.StackTrace, Environment.NewLine);

            }

            infoToLog.AppendFormat("Logged User: {0}", "None"); //TODO: The current logged user name;
            infoToLog.Append(Environment.NewLine);
            infoToLog.AppendFormat("IP Address: {0}",
								   HttpContext.Current != null && HttpContext.Current.Request != null
                                       ? HttpContext.Current.Request.UserHostAddress
                                       : string.Empty);
            infoToLog.Append(Environment.NewLine);
            infoToLog.AppendFormat("Request Verb: {0}",
								   HttpContext.Current != null && HttpContext.Current.Request != null ? HttpContext.Current.Request.RequestType : string.Empty);
            infoToLog.Append(Environment.NewLine);
            infoToLog.AppendFormat("Locale ID: {0}", GetCurrentLocaleId);
            infoToLog.Append(Environment.NewLine);
            
            infoToLog.AppendFormat("-------------<<<<<<", Environment.NewLine);
            
            return infoToLog.ToString();
        }

        /// <summary>
        /// Gets the full message (Exception message + NewLine + InnerException message). 
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static string GetFullMessage(Exception exception)
        {
            if (exception != null)
            {
                string msg = exception.Message;

                if (exception.InnerException != null)
                    msg = string.Concat(msg, Environment.NewLine, GetFullMessage(exception.InnerException));

                return msg;
            }

            return string.Empty;
        }

        #region Nested type: Logging

        public class Logging
        {
            /// <summary>
            /// Debugs the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            public static void Debug(string message)
            {
                Debug(message, null);
            }

            /// <summary>
            /// Debugs the specified exception.
            /// </summary>
            /// <param name="exception">The exception.</param>
            public static void Debug(Exception exception)
            {
                Debug(null, exception);
            }

            /// <summary>
            /// Debugs the specified message and exception.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public static void Debug(string message, Exception exception)
            {
                if (loggerDebug.IsDebugEnabled)
                {
                    try
                    {
                        SetMDCsToLog4net();
                        loggerDebug.Debug(GetInformationToLog(message, exception));    
                    }
                    catch(Exception ex)
                    {
                        loggerDebug.Debug(ex.Message);
                    }
                    
                }
            }

            /// <summary>
            /// Infoes the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            public static void Info(string message)
            {
                Info(message, null);
            }

            /// <summary>
            /// Infoes the specified exception.
            /// </summary>
            /// <param name="exception">The exception.</param>
            public static void Info(Exception exception)
            {
                Info(null, exception);
            }

            /// <summary>
            /// Infoes the specified message and exception.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public static void Info(string message, Exception exception)
            {
                if (loggerInfo.IsInfoEnabled)
                {
					try
					{
						SetMDCsToLog4net();
						loggerInfo.Info(GetInformationToLog(message, exception));
					}
					catch(Exception ex)
					{
						loggerInfo.Info(ex.Message);
						loggerInfo.Info(ex.StackTrace);
					}
                }
            }

            /// <summary>
            /// Errors the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            public static void Error(string message)
            {
                Error(message, null);
            }

            /// <summary>
            /// Errors the specified exception.
            /// </summary>
            /// <param name="exception">The exception.</param>
            public static void Error(Exception exception)
            {
                Error(null, exception);
            }

            /// <summary>
            /// Errors the specified message and exception.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public static void Error(string message, Exception exception)
            {
                if (loggerError.IsErrorEnabled)
                {
					try
					{
						SetMDCsToLog4net();
						loggerError.Error(GetInformationToLog(message, exception));
					}
					catch(Exception ex)
					{
						loggerError.Error(ex.Message);
						loggerError.Error(ex.StackTrace);
					}
                	
                }
            }

            /// <summary>
            /// Warnings the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            public static void Warning(string message)
            {
                Warning(message, null);
            }

            /// <summary>
            /// Warnings the specified exception.
            /// </summary>
            /// <param name="exception">The exception.</param>
            public static void Warning(Exception exception)
            {
                Warning(null, exception);
            }

            /// <summary>
            /// Warnings the specified message and exception.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public static void Warning(string message, Exception exception)
            {
                if (loggerWarning.IsWarnEnabled)
                {
					try
					{
						SetMDCsToLog4net();
						loggerWarning.Warn(GetInformationToLog(message, exception));
					}
					catch(Exception ex)
					{
						loggerWarning.Warn(ex.Message);
						loggerWarning.Warn(ex.StackTrace);
					}
                	
                }
            }

            /// <summary>
            /// Fatals the specified message.
            /// </summary>
            /// <param name="message">The message.</param>
            public static void Fatal(string message)
            {
                Fatal(message, null);
            }

            /// <summary>
            /// Fatals the specified exception.
            /// </summary>
            /// <param name="exception">The exception.</param>
            public static void Fatal(Exception exception)
            {
                Fatal(null, exception);
            }

            /// <summary>
            /// Fatals the specified message and exception.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="exception">The exception.</param>
            public static void Fatal(string message, Exception exception)
            {
                if (loggerFatal.IsFatalEnabled)
                {
					try
					{
						SetMDCsToLog4net();
						loggerFatal.Fatal(GetInformationToLog(message, exception));	
					}
                    catch(Exception e)
                    {
                        loggerFatal.Fatal(exception.Message);
						loggerFatal.Fatal(exception.StackTrace);
                    }
                 }
                
            }

        }

        #endregion
    }
}