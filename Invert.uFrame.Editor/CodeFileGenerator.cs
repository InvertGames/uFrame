using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CSharp;

namespace Invert.uFrame.Editor
{
    public class CodeFileGenerator
    {
        public CodeNamespace Namespace { get; set; }
        public CodeCompileUnit Unit { get; set; }

        public bool RemoveComments { get; set; }
        public string NamespaceName { get; set; }
        public CodeFileGenerator(string ns = null)
        {
            NamespaceName = ns;
        }

        public void Generate()
        {
            AddImports();
        }
        public CodeGenerator[] Generators
        {
            get;
            set;
        }

        public string Filename { get; set; }

        public virtual void AddImports()
        {
            Namespace.Imports.Add(new CodeNamespaceImport("System"));
            Namespace.Imports.Add(new CodeNamespaceImport("System.Collections"));
            Namespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
        }

        public override string ToString()
        {
            RemoveComments = Generators.Any(p => !p.IsDesignerFile);

            Namespace = new CodeNamespace(NamespaceName);
            Unit = new CodeCompileUnit();
            Unit.Namespaces.Add(Namespace);
            AddImports();
            foreach (var codeGenerator in Generators)
            {
                codeGenerator.Initialize(this);
            }
            var provider = new CSharpCodeProvider();

            var sb = new StringBuilder();
            var tw1 = new IndentedTextWriter(new StringWriter(sb), "    ");
            provider.GenerateCodeFromCompileUnit(Unit, tw1, new CodeGeneratorOptions());
            tw1.Close();
            if (RemoveComments)
            {
                var removedLines = sb.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Skip(10).ToArray();
                return string.Join(Environment.NewLine, removedLines);
            }
            return sb.ToString();
        }

        public bool CanGenerate(FileInfo fileInfo)
        {
            if (Generators.Any(p => p.IsDesignerFile)) return true;
            var doesTypeExist = Generators.Any(p => p.RelatedType != null);

            if (doesTypeExist || fileInfo.Exists)
            {
                UnityEngine.Debug.Log("Skipping file: " + fileInfo.FullName);
                return false;
            }

            return true;
        }
    }
}