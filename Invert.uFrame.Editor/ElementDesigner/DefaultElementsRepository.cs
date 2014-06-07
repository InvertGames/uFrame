using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.Refactoring;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

public class DefaultElementsRepository : IElementsDataRepository
{
    //#region Paths

    //public string AssetPath
    //{
    //    get
    //    {
    //        return AssetDatabase.GetAssetPath(Diagram).Replace(string.Format("{0}.asset", Diagram.name), "");
    //    }
    //}

    //public string BehavioursPath
    //{
    //    get
    //    {
    //        return Path.Combine(AssetPath, "Behaviours");
    //    }
    //}

    //public string CreatePath(string name)
    //{
    //    var path = Path.Combine(AssetPath, "Behaviours");
    //    if (!Directory.Exists(path))
    //    {
    //        Directory.CreateDirectory(path);
    //        AssetDatabase.Refresh();
    //    }
    //    return path;
    //}
    //public string ControllersDesignerFileFullPath
    //{
    //    get
    //    {
    //        return Path.Combine(RootPath, ControllersDesignerFilename);
    //    }
    //}

    //public string ControllersDesignerFilename
    //{
    //    get
    //    {
    //        return AssetPath + string.Format("{0}Controllers.designer.cs", Diagram.Name);
    //    }
    //}

    //public string ControllersPath
    //{
    //    get
    //    {
    //        return Path.Combine(AssetPath, "Controllers");
    //    }
    //}

    //public FileInfo DiagramFileInfo
    //{
    //    get
    //    {
    //        return new FileInfo(Path.Combine(RootPath, DiagramPath));
    //    }
    //}

    //public string DiagramPath
    //{
    //    get { return AssetDatabase.GetAssetPath(Diagram); }
    //}

    //public string RootPath
    //{
    //    get
    //    {
    //        return Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
    //    }
    //}

    //public string SceneManagersPath
    //{
    //    get
    //    {
    //        return Path.Combine(AssetPath, "SceneManagers");
    //    }
    //}

    //public string ScenesPath
    //{
    //    get
    //    {
    //        return Path.Combine(AssetPath, "Scenes");
    //    }
    //}

    //public string ViewComponentsPath
    //{
    //    get
    //    {
    //        return Path.Combine(AssetPath, "Components");
    //    }
    //}

    //public string ViewModelsDesignerFileFullPath
    //{
    //    get
    //    {
    //        return Path.Combine(RootPath, ViewModelsDesignerFilename);
    //    }
    //}

    //public string ViewModelsDesignerFilename
    //{
    //    get
    //    {
    //        return AssetPath + string.Format("{0}.designer.cs", Diagram.Name);
    //    }
    //}

    //public string ViewModelsPath
    //{
    //    get
    //    {
    //        return Path.Combine(AssetPath, "ViewModels");
    //    }
    //}

    //public string ViewsDesignerFileFullPath
    //{
    //    get
    //    {
    //        return Path.Combine(RootPath, ViewsDesignerFilename);
    //    }
    //}

    //public string ViewsDesignerFilename
    //{
    //    get
    //    {
    //        return AssetPath + string.Format("{0}Views.designer.cs", Diagram.Name);
    //    }
    //}

    //public string ViewsPath
    //{
    //    get
    //    {
    //        return Path.Combine(AssetPath, "Views");
    //    }
    //}



    //public ElementDesignerData GetData()
    //{
    //    //Diagram.ImportedElements.RemoveAll(p => Type.GetType(p.TypeAssemblyName) == null);
    //    //foreach (var importedElementData in Diagram.ImportedElements)
    //    //{
    //    //    importedElementData.Properties.Clear();
    //    //    importedElementData.Collections.Clear();
    //    //    importedElementData.Commands.Clear();
    //    //    var type = Type.GetType(importedElementData.TypeAssemblyName);
    //    //    FillElementData(type, importedElementData);
    //    //}

    //    return Diagram;
    //}

    //#endregion Paths

    [Inject]
    public IUFrameContainer Container { get; set; }

    private Dictionary<string, string> _derivedTypes;
    //private SerializedObject _serializedObject;    
    //public ElementDesignerData Diagram { get; set; }

    public Dictionary<string, string> GetProjectDiagrams()
    {
        var items = new Dictionary<string, string>();
        foreach (var elementDesignerData in UFrameAssetManager.Diagrams)
        {
            items.Add(elementDesignerData.name, AssetDatabase.GetAssetPath(elementDesignerData));
        }
        return items;
    }

    public ElementDesignerData LoadDiagram(string path)
    {
        var data = AssetDatabase.LoadAssetAtPath(path, typeof(ElementDesignerData)) as ElementDesignerData;
        if (data == null)
        {
            throw new Exception(
                "Invalid data format for this diagram.  Make sure the correct diagram repository is available.");
        }

        data.AssetPath = path.Replace(string.Format("{0}.asset", data.name), "");
        data.CodePathStrategy = Container.Resolve<ICodePathStrategy>(data.CodePathStrategyName ?? "Default");
        return data;
    }

    public void SaveDiagram(ElementDesignerData data)
    {
        EditorUtility.SetDirty(data);
    }

    public void FastUpdate()
    {

    }

    public void FastSave()
    {

    }
}

public interface ICodePathStrategy
{
    string AssetPath { get; set; }
}

public class DefaultCodePathStrategy : ICodePathStrategy
{
    public string AssetPath { get; set; }
}