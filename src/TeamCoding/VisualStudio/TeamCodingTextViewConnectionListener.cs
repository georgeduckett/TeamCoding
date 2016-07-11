using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using TeamCoding.Extensions;
using LibGit2Sharp;
using System.IO;
using TeamCoding.SourceControl;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Linq;
using Microsoft.VisualStudio.PlatformUI.Shell.Controls;
using System.Windows.Controls;
using Microsoft.VisualStudio.Platform.WindowManagement;

namespace TeamCoding.VisualStudio
{
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
    /// </summary>
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class TeamCodingTextViewConnectionListener : IWpfTextViewConnectionListener
    {
        public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            if (reason == ConnectionReason.TextViewLifetime)
            { // TextView opened
                TeamCodingPackage.Current.IdeModel.OpenedTextView(textView);
            }
        }
        public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            if (reason == ConnectionReason.TextViewLifetime)
            { // TextView closed
                TeamCodingPackage.Current.IdeModel.ClosedTextView(textView);
            }
        }
    }
}
