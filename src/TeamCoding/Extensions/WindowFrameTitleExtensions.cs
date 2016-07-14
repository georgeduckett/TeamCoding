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
        public static string GetUpdatedTooltip(this WindowFrameTitle windowFrameTitle)
        {
            windowFrameTitle.BindToolTip();
            return windowFrameTitle.ToolTip;
        }
    }
}
