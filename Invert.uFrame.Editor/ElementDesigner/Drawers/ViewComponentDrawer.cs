using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ViewComponentDrawer : DiagramItemDrawer<ViewComponentData>
{
    private DiagramItemHeader _additiveScenesHeader;

    public override GUIStyle BackgroundStyle
    {
        get
        {

            return UFStyles.DiagramBox6;
        }
    }

    public ViewComponentDrawer()
    {
    }

    public ViewComponentDrawer(ViewComponentData data, ElementsDiagram diagram)
        : base(data, diagram)
    {
        Diagram = diagram;
    }

    public override bool AllowCollapsing
    {
        get { return false; }
    }

    public DiagramItemHeader AdditiveScenesHeader
    {
        get { return _additiveScenesHeader ?? (_additiveScenesHeader = new DiagramItemHeader() { Label = "Additive Scenes", HeaderType = typeof(AdditiveSceneData) }); }
        set { _additiveScenesHeader = value; }
    }

    protected override IEnumerable<DiagramSubItemGroup> GetItemGroups()
    {
        if (!Data.Items.Any()) yield break;
        yield return new DiagramSubItemGroup()
        {
            Header = AdditiveScenesHeader,
            Items = Data.Items.ToArray()
        };
    }

    public override GUIStyle ItemStyle
    {
        get { return UFStyles.Item4; }
    }

}