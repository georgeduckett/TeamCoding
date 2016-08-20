using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using TeamCoding.Extensions;
using Microsoft.CodeAnalysis;
using TeamCoding.IdentityManagement;
using Microsoft.VisualStudio.Shell;

namespace TeamCoding.VisualStudio.TextAdornment
{
    /// <summary>
    /// TextAdornment places red boxes behind all the "a"s in the editor window
    /// </summary>
    internal sealed class TextAdornment : IDisposable
    {
        /// <summary>
        /// The layer of the adornment.
        /// </summary>
        private readonly IAdornmentLayer Layer;

        /// <summary>
        /// Text view where the adornment is created.
        /// </summary>
        private readonly IWpfTextView View;

        /// <summary>
        /// Adornment brush.
        /// </summary>
        private readonly Brush brush;

        private readonly string RelativePath;

        /// <summary>
        /// Adornment pen.
        /// </summary>
        private readonly Pen pen;
        public TextAdornment(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            Layer = view.GetAdornmentLayer("TextAdornment");

            View = view;
            RelativePath = TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(View.TextBuffer.GetTextDocumentFilePath()).RelativePath;
            TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived += RemoteModelChangeManager_RemoteModelReceived;
            View.LayoutChanged += OnLayoutChanged;

            // Create the pen and brush to color the box behind the a's
            brush = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x00, 0xff));
            brush.Freeze();

            var penBrush = new SolidColorBrush(Colors.Red);
            penBrush.Freeze();
            pen = new Pen(penBrush, 0.5);
            pen.Freeze();
        }

        private async void RemoteModelChangeManager_RemoteModelReceived(object sender, EventArgs e)
        {
            var CaretMemberHashCodeToDataPointString = TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles()
                                                                    .Where(of => of.RelativePath == RelativePath && of.CaretPositionInfo != null)
                                                                    .Select(of => new
                                                                    {
                                                                        CaretMemberHashCode = of.CaretPositionInfo.MemberHashCodes[0],
                                                                        of.CaretPositionInfo.LeafMemberCaretOffset,
                                                                        of.IdeUserIdentity
                                                                    })
                                                                    .GroupBy(of => of.CaretMemberHashCode)
                                                                    .ToDictionary(g => g.Key, g => g.Select(of => new { of.IdeUserIdentity, of.LeafMemberCaretOffset }).Distinct());

            if(CaretMemberHashCodeToDataPointString.Keys.Count == 0)
            {
                return;
            }

            var syntaxTree = await View.TextSnapshot.GetOpenDocumentInCurrentContextWithChanges().GetSyntaxTreeAsync();
            var rootNode = await syntaxTree.GetRootAsync();
            var nodes = rootNode.DescendantNodesAndSelf();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Layer.RemoveAllAdornments();
            foreach (var node in nodes)
            {
                var nodeTreeHashCode = node.GetTreePositionHashCode();
                if (CaretMemberHashCodeToDataPointString.ContainsKey(nodeTreeHashCode))
                {
                    foreach (var matchedRemoteCaret in CaretMemberHashCodeToDataPointString[nodeTreeHashCode])
                    {
                        CreateVisual(node, matchedRemoteCaret.LeafMemberCaretOffset, matchedRemoteCaret.IdeUserIdentity);
                    }
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
        internal async void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            var CaretMemberHashCodeToDataPointString = TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles()
                                                                    .Where(of => of.RelativePath == RelativePath && of.CaretPositionInfo != null)
                                                                    .Select(of => new
                                                                    {
                                                                        CaretMemberHashCode = of.CaretPositionInfo.MemberHashCodes[0],
                                                                        of.CaretPositionInfo.LeafMemberCaretOffset,
                                                                        of.IdeUserIdentity
                                                                    })
                                                                    .GroupBy(of => of.CaretMemberHashCode)
                                                                    .ToDictionary(g => g.Key, g => g.Select(of => new { of.IdeUserIdentity, of.LeafMemberCaretOffset }).Distinct());
            if (CaretMemberHashCodeToDataPointString.Keys.Count == 0)
            {
                return;
            }

            var syntaxTree = await e.NewSnapshot.GetOpenDocumentInCurrentContextWithChanges().GetSyntaxTreeAsync();
            var newOrChangedNodes =
                e.NewOrReformattedLines.SelectMany(line => syntaxTree.GetRoot().DescendantNodes(new TextSpan(line.Extent.Start, line.Extent.Length))).Distinct();

            foreach (var node in newOrChangedNodes)
            {
                var nodeTreeHashCode = node.GetTreePositionHashCode();
                if (CaretMemberHashCodeToDataPointString.ContainsKey(nodeTreeHashCode))
                {
                    foreach (var matchedRemoteCaret in CaretMemberHashCodeToDataPointString[nodeTreeHashCode])
                    {
                        CreateVisual(node, matchedRemoteCaret.LeafMemberCaretOffset, matchedRemoteCaret.IdeUserIdentity);
                    }
                }
            }
        }

        private void CreateVisual(SyntaxNode node, int caretOffset, UserIdentity userIdentity)
        {
            // TODO: Improve what we render here
            var span = new SnapshotSpan(View.TextSnapshot, node.SpanStart + caretOffset, 1);
            Geometry geometry = View.TextViewLines.GetMarkerGeometry(span);
            if (geometry != null)
            {
                var drawing = new GeometryDrawing(brush, pen, geometry);
                drawing.Freeze();

                var drawingImage = new DrawingImage(drawing);
                drawingImage.Freeze();

                var image = new Image
                {
                    Source = drawingImage,
                };

                // Align the image with the top of the bounds of the text geometry
                Canvas.SetLeft(image, geometry.Bounds.Left);
                Canvas.SetTop(image, geometry.Bounds.Top);

                Layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
            }
        }

        public void Dispose()
        {
            TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived -= RemoteModelChangeManager_RemoteModelReceived;
        }
    }
}
