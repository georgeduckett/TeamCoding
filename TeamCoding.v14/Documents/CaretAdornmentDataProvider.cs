using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using TeamCoding.Interfaces.Documents;
using Microsoft.CodeAnalysis.Text;
using TeamCoding.Extensions;
using Microsoft.CodeAnalysis;

namespace TeamCoding.Documents
{
    public class CaretAdornmentDataProvider : ICaretAdornmentDataProvider
    {
        public async Task<IEnumerable<CaretAdornmentData>> GetCaretAdornmentData(ITextSnapshot textSnapshot, int[] caretMemberHashcodes)
        {
            if(caretMemberHashcodes == null || caretMemberHashcodes.Length == 0)
            {
                return Enumerable.Empty<CaretAdornmentData>();
            }

            var document = textSnapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
            {
                return Enumerable.Empty<CaretAdornmentData>();
            }
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var rootNode = await syntaxTree.GetRootAsync();

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
                    foreach(var childTokenOrNode in nodes[nodeIndex].ChildNodesAndTokens())
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
            
            return nodes.Select(n => new CaretAdornmentData(n.FullSpan.Start + (n.HasLeadingTrivia ? n.GetLeadingTrivia().SelectMany(t => t.ToFullString()).TakeWhile(c => char.IsWhiteSpace(c)).Count() : 0), n.SpanStart, n.Span.End));
        }
    }
}
