using System.CodeDom;
using System.Collections.Generic;

public class ViewModelFileGenerator : ElementGenerator
{
    public ViewModelFileGenerator(ElementDesignerData diagramData) : base(diagramData)
    {
        
    }
    public CodeMemberProperty ToCommandCodeMemberProperty(ViewModelCommandData itemData)
    {
        var property = new CodeMemberProperty();
        property.Name = itemData.Name;
        property.Attributes = MemberAttributes.Public;
        //if (string.IsNullOrEmpty(_parameterType))
        //{

        property.Type = new CodeTypeReference(typeof(ICommand));
        //}
        //else
        //{
        //    property.Type = new CodeTypeReference(typeof(ICommandWith<>));
        //    if (ParameterType != null)
        //    {
        //        property.Type.TypeArguments.Add(new CodeTypeReference(ParameterType));
        //    }
        //    else
        //    {
        //        property.Type.TypeArguments.Add(new CodeTypeReference(RelatedTypeName));
        //    }
        //}

        property.GetStatements.Add(
            new CodeMethodReturnStatement(new CodeSnippetExpression(string.Format("{0}", itemData.FieldName))));

        property.SetStatements.Add(new CodeSnippetExpression(string.Format("{0} = value", itemData.FieldName)));
        return property;
    }

    public CodeMemberField ToCommandCodeMemberField(ViewModelCommandData itemData)
    {
        var property = new CodeMemberField();
        property.Name = itemData.FieldName;

        // if (string.IsNullOrEmpty(_parameterType))
        // {

        property.Type = new CodeTypeReference(typeof(ICommand));
        //}
        //else
        //{
        //    property.Type = new CodeTypeReference(typeof(ICommandWith<>));
        //    if (ParameterType != null)
        //    {
        //        property.Type.TypeArguments.Add(new CodeTypeReference(ParameterType));
        //    }
        //    else
        //    {
        //        property.Type.TypeArguments.Add(new CodeTypeReference(RelatedTypeName));
        //    }
        //}

        return property;

    }

    public CodeMemberField ToCodeMemberField(ViewModelPropertyData itemData)
    {
        //return new CodeSnippetStatement(string.Format("public readonly P<{0}> {1} = new P<{0}();",RelatedTypeName,FieldName));
        var field = new CodeMemberField { Name = itemData.FieldName };

        field.Attributes = MemberAttributes.Public;

        var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
        var relatedType = typeViewModel == null ? itemData.RelatedTypeName : typeViewModel.NameAsViewModel;


        field.Type = new CodeTypeReference(string.Format("readonly P<{0}>", relatedType));
        var t = new CodeTypeReference(typeof(P<>));
        t.TypeArguments.Add(new CodeTypeReference(relatedType));
        field.InitExpression = new CodeObjectCreateExpression(t);
        return field;

    }

    public CodeMemberProperty ToCodeMemberProperty(ViewModelPropertyData itemData)
    {
        var property = new CodeMemberProperty();
        property.Name = itemData.Name;
        property.Attributes = MemberAttributes.Public;

        var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);

        if (typeViewModel == null)
        {
            property.Type = new CodeTypeReference(itemData.RelatedTypeName);
        }
        else
        {
            property.Type = new CodeTypeReference(typeViewModel.NameAsViewModel);

        }
        property.GetStatements.Add(
            new CodeMethodReturnStatement(new CodeSnippetExpression(string.Format("{0}.Value", itemData.FieldName))));

        property.SetStatements.Add(new CodeSnippetExpression(string.Format("{0}.Value = value", itemData.FieldName)));

        return property;

    }

    public CodeMemberField ToCollectionCodeMemberField(ViewModelCollectionData itemData)
    {
        var field = new CodeMemberField { Name = itemData.FieldName };

        field.Attributes = MemberAttributes.Public;
        var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);

        var relatedType = typeViewModel == null ? itemData.RelatedTypeName : typeViewModel.NameAsViewModel;

        field.Type = new CodeTypeReference(string.Format("readonly ModelCollection<{0}>", relatedType));

        var t = new CodeTypeReference(typeof(ModelCollection<>));
        t.TypeArguments.Add(new CodeTypeReference(relatedType));
        field.InitExpression = new CodeObjectCreateExpression(t);

        return field;

    }

    public CodeMemberProperty ToCollectionCodeMemberProperty(ViewModelCollectionData itemData)
    {
        var property = new CodeMemberProperty();
        property.Name = itemData.Name;
        property.Type = new CodeTypeReference(typeof(ICollection<>));
        property.Attributes = MemberAttributes.Public;
        var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
        if (typeViewModel == null)
        {
            property.Type.TypeArguments.Add(itemData.RelatedTypeName);
        }
        else
        {
            property.Type.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
        }

        property.GetStatements.Add(
            new CodeMethodReturnStatement(new CodeSnippetExpression(string.Format("{0}", itemData.FieldName))));

        property.SetStatements.Add(new CodeSnippetExpression(string.Format("{0}.Value = value.ToList()", itemData.FieldName)));

        return property;

    }

    public void AddEnum(EnumData data)
    {
        var enumDecleration = new CodeTypeDeclaration(data.Name);
        enumDecleration.IsEnum = true;
        foreach (var item in data.EnumItems)
        {
            enumDecleration.Members.Add(new CodeMemberField(enumDecleration.Name, item.Name));
        }
        Namespace.Types.Add(enumDecleration);
    }

    public void AddViewModel(ElementData data)
    {
        var tDecleration = new CodeTypeDeclaration(data.NameAsViewModel);

        if (IsDesignerFile)
        {
            tDecleration.BaseTypes.Add(string.Format("{0}ViewModel", data.BaseTypeShortName.Replace("ViewModel", "")));
            tDecleration.CustomAttributes.Add(
                new CodeAttributeDeclaration(new CodeTypeReference(typeof(DiagramInfoAttribute)),
                    new CodeAttributeArgument(new CodePrimitiveExpression(DiagramData.name))));
        }

        tDecleration.IsPartial = true;
        if (IsDesignerFile)
        {
            // Now Generator code here
            foreach (var viewModelPropertyData in data.Properties)
            {
                tDecleration.Members.Add(ToCodeMemberField(viewModelPropertyData));
                tDecleration.Members.Add(ToCodeMemberProperty(viewModelPropertyData));
            }
            foreach (var viewModelPropertyData in data.Collections)
            {
                tDecleration.Members.Add(ToCollectionCodeMemberField(viewModelPropertyData));
                tDecleration.Members.Add(ToCollectionCodeMemberProperty(viewModelPropertyData));
            }
            foreach (var viewModelPropertyData in data.Commands)
            {
                tDecleration.Members.Add(ToCommandCodeMemberField(viewModelPropertyData));
                tDecleration.Members.Add(ToCommandCodeMemberProperty(viewModelPropertyData));
            }
        }

        Namespace.Types.Add(tDecleration);
    }
}