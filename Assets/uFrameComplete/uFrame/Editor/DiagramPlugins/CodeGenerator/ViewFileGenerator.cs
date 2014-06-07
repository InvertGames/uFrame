using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ViewFileGenerator : ElementGenerator
{
    public ViewFileGenerator(ElementDesignerData diagramData)
        : base(diagramData)
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

        GenerateBindMethod(decl, data);


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
            Attributes = MemberAttributes.Override | MemberAttributes.Public
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
        AddExecuteMethods(data, decl);
        decl.Members.Add(initializeViewModelMethod);
        Namespace.Types.Add(decl);
    }

    private void AddExecuteMethods(ElementData data, CodeTypeDeclaration decl, bool useViewReference = false)
    {
        foreach (var viewModelCommandData in data.Commands)
        {
            var executeMethod = new CodeMemberMethod
            {
                Name = viewModelCommandData.NameAsExecuteMethod,
                Attributes = MemberAttributes.Public
            };

            CodeExpression executeCommandReference = new CodeThisReferenceExpression();
            if (useViewReference)
                executeCommandReference = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "View");

            var relatedElement = DiagramData.GetElement(viewModelCommandData);
            if (relatedElement == null)
            {



                if (viewModelCommandData.RelatedTypeName != null)
                {
                    executeMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                        viewModelCommandData.RelatedTypeName, "arg"));

                    executeMethod.Statements.Add(new CodeMethodInvokeExpression(
                        executeCommandReference, "ExecuteCommand",
                        new CodeSnippetExpression(string.Format("{0}.{1}", data.Name, viewModelCommandData.Name)),
                        new CodeVariableReferenceExpression("arg")
                        ));
                }
                else
                {
                    executeMethod.Statements.Add(new CodeMethodInvokeExpression(
                        executeCommandReference, "ExecuteCommand",
                        new CodeSnippetExpression(string.Format("{0}.{1}", data.Name, viewModelCommandData.Name))
                        ));
                }
            }
            else
            {
                executeMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                    relatedElement.NameAsViewModel, relatedElement.NameAsVariable));

                executeMethod.Statements.Add(new CodeMethodInvokeExpression(
                    executeCommandReference, "ExecuteCommand",
                    new CodeSnippetExpression(string.Format("{0}.{1}", data.Name, viewModelCommandData.Name)),
                    new CodeVariableReferenceExpression(relatedElement.NameAsVariable)
                    ));
            }
            decl.Members.Add(executeMethod);
        }
    }

    private void AddComponentReferences(CodeTypeDeclaration decl, ElementData data)
    {
        if (data.IsControllerDerived) return;

        foreach (var viewComponentData in data.ViewComponents)
        {

            var backingField = new CodeMemberField(viewComponentData.Name, "_" + viewComponentData.Name);
            backingField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializeField))));
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
                AddExecuteMethods(element, decl, true);
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
                        new CodeSnippetExpression(String.Format((string)"{0} == null", (object)viewPrefabField.Name)));

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