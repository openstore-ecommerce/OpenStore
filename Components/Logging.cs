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

    public static class Logger
    {
        private static ILog GetLogger(string className)
        {
            return LoggerSource.Instance.GetLogger(className);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="frameIndex">frameIndex: 0 will be this method itself. 1 will be its caller, 2 will be the caller's caller, etc.</param>
        /// <returns></returns>
        private static string GetCallingClassName(int frameIndex = 2)
        {
            StackTrace st = new StackTrace();
            var method = st.GetFrame(frameIndex).GetMethod();
            var declaringType = method?.DeclaringType?.ToString();
            return declaringType;
        }

        public static void Debug(string logMessage) => GetLogger(GetCallingClassName()).Debug(logMessage);
        public static void Info(string logMessage) => GetLogger(GetCallingClassName()).Info(logMessage);
        public static void Error(string logMessage) => GetLogger(GetCallingClassName()).Error(logMessage);
        public static void Error(Exception exception) => GetLogger(GetCallingClassName()).Error($"Exception: {exception.Message}\r\nStackTrace: {exception.StackTrace}");
        public static void Error(string logMessage, Exception exception) => GetLogger(GetCallingClassName()).Error($"{logMessage} Exception: {exception.Message}\r\nStackTrace: {exception.StackTrace}");
        public static void Fatal(string logMessage) => GetLogger(GetCallingClassName()).Fatal(logMessage);
        public static void Warn(string logMessage) => GetLogger(GetCallingClassName()).Warn(logMessage);
    }


    public class LogUtils
    {

        //  --------------------- Debug Log files ------------------------------
        /// <summary>
        /// Write a data file, to the Portal \DNNrocketTemp\debug folder.
        /// </summary>
        /// <param name="outFileName">Name of file</param>
        /// <param name="content">content of file</param>
        public static void OutputDebugFile(string outFileName, string content)
        {
            var mappath = StoreSettings.Current.FolderTempMapPath.TrimEnd(Convert.ToChar("\\")) + "\\Logs";
            if (!Directory.Exists(mappath)) Directory.CreateDirectory(mappath);
            Utils.SaveFile(mappath + "\\" + outFileName, content);
        }
        //  --------------------- Private utils ------------------------------
        public static void LogOnDebugMode(string message)
        {
            if (StoreSettings.Current.DebugMode)
            {
                var mappath = StoreSettings.Current.FolderTempMapPath.TrimEnd(Convert.ToChar("\\")) + "\\Logs";
                if (!Directory.Exists(mappath)) Directory.CreateDirectory(mappath);
                AppendToLog(mappath, "openstore_debug", message);
            }
        }
        public static void Log(string message)
        {
            var mappath = StoreSettings.Current.FolderTempMapPath.TrimEnd(Convert.ToChar("\\")) + "\\Logs";
            if (!Directory.Exists(mappath)) Directory.CreateDirectory(mappath);
            AppendToLog(mappath, "openstore", message);
        }
        public static void LogClear(int portalId, int daysToKeep = 0)
        {
            var mappath = StoreSettings.Current.FolderTempMapPath.TrimEnd(Convert.ToChar("\\")) + "\\Logs";
            if (!Directory.Exists(mappath)) Directory.CreateDirectory(mappath);
            DirectoryInfo di = new DirectoryInfo(mappath);
            foreach (FileInfo file in di.GetFiles())
            {
                if (file.CreationTime < DateTime.Now.AddDays(daysToKeep)) file.Delete();
            }
        }
        public static void AppendToLog(string logMapPathFolder, string logName, string logMessage)
        {
            var dstring = DateTime.Now.ToString("yyyy-MM-dd");
            using (StreamWriter w = File.AppendText(logMapPathFolder.TrimEnd('\\') + "\\" + dstring + "_" + Path.GetFileNameWithoutExtension(logName) + ".txt"))
            {
                Log(DateTime.Now.ToString("d/MM/yyyy HH:mm:ss") + " :  " + logMessage, w);
            }
        }
        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine($"{logMessage}");
        }


    }


}
