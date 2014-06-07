using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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