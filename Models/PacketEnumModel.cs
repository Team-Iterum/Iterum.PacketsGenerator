using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Iterum.PacketsGenerator;

public class PacketEnumModel
{
    public string Name  { get; set; }
    public List<PacketEnumFieldModel> Fields  { get; set; }
    
    public static List<PacketEnumModel> GetEnums(StructDeclarationSyntax syntax)
    {
        return syntax.Members.OfType<EnumDeclarationSyntax>().Select(e =>
            new PacketEnumModel
            {
                Name = e.Identifier.Text,
                Fields = e.Members.Select(enumDec => new PacketEnumFieldModel {Name = enumDec.Identifier.Text}).ToList()
            }).ToList();
    }
}

public class PacketEnumFieldModel
{
    public string Name  { get; set; }
}