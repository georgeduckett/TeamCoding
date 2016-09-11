using Microsoft.VisualStudio.Alm.Roslyn.Client.Features.WorkspaceUpdateManager;
using Microsoft.VisualStudio.CodeSense;
using Microsoft.VisualStudio.CodeSense.Roslyn;
using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.CodeLens
{
    public class CurrentUsersDataPointV15 : DataPoint<string>
    {
        private readonly IWorkspaceUpdateManager WorkspaceUpdateManager;
        private readonly Task WorkspaceChangedTask;
        private readonly CurrentUsersDataPointV15Updater DataPointUpdater;
        private bool Disposed;
        private object DisposedLock = new object();
        public readonly ICodeElementDescriptor CodeElementDescriptor;
        public CurrentUsersDataPointV15(CurrentUsersDataPointV15Updater dataPointUpdater, IWorkspaceUpdateManager workspaceUpdateManager, ICodeElementDescriptor codeElementDescriptor)
        {
            DataPointUpdater = dataPointUpdater;
            CodeElementDescriptor = codeElementDescriptor;
            WorkspaceUpdateManager = workspaceUpdateManager;
            WorkspaceChangedTask = WorkspaceUpdateManager.AddWorkspaceChangedAsync(OnWorkspaceChanged);
        }
        public override Task<string> GetDataAsync()
        {
            return DataPointUpdater.GetTextForDataPoint(CodeElementDescriptor);
        }
        private void OnWorkspaceChanged(object sender, WorkspaceChangesEventArgs e)
        {
            Task.Run(() =>
            {
                object obj = DisposedLock;
                lock (obj)
                {
                    if (!Disposed)
                    {
                        System.Threading.Thread.Sleep(2000);
                        Invalidate();
                    }
                }
            }).FireAndForget();
        }
        protected override void Dispose(bool disposing)
        {
            object obj = DisposedLock;
            if (!Disposed)
            {
                lock (obj)
                {
                    if (!Disposed)
                    {
                        if (disposing)
                        {
                            WorkspaceUpdateManager.RemoveWorkspaceChangedAsync(new EventHandler<WorkspaceChangesEventArgs>(OnWorkspaceChanged)).FireAndForget();
                        }
                        Disposed = true;
                    }
                }
            }
            base.Dispose(disposing);
        }
    }

}