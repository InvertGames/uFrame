using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using UnityEditor;
using UnityEngine;

public class DiagramEnumDrawer : DiagramItemDrawer<EnumData>
{
    private DiagramItemHeader _itemsHeader;

    public override GUIStyle BackgroundStyle
    {
        get
        {

            return UFStyles.DiagramBox9;
        }
    }

    //public override GUIStyle ItemStyle
    //{
    //    get { return UFStyles.Item6; }
    //}
    public DiagramEnumDrawer()
    {
    }

    public DiagramEnumDrawer(EnumData data, ElementsDiagram diagram)
        : base(data, diagram)
    {

    }

    public DiagramItemHeader ItemsHeader
    {
        get
        {

            if (_itemsHeader == null)
            {
                _itemsHeader = Container.Resolve<DiagramItemHeader>();
                _itemsHeader.Label = "Items";
                _itemsHeader.HeaderType = typeof(EnumData);
                _itemsHeader.AddCommand = Container.Resolve<AddEnumItemCommand>();
            }

            return _itemsHeader;
        }
        set { _itemsHeader = value; }
    }

    protected override IEnumerable<DiagramSubItemGroup> GetItemGroups()
    {
        yield return new DiagramSubItemGroup()
        {
            Header = ItemsHeader,
            Items = Data.EnumItems.Cast<IDiagramSubItem>().ToArray()
        };
    }

    protected override void DrawSelectedItem(IDiagramSubItem item, ElementsDiagram diagram)
    {
        base.DrawSelectedItem(item, diagram);

    }
}