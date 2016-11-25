using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class ParameterListSyntaxExtensions
    {
        internal static string GetParameterTypesString(this Microsoft.CodeAnalysis.VisualBasic.Syntax.ParameterListSyntax typeParameters)
        {
            if (typeParameters?.Parameters.Any() ?? false)
            {
                return "(" + string.Join(", ", typeParameters?.Parameters.Select(p => p.AsClause?.Type?.ToString() ?? "Nothing")) + ")";
            }
            return string.Empty;
        }
        internal static string GetParameterTypesString(this Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax typeParameters)
        {
            if (typeParameters?.Parameters.Any() ?? false)
            {
                return "(" + string.Join(", ", typeParameters?.Parameters.Select(p => p.Type.ToString())) + ")";
            }
            return string.Empty;
        }
        internal static string GetGenericParametersString(this Microsoft.CodeAnalysis.VisualBasic.Syntax.TypeParameterListSyntax typeParameters)
        {
            if (typeParameters?.Parameters.Any() ?? false)
            {
                return "<" + typeParameters?.Parameters.Count().ToString() + ">";
            }
            return string.Empty;
        }
        internal static string GetGenericParametersString(this Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax typeParameters)
        {
            if (typeParameters?.Parameters.Any() ?? false)
            {
                return "<" + typeParameters?.Parameters.Count().ToString() + ">";
            }
            return string.Empty;
        }
    }
}
