
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PacketsGenerator.Extensions;

namespace Iterum.PacketsGenerator;

public enum PacketDir
{
    /// <summary>
    /// Server -> client and client -> server
    /// </summary>
    Both,
    /// <summary>
    /// Server -> client
    /// </summary>
    SC,
    /// <summary>
    /// Client -> server
    /// </summary>
    CS
}

public class PacketModel
{
    public string Name { get; set; }

    public string EventName => Name;
    public bool IsServer { get; set; }
    public PacketDir Direction { get; set; }
    public string Namespace { get; set; }
    public int Header { get; set; }

    public List<PacketFieldModel> Fields { get; set; }
    public Dictionary<string, List<BoundedRangeModel>> BoundedRanges { get; set; }

    public void TryUpdatePacketAttribute(TypeDeclarationSyntax syntax)
    {
        var packet = syntax.FindAttributes(AttributeModel.Packet).FirstOrDefault();
        
        // packet direction
        {
            var args = packet.GetArgumentsValues<MemberAccessExpressionSyntax>();
            Direction = PacketDir.Both;
            
            var packetDir = args.FirstOrDefault()?.ToString();
            if (packetDir != null)
            {
                if (packetDir.Contains("CS")) Direction = PacketDir.CS;
                if (packetDir.Contains("SC")) Direction = PacketDir.SC;
            }
        }

        // packet header
        {
            var args = packet.GetArgumentsValues<LiteralExpressionSyntax>();

            var valueText = args.FirstOrDefault()?.Token.ValueText;
            if (valueText != null)
            {
                Header = int.Parse(valueText);
            }
        }

    }
}
