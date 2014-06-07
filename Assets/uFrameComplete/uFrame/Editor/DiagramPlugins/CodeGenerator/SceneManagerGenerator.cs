using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invert.uFrame.Editor;
using UnityEngine;

public abstract class SceneManagerClassGenerator : CodeGenerator
{
    public SceneManagerData Data
    {
        get; set;
    }
    public ElementDesignerData DiagramData
    {
        get; set;
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

    public virtual void AddSceneManager(SceneManagerData sceneManager)
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
            foreach (var sceneManagerTransition in sceneManager.Transitions)
            {
                //commands.Where(p=>p.Identifier == SceneManagerTransition.Id)
                var transitionItem = DiagramData.SceneManagers.FirstOrDefault(p => p.Identifier == sceneManagerTransition.ToIdentifier);
                if (transitionItem == null || transitionItem.SubSystem == null) continue;

                var settingsField = new CodeMemberField(transitionItem.NameAsSettings, sceneManagerTransition.NameAsSettingsField)
                {
                    Attributes = MemberAttributes.Public,
                    InitExpression = new CodeObjectCreateExpression(transitionItem.NameAsSettings)
                };

                decl.Members.Add(settingsField);

                var transitionMethod = new CodeMemberMethod()
                {
                    Name = sceneManagerTransition.Name,
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
            var settingsField2 = new CodeMemberField(sceneManager.NameAsSettings, sceneManager.NameAsSettingsField)
            {
                Attributes = MemberAttributes.Public,
                InitExpression = new CodeObjectCreateExpression(sceneManager.NameAsSettings)
            };
            decl.Members.Add(settingsField2);
            AddSceneManagerSettings(sceneManager, rootElements);
        }
        Namespace.Types.Add(decl);
    }

    public virtual void AddSceneManagerSettings( SceneManagerData sceneManagerData, List<ElementDataBase> rootElements)
    {
        var decl = new CodeTypeDeclaration
        {
            Name = sceneManagerData.NameAsSettings,
            TypeAttributes = TypeAttributes.Sealed | TypeAttributes.Public,
            IsPartial = true
        };
        if (IsDesignerFile)
        {
            decl.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializableAttribute))));
            decl.Members.Add(new CodeMemberField(typeof(string[]), "_Scenes") { Attributes = MemberAttributes.Public });



            foreach (var element in rootElements)
            {
                var field = new CodeMemberField(sceneManagerData.NameAsSceneManager + element.NameAsTypeEnum,
                    element.NameAsTypeEnum) { Attributes = MemberAttributes.Public };
                decl.Members.Add(field);
            }
        }
       
        Namespace.Types.Add(decl);
    }

}

public class SceneManagerGenerator : SceneManagerClassGenerator
{

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        AddSceneManager(Data);
        
        //AddSceneManagerSettings(DiagramData);
    }
}

public class SceneManagerSettingsGenerator : SceneManagerClassGenerator
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        AddSceneManagerSettings(Data,null);
    }
}