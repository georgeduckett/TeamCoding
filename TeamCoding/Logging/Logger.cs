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
        private Guid OutputWindowCategoryGuid = new Guid("3C4076F2-E4E4-4B36-9643-FF48585BD80C"); // TODO: Move all guids to the same place
        private const string OutputWindowCategory = "Team Coding Output";
        private IVsOutputWindowPane TeamCodingPane;
        private void EnsureOutputPane()
        {
            if (TeamCodingPane == null)
            {
                var outputWindow = (IVsOutputWindow)Package.GetGlobalService(typeof(SVsOutputWindow));
                outputWindow.CreatePane(ref OutputWindowCategoryGuid, OutputWindowCategory, 0, 0);
                outputWindow.GetPane(ref OutputWindowCategoryGuid, out TeamCodingPane);
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
