using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PacketsGenerator.Extensions;
using Scriban;

namespace Iterum.PacketsGenerator;

[Generator]
public class PacketGenerator : ISourceGenerator
{
    
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new PacketSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        List<PacketModel> packets = new();
        
        var packetTemplateText = ResourceReader.GetResource("Packet.txt");
        
        var packetTemplate = Template.Parse(packetTemplateText);
        if (packetTemplate.HasErrors)
            throw new InvalidOperationException($"Template parse error: {packetTemplate.Messages}");

        if (context.SyntaxReceiver is PacketSyntaxReceiver syntaxReceiver)
        {
            List<PacketModel> models = new();
            foreach (var candidate in syntaxReceiver.Candidates)
            {
                var model = GetModel(candidate, context.Compilation);
                models.Add(model);
            }

            int headerId = 1;
            foreach (var model in models)
            {
                if (model.Header == 0)
                {
                    model.Header = headerId;
                    headerId++;
                }


                var result = packetTemplate.Render(model, memberRenamer: member => member.Name);
                result = SyntaxFactory.ParseCompilationUnit(result)
                    .NormalizeWhitespace()
                    .GetText()
                    .ToString();
                context.AddSource($"{model.Name}.gen.cs", result);
                
                
                
                packets.Add(model);
            }
        }
        
        
        if (packets.Count > 0)
        {
            var dispatcherTemplateText = ResourceReader.GetResource("ProtocolDispatcher.txt");
            
            var template = Template.Parse(dispatcherTemplateText);
            if (template.HasErrors)
                throw new InvalidOperationException($"Template parse error: {template.Messages}");

            void Render(ProtocolDispatcherModel model)
            {
                var result = template.Render(model, memberRenamer: member => member.Name);
                result = SyntaxFactory.ParseCompilationUnit(result)
                    .NormalizeWhitespace()
                    .GetText()
                    .ToString();
                
                context.AddSource($"{(model.IsServer ? "Server" : "Client")}ProtocolDispatcher.gen.cs", result);
            }
            
            var serverDispatcherModel = new ProtocolDispatcherModel
            {
                Packets = packets.Where(e => e.Direction == PacketDir.Both || e.Direction == PacketDir.CS).ToList(),
                IsServer = true
            };
            Render(serverDispatcherModel);

            var clientDispatchModel = new ProtocolDispatcherModel
            {
                Packets = packets.Where(e => e.Direction == PacketDir.Both || e.Direction == PacketDir.SC).ToList(),
                IsServer = false
            };
            Render(clientDispatchModel);

        }
        
    }

    private PacketModel GetModel(StructDeclarationSyntax syntax, Compilation compilation)
    {
        var model = new PacketModel
        {
            Name = syntax.Identifier.Text,
            Namespace = syntax.GetCompilationUnit().GetNamespace(),
            
            BoundedRanges = new(),
        };
        model.TryUpdatePacketAttribute(syntax);
        
        model.Fields = syntax.GetFields().Select((fieldSyntax, fieldIndex) =>
        {
            var field = new PacketFieldModel
            {
                Type = fieldSyntax.GetTypeName().Replace("[]", ""), 
                Name = fieldSyntax.GetFieldName(),
                IsEnum = fieldSyntax.GetFieldTypeKind(compilation) == TypeKind.Enum,
                IsList = fieldSyntax.GetFieldTypeKind(compilation) == TypeKind.Array,
                FieldIndex = fieldIndex
            };
            if (field.IsList)
            {
                var innerStruct = syntax.ChildNodes().OfType<StructDeclarationSyntax>().FirstOrDefault(e=>field.Type.Contains(e.GetStructName()));
                if (innerStruct != null)
                {
                    field.ListFields = innerStruct.GetFields().Select((e, innerFieldIndex) =>
                    {
                        var innerFieldTypeKind = e.GetFieldTypeKind(compilation);
                        var innerField = new PacketFieldModel
                        {
                            Type = e.GetTypeName(),
                            Name = $"{field.Name}[i{field.FieldIndex}].{e.GetFieldName()}",
                            FieldIndex = fieldIndex * 100 + innerFieldIndex,
                            IsEnum = innerFieldTypeKind == TypeKind.Enum,
                        };
                        if (innerFieldTypeKind == TypeKind.Enum) // nested
                        {
                            innerField.Type = $"{field.Type}.{innerField.Type}";
                        }

                        return innerField;
                    }).ToList();
                }
            }
            
            field.TryAddBitsAttribute(fieldSyntax);
            field.TryAddUseIfAttribute(fieldSyntax);
            field.TryAddBoundedRange(fieldSyntax, ref model);

            return field;
            
        }).ToList();
        
        foreach (var field in model.Fields)
        {
            var (ser, deser) = PacketFieldSerDesFactory.GetSerDes(field);
            field.Ser = ser;
            field.Des = deser;
        }

        return model;
    }
}