using System;

namespace TeamCoding.Logging
{
    public interface ILogger
    {
        void WriteError(string error);
        void WriteError(Exception ex);
        void WriteInformation(string info);
    }
}