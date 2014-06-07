using System.CodeDom;
using System.Linq;
using Invert.uFrame.Editor;

public class ViewGenerator : ViewClassGenerator
{
    public ViewData View
    {
        get; set;
    }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        if (View.ViewForElement != null)
        AddView(View);
    }

    public void AddView(ViewData view)
    {
        
        

        var decl = new CodeTypeDeclaration(view.NameAsView);
        decl.IsPartial = true;
        // var baseView = new CodeTypeReference(_diagramData.NameAsViewBase);
        //baseView.TypeArguments.Add(_diagramData.NameAsViewModel);
        
        if (IsDesignerFile)
        {
           
                decl.BaseTypes.Add(new CodeTypeReference(view.BaseViewName));
            
        }
        else
        {
            var bindMethod = new CodeMemberMethod()
            {
                Name = "Bind",
                Attributes = MemberAttributes.Override | MemberAttributes.Public
            };
            decl.Members.Add(bindMethod);
            bindMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Bind"));

        }
        Namespace.Types.Add(decl);
    }
}