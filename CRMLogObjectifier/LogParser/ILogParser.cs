using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMLogObjectifier.LogParser
{
    public interface ILogParser    
    {

        List<LogEntry> LogList
        {
            get;
        }

        Boolean ignoreHeader(string line);
        DateTime? parseTimeStamp(string line);
        string parseProcess(string line);
        Guid? parseOrganizationID(string line);
        int parseThread(string line);
        string parseCategory(string line);
        Guid? parseUserID(string line);
        LogLevel parseLogLevel(string line);
        Guid? parseReqID(string line);
        string parseOperation(string line);
        StringBuilder parseStackTrace(string line);
        StringBuilder parseTraceMessage(string line);

    }
}
