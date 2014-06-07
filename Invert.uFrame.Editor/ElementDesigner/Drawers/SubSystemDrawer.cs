using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SubSystemDrawer : DiagramItemDrawer<SubSystemData>
{
    private DiagramItemHeader _transitionsHeader;

    public override GUIStyle BackgroundStyle
    {
        get
        {

            return UFStyles.DiagramBox1;
        }
    }

    public SubSystemDrawer()
    {
    }

    public SubSystemDrawer(SubSystemData data, ElementsDiagram diagram)
        : base(data, diagram)
    {
        Diagram = diagram;

    }

    public override bool AllowCollapsing
    {
        get
        {
            return Diagram.Data.CurrentFilter == this.Data;
        }
    }

    public DiagramItemHeader TransitionsHeader
    {
        get { return _transitionsHeader ?? (_transitionsHeader = new DiagramItemHeader() { Label = "Types", HeaderType = typeof(IDiagramItem) }); }
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