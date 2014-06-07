using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class ModelCollectionBinding<TCollectionType> : Binding
{
    private bool _isImmediate = true;

    public ModelCollection<TCollectionType> Collection
    {
        get
        {
            return ModelProperty as ModelCollection<TCollectionType>;
        }
    }

    public bool IsImmediate
    {
        get { return _isImmediate; }
        set { _isImmediate = value; }
    }

    public Action<TCollectionType> OnAdd { get; set; }

    public Action<TCollectionType> OnRemove { get; set; }

    public override void Bind()
    {
        base.Bind();
        Collection.Changed += CollectionOnChanged;
        if (IsImmediate)
        {
            BindNow();
        }
    }

    public void Immediate()
    {
        if (IsBound)
        {
            IsImmediate = true;
            BindNow();
        }
        else
        {
            IsImmediate = true;
        }
    }

    public ModelCollectionBinding<TCollectionType> SetAddHandler(Action<TCollectionType> onAddHandler)
    {
        OnAdd = onAddHandler;
        return this;
    }

    public ModelCollectionBinding<TCollectionType> SetRemoveHandler(Action<TCollectionType> onRemoveHandler)
    {
        OnRemove = onRemoveHandler;
        return this;
    }

    public override void Unbind()
    {
        Collection.Changed -= CollectionOnChanged;
        base.Unbind();
    }

    private void BindNow()
    {
        CollectionOnChanged(new ModelCollectionChangeEvent()
        {
            NewItems = Collection.Cast<object>().ToArray()
        });
    }

    private void CollectionOnChanged(ModelCollectionChangeEvent changeArgs)
    {
        if (changeArgs.NewItems != null)
            foreach (var newItem in changeArgs.NewItems)
            {
                if (OnAdd != null)
                    OnAdd((TCollectionType)newItem);
            }
        if (changeArgs.OldItems != null)
            foreach (var oldItem in changeArgs.OldItems)
            {
                if (OnRemove != null)
                    OnRemove((TCollectionType)oldItem);
            }
    }
}

/// <summary>
/// Class for a view collection binding. Binds a ViewModel collection to a set of corresponding Views
/// </summary>
public class ModelViewModelCollectionBinding : Binding
{
    private bool _isImmediate = true;
    private bool _viewFirst = false;
    private Dictionary<int, GameObject> _gameObjectLookup = new Dictionary<int, GameObject>();
    private Dictionary<ViewModel, int> _objectIdLookup;

    public IModelCollection Collection
    {
        get
        {
            return ModelProperty as IModelCollection;
        }
    }

    public bool IsImmediate
    {
        get { return _isImmediate; }
        set { _isImmediate = value; }
    }

    public Action<ViewBase> OnAddView { get; set; }

    public Func<ViewModel, ViewBase> OnCreateView { get; set; }

    public Action<ViewBase> OnRemoveView { get; set; }

    public Transform Parent { get; set; }

    public string ViewName { get; set; }

    public ModelViewModelCollectionBinding Immediate(bool immediate = true)
    {
        IsImmediate = immediate;
        return this;
    }

    public ModelViewModelCollectionBinding SetAddHandler(Action<ViewBase> onAdd)
    {
        OnAddView = onAdd;
        return this;
    }

    public ModelViewModelCollectionBinding SetCreateHandler(Func<ViewModel, ViewBase> onCreateView)
    {
        OnCreateView = onCreateView;
        return this;
    }

    public ModelViewModelCollectionBinding SetParent(Transform parent)
    {
        Parent = parent;
        return this;
    }

    public ModelViewModelCollectionBinding SetRemoveHandler(Action<ViewBase> onRemove)
    {
        OnRemoveView = onRemove;
        return this;
    }

    public ModelViewModelCollectionBinding SetView(string viewName)
    {
        ViewName = viewName;
        return this;
    }

    public override void Unbind()
    {
        Collection.Changed -= CollectionOnChanged;
        GameObjectLookup.Clear();
        base.Unbind();
    }

    public Dictionary<int, GameObject> GameObjectLookup
    {
        get { return _gameObjectLookup ?? (_gameObjectLookup = new Dictionary<int, GameObject>()); }
        set { _gameObjectLookup = value; }
    }

    public Dictionary<ViewModel, int> ObjectIdLookup
    {
        get { return _objectIdLookup ?? (_objectIdLookup = new Dictionary<ViewModel, int>()); }
        set { _objectIdLookup = value; }
    }

    protected void AddLookup(GameObject obj, ViewModel viewModel)
    {
        if (obj == null || viewModel == null) return;
        var instanceId = obj.GetInstanceID();
        GameObjectLookup.Add(instanceId, obj);
        ObjectIdLookup.Add(viewModel, instanceId);

    }

    protected void RemoveLookup(ViewModel model)
    {
        if (ObjectIdLookup.ContainsKey(model))
        {
            var instanceId = ObjectIdLookup[model];
            ObjectIdLookup.Remove(model);
            var go = GameObjectLookup[instanceId];
            GameObjectLookup.Remove(instanceId);
            if (OnRemoveView != null)
            {
                OnRemoveView(go.GetView());
            }
            else
            {
                Object.Destroy(go);
            }
        }
    }
    public override void Bind()
    {
        base.Bind();


        // If we are syncing from the collection first on not the scene
        if (!_viewFirst)
        {
            var targetTransform = Parent;
            if (targetTransform != null)
            {
                for (var i = 0; i < targetTransform.childCount; i++)
                {

                    Object.Destroy(targetTransform.GetChild(i).gameObject);

                }
            }
        }
        else
        {
            var targetTransform = Parent ?? Source.transform;
            if (targetTransform != null)
            {
                for (var i = 0; i < targetTransform.childCount; i++)
                {
                    var view = targetTransform.GetChild(i).GetView();
                    if (view != null)
                    {
                        if (view.ViewModelObject == null)
                        {
                            view.ViewModelObject = view.CreateModel();
                        }
                        Collection.AddObject(view.ViewModelObject);
                        AddLookup(view.gameObject, view.ViewModelObject);

                        if (OnAddView != null)
                            OnAddView(view);
                    }
                }
            }
        }
        Collection.Changed += CollectionOnChanged;
        if (!_viewFirst && IsImmediate)
        {
            CollectionOnChanged(new ModelCollectionChangeEvent
            {
                Action = ModelCollectionAction.Reset,
                NewItems = Collection.Value.ToArray()
            });
        }
    }

    private void CollectionOnChanged(ModelCollectionChangeEvent changeArgs)
    {
        var targetTransform = Parent ?? Source.transform;
        if (changeArgs.NewItems != null)
            foreach (var item in changeArgs.NewItems)
            {
                ViewBase view = null;
                if (OnCreateView != null)
                {
                    view = OnCreateView(item as ViewModel);
                }
                else
                {
                    view = ViewName == null
                        ? Source.InstantiateView(item as ViewModel)
                        : Source.InstantiateView(ViewName, item as ViewModel) as ViewBase;
                }
                if (view != null)
                {
                    AddLookup(view.gameObject, item as ViewModel);
                    view.transform.parent = targetTransform;
                    if (OnAddView != null)
                    {
                        OnAddView(view);
                    }
                }
            }

        if (changeArgs.OldItems != null &&
            changeArgs.OldItems.Length > 0)
        {
            foreach (var oldItem in changeArgs.OldItems)
            {
                RemoveLookup(oldItem as ViewModel);
            }

        }
    }

    public void ViewFirst()
    {
        _viewFirst = true;
    }
}