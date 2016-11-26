using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static partial class SyntaxNodeExtensions
    {
        /// <summary>
        /// Caches syntax node hashes. Handles removing old syntax nodes with a file path of a compilation unit we're asked for the hash of
        /// </summary>
        private static class SyntaxNodeHashCache
        {
            private static readonly Dictionary<SyntaxNode, int> _SyntaxNodeHashes = new Dictionary<SyntaxNode, int>();
            private static readonly MultiValueDictionary<string, SyntaxNode> _SyntaxNodesFromFilePath = new MultiValueDictionary<string, SyntaxNode>();

            public static bool TryGetHash(SyntaxNode node, out int hash) => _SyntaxNodeHashes.TryGetValue(node, out hash);

            public static void Add(SyntaxNode syntaxNode, int identityHash)
            {
                var filePath = syntaxNode.SyntaxTree?.FilePath;
                if (syntaxNode is ICompilationUnitSyntax && filePath != null && _SyntaxNodesFromFilePath.ContainsKey(filePath))
                {
                    // If we've got a new compilation unit syntax then any syntax nodes from the same file won't get used so remove them.
                    foreach (var node in _SyntaxNodesFromFilePath[filePath])
                    {
                        _SyntaxNodeHashes.Remove(node);
                    }
                    _SyntaxNodesFromFilePath.Remove(filePath);
                }

                if (filePath != null)
                {
                    _SyntaxNodesFromFilePath.Add(filePath, syntaxNode);
                }
                _SyntaxNodeHashes.Add(syntaxNode, identityHash);
            }
        }
    }
}
