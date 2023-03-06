using System.Diagnostics;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PacketsGenerator.Extensions;

namespace Iterum.PacketsGenerator;

public class PacketFieldModel
{
    public string Type { get; set; }
    public string Name { get; set; }
    public int FieldIndex { get; set; }
    public string Ser { get; set; }
    public string Des { get; set; }
    public int BitValue { get; set; }
    public string UseIfTrue { get; set; }
    public string BoundedRange { get; set; }
    public bool IsEnum { get; set; }
    public bool IsList { get; set; }
    public List<PacketFieldModel> ListFields { get; set; }
    

    public void TryAddBitsAttribute(FieldDeclarationSyntax e)
    {
        var bits = e.FindAttributes(AttributeModel.Bits).FirstOrDefault();
        if (bits != null)
        {
            if (int.TryParse(bits.GetArgumentsValues().FirstOrDefault(), out var bitValue))
                BitValue = bitValue;
        }
    }
    
    public void TryAddUseIfAttribute(FieldDeclarationSyntax e)
    {
        var useIf = e.FindAttributes(AttributeModel.UseIf).FirstOrDefault();
        if (useIf != null)
        {
            string useIfString = useIf.GetArgumentsValues().FirstOrDefault();
            
            if (useIfString != null)
            {
                if (useIfString.Contains("nameof(")) useIfString = useIfString.Replace("nameof(", "").Replace(")", "");
                if (useIfString.Contains("\"")) useIfString = useIfString.Replace("\"", "");
                UseIfTrue = useIfString;
            }
        }
    }

    public void TryAddBoundedRange(FieldDeclarationSyntax e, ref PacketModel packet)
    { ;
        var boundedRanges = e.FindAttributes(AttributeModel.BoundedRange);
        if (Type is "Vector2" or "Vector3" or "Vector4")
        {
            string key = $"range_{packet.BoundedRanges.Count}";

            foreach (var attr in boundedRanges)
            {
                var args = attr.GetArgumentsValues();
                
                float min = float.Parse(args[0].Replace("f", ""), NumberStyles.Any);
                float max = float.Parse(args[1].Replace("f", ""), NumberStyles.Any);
                float precision = float.Parse(args[2].Replace("f", ""), NumberStyles.Any);
                
                if (!packet.BoundedRanges.ContainsKey(key)) packet.BoundedRanges.Add(key, new List<BoundedRangeModel>());
                
                packet.BoundedRanges[key].Add(new BoundedRangeModel(min, max, precision));
            }

            if (packet.BoundedRanges.ContainsKey(key))
                BoundedRange = key;
        }
    }
    
}