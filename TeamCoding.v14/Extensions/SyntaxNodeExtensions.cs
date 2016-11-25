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


        public static bool IsTrackedLeafNode(this SyntaxNode syntaxNode)
        {
            return syntaxNode is MemberDeclarationSyntax ||
                   syntaxNode is TypeBlockSyntax ||
                   syntaxNode is MethodBlockBaseSyntax;
        }
        public static string GetName(this SyntaxNode syntaxNode)
        {
            string name = null;
            switch (syntaxNode)
            {
                // Don't throw an exception on null, just return null
                case null: return null;
                // If we're a visual basic node and the parent is a "block" type one then we should return null as it's the parent that has the name;
                case VisualBasicSyntaxNode typedNode when (typedNode.Parent.GetType().Name.Contains("Block")): return null;
                // CSharp Nodes
                case ConstructorDeclarationSyntax typedNode: name = typedNode.Identifier.ToString(); break;
                case DestructorDeclarationSyntax typedNode: name = "~" + typedNode.Identifier.ToString(); break;

                case ClassDeclarationSyntax typedNode: name = typedNode.Identifier.ToString() + typedNode.TypeParameterList.GetGenericParametersString(); break;
                case StructDeclarationSyntax typedNode: name = typedNode.Identifier.ToString() + typedNode.TypeParameterList.GetGenericParametersString(); break;
                case DelegateDeclarationSyntax typedNode: name = typedNode.Identifier.ToString() + typedNode.TypeParameterList.GetGenericParametersString() + typedNode.ParameterList.GetParameterTypesString(); break;
                case InterfaceDeclarationSyntax typedNode: name = typedNode.Identifier.ToString() + typedNode.TypeParameterList.GetGenericParametersString(); break;

                case NamespaceDeclarationSyntax typedNode: name = typedNode.Name.ToString(); break;
                case EnumDeclarationSyntax typedNode: name = typedNode.Identifier.ToString(); break;
                case OperatorDeclarationSyntax typedNode: name = typedNode.OperatorToken.ToString(); break;
                case ConversionOperatorDeclarationSyntax typedNode: name = typedNode.Type.ToString(); break;
                    // TODO: Test multiple indexers with different types
                case IndexerDeclarationSyntax typedNode:
                    var parameters = typedNode.ParameterList.Parameters.Select(p => p.Type.ToString());
                    string parameterString = string.Join(", ", parameters);
                    name = typedNode.ExplicitInterfaceSpecifier != null ? typedNode.ExplicitInterfaceSpecifier.Name + $".[{parameterString}]" : $"[{parameterString}]"; break;


                case MethodDeclarationSyntax typedNode:
                    name = typedNode.Identifier.ToString() + typedNode.TypeParameterList.GetGenericParametersString() + typedNode.ParameterList.GetParameterTypesString();
                    if (typedNode.ExplicitInterfaceSpecifier != null)
                    {
                        name = typedNode.ExplicitInterfaceSpecifier.Name.ToString() + "." + name;
                    }
                    break;
                case PropertyDeclarationSyntax typedNode:
                    name = typedNode.Identifier.ToString();
                    if (typedNode.ExplicitInterfaceSpecifier != null)
                    {
                        name = typedNode.ExplicitInterfaceSpecifier.Name.ToString() + "." + name;
                    }
                    break;

                // Visual Basic Nodes
                case ClassStatementSyntax typedNode: name = typedNode.Identifier.ToString() + typedNode.TypeParameterList.GetGenericParametersString(); break;
                case DelegateStatementSyntax typedNode: name = typedNode.Identifier.ToString() + typedNode.TypeParameterList.GetGenericParametersString() + typedNode.ParameterList.GetParameterTypesString(); break;
                case InterfaceStatementSyntax typedNode: name = typedNode.Identifier.ToString() + typedNode.TypeParameterList.GetGenericParametersString(); break;
                case MethodStatementSyntax typedNode: name = typedNode.Identifier.ToString() + typedNode.TypeParameterList.GetGenericParametersString() + typedNode.ParameterList.GetParameterTypesString(); break;
                case ModuleStatementSyntax typedNode: name = typedNode.Identifier.ToString() + typedNode.TypeParameterList.GetGenericParametersString(); break;
                case StructureStatementSyntax typedNode: name = typedNode.Identifier.ToString() + typedNode.TypeParameterList.GetGenericParametersString(); break;
                case DeclareStatementSyntax typedNode: name = typedNode.Identifier.ToString(); break;
                case EnumStatementSyntax typedNode: name = typedNode.Identifier.ToString(); break;
                case NamespaceStatementSyntax typedNode: name = typedNode.Name.ToString(); break;
                case OperatorStatementSyntax typedNode: name = typedNode.OperatorToken.ToString(); break;
                case PropertyStatementSyntax typedNode: name = typedNode.Identifier.ToString(); break;
                case SubNewStatementSyntax typedNode: name = typedNode.NewKeyword.ToString(); break;
                case ClassBlockSyntax typedNode: name = typedNode.ClassStatement.GetName(); break;
                case ConstructorBlockSyntax typedNode: name = typedNode.BlockStatement.GetName(); break;
                case EnumBlockSyntax typedNode: name = typedNode.EnumStatement.GetName(); break;
                case InterfaceBlockSyntax typedNode: name = typedNode.BlockStatement.GetName(); break;
                case MethodBlockSyntax typedNode: name = typedNode.BlockStatement.GetName(); break;
                case ModuleBlockSyntax typedNode: name = typedNode.BlockStatement.GetName(); break;
                case NamespaceBlockSyntax typedNode: name = typedNode.NamespaceStatement.GetName(); break;
                case PropertyBlockSyntax typedNode: name = typedNode.PropertyStatement.GetName(); break;
                case StructureBlockSyntax typedNode: name = typedNode.BlockStatement.GetName(); break;
                default: return syntaxNode.Parent.GetName();
            }

            string parentName = syntaxNode.Parent.GetName();
            if (parentName != null)
            {
                return parentName + "." + name;
            }
            else
            {
                return name;
            }
        }
        public static int GetValueBasedHashCode(this SyntaxNode syntaxNode)
        { // TODO: Maybe figure out how to get a hash code of statements, so we can be more specific as to where the cursor is
            if (syntaxNode == null)
            {
                throw new ArgumentNullException(nameof(syntaxNode));
            }

            if (SyntaxNodeHashCache.TryGetHash(syntaxNode, out int hash))
            {
                return hash;
            }

            var name = syntaxNode.GetName();

            var identityHash = 0;

            if (name != null)
            {
                identityHash = syntaxNode is VisualBasicSyntaxNode ? StringComparer.OrdinalIgnoreCase.GetHashCode(name) : StringComparer.Ordinal.GetHashCode(name);
            }

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

            SyntaxNodeHashCache.Add(syntaxNode, identityHash);

            return identityHash;
        }
    }
}
