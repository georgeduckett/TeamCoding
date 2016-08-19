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

            unchecked
            {
                return node.AncestorsAndSelf().Select(a => a.ToString().GetHashCode()).Aggregate(17, (acc, next) => acc * 31 + next);
            }
        }
    }
}
