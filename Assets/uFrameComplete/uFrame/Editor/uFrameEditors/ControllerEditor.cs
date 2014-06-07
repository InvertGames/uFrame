//using System;
//using System.IO;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;
//using Object = UnityEngine.Object;

////[CustomEditor(typeof(Controller), true)]
//public class ControllerEditor : ManagerEditor<ViewBase>
//{
//    public string[] ViewList
//    {
//        get
//        {
  
//            var viewPath = Path.Combine(Application.dataPath, GCManager._ViewPrefabsPath);
//            var files = Directory.GetFiles(viewPath, "*.prefab", SearchOption.TopDirectoryOnly);
//            return files.Select(p=>Path.GetFileNameWithoutExtension(p)).ToArray();
//        }
//    }
    
//    protected override void OnAdd(string typeName)
//    {
//        //base.OnAdd(typeName);
//        var path = Path.Combine("Assets/", GCManager._ViewPrefabsPath);
//        //var index = path.IndexOf("Resources/", System.StringComparison.Ordinal);
//        //if (index > -1)
//        //{
//        //    path = path.Substring(index + "Resources/".Length);
//        //}
        
//        var asset = Path.Combine(path , typeName + ".prefab");
//        var assetObject = AssetDatabase.LoadAssetAtPath(asset, typeof(GameObject));
//        var obj = (GameObject)PrefabUtility.InstantiatePrefab(assetObject);
//        obj.transform.parent = Target.transform;
//        Selection.objects = new Object[] {obj};
//        //obj.transform.position = Vector3.zero;
//    }

//    private bool _prefabsOpen = false;
    
//    protected override void TypeListManager()
//    {
        
//        //base.TypeListManager();
//        _prefabsOpen = Toggle("View Prefabs", _prefabsOpen);
//        if (_prefabsOpen)
//        {
//            var list = ViewList;

//            var itemWidth = 150;
//            var columnsCount = Screen.width / itemWidth;
//            var rowCount = (list.Length / columnsCount) + 1;

//            var itemCount = 0;
//            GUILayout.BeginVertical();
//            for (var y = 0; y < rowCount; y++)
//            {
//                GUILayout.BeginHorizontal();
//                for (var x = 0; x < columnsCount; x++)
//                {



//                    if (itemCount >= list.Length)
//                        break;
//                    var item = list[itemCount];

//                    if (GUILayout.Button(item, GUILayout.Width(itemWidth)))
//                    {
//                        OnAdd(item);
//                    }

//                    itemCount++;

//                }
//                GUILayout.EndHorizontal();
//            }

//            GUILayout.EndVertical();
//        }
        
//        //base.TypeListManager();
       
//    }

//    protected override bool ExistsInScene(Type itemType)
//    {
//        return false;
//    }

//    protected override string GetTypeNameFromName(string name)
//    {
//        return name + "View";
//    }
//}