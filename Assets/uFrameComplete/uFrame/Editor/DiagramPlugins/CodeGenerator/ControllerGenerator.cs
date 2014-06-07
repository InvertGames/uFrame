using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invert.uFrame.Editor;

public class ControllerGenerator : CodeGenerator
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        AddController(ElementData);
    }

    public ElementData ElementData
    {
        get; set;
    }
    public ElementDesignerData DiagramData
    {
        get; set;
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

    public virtual void AddController(ElementData data)
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

            if (data.BaseElement != null)
            {
                initializeOverrideMethod.Statements.Add(new CodeSnippetExpression("base.Initialize(viewModel)"));
            }
            initializeOverrideMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), initializeTypedMethod.Name,
                new CodeCastExpression(new CodeTypeReference(data.NameAsViewModel), new CodeVariableReferenceExpression("viewModel"))));
          
        }
      

        // Command functions
        if (IsDesignerFile)
            AddCommandMethods(data, viewModelTypeReference, tDecleration);

        Namespace.Types.Add(tDecleration);
    }


    public virtual void AddControllerInitMethod(ElementData data)
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