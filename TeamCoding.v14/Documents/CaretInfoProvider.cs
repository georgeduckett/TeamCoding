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

namespace TeamCoding.Documents
{
    public class CaretInfoProvider : ICaretInfoProvider
    {
        public async Task<DocumentRepoMetaData.CaretInfo> GetCaretInfo(SnapshotPoint snapshotPoint)
        {
            var document = snapshotPoint.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
            {
                return null;
            }
            var syntaxRoot = await document.GetSyntaxRootAsync();
            var caretToken = syntaxRoot.FindToken(snapshotPoint);
            int[] memberHashCodes = null;
            IEnumerable<SyntaxNode> memberNodes = null;

            var desiredLeafNode = caretToken.Parent.AncestorsAndSelf().FirstOrDefault(n => n.IsTrackedLeafNode());

            switch (caretToken.Language)
            {
                case "C#":
                case "Visual Basic":
                    memberNodes = caretToken.Parent.AncestorsAndSelf().Reverse().TakeWhileInclusive(n => n != desiredLeafNode).ToArray();
                    memberHashCodes = memberNodes.Select(n => n.GetValueBasedHashCode()).ToArray();
                    break;
                    // TODO: Add the logging back in properly
                // default:
                    //TeamCodingPackage.Current.Logger.WriteInformation($"Document with unsupported language found: {caretToken.Language}"); return null;
            }

            return new DocumentRepoMetaData.CaretInfo() { SyntaxNodeIds = memberHashCodes, LeafMemberCaretOffset = snapshotPoint.Position - memberNodes.Last().SpanStart };
        }
    }
}
