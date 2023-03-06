using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PacketsGenerator.Extensions;

namespace Iterum.PacketsGenerator;

public class PacketSyntaxReceiver : ISyntaxReceiver
{
    public List<StructDeclarationSyntax> Candidates { get; } = new List<StructDeclarationSyntax>();
    
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is StructDeclarationSyntax syntax && syntax.HaveAttribute(AttributeModel.Packet))
        {
            Candidates.Add(syntax);
        }
    }
}