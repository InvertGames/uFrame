using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEditor;
using UnityEngine;

public abstract class DiagramItemDrawer<TData> : IElementDrawer where TData : IDiagramItem
{
    private static GUIStyle _itemStyle;
    private TData _data;
    [Inject]
    public IUFrameContainer Container { get; set; }
    protected DiagramItemDrawer()
    {
    }

    protected DiagramItemDrawer(TData data, ElementsDiagram diagram)
    {
        Data = data;
        Diagram = diagram;
    }

    public TData Data
    {
        get { return _data; }
        set
        {
            _data = value;
            CalculateBounds();
        }
    }

    public virtual float HeaderSize { get { return 35 ; } }

    public float Scale
    {
        get { return UFStyles.Scale; }
    }

    public virtual GUIStyle ItemStyle
    {
        get
        {

            return UFStyles.Item4;
        }
    }

    public GUIStyle SelectedItemStyle
    {
        get { return UFStyles.SelectedItemStyle; }
    }

    public virtual float Width
    {
        get
        {
            var maxLengthItem = EditorStyles.largeLabel.CalcSize(new GUIContent(Data.FullLabel));
            if (AllowCollapsing && !Data.IsCollapsed)
            {
                foreach (var item in Data.Items)
                {
                    var newSize = EditorStyles.largeLabel.CalcSize(new GUIContent(item.FullLabel));

                    if (maxLengthItem.x < newSize.x)
                    {
                        maxLengthItem = newSize;
                    }
                }
            }


            return Math.Max(150f, maxLengthItem.x + 35);
        }
    }

    public float ItemHeight { get { return 20 ; } }

    public virtual float Padding
    {
        get { return 5; }
    }

    public virtual GUIStyle BackgroundStyle
    {
        get
        {

            return UFStyles.DiagramBox1;
        }
    }

    public float ItemExpandedHeight
    {
        get { return 0; }
    }

    public Rect CalculateItemBounds(float width, float localY)
    {
        if (Data.IsCollapsed) return Data.HeaderPosition;
        var location = Data.Location;

        var itemRect = new Rect
        {
            x = location.x + Padding,
            width = width - (Padding * 2),
            height = ItemHeight,
            y = location.y + localY
        };
        return itemRect;
    }

    protected virtual void DrawHeader(ElementsDiagram diagram, bool importOnly)
    {


        if (AllowCollapsing)
        {
            var rect = new Rect(Data.HeaderPosition.x + 5 , Data.HeaderPosition.y + (HeaderSize / 2f) - 3, 16f, 16f);
            if (!Data.IsCollapsed)
            {
                rect.y -= 3;
                rect.x += 2;
            }

            if (GUI.Button(rect.Scale(Scale), string.Empty,
                Data.IsCollapsed ? UFStyles.CollapseRight : UFStyles.CollapseDown))
            {
                Data.IsCollapsed = !Data.IsCollapsed;
                Data.Dirty = true;
                diagram.Dirty = true;
                CalculateBounds();
                EditorUtility.SetDirty(diagram.Data);
                diagram.Refresh();
            }

        }


        var style = new GUIStyle(UFStyles.ViewModelHeaderStyle);
        style.normal.textColor = BackgroundStyle.normal.textColor;
        style.alignment = TextAnchor.MiddleCenter;
        var position = new Rect(Data.HeaderPosition);
        if (AllowCollapsing)
        {
            position.x += 5;
        }

        position.y += 10 ;

        if (Data.IsEditing && !importOnly)
        {

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(Data.Name);


            var newText = GUI.TextField(position.Scale(Scale), Data.Name, style);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(diagram.Data, "Set Element Name");
                Data.Rename(newText);
                CalculateBounds();
                EditorUtility.SetDirty(diagram.Data);
            }

        }
        else
        {
            //if (!AllowCollapsing)
            //{
            //    style.alignment = TextAnchor.MiddleCenter;
            //}

            GUI.Label(position.Scale(Scale), Data.Label, style);
        }
    }

    public virtual bool AllowCollapsing
    {
        get
        {
            return true;
        }
    }

    public ElementsDiagram Diagram { get; set; }

    public virtual Type CommandsType
    {
        get { return typeof (IDiagramItem); }
    }

    protected abstract IEnumerable<DiagramSubItemGroup> GetItemGroups();

    protected virtual void DrawContent(ElementsDiagram diagram, bool importOnly)
    {
        foreach (var diagramSubItemGroup in CachedItemGroups)
        {
            diagramSubItemGroup.Header.Draw(Diagram, Scale, BackgroundStyle);
            foreach (var item in diagramSubItemGroup.Items)
            {
                DrawItem(item, diagram, importOnly);
            }
        }

        //PropertiesHeader.Draw();
        //foreach (var viewModelPropertyData in Data.Properties.ToArray())
        //{
        //    DrawItem(viewModelPropertyData, diagram, importOnly);
        //}
        //CollectionsHeader.Draw();
        //var collections = Data.Collections.ToArray();
        //foreach (var viewModelCollectionData in collections)
        //{
        //    DrawItem(viewModelCollectionData, diagram, importOnly);
        //}
        //CommandsHeader.Draw();
        //foreach (var viewModelCommandData in Data.Commands.ToArray())
        //{
        //    DrawItem(viewModelCommandData, diagram, importOnly);
        //}
        //if (Data.CurrentViewModelType != null)
        //{
        //    BehavioursHeader.Draw();
        //    foreach (var behaviour in Behaviours)
        //    {
        //        DrawItem(behaviour, diagram, importOnly);
        //    }
        //}
    }

    public string ShouldFocus { get { return Data.IsEditing ? Data.Name : null; } }

    public virtual void Draw(ElementsDiagram diagram)
    {

        //if (((IDiagramItem) Data).Dirty || Enumerable.Any<BehaviourItem>(Behaviours, p=>p == null))
        //{
        //    CalculateBounds();
        //    ((IDiagramItem) Data).Dirty = false;
        //}
        bool importOnly = Data is ImportedElementData;

        var isDiagramFilter = Data is IDiagramFilter && !Data.Equals(Data.Filter);

        var offsetPosition = new Rect(Data.Position);
        offsetPosition.x += 6;
        offsetPosition.y += 6;

        if (isDiagramFilter)
            UFStyles.DrawExpandableBox(offsetPosition.Scale(Scale), importOnly ? UFStyles.DiagramBox3 : BackgroundStyle, string.Empty);
        offsetPosition.x -= 3;
        offsetPosition.y -= 3;

        if (isDiagramFilter)
            UFStyles.DrawExpandableBox(offsetPosition.Scale(Scale), importOnly ? UFStyles.DiagramBox3 : BackgroundStyle, string.Empty);

        offsetPosition.y -= 16;
        offsetPosition.height = 16;
        var label = Data.InfoLabel;
        if (!string.IsNullOrEmpty(label))
        {
            var style = new GUIStyle(EditorStyles.miniLabel);
            style.normal.textColor = new Color(0.1f, 0.1f, 0.1f);
            style.fontSize = Mathf.RoundToInt(10*Scale);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Italic;
            GUI.Label(offsetPosition.Scale(Scale), label, style);

        }



        UFStyles.DrawExpandableBox(((IDrawable)Data).Position.Scale(Scale), importOnly ? UFStyles.DiagramBox3 : BackgroundStyle, string.Empty);


        if (Data.IsSelected)
        {
            var pos = new Rect(((IDrawable)Data).Position);
            pos.x -= 4;
            pos.width += 8;
            UFStyles.DrawExpandableBox(pos.Scale(Scale), UFStyles.BoxHighlighter2, string.Empty);
        }

        else if (Data.Equals(diagram.CurrentMouseOverItem))
            UFStyles.DrawExpandableBox(((IDrawable)Data).Position.Scale(Scale), UFStyles.BoxHighlighter1, string.Empty);
        else
        {
            //var highlighter = GetHighlighter();

            //if (highlighter != null)
            //{
            //    UFStyles.DrawExpandableBox(((IDrawable)Data).Position, highlighter, string.Empty);
            //}
        }


        DrawHeader(diagram, importOnly);

        if (!Data.IsCollapsed)
            DrawContent(diagram, importOnly);
    }

    protected virtual GUIStyle GetHighlighter()
    {
        return UFStyles.BoxHighlighter4;
    }

    protected virtual void DrawItem(IDiagramSubItem item, ElementsDiagram diagram, bool importOnly)
    {
        if (item.IsSelected && item.IsSelectable && !importOnly)
        {
            var rect = new Rect(item.Position);
            //rect.y += ItemHeight;
            //rect.height -= ItemHeight;
            //rect.height += ItemExpandedHeight;

            GUILayout.BeginArea(rect.Scale(Scale));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            DrawSelectedItem(item, diagram);
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        else
        {

            GUI.Box(item.Position.Scale(Scale), string.Empty, item.IsSelected ? SelectedItemStyle : ItemStyle);

            DrawItemLabel(item);

        }
        if (!string.IsNullOrEmpty(item.Highlighter))
        {
            var highlighterPosition = new Rect(item.Position);
            highlighterPosition.width = 4;
            highlighterPosition.y += 2;
            highlighterPosition.x += 2;
            highlighterPosition.height = ItemHeight - 5;
            GUI.Box(highlighterPosition.Scale(Scale), string.Empty, UFStyles.GetHighlighter(item.Highlighter));
        }
    }

    protected virtual void DrawItemLabel(IDiagramSubItem item)
    {
        var style = new GUIStyle(ItemStyle);
        style.normal.textColor = BackgroundStyle.normal.textColor;
        GUI.Label(item.Position.Scale(Scale), item.Label, style);


    }

    protected virtual void DrawSelectedItem(IDiagramSubItem item, ElementsDiagram diagram)
    {
        GUI.SetNextControlName(item.Name);
        var newName = EditorGUILayout.TextField(item.Name, UFStyles.ClearItemStyle);
        if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName))
        {
            if (Data.Items.All(p => p.Name != newName))
            {
                Undo.RecordObject(diagram.Data, "Rename");

                item.Rename(Data, newName);
                EditorUtility.SetDirty(diagram.Data);

            }
        }


        if (GUILayout.Button(string.Empty, UBStyles.RemoveButtonStyle.Scale(Scale)))
        {
            Undo.RecordObject(diagram.Data, "RemoveFromDiagram " + item.Name);
            item.Remove(diagram.SelectedData);
            CalculateBounds();
            diagram.Refresh(true);
            EditorUtility.SetDirty(diagram.Data);
        }

    }

    public virtual void CalculateBounds()
    {

        var location = Data.Location;

        var width = Width;
        var startY = HeaderSize;

        startY = CalculateContentBounds(startY, width);

        startY += Padding;
        Data.Position = new Rect(location.x, location.y , width, startY + Padding);
        Data.HeaderPosition = new Rect(location.x, location.y, width, HeaderSize - 2);
    }

    public bool IsSelected { get { return Data.IsSelected; } }
    public IDiagramItem Model { get { return Data; } set { Data = (TData)value; } }

    private float CalculateContentBounds(float startY, float width)
    {
        var y = startY;
        CachedItemGroups = GetItemGroups().Where(p => Diagram.Data.CurrentFilter.IsItemAllowed(p, p.Header.HeaderType)).ToArray();
        foreach (var itemGroup in CachedItemGroups)
        {
            y = CalculateGroupBounds(itemGroup, width, y);
        }
        return y;
    }

    public IEnumerable<DiagramSubItemGroup> CachedItemGroups { get; set; }

    public IEnumerable<IDiagramSubItem> Items
    {
        get { return CachedItemGroups.SelectMany(p => p.Items); }
    }

    public virtual void DoubleClicked()
    {
            
    }

    private float CalculateGroupBounds(DiagramSubItemGroup group, float width, float startY)
    {
        var sy = startY;
        @group.Header.Position = CalculateItemBounds(width, sy);
        sy += @group.Header.Position.height;
        foreach (var property in @group.Items)
        {
            property.Position = CalculateItemBounds(width, sy);
            sy += property.Position.height;
            if (property.IsSelected)
            {
                sy += ItemExpandedHeight;
            }
        }
        if (Data.IsCollapsed) 
            return startY;
        return sy;
    }

    public void Execute(IEditorCommand command)
    {
        UnityEngine.Debug.Log("Exeucting: " + command.Name);
        Diagram.Execute(command);
    }

    public IEnumerable<object> ContextObjects
    {
        get
        {
            yield return this;
            yield return Data;
            yield return Diagram;
        }
    }
}