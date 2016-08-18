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
    public class CurrentUsersDataPoint : DataPoint<string>
    {
        private readonly IWorkspaceUpdateManager WorkspaceUpdateManager;
        private readonly Task WorkspaceChangedTask;
        private readonly ICodeElementDescriptor CodeElementDescriptor;
        private bool Disposed;
        private object DisposedLock = new object();
        public CurrentUsersDataPoint(IWorkspaceUpdateManager workspaceUpdateManager, ICodeElementDescriptor codeElementDescriptor)
        {
            CodeElementDescriptor = codeElementDescriptor;
            WorkspaceUpdateManager = workspaceUpdateManager;
            WorkspaceChangedTask = WorkspaceUpdateManager.AddWorkspaceChangedAsync(OnWorkspaceChanged);
        }

        public override Task<string> GetDataAsync()
        {
            var MatchingUsers = TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles()
                                                      .Where(of => of.CaretMemberHashCode == CodeElementDescriptor.SyntaxNode.GetTreePositionHashCode())
                                                      .GroupBy(of => of.IdeUserIdentity.DisplayName)
                                                      .Select(g => g.Key).ToArray();

            if(MatchingUsers.Length == 0)
            {
                return Task.FromResult<string>(null);
            }
            return Task.FromResult("Current coders: " + string.Join(", ", MatchingUsers));
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