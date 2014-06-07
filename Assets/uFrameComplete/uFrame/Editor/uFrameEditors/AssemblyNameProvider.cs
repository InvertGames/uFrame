using UnityEngine;
using System.Collections;
using UnityEditor;

[InitializeOnLoad]
public class AssemblyNameProvider : IAssemblyNameProvider
{
    static AssemblyNameProvider()
    {
        UFrameAssetManager.AssemblyNameProvider = new AssemblyNameProvider();
    }

    public string AssemblyName
    {
        get
        {
            var assemblyQualifiedName = typeof(AssemblyNameProvider).AssemblyQualifiedName;
            if (assemblyQualifiedName != null)
                return assemblyQualifiedName.Replace("-Editor","");

            return "AssemblyNameProvider, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        }
    }

}
