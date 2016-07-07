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
    public class IDEModel
    {
        static ConcurrentDictionary<IWpfTextView, string> _OpenFiles = new ConcurrentDictionary<IWpfTextView, string>();

        public static void OpenedTextView(IWpfTextView view)
        {
            if (!_OpenFiles.ContainsKey(view))
            {
                _OpenFiles.AddOrUpdate(view, new SourceControl.SourceControlRepo().GetRelativePath(view.GetTextDocumentFilePath()), (v, e) => e);
            }
        }

        public static void ClosedTextView(IWpfTextView view)
        {
            string tmp;
            _OpenFiles.TryRemove(view, out tmp);
        }

        public static string[] OpenDocs()
        {
            return _OpenFiles.Values.ToArray();
        }
    }
}
