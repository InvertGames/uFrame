using Invert.uFrame.Editor;

public class ViewBaseGenerator : ViewClassGenerator
{
    public ElementData ElementData
    {
        get; set;
    }
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        AddViewBase(ElementData);
    }
}