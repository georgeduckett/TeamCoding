using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;
using TeamCoding.Extensions;
using TeamCoding.Interfaces.Documents;
using TeamCoding.Interfaces.Extensions;

namespace TeamCoding.Documents
{
    public class CaretInfoProvider : ICaretInfoProvider
    {
        public async Task<DocumentRepoMetaData.CaretInfo> GetCaretInfoAsync(SnapshotPoint snapshotPoint)
        {
            var document = snapshotPoint.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
            {
                return GetTextCaretInfo(snapshotPoint);
            }

            var syntaxRoot = await document.GetSyntaxRootAsync();
            if (syntaxRoot == null)
            {
                return null;
            }

            return GetRoslynCaretInfo(syntaxRoot, snapshotPoint);
        }
        private DocumentRepoMetaData.CaretInfo GetRoslynCaretInfo(SyntaxNode syntaxRoot, SnapshotPoint snapshotPoint)
        {
            var caretToken = syntaxRoot.FindToken(snapshotPoint);
            int[] memberHashCodes = null;
            IEnumerable<SyntaxNode> memberNodes = null;

            var desiredLeafNode = caretToken.Parent.AncestorsAndSelf().FirstOrDefault(n => n.IsUniquelyIdentifiedNode());

            switch (caretToken.Language)
            {
                case "C#":
                case "Visual Basic":
                    memberNodes = caretToken.Parent.AncestorsAndSelf().Reverse().TakeWhileInclusive(n => n != desiredLeafNode).ToArray();
                    memberHashCodes = memberNodes.Select(n => n.GetValueBasedHashCode()).ToArray();
                    break;
                default:
                    TeamCodingProjectTypeProvider.Get<ITeamCodingPackageProvider>().Logger.WriteInformation($"Document with unsupported language found: {caretToken.Language}"); return null;
            }

            var lastNode = memberNodes.Last();

            var caretLine = snapshotPoint.GetContainingLine();
            var lastNodeLine = snapshotPoint.Snapshot.GetLineFromPosition(lastNode.Span.Start);

            return new DocumentRepoMetaData.CaretInfo()
            {
                SyntaxNodeIds = memberHashCodes,
                LeafMemberLineOffset = caretLine.LineNumber - lastNodeLine.LineNumber,
                LeafMemberCaretOffset = snapshotPoint.Position - caretLine.Start
            };
        }
        private DocumentRepoMetaData.CaretInfo GetTextCaretInfo(SnapshotPoint snapshotPoint)
        {
            return new DocumentRepoMetaData.CaretInfo()
            {
                SyntaxNodeIds = new[] { snapshotPoint.Snapshot.GetText().ToIntegerCode() },
                LeafMemberCaretOffset = snapshotPoint.Position
            };
        }
    }
}
