using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneManagerDrawer : DiagramItemDrawer<SceneManagerData>
{
    private DiagramItemHeader _transitionsHeader;

    //public override float Padding
    //{
    //    get { return 15; }
    //}
    public SceneManagerDrawer()
    {
    }

    public override GUIStyle BackgroundStyle
    {
        get
        {

            return UFStyles.DiagramBox8;
        }
    }

    public SceneManagerDrawer(SceneManagerData data, ElementsDiagram diagram)
        : base(data, diagram)
    {
        Diagram = diagram;

    }

    public override bool AllowCollapsing
    {
        get { return true; }
    }

    public DiagramItemHeader TransitionsHeader
    {
        get { return _transitionsHeader ?? (_transitionsHeader = new DiagramItemHeader() { Label = "Transitions", HeaderType = typeof(SceneManagerData) }); }
        set { _transitionsHeader = value; }
    }

    protected override IEnumerable<DiagramSubItemGroup> GetItemGroups()
    {
        if (!Data.Items.Any()) yield break;
        yield return new DiagramSubItemGroup()
        {
            Header = TransitionsHeader,
            Items = Data.Items.ToArray()
        };
    }

    public override GUIStyle ItemStyle
    {
        get { return UFStyles.Item4; }
    }

}