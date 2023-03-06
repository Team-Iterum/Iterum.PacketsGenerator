namespace Iterum.PacketsGenerator;

public static class PacketFieldSerDesFactory
{
    public static (string, string) GetSerDes(PacketFieldModel f)
    {
        string header = $"// {f.Type} {f.Name}{(f.IsEnum ? " Enum" : "")}{(f.IsList ? " Array" : "")}\n";
        string ser = "";
        string des = "";

        switch (f.Type)
        {
            case "Vector3":
            {
                if (f.BoundedRange != null)
                {
                    var local = $"local_{f.Name.ToLower()}_{f.FieldIndex}";
                    
                    ser += $"var {local} = BoundedRange.Compress({f.Name}, {f.BoundedRange});" +
                           $"data.AddUInt({local}.x);\n" +
                           $"data.AddUInt({local}.y);\n" +
                           $"data.AddUInt({local}.z);\n";
                    des += $"{f.Name} = BoundedRange.Decompress(new CompressedVector3(data.ReadUInt(),data.ReadUInt(),data.ReadUInt()), {f.BoundedRange});";
                }
                else
                {
                    
                    ser += $"data.AddUShort(HalfPrecision.Compress({f.Name}.x));\n" +
                           $"data.AddUShort(HalfPrecision.Compress({f.Name}.y));\n" +
                           $"data.AddUShort(HalfPrecision.Compress({f.Name}.z));";
                    des +=
                        $"{f.Name} = new Vector3(HalfPrecision.Decompress(data.ReadUShort()), HalfPrecision.Decompress(data.ReadUShort()), HalfPrecision.Decompress(data.ReadUShort()));";
                }
            }
                break;
            case "Vector2":
                
                if (f.BoundedRange != null)
                {
                    var local = $"local_{f.Name.ToLower()}_{f.FieldIndex}";

                    ser += $"var {local} = BoundedRange.Compress({f.Name}, {f.BoundedRange});" +
                           $"data.AddUInt({local}.x);\n" +
                           $"data.AddUInt({local}.y);";
                    des += $"{f.Name} = BoundedRange.Decompress(new CompressedVector2(data.ReadUInt(),data.ReadUInt()), {f.BoundedRange});";
                }
                else
                {
                    ser += $"data.AddUShort(HalfPrecision.Compress({f.Name}.x))\n;" +
                           $"data.AddUShort(HalfPrecision.Compress({f.Name}.y));";
                
                    des += $"{f.Name} = new Vector3(HalfPrecision.Decompress(data.ReadUShort()), HalfPrecision.Decompress(data.ReadUShort()));";    
                }

                break;
            case "Vector4":
                if (f.BoundedRange != null)
                {
                    var local = $"local_{f.Name.ToLower()}_{f.FieldIndex}";

                    ser += $"var {local} = BoundedRange.Compress({f.Name}, {f.BoundedRange});" +
                           $"data.AddUInt({local}.x);\n" +
                           $"data.AddUInt({local}.y);\n" +
                           $"data.AddUInt({local}.z);\n" +
                           $"data.AddUInt({local}.w);";
                    des += $"{f.Name} = BoundedRange.Decompress(new CompressedVector4(data.ReadUInt(),data.ReadUInt(),data.ReadUInt(),data.ReadUInt()), {f.BoundedRange});";
                }
                else
                {
                    ser += $"data.AddUShort(HalfPrecision.Compress({f.Name}.x));\n" +
                           $"data.AddUShort(HalfPrecision.Compress({f.Name}.y));\n" +
                           $"data.AddUShort(HalfPrecision.Compress({f.Name}.z));\n" +
                           $"data.AddUShort(HalfPrecision.Compress({f.Name}.w));";
                    des +=
                        $"{f.Name} = new Vector3(HalfPrecision.Decompress(data.ReadUShort()), HalfPrecision.Decompress(data.ReadUShort()), HalfPrecision.Decompress(data.ReadUShort()),  HalfPrecision.Decompress(data.ReadUShort()));";
                }

                break;
            case "Quaternion":
            {
                var local = $"local_{f.Name.ToLower()}_{f.FieldIndex}";

                ser += $"var {local} = SmallestThree.Compress({f.Name});" +
                       $"data.AddByte({local}.m);\n" +
                       $"data.AddShort({local}.a);\n" +
                       $"data.AddShort({local}.b);\n" +
                       $"data.AddShort({local}.c);";
                des += $"{f.Name} = SmallestThree.Decompress(new CompressedQuaternion(data.ReadByte(), data.ReadShort(), data.ReadShort(), data.ReadShort()));";
            }
                break;
            case "string":
                
                ser += $"data.AddString({f.Name});";
                des += $"{f.Name} = data.ReadString();";
                
                break;
            case "float":
                
                ser += $"data.AddUShort(HalfPrecision.Compress({f.Name}));";
                des += $"{f.Name} = HalfPrecision.Decompress(data.ReadUShort());";
                
                break;
            case "int":
                
                ser += $"data.AddInt({f.Name});";
                des += $"{f.Name} = data.ReadInt();";
                
                break;
            case "uint":
                
                if (f.BitValue > 0)
                {
                    ser += $"data.Add({f.BitValue}, {f.Name});";
                    des += $"{f.Name} = data.Read({f.BitValue});";
                }
                else
                {
                    ser += $"data.AddUInt({f.Name});";
                    des += $"{f.Name} = data.ReadUInt();";
                }

                break;
            case "ulong":
                
                ser += $"data.AddULong({f.Name});";
                des += $"{f.Name} = data.ReadULong();";
                
                break;
            case "ushort":
                
                ser += $"data.AddUShort({f.Name});";
                des += $"{f.Name} = data.ReadUShort();";
                
                break;
            case "short":

                ser += $"data.AddShort({f.Name});";
                des += $"{f.Name} = data.ReadShort();";
                
                break;
            case "int64":

                ser += $"data.AddLong({f.Name});";
                des += $"{f.Name} = data.ReadLong();";

                break;
            case "byte":
                
                ser += $"data.AddByte({f.Name});";
                des += $"{f.Name} = data.ReadByte();";
                
                break;
            case "bool":
                
                ser += $"data.AddBool({f.Name});";
                des += $"{f.Name} = data.ReadBool();";
                
                break;
            default:

                if (f.IsList)
                {
                    ser += $"data.AddUShort((ushort){f.Name}.Length);" +
                           $"for (int i{f.FieldIndex} = 0; i{f.FieldIndex} < {f.Name}.Length; i{f.FieldIndex}++)" +
                           "{\n";

                    des += $"{f.Name} = new {f.Type}[(int)data.ReadUShort()];"+
                           $"for (int i{f.FieldIndex} = 0; i{f.FieldIndex} < {f.Name}.Length; i{f.FieldIndex}++)" +
                           "{\n";
                    
                    foreach (var field in f.ListFields)
                    {
                        var (innerSer, innerDes) = GetSerDes(field);
                        ser += innerSer+"\n";
                        des += innerDes+"\n";
                    }
                           
                    ser += "}";
                    des += "}";
                    

                }
                else if (f.IsEnum)
                {
                    ser += $"data.AddByte((byte){f.Name});";
                    des += $"{f.Name} = ({f.Type})data.ReadByte();";
                }
                else
                {
                    ser += $"// Error. Can't find serializer {f.Type} {f.Name}";
                    des += $"// Error. Can't find deserializer {f.Type} {f.Name}";
                }

                break;
        }

        if (f.UseIfTrue != null)
        {
            ser = $"if ({f.UseIfTrue}) {{\n" + ser + "\n}";
            des = $"if ({f.UseIfTrue}) {{\n" + des + "\n}";
        }

        ser = header + ser;
        des = header + des;

        return (ser, des);
    }
}