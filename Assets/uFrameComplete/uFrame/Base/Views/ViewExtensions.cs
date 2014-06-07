using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ViewExtensions
{
    public static ViewBase GetView(this GameObject go)
    {
        return go.GetComponent<ViewBase>();
    }

    public static T GetView<T>(this GameObject go) where T : class
    {
        return GetView(go) as T;
    }

    public static ViewBase GetView(this Collision go)
    {
        return GetView(go.collider);
    }

    public static T GetView<T>(this Collision go) where T : class
    {
        return GetView(go.collider) as T;
    }

    public static ViewBase GetView(this Transform go)
    {
        return go.GetComponent<ViewBase>();
    }

    public static T GetView<T>(this Transform go) where T : class
    {
        return GetView(go) as T;
    }

    public static ViewBase GetView(this Collider go)
    {
        return go.GetComponent<ViewBase>();
    }

    public static T GetView<T>(this Collider go) where T : class
    {
        return GetView(go) as T;
    }

    public static ViewBase GetView(this MonoBehaviour go)
    {
        if (go == null) return null;
        return go.GetComponent<ViewBase>();
    }

    public static T GetView<T>(this MonoBehaviour go) where T : class
    {
        return GetView(go) as T;
    }

    public static ViewModel GetViewModel(this GameObject go)
    {
        var view = GetView(go);
        if (view == null)
            return null;
        return view.ViewModelObject;
    }

    public static T GetViewModel<T>(this GameObject go) where T : class
    {
        return GetViewModel(go) as T;
    }

    public static T GetViewModel<T>(this Collision go) where T : class
    {
        return GetViewModel(go.collider) as T;
    }

    public static ViewModel GetViewModel(this Transform go)
    {
        var view = GetView(go);
        if (view == null)
            return null;
        return view.ViewModelObject;
    }

    public static T GetViewModel<T>(this Transform go) where T : class
    {
        return GetViewModel(go) as T;
    }

    public static ViewModel GetViewModel(this Collider go)
    {
        var view = GetView(go);
        if (view == null)
            return null;
        return view.ViewModelObject;
    }

    public static ViewModel GetViewModel(this Collision go)
    {
        var view = GetView(go.collider);
        if (view == null)
            return null;
        return view.ViewModelObject;
    }

    public static T GetViewModel<T>(this Collider go) where T : class
    {
        return GetViewModel(go) as T;
    }

    public static ViewModel GetViewModel(this MonoBehaviour go)
    {
        var view = GetView(go);
        if (view == null)
            return null;
        return view.ViewModelObject;
    }

    public static T GetViewModel<T>(this MonoBehaviour go) where T : class
    {
        return GetViewModel(go) as T;
    }

    public static ViewBase InitializeView(this Transform parent, string name, ViewModel model, GameObject viewObject)
    {
        var view = viewObject.GetComponent<ViewBase>();

        if (view == null)
        {
            Object.Destroy(viewObject);
            throw new Exception(string.Format("View Object does not have a 'View<{0}>' component.", model.GetType().Name));
        }

        view.transform.parent = parent;
        view.ParentView = parent.GetView();
        view.ViewName = name;
       
        if (model != null)
        {
            view.OverrideViewModel = false;
            view.ForceResolveViewModel = false;
            view.ViewModelObject = model;
            //view.InitializeViewModel(model);
            view.SetupBindings();
        }

        return view;
    }

    public static ViewBase InstantiateView(this Transform parent, GameObject prefab, ViewModel model, bool registerModel = false)
    {
        return InstantiateView(parent, prefab, model, Vector3.zero);
    }

    public static ViewBase InstantiateView(this Transform parent, string viewName, ViewModel model)
    {
        return InstantiateView(parent, viewName, model, Vector3.zero);
    }

    public static ViewBase InstantiateView(this Transform parent, string viewName, ViewModel model, Vector3 position)
    {
        return InstantiateView(parent, viewName, model, position, Quaternion.identity);
    }

    public static ViewBase InstantiateView(this Transform parent, string viewName, ViewModel model, Vector3 position, Quaternion rotation)
    {
        return InstantiateView(parent, UFrame.ViewManager.FindView(viewName), model, position, rotation);
    }

    public static ViewBase InstantiateView(this Transform parent, GameObject prefab, ViewModel model, Vector3 position)
    {
        return InstantiateView(parent, prefab, model, position, Quaternion.identity);
    }

    public static ViewBase InstantiateView(this Transform parent, GameObject prefab, ViewModel model, Vector3 position, Quaternion rotation)
    {
        // Create the view object from the specified prefab
        var viewObject = (GameObject)Object.Instantiate(prefab, position, rotation);

        var view = InitializeView(parent, prefab.name, model, viewObject);

        return view;
    }

    public static ViewBase InstantiateView(this Transform parent, ViewModel model)
    {
        return InstantiateView(parent, model, Vector3.zero);
    }

    public static ViewBase InstantiateView(this Transform parent, ViewModel model, Vector3 position)
    {
        return InstantiateView(parent, model, position, Quaternion.identity);
    }

    public static ViewBase InstantiateView(this Transform parent, ViewModel model, Vector3 position, Quaternion rotation)
    {
        return InstantiateView(parent, UFrame.ViewManager.FindView(model), model, position, rotation);
    }

    public static bool IsView<TView>(this Transform go) where TView : ViewBase
    {
        if (go == null) return false;
        var c = go.GetComponent<TView>();
        return c != null;
    }

    public static bool IsView<TView>(this GameObject go) where TView : ViewBase
    {
        if (go == null) return false;
        var c = go.GetComponent<TView>();
        return c != null;
    }
}