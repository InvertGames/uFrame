using UnityEngine;

public class ViewEventTrigger : MonoBehaviour
{
    //public ViewBindingEventType _EventType;
    //public ViewBase _View;
    //public string _Event;

    //public bool _Filter = false;

    //public bool _ByName = false;
    //public bool _ByTag = false;
    //public bool _ByView = false;
    //public bool _ByLayer = false;

    //public string _NameFilter = string.Empty;
    //public string _TagFilter = string.Empty;
    //public LayerMask _LayersFilter = -1;

    //public string _ViewTypeFilter;

    //private Type _collisionViewType;

    //public Type CollisionViewType
    //{
    //    get
    //    {
    //        if (_collisionViewType == null && !string.IsNullOrEmpty(_ViewTypeFilter))
    //        {
    //            _collisionViewType = Assembly.GetAssembly(typeof(ViewEventTrigger)).GetType(_ViewTypeFilter);
    //        }
    //        return _collisionViewType;
    //    }
    //    set { _collisionViewType = value; }
    //}

    //void OnBecameInvisible()
    //{
    //    if (_EventType == ViewBindingEventType.OnBecameInvisible)
    //    {
    //        _View.Event(_Event);
    //    }
    //}
    //void OnBecameVisible()
    //{
    //    if (_EventType == ViewBindingEventType.OnBecameVisible)
    //    {
    //        _View.Event(_Event);
    //    }
    //}
    //void OnCollisionEnter(Collision collision)
    //{
    //    if (_EventType == ViewBindingEventType.OnCollisionEnter)
    //    {
    //        HandleColliderEvent(collision.collider);
    //    }
    //}
    //void OnCollisionExit(Collision collision)
    //{
    //    if (_EventType == ViewBindingEventType.OnCollisionExit)
    //    {
    //        HandleColliderEvent(collision.collider);
    //    }
    //}
    //void OnCollisionStay(Collision collision)
    //{
    //    if (_EventType == ViewBindingEventType.OnCollisionStay)
    //    {
    //        HandleColliderEvent(collision.collider);
    //    }
    //}
    //void OnMouseDown()
    //{
    //    if (_EventType == ViewBindingEventType.OnMouseDown)
    //    {
    //        _View.Event(_Event);
    //    }
    //}
    //void OnMouseDrag()
    //{
    //    if (_EventType == ViewBindingEventType.OnMouseDrag)
    //    {
    //        _View.Event(_Event);
    //    }
    //}
    //void OnMouseEnter()
    //{
    //    if (_EventType == ViewBindingEventType.OnMouseEnter)
    //    {
    //        _View.Event(_Event);
    //    }
    //}
    //void OnMouseExit()
    //{
    //    if (_EventType == ViewBindingEventType.OnMouseExit)
    //    {
    //        _View.Event(_Event);
    //    }
    //}
    //void OnMouseOver()
    //{
    //    if (_EventType == ViewBindingEventType.OnMouseOver)
    //    {
    //        _View.Event(_Event);
    //    }
    //}
    //void OnMouseUp()
    //{
    //    if (_EventType == ViewBindingEventType.OnMouseUp)
    //    {
    //        _View.Event(_Event);
    //    }
    //}

    //void OnMouseUpAsButton()
    //{
    //    if (_EventType == ViewBindingEventType.OnMouseUpAsButton)
    //    {
    //        _View.Event(_Event);
    //    }
    //}
    //void OnTriggerEnter(Collider other)
    //{
    //    if (_EventType == ViewBindingEventType.OnTriggerEnter)
    //    {
    //        HandleColliderEvent(other);
    //    }
    //}
    //void OnTriggerExit(Collider other)
    //{
    //    if (_EventType == ViewBindingEventType.OnTriggerExit)
    //    {
    //        HandleColliderEvent(other);
    //    }
    //}
    //void OnTriggerStay(Collider other)
    //{
    //    if (_EventType == ViewBindingEventType.OnTriggerStay)
    //    {
    //        HandleColliderEvent(other);
    //    }
    //}

    //private void HandleColliderEvent(Collider c)
    //{
    //    if (_Filter)
    //    {
    //        if (_ByLayer)
    //            if (((1 << c.gameObject.layer) & _LayersFilter) == 0)
    //                return;
    //        if (_ByTag)
    //            if (!c.CompareTag(_TagFilter))
    //                return;
    //        if (_ByName)
    //            if (c.name.ToUpper() != _NameFilter.ToUpper())
    //                return;

    //        var view = c.GetComponent<ViewBase>();

    //        if (_ByView )
    //        {
    //            if (view != null)
    //            {
    //                var viewType = view.GetType();
    //                if (viewType != CollisionViewType && !viewType.IsSubclassOf(CollisionViewType))
    //                    return;

    //                _View.Event(_Event, view.ViewModelObject);
    //            }
    //        }
    //        else
    //        {
    //            if (view != null)
    //                _View.Event(_Event, view.ViewModelObject);
    //            else
    //                _View.Event(_Event);
    //        }

    //    }
    //    else
    //    {
    //        var view = c.GetComponent<ViewBase>();

    //        if (view == null)
    //        {
    //            _View.Event(_Event);
    //            return;
    //        }

    //        _View.Event(_Event,view.ViewModelObject);
    //    }

    //}
}