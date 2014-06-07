using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum ModelCollectionAction
{
    Add,
    Remove,
    Move,
    Replace,
    Reset
}

public delegate void ModelCollectionChanged(ModelCollectionChangeEvent changeArgs);

public interface IModelCollection
{
    event ModelCollectionChanged Changed;

    IEnumerable<object> Value { get; }
    Type ItemType { get; }
    void AddObject(object item);
    void RemoveObject(object item);
    void Clear();
}

/// <summary>
/// An observable collection to use in viewmodels.
/// </summary>
public class ModelCollection<T> : P<List<T>>, ICollection<T>, IModelCollection
{
    public delegate void ModelCollectionChangedWith(ModelCollectionChangeEventWith<T> changeArgs);

    public event ModelCollectionChanged Changed;

    public event ModelCollectionChangedWith ChangedWith;

    public int Count
    {
        get
        {
            return Value.Count;
        }
    }

    IEnumerable<object> IModelCollection.Value
    {
        get
        {
            return Value.Cast<object>();
        }
    }

    public bool IsReadOnly { get { return false; } }

    public Action<T> OnAdd { get; set; }

    public Action<T> OnRemove { get; set; }

    public override Type ValueType
    {
        get
        {
            return typeof(List<T>);
        }
    }

    public Type ItemType
    {
        get { return typeof (T); }
    }

    public void AddObject(object item)
    {
        Add((T)item);
    }

    public void RemoveObject(object item)
    {
        Remove((T) item);
    }

    public ModelCollection()
    {
        Value = new List<T>();
    }

    public ModelCollection(IEnumerable<T> enumerable)
    {
        if (enumerable != null)
            Value = enumerable.ToList();
    }

    public virtual void Add(T item)
    {
        Value.Add(item);
        var value = new ModelCollectionChangeEventWith<T>()
        {
            Action = ModelCollectionAction.Add,
            NewItemsOfT = new[] { item }
        };

        OnChangedWith(value);
    }

    public override bool CanSetValue(List<T> value)
    {
        base.CanSetValue(value);
        if (value == null) return false;

        var changed = new ModelCollectionChangeEventWith<T>()
        {
            Action = ModelCollectionAction.Reset,
            NewItemsOfT = value.ToArray(),
            OldItemsOfT = ObjectValue == null ? null : Value.ToArray()
        };

        OnChangedWith(changed);
        return true;
    }

    public virtual void Clear()
    {
        var args = new ModelCollectionChangeEventWith<T>()
        {
            Action = ModelCollectionAction.Reset,
            NewItemsOfT = null,
            OldItemsOfT = Value.ToArray()
        };

        Value.Clear();
        OnChangedWith(args);
    }

    public virtual bool Contains(T item)
    {
        return Value.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Value.CopyTo(array, arrayIndex);
    }

    public override void Deserialize(JSONNode node)
    {
        Value = new List<T>();
        var array = node.AsArray;
        foreach (var item in array.Childs)
        {
            if (item is JSONClass)
            {
                var typeName = item["TypeName"];
                var type = Type.GetType(typeName);
                Add((T)DeserializeObject(type, item));
            }
            else
            {
                Add((T)DeserializeObject(typeof(T), item));
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Value.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public virtual bool Remove(T item)
    {
        var result = Value.Remove(item);
        OnChangedWith(new ModelCollectionChangeEventWith<T>()
        {
            Action = ModelCollectionAction.Remove,
            OldItemsOfT = new[] { item }
        });
        return result;
    }

    public override JSONNode Serialize()
    {
        var jsonArray = new JSONArray();
        foreach (var item in Value)
        {
            var itemType = item.GetType();
            var node = SerializeObject(itemType, item);
            if (node is JSONClass)
            {
                node.Add("TypeName", itemType.AssemblyQualifiedName);
            }
            jsonArray.Add(node);
        }
        return jsonArray;
    }

    //protected List<T> _list = new List<T>();
    public override string ToString()
    {
        return Serialize().ToString();
    }

    protected virtual void OnChangedWith(ModelCollectionChangeEventWith<T> changeargs)
    {
        ModelCollectionChanged changed = Changed;
        if (changed != null)
            changed(changeargs);

        ModelCollectionChangedWith handler = ChangedWith;
        if (handler != null)
            handler(changeargs);
    }
}

public class ModelCollectionChangeEvent
{
    private object[] _newItems;
    private object[] _oldItems;

    public ModelCollectionAction Action { get; set; }

    public object[] NewItems
    {
        get { return _newItems ?? (_newItems = new object[] { }); }
        set { _newItems = value; }
    }

    public object[] OldItems
    {
        get { return _oldItems ?? (_oldItems = new object[] { }); }
        set { _oldItems = value; }
    }
}

public class ModelCollectionChangeEventWith<T> : ModelCollectionChangeEvent
{
    public T[] NewItemsOfT
    {
        get
        {
            return NewItems.Cast<T>().ToArray();
        }
        set
        {
            if (value == null) return;
            NewItems = value.Cast<object>().ToArray();
        }
    }

    public T[] OldItemsOfT
    {
        get
        {
            return OldItems.Cast<T>().ToArray();
        }
        set
        {
            if (value == null) return;
            OldItems = value.Cast<object>().ToArray();
        }
    }
}