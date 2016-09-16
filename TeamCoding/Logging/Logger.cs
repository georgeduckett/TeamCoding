using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Logging
{
    public class Logger : ILogger
    {
        public const string OutputWindowCategory = "Team Coding";
        private IVsOutputWindowPane TeamCodingPane;
        private EnvDTE.OutputWindow OutputWindow;
        public Logger()
        {
            var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            OutputWindow = (EnvDTE.OutputWindow)dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput).Object;
        }
        private void EnsureOutputPane()
        {
            if (TeamCodingPane == null)
            {
                var OldPane = OutputWindow.ActivePane;
                var outputWindow = (IVsOutputWindow)Package.GetGlobalService(typeof(SVsOutputWindow));
                var outputWindowCategoryGuid = new Guid(Guids.OutputWindowCategoryGuidString);
                outputWindow.CreatePane(ref outputWindowCategoryGuid, OutputWindowCategory, 0, 0);
                outputWindow.GetPane(ref outputWindowCategoryGuid, out TeamCodingPane);
                TeamCodingPane.Activate(); // Activate it so it's visible
                try
                {
                    OldPane?.Activate(); // Then activate the old one again
                }
                catch(Exception ex)
                {
                    WriteError(ex);
                }
            }
        }
        public void WriteInformation(string info, [CallerFilePath] string filePath = null, [CallerMemberName] string memberName = null)
        {
            LogText(info, filePath, memberName);
        }
        private void LogText(string text, string filePath, string methodName)
        {
            EnsureOutputPane();
            ActivityLog.TryLogInformation(OutputWindowCategory, $"{Path.GetFileNameWithoutExtension(filePath)}.{methodName}:{text}");
            TeamCodingPane.OutputStringThreadSafe($"{DateTime.Now} {Path.GetFileNameWithoutExtension(filePath)}.{methodName}:{text}{Environment.NewLine}");
        }
        public void WriteError(string error, Exception ex = null, [CallerFilePath] string filePath = null, [CallerMemberName] string memberName = null)
        {
            if(ex != null)
            {
                WriteError(ex, filePath, memberName);
            }
            LogText(error, filePath, memberName);
            TeamCodingPane.Activate();
        }
        public void WriteError(Exception ex, [CallerFilePath] string filePath = null, [CallerMemberName] string memberName = null) => WriteError(ex.ToString(), null, filePath, memberName);
    }
}
