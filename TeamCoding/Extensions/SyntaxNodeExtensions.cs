using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.TeamFoundation.CodeSense.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static int GetTreePositionHashCode(this SyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var Identity = Microsoft.VisualStudio.CodeSense.Client.Common.CommonSyntaxNodeExtensions.GetCodeElementIdentityCommon(node);

            if(Identity == null)
            {
                TeamCodingPackage.Current.Logger.WriteError($"Unable to get node identity in function {nameof(GetTreePositionHashCode)}. Reverting to using anscestor HashCodes. Node: {node}");
                // Can't just use the node's parents and self's hashcode since that includes trivia (whitespace), so we combine the hash code of all token's text
                return node.AncestorsAndSelf().SelectMany(n => n.DescendantTokens()).Aggregate(17, (acc, next) => unchecked(acc * 31 + next.ToString().GetHashCode()));
            }

            return Identity.GetHashCode();
        }
    }
}
