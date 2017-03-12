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
using TeamCoding.VisualStudio.Controls;
using TeamCoding.Documents.SourceControlRepositories;

namespace TeamCoding.VisualStudio.TextAdornment
{
    internal sealed class TextAdornment : IDisposable
    {
        private readonly IAdornmentLayer Layer;
        private readonly IWpfTextView View;
        private readonly Pen CaretPen;

        private readonly DocumentRepoMetaData RepoDocument;

        private Func<IRemotelyAccessedDocumentData, bool> OpenFilesFilter;
        private readonly Dictionary<string, Queue<FrameworkElement>> UserAvatars = new Dictionary<string, Queue<FrameworkElement>>();
        private readonly ISourceControlRepository SourceControlRepo;
        public TextAdornment(IWpfTextView view)
        {
            SourceControlRepo = TeamCodingPackage.Current.SourceControlRepo;
            OpenFilesFilter = of => of.Repository.Equals(RepoDocument.RepoUrl, StringComparison.OrdinalIgnoreCase) &&
                                    of.RelativePath.Equals(RepoDocument.RelativePath, StringComparison.OrdinalIgnoreCase) &&
                                    of.RepositoryBranch == RepoDocument.RepoBranch &&
                                    of.CaretPositionInfo != null;

            View = view ?? throw new ArgumentNullException(nameof(view));

            Layer = view.GetAdornmentLayer("TextAdornment");
            RepoDocument = SourceControlRepo.GetRepoDocInfo(View.TextBuffer.GetTextDocumentFilePath());

            TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived += RefreshAdornmentsAsync;
            TeamCodingPackage.Current.Settings.UserSettings.UserCodeDisplayChanged += UserSettings_UserCodeDisplayChangedAsync;
            View.LayoutChanged += OnLayoutChanged;

            var penBrush = new SolidColorBrush(Colors.Red);
            penBrush.Freeze();
            CaretPen = new Pen(penBrush, 1);
            CaretPen.Freeze();
        }

        private async void UserSettings_UserCodeDisplayChangedAsync(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Layer.RemoveAllAdornments();
            UserAvatars.Clear();
            RefreshAdornmentsAsync(sender, e);
        }

        private async void RefreshAdornmentsAsync(object sender, EventArgs e)
        {
            var CaretPositions = TeamCodingPackage.Current.RemoteModelChangeManager.GetOpenFiles()
                                                          .Where(OpenFilesFilter)
                                                          .Select(of => new
                                                          {
                                                              CaretMemberHashCodes = of.CaretPositionInfo.SyntaxNodeIds,
                                                              of.CaretPositionInfo.LeafMemberCaretOffset,
                                                              of.CaretPositionInfo.LeafMemberLineOffset,
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
                var nodes = await TeamCodingPackage.Current.CaretAdornmentDataProvider.GetCaretAdornmentDataAsync(View.TextSnapshot, caret.CaretMemberHashCodes);

                foreach(var node in nodes)
                {
                    // We got to the end, matching all nodes all the way down
                    CreateVisual(node, caret.LeafMemberLineOffset, caret.LeafMemberCaretOffset, caret.IdeUserIdentity);
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
            RefreshAdornmentsAsync(sender, EventArgs.Empty);
        }

        private void CreateVisual(CaretAdornmentData nodeData, int caretLineOffset, int caretOffset, IUserIdentity userIdentity)
        {
            if (RepoDocument != null && nodeData.RelativeToServerSource)
            {
                var changes = SourceControlRepo.GetDiffWithServer(View.TextBuffer.GetTextDocumentFilePath());

                if (changes != null)
                {
                    var (additions, deletions) = changes.Value;

                    // If we've added lines before the caret line, then in this file the caret would be on a line below for each line added (therefore add additions),
                    // and for deletions the caret would be on a line above (therefore subtract deletions)
                    caretLineOffset += additions.Count(n => n <= caretLineOffset) - deletions.Count(n => n <= caretLineOffset);
                }
            }

            var caretLineNumber = View.TextSnapshot.GetLineNumberFromPosition(nodeData.SpanStart) + caretLineOffset;

            var caretPosition = View.TextSnapshot.GetLineFromLineNumber(Math.Min(caretLineNumber, View.TextSnapshot.LineCount - 1)).Start.Position + caretOffset;

            if(caretPosition < nodeData.NonWhiteSpaceStart)
            {
                caretPosition = nodeData.NonWhiteSpaceStart;
            }
            else if(caretPosition > nodeData.SpanEnd)
            {
                caretPosition = nodeData.SpanEnd;
            }

            var atEnd = caretPosition >= View.TextSnapshot.Length;
            var remoteCaretSpan = new SnapshotSpan(View.TextSnapshot, atEnd ? View.TextSnapshot.Length - 1 : caretPosition, 1);
            var onSameLineAsEnd = remoteCaretSpan.Start.GetContainingLine().LineNumber == View.TextSnapshot.GetLineNumberFromPosition(View.TextSnapshot.Length);

            Geometry characterGeometry = View.TextViewLines.GetMarkerGeometry(remoteCaretSpan);
            if (characterGeometry != null)
            {
                var caretGeometry = new LineGeometry(atEnd && onSameLineAsEnd ? characterGeometry.Bounds.TopRight : characterGeometry.Bounds.TopLeft,
                                                     atEnd && onSameLineAsEnd ? characterGeometry.Bounds.BottomRight : characterGeometry.Bounds.BottomLeft);
                var drawing = new GeometryDrawing(null, UserColours.GetUserPen(userIdentity), caretGeometry);
                drawing.Freeze();

                var drawingImage = new DrawingImage(drawing);
                drawingImage.Freeze();

                var image = new Image
                {
                    Source = drawingImage,
                };

                // Align the image with the top of the bounds of the text geometry
                var bounds = caretGeometry.Bounds;
                Canvas.SetLeft(image, bounds.Left);
                Canvas.SetTop(image, bounds.Top);

                Layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, remoteCaretSpan, null, image, null);

                FrameworkElement userControl = null;
                if (UserAvatars.ContainsKey(userIdentity.Id) && UserAvatars[userIdentity.Id].Count != 0)
                {
                    userControl = UserAvatars[userIdentity.Id].Dequeue();
                }
                if (TeamCodingPackage.Current.Settings.UserSettings.UserCodeDisplay == Options.UserSettings.UserDisplaySetting.Colour)
                {
                    if (userControl == null)
                    {
                        userControl = new Border()
                        {
                            Tag = userIdentity.Id,
                            Background = UserColours.GetUserBrush(userIdentity),
                            CornerRadius = new CornerRadius(bounds.Height / 2 / 2)
                        };
                    }
                    userControl.Width = bounds.Height / 2;
                    userControl.Height = bounds.Height / 2;
                    Canvas.SetTop(userControl, bounds.Top - (userControl.Height * 0.75));
                }
                else
                {
                    if (userControl == null)
                    {
                        userControl = TeamCodingPackage.Current.UserImages.CreateUserIdentityControl(userIdentity);
                    }
                    userControl.Width = bounds.Height / 1.25f;
                    userControl.Height = bounds.Height / 1.25f;
                    Canvas.SetTop(userControl, bounds.Top - userControl.Height);
                }
                userControl.ToolTip = userIdentity.DisplayName ?? userIdentity.Id;
                Canvas.SetLeft(userControl, bounds.Left - userControl.Width / 2);
                Layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, remoteCaretSpan, null, userControl, AdornmentRemoved);
            }
        }
        public void AdornmentRemoved(object sender, UIElement element)
        {
            var frameworkElement = element as FrameworkElement;
            if (frameworkElement?.Tag as string != null)
            {
                if(!UserAvatars.ContainsKey(frameworkElement.Tag as string))
                {
                    UserAvatars.Add(frameworkElement.Tag as string, new Queue<FrameworkElement>());
                }
                UserAvatars[frameworkElement.Tag as string].Enqueue(frameworkElement);
            }
        }
        public void Dispose()
        {
            TeamCodingPackage.Current.RemoteModelChangeManager.RemoteModelReceived -= RefreshAdornmentsAsync;
            TeamCodingPackage.Current.Settings.UserSettings.UserCodeDisplayChanged -= UserSettings_UserCodeDisplayChangedAsync;
        }
    }
}
