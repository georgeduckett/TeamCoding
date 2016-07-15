using Microsoft.VisualStudio.Platform.WindowManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class WindowFrameTitleExtensions
    {
        public static string GetRelatedFilePath(this WindowFrameTitle windowFrameTitle)
        {
            // TODO: Is there a better way to get the tab's full file path than parsing the tooltip? (there must be!)
            windowFrameTitle.BindToolTip();
            return windowFrameTitle.ToolTip.TrimStart('*').GetCorrectCaseOfParentFolder();
        }
    }
}
