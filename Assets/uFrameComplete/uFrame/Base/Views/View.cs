using System;

/// <summary>
/// A View is a visual representation of a ViewModel. For example: A UI dialog, Player, Weapon, etc...
/// <typeparam name="TModel">The ViewModel Type</typeparam>
/// </summary>
public abstract class View<TModel> : ViewBase where TModel : ViewModel, new()
{
    /// <summary>
    /// Gets or sets the ViewModel. Note: The setter will reinvoke the bind method.  To set quietly use ViewModelObject
    /// </summary>
    public TModel Model
    {
        get
        {
            return ViewModelObject as TModel;
        }
        set
        {
            ViewModelObject = value;
            //Bind();
        }
    }

    public override Type ViewModelType
    {
        get { return typeof(TModel); }
    }

    public sealed override void InitializeViewModel(ViewModel model)
    {
        InitializeViewModel(model as TModel);
    }

    /// <summary>
    /// The method InitializeViewModel should be overriden to initialize anything from the Inspector Editor.
    /// </summary>
    /// <param name="viewModel"></param>
    protected virtual void InitializeViewModel(TModel viewModel)
    {
    }
}