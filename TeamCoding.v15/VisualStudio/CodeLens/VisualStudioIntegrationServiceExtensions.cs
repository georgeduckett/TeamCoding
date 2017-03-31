using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Alm.Roslyn.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.CodeLens
{
    public static class VisualStudioIntegrationServiceExtensions
    {
        public static Task AddWorkspaceChangedAsync(this IVisualStudioIntegrationService service, EventHandler<WorkspaceChangeEventArgs> eventHandler)
        {
            return Task.Run(() => service.AddWorkspaceChanged(eventHandler));
        }
        public static Task RemoveWorkspaceChangedAsync(this IVisualStudioIntegrationService service, EventHandler<WorkspaceChangeEventArgs> eventHandler)
        {
            return Task.Run(() => service.RemoveWorkspaceChanged(eventHandler));
        }
    }
}
