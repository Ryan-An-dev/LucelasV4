using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogWriter
{
    public enum LogType
    {
        Page,
    }
    public static class ErpLogWriter
    {
        private static Logger logWriter = null;
        public static Logger LogWriter
        {
            get { return logWriter; }
        }

        public static void CreateLogger(LogType type)
        {
            switch (type)
            {
                case LogType.Page:
                    logWriter = LogManager.GetLogger("Page");
                    break;
             
                default:
                    break;
            }
        }
        public static void DisposeLogger()
        {
            logWriter = null;
        }
    }
}
