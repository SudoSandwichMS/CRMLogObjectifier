using System;
using System.Text;
using System.Text.RegularExpressions;

namespace CRMLogObjectifier.LogParser
{
    /// <summary>
    /// Default Micro-Parsers for CRM log entries
    /// </summary>
    public class LogParserBase
    {

        #region Helper Properties
        private static string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private static Regex digitsOnly = new Regex(@"[^\d]");
        #endregion

        #region Micro-Parsers

        /// <summary>
        /// Parse Category from log entry header line
        /// </summary>
        /// <param name="line"> Line of text to be parsed </param>
        /// <returns> Parsed Category string </returns>
        public string parseCategory(string line)
        {
            string result = string.Empty;

            // String begining with Category: up to next space or '|'
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

        /// <summary>
        /// Parse log level from log entry header line
        /// </summary>
        /// <param name="line"> Line of text to be parsed </param>
        /// <returns> Parsed log level as LogLevel object <see cref="LogLevel.ToString"/> and <see cref="LogLevel.value"/> </returns>
        public LogLevelEnum parseLogLevel(string line)
        {
            LogLevelEnum result = LogLevelEnum.None;

            // String begining with Level: up to next space or '|'
            Regex regex = new Regex(@"Level:\s*[^ |]+");
            Match match = regex.Match(line);

            if (match.Success)
            {
                string level = match.Value.Split(':')[1].Trim();

                try
                {
                    result = (LogLevelEnum)Enum.Parse(typeof(LogLevelEnum), level);
                }
                catch (ArgumentException)
                {
                    // parsed string is not a member of LogLevelEnum
                }
            }

            return result;
        }

        public string parseOperation(string line)
        {
            string result = string.Empty;

            // Last occurence of '|' including any characters following up to the next white space
            Regex regex = new Regex(@"\|(?!.*\|)\s+[^ ]*");
            Match match = regex.Match(line);

            if (match.Success)
            {
                // Split on | and grab the second element in array
                result = match.Value.Split('|')[1].Trim();
            }

            return result;
        }

        /// <see cref="parseGuid"/>
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

        /// <see cref="parseGuid"/>
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

            // Strings that do not begin with '>'
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
             * Sometimes thread is seen as 7(text.here)
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
        /// Currently no special handling of trace message
        /// </summary>
        /// <param name="line"> Current line of text to be parsed </param>
        /// <returns> trace message string </returns>
        public StringBuilder parseTraceMessage(string line)
        {
            return new StringBuilder(line);
        }


        /// <see cref="parseGuid"/>
        public Guid? parseUserID(string line)
        {
            return parseGuid(line, "User:");
        }
#endregion
    }
}
