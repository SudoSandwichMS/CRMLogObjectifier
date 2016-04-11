using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMLogObjectifier.LogParser
{

    public class LogEntry
    {

        public LogEntry()
        {
            stackTrace = new StringBuilder();
            traceMessage = new StringBuilder();
        }

        #region objectProperties
        public DateTime timeStamp { get; set; }

        public string process { get; set; }

        public Guid? organizationID { get; set; }

        public string organizationName { get; set; }

        public int thread { get; set; }

        public string category { get; set; }

        public Guid? userID { get; set; }

        public string userName { get; set; }
  
        public LogLevel logLevel { get; set;}

        public Guid? ReqId { get; set; }

        public string operation { get; set; }

        public StringBuilder stackTrace { get; set; }

        public StringBuilder traceMessage { get; set; }
        #endregion

        /// <summary>
        /// Pretty print log entry
        /// </summary>
        /// <returns> Formatted log entry for printing </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

            sb.Append("Timestamp:\t" + timeStamp.ToString(_dateTimeFormat) + "\n");
            sb.Append("Process:\t" + process + "\n");
            sb.Append("ORG Guid:\t" + organizationID + "\n");
            sb.Append("Thread:\t\t" + thread + "\n");
            sb.Append("Category:\t" + category + "\n");
            sb.Append("User Guid:\t" + userID + "\n");
            sb.Append("Log Level:\t" + logLevel.ToString() + "\n");
            sb.Append("ReqID:\t\t" + ReqId + "\n");
            sb.Append("Operation:\t" + operation + "\n");
            sb.Append(stackTrace);
            sb.Append(traceMessage);
            sb.Append(" END OF ENTRY \n\n");

            return sb.ToString();
        }
    }
}
