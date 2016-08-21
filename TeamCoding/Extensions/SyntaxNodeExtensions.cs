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
        private readonly static ConcurrentDictionary<SyntaxNode, SyntaxNodeIdentifier> CachedHashes =
            new ConcurrentDictionary<SyntaxNode, SyntaxNodeIdentifier>();
        public static SyntaxNodeIdentifier GetTreePositionHashCode(this SyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            SyntaxNodeIdentifier hash;
            if(CachedHashes.TryGetValue(node, out hash))
            {
                return hash;
            }
            
            hash = new SyntaxNodeIdentifier(node.AncestorsAndSelf().Select(a => a.GetHashCode()).ToArray());

            CachedHashes.AddOrUpdate(node, hash, (n, e) => e);
            return hash;
        }
    }
}
