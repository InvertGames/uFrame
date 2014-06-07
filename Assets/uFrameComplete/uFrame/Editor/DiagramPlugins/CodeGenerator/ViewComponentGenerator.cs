using System.CodeDom;

public class ViewComponentGenerator : ViewClassGenerator
{

    public ViewComponentData ViewComponentData
    {
        get; set;
    }

    public void AddViewComponent(ViewComponentData componentData)
    {
        var element = componentData.Element;
        if (element == null) return;

        var baseComponent = componentData.Base;

        var ctr = baseComponent == null
            ? new CodeTypeReference(typeof(ViewComponent))
            : new CodeTypeReference(baseComponent.Name);


        var decl = new CodeTypeDeclaration()
        {
            Name = componentData.Name,
            Attributes = MemberAttributes.Public,
            IsPartial = true
        };
        if (IsDesignerFile)
        {
            decl.BaseTypes.Add(ctr);

            if (baseComponent == null)
            {
                var viewModelProperty = new CodeMemberProperty
                {
                    Name = element.Name,
                    Attributes = MemberAttributes.Public,
                    Type = new CodeTypeReference(element.NameAsViewModel),
                    HasGet = true,
                    HasSet = false
                };

                viewModelProperty.GetStatements.Add(new CodeMethodReturnStatement(
                    new CodeCastExpression(viewModelProperty.Type, new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "View"), "ViewModelObject"))
                    ));
                decl.Members.Add(viewModelProperty);
                AddExecuteMethods(element, decl, true);
            }
        }
        Namespace.Types.Add(decl);
    }

}