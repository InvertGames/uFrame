//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;
//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(View), true)]
//public class GameEditor : ManagerEditor<Controller>
//{
//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        //var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
//        //foreach (var fieldInfo in fields)
//        //{
            
//        //}

//    }

//    protected override bool ExistsInScene(Type itemType)
//    {
//        return Target.GetComponentInChildren(itemType) != null;
//    }

//    protected override string GetTypeNameFromName(string name)
//    {
//        return name + "Controller";
//    }

    
//}