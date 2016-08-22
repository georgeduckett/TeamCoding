using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
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
        public static int GetValueBasedHashCode(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
            {
                throw new ArgumentNullException(nameof(syntaxNode));
            }

            var identityHash = CommonSyntaxNodeExtensions.GetCodeElementIdentityCommon(syntaxNode)?.GetHashCode() ?? 0;

            if (syntaxNode is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)
            {
                // We could have seveal methods with the overloads, so base the hash on the paramter types too
                identityHash = 17 * 31 + identityHash;

                foreach(var param in ((Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)syntaxNode).ParameterList.Parameters)
                {
                    identityHash = identityHash * 31 + param.Type.GetValueBasedHashCode();
                }
            }
            else if (syntaxNode is Microsoft.CodeAnalysis.VisualBasic.Syntax.MethodBlockBaseSyntax)
            {
                // We could have seveal methods with the overloads, so base the hash on the paramter types too
                identityHash = 17 * 31 + identityHash;

                foreach (var param in ((Microsoft.CodeAnalysis.VisualBasic.Syntax.MethodBaseSyntax)syntaxNode).ParameterList.Parameters)
                {
                    identityHash = identityHash * 31 + param.AsClause.Type.GetValueBasedHashCode();
                }
            }

            if (identityHash == 0 || syntaxNode.ChildNodes().Count() == 0)
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
