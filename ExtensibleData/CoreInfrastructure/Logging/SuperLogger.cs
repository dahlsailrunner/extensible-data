using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.ExtraInformation;

namespace CoreInfrastructure.Logging
{
    public static class SuperLogger
    {
        private static readonly LogWriter logWriter;

        static SuperLogger()
        {
            var path = @".\";
            //if (!Directory.Exists(path))
            //    path = UtilityMethods.GetCommonPath();

            var configSource = new FileConfigurationSource(Path.Combine(path, "EntLib.config"));
            var logWriterFactory = new LogWriterFactory(configSource);
            logWriter = logWriterFactory.Create();

            
        }

        public static void WriteUpdateLog(int customerId, string updateType, DateTime updateDs, string userName, string oldVal, string newVal, string remarks = null)
        {
            var dict = new Dictionary<string, object>
            {
                {"CustomerID", customerId},                
                {"UpdateType", updateType},
                {"UpdateDateTime", updateDs},
                {"UserName", userName},
                {"OldValue", oldVal},
                {"NewValue", newVal},
                {"Remarks", remarks},                
            };
            WriteLog("", LoggingCategory.UpdateLog, dict);
        }

        public static void WriteLog(object message, IEnumerable<string> categories, IDictionary<string, object> additionalProperties)
        {
            MethodBase method;
            var title = string.Empty;
            var exception = message as Exception;
            string summaryMessage;

            var extendedInfo = new Dictionary<string, object>();
            if (exception != null)
            {
                message = FormatExceptionMessage(exception, ref title, out summaryMessage);
                method = exception.TargetSite;
                if (method.Name == "Execute" && method.DeclaringType != null && method.DeclaringType.Name == "MSSqlSP")
                {
                    // go up the call stack of the exception to find the method that called MSSqlSP.Execute!
                    var st = new StackTrace(exception);
                    var i = 0;
                    while (i < st.FrameCount - 1)
                    {
                        method = st.GetFrame(i).GetMethod();
                        if (method.Name != "Execute" && method.DeclaringType != null && method.DeclaringType.Name != "MSSqlSP")
                            break;
                        i++;
                    }
                }
            }
            else
            {
                summaryMessage = message.ToString();
                var st = new StackTrace();
                method = st.GetFrame(1).GetMethod();
                if (method.Name == "WriteLog")  // don't want to show WriteLog if the caller just call an overload
                    method = st.GetFrame(2).GetMethod();
            }

            var entryAssembly = Assembly.GetEntryAssembly();

            if (additionalProperties == null)
                additionalProperties = new Dictionary<string, object>();

            if (!additionalProperties.ContainsKey("Executable") && entryAssembly != null)
                additionalProperties.Add("Executable", Path.GetFileName(entryAssembly.Location));

            if (method.DeclaringType != null && !additionalProperties.ContainsKey("Class"))
                additionalProperties.Add("Class", method.DeclaringType.Name);

            if (!additionalProperties.ContainsKey("Assembly"))
                additionalProperties.Add("Assembly", method.Module.Name);

            if (!additionalProperties.ContainsKey("Method"))
                additionalProperties.Add("Method", method.Name);

            var moreInfo = GetExtendedInfoProperties(additionalProperties);
            extendedInfo.Add("SummaryMessage", summaryMessage);
            foreach (var key in moreInfo.Keys.Where(key => !extendedInfo.ContainsKey(key) && moreInfo[key] != null))
            {
                extendedInfo.Add(key, moreInfo[key].ToString());
            }

            //NOTE: don't want empty values getting logged
            var realList = extendedInfo
                .Where(item => !string.IsNullOrEmpty(item.Key) && (!string.IsNullOrEmpty(item.Value.ToString())))
                .ToDictionary<KeyValuePair<string, object>, string, object>(item => item.Key,
                                                                            item => item.Value.ToString());

            title = (string.IsNullOrEmpty(title)) ? "Informational" : title;
            logWriter.Write(message, categories, 0, 0, TraceEventType.Information, title, realList);
        }

        public static void WriteLog(object message)
        {
            WriteLog(message, new List<string> { "General" }, new Dictionary<string, object>());
        }

        public static void WriteLog(object message, LoggingCategory category, IDictionary<string, object> additionalProperties)
        {
            WriteLog(message, new List<string> { category.ToString() }, additionalProperties);
        }

        public static void WriteLog(object message, LoggingCategory category)
        {
            WriteLog(message, new List<string> { category.ToString() }, new Dictionary<string, object>());
        }

        public static void WriteLog(object message, IEnumerable<string> categories)
        {
            WriteLog(message, categories, new Dictionary<string, object>());
        }

        private static string FormatExceptionMessage(Exception exception, ref string title, out string summary, string prepend = "\t")
        {
            var exceptionMessage = new StringBuilder();

            //var nwpSqlEx = exception as NwpSqlException;
            //if (String.IsNullOrEmpty(title))
            //{
            //    title = nwpSqlEx != null ? nwpSqlEx.StoredProcName : (exception.TargetSite == null ? "" : exception.TargetSite.Name);
            //}
            // todo: Get stored proc name in here.....
            summary = exception.Message;

            exceptionMessage.Append("\n" + prepend + "Exception:" + exception.GetType());
            exceptionMessage.Append("\n" + prepend + "Message:" + exception.Message);

            exceptionMessage.Append(GetOtherExceptionProperties(exception, "\n" + prepend));

            exceptionMessage.Append("\n" + prepend + "Source:" + exception.Source);
            exceptionMessage.Append("\n" + prepend + "StackTrace:" + exception.StackTrace);

            exceptionMessage.Append(GetExceptionData("\n" + prepend, exception));

            if (exception.InnerException != null)
                exceptionMessage.Append("\n" + prepend + "InnerException: " + FormatExceptionMessage(exception.InnerException, ref title, out summary, prepend + "\t"));

            return exceptionMessage.ToString();
        }

        private static string GetExceptionData(string prependText, Exception exception)
        {
            var exData = new StringBuilder();
            foreach (var key in exception.Data.Keys.Cast<object>().Where(key => exception.Data[key] != null))
            {
                exData.Append(prependText + String.Format("DATA-{0}:{1}", key, exception.Data[key]));
            }

            return exData.ToString();
        }

        private static string GetOtherExceptionProperties(Exception exception, string s)
        {
            var allOtherProps = new StringBuilder();
            var exPropList = exception.GetType().GetProperties();

            var propertiesAlreadyHandled = new List<string> { "StackTrace", "Message", "InnerException", "Data", "HelpLink", "Source", "TargetSite" };

            foreach (var prop in exPropList.Where(prop => !propertiesAlreadyHandled.Contains(prop.Name)))
            {
                var propObject = exception.GetType().GetProperty(prop.Name).GetValue(exception, null);
                var propEnumerable = propObject as IEnumerable;

                if (propEnumerable == null || propObject is string)
                    allOtherProps.Append(s + String.Format("{0} : {1}", prop.Name, propObject));
                else
                {
                    var enumerableSb = new StringBuilder();
                    foreach (var item in propEnumerable)
                    {
                        enumerableSb.Append(item + "|");
                    }
                    allOtherProps.Append(s + String.Format("{0} : {1}", prop.Name, enumerableSb));
                }
            }

            return allOtherProps.ToString();
        }

        private static Dictionary<string, object> GetExtendedInfoProperties(IEnumerable<KeyValuePair<string, object>> additionalProperties)
        {
            var extInfo = additionalProperties.ToDictionary(item => item.Key, item => item.Value);

            var moreInfo = new UnmanagedSecurityContextInformationProvider();

            if (!extInfo.ContainsKey("CurrentUser"))
                extInfo.Add("CurrentUser", moreInfo.CurrentUser);

            if (!extInfo.ContainsKey("ProcessAccountName"))
                extInfo.Add("ProcessAccountName", moreInfo.ProcessAccountName);

            return extInfo;
        }
    }
}
