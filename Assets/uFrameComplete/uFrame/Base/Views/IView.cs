using System;
using System.Collections.Generic;
using UnityEngine;

public interface IView
#if !UNITY_EDITOR
   : IViewModelObserver
#endif
{
    /// <summary>
    /// Gets the view model object.
    /// </summary>
    /// <value>The view model object.</value>
    ViewModel ViewModelObject { get; }

    /// <summary>
    /// Gets the type of the view model.
    /// </summary>
    /// <value>The type of the model.</value>
    Type ViewModelType { get; }

    /// <summary>
    /// The name of the prefab that created this view
    /// </summary>
    string ViewName { get; set; }
}