using System.CodeDom;
using System.Collections.Generic;
using Invert.uFrame.Editor;

public class ViewModelGenerator : CodeGenerator
{
    public ViewModelGenerator(bool isDesignerFile, ElementData data)
    {
        IsDesignerFile = isDesignerFile;
        Data = data;
    }

    public ElementDesignerData DiagramData
    {
        get; set;
    }

    public ElementData Data
    {
        get;
        set;
    }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        AddViewModel(Data);
    }
    
    public virtual CodeMemberProperty ToCommandCodeMemberProperty(ViewModelCommandData itemData)
    {
        var property = new CodeMemberProperty
        {
            Name = itemData.Name,
            Attributes = MemberAttributes.Public,
            Type = new CodeTypeReference(typeof (ICommand))
        };

        property.GetStatements.Add(
            new CodeMethodReturnStatement(new CodeSnippetExpression(string.Format("{0}", itemData.FieldName))));

        property.SetStatements.Add(new CodeSnippetExpression(string.Format("{0} = value", itemData.FieldName)));
        return property;
    }

    public virtual CodeMemberField ToCommandCodeMemberField(ViewModelCommandData itemData)
    {
        var property = new CodeMemberField();
        property.Name = itemData.FieldName;
        property.Type = new CodeTypeReference(typeof(ICommand));
        return property;
    }

    public virtual CodeMemberField ToCodeMemberField(ViewModelPropertyData itemData)
    {
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

    public virtual CodeMemberProperty ToCodeMemberProperty(ViewModelPropertyData itemData)
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

    public virtual CodeMemberField ToCollectionCodeMemberField(ViewModelCollectionData itemData)
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

    public virtual CodeMemberProperty ToCollectionCodeMemberProperty(ViewModelCollectionData itemData)
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

    public virtual void AddViewModel(ElementData data)
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