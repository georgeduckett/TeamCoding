using Microsoft.CodeAnalysis;
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
            if(node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return node.AncestorsAndSelf().Aggregate(17, (acc, next) => unchecked(acc * 31 + next.GetHashCode()));
        }
    }
}
