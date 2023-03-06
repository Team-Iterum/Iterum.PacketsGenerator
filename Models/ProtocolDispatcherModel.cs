namespace Iterum.PacketsGenerator;

public class ProtocolDispatcherModel
{
    public List<PacketModel> Packets = new List<PacketModel>();
    public bool IsServer { get; set; }
}