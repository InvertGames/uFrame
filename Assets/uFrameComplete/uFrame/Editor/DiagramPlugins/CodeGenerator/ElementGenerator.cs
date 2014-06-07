using System.IO;

public class ElementGenerator : ElementGeneratorBase
{
    private ElementDesignerData _diagramData;

    public ElementDesignerData DiagramData
    {
        get { return _diagramData; }
        set { _diagramData = value; }
    }

    public ElementGenerator(ElementDesignerData diagramData)
    {
        _diagramData = diagramData;
    }
}