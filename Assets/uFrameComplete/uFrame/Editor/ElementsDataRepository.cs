using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Invert.uFrame.Editor;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

public class ElementsDataRepository : DefaultElementsRepository
{
  

    //public override IEnumerable<ElementItemType> GetAvailableTypes(bool allowNone, bool primitiveOnly = false)
    //{
    //    var baseItems = base.GetAvailableTypes(allowNone, primitiveOnly);
    //    foreach (var baseItem in baseItems)
    //    {
    //        yield return baseItem;
    //    }
    //    var projectAssembly = typeof(ViewModel).Assembly;
    //    foreach (var type in projectAssembly.GetTypes())
    //    {
    //        if (typeof(ViewModel).IsAssignableFrom(type) || typeof(ICommand).IsAssignableFrom(type))
    //        {
    //            yield return new ElementItemType() { Type = type, Group = "Project", Label = type.Name };
    //        }
    //    }
    //}




    //public ElementDataBase GetViewModelFromType(Type type)
    //{
    //    ElementDataBase vmData = IsImportOnly(type) ? (ElementDataBase)new ImportedElementData()
    //    {
    //        TypeAssemblyName = type.AssemblyQualifiedName,
    //        Data = Diagram,
    //        Location = new Vector2(15f, 15f)
    //    } : new ElementData()
    //    {
    //        Data = Diagram,
    //        Location = new Vector2(15f, 15f)
    //    };

    //    FillElementData(type, vmData);
    //    return vmData;
    //}


    //public void Save2()
    //{
    //    // Important ensure all data properties are wired up
    //    foreach (var allDiagramItem in Diagram.AllDiagramItems)
    //    {
    //        allDiagramItem.Data = Diagram;
    //    }

    //    var viewModelGenerator = new ViewModelFileGenerator(Diagram) { IsDesignerFile = true };
    //    var controllerGenerator = new ControllerFileGenerator(Diagram) { IsDesignerFile = true };
    //    var viewGenerator = new ViewFileGenerator(Diagram) { IsDesignerFile = true };

    //    if (!Directory.Exists(ViewModelsPath))
    //        Directory.CreateDirectory(ViewModelsPath);
    //    if (!Directory.Exists(ControllersPath))
    //        Directory.CreateDirectory(ControllersPath);
    //    if (!Directory.Exists(ViewsPath))
    //        Directory.CreateDirectory(ViewsPath);
    //    if (!Directory.Exists(SceneManagersPath))
    //        Directory.CreateDirectory(SceneManagersPath);
    //    if (!Directory.Exists(ViewComponentsPath))
    //        Directory.CreateDirectory(ViewComponentsPath);

    //    foreach (var sceneManager in Diagram.SceneManagers.ToArray())
    //    {
    //        controllerGenerator.AddSceneManager(sceneManager);
    //        var filePath = GetContainerCustomFileFullPath(sceneManager.Name);
    //        if (Type.GetType(UFrameAssetManager.DesignerVMAssemblyName.Replace("ViewModel", sceneManager.NameAsSceneManager)) == null && sceneManager.SubSystem != null && !File.Exists(filePath))
    //        {
    //            var containerGenerator = new ControllerFileGenerator(Diagram) { IsDesignerFile = false };
    //            containerGenerator.AddSceneManager(sceneManager);
    //            File.WriteAllText(filePath, containerGenerator.ToString());
    //        }
    //    }

    //    foreach (var enumData in Diagram.Enums.ToArray())
    //        viewModelGenerator.AddEnum(enumData);
    //    foreach (var vm in Diagram.ViewModels.ToArray())
    //    {
    //        var viewModelPath = GetViewModelCustomFileFullPath(vm.Name);
    //        if (vm.CurrentViewModelType == null && !File.Exists(viewModelPath))
    //        {
    //            var gen2 = new ViewModelFileGenerator(Diagram) { IsDesignerFile = false };
    //            gen2.AddViewModel(vm);
    //            File.WriteAllText(viewModelPath, gen2.ToString());
    //        }
    //        var controllerPath = GetControllerCustomFileFullPath(vm.Name);
    //        if (vm.ControllerType == null && !File.Exists(controllerPath))
    //        {
    //            var gen2 = new ControllerFileGenerator(Diagram) { IsDesignerFile = false };
    //            gen2.AddController(vm);
    //            File.WriteAllText(controllerPath, gen2.ToString());
    //        }

    //        controllerGenerator.AddController(vm);
    //        viewModelGenerator.AddViewModel(vm);
    //        viewGenerator.AddViewBase(vm);
    //    }
    //    foreach (var view in Diagram.Views.ToArray())
    //    {
    //        if (view.ForAssemblyQualifiedName == null)
    //        {
    //            Debug.LogWarning(string.Format("Skipping view {0} because it is not associated with an Element.", view.Name));
    //            continue;
    //        }
    //        var filePath = GetViewCustomFileFullPath(view.Name);
    //        if (view.CurrentViewType == null && !File.Exists(filePath))
    //        {
    //            var gen2 = new ViewFileGenerator(Diagram) { IsDesignerFile = false };
    //            gen2.AddView(view);
    //            File.WriteAllText(filePath, gen2.ToString());
    //        }
    //        viewGenerator.AddView(view);
    //    }
    //    foreach (var viewComponentData in Diagram.ViewComponents)
    //    {
    //        var filePath = GetViewComponentCustomFileFullPath(viewComponentData.Name);
    //        if (viewComponentData.CurrentType == null && !File.Exists(filePath))
    //        {
    //            var gen2 = new ViewFileGenerator(Diagram) { IsDesignerFile = false };
    //            gen2.AddViewComponent(viewComponentData);
    //            File.WriteAllText(filePath, gen2.ToString());
    //        }
    //        viewGenerator.AddViewComponent(viewComponentData);
    //    }
    //    File.WriteAllText(ViewModelsDesignerFileFullPath, viewModelGenerator.ToString());
    //    File.WriteAllText(ControllersDesignerFileFullPath, controllerGenerator.ToString());
    //    File.WriteAllText(ViewsDesignerFileFullPath, viewGenerator.ToString());
    //    AssetDatabase.Refresh();
    //    EditorUtility.SetDirty(Diagram);
    //}
}