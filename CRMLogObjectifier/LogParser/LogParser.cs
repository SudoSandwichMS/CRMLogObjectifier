using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CRMLogObjectifier.LogParser
{
    public class LogParser : ILogParser
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

        #region Helper Properties
        private static string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private static Regex digitsOnly = new Regex(@"[^\d]");
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
                    Regex regex = new Regex(@"^\s+");
                    Match match = regex.Match(_logLines[i]);
                    Boolean isEndStack = false;
                    while (!isEndStack)
                    {
                        entry.stackTrace.Append(parseStackTrace(_logLines[i]));

                        if (match.Success)
                        {
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

        #region Micro Parsers
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

        public string parseCategory(string line)
        {
            string result = string.Empty;

            Regex regex = new Regex(@"Category:\s*[^ |]+");
            Match match = regex.Match(line);
            if (match.Success)
            {
                // Split on : and grab the second element in array
                result = match.Value.Split(':')[1].Trim();
            }

            return result;
        }

        /// <summary>
        /// Generic Guid parse method. Parse any guid following a given string.
        /// </summary>
        /// <param name="line"> line of text containing Guid(s) </param>
        /// <param name="key"> String found before guid </param>
        /// <returns></returns>
        public Guid? parseGuid(string line, string key)
        {
            Guid? result = null;
            /*
             * Match a Guid following a given string regardless of spacing between key and Guid. Will match the following Guid types
             * ca761232ed4211cebacd00aa0057b223
             * CA761232-ED42-11CE-BACD-00AA0057B223
             * {CA761232-ED42-11CE-BACD-00AA0057B223} 
             * (CA761232-ED42-11CE-BACD-00AA0057B223) 
             */
            Regex regex = new Regex(key + @"(\s+|)[{(]?[0-9A-Fa-f]{8}[-]?([0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?");
            Match match = regex.Match(line);

            if (match.Success)
            {
                // Split on : and grab the second element in array
                result = new Guid(match.Value.Split(':')[1].Trim());
            }

            return result;
        }

        public LogLevel parseLogLevel(string line)
        {
            LogLevel result = new LogLevel();

            Regex regex = new Regex(@"Level:\s*[^ |]+");
            Match match = regex.Match(line);

            if (match.Success)
            {
                string level = match.Value.Split(':')[1].Trim();
                result.set(level);
            }

            return result;
        }

        public string parseOperation(string line)
        {
            string result = string.Empty;

            Regex regex = new Regex(@"\|(?!.*\|)\s+[^ ]*");
            Match match = regex.Match(line);

            if (match.Success)
            {
                // Split on | and grab the second element in array
                result = match.Value.Split('|')[1].Trim();
            }

            return result;
        }

        public Guid? parseOrganizationID(string line)
        {
            string key = "Organization:";
            return parseGuid(line, key);
        }

        public string parseProcess(string line)
        {
            string result = string.Empty;

            /*
             * Match on the word Process: and any characters following until ' ' or | 
             * **There is no garuntee that there will be a space after Process:
             * 
             * Usine negation [^ |]+ instead of .+? to avoid backtracking slowing down match
             */
            Regex regex = new Regex(@"Process:\s*[^ |]+");

            Match match = regex.Match(line);

            if (match.Success)
            {
                // Split on : and grab the second element in array
                result = match.Value.Split(':')[1].Trim();


            }
            return result;
        }

        public Guid? parseReqID(string line)
        {
            return parseGuid(line, "ReqId:");
        }

        /// <summary>
        /// Parse stacktrace up to trace message as Stringbuilder 
        /// </summary>
        /// <param name="line"> Current line of text to be parsed </param>
        /// <returns> Stacktrace line with newline character appended </returns>
        public StringBuilder parseStackTrace(string line)
        {


            StringBuilder result = new StringBuilder();

            Regex regex = new Regex(@"^(?!>).*$");
            Match match = regex.Match(line);

            if (match.Success)
            {
                result.Append(line + "\n");
            }
            return result;
        }


        /// <returns> Thread number </returns>
        public int parseThread(string line)
        {
            int result = -1;

            /*
             * Match Thread: plus any white space before digits 
             * Sometimes thread is seen as 7(MSCRM:-RegistryWatcher.RegTrackerThreadProc)
             * Using . to match any character not expected after thread number
             */
            Regex regex = new Regex(@"Thread:\s+\d+( |\||.)");
            Match match = regex.Match(line);
            if (match.Success)
            {
                // Sanitize thread number string and convert to int.
                if (!Int32.TryParse(digitsOnly.Replace(match.Value.Split(':')[1], ""), out result))
                {
                    // Failed to parse int
                }
            }
            return result;
        }

        /// <summary>
        /// Parse timestamp as nullable DateTime
        /// </summary>
        /// <param name="line"> Current line of text to be parsed </param>
        /// <returns> parsed timestamp or null if no timestamp found </returns>
        public DateTime? parseTimeStamp(string line)
        {
            DateTime timeStamp;

            // regex match timestamp format in file [yyyy-MM-dd HH:mm:ss.fff] 
            Regex regex = new Regex(@"^\[\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}.\d{3}\]");
            Match match = regex.Match(line);

            if (match.Success)
            {
                DateTime.TryParseExact(match.Value.TrimEnd(']').TrimStart('['), _dateTimeFormat, null, System.Globalization.DateTimeStyles.None, out timeStamp);
                return timeStamp;
            }

            return null;

        }

        /// <summary>
        /// Parse trace message at the end of callstack into string
        /// </summary>
        /// <param name="line"> Current line of text to be parsed </param>
        /// <returns> trace message string </returns>
        public StringBuilder parseTraceMessage(string line)
        {
            return new StringBuilder(line);
        }

        public Guid? parseUserID(string line)
        {
            return parseGuid(line, "User:");
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
