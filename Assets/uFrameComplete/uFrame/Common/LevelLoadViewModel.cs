/// <summary>
/// The view model that is used when a level/scene is loading.
/// </summary>
public sealed class LevelLoadViewModel : ViewModel
{
    public readonly P<float> _Progress = new P<float>(0f);
    public readonly P<string> _Status = new P<string>("Loading...");



    public float Progress
    {
        get { return _Progress.Value; }
        set { _Progress.Value = value; }
    }

    public string Status
    {
        get { return _Status.Value; }
        set { _Status.Value = value; }
    }
}