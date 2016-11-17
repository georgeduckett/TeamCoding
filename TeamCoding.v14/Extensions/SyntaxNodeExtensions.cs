using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
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
        private static readonly Dictionary<SyntaxNode, int> _SyntaxNodeHashes = new Dictionary<SyntaxNode, int>();
        private static readonly MultiValueDictionary<string, SyntaxNode> _SyntaxNodesFromFilePath = new MultiValueDictionary<string, SyntaxNode>();

        public static bool IsTrackedLeafNode(this SyntaxNode syntaxNode)
        {
            return syntaxNode is MemberDeclarationSyntax ||
                   syntaxNode is TypeBlockSyntax ||
                   syntaxNode is MethodBlockBaseSyntax;
        }
        public static int GetNameBasedHashCode(this SyntaxNode syntaxNode)
        {
            // TODO: Get name from Microsoft.TeamFoundation.CodeSense.Client.Common.dll
            // Get name based on all types the classes CSharpSyntaxNodeExtensions.PropertyVisitor and VisualBasicSyntaxNodeExtensions.PropertyVisitor visit
            // If language is Visual Basic use StringComparer.OrdinalIgnoreCase.GetHashCode(name) else StringComparer.Ordinal(name)
            return 0;
        }
        public static int GetValueBasedHashCode(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
            {
                throw new ArgumentNullException(nameof(syntaxNode));
            }

            if (_SyntaxNodeHashes.ContainsKey(syntaxNode))
            {
                return _SyntaxNodeHashes[syntaxNode];
            }

            var identityHash = syntaxNode.GetNameBasedHashCode();

            var fieldDeclarationNodeCS = syntaxNode as Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax;
            var fieldDeclarationNodeVB = syntaxNode as Microsoft.CodeAnalysis.VisualBasic.Syntax.FieldDeclarationSyntax;
            var methodDeclarationNode = syntaxNode as BaseMethodDeclarationSyntax;
            var methodBaseNode = syntaxNode as MethodBlockBaseSyntax;

            if(fieldDeclarationNodeCS != null)
            {
                // We could have seveal methods with the overloads, so base the hash on the parameter types too
                identityHash = 17 * 31 + identityHash;

                foreach (var var in fieldDeclarationNodeCS.Declaration.Variables)
                {
                    identityHash = identityHash * 31 + var.Identifier.Text.GetHashCode();
                }
            }
            else if (fieldDeclarationNodeVB != null)
            {
                foreach (var dec in fieldDeclarationNodeVB.Declarators)
                {
                    foreach (var var in dec.Names)
                    {
                        identityHash = identityHash * 31 + var.Identifier.Text.GetHashCode();
                    }
                }
            }
            else if (methodDeclarationNode != null)
            {
                // We could have seveal methods with the overloads, so base the hash on the parameter types too
                identityHash = 17 * 31 + identityHash;

                foreach (var param in methodDeclarationNode.ParameterList.Parameters)
                {
                    identityHash = identityHash * 31 + param.Type.GetValueBasedHashCode();
                }
            }
            else if (methodBaseNode != null)
            {
                // We could have seveal methods with the overloads, so base the hash on the parameter types too
                identityHash = 17 * 31 + identityHash;

                if (methodBaseNode.BlockStatement?.ParameterList != null)
                {
                    foreach (var param in methodBaseNode.BlockStatement.ParameterList.Parameters)
                    {
                        identityHash = identityHash * 31 + param.AsClause.Type.GetValueBasedHashCode();
                    }
                }
            }

            if (!(syntaxNode is ICompilationUnitSyntax) && (identityHash == 0 || syntaxNode.ChildNodes().Count() == 0))
            {
                identityHash = syntaxNode.ToString().GetHashCode();
            }

            var filePath = syntaxNode.SyntaxTree?.FilePath;
            if(syntaxNode is ICompilationUnitSyntax && filePath != null && _SyntaxNodesFromFilePath.ContainsKey(filePath))
            {
                // If we've got a new compilation unit syntax then any syntax nodes from the same file won't get used so remove them.
                foreach(var node in _SyntaxNodesFromFilePath[filePath])
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

            return identityHash;
        }
    }
}
