using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using TeamCoding.Interfaces.Documents;
using Microsoft.CodeAnalysis.Text;
using TeamCoding.Extensions;

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
            var nodes = new[] { rootNode };
            var i = 1;
            while (nodes.Length != 0 && i < caretMemberHashcodes.Length)
            {
                nodes = nodes.SelectMany(node => node.ChildNodes().Where(c => c.GetValueBasedHashCode() == caretMemberHashcodes[i])).ToArray();
                i++;
            }
            return nodes.Select(n => new CaretAdornmentData(n.SpanStart, n.Span.End));
        }
    }
}
