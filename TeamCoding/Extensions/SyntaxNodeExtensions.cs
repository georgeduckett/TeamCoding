using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.TeamFoundation.CodeSense.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;

namespace TeamCoding.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static SyntaxNodeIdentifier GetTreePositionHashCode(this SyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return SyntaxNodeIdentifier.Cache.GetIdentifier(node);
        }
    }
}