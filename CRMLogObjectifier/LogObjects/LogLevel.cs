using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMLogObjectifier.LogParser
{

    public enum LogLevelEnum { Verbose, Info, Warning, Error, None };

    /// <summary>
    /// Allow log level to be parsed as a string or enum. This is done to make handling log level easier on the backend avoding string comparisons.
    /// </summary>
    public class LogLevel
    {
        private LogLevelEnum _logLevel;

        public LogLevel()
        {
            _logLevel = LogLevelEnum.None;
        }

        /// <returns> Log level Enum value </returns>
        public LogLevelEnum value()
        {
            return _logLevel;
        }

        /// <summary>
        /// Convert Enum to equivalent string
        /// </summary>
        /// <returns> String equivilant of Enum value </returns>
        public override string ToString()
        {
            switch (_logLevel)
            {
                case LogLevelEnum.Verbose:
                    return "Verbose";
                    
                case LogLevelEnum.Info:
                    return "Info";

                case LogLevelEnum.Warning:
                    return "Warning";

                case LogLevelEnum.Error:
                    return "Error";
                    
                default:
                    return "None";
            }
        }

        /// <summary>
        /// Directly set Enum value for log level
        /// </summary>
        /// <param name="level"> Logging level Enum </param>
        public void set(LogLevelEnum level)
        {
            _logLevel = level;
        }

        /// <summary>
        /// Set log level using string value
        /// </summary>
        /// <param name="level"> Logging level as a string </param>
        /// <remarks> Accepts: Verbose, Info, Warning and Error </remarks>
        public void set(string level)
        {
            switch (level)
            {
                case "Verbose":
                    _logLevel = LogLevelEnum.Verbose;
                    break;
                case "Info":
                    _logLevel = LogLevelEnum.Info;
                    break;
                case "Warning":
                    _logLevel = LogLevelEnum.Warning;
                    break;
                case "Error":
                    _logLevel = LogLevelEnum.Error;
                    break;
                default:
                    break;
            }
        }
    }
}
