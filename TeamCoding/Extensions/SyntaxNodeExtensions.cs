using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.VisualStudio.CodeSense.Client.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static bool IsTrackedLeafNode(this SyntaxNode syntaxNode)
        {
            return syntaxNode is MemberDeclarationSyntax ||
                   syntaxNode is MethodBaseSyntax;
        }
        public static int GetValueBasedHashCode(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
            {
                throw new ArgumentNullException(nameof(syntaxNode));
            }

            var identityHash = syntaxNode.GetCodeElementIdentityCommon()?.GetHashCode() ?? 0;

            var methodDeclarationNode = syntaxNode as BaseMethodDeclarationSyntax;
            var methodBaseNode = syntaxNode as MethodBaseSyntax;

            if (methodDeclarationNode != null)
            {
                // We could have seveal methods with the overloads, so base the hash on the paramter types too
                identityHash = 17 * 31 + identityHash;

                foreach(var param in methodDeclarationNode.ParameterList.Parameters)
                {
                    identityHash = identityHash * 31 + param.Type.GetValueBasedHashCode();
                }
            }
            else if (methodBaseNode != null)
            {
                // We could have seveal methods with the overloads, so base the hash on the paramter types too
                identityHash = 17 * 31 + identityHash;

                foreach (var param in methodBaseNode.ParameterList.Parameters)
                {
                    identityHash = identityHash * 31 + param.AsClause.Type.GetValueBasedHashCode();
                }
            }

            if (!(syntaxNode is ICompilationUnitSyntax) && (identityHash == 0 || syntaxNode.ChildNodes().Count() == 0))
            {
                return syntaxNode.ToString().GetHashCode();
            }
            else
            {
                return identityHash;
            }
        }
    }
}
