using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using NBrightCore.common;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class Logging
    {
        private static ILog Logger
        {
            get
            {
                if (StoreSettings.Current == null)
                {
                    return LoggerSource.Instance.GetLogger($"NBrightBuy.Host");
                }
                else if (StoreSettings.Current.EnableFileLogging)
                {
                    return LoggerSource.Instance.GetLogger($"NBrightBuy.Portal-{StoreSettings.Current.SettingsInfo.PortalId}");
                }
                return null;
            }
        }

        public static void LogException(Exception exc)
        {
            // put it in the dnn exceptionlog
            DotNetNuke.Services.Exceptions.Exceptions.LogException(exc);
            // enter a log entry with some debugging info
            StackTrace st = new StackTrace();
            var msg = $"Exception in {st.GetFrame(2).GetMethod().Name}: {exc.Message}. StackTrace: {exc.StackTrace}";
            Logger?.Error(msg);
        }

        public static void Debug(String message)
        {
            Logger?.Debug(message);
        }
        public static void Error(String message)
        {
            Logger?.Error(message);
        }
        public static void Fatal(String message)
        {
            Logger?.Fatal(message);
        }
        public static void Info(String message)
        {
            Logger?.Info(message);
        }
        public static void Warn(String message)
        {
            Logger?.Warn(message);
        }
    }



}
