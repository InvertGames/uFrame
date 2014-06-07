using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CSharp;

public class ElementGeneratorBase
{
    public ElementGeneratorBase()
    {
        Namespace = new CodeNamespace(typeof(ViewModel).Namespace);
        Unit = new CodeCompileUnit();
        Namespace.Imports.Add(new CodeNamespaceImport("System"));
        Namespace.Imports.Add(new CodeNamespaceImport("System.Collections"));
        Namespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
        Namespace.Imports.Add(new CodeNamespaceImport("UnityEngine"));
        Unit.Namespaces.Add(Namespace);
    }

    public bool IsDesignerFile { get; set; }
    public CodeNamespace Namespace { get; set; }
    public CodeCompileUnit Unit { get; set; }

    public override string ToString()
    {
        var provider = new CSharpCodeProvider();

        var sb = new StringBuilder();
        var tw1 = new IndentedTextWriter(new StringWriter(sb), "    ");
        provider.GenerateCodeFromCompileUnit(Unit, tw1, new CodeGeneratorOptions());
        tw1.Close();
        if (!IsDesignerFile)
        {
            var removedLines = sb.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Skip(9).ToArray();
            return string.Join(Environment.NewLine, removedLines);
        }
        return sb.ToString();
    }
}