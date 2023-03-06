using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PacketsGenerator.Extensions;

public static class RoslynExtensions
{
    public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
    {
        var current = type;
        while (current != null)
        {
            yield return current;
            current = current.BaseType;
        }
    }

    public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol type)
    {
        return type.GetBaseTypesAndThis().SelectMany(n => n.GetMembers());
    }

    public static CompilationUnitSyntax GetCompilationUnit(this SyntaxNode syntaxNode)
    {
        return syntaxNode.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault();
    }
    
    public static IEnumerable<FieldDeclarationSyntax> GetFields(this StructDeclarationSyntax syntaxNode)
    {
        return syntaxNode.Members.OfType<FieldDeclarationSyntax>();
    }
    
    public static TypeKind? GetFieldTypeKind(this FieldDeclarationSyntax fieldSyntax, Compilation compilation)
    {
        var structSemanticModel = compilation.GetSemanticModel(fieldSyntax.SyntaxTree);
        return structSemanticModel.GetTypeInfo(fieldSyntax.Declaration.Type).Type?.TypeKind;
    }

    public static string GetTypeName(this FieldDeclarationSyntax field)
    {
        return field.Declaration.Type.ToString();
    }
    
    public static string GetFieldName(this FieldDeclarationSyntax type)
    {
        return type.Declaration.Variables[0].Identifier.Text;
    }

    public static string GetStructName(this StructDeclarationSyntax proxy)
    {
        return proxy.Identifier.Text;
    }

    public static string GetStructModifier(this StructDeclarationSyntax proxy)
    {
        return proxy.Modifiers.ToFullString().Trim();
    }

    public static bool HaveAttribute(this StructDeclarationSyntax syntax, string attributeName)
    {
        return syntax.AttributeLists.Count > 0 &&
               syntax.AttributeLists.SelectMany(al => al.Attributes
                       .Where(a => (a.Name as IdentifierNameSyntax)?.Identifier.Text == attributeName))
                   .Any();
    }
    
    public static List<AttributeSyntax> FindAttributes(this TypeDeclarationSyntax syntax, string attributeName)
    {
        return syntax.AttributeLists.SelectMany(al => al.Attributes
                .Where(a => (a.Name as IdentifierNameSyntax)?.Identifier.Text == attributeName))
            .ToList();
    }
    
    public static List<AttributeSyntax> FindAttributes(this FieldDeclarationSyntax syntax, string attributeName)
    {
        return syntax.AttributeLists.SelectMany(al => al.Attributes
                .Where(a => (a.Name as IdentifierNameSyntax)?.Identifier.Text == attributeName))
            .ToList();
    }
    

    public static List<string> GetArgumentsValues(this AttributeSyntax syntax)
    {
        return syntax.ArgumentList?.Arguments.Select(al => al.Expression.ToString()).ToList();
    }

    public static List<T> GetArgumentsValues<T>(this AttributeSyntax syntax) where T:ExpressionSyntax
    {
        return syntax.ArgumentList?.Arguments.Select(al => al.Expression).OfType<T>().ToList();
    }

    public static string GetNamespace(this CompilationUnitSyntax root)
    {
        return root.ChildNodes()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault()
            ?.Name
            .ToString();
    }

    public static List<string> GetUsings(this CompilationUnitSyntax root)
    {
        return root.ChildNodes()
            .OfType<UsingDirectiveSyntax>()
            .Select(n => n.Name.ToString())
            .ToList();
    }
}