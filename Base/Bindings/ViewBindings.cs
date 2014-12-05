using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using UniRx;
/// <summary>
/// Binding extension methods that make it easy to bind ViewModels to Views, Any method that starts with Bind{...} will properly be unbound when a view is destroyed, if not
/// it is the developers repsonsibility to properly dispose any subscriptions using the returned IDisposable.
/// </summary>
public static class ViewBindings
{
    /// <summary>
    /// Bind to a ViewModel collection.
    /// </summary>
    /// <typeparam name="TCollectionItemType">The type that the collection contains.</typeparam>
    /// <param name="t">This</param>
    /// <param name="collectionSelector">Select a model collection.</param>
    /// <returns>The binding class that allows chaining extra options.</returns>
    [Obsolete]
    public static ModelCollectionBinding<TCollectionItemType> BindCollection<TCollectionItemType>(this ViewModel t, ModelCollection<TCollectionItemType> collectionSelector)
    {
        var binding = new ModelCollectionBinding<TCollectionItemType>()
        {
            ModelPropertySelector = () => collectionSelector,
        };

        t.AddBinding(binding);
        binding.Bind();
        return binding;
    }

    /// <summary>
    /// Bind to a ViewModel collection.
    /// </summary>
    /// <typeparam name="TCollectionItemType">The type that the collection contains.</typeparam>
    /// <param name="t">This</param>
    /// <param name="collection">The Model Collection to bind to</param>
    /// <param name="added"></param>
    /// <param name="removed"></param>
    /// <returns>The binding class that allows chaining extra options.</returns>
    public static IDisposable BindCollection<TCollectionItemType>(this IBindable t, ObservableCollection<TCollectionItemType> collection, Action<TCollectionItemType> added, Action<TCollectionItemType> removed)
    {
        NotifyCollectionChangedEventHandler collectionChanged = delegate(object sender, NotifyCollectionChangedEventArgs args)
         {
             if (added != null && args.NewItems != null)
                 foreach (var item in args.NewItems)
                     added((TCollectionItemType)item);

             if (removed != null && args.OldItems != null)
                 foreach (var item in args.OldItems)
                     removed((TCollectionItemType)item);
         };

        collection.CollectionChanged += collectionChanged;
        collectionChanged(t, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection.ToArray()));
        return t.AddBinding(Disposable.Create(() => UnbindCollectionChanged(collection, collectionChanged)));
    }

    private static void UnbindCollectionChanged<t>(ObservableCollection<t> collection, NotifyCollectionChangedEventHandler collectionChanged)
    {
        collection.CollectionChanged -= collectionChanged;
    }

    /// <summary>
    /// Bind to a ViewModel collection.
    /// </summary>
    /// <typeparam name="TCollectionItemType">The type that the collection contains.</typeparam>
    /// <param name="t">This</param>
    /// <param name="collectionSelector">Select a model collection.</param>
    /// <returns>The binding class that allows chaining extra options.</returns>
    [Obsolete]
    public static ModelCollectionBinding<TCollectionItemType> BindCollection<TCollectionItemType>(this ViewBase t, Func<ModelCollection<TCollectionItemType>> collectionSelector)
    {
        var binding = new ModelCollectionBinding<TCollectionItemType>()
        {
            ModelPropertySelector = () => collectionSelector(),
        };
        t.AddBinding(binding);
        binding.Bind();
        return binding;
    }

    /// <summary>
    /// Adds a binding to a collision, when the collusion occurs the call back will be invoked.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="eventType"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IDisposable BindCollision(this ViewBase t, CollisionEventType eventType, Action<Collision> action)
    {
        return t.AddBinding(OnCollisionObservable(t.gameObject, eventType).Subscribe(action));
    }

    /// <summary>
    /// Bind a Unity Collision event to a ViewModel command.
    /// </summary>
    /// <param name="t">The view that owns the binding</param>
    /// <param name="eventType">The collision event to bind to.</param>
    /// <returns>The collision binding class that allows chaining extra options.</returns>
    public static IObservable<Collision> OnCollisionObservable(this GameObject t, CollisionEventType eventType)
    {
        if (eventType == CollisionEventType.Enter)
        {
            return t.EnsureComponent<ObservableCollisionEnterBehaviour>().OnCollisionEnterAsObservable();
        }
        else if (eventType == CollisionEventType.Exit)
        {
            return t.EnsureComponent<ObservableCollisionExitBehaviour>().OnCollisionExitAsObservable();
        }
        else
        {
            return t.EnsureComponent<ObservableCollisionStayBehaviour>().OnCollisionStayAsObservable();
        }
    }

    /// <summary>
    /// Bind a Unity Collision event to a ViewModel command.
    /// </summary>
    /// <param name="t">The view that owns the binding</param>
    /// <param name="eventType">The collision event to bind to.</param>
    /// <returns>The collision binding class that allows chaining extra options.</returns>
    public static IObservable<Collision2D> OnCollision2DObservable(this GameObject t, CollisionEventType eventType)
    {
        if (eventType == CollisionEventType.Enter)
        {
            return t.EnsureComponent<ObservableCollisionEnter2DBehaviour>().OnCollisionEnter2DAsObservable();
        }
        else if (eventType == CollisionEventType.Exit)
        {
            return t.EnsureComponent<ObservableCollisionExit2DBehaviour>().OnCollisionExit2DAsObservable();
        }
        else
        {
            return t.EnsureComponent<ObservableCollisionStay2DBehaviour>().OnCollisionStay2DAsObservable();
        }
    }

    /// <summary>
    /// Bind a Unity Collision event to a ViewModel command.
    /// </summary>
    /// <param name="t">The view that owns the binding</param>
    /// <param name="eventType">The collision event to bind to.</param>
    /// <returns>The collision binding class that allows chaining extra options.</returns>
    public static IObservable<Collider> OnTriggerObservable(this GameObject t, CollisionEventType eventType)
    {
        if (eventType == CollisionEventType.Enter)
        {
            return t.EnsureComponent<ObservableTriggerEnterBehaviour>().OnTriggerEnterAsObservable();
        }
        else if (eventType == CollisionEventType.Exit)
        {
            return t.EnsureComponent<ObservableTriggerExitBehaviour>().OnTriggerExitAsObservable();
        }
        else
        {
            return t.EnsureComponent<ObservableTriggerStayBehaviour>().OnTriggerStayAsObservable();
        }
    }
    /// <summary>
    /// Bind a Unity Collision event to a ViewModel command.
    /// </summary>
    /// <param name="t">The view that owns the binding</param>
    /// <param name="eventType">The collision event to bind to.</param>
    /// <returns>The collision binding class that allows chaining extra options.</returns>
    public static IObservable<Collider2D> OnTrigger2DObservable(this GameObject t, CollisionEventType eventType)
    {
        if (eventType == CollisionEventType.Enter)
        {
            return t.EnsureComponent<ObservableTriggerEnter2DBehaviour>().OnTriggerEnter2DAsObservable();
        }
        else if (eventType == CollisionEventType.Exit)
        {
            return t.EnsureComponent<ObservableTriggerExit2DBehaviour>().OnTriggerExit2DAsObservable();
        }
        else
        {
            return t.EnsureComponent<ObservableTriggerStay2DBehaviour>().OnTriggerStay2DAsObservable();
        }
    }
    ///// <summary>
    ///// Ensures that a component exists and returns it.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="t"></param>
    ///// <returns></returns>
    //public static T EnsureComponent<T>(this ViewBase t) where T : Component
    //{
    //    return t.GetComponent<T>() ?? t.gameObject.AddComponent<T>();
    //}

    /// <summary>
    /// Ensures that a component exists and returns it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public static T EnsureComponent<T>(this GameObject t) where T : MonoBehaviour
    {
        if (t.GetComponent<T>() != null) return t.GetComponent<T>();
        return t.AddComponent<T>();
    }

    /// <summary>
    /// Creates a binding on collisions that filter to views only.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="eventType"></param>
    /// <param name="collision"></param>
    /// <returns></returns>
    public static IDisposable BindViewCollision(this ViewBase t, CollisionEventType eventType, Action<ViewBase> collision)
    {
        return t.AddBinding(OnViewCollision(t.gameObject, eventType).Subscribe(collision));
    }

    public static IDisposable BindViewCollisionWith<T>(this ViewBase t, CollisionEventType eventType, Action<T> collision) where T : ViewBase
    {
        return t.AddBinding(OnViewCollisionWith<T>(t.gameObject, eventType).Subscribe(collision));
    }

    public static IObservable<ViewBase> OnViewCollision(this GameObject t, CollisionEventType eventType)
    {
        return OnCollisionObservable(t, eventType).Select(p => p.GetView()).Where(p => p != null);
    }

    public static IObservable<T> OnViewCollisionWith<T>(this GameObject t, CollisionEventType eventType) where T : ViewBase
    {
        return OnCollisionObservable(t, eventType).Where(p => p.GetView<T>() != null).Select(p => p.GetView<T>());
    }

    /// <summary>
    /// Bind a input button to a ViewModel Command
    /// </summary>
    /// <param name="t">The view that owns the binding</param>
    /// <param name="commandSelector">The command to bind the input to</param>
    /// <param name="buttonName">The name of the input button to bind to.</param>
    /// <returns>The binding class that allows chaining extra options.</returns>
    public static IDisposable BindInputButton(this ViewBase t, IuFrameCommand commandSelector, string buttonName, InputButtonEventType buttonEventType = InputButtonEventType.ButtonDown)
    {
        if (buttonEventType == InputButtonEventType.Button)
        {
            return t.AddBinding(t.UpdateAsObservable().Where(p => Input.GetButton(buttonName)).Subscribe(_ => t.ExecuteCommand(commandSelector)));
        }
        else if (buttonEventType == InputButtonEventType.ButtonDown)
        {
            return t.AddBinding(t.UpdateAsObservable().Where(p => Input.GetButtonDown(buttonName)).Subscribe(_ => t.ExecuteCommand(commandSelector)));
        }

        return t.AddBinding(t.UpdateAsObservable().Where(p => Input.GetButtonUp(buttonName)).Subscribe(_ => t.ExecuteCommand(commandSelector)));
    }

    /// <summary>
    /// Bind a key to a ViewModel Command
    /// </summary>
    /// <param name="t">The view that owns the binding</param>
    /// <param name="commandSelector"></param>
    /// <param name="key"></param>
    /// <returns>The binding class that allows chaining extra options.</returns>
    public static IDisposable BindKey(this ViewBase t, IuFrameCommand commandSelector, KeyCode key, object parameter = null)
    {
        return t.AddBinding(t.UpdateAsObservable().Where(p => Input.GetKey(key)).Subscribe(_ => t.ExecuteCommand(commandSelector, parameter)));
    }

    /// <summary>
    /// Bind a key to a ViewModel Command
    /// </summary>
    /// <param name="t">The view that owns the binding</param>
    /// <returns>The binding class that allows chaining extra options.</returns>
    public static IObservable<T> ScreenToRaycastAsObservable<T>(this ViewBase t, Func<RaycastHit, T> onHit)
    {
        return t.UpdateAsObservable().Select(p =>
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                return onHit(hit);
            }
            return default(T);
        });

    }

    /// <summary>
    /// Binds a mouse event to a ViewModel Command.
    /// </summary>
    /// <param name="view">The view that will own the Binding.</param>
    /// <param name="eventType">The mouse event to bind to.</param>
    /// <returns>The binding class that allows chaining extra options.</returns>
    public static IObservable<Unit> OnMouseEvent(this ViewBase view, MouseEventType eventType)
    {
        if (eventType == MouseEventType.OnMouseDown)
        {
            var component = view.AddComponentBinding<ObservableMouseDownBehaviour>();
            return component.OnMouseDownAsObservable();
        }
        else if (eventType == MouseEventType.OnMouseDrag)
        {
            var component = view.AddComponentBinding<ObservableMouseDragBehaviour>();
            return component.OnMouseDragAsObservable();
        }
        else if (eventType == MouseEventType.OnMouseEnter)
        {
            var component = view.AddComponentBinding<ObservableMouseEnterBehaviour>();
            return component.OnMouseEnterAsObservable();
        }
        else if (eventType == MouseEventType.OnMouseExit)
        {
            var component = view.AddComponentBinding<ObservableMouseExitBehaviour>();
            return component.OnMouseExitAsObservable();

        }
        else if (eventType == MouseEventType.OnMouseOver)
        {
            var component = view.AddComponentBinding<ObservableMouseOverBehaviour>();
            return component.OnMouseOverAsObservable();
        }
        return view.AddComponentBinding<ObservableMouseOverBehaviour>().OnMouseOverAsObservable();
    }

    public static IObservable<Unit> OnDestroyObservable(this GameObject gameObject)
    {
        return gameObject.EnsureComponent<ObservableOnDestroyBehaviour>().OnDestroyAsObservable();
    }

    public static IDisposable DisposeWith(this IDisposable disposable, GameObject gameObject)
    {
        return gameObject.OnDestroyObservable().First().Subscribe(p => disposable.Dispose());
    }

    public static IDisposable DisposeWith(this IDisposable disposable, IBindable bindable)
    {
        return bindable.AddBinding(disposable);
    }

    public static IDisposable DisposeWhenChanged<T>(this IDisposable disposable, P<T> sourceProperty, bool onlyWhenChanged = true)
    {
        if (onlyWhenChanged)
        {
            var d = sourceProperty.Where(p => sourceProperty.LastValue != sourceProperty.ObjectValue).First().Subscribe(_ => { disposable.Dispose(); });
            return d;
        }
        return sourceProperty.First().Subscribe(_ => { disposable.Dispose(); });

    }

    /// <summary>
    /// Binds a property to a view, this is the standard property binding extension method.
    /// </summary>
    /// <typeparam name="TBindingType"></typeparam>
    /// <param name="bindable"></param>
    /// <param name="sourceProperty"></param>
    /// <param name="targetSetter"></param>
    /// <param name="onlyWhenChanged"></param>
    /// <returns></returns>
    public static IDisposable BindProperty<TBindingType>(this IBindable bindable, P<TBindingType> sourceProperty, Action<TBindingType> targetSetter, bool onlyWhenChanged = true)
    {
        targetSetter(sourceProperty.Value);
        if (onlyWhenChanged)
        {
            return bindable.AddBinding(sourceProperty.Where(p => sourceProperty.LastValue != sourceProperty.ObjectValue).Subscribe(targetSetter));
        }

        return bindable.AddBinding(sourceProperty.Subscribe(targetSetter));
    }

    /// <summary>
    /// Binds to a commands execution and is diposed with the bindable
    /// </summary>
    /// <param name="bindable"></param>
    /// <param name="sourceCommand"></param>
    /// <param name="onExecuted"></param>
    /// <returns></returns>
    public static IDisposable BindCommandExecuted(this IBindable bindable, IuFrameCommand sourceCommand, Action onExecuted)
    {
        return bindable.AddBinding(sourceCommand.Subscribe(delegate { onExecuted(); }));
    }

    /// <summary>
    /// The binding class that allows chaining extra options.
    /// </summary>
    /// <typeparam name="TBindingType">The type of the model property to bind to.</typeparam>
    /// <param name="bindable">The view that owns the binding</param>
    /// <param name="sourceProperty">The ViewModel property to bind to. Ex. ()=>Model.MyViewModelProperty</param>
    /// <param name="targetSetter">Should set the value of the target.</param>
    /// <returns>The binding class that allows chaining extra options.</returns>
    [Obsolete("Use other overload without function selector.")]
    public static IDisposable BindProperty<TBindingType>(this IBindable bindable, Func<P<TBindingType>> sourceProperty, Action<TBindingType> targetSetter)
    {
        return bindable.AddBinding(sourceProperty().Subscribe(targetSetter));
    }

    /// <summary>
    /// The binding class that allows chaining extra options.
    /// </summary>
    /// <typeparam name="TBindingType">The type of the model property to bind to.</typeparam>
    /// <param name="bindable">The view that owns the binding</param>
    /// <param name="sourceProperty">The ViewModel property to bind to. Ex. ()=>Model.MyViewModelProperty</param>
    /// <param name="targetSetter">Should set the value of the target.</param>
    /// <returns>The binding class that allows chaining extra options.</returns>
    [Obsolete("Use other overload without function selector.")]
    public static IDisposable BindProperty<TBindingType>(this IBindable bindable, Func<Computed<TBindingType>> sourceProperty, Action<TBindingType> targetSetter)
    {
        return bindable.AddBinding(sourceProperty().Subscribe(targetSetter));
    }

    /// <summary>
    /// Bind a ViewModel Collection to a View Collection.
    /// </summary>
    /// <typeparam name="TView">The view that owns the binding</typeparam>
    /// <typeparam name="TViewModelType"></typeparam>
    /// <param name="view"></param>
    /// <param name="sourceViewModelCollection"></param>
    /// <param name="viewCollection">The view collection is a list of ICollection that can be used to keep track of the Views created from the ViewModel Collection.</param>
    /// <param name="viewFirst"></param>
    /// <returns>The collection binding class that allows chaining extra options.</returns>
    [Obsolete("User other bindings, or regenerate this code.")]
    public static ModelViewModelCollectionBinding BindToViewCollection<TView, TViewModelType>(
        this ViewBase view,
        Func<ModelCollection<TViewModelType>> sourceViewModelCollection,
        ICollection<TView> viewCollection, bool viewFirst = false
        )
        where TView : ViewBase
        where TViewModelType : ViewModel
    {
        var binding = new ModelViewModelCollectionBinding()
        {
            SourceView = view,
            ModelPropertySelector = () => sourceViewModelCollection() as IObservableProperty
        }
        .SetAddHandler(v => viewCollection.Add(v as TView))
        .SetRemoveHandler(v => viewCollection.Remove(v as TView));

        if (viewFirst)
        {
            binding.ViewFirst();
        }
        view.AddBinding(binding);
        binding.Bind();
        return binding;
    }

    /// <summary>
    /// The binding class that allows chaining extra options.
    /// </summary>
    /// <typeparam name="TBindingType">The type of the model property to bind to.</typeparam>
    /// <param name="view">The view that owns the binding</param>
    /// <param name="sourceViewModelSelector">Selector for the ViewModel Property</param>
    /// <param name="setLocal">Set a local variable on your view to store the bound view.</param>
    /// <param name="getLocal">Get the local variable on your view used in this binding.</param>
    /// <returns>The binding class that allows chaining extra options.</returns>
    [Obsolete]
    public static ModelViewPropertyBinding BindToView<TBindingType>(this ViewBase view, Func<P<TBindingType>> sourceViewModelSelector, Action<ViewBase> setLocal = null, Func<ViewBase> getLocal = null)
    where TBindingType : ViewModel
    {
        var binding = new ModelViewPropertyBinding()
        {
            SourceView = view,
            ModelPropertySelector = () => (IObservableProperty)sourceViewModelSelector(),
            TwoWay = false
        };
        if (getLocal != null)
        {
            binding.GetTargetValueDelegate = () => getLocal();
            if (setLocal == null)
                throw new Exception("When using a BindToView you must set the setLocal parameter and getLocal parameter.");
            binding.SetTargetValueDelegate = (o) => setLocal((ViewBase)o);
        }

        view.AddBinding(binding);
        binding.Bind();
        return binding;
    }

    public static ModelViewModelCollectionBinding BindToViewCollection<TCollectionType>(this ViewBase view,
        Func<ModelCollection<TCollectionType>> viewModelCollection, Func<ViewModel,
        ViewBase> createView,
        Action<ViewBase> added,
        Action<ViewBase> removed,
        Transform parent,
        bool viewFirst = false)
    {
        var binding = new ModelViewModelCollectionBinding()
        {
            SourceView = view,
            ModelPropertySelector = () => viewModelCollection()
        };
        binding.SetParent(parent);
        binding.SetAddHandler(added);
        binding.SetRemoveHandler(removed);
        binding.SetCreateHandler(createView);
        view.AddBinding(binding);
        if (viewFirst)
        {
            binding.ViewFirst();
        }
        binding.Bind();

        return binding;
    }

    public static ModelViewModelCollectionBinding BindToViewCollection<TCollectionType>(this ViewBase view,
        ModelCollection<TCollectionType> viewModelCollection, Func<ViewModel,
        ViewBase> createView,
        Action<ViewBase> added,
        Action<ViewBase> removed,
        Transform parent,
        bool viewFirst = false)
    {
        var binding = new ModelViewModelCollectionBinding()
        {
            SourceView = view,
            ModelPropertySelector = () => viewModelCollection
        };
        binding.SetParent(parent);
        binding.SetAddHandler(added);
        binding.SetRemoveHandler(removed);
        binding.SetCreateHandler(createView);
        if (viewFirst)
        {
            binding.ViewFirst();
        }
        view.AddBinding(binding);
        binding.Bind();

        return binding;
    }

    /// <summary>
    /// Bind a ViewModel Collection
    /// </summary>
    /// <typeparam name="TCollectionType"></typeparam>
    /// <param name="view">The view that owns the binding</param>
    /// <param name="viewModelCollection">The view collection is a list of ICollection that can be used to keep track of the Views created from the ViewModel Collection.</param>
    /// <param name="viewFirst">Should the collection be initialized from the view. If false the View will be initialized to the ViewModel.</param>
    /// <returns>The Collection Binding class that allows chaining extra options.</returns>
    [Obsolete]
    public static ModelViewModelCollectionBinding BindToViewCollection<TCollectionType>(this ViewBase view,
        Func<ModelCollection<TCollectionType>> viewModelCollection, bool viewFirst = false)
    {
        var binding = new ModelViewModelCollectionBinding()
        {
            SourceView = view,
            ModelPropertySelector = () => viewModelCollection()
        };
        if (viewFirst)
        {
            binding.ViewFirst();
        }
        view.AddBinding(binding);
        return binding;
    }



}

public enum InputButtonEventType
{
    Button,
    ButtonDown,
    ButtonUp
}