using System;
using System.Collections.Generic;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEditor;

public interface IElementsDataRepository
{
    ElementDesignerData LoadDiagram(string path);
    void SaveDiagram(ElementDesignerData data);
}