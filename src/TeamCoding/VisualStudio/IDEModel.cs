using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Extensions;
namespace TeamCoding.VisualStudio
{
    /// <summary>
    /// Represents and maintains a model of the IDE
    /// </summary>
    public class IDEModel
    {
        private static IDEModel _Current = new IDEModel();

        private readonly ConcurrentDictionary<IWpfTextView, string> _OpenFiles = new ConcurrentDictionary<IWpfTextView, string>();

        public void OpenedTextView(IWpfTextView view)
        {
            if (!_OpenFiles.ContainsKey(view))
            {
                // TODO: Use https://msdn.microsoft.com/en-us/library/envdte.sourcecontrol.aspx to check if it's in source control
                _OpenFiles.AddOrUpdate(view, new SourceControl.SourceControlRepo().GetRelativePath(view.GetTextDocumentFilePath()), (v, e) => e);
            }
        }

        public void ClosedTextView(IWpfTextView view)
        {
            string tmp;
            _OpenFiles.TryRemove(view, out tmp);
        }

        public string[] OpenDocs()
        {
            return _OpenFiles.Values.ToArray();
        }
    }
}
