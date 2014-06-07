public class LevelLoaderView : View<LevelLoadViewModel>
{

    public override void Bind()
    {
       GameManager.Instance.StartCoroutine(GameManager.Load());
    }

    public override ViewModel CreateModel()
    {
        return GameManager.Progress;
    }

    protected override void InitializeViewModel(LevelLoadViewModel viewViewModel)
    {
    }

}