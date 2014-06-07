using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ElementDesigner.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public static class uFrameEditor
    {
        private static IEditorCommand[] _commands;
        private static uFrameContainer _container;

        private static IEnumerable<CodeGenerator> _generators;

        private static IDiagramPlugin[] _plugins;

        private static IToolbarCommand[] _toolbarCommands;

        public static IEditorCommand[] Commands
        {
            get
            {
                return _commands ?? (_commands = Container.ResolveAll<IEditorCommand>().ToArray());
            }
        }

        public static uFrameContainer Container
        {
            get
            {
                if (_container != null) return _container;
                _container = new uFrameContainer();
                InitializeContainer(_container);
                return _container;
            }
            set { _container = value; }
        }

        public static IDiagramPlugin[] Plugins
        {
            get
            {
                return _plugins ?? (_plugins = Container.ResolveAll<IDiagramPlugin>().ToArray());
            }
            set { _plugins = value; }
        }

        public static IEnumerable<IEditorCommand> CreateCommandsFor<T>()
        {
            var commands = Container.ResolveAll<T>();

            return Commands.Where(p => typeof(T).IsAssignableFrom(p.For));
        }

        public static TCommandUI CreateCommandUI<TCommandUI>(ICommandHandler handler, params Type[] contextTypes) where TCommandUI : class,ICommandUI
        {
            var ui = Container.Resolve<TCommandUI>() as ICommandUI;
            ui.Handler = handler;
            foreach (var contextType in contextTypes)
            {
                var commands = Container.ResolveAll(contextType).Cast<IEditorCommand>().ToArray();

                foreach (var command in commands)
                {
                    ui.AddCommand(command);
                }
            }
            return (TCommandUI)ui;
        }

        public static IElementDrawer CreateDrawer(IDiagramItem data, ElementsDiagram diagram)
        {
            if (data == null)
            {
                Debug.Log("Data is null.");
            }
            var drawer = Container.ResolveRelation<IElementDrawer>(data.GetType());
            if (drawer == null)
            {
                Debug.Log("Couldn't Create drawer for.");
            }
            drawer.Diagram = diagram;
            drawer.Model = data;
            return drawer;
        }

        public static void ExecuteCommand(this ICommandHandler handler, IEditorCommand command)
        {
            var objs = handler.ContextObjects.ToArray();
            foreach (var o in objs)
            {
                if (o == null) continue;

                if (command.For.IsAssignableFrom(o.GetType()))
                {
                    command.Execute(o);
                }
            }
        }


        public static IEnumerable<CodeGenerator> GetAllCodeGenerators(ElementDesignerData diagramData)
        {
            // Grab all the code generators
            var diagramItemGenerators = Container.ResolveAll<DiagramItemGenerator>().ToArray();

            foreach (var diagramItemGenerator in diagramItemGenerators)
            {
                DiagramItemGenerator generator = diagramItemGenerator;
                var items = diagramData.AllDiagramItems.Where(p => p.GetType() == generator.DiagramItemType);

                foreach (var item in items)
                {
                    var codeGenerators = generator.GetGenerators(diagramData, item);
                    foreach (var codeGenerator in codeGenerators)
                    {
                        codeGenerator.ObjectData = item;
                        codeGenerator.GeneratorFor = diagramItemGenerator.DiagramItemType;
                        yield return codeGenerator;
                    }
                }
            }
        }

        public static IEnumerable<CodeFileGenerator> GetAllFileGenerators(ElementDesignerData diagramData)
        {
            var codeGenerators = GetAllCodeGenerators(diagramData).ToArray();
            var groups = codeGenerators.GroupBy(p => p.Filename);
            foreach (var @group in groups)
            {
                var generator = new CodeFileGenerator()
                {
                    Filename = @group.Key,
                    Generators = @group.ToArray()
                };
                yield return generator;
            }
        }

        public static IEnumerable<IEditorCommand> GetContextCommandsFor<T>()
        {
            return Commands.Where(p => p is IContextMenuItemCommand && typeof(T).IsAssignableFrom(p.For));
        }

        public static IEnumerable<Type> GetDerivedTypes<T>(bool includeAbstract = false, bool includeBase = true)
        {
            var type = typeof(T);
            if (includeBase)
                yield return type;
            if (includeAbstract)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in assembly
                        .GetTypes()
                        .Where(x => x.IsSubclassOf(type)))
                    {
                        yield return t;
                    }
                }
            }
            else
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in assembly
                        .GetTypes()
                        .Where(x => x.IsSubclassOf(type) && !x.IsAbstract))
                    {
                        yield return t;
                    }
                }
            }
        }

        public static void ShowCommandContextMenu()
        {
            GenericMenu menu = new GenericMenu();
        }

        private static void InitializeContainer(uFrameContainer container)
        {
            container.Register<DiagramItemHeader, DiagramItemHeader>();

            container.RegisterInstance<IUFrameContainer>(container);
            container.RegisterInstance<uFrameContainer>(container);

            // Register the diagram type
            container.Register<ElementsDiagram,ElementsDiagram>();

            // Repositories
            container.RegisterInstance<IElementsDataRepository>(new DefaultElementsRepository(),".asset");

            // Command Drawers
            container.Register<ToolbarUI, ToolbarUI>();
            container.Register<ContextMenuUI, ContextMenuUI>();
            //container.RegisterInstance(new ContextMenuUI());

            container.RegisterInstance(new AddElementCommandCommand());
            container.RegisterInstance(new AddElementCollectionCommand());
            container.RegisterInstance(new AddElementPropertyCommand());
            container.RegisterInstance(new AddEnumItemCommand());

            container.RegisterInstance<IToolbarCommand>(new PopToFilterCommand(), "PopToFilterCommand");
            container.RegisterInstance<IToolbarCommand>(new SaveCommand(), "SaveCommand");
            container.RegisterInstance<IToolbarCommand>(new AutoLayoutCommand(), "AutoLayoutCommand");

            container.RegisterInstance<IToolbarCommand>(new AddNewCommand(), "AddNewCommand");

            container.RegisterInstance<AddNewCommand>(new AddNewSceneManagerCommand(), "AddNewSceneManagerCommand");
            container.RegisterInstance<AddNewCommand>(new AddNewSubSystemCommand(), "AddNewSubSystemCommand");
            container.RegisterInstance<AddNewCommand>(new AddNewElementCommand(), "AddNewElementCommand");
            container.RegisterInstance<AddNewCommand>(new AddNewEnumCommand(), "AddNewEnumCommand");
            container.RegisterInstance<AddNewCommand>(new AddNewViewCommand(), "AddNewViewCommand");
            container.RegisterInstance<AddNewCommand>(new AddNewViewComponentCommand(), "AddNewViewComponentCommand");

            container.RegisterInstance<IDiagramContextCommand>(new AddNewSceneManagerCommand(), "AddNewSceneManagerCommand");
            container.RegisterInstance<IDiagramContextCommand>(new AddNewSubSystemCommand(), "AddNewSubSystemCommand");
            container.RegisterInstance<IDiagramContextCommand>(new AddNewElementCommand(), "AddNewElementCommand");
            container.RegisterInstance<IDiagramContextCommand>(new AddNewEnumCommand(), "AddNewEnumCommand");
            container.RegisterInstance<IDiagramContextCommand>(new AddNewViewCommand(), "AddNewViewCommand");
            container.RegisterInstance<IDiagramContextCommand>(new AddNewViewComponentCommand(), "AddNewViewComponentCommand");
            container.RegisterInstance<IDiagramContextCommand>(new ShowItemCommand(), "ShowItem");

            container.RegisterInstance<IDiagramItemCommand>(new OpenCommand(), "OpenCode");
            container.RegisterInstance<IDiagramItemCommand>(new DeleteCommand(), "Delete");
            container.RegisterInstance<IDiagramItemCommand>(new RenameCommand(), "Reanme");
            container.RegisterInstance<IDiagramItemCommand>(new HideCommand(), "Hide");
            container.RegisterInstance<IDiagramItemCommand>(new RemoveLinkCommand(), "RemoveLink");
            container.RegisterInstance<IDiagramItemCommand>(new SelectViewBaseElement(), "SelectView");

            // Drawers
            container.RegisterRelation<ViewData, IElementDrawer, ViewDrawer>();
            container.RegisterRelation<ViewComponentData, IElementDrawer, ViewComponentDrawer>();
            container.RegisterRelation<ElementData, IElementDrawer, ElementDrawer>();
            container.RegisterRelation<ElementDataBase, IElementDrawer, ElementDrawer>();
            container.RegisterRelation<ImportedElementData, IElementDrawer, ElementDrawer>();
            container.RegisterRelation<SubSystemData, IElementDrawer, SubSystemDrawer>();
            container.RegisterRelation<SceneManagerData, IElementDrawer, SceneManagerDrawer>();
            container.RegisterRelation<EnumData, IElementDrawer, DiagramEnumDrawer>();

            foreach (var diagramPlugin in GetDerivedTypes<DiagramPlugin>(false, false))
            {
                container.RegisterInstance(Activator.CreateInstance((Type) diagramPlugin) as IDiagramPlugin, diagramPlugin.Name, false);
            }

            container.InjectAll();
            foreach (var diagramPlugin in Plugins)
            {
#if DEBUG
                Debug.Log("Loaded Plugin: " + diagramPlugin);
#endif
                diagramPlugin.Initialize(Container);
            }
        }

        //public static TCommandUI DoCommands<TCommandUI>(params object[] contextObjects) where TCommandUI : class, ICommandUI
        //{
        //    return DoCommands<TCommandUI>(null, contextObjects);
        //}

        //public static TCommandUI DoCommands<TCommandUI>(Predicate<IEditorCommand> filter, params object[] contextObjects) where TCommandUI : class, ICommandUI
        //{
        //    var commandUI = Container.Resolve<TCommandUI>();
        //    commandUI.Initialize();

        //    var commands = new List<IEditorCommand>();
        //    foreach (var contextObject in contextObjects)
        //    {
        //        if (contextObject == null) continue;

        //        var objCommands = Commands.Where(p => p.For.IsAssignableFrom(contextObject.GetType()) && !(p is IChildCommand));
        //        if (filter != null)
        //        {
        //            objCommands = objCommands.Where(p => filter(p));
        //        }
        //        foreach (var command in objCommands)
        //        {
        //            if (!commands.Contains(command))
        //            {
        //                //if (command.CanPerform(contextObject) == null)
        //                //{
        //                commands.Add(command);
        //                //}
        //            }
        //        }
        //    }

        //    // Loop through each command
        //    foreach (var editorCommand in commands)
        //    {
        //        if (editorCommand is IParentCommand)
        //        {
        //            var parentCommandType = editorCommand.GetType();

        //            var childs = Container.ResolveAll(parentCommandType).Cast<IEditorCommand>().ToArray();
        //            commandUI.DoMultiCommand(editorCommand, childs, contextObjects);
        //        }
        //        else if (editorCommand is IChildCommand)
        //        {
        //        }
        //        else if (editorCommand is IDynamicOptionsCommand)
        //        {
        //            var cmd = editorCommand as IDynamicOptionsCommand;
        //            var options = cmd.GetOptions(contextObjects.FirstOrDefault(p => cmd.For.IsAssignableFrom(p.GetType())));
        //            if (cmd.OptionsType == MultiOptionType.Buttons)
        //            {
        //                foreach (var contextMenuItem in options)
        //                {
        //                    commandUI.DoSingleCommand(editorCommand, contextObjects, contextMenuItem);
        //                }
        //            }

        //        }
        //        else
        //        {
        //            commandUI.DoSingleCommand(editorCommand, contextObjects);
        //        }
        //    }
        //    return commandUI;
        //}

        //public static void DoToolbar(IEnumerable<IToolbarCommand> commands, params object[] contextObjects)
        //{
        //    foreach (var command in commands.OrderBy(p => p.Order))
        //    {
        //        var dynamicOptionsCommand = command as IDynamicOptionsCommand;
        //        var parentCommand = command as IParentCommand;
        //        if (dynamicOptionsCommand != null && dynamicOptionsCommand.OptionsType == MultiOptionType.Buttons)
        //        {
        //            foreach (var multiCommandOption in dynamicOptionsCommand.GetOptions(Container.Resolve(command.For)))
        //            {
        //                if (GUILayout.Button(multiCommandOption.Name, EditorStyles.toolbarButton))
        //                {
        //                    dynamicOptionsCommand.SelectedOption = multiCommandOption;

        //                    ExecuteCommand(command, contextObjects);
        //                }
        //            }
        //        }
        //        else if (dynamicOptionsCommand != null && dynamicOptionsCommand.OptionsType == MultiOptionType.DropDown)
        //        {
        //            if (GUILayout.Button(command.Name, EditorStyles.toolbarButton))
        //            {
        //                foreach (var multiCommandOption in dynamicOptionsCommand.GetOptions(Container.Resolve(command.For)))
        //                {
        //                    var genericMenu = new GenericMenu();
        //                    Invert.uFrame.Editor.ElementDesigner.UFContextMenuItem option = multiCommandOption;
        //                    var closureSafeCommand = command;
        //                    genericMenu.AddItem(new GUIContent(multiCommandOption.Name), multiCommandOption.Checked,
        //                        () =>
        //                        {
        //                            dynamicOptionsCommand.SelectedOption = option;
        //                            ExecuteCommand(closureSafeCommand, contextObjects);
        //                            closureSafeCommand.Execute(Container.Resolve(closureSafeCommand.For));
        //                        });
        //                    genericMenu.ShowAsContext();
        //                }
        //            }
        //        }
        //        else if (parentCommand != null)
        //        {
        //            var contextCommands = Commands.OfType<IChildCommand>().Where(p => p.ChildCommandFor == parentCommand.GetType()).Cast<IContextMenuItemCommand>();
        //            //DoContextOptions(contextCommands);

        //        }
        //        else
        //        {
        //            if (GUILayout.Button(command.Name, EditorStyles.toolbarButton))
        //            {
        //                ExecuteCommand(command, contextObjects);
        //            }
        //        }
        //    }
        //}
    }
}