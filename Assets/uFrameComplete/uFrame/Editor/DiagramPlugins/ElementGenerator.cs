using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using UnityEngine;

public class ElementGenerator : ElementGeneratorBase
{
    private ElementDesignerData _diagramData;

    public ElementDesignerData DiagramData
    {
        get { return _diagramData; }
        set { _diagramData = value; }
    }

    public ElementGenerator(ElementDesignerData diagramData)
    {
        _diagramData = diagramData;
    }

   
}

public class ViewFileGenerator : ElementGenerator
{
    public ViewFileGenerator(ElementDesignerData diagramData) : base(diagramData)
    {
    }

    public CodeConditionStatement AddBindingCondition(CodeTypeDeclaration decl, CodeStatementCollection statements, IViewModelItem item, ElementDataBase relatedElement)
    {
        var bindField = new CodeMemberField
        {
            Name = "_Bind" + item.Name,
            Type = new CodeTypeReference(typeof(bool)),
            Attributes = MemberAttributes.Public
        };

        bindField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(UFToggleGroup)),
            new CodeAttributeArgument(new CodePrimitiveExpression(item.Name))));
        bindField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(HideInInspector))));

        var prop = item as ViewModelPropertyData;
        var coll = item as ViewModelCollectionData;
        if (prop != null)
        {
            if (relatedElement == null)
            {
                bindField.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(UFRequireInstanceMethod)),
                        new CodeAttributeArgument(new CodePrimitiveExpression(prop.NameAsChangedMethod))));
            }

        }
        else if (coll != null)
        {
            //bindField.CustomAttributes.Add(
            //  new CodeAttributeDeclaration(new CodeTypeReference(typeof(UFRequireInstanceMethod)),
            //      new CodeAttributeArgument(new CodePrimitiveExpression(relatedElement == null ? coll.NameAsAddHandler : coll.NameAsCreateHandler))));
        }

        decl.Members.Add(bindField);
        var conditionStatement = new CodeConditionStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), bindField.Name));
        statements.Add(conditionStatement);
        return conditionStatement;
    }

    public void AddView(ViewData view)
    {
        var data = DiagramData.AllDiagramItems.FirstOrDefault(p => p.AssemblyQualifiedName == view.ForAssemblyQualifiedName);
        if (data == null) return;

        var decl = new CodeTypeDeclaration(view.NameAsView);
        decl.IsPartial = true;
        // var baseView = new CodeTypeReference(_diagramData.NameAsViewBase);
        //baseView.TypeArguments.Add(_diagramData.NameAsViewModel);
        var elementData = data as ElementData;
        if (IsDesignerFile)
        {
            if (elementData == null)
            {
                decl.BaseTypes.Add(new CodeTypeReference(data.Name));
            }
            else
            {
                decl.BaseTypes.Add(new CodeTypeReference(elementData.NameAsViewBase));
            }
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

    public void AddViewBase(ElementData data)
    {
        var decl = new CodeTypeDeclaration(data.NameAsViewBase)
        {
            TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public
        };
        if (data.IsControllerDerived)
        {
            var baseType = DiagramData.AllElements.First(p => p.Name == data.BaseTypeShortName);
            decl.BaseTypes.Add(new CodeTypeReference(baseType.NameAsViewBase));
        }
        else
        {
            decl.BaseTypes.Add(new CodeTypeReference(typeof(ViewBase)));
        }

        var viewModelTypeProperty = new CodeMemberProperty
        {
            Attributes = MemberAttributes.Override | MemberAttributes.Public,
            Type = new CodeTypeReference(typeof(Type)),
            Name = "ViewModelType"
        };
        viewModelTypeProperty.HasSet = false;
        viewModelTypeProperty.HasGet = true;
        viewModelTypeProperty.GetStatements.Add(
            new CodeSnippetExpression(string.Format("return typeof({0})", data.NameAsViewModel)));
        decl.Members.Add(viewModelTypeProperty);


        AddComponentReferences(decl, data);

        var multiInstanceProperty = new CodeMemberProperty
        {
            Attributes = MemberAttributes.Override | MemberAttributes.Public,
            Type = new CodeTypeReference(typeof(bool)),
            Name = "IsMultiInstance",
            HasSet = false,
            HasGet = true
        };
        multiInstanceProperty.GetStatements.Add(
            new CodeSnippetExpression(string.Format("return {0}", data.IsMultiInstance ? "true" : "false")));
        decl.Members.Add(multiInstanceProperty);
        //var tViewModelTypeParameter = new CodeTypeParameter("TViewModel") { };
        //tViewModelTypeParameter.Constraints.Add(new CodeTypeReference(_diagramData.NameAsViewModel));
        //decl.TypeParameters.Add(tViewModelTypeParameter);
        if (DiagramData.GenerateViewBindings)
        {
            GenerateBindMethod(decl, data);
        }

        var viewModelProperty = new CodeMemberProperty { Name = data.Name, Attributes = MemberAttributes.Public | MemberAttributes.Final, Type = new CodeTypeReference(data.NameAsViewModel) };
        viewModelProperty.GetStatements.Add(
            new CodeMethodReturnStatement(new CodeCastExpression(new CodeTypeReference(data.NameAsViewModel),
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ViewModelObject"))));
        viewModelProperty.SetStatements.Add(
            new CodeAssignStatement(
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ViewModelObject"),
                new CodePropertySetValueReferenceExpression()));
        decl.Members.Add(viewModelProperty);

        var initializeViewModelMethod = new CodeMemberMethod
        {
            Name = "InitializeViewModel",
            Attributes = MemberAttributes.Override | MemberAttributes.Family
        };

        initializeViewModelMethod.Parameters.Add(
            new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(ViewModel)), "viewModel"));

        if (data.IsControllerDerived)
        {
            var baseCall = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(),
                initializeViewModelMethod.Name);
            baseCall.Parameters.Add(new CodeVariableReferenceExpression("viewModel"));
            initializeViewModelMethod.Statements.Add(baseCall);
        }

        if (data.Properties.Count > 0)
            initializeViewModelMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeTypeReference(data.NameAsViewModel), data.NameAsVariable, new CodeCastExpression(
                        new CodeTypeReference(data.NameAsViewModel),
                        new CodeVariableReferenceExpression("viewModel"))));

        var createModelMethod = new CodeMemberMethod()
        {
            Name = "CreateModel",
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            ReturnType = new CodeTypeReference(typeof(ViewModel))
        };

        //if (data.IsMultiInstance)
        //{
            createModelMethod.Statements.Add(
                new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                    "RequestViewModel",
                    new CodeSnippetExpression(string.Format("GameManager.Container.Resolve<{0}>()",
                        data.NameAsController)))));
        //}
        //else
        //{
        //    createModelMethod.Statements.Add(
        //        new CodeMethodReturnStatement(
        //            new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "ResolveViewModel",
        //                new CodeSnippetExpression(string.Format("GameManager.Container.Resolve<{0}>()",
        //                data.NameAsController)))));
        //}

        decl.Members.Add(createModelMethod);

        foreach (var property in data.Properties)
        {
            var relatedViewModel = DiagramData.GetViewModel(property.RelatedTypeName);
            if (relatedViewModel == null) // Non ViewModel Properties
            {
                var field = new CodeMemberField(new CodeTypeReference(property.RelatedTypeName),
                    property.ViewFieldName) { Attributes = MemberAttributes.Public };
                field.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(UFGroup)), new CodeAttributeArgument(new CodePrimitiveExpression("View Model Properties"))));
                field.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(HideInInspector))));
                decl.Members.Add(field);

                initializeViewModelMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(
                    new CodeVariableReferenceExpression(data.NameAsVariable), property.Name),
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), property.ViewFieldName)));
            }
            else // ViewModel Properties
            {
                var field = new CodeMemberField(new CodeTypeReference(relatedViewModel.NameAsViewBase),
                    property.ViewFieldName) { Attributes = MemberAttributes.Public };

                field.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(UFGroup)), new CodeAttributeArgument(new CodePrimitiveExpression("View Model Properties"))));
                field.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(HideInInspector))));
                decl.Members.Add(field);
                initializeViewModelMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(
                    new CodeVariableReferenceExpression(data.NameAsVariable), property.Name),
                    new CodeSnippetExpression(string.Format("this.{0} == null ? null : this.{0}.ViewModelObject as {1}", property.ViewFieldName, relatedViewModel.NameAsViewModel))));
            }
        }
        foreach (var viewModelCommandData in data.Commands)
        {
            var executeMethod = new CodeMemberMethod { Name = viewModelCommandData.NameAsExecuteMethod, Attributes = MemberAttributes.Public };
            var relatedElement = DiagramData.GetElement(viewModelCommandData);
            if (relatedElement == null)
            {
                if (viewModelCommandData.RelatedTypeName != null)
                {
                    executeMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                        viewModelCommandData.RelatedTypeName, "arg"));

                    executeMethod.Statements.Add(new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(), "ExecuteCommand",
                        new CodeSnippetExpression(string.Format("{0}.{1}", data.Name, viewModelCommandData.Name)),
                        new CodeVariableReferenceExpression("arg")
                        ));
                }
                else
                {
                    executeMethod.Statements.Add(new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(), "ExecuteCommand",
                        new CodeSnippetExpression(string.Format("{0}.{1}", data.Name, viewModelCommandData.Name))
                        ));
                }
            }
            else
            {
                executeMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                    relatedElement.NameAsViewModel, relatedElement.NameAsVariable));

                executeMethod.Statements.Add(new CodeMethodInvokeExpression(
                    new CodeThisReferenceExpression(), "ExecuteCommand",
                    new CodeSnippetExpression(string.Format("{0}.{1}", data.Name, viewModelCommandData.Name)),
                    new CodeVariableReferenceExpression(relatedElement.NameAsVariable)
                    ));
            }
            decl.Members.Add(executeMethod);
        }
        decl.Members.Add(initializeViewModelMethod);
        Namespace.Types.Add(decl);
    }

    private void AddComponentReferences(CodeTypeDeclaration decl, ElementData data)
    {
        if (data.IsControllerDerived) return;

        foreach (var viewComponentData in data.ViewComponents)
        {
           
            var backingField = new CodeMemberField(viewComponentData.Name, "_" + viewComponentData.Name);
            backingField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof (SerializeField))));
            var property = new CodeMemberProperty()
            {
                Type = new CodeTypeReference(viewComponentData.Name),
                Name = viewComponentData.Name,
                Attributes = MemberAttributes.Public,
                HasGet = true,
                HasSet = true
            };

            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression(
                string.Format("{0} ?? ({0} = GetComponent<{1}>())", backingField.Name, viewComponentData.Name))));
            property.SetStatements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingField.Name),
                    new CodePropertySetValueReferenceExpression()));
            decl.Members.Add(backingField);
            decl.Members.Add(property);
        }
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
            }
        }
        Namespace.Types.Add(decl);
    }

    public void AddPropertyBinding(CodeTypeDeclaration decl, string modelName, CodeStatementCollection statements, ViewModelPropertyData propertyData, bool asTwoWay, ElementDataBase relatedElement)
    {
        var memberInvoke = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BindProperty");

        memberInvoke.Parameters.Add(
            new CodeSnippetExpression(string.Format("()=>{0}.{1}", modelName, propertyData.FieldName)));
        memberInvoke.Parameters.Add(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), propertyData.NameAsChangedMethod));

        if (!asTwoWay)
        {
            var setterMethod = new CodeMemberMethod { Name = propertyData.NameAsChangedMethod };
            setterMethod.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference(relatedElement == null ? propertyData.RelatedTypeName : relatedElement.NameAsViewModel), "value"));
            setterMethod.Attributes = MemberAttributes.Public;

            if (relatedElement == null)
            {
                //setterMethod.Statements.Add(
                //    new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof (NotImplementedException))));
            }
            else
            {
                var viewPrefabField = AddPropertyBindingField(decl, typeof(GameObject).FullName, propertyData.Name, "Prefab");
                setterMethod.Statements.Add(new CodeConditionStatement(new CodeSnippetExpression("value == null"),
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(null,
                            "Destroy",
                            new CodeSnippetExpression(string.Format("{0}.gameObject", propertyData.ViewFieldName))))));

                var prefabSetCondition =
                    new CodeConditionStatement(
                        new CodeSnippetExpression(String.Format((string) "{0} == null", (object) viewPrefabField.Name)));

                prefabSetCondition.TrueStatements.Add(new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), propertyData.ViewFieldName),
                    new CodeCastExpression(new CodeTypeReference(relatedElement.NameAsViewBase),
                        new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InstantiateView",
                            new CodeVariableReferenceExpression("value"))
                        )));

                prefabSetCondition.FalseStatements.Add(new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), propertyData.ViewFieldName),
                    new CodeCastExpression(new CodeTypeReference(relatedElement.NameAsViewBase),
                        new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InstantiateView",
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), viewPrefabField.Name),
                            new CodeVariableReferenceExpression("value"))
                        )));
                setterMethod.Statements.Add(prefabSetCondition);
            }

            decl.Members.Add(setterMethod);
        }
        else
        {
            memberInvoke.Parameters.Add(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), propertyData.NameAsTwoWayMethod));

            var getterMethod = new CodeMemberMethod
            {
                Name = propertyData.NameAsTwoWayMethod,
                ReturnType = new CodeTypeReference(relatedElement == null ? propertyData.RelatedTypeName : relatedElement.NameAsViewModel),
                Attributes = MemberAttributes.Public
            };
            if (relatedElement == null)
            {
                getterMethod.Statements.Add(
                    new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(NotImplementedException))));
            }
            else
            {
                getterMethod.Statements.Add(new CodeConditionStatement(
                    new CodeSnippetExpression(
                        string.Format("this.{0} == null || this.{0}.ViewModelObject == null", propertyData.ViewFieldName)),
                    new CodeMethodReturnStatement(new CodeSnippetExpression("null"))
                    ));

                getterMethod.Statements.Add(new CodeMethodReturnStatement(new CodeCastExpression(
                    new CodeTypeReference(relatedElement.NameAsViewModel),
                    new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), propertyData.ViewFieldName),
                        "ViewModelObject")
                    )));
            }

            decl.Members.Add(getterMethod);
        }

        //    public virtual void CurrentPlayerChanged(FPSPlayerViewModel value) {
        //    if (value == null)
        //        Destroy(this._CurrentPlayer.gameObject);
        //    this._CurrentPlayer = ((FPSPlayerViewBase)(this.InstantiateView(value)));
        //}

        //public virtual FPSPlayerViewModel GetCurrentPlayerTwoWayValue()
        //{
        //    if (this._CurrentPlayer == null || this._CurrentPlayer.ViewModelObject == null) return null;
        //    return ((FPSPlayerViewModel)(this._CurrentPlayer.ViewModelObject));
        //}

        statements.Add(memberInvoke);
    }

    public CodeMemberField AddPropertyBindingField(CodeTypeDeclaration decl, string typeFullName, string propertyName, string name, bool keepHidden = false)
    {
        var memberField =
            new CodeMemberField(
                typeFullName,
                "_" + propertyName + name) { Attributes = MemberAttributes.Public };
        if (!keepHidden)
        {
            memberField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(UFGroup)),
                new CodeAttributeArgument(new CodePrimitiveExpression(propertyName))));
        }

        memberField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(HideInInspector))));

        decl.Members.Add(memberField);
        return memberField;
    }

    public void CreateViewPropertyBinding(string modelName, CodeStatementCollection statements,
        ViewModelPropertyData propertyData)
    {
        statements.Add(
            new CodeSnippetExpression(string.Format("this.BindToView(() => {0}.{1}, v => {2} = v, () => {2})",
                modelName, propertyData.FieldName,
                propertyData.ViewFieldName)));
    }

    private void AddCollectionBinding(CodeTypeDeclaration decl, string modelName, CodeStatementCollection statements, ViewModelCollectionData collectionProperty, ElementDataBase relatedElement)
    {
        var varTypeName = relatedElement == null ? collectionProperty.RelatedTypeName : relatedElement.NameAsViewModel;
        var parameterTypeName = relatedElement == null ? collectionProperty.RelatedTypeName : relatedElement.NameAsViewBase;
        var varName = relatedElement == null ? "item" : relatedElement.NameAsVariable;

        var addHandlerMethod = new CodeMemberMethod()
        {
            Attributes = MemberAttributes.Public,
            Name = collectionProperty.Name + "Added",
        };
        addHandlerMethod.Parameters.Add(new CodeParameterDeclarationExpression(parameterTypeName, varName));

        decl.Members.Add(addHandlerMethod);
        var removeHandlerMethod = new CodeMemberMethod()
        {
            Attributes = MemberAttributes.Public,
            Name = collectionProperty.Name + "Removed"
        };
        removeHandlerMethod.Parameters.Add(new CodeParameterDeclarationExpression(parameterTypeName, varName));
        if (relatedElement != null)
        {
            var listField = AddPropertyBindingField(decl, typeof(List<ViewModel>).FullName.Replace("ViewModel", relatedElement.NameAsViewBase), collectionProperty.Name, "List", true);
            addHandlerMethod.Statements.Add(new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), listField.Name), "Add",
                new CodeVariableReferenceExpression(relatedElement.NameAsVariable)));
            removeHandlerMethod.Statements.Add(new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), listField.Name), "Remove",
                new CodeVariableReferenceExpression(relatedElement.NameAsVariable)));
            removeHandlerMethod.Statements.Add(new CodeSnippetExpression(string.Format("UnityEngine.Object.Destroy({0}.gameObject)", relatedElement.NameAsVariable)));
        }

        decl.Members.Add(removeHandlerMethod);

        if (relatedElement != null)
        {
            var createHandlerMethod = new CodeMemberMethod()
            {
                Attributes = MemberAttributes.Public,
                Name = collectionProperty.NameAsCreateHandler,
                ReturnType = new CodeTypeReference(typeof(ViewBase))
            };
            createHandlerMethod.Statements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InstantiateView", new CodeVariableReferenceExpression(varName))));
            createHandlerMethod.Parameters.Add(new CodeParameterDeclarationExpression(varTypeName, varName));
            decl.Members.Add(createHandlerMethod);

            statements.Add(
                new CodeSnippetExpression(string.Format("var binding = this.BindToViewCollection(() => {0}.{1})", modelName,
                    collectionProperty.FieldName)));

            statements.Add(
                new CodeSnippetExpression(string.Format("binding.SetAddHandler(item=>{0}(item as {1}))",
                    addHandlerMethod.Name, relatedElement.NameAsViewBase)));
            statements.Add(
                new CodeSnippetExpression(string.Format("binding.SetRemoveHandler(item=>{0}(item as {1}))",
                    removeHandlerMethod.Name, relatedElement.NameAsViewBase)));
            statements.Add(
                new CodeSnippetExpression(
                    string.Format("binding.SetCreateHandler(viewModel=>{{ return {0}(viewModel as {1}); }}); ",
                        createHandlerMethod.Name, relatedElement.NameAsViewModel)));
        }
        else
        {
            statements.Add(
                new CodeSnippetExpression(string.Format("var binding = this.BindCollection(() => {0}.{1})", modelName,
                    collectionProperty.FieldName)));

            statements.Add(
                new CodeSnippetExpression(string.Format("binding.SetAddHandler({0})",
                    addHandlerMethod.Name)));
            statements.Add(
                new CodeSnippetExpression(string.Format("binding.SetRemoveHandler({0})",
                    removeHandlerMethod.Name)));
        }

        if (relatedElement != null)
        {

            var sceneFirstField = AddPropertyBindingField(decl, typeof(bool).FullName, collectionProperty.Name, "SceneFirst");
            //sceneFirstField.CustomAttributes.Add(
            //new CodeAttributeDeclaration(new CodeTypeReference(typeof(UFRequireInstanceMethod)),
            //    new CodeAttributeArgument(new CodePrimitiveExpression(collectionProperty.NameAsCreateHandler))));

            var containerField = AddPropertyBindingField(decl, typeof(Transform).FullName, collectionProperty.Name, "Container");
            //containerField.CustomAttributes.Add(
            //   new CodeAttributeDeclaration(new CodeTypeReference(typeof(UFRequireInstanceMethod)),
            //       new CodeAttributeArgument(new CodePrimitiveExpression(collectionProperty.NameAsCreateHandler))));
            var containerNullCondition =
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(containerField.Name),
                        CodeBinaryOperatorType.ValueEquality, new CodeSnippetExpression("null")));

            containerNullCondition.FalseStatements.Add(new CodeSnippetExpression(string.Format("binding.SetParent({0})", containerField.Name)));
            statements.Add(containerNullCondition);

            var sceneFirstCondition =
                new CodeConditionStatement(
                    new CodeVariableReferenceExpression(sceneFirstField.Name));

            sceneFirstCondition.TrueStatements.Add(new CodeSnippetExpression("binding.ViewFirst()"));
            statements.Add(sceneFirstCondition);
        }

        //this.BindToViewCollection(() => Model._WeaponsProperty)
        //            .SetAddHandler((weapon) =>
        //            {
        //                // set the rotation to the camera rotation
        //                weapon.transform.localPosition = Vector3.zero;
        //                weapon.transform.localRotation = Quaternion.identity;
        //                _Weapons.Add(weapon);
        //            })
        //            .SetParent(_GunsTransform).ViewFirst();
    }

    private void GenerateBindMethod(CodeTypeDeclaration decl, ElementData data)
    {
        var bindMethod = new CodeMemberMethod
        {
            Name = "Bind",
            Attributes = MemberAttributes.Public | MemberAttributes.Override
        };

        if (data.IsControllerDerived)
        {
            bindMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Bind"));
        }

        foreach (var property in data.Properties)
        {
            var relatedElement = DiagramData.AllElements.FirstOrDefault(p => p.Name == property.RelatedTypeName);
            var bindingCondition = AddBindingCondition(decl, bindMethod.Statements, property, relatedElement);

            var twoWayField = AddPropertyBindingField(decl, typeof(bool).FullName, property.Name, "IsTwoWay");
            twoWayField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(UFRequireInstanceMethod)),
                new CodeAttributeArgument(new CodePrimitiveExpression(property.NameAsTwoWayMethod))));

            var twoWayCondition =
                new CodeConditionStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                    twoWayField.Name));

            bindingCondition.TrueStatements.Add(twoWayCondition);


            AddPropertyBinding(decl, data.Name, twoWayCondition.FalseStatements, property, false, relatedElement);
            AddPropertyBinding(decl, data.Name, twoWayCondition.TrueStatements, property, true, relatedElement);
        }
        foreach (var collectionProperty in data.Collections)
        {
            var relatedElement = DiagramData.AllElements.FirstOrDefault(p => p.Name == collectionProperty.RelatedTypeName);
            var bindingCondition = AddBindingCondition(decl, bindMethod.Statements, collectionProperty, relatedElement);

            if (relatedElement == null)
            {
                AddCollectionBinding(decl, data.Name, bindingCondition.TrueStatements, collectionProperty, relatedElement);
            }
            else
            {
                AddCollectionBinding(decl, data.Name, bindingCondition.TrueStatements, collectionProperty, relatedElement);
            }

        }
        decl.Members.Add(bindMethod);
    }
}

public class ControllerFileGenerator : ElementGenerator
{
    public ControllerFileGenerator(ElementDesignerData diagramData) : base(diagramData)
    {
    }
    public CodeTypeReference GetCommandTypeReference(ViewModelCommandData itemData, CodeTypeReference senderType, ElementData element)
    {
        if (!itemData.IsYield)
        {
            if (string.IsNullOrEmpty(itemData.RelatedTypeName))
            {
                if (element.IsMultiInstance)
                {
                    var commandWithType = new CodeTypeReference(typeof(CommandWithSender<>));
                    commandWithType.TypeArguments.Add(senderType);
                    return commandWithType;
                }
                else
                {
                    var commandWithType = new CodeTypeReference(typeof(Command));
                    return commandWithType;
                }

            }
            else
            {
                if (element.IsMultiInstance)
                {
                    var commandWithType = new CodeTypeReference(typeof(CommandWithSenderAndArgument<,>));
                    commandWithType.TypeArguments.Add(senderType);
                    var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
                    if (typeViewModel == null)
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(itemData.RelatedTypeName));
                    }
                    else
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
                    }

                    return commandWithType;
                }
                else
                {
                    var commandWithType = new CodeTypeReference(typeof(CommandWith<>));

                    var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
                    if (typeViewModel == null)
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(itemData.RelatedTypeName));
                    }
                    else
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
                    }

                    return commandWithType;

                }

            }
        }
        else
        {
            if (string.IsNullOrEmpty(itemData.RelatedTypeName))
            {
                if (element.IsMultiInstance)
                {
                    var commandWithType = new CodeTypeReference(typeof(YieldCommandWithSender<>));
                    commandWithType.TypeArguments.Add(senderType);
                    return commandWithType;
                }
                else
                {
                    var commandWithType = new CodeTypeReference(typeof(YieldCommand));

                    return commandWithType;
                }

            }
            else
            {
                if (element.IsMultiInstance)
                {
                    var commandWithType = new CodeTypeReference(typeof(YieldCommandWithSenderAndArgument<,>));
                    commandWithType.TypeArguments.Add(senderType);
                    var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
                    if (typeViewModel == null)
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(itemData.RelatedTypeName));
                    }
                    else
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
                    }
                    return commandWithType;
                }
                else
                {
                    var commandWithType = new CodeTypeReference(typeof(YieldCommandWith<>));
                    var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
                    if (typeViewModel == null)
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(itemData.RelatedTypeName));
                    }
                    else
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
                    }
                    return commandWithType;
                }

            }
        }


    }
    public CodeTypeDeclaration AddTypeEnum(ElementDataBase rootElement, SceneManagerData sceneManager, ElementDataBase[] elements)
    {
        var derivedElements = rootElement.DerivedElements.Where(elements.Contains).ToArray();
        if (derivedElements.Length < 1) return null;
        var enumDecleration = new CodeTypeDeclaration(sceneManager.NameAsSceneManager + rootElement.NameAsTypeEnum) { IsEnum = true };
        enumDecleration.Members.Add(new CodeMemberField(enumDecleration.Name, rootElement.Name));
        foreach (var item in derivedElements)
        {
            enumDecleration.Members.Add(new CodeMemberField(enumDecleration.Name, item.Name));
        }
        Namespace.Types.Add(enumDecleration);
        return enumDecleration;
    }

    public void AddController(ElementData data)
    {
        var viewModelTypeReference = new CodeTypeReference(data.NameAsViewModel);
        var tDecleration = new CodeTypeDeclaration();

        if (IsDesignerFile)
        {
            AddDependencyControllers(tDecleration, data);
            tDecleration.Name = data.NameAsControllerBase;
            tDecleration.TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public;
            if (data.IsControllerDerived)
            {
                tDecleration.BaseTypes.Add(string.Format("{0}Controller", data.BaseTypeShortName.Replace("ViewModel", "")));
            }
            else
            {
                tDecleration.BaseTypes.Add(new CodeTypeReference(typeof(Controller)));
            }

            if (!data.IsMultiInstance)
            {
                var property = new CodeMemberProperty
                {
                    Name = data.Name,
                    Type = new CodeTypeReference(data.NameAsViewModel),
                    HasGet = true,
                    HasSet = false,
                    Attributes = MemberAttributes.Public
                };
                property.GetStatements.Add(
                    new CodeMethodReturnStatement(
                        new CodeSnippetExpression(string.Format("Container.Resolve<{0}>()", data.NameAsViewModel))));
                tDecleration.Members.Add(property);
            }
        }
        else
        {
            tDecleration.TypeAttributes = TypeAttributes.Public;
            tDecleration.Name = data.ControllerName;
            tDecleration.BaseTypes.Add(new CodeTypeReference(data.NameAsControllerBase));
        }

        var initializeTypedMethod = new CodeMemberMethod { Name = string.Format("Initialize{0}", data.Name) };
        tDecleration.Members.Add(initializeTypedMethod);
        initializeTypedMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(data.NameAsViewModel), data.NameAsVariable));
        if (IsDesignerFile)
        {
            initializeTypedMethod.Attributes = MemberAttributes.Abstract | MemberAttributes.Public;

            AddWireCommandsMethod(data, tDecleration, viewModelTypeReference);
            AddCreateMethod(data, viewModelTypeReference, initializeTypedMethod, tDecleration);
        }
        else
        {
            initializeTypedMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
        }
        if (IsDesignerFile)
        {
            var initializeOverrideMethod = new CodeMemberMethod()
            {
                Name = "Initialize",
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };
            tDecleration.Members.Add(initializeOverrideMethod);
            initializeOverrideMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(ViewModel)), "viewModel"));
            initializeOverrideMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), initializeTypedMethod.Name,
                new CodeCastExpression(new CodeTypeReference(data.NameAsViewModel), new CodeVariableReferenceExpression("viewModel"))));
        }
      

        // Command functions
        if (IsDesignerFile)
            AddCommandMethods(data, viewModelTypeReference, tDecleration);

        Namespace.Types.Add(tDecleration);
    }

    public void AddSceneManager(SceneManagerData sceneManager)
    {
        // Grab the root items
        var subSystem = sceneManager.SubSystem;
        if (subSystem == null)
        {
            Debug.LogWarning(string.Format("Scene Manager {0} doesn't have an associated SubSystem.  To create the type please associate one.", sceneManager.Name));
            return;
        }

        var elements = subSystem.IncludedElements.ToArray();

        var decl = new CodeTypeDeclaration(IsDesignerFile ? sceneManager.NameAsSceneManagerBase : sceneManager.NameAsSceneManager);
        if (IsDesignerFile)
        {
            decl.BaseTypes.Add(new CodeTypeReference(typeof(SceneManager)));

            //var singleInstanceElements = elements.Where(p => !p.IsMultiInstance).ToArray();

            //foreach (var element in singleInstanceElements)
            //{


            //    //Container.RegisterInstance(AICheckersGameController.CreateAICheckersGame());
            //    //progress("Loading CheckersGame", 100);
            //}

        }
        else
        {
            decl.BaseTypes.Add(new CodeTypeReference(sceneManager.NameAsSceneManagerBase));

            var loadMethod = new CodeMemberMethod();
            loadMethod.Name = "Load";
            loadMethod.ReturnType = new CodeTypeReference(typeof(IEnumerator));
            loadMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            loadMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(UpdateProgressDelegate), "progress"));
            loadMethod.Statements.Add(new CodeCommentStatement("Use the controllers to create the game."));
            loadMethod.Statements.Add(new CodeSnippetExpression("yield break"));

            decl.Members.Add(loadMethod);
        }

        var setupMethod = new CodeMemberMethod()
        {
            Name = "Setup"
        };
        setupMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
        setupMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Setup"));
        decl.Members.Add(setupMethod);
        if (IsDesignerFile)
        {
            //var commands = sceneManager.SubSystem.IncludedCommands.ToArray();
            foreach (var SceneManagerTransition in sceneManager.Transitions)
            {
                //commands.Where(p=>p.Identifier == SceneManagerTransition.Id)
                var transitionItem = DiagramData.SceneManagers.FirstOrDefault(p => p.Identifier == SceneManagerTransition.ToIdentifier);
                if (transitionItem == null || transitionItem.SubSystem == null) continue;

                var settingsField = new CodeMemberField(transitionItem.NameAsSettings, SceneManagerTransition.NameAsSettingsField)
                {
                    Attributes = MemberAttributes.Public,
                    InitExpression = new CodeObjectCreateExpression(transitionItem.NameAsSettings)
                };
                decl.Members.Add(settingsField);

                var transitionMethod = new CodeMemberMethod()
                {
                    Name = SceneManagerTransition.Name,
                    Attributes = MemberAttributes.Public
                };

                var switchGameAndLevelCall =
                    new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(GameManager)),
                        String.Format("SwitchGameAndLevel<{0}>", transitionItem.NameAsSceneManager));

                switchGameAndLevelCall.Parameters.Add(
                    new CodeSnippetExpression(string.Format("(container) =>{{container.{0} = {1}; }}",
                        transitionItem.NameAsSettingsField, settingsField.Name)));

                switchGameAndLevelCall.Parameters.Add(new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), settingsField.Name), "_Scenes"));

                transitionMethod.Statements.Add(switchGameAndLevelCall);
                decl.Members.Add(transitionMethod);
                //transitionMethod.Statements.Add(new CodeSnippetExpression(
                //    string.Format(
                //        "GameManager.SwitchGameAndLevel<CheckersMenuManager>((checkersMenu) =>{checkersMenu._CheckersMenuSettings = _QuitTransition;},\"CheckersMenu\")")));
            }


            List<ElementDataBase> rootElements = new List<ElementDataBase>();
            List<ElementDataBase> baseElements = new List<ElementDataBase>();
            foreach (var element in elements)
            {
                decl.Members.Add(
                    new CodeSnippetTypeMember(string.Format("public {0} {0} {{ get; set; }}", element.NameAsController)));
                var property = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), element.NameAsController);

                var assignProperty =
                    new CodeAssignStatement(property,
                        new CodeObjectCreateExpression(element.NameAsController)
                        );
                setupMethod.Statements.Add(assignProperty);

                var registerInstance = new CodeMethodInvokeExpression(
                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Container"), "RegisterInstance");
                registerInstance.Parameters.Add(
                    new CodePropertyReferenceExpression(
                        new CodeThisReferenceExpression(), element.NameAsController));

                registerInstance.Parameters.Add(new CodeSnippetExpression("false"));
                setupMethod.Statements.Add(registerInstance);

                if (element.BaseElement == null && !element.IsMultiInstance)
                {
                    if (AddTypeEnum(element, sceneManager, elements) == null)
                    {
                        baseElements.Add(element);
                        
                    }
                    else
                    {
                        rootElements.Add(element);
                    }

                }
            }
            setupMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Container"),
                    "InjectAll"));

            foreach (var element in baseElements)
            {

                setupMethod.Statements.Add(new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1}.Create{2}(), false)", element.NameAsViewModel, element.NameAsController, element.Name)));
            }

            foreach (var element in rootElements)
            {
                var derived = element.DerivedElements.Where(p => elements.Contains(p)).ToArray();

                foreach (var derivedElement in derived.Concat(new[] { element }))
                {
                    var condition =
                        new CodeConditionStatement(new CodeBinaryOperatorExpression(
                            new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), sceneManager.NameAsSettingsField), element.NameAsTypeEnum
                                ), CodeBinaryOperatorType.ValueEquality,
                            new CodeSnippetExpression(string.Format("{0}.{1}",
                                sceneManager.NameAsSceneManager + element.NameAsTypeEnum, derivedElement.Name))));
                    condition.TrueStatements.Add(
                        new CodeVariableDeclarationStatement(new CodeTypeReference(derivedElement.NameAsViewModel),
                            derivedElement.NameAsVariable, new CodeSnippetExpression(string.Format("{0}.CreateEmpty() as {1}", derivedElement.NameAsController, derivedElement.NameAsViewModel))));

                    condition.TrueStatements.Add(new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1}, false)", derivedElement.NameAsViewModel, derivedElement.NameAsVariable)));
                    // TODO add a while here for each base type
                    foreach (var baseType in derivedElement.AllBaseTypes)
                    {
                        //if (baseType == derivedElement) continue;
                        //condition.TrueStatements.Add(new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1}, false)", baseType.NameAsController, derivedElement.NameAsController)));
                        //condition.TrueStatements.Add(new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1}, false)", element.NameAsController, baseType.NameAsController)));
                        condition.TrueStatements.Add(new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1}, false)", baseType.NameAsViewModel, derivedElement.NameAsVariable)));
                        condition.TrueStatements.Add(new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1}, false)", baseType.NameAsController, derivedElement.NameAsController)));
                    }



                    //if (!derivedElement.IsMultiInstance)
                    //{
                    //    condition.TrueStatements.Add(
                    //        new CodeMethodInvokeExpression(
                    //        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Container"),
                    //        string.Format("RegisterInstance<{0}>", element.NameAsController), new CodeSnippetExpression()));

                    //}


                    setupMethod.Statements.Add(condition);
                }
            }
            AddSceneManagerSettings(decl, sceneManager, rootElements);


        }
        Namespace.Types.Add(decl);
    }

    public void AddSceneManagerSettings(CodeTypeDeclaration container, SceneManagerData SceneManager, List<ElementDataBase> rootElements)
    {
        var decl = new CodeTypeDeclaration()
        {
            Name = SceneManager.NameAsSettings,
            TypeAttributes = TypeAttributes.Sealed | TypeAttributes.Public
        };
        decl.IsPartial = true;
        decl.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializableAttribute))));
        decl.Members.Add(new CodeMemberField(typeof(string[]), "_Scenes") { Attributes = MemberAttributes.Public });

        var settingsField = new CodeMemberField(decl.Name, SceneManager.NameAsSettingsField)
        {
            Attributes = MemberAttributes.Public,
            InitExpression = new CodeObjectCreateExpression(decl.Name)
        };
        container.Members.Add(settingsField);

        foreach (var element in rootElements)
        {
            var field = new CodeMemberField(SceneManager.NameAsSceneManager + element.NameAsTypeEnum,
                element.NameAsTypeEnum) { Attributes = MemberAttributes.Public };
            decl.Members.Add(field);
        }
        Namespace.Types.Add(decl);


    }

    public void AddControllerInitMethod(ElementData data)
    {

    }

    private void AddCreateMethod(ElementData data, CodeTypeReference viewModelTypeReference,
        CodeMemberMethod initializeMethod, CodeTypeDeclaration tDecleration)
    {
        var createMethod = new CodeMemberMethod
        {
            Name = string.Format("Create{0}", data.Name),
            Attributes = MemberAttributes.Public,
            ReturnType = viewModelTypeReference
        };
        createMethod.Statements.Add(
            new CodeMethodReturnStatement(new CodeCastExpression(data.NameAsViewModel,
                new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Create"))));

        //var modelDecleration = new CodeVariableDeclarationStatement
        //{
        //    Name = data.NameAsVariable,
        //    Type = new CodeTypeReference(data.NameAsViewModel)
        //};
        ////createMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Action<ViewModel>),
        ////    "preInitializer = null") { });
        //modelDecleration.InitExpression = new CodeObjectCreateExpression(modelDecleration.Type);
        //createMethod.Statements.Add(modelDecleration);
        //var reference = new CodeVariableReferenceExpression(data.NameAsVariable);
        //var callWireMethod = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression()
        //{
        //    MethodName = "WireCommands",
        //}, reference);

        //var callInitializeMethod = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression()
        //{
        //    MethodName = initializeMethod.Name,
        //}, reference);

        //createMethod.Statements.Add(callWireMethod);
        //createMethod.Statements.Add(callInitializeMethod);
        //createMethod.Statements.Add(
        //    new CodeMethodReturnStatement(reference));

        var createEmptyMethod = new CodeMemberMethod
        {
            Name = "CreateEmpty",
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            ReturnType = new CodeTypeReference(typeof(ViewModel))
        };
        //createOverrideMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof (Action<ViewModel>),
        //    "preInitializer = null") {});
        createEmptyMethod.Statements.Add(
            new CodeMethodReturnStatement(new CodeObjectCreateExpression(data.NameAsViewModel)));


        //createEmptyMethod.Statements.Add(new CodeMethodReturnStatement(
        //    new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), createMethod.Name)));
        tDecleration.Members.Add(createEmptyMethod);

        tDecleration.Members.Add(createMethod);
    }

    private void AddWireCommandsMethod(ElementData data, CodeTypeDeclaration tDecleration,
        CodeTypeReference viewModelTypeReference)
    {
        var wireMethod = new CodeMemberMethod { Name = string.Format("WireCommands") };
        tDecleration.Members.Add(wireMethod);
        wireMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(ViewModel)),
            "viewModel"));
        if (data.IsControllerDerived)
        {
            wireMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            var callBase = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), wireMethod.Name,
                new CodeVariableReferenceExpression("viewModel"));
            wireMethod.Statements.Add(callBase);
        }
        else
        {
            wireMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
        }
        if (data.Commands.Count > 0)
        {
            wireMethod.Statements.Add(
                new CodeSnippetExpression(string.Format("var {0} = viewModel as {1}", data.NameAsVariable, data.NameAsViewModel)));
        }

        foreach (var command in data.Commands)
        {
            var assigner = new CodeAssignStatement();
            assigner.Left = new CodeFieldReferenceExpression(new CodeSnippetExpression(data.NameAsVariable), command.Name);
            var commandWithType = GetCommandTypeReference(command, viewModelTypeReference, data);
            var commandWith = new CodeObjectCreateExpression(commandWithType);
            if (data.IsMultiInstance)
            {
                commandWith.Parameters.Add(new CodeVariableReferenceExpression(data.NameAsVariable));
            }
            commandWith.Parameters.Add(new CodeMethodReferenceExpression() { MethodName = command.Name });
            assigner.Right = commandWith;
            wireMethod.Statements.Add(assigner);
        }
    }

    private void AddCommandMethods(ElementData data, CodeTypeReference viewModelTypeReference,
        CodeTypeDeclaration tDecleration)
    {
        foreach (var command in data.Commands)
        {
            var commandMethod = new CodeMemberMethod
            {
                Name = command.Name,


            };

            if (command.IsYield)
            {
                commandMethod.ReturnType = new CodeTypeReference(typeof(IEnumerator));
            }
            var transition =
                DiagramData.SceneManagers.SelectMany(p => p.Transitions)
                    .FirstOrDefault(p => p.CommandIdentifier == command.Identifier && !string.IsNullOrEmpty(p.ToIdentifier));

            var hasTransition = transition != null && DiagramData.SceneManagers.Any(p => p.Identifier == transition.ToIdentifier);
            if (IsDesignerFile)
            {
                commandMethod.Attributes = MemberAttributes.Public;
            }
            else
            {
                commandMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            }

            if (data.IsMultiInstance)
            {
                commandMethod.Parameters.Add(new CodeParameterDeclarationExpression(viewModelTypeReference,
                    data.NameAsVariable));
            }

            // TODO this should propably be injection but for now whatever
            // Add transition code
            if (IsDesignerFile && hasTransition)
            {
                commandMethod.Attributes = MemberAttributes.Public;

                commandMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                    "GameEvent", new CodePrimitiveExpression(transition.Name)));
            }
            if (IsDesignerFile && command.IsYield)
            {
                commandMethod.Statements.Add(new CodeSnippetExpression("yield break"));
            }
            //if (!IsDesignerFile)
            //{
            //    commandMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(),commandMethod.Name,))
            //}
            if (!string.IsNullOrEmpty(command.RelatedTypeName))
            {
                var relatedViewModel = DiagramData.GetViewModel(command.RelatedTypeName);
                if (relatedViewModel == null)
                {
                    commandMethod.Parameters.Add(
                        new CodeParameterDeclarationExpression(new CodeTypeReference(command.RelatedTypeName), "arg"));
                }
                else
                {
                    commandMethod.Parameters.Add(
                        new CodeParameterDeclarationExpression(new CodeTypeReference(relatedViewModel.NameAsViewModel), "arg"));
                }
            }
            //commandMethod.Statements.Add(commandMethod);

            tDecleration.Members.Add(commandMethod);
        }
    }

    private void AddDependencyControllers(CodeTypeDeclaration tDecleration, ElementData data)
    {
        //var associationLinks = allData.AllElements.SelectMany(p => p.Items)
        //    .SelectMany(p => p.GetLinks(allData))
        //    .OfType<AssociationLink>()
        //    .ToArray();

        var controllers = new List<string>();
        var diagramItems = DiagramData.AllDiagramItems.ToArray();
        foreach (var elementDataBase in DiagramData.AllElements.ToArray())
        {
            var links = elementDataBase.Items.SelectMany(p => p.GetLinks(diagramItems));
            foreach (var diagramLink in links)
            {
                var link = diagramLink as AssociationLink;
                if (link == null) continue;

                if (link.Element == data)
                {
                    var controllerElement = DiagramData.AllElements.FirstOrDefault(p => p.Items.Contains(link.Item));
                    if (controllerElement == null) continue;
                    controllers.Add(controllerElement.NameAsController);
                }
                else if (data.Items.Contains(link.Item))
                {
                    var controllerElement = DiagramData.AllElements.FirstOrDefault(p => p.Name == link.Item.RelatedTypeName);
                    if (controllerElement == null) continue;
                    controllers.Add(controllerElement.NameAsController);
                }
            }
        }
        // tDecleration.Members.Add(new CodeSnippetTypeMember(string.Format("[Inject] public {0} {0} {{get;set;}}", element.NameAsController)));
        foreach (var controller in controllers.Distinct())
        {
            tDecleration.Members.Add(new CodeSnippetTypeMember(string.Format("[Inject] public {0} {0} {{get;set;}}", controller)));
        }
    }
}

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