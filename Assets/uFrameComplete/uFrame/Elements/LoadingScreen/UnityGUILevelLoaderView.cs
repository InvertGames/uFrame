using UnityEngine;

public class UnityGUILevelLoaderView : LevelLoaderView
{
    public Texture progressBackground;
    public Texture progressForground;
    public Texture background;

    public string[] layersToDisable = { "Everything" };

    private LayerMask _oldMask;
    private bool _areLayersDisabled = false;

    public override void Bind()
    {
        base.Bind();
        Model._Progress.PropertyChanged += _Progress_PropertyChanged;
    }

    void _Progress_PropertyChanged(object value)
    {
        if ((float)value == 1 && Camera.main != null)
        {
            Camera.main.cullingMask = _oldMask;
        }
    }

    public void OnGUI()
    {
        if (!_areLayersDisabled && Camera.main != null)
        {
            _areLayersDisabled = true;
            _oldMask = Camera.main.cullingMask;
            for (int i = 0; i < layersToDisable.Length; i++)
            {
                Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer(layersToDisable[i]));
            }
        }

        var size = new Vector2(150, 25);
        var x = Screen.width / 2f - (size.x / 2f);
        var y = Screen.height / 2f - (size.y / 2f);
        var location = new Vector2(x, y);
        var labelLocation = new Rect(x, y - 20, 200, 50);
        DrawProgress(location, size, Model.Progress);
        GUI.Label(labelLocation, Model.Status);
    }

    private void DrawProgress(Vector2 location, Vector2 size, float progress)
    {
        //GUI.Box(new Rect(0,0,Screen.width,Screen.height),string.Empty);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background,ScaleMode.StretchToFill,false);
        GUI.DrawTexture(new Rect(location.x, location.y, size.x, size.y), progressBackground);
        GUI.DrawTexture(new Rect(location.x, location.y, size.x * progress, size.y), progressForground);
    }
}