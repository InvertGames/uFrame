using System.Collections;
using System.Linq;
using UnityEngine;

public class DestroyChildView : UBAction
{
    public UBSystemObject _viewModel = new UBSystemObject(typeof(ViewModel));

    protected override void PerformExecute(IUBContext context)
    {
        var viewModel = _viewModel.GetValueAs<ViewModel>(context);
        if (viewModel != null)
        {
            var view = context.GameObject.GetView().ChildViews.FirstOrDefault(p => p.ViewModelObject == viewModel);
            if (view != null)
            {
                Object.Destroy(view.gameObject);
            }
        }
    }
}