using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// A base class for all view containers.
/// Simply just utility methods for views and events.
/// </summary>
public class ViewContainer : ViewModelObserver
{
    public virtual TView CreateView<TView>() where TView : ViewBase
    {
        return CreateView<TView>(null);
    }

    public virtual TView CreateView<TView>(ViewModel model) where TView : ViewBase
    {
        return CreateView<TView>(model, Vector3.zero);
    }

    public virtual TView CreateView<TView>(ViewModel model, Vector3 position) where TView : ViewBase
    {
        return CreateView<TView>(model, position, new Quaternion());
    }

    public virtual TView CreateView<TView>(ViewModel model, Vector3 position, Quaternion rotation) where TView : ViewBase
    {
        var viewGo = new GameObject(typeof(TView).Name, typeof(TView));

        viewGo.transform.position = position;
        viewGo.transform.rotation = rotation;
        var view = transform.InitializeView(viewGo.name, model, viewGo);

        return view as TView;
    }

    public ViewBase InstantiateView(ViewModel model)
    {
        return InstantiateView(model, Vector3.zero);
    }

    public ViewBase InstantiateView(ViewModel model, Vector3 position)
    {
        return InstantiateView(model, position, Quaternion.identity);
    }

    public ViewBase InstantiateView(ViewModel model, Vector3 position, Quaternion rotation)
    {
        return transform.InstantiateView(model, position, rotation);
    }

    public ViewBase InstantiateView(GameObject prefab, ViewModel model)
    {
        return InstantiateView(prefab, model, Vector3.zero);
    }

    public ViewBase InstantiateView(GameObject prefab, ViewModel model, Vector3 position)
    {
        return InstantiateView(prefab, model, position, Quaternion.identity);
    }

    public ViewBase InstantiateView(string viewName)
    {
        return InstantiateView(viewName, null);
    }

    /// <summary>
    /// Instantiates a view.
    /// </summary>
    /// <param name="viewName">The name of the prefab/view to instantiate</param>
    /// <param name="model">The model that will be passed to the view.</param>
    /// <returns>The instantiated view</returns>
    public ViewBase InstantiateView(string viewName, ViewModel model)
    {
        return InstantiateView(viewName, model, Vector3.zero);
    }
    /// <summary>
    /// Instantiates a view.
    /// </summary>
    /// <param name="viewName">The name of the prefab/view to instantiate</param>
    
    /// <param name="position">The position to instantiate the view.</param>
    /// <returns>The instantiated view</returns>
    public ViewBase InstantiateView(string viewName, Vector3 position)
    {
        return InstantiateView(viewName, null, position, Quaternion.identity);
    }

    /// <summary>
    /// Instantiates a view.
    /// </summary>
    /// <param name="viewName">The name of the prefab/view to instantiate</param>
    /// <param name="model">The model that will be passed to the view.</param>
    /// <param name="position">The position to instantiate the view.</param>
    /// <returns>The instantiated view</returns>
    public ViewBase InstantiateView(string viewName, ViewModel model, Vector3 position)
    {
        return InstantiateView(viewName, model, position, Quaternion.identity);
    }

    /// <summary>
    /// Instantiates a view.
    /// </summary>
    /// <param name="viewName">The name of the prefab/view to instantiate</param>
    /// <param name="model">The model that will be passed to the view.</param>
    /// <param name="position">The position to instantiate the view.</param>
    /// <param name="rotation">The rotation to instantiate the view with.</param>
    /// <returns>The instantiated view</returns>
    public ViewBase InstantiateView(string viewName, ViewModel model, Vector3 position,
        Quaternion rotation)
    {
        return transform.InstantiateView(viewName, model, position, rotation);
    }

    /// <summary>
    /// Instantiates a view.
    /// </summary>
    /// <param name="prefab">The prefab/view to instantiate</param>
    /// <param name="model">The model that will be passed to the view.</param>
    /// <param name="position">The position to instantiate the view.</param>
    /// <param name="rotation">The rotation to instantiate the view with.</param>
    /// <returns>The instantiated view</returns>
    public ViewBase InstantiateView(GameObject prefab, ViewModel model, Vector3 position,
        Quaternion rotation)
    {
        return transform.InstantiateView(prefab, model, position, rotation);
    }

    public Coroutine LoadAdditive(string rootObjectName, string levelName, Action<GameObject> complete = null)
    {
        return StartCoroutine(LoadAdditiveInternal(rootObjectName, levelName, complete));
    }

    public Coroutine Task(Func<IEnumerator> coroutine)
    {
        return StartCoroutine(coroutine());
    }

    private IEnumerator LoadAdditiveInternal(string rootObjectName, string levelName, Action<GameObject> complete)
    {
        if (GameManager.IsPro)
        {
            var async = Application.LoadLevelAdditiveAsync(levelName);
            while (!async.isDone)
            {
                yield return null;
            }
        }
        else
        {
            Application.LoadLevelAdditive(levelName);
        }
        
        //yield return new WaitForSeconds(1f);
        var rootObject = GameObject.Find(rootObjectName);

        if (rootObject != null)
        {
            //rootObject.transform.parent = this.transform;
            if (complete != null)
                complete(rootObject);
        }
        else
        {
            throw new Exception(string.Format("Root object {0} could not be found.", rootObjectName));
        }
    }

    //protected void Event(string controllerName, string message)
    //{
    //    Event(controllerName, message, null);
    //}

    //protected void Event(string controllerName, string message, ViewModel model, params object[] additionalParameters)
    //{
    //    var c = GameManager.ActiveGame[controllerName];
    //    if (c == null)
    //    {
    //        throw new Exception(string.Format("Controller {0} was not found while trying to send event {1}.", controllerName, message));
    //    }

    //    Event(c, model, message, additionalParameters);
    //}
}