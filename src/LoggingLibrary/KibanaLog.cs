using Microsoft.Extensions.Logging;

namespace LoggingLibrary
{
    public class KibanaLog
    {
        internal string m_Context;
        internal string m_Message;
        internal string m_Details;
        internal double m_DurationInSeconds;
        internal int m_Count;
        internal LogLevel m_LogLevel;

        public KibanaLog(LogLevel i_LogLevel, string i_Context, string i_Message = "", string i_Details = "", int i_Count = 0, double i_DurationInSeconds = 0)
        {
            m_LogLevel = i_LogLevel;
            m_Context = i_Context;
            m_Message = i_Message;
            m_Details = i_Details;
            m_DurationInSeconds = i_DurationInSeconds;
            m_Count = i_Count;
        }
    }
}