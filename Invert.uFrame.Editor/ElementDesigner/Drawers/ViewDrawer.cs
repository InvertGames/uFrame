using System.Collections.Generic;
using System.Linq;
using Invert.uFrame;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEditor;
using UnityEngine;

public class ViewDrawer : DiagramItemDrawer<ViewData>
{
    private ElementDataBase _forElement;

    public ViewDrawer()
    {
    }

    public override GUIStyle BackgroundStyle
    {
        get
        {

            return UFStyles.DiagramBox2;
        }
    }

    public ViewDrawer(ViewData data, ElementsDiagram diagram)
        : base(data, diagram)
    {
        Diagram = diagram;

    }

    public override bool AllowCollapsing
    {
        get { return true; }
    }


    private DiagramItemHeader _behavioursHeader;

    protected override IEnumerable<DiagramSubItemGroup> GetItemGroups()
    {
        yield break;
    }

    public override GUIStyle ItemStyle
    {
        get { return UFStyles.Item4; }
    }
    [Inject("ViewDoubleClick")]
    public IEditorCommand DoubleClickCommand { get; set; }

    public override void DoubleClicked()
    {
        base.DoubleClicked();
       
    }

    public override float HeaderSize
    {
        get { return base.HeaderSize + 6; }
    }

    protected override void DrawHeader(ElementsDiagram diagram, bool importOnly)
    {
        base.DrawHeader(diagram, importOnly);
        var subTitlePosition = Data.HeaderPosition;
        subTitlePosition.y += 15;
        var style = new GUIStyle(EditorStyles.miniLabel);
        style.alignment = TextAnchor.MiddleCenter;
        GUI.Label(subTitlePosition,Data.BaseViewName,style);
    }

    protected override void DrawContent(ElementsDiagram diagram, bool importOnly)
    {
        base.DrawContent(diagram, importOnly);
    }

    protected override void DrawSelectedItem(IDiagramSubItem item, ElementsDiagram diagram)
    {
        base.DrawSelectedItem(item, diagram);
    }
}