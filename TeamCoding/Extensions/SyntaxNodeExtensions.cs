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

namespace TeamCoding.Extensions
{
    public static class SyntaxNodeExtensions
    {
        private readonly static ConcurrentDictionary<SyntaxNode, Documents.DocumentRepoMetaData.CaretInfo.SyntaxNodeIdentifier> CachedHashes =
            new ConcurrentDictionary<SyntaxNode, Documents.DocumentRepoMetaData.CaretInfo.SyntaxNodeIdentifier>();
        public static Documents.DocumentRepoMetaData.CaretInfo.SyntaxNodeIdentifier GetTreePositionHashCode(this SyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            Documents.DocumentRepoMetaData.CaretInfo.SyntaxNodeIdentifier hash;
            if(CachedHashes.TryGetValue(node, out hash))
            {
                return hash;
            }

            unchecked
            {
                // Use a hash of the content of the node and all parents, hashed together
                hash = new Documents.DocumentRepoMetaData.CaretInfo.SyntaxNodeIdentifier(node.AncestorsAndSelf().Select(a => a.ToString().GetHashCode()).Aggregate(17, (acc, next) => acc * 31 + next));
            }

            CachedHashes.AddOrUpdate(node, hash, (n, e) => e);
            return hash;
        }
    }
}
