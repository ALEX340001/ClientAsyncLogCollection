using System;

namespace MyCompany.Logging
{
    public interface ILogger
    {
        void Debug(string message, string category = null);
        void Info(string message, string category = null);
        void Warn(string message, string category = null);
        void Error(string message, string category = null);
        void Error(Exception ex, string message, string category = null);
    }
}