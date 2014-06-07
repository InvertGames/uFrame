using System.CodeDom;
using Invert.uFrame.Editor;

public class EnumCodeGenerator : CodeGenerator
{
    public EnumData EnumData { get; set; }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        AddEnum(EnumData);
    }

    public void AddEnum(EnumData data)
    {
        var enumDecleration = new CodeTypeDeclaration(data.Name) {IsEnum = true};
        foreach (var item in data.EnumItems)
        {
            enumDecleration.Members.Add(new CodeMemberField(enumDecleration.Name, item.Name));
        }
        Namespace.Types.Add(enumDecleration);
    }
}