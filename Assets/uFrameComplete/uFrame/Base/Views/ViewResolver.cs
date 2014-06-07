using System;
using UnityEngine;

/// <summary>
/// The View Managers responsibility is to provide prefabes based off of a view model
/// This implementation finds a prefab based off of the ViewModel's type name removing "View" from it.
/// </summary>
public class ViewResolver
{
    /// <summary>
    /// Provides a prefab
    /// </summary>
    /// <param name="model">The model for the view prefab we are looking for</param>
    /// <returns></returns>
    public virtual GameObject FindView(ViewModel model)
    {
        if (model == null)
        {
            throw new ArgumentNullException("model");
        }
        return FindView(model.GetType().Name.Replace("ViewModel", ""));
    }

    /// <summary>
    /// Provides a prefab based off a viewname
    /// </summary>
    /// <param name="viewName">The name of the view prefab we are looking for</param>
    /// <returns></returns>
    public virtual GameObject FindView(string viewName)
    {
        var viewPrefab = (GameObject)Resources.Load(viewName);
        if (viewPrefab == null)
            throw new Exception(string.Format("Could not find view prefab  `{0}`", viewName));

        return viewPrefab;
    }
}