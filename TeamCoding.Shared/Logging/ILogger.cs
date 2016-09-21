using System;
using System.Runtime.CompilerServices;

namespace TeamCoding.Logging
{
    public interface ILogger
    {
        void WriteError(string error, Exception ex = null, [CallerMemberName] string filePath = null, [CallerMemberName] string memberName = null);
        void WriteError(Exception ex, [CallerMemberName] string filePath = null, [CallerMemberName] string memberName = null);
        void WriteInformation(string info, [CallerMemberName] string filePath = null, [CallerMemberName] string memberName = null);
    }
}