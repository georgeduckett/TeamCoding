using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using TeamCoding.Extensions;
using TeamCoding.IdentityManagement;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using System.Collections.Generic;
using TeamCoding.Interfaces.Documents;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.TextAdornment
{
    internal sealed class TextAdornment : IDisposable
    {
        private readonly IAdornmentLayer Layer;
        private readonly IWpfTextView View;
        private readonly Pen CaretPen;

        private readonly DocumentRepoMetaData RepoDocument;
        public TextAdornment(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            Layer = view.GetAdornmentLayer("TextAdornment");

            View = view;
            RepoDocument = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(View.TextBuffer.GetTextDocumentFilePath());

            TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived += RemoteModelChangeManager_RemoteModelReceived;
            View.LayoutChanged += OnLayoutChanged;

            var penBrush = new SolidColorBrush(Colors.Red);
            penBrush.Freeze();
            CaretPen = new Pen(penBrush, 1);
            CaretPen.Freeze();
        }

        private async void RemoteModelChangeManager_RemoteModelReceived(object sender, EventArgs e)
        {
            var CaretPositions = TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles()
                                                          .Where(of => of.Repository == RepoDocument.RepoUrl && 
                                                                       of.RelativePath == RepoDocument.RelativePath &&
                                                                       of.RepositoryBranch == RepoDocument.RepoBranch &&
                                                                       of.CaretPositionInfo != null)
                                                          .Select(of => new
                                                          {
                                                              CaretMemberHashCodes = of.CaretPositionInfo.SyntaxNodeIds,
                                                              of.CaretPositionInfo.LeafMemberCaretOffset,
                                                              of.IdeUserIdentity
                                                          }).ToArray();

            if(CaretPositions.Length == 0)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                Layer.RemoveAllAdornments();
                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Layer.RemoveAllAdornments();

            foreach(var caret in CaretPositions)
            {
                var nodes = await TeamCodingPackage.Current.CaretAdornmentDataProvider.GetCaretAdornmentData(View.TextSnapshot, caret.CaretMemberHashCodes);

                foreach(var node in nodes)
                {
                    // We got to the end, matching all nodes all the way down
                    CreateVisual(node, caret.LeafMemberCaretOffset, caret.IdeUserIdentity);
                }
            }
        }

        /// <summary>
        /// Handles whenever the text displayed in the view changes by adding the adornment to any reformatted lines
        /// </summary>
        /// <remarks><para>This event is raised whenever the rendered text displayed in the <see cref="ITextView"/> changes.</para>
        /// <para>It is raised whenever the view does a layout (which happens when DisplayTextLineContainingBufferPosition is called or in response to text or classification changes).</para>
        /// <para>It is also raised whenever the view scrolls horizontally or when its size changes.</para>
        /// </remarks>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            RemoteModelChangeManager_RemoteModelReceived(sender, EventArgs.Empty);
        }

        private void CreateVisual(CaretAdornmentData nodeData, int caretOffset, IUserIdentity userIdentity)
        {
            if (nodeData.SpanStart + caretOffset > View.TextSnapshot.Length)
            {
                return;
            }
            var remoteCaretSpan = new SnapshotSpan(View.TextSnapshot, Math.Min(nodeData.SpanStart + caretOffset, nodeData.SpanEnd), 1);
            Geometry characterGeometry = View.TextViewLines.GetMarkerGeometry(remoteCaretSpan);
            if (characterGeometry != null)
            {
                var caretGeometry = new LineGeometry(characterGeometry.Bounds.TopLeft, characterGeometry.Bounds.BottomLeft);
                var drawing = new GeometryDrawing(null, UserColours.GetUserPen(userIdentity), caretGeometry);
                drawing.Freeze();

                var drawingImage = new DrawingImage(drawing);
                drawingImage.Freeze();

                var image = new Image
                {
                    Source = drawingImage,
                };

                // Align the image with the top of the bounds of the text geometry
                Canvas.SetLeft(image, caretGeometry.Bounds.Left);
                Canvas.SetTop(image, caretGeometry.Bounds.Top);

                Layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, remoteCaretSpan, null, image, null);

                FrameworkElement userControl = TeamCodingPackage.Current.UserImages.CreateUserIdentityControl(userIdentity, true);
                userControl.Width = caretGeometry.Bounds.Height / 1.25f;
                userControl.Height = caretGeometry.Bounds.Height / 1.25f;
                Canvas.SetLeft(userControl, caretGeometry.Bounds.Left - userControl.Width / 2);
                Canvas.SetTop(userControl, caretGeometry.Bounds.Top - userControl.Height);
                Layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, remoteCaretSpan, null, userControl, null);
            }
        }

        public void Dispose()
        {
            TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived -= RemoteModelChangeManager_RemoteModelReceived;
        }
    }
}
