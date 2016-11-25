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
    public static partial class SyntaxNodeExtensions
    {
        public static bool IsTrackedLeafNode(this SyntaxNode syntaxNode)
        {
            return syntaxNode is MemberDeclarationSyntax ||
                   syntaxNode is TypeBlockSyntax ||
                   syntaxNode is MethodBlockBaseSyntax;
        }
        public static string GetIdentifyingString(this SyntaxNode syntaxNode)
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
                case ClassBlockSyntax typedNode: name = typedNode.ClassStatement.GetIdentifyingString(); break;
                case ConstructorBlockSyntax typedNode: name = typedNode.BlockStatement.GetIdentifyingString(); break;
                case EnumBlockSyntax typedNode: name = typedNode.EnumStatement.GetIdentifyingString(); break;
                case InterfaceBlockSyntax typedNode: name = typedNode.BlockStatement.GetIdentifyingString(); break;
                case MethodBlockSyntax typedNode: name = typedNode.BlockStatement.GetIdentifyingString(); break;
                case ModuleBlockSyntax typedNode: name = typedNode.BlockStatement.GetIdentifyingString(); break;
                case NamespaceBlockSyntax typedNode: name = typedNode.NamespaceStatement.GetIdentifyingString(); break;
                case PropertyBlockSyntax typedNode: name = typedNode.PropertyStatement.GetIdentifyingString(); break;
                case StructureBlockSyntax typedNode: name = typedNode.BlockStatement.GetIdentifyingString(); break;
                default: return syntaxNode.Parent.GetIdentifyingString();
            }

            string parentName = syntaxNode.Parent.GetIdentifyingString();
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

            var name = syntaxNode.GetIdentifyingString();

            var identityHash = name == null ? 0 : syntaxNode is VisualBasicSyntaxNode ? StringComparer.OrdinalIgnoreCase.GetHashCode(name) : StringComparer.Ordinal.GetHashCode(name);

            if (!(syntaxNode is ICompilationUnitSyntax) && (identityHash == 0 || syntaxNode.ChildNodes().Count() == 0))
            {
                identityHash = syntaxNode.ToString().GetHashCode();
            }

            SyntaxNodeHashCache.Add(syntaxNode, identityHash);

            return identityHash;
        }
    }
}
