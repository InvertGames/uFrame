using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ElementsDataRepository : DefaultElementsRepository
{
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

    public override void CreateScene(SceneManagerData sceneManagerData)
    {
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

    public void CreateUBehaviour(ViewData data)
    {
        if (!Directory.Exists(BehavioursPath))
        {
            Directory.CreateDirectory(BehavioursPath);
        }
        var asset = UBAssetManager.CreateAsset<UFrameBehaviours>(BehavioursPath, data.Name + "Behaviour");
        asset.ViewModelTypeString = data.ViewForElement.ViewModelAssemblyQualifiedName;
        Save();
    }

    public override IEnumerable<ElementItemType> GetAvailableTypes(bool allowNone, bool primitiveOnly = false)
    {
        var baseItems = base.GetAvailableTypes(allowNone, primitiveOnly);
        foreach (var baseItem in baseItems)
        {
            yield return baseItem;
        }
        var projectAssembly = typeof(ViewModel).Assembly;
        foreach (var type in projectAssembly.GetTypes())
        {
            if (typeof(ViewModel).IsAssignableFrom(type) || typeof(ICommand).IsAssignableFrom(type))
            {
                yield return new ElementItemType() { Type = type, Group = "Project", Label = type.Name };
            }
        }
    }

    public ElementDataBase GetViewModelFromType(Type type)
    {
        ElementDataBase vmData = IsImportOnly(type) ? (ElementDataBase)new ImportedElementData()
        {
            TypeAssemblyName = type.AssemblyQualifiedName,
            Data = Diagram,
            Location = new Vector2(15f, 15f)
        } : new ElementData()
        {
            Data = Diagram,
            Location = new Vector2(15f, 15f)
        };

        FillElementData(type, vmData);
        return vmData;
    }

    public void ImportAllViewModels()
    {
        foreach (var derivedType in GetDerivedTypes<ViewModel>(true, false))
        {
            ImportType(derivedType);
        }
    }

    public override DiagramItem ImportType(Type type)
    {
        if (type.IsEnum)
        {
            var enumData = new EnumData { Name = type.Name, Data = Diagram };
            //var enumValues = Enum.GetValues(item);
            var enumNames = Enum.GetNames(type);
            for (var i = 0; i < enumNames.Length; i++)
            {
                enumData.EnumItems.Add(new EnumItem()
                {
                    Name = enumNames[i]
                });
            }
            Diagram.Enums.Add(enumData);
            return enumData;
        }
        if (typeof(ViewModel).IsAssignableFrom(type))
        {
            var vm = GetViewModelFromType(type);
            if (vm is ImportedElementData)
            {
                Diagram.ImportedElements.Add(vm as ImportedElementData);
            }
            else
            {
                Diagram.ViewModels.Add(vm as ElementData);
            }
            return vm;
        }
        if (typeof(ViewBase).IsAssignableFrom(type))
        {
            var view = new ViewData();
            view.Name = type.Name;
            view.Data = Diagram;
            if (type.BaseType != null)
                if (type.BaseType.AssemblyQualifiedName != null)
                    view.ForAssemblyQualifiedName = type.BaseType.AssemblyQualifiedName.Replace("ViewModel", "");

            Diagram.Views.Add(view);
            return view;
        }
        if (typeof(SceneManager).IsAssignableFrom(type))
        {
            var sceneManager = new SceneManagerData();
            sceneManager.Name = type.Name;
            sceneManager.Data = Diagram;
            Diagram.SceneManagers.Add(sceneManager);

            return sceneManager;
        }
        return null;
    }

    public override void Save()
    {
        ProcessRefactorings();

        // Important ensure all data properties are wired up
        foreach (var allDiagramItem in Diagram.AllDiagramItems)
        {
            allDiagramItem.Data = Diagram;
        }

        var viewModelGenerator = new ViewModelFileGenerator(Diagram) { IsDesignerFile = true };
        var controllerGenerator = new ControllerFileGenerator(Diagram) { IsDesignerFile = true };
        var viewGenerator = new ViewFileGenerator(Diagram) { IsDesignerFile = true };

        if (!Directory.Exists(ViewModelsPath))
            Directory.CreateDirectory(ViewModelsPath);
        if (!Directory.Exists(ControllersPath))
            Directory.CreateDirectory(ControllersPath);
        if (!Directory.Exists(ViewsPath))
            Directory.CreateDirectory(ViewsPath);
        if (!Directory.Exists(SceneManagersPath))
            Directory.CreateDirectory(SceneManagersPath);
        if (!Directory.Exists(ViewComponentsPath))
            Directory.CreateDirectory(ViewComponentsPath);

        foreach (var sceneManager in Diagram.SceneManagers.ToArray())
        {
            controllerGenerator.AddSceneManager(sceneManager);
            var filePath = GetContainerCustomFileFullPath(sceneManager.Name);
            if (Type.GetType(UFrameAssetManager.DesignerVMAssemblyName.Replace("ViewModel", sceneManager.NameAsSceneManager)) == null && sceneManager.SubSystem != null && !File.Exists(filePath))
            {
                var containerGenerator = new ControllerFileGenerator(Diagram) { IsDesignerFile = false };
                containerGenerator.AddSceneManager(sceneManager);
                File.WriteAllText(filePath, containerGenerator.ToString());
            }
        }

        foreach (var enumData in Diagram.Enums.ToArray())
            viewModelGenerator.AddEnum(enumData);
        foreach (var vm in Diagram.ViewModels.ToArray())
        {
            var viewModelPath = GetViewModelCustomFileFullPath(vm.Name);
            if (vm.CurrentViewModelType == null && !File.Exists(viewModelPath))
            {
                var gen2 = new ViewModelFileGenerator(Diagram) { IsDesignerFile = false };
                gen2.AddViewModel(vm);
                File.WriteAllText(viewModelPath, gen2.ToString());
            }
            var controllerPath = GetControllerCustomFileFullPath(vm.Name);
            if (vm.ControllerType == null && !File.Exists(controllerPath))
            {
                var gen2 = new ControllerFileGenerator(Diagram) { IsDesignerFile = false };
                gen2.AddController(vm);
                File.WriteAllText(controllerPath, gen2.ToString());
            }

            controllerGenerator.AddController(vm);
            viewModelGenerator.AddViewModel(vm);
            viewGenerator.AddViewBase(vm);
        }
        foreach (var view in Diagram.Views.ToArray())
        {
            if (view.ForAssemblyQualifiedName == null)
            {
                Debug.LogWarning(string.Format("Skipping view {0} because it is not associated with an Element.", view.Name));
                continue;
            }
            var filePath = GetViewCustomFileFullPath(view.Name);
            if (view.CurrentViewType == null && !File.Exists(filePath))
            {
                var gen2 = new ViewFileGenerator(Diagram) { IsDesignerFile = false };
                gen2.AddView(view);
                File.WriteAllText(filePath, gen2.ToString());
            }
            viewGenerator.AddView(view);
        }
        foreach (var viewComponentData in Diagram.ViewComponents)
        {
            var filePath = GetViewComponentCustomFileFullPath(viewComponentData.Name);
            if (viewComponentData.CurrentType == null && !File.Exists(filePath))
            {
                var gen2 = new ViewFileGenerator(Diagram) { IsDesignerFile = false };
                gen2.AddViewComponent(viewComponentData);
                File.WriteAllText(filePath, gen2.ToString());
            }
            viewGenerator.AddViewComponent(viewComponentData);
        }
        File.WriteAllText(ViewModelsDesignerFileFullPath, viewModelGenerator.ToString());
        File.WriteAllText(ControllersDesignerFileFullPath, controllerGenerator.ToString());
        File.WriteAllText(ViewsDesignerFileFullPath, viewGenerator.ToString());
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(Diagram);
    }

    protected override void FillElementData(Type type, ElementDataBase vmData)
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

    private static void EnsureSceneContainerInScene(SceneManagerData sceneManagerData)
    {
        if (sceneManagerData.CurrentType != null)
        {
            if (UnityEngine.Object.FindObjectsOfType(sceneManagerData.CurrentType).Length < 1)
            {
                var go = new GameObject("_SceneManager", sceneManagerData.CurrentType);
                go.name = "_SceneManager";
            }
        }
    }
}