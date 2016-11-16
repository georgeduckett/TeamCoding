using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.Alm.Roslyn.Client.Features.WorkspaceUpdateManager
{
    public class WorkspaceChangeInfo { }
    public class WorkspaceChangesEventArgs : EventArgs
    {
        public WorkspaceChangesEventArgs(IEnumerable<WorkspaceChangeInfo> changes)
        {
            Changes = changes;
        }

        public IEnumerable<WorkspaceChangeInfo> Changes { get; }
    }
    public interface IWorkspaceUpdateManager
    {
        Task AddWorkspaceChangedAsync(EventHandler<WorkspaceChangesEventArgs> handler);
        Task RemoveWorkspaceChangedAsync(EventHandler<WorkspaceChangesEventArgs> handler);
    }
}