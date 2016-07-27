using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Logging
{
    public class Logger
    {
        private const string OutputWindowCategory = "Team Coding Output";
        private IVsOutputWindowPane TeamCodingPane;
        private void EnsureOutputPane()
        {
            if (TeamCodingPane == null)
            {
                var outputWindow = (IVsOutputWindow)Package.GetGlobalService(typeof(SVsOutputWindow));
                var outputWindowCategoryGuid = new Guid(Guids.OutputWindowCategoryGuidString);
                outputWindow.CreatePane(ref outputWindowCategoryGuid, OutputWindowCategory, 0, 0);
                outputWindow.GetPane(ref outputWindowCategoryGuid, out TeamCodingPane);
            }
        }
        public void WriteInformation(string info)
        {
            EnsureOutputPane();
            ActivityLog.TryLogInformation(OutputWindowCategory, info);
            TeamCodingPane.OutputStringThreadSafe(info + Environment.NewLine);
        }
        public void WriteError(string error)
        {
            EnsureOutputPane();
            ActivityLog.TryLogError(OutputWindowCategory, error);
            TeamCodingPane.OutputStringThreadSafe(error + Environment.NewLine);
            TeamCodingPane.Activate();
        }
        public void WriteError(Exception ex) => WriteError(ex.ToString());
    }
}
