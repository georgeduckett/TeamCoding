using Microsoft.VisualStudio.Alm.Roslyn.Client.Features.WorkspaceUpdateManager;
using Microsoft.VisualStudio.CodeSense.Roslyn;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.CodeLens
{
    [Export(typeof(ICodeLensDataPointProvider)), Name("Team Coding")]
    public class CurrentUsersDataPointProvider : ICodeLensDataPointProvider
    {
        [Import]
        private readonly CurrentUsersDataPointUpdater DataPointUpdater = null;
        [Import]
        private readonly IWorkspaceUpdateManager WorkspaceUpdateManager = null;
        public bool CanCreateDataPoint(ICodeLensDescriptor descriptor)
        {
            return descriptor is ICodeElementDescriptor;
        }
        public ICodeLensDataPoint CreateDataPoint(ICodeLensDescriptor codeLensDescriptor)
        {
            return new CurrentUsersDataPoint(DataPointUpdater, WorkspaceUpdateManager, (ICodeElementDescriptor)codeLensDescriptor);
        }
    }
}
