using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ElementDesigner.Data;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class UFrameEditorPlugin : DiagramPlugin
{
    public override void Initialize(uFrameContainer container)
    {
#if DEBUG
        Debug.Log("Registering " + "UFrameEditorPlugin");
#endif
        
        container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand(), "ViewModelPropertyTypeSelection");
        container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand(), "ViewModelCommandTypeSelection");
        container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand(), "ViewModelCollectionTypeSelection");
        container.RegisterInstance<IDiagramItemCommand>(new CreateSceneCommand(), "Create Scene");

       
        container.Register<DiagramItemGenerator,ElementDataGenerator>("ElementData");
        container.Register<DiagramItemGenerator,EnumDataGenerator>("EnumData");
        container.Register<DiagramItemGenerator,ViewDataGenerator>("ViewData");
        container.Register<DiagramItemGenerator,SceneManagerDataGenerator>("SceneManagerData");

        container.RegisterInstance<IToolbarCommand>(new ImportCommand(),"Import");
    }
}

public class CreateSceneCommand : EditorCommand<IDiagramItem>, IDiagramItemCommand
{

    public override void Perform(IDiagramItem item)
    {
        var sceneManagerData = item as SceneManagerData;

        if (!Directory.Exists(ScenesPath))
        {
            Directory.CreateDirectory(ScenesPath);
        }
        EditorApplication.NewScene();
        var go = new GameObject("_GameManager");
        go.AddComponent<GameManager>()._LoadingLevel = "Loading";
        EditorUtility.SetDirty(go);
        EnsureSceneContainerInScene(sceneManagerData);
        EditorApplication.SaveScene();
    }

  
    private static void EnsureSceneContainerInScene(SceneManagerData sceneManagerData)
    {
        if (sceneManagerData.CurrentType != null)
        {
            if (Object.FindObjectsOfType(sceneManagerData.CurrentType).Length < 1)
            {
                var go = new GameObject("_SceneManager", sceneManagerData.CurrentType);
                go.name = "_SceneManager";
            }
        }
    }

    public override string CanPerform(IDiagramItem item)
    {
        if (item is SceneManagerData) return null;
        return "Must be a scene manager to perform this action.";
    }
}

public class SelectItemTypeCommand : EditorCommand<ElementsDiagram>
{
    public bool AllowNone { get; set; }
    public bool PrimitiveOnly { get; set; }

    public override void Perform(ElementsDiagram item)
    {
        var typesList = GetRelatedTypes(item.Data);

        var viewModelItem = item.SelectedItem as IViewModelItem;
        if (viewModelItem == null)
        {
            return;
        }
        ElementItemTypesWindow.InitTypeListWindow("Choose Type", typesList.ToArray(), (selected) =>
        {
            Undo.RecordObject(item.Data, "Set Type");
            viewModelItem.RelatedType = selected.AssemblyQualifiedName;
            item.Refresh();
            EditorUtility.SetDirty(item.Data);
            EditorWindow.GetWindow<ElementItemTypesWindow>().Close();
        });
    }

    public virtual IEnumerable<ElementItemType> GetRelatedTypes(ElementDesignerData diagramData)
    {
        if (AllowNone)
        {
            yield return new ElementItemType() { AssemblyQualifiedName = null, Group = "", Label = "None" };
        }

        if (!PrimitiveOnly)
        {
            foreach (var viewModel in diagramData.ViewModels)
            {
                yield return new ElementItemType()
                {
                    AssemblyQualifiedName = viewModel.AssemblyQualifiedName,
                    Label = viewModel.Name,
                    Group = diagramData.name
                };
            }
        }
        yield return new ElementItemType() { Type = typeof(int), Group = "", Label = "int" };
        yield return new ElementItemType() { Type = typeof(string), Group = "", Label = "string" };
        yield return new ElementItemType() { Type = typeof(decimal), Group = "", Label = "decimal" };
        yield return new ElementItemType() { Type = typeof(float), Group = "", Label = "float" };
        yield return new ElementItemType() { Type = typeof(bool), Group = "", Label = "bool" };
        yield return new ElementItemType() { Type = typeof(char), Group = "", Label = "char" };
        yield return new ElementItemType() { Type = typeof(DateTime), Group = "", Label = "date" };
        yield return new ElementItemType() { Type = typeof(Vector2), Group = "", Label = "Vector2" };
        yield return new ElementItemType() { Type = typeof(Vector3), Group = "", Label = "Vector3" };

        if (PrimitiveOnly) yield break;

        var projectAssembly = typeof(ViewModel).Assembly;
        foreach (var type in projectAssembly.GetTypes())
        {
            if (typeof(ViewModel).IsAssignableFrom(type) || typeof(ICommand).IsAssignableFrom(type))
            {
                yield return new ElementItemType() { Type = type, Group = "Project", Label = type.Name };
            }
        }
    }

    public override string CanPerform(ElementsDiagram item)
    {
        
        if (item == null) return "No element data.";
        if (item.SelectedItem == null) return "No selection";
        if (item.SelectedItem as IViewModelItem == null) return "Must be an element item";
        return null;
    }
}

public class FindInSceneCommand : EditorCommand<ViewData>
{
    public override void Perform(ViewData item)
    {
        var currentViewType = item.CurrentViewType;
        if (currentViewType != null)
        {
            uFrameEditorSceneManager.NavigateToFirstView(currentViewType);
        }
        else
        {
            Debug.Log("The view you are attempting to navigate to (by double clicking) couldn't be found.  Try saving the diagram before attempting to navigate.");
        }
    }

    public override string CanPerform(ViewData item)
    {
        if (item == null) return "Must be a View";
        return null;
    }
}

public class ImportCommand : ElementsDiagramToolbarCommand
{
    public override void Perform(ElementsDiagram diagram)
    {
        var typesList = ActionSheetHelpers.GetDerivedTypes<ViewModel>(false, false).ToList();
        typesList.AddRange(ActionSheetHelpers.GetDerivedTypes<Enum>(false, false));
        typesList.AddRange(ActionSheetHelpers.GetDerivedTypes<SceneManager>(false, false));
        typesList.AddRange(ActionSheetHelpers.GetDerivedTypes<ViewBase>(false, false));

        ImportTypeListWindow.InitTypeListWindow("Choose Type", typesList, (item) =>
        {
            if (IsImportOnly(item))
            {
                EditorUtility.DisplayDialog("Can't do that", String.Format("Can't import {0} because it already belongs to another diagram.", item.FullName), "OK");

                return;
            }

            var result = ImportType(item, diagram.Data);
            if (result != null)
            {
                result.Data = diagram.Data;
                if (diagram.Data.CurrentFilter.IsAllowed(result, item))
                {
                    diagram.Data.CurrentFilter.Locations[result] = new Vector2(15f, 15f);
                }
            }
            diagram.Refresh(true);
        });
    }

    public DiagramItem ImportType(Type type, ElementDesignerData diagramData)
    {
        if (type.IsEnum)
        {
            var enumData = new EnumData { Name = type.Name, Data = diagramData };
            //var enumValues = Enum.GetValues(item);
            var enumNames = Enum.GetNames(type);
            for (var i = 0; i < enumNames.Length; i++)
            {
                enumData.EnumItems.Add(new EnumItem()
                {
                    Name = enumNames[i]
                });
            }
            diagramData.Enums.Add(enumData);
            return enumData;
        }
        if (typeof(ViewModel).IsAssignableFrom(type))
        {
            var vm = GetViewModelFromType(type, diagramData);
            if (vm is ImportedElementData)
            {
                diagramData.ImportedElements.Add(vm as ImportedElementData);
            }
            else
            {
                diagramData.ViewModels.Add(vm as ElementData);
            }
            return vm;
        }
        if (typeof(ViewBase).IsAssignableFrom(type))
        {
            var view = new ViewData();
            view.Name = type.Name;
            view.Data = diagramData;
            if (type.BaseType != null)
                if (type.BaseType.AssemblyQualifiedName != null)
                    view.ForAssemblyQualifiedName = type.BaseType.AssemblyQualifiedName.Replace("ViewModel", "");

            diagramData.Views.Add(view);
            return view;
        }
        if (typeof(SceneManager).IsAssignableFrom(type))
        {
            var sceneManager = new SceneManagerData();
            sceneManager.Name = type.Name;
            sceneManager.Data = diagramData;
            diagramData.SceneManagers.Add(sceneManager);

            return sceneManager;
        }
        return null;
    }

    public ElementDataBase GetViewModelFromType(Type type, ElementDesignerData diagramData)
    {
        ElementDataBase vmData = IsImportOnly(type) ? (ElementDataBase)new ImportedElementData()
        {
            TypeAssemblyName = type.AssemblyQualifiedName,
            Data = diagramData,
            Location = new Vector2(15f, 15f)
        } : new ElementData()
        {
            Data = diagramData,
            Location = new Vector2(15f, 15f)
        };

        FillElementData(type, vmData);
        return vmData;
    }

    public static string ConvertType(Type type)
    {
        if (typeof(ViewModel).IsAssignableFrom(type))
        {
            if (type == typeof(ViewModel))
            {
                return UFrameAssetManager.DesignerVMAssemblyName;
            }
            var assemblyQualifiedName = UFrameAssetManager.DesignerVMAssemblyName;
            if (assemblyQualifiedName != null)
                return assemblyQualifiedName.Replace("ViewModel", type.Name.Replace("ViewModel", ""));
        }
        return type == null ? null : type.AssemblyQualifiedName;
    }

    protected void FillElementData(Type type, ElementDataBase vmData)
    {
        vmData.Dirty = true;
        vmData.BaseTypeName = ConvertType(type.BaseType);
        vmData.Name = type.Name.Replace("ViewModel", "");
        var vProperties = ViewModel.GetReflectedModelProperties(type).Where(p => p.Value.DeclaringType == type);

        foreach (var property in vProperties)
        {
            if (typeof(IModelCollection).IsAssignableFrom(property.Value.FieldType))
            {
                var data = new ViewModelCollectionData();
                data.Name = property.Key.Replace("_", "").Replace("Property", "");
                data.RelatedType = ConvertType(property.Value.FieldType.GetGenericArguments().First());
                vmData.Collections.Add(data);
            }
            else
            {
                var data = new ViewModelPropertyData
                {
                    Name = property.Key.Replace("_", "").Replace("Property", ""),
                    RelatedType = ConvertType(property.Value.FieldType.GetGenericArguments().First()),
                    DefaultValue = property.Value
                };
                vmData.Properties.Add(data);
            }
        }

        var commands = ViewModel.GetReflectedCommands(type).Where(p => p.Value.DeclaringType == type);
        foreach (var command in commands)
        {
            var data = new ViewModelCommandData();
            data.Name = command.Key;
            data.RelatedType = ConvertType(command.Value.PropertyType.GetGenericArguments().FirstOrDefault());

            vmData.Commands.Add(data);
        }
    }

    public bool IsImportOnly(Type type)
    {
        if (type == null) return false;
        var attribute =
            type.GetCustomAttributes(false).FirstOrDefault();
        if (attribute != null && attribute.GetType().Name == "DiagramInfoAttribute")
        {
            return true;
        }
        if (attribute == null)
            return false;

        return true;
    }
}