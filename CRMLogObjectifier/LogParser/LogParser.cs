using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CRMLogObjectifier.LogParser
{
    public class LogParser : LogParserBase, ILogParser
    {
        #region Class Properties
        private string[] _logLines;

        private List<LogEntry> _logList;
        public List<LogEntry> LogList
        {
            get
            {
                return this._logList;
            }
        }
        #endregion

        public LogParser(string filepath)
        {
            #region Initalize
            // Read each line of the file into a string array. Each element
            // of the array is one line of the file.
            _logLines = System.IO.File.ReadAllLines(filepath);
            _logList = new List<LogEntry>();

            LogEntry entry = null;

            int entryCount = 0;
            #endregion

            #region Parse Controller
            for (int i = 0; i < _logLines.Length; i++)
            {
                ignoreHeader(_logLines[i]);

                DateTime? timeStamp = parseTimeStamp(_logLines[i]);
                /*
                 * Parse log opening line only if timestamp is found.
                 * Otherwise, we are looking at the stack trace & trace msg
                 */
                if (timeStamp != null)
                {
                    #region Add log entry to list
                    // Add to list everytime we see a new entry except first
                    if (entry != null)
                    {
                        LogList.Add(entry);
                        entryCount++;
                    }
                    #endregion

                    entry = new LogEntry();
                    entry.timeStamp = (DateTime)timeStamp;

                    // Parse new log first line
                    entry.process = parseProcess(_logLines[i]);
                    entry.organizationID = parseOrganizationID(_logLines[i]);
                    entry.thread = parseThread(_logLines[i]);
                    entry.category = parseCategory(_logLines[i]);
                    entry.userID = parseUserID(_logLines[i]);
                    entry.logLevel = parseLogLevel(_logLines[i]);
                    entry.ReqId = parseReqID(_logLines[i]);
                    entry.operation = parseOperation(_logLines[i]);
                }
                else if (entry != null)
                {
                    // Stack trace & Trace message

                    /*
                     * Checking for space in start of string implies stack trace
                     * Checking for > at the start of the string fails in certain cases.
                     */
                    Regex regex = new Regex(@"^\t+at");
                    Match match = regex.Match(_logLines[i]);
                    Boolean isEndStack = false;
                    while (!isEndStack)
                    {
                        if (match.Success)
                        {
                            entry.stackTrace.Append(parseStackTrace(_logLines[i]));
                            i++;
                            match = regex.Match(_logLines[i]);
                        }
                        else
                        {
                            isEndStack = true;
                        }
                    }

                    entry.traceMessage.Append(parseTraceMessage(_logLines[i]));



                }

            } // End ForEach
            #endregion

            // Add last entry
            if (entry != null)
                LogList.Add(entry);

        }

        #region Micro-Parsers
        /// <summary>
        /// Ignore headers in CRM log file
        /// </summary>
        /// <param name="line"> Possible file header string </param>
        /// <returns> True is header line has been ignored </returns>
        public Boolean ignoreHeader(string line)
        {
            Boolean result = false;

            // Match lines begining with # and save following text
            Regex regex = new Regex(@"^#.*");
            Match match = regex.Match(line);

            if (match.Success)
            {
                result = true;
            }

            return result;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Output all lines if input is -1. Otherwise, output specific entry
        /// 
        /// Debugging purposes
        /// </summary>
        /// <param name="i"> Element to print; -1 for all </param>
        public void outputLogs(int i)
        {
            if (i >= 0)
            {
                Console.WriteLine(_logList.ToArray()[i].ToString());
            }
            else
            {
                foreach (LogEntry entry in _logList)
                {
                    Console.WriteLine(entry.ToString());
                }
            }
        }
        #endregion
    }
}
