using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using TeamCoding.Interfaces.Documents;
using Microsoft.CodeAnalysis.Text;
using TeamCoding.Extensions;
using TeamCoding.Interfaces.Extensions;
using Microsoft.CodeAnalysis;

namespace TeamCoding.Documents
{
    public class CaretAdornmentDataProvider : ICaretAdornmentDataProvider
    {
        public async Task<IEnumerable<CaretAdornmentData>> GetCaretAdornmentDataAsync(ITextSnapshot textSnapshot, int[] caretMemberHashcodes)
        {
            if (caretMemberHashcodes == null || caretMemberHashcodes.Length == 0)
            {
                return Enumerable.Empty<CaretAdornmentData>();
            }

            var document = textSnapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
            {
                return GetTextCaretAdornmentData(textSnapshot, caretMemberHashcodes);
            }

            var syntaxTree = await document.GetSyntaxTreeAsync();
            if (syntaxTree == null)
            {
                return GetTextCaretAdornmentData(textSnapshot, caretMemberHashcodes);
            }

            var rootNode = await syntaxTree.GetRootAsync();
            return GetRoslynCaretAdornmentData(caretMemberHashcodes, rootNode);
        }
        private IEnumerable<CaretAdornmentData> GetTextCaretAdornmentData(ITextSnapshot textSnapshot, int[] caretMemberHashcodes)
        {
            string snapshotText = textSnapshot.GetText();
            if (caretMemberHashcodes.Length == 1)
            {
                return new[] { new CaretAdornmentData(true, 0, 0, snapshotText.Length) };
            }
            else
            {
                return Enumerable.Empty<CaretAdornmentData>();
            }
        }
        private static IEnumerable<CaretAdornmentData> GetRoslynCaretAdornmentData(int[] caretMemberHashcodes, SyntaxNode rootNode)
        {
            if (rootNode.GetValueBasedHashCode() != caretMemberHashcodes[0])
            {
                return Enumerable.Empty<CaretAdornmentData>();
            }
            var nodes = new List<SyntaxNode>() { rootNode };
            var i = 1;
            while (nodes.Count != 0 && i < caretMemberHashcodes.Length)
            {
                var oldNodesCount = nodes.Count;
                for (int nodeIndex = 0; nodeIndex < oldNodesCount; nodeIndex++)
                {
                    foreach (var childTokenOrNode in nodes[nodeIndex].ChildNodesAndTokens())
                    {
                        if (childTokenOrNode.IsNode)
                        {
                            var childNode = childTokenOrNode.AsNode();
                            if (childNode.GetValueBasedHashCode() == caretMemberHashcodes[i])
                            {
                                nodes.Add(childNode);
                            }
                        }
                    }
                }
                nodes.RemoveRange(0, oldNodesCount);
                i++;
            }

            return nodes.Select(n => new CaretAdornmentData(false, n.FullSpan.Start + (n.HasLeadingTrivia ? n.GetLeadingTrivia().SelectMany(t => t.ToFullString()).TakeWhile(c => char.IsWhiteSpace(c)).Count() : 0), n.SpanStart, n.Span.End));
        }
    }
}
