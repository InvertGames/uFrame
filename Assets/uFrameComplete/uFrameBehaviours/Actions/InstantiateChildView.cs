using UBehaviours.Actions;

public class InstantiateChildView : UBAction
{
    [UBRequireVariable]
    public UBObject _Result = new UBObject(typeof(ViewBase), true);

    [UBRequired]
    public UBSystemObject _viewModel = new UBSystemObject(typeof(ViewModel));

    protected override void PerformExecute(IUBContext context)
    {
        var viewModel = _viewModel.GetValueAs<ViewModel>(context);
        if (viewModel != null)
            context.GameObject.GetView().InstantiateView(viewModel);
    }
}