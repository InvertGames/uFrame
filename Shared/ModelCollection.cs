using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
#if DLL
using Invert.uFrame.Editor;
namespace Invert.MVVM
{
#else 

#endif
using UniRx;

public class ModelCollection<T> : ObservableCollection<T>
#if !DLL
    , IObservable<NotifyCollectionChangedEventArgs>,IObservableProperty
#endif
{
#if !DLL
    public ModelCollection(ViewModel owner, string propertyName)
    {
        Owner = owner;
        PropertyName = propertyName;
    }
#endif
    
    public ModelCollection()
    {
    }

    public IDisposable Subscribe(IObserver<NotifyCollectionChangedEventArgs> observer)
    {
        NotifyCollectionChangedEventHandler evt = (object sender, NotifyCollectionChangedEventArgs args) => observer.OnNext(args);
            
        CollectionChanged += evt;
        return new SimpleDisposable(() => CollectionChanged -= evt);
    }

    public object ObjectValue
    {
        get { return this; }
        set {  }
    }

    public string PropertyName { get; set; }
    public ViewModel Owner { get; set; }

    public Type ValueType
    {
        get
        {
            return typeof (ICollection<T>);
        }
    }

    public IObservable<Unit> AsUnit
    {
        get { return Observable.Select(this, _ => Unit.Default); }
    }

    public IDisposable SubscribeInternal(Action<object> propertyChanged)
    {
        return this.Subscribe((v) => { propertyChanged(v); });
    }

    public void AddRange(IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
            Add(item);
    }
    [Obsolete]
    public delegate void ModelCollectionChangedWith(ModelCollectionChangeEventWith<T> changeArgs);

    [Obsolete]
    public event ModelCollectionChangedWith CollectionChangedWith;
}

[Obsolete]
public enum ModelCollectionAction
{
    Add,
    Remove,
    Move,
    Replace,
    Reset
}

    //public delegate void ModelCollectionChanged(ModelCollectionChangeEvent changeArgs);

    //public interface IModelCollection
    //{
    //    event ModelCollectionChanged CollectionChanged;

    //    IEnumerable<object> Value { get; }
    //    Type ItemType { get; }
    //    void AddObject(object item);
    //    void RemoveObject(object item);
    //    void Clear();
    //}

    ///// <summary>
    ///// An observable collection to use in viewmodels.
    ///// </summary>
    //public class ModelCollection<T> : P<List<T>>, ICollection<T>, IModelCollection, IList<T>, INotifyPropertyChanged
    //{
    //    public delegate void ModelCollectionChangedWith(ModelCollectionChangeEventWith<T> changeArgs);

    //    public ModelCollection(List<T> value)
    //        : base(value)
    //    {
    //    }

    //    public event ModelCollectionChanged CollectionChanged;

    //    public event ModelCollectionChangedWith CollectionChangedWith;

    //    public int Count
    //    {
    //        get { return Value.Count; }
    //    }

    //    IEnumerable<object> IModelCollection.Value
    //    {
    //        get { return Value.Cast<object>(); }
    //    }

    //    public bool IsReadOnly
    //    {
    //        get { return false; }
    //    }


    //    public override Type ValueType
    //    {
    //        get { return typeof (List<T>); }
    //    }

    //    public Type ItemType
    //    {
    //        get { return typeof (T); }
    //    }

    //    public void AddObject(object item)
    //    {
    //        Add((T) item);
    //    }

    //    public void RemoveObject(object item)
    //    {
    //        Remove((T) item);
    //    }

    //    public ModelCollection(ViewModel owner, string propertyName)
    //        : base(owner, propertyName)
    //    {
    //        Value = new List<T>();
    //    }

    //    public ModelCollection(ViewModel owner, string propertyName, IEnumerable<T> enumerable)
    //        : base(owner, propertyName)
    //    {
    //        if (enumerable != null)
    //            Value = enumerable.ToList();
    //        else
    //        {
    //            Value = new List<T>();
    //        }
    //    }

    //    public ModelCollection()
    //    {
    //        Value = new List<T>();
    //    }

    //    public ModelCollection(IEnumerable<T> enumerable)
    //    {
    //        if (enumerable != null)
    //            Value = enumerable.ToList();
    //        else
    //        {
    //            Value = new List<T>();
    //        }
    //    }


    //    public virtual void Add(T item)
    //    {

    //        Value.Add(item);
    //        var value = new ModelCollectionChangeEventWith<T>()
    //        {
    //            Action = ModelCollectionAction.Add,
    //            NewItemsOfT = new[] {item}
    //        };

    //        OnChangedWith(value);
    //        OnPropertyChanged(value);
    //    }

    //    public override bool CanSetValue(List<T> value)
    //    {
    //        base.CanSetValue(value);
    //        if (value == null) return false;

    //        //var changed = new ModelCollectionChangeEventWith<T>()
    //        //{
    //        //    Action = ModelCollectionAction.Reset,
    //        //    NewItemsOfT = value.ToArray(),
    //        //    OldItemsOfT = ObjectValue == null ? null : Value.ToArray()
    //        //};

    //        //OnChangedWith(changed);
    //        return true;
    //    }

    //    public virtual void Clear()
    //    {
    //        var args = new ModelCollectionChangeEventWith<T>()
    //        {
    //            Action = ModelCollectionAction.Reset,
    //            NewItemsOfT = null,
    //            OldItemsOfT = Value.ToArray()
    //        };

    //        Value.Clear();
    //        OnChangedWith(args);
    //    }

    //    public virtual bool Contains(T item)
    //    {
    //        return Value.Contains(item);
    //    }

    //    public void CopyTo(T[] array, int arrayIndex)
    //    {
    //        Value.CopyTo(array, arrayIndex);
    //    }

    //    public override void Deserialize(JSONNode node)
    //    {
    //        Value = new List<T>();
    //        var array = node.AsArray;
    //        foreach (var item in array.Childs)
    //        {
    //            if (item is JSONClass)
    //            {
    //                var typeName = item["TypeName"];
    //                var type = Type.GetType(typeName);
    //                Add((T) DeserializeObject(type, item));
    //            }
    //            else
    //            {
    //                Add((T) DeserializeObject(typeof (T), item));
    //            }
    //        }
    //    }

    //    public IEnumerator<T> GetEnumerator()
    //    {
    //        return Value.GetEnumerator();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }

    //    public virtual bool Remove(T item)
    //    {
    //        var result = Value.Remove(item);
    //        OnChangedWith(new ModelCollectionChangeEventWith<T>()
    //        {
    //            Action = ModelCollectionAction.Remove,
    //            OldItemsOfT = new[] {item}
    //        });
    //        return result;
    //    }

    //    public override JSONNode Serialize()
    //    {
    //        var jsonArray = new JSONArray();
    //        foreach (var item in Value)
    //        {
    //            var itemType = item.GetType();
    //            var node = SerializeObject(itemType, item);
    //            if (node is JSONClass)
    //            {
    //                node.Add("TypeName", itemType.AssemblyQualifiedName);
    //            }
    //            jsonArray.Add(node);
    //        }
    //        return jsonArray;
    //    }

    //    //protected List<T> _list = new List<T>();
    //    public override string ToString()
    //    {
    //        return Serialize().ToString();
    //    }

    //    protected virtual void OnChangedWith(ModelCollectionChangeEventWith<T> changeargs)
    //    {
    //        ModelCollectionChanged changed = CollectionChanged;
    //        if (changed != null)
    //            changed(changeargs);

    //        ModelCollectionChangedWith handler = CollectionChangedWith;
    //        if (handler != null)
    //            handler(changeargs);
    //    }

    //    public void AddRange(IEnumerable<T> value)
    //    {
    //        foreach (var item in value)
    //        {
    //            Add(item);
    //        }
    //    }

    //    public int IndexOf(T item)
    //    {
    //        return Value.IndexOf(item);
    //    }

    //    public void Insert(int index, T item)
    //    {
    //        Value.Insert(index, item);
    //    }

    //    public void RemoveAt(int index)
    //    {
    //        Value.RemoveAt(index);
    //    }

    //    public T this[int index]
    //    {
    //        get { return Value[index]; }
    //        set { Value[index] = value; }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected virtual void OnPropertyChanged(string propertyName)
    //    {
    //        PropertyChangedEventHandler handler = PropertyChanged;
    //        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //}
[Obsolete]
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
[Obsolete]
public class ModelCollectionChangeEventWith<T> : ModelCollectionChangeEvent
{
    public T[] NewItemsOfT
    {
        get { return NewItems.Cast<T>().ToArray(); }
        set
        {
            if (value == null) return;
            NewItems = value.Cast<object>().ToArray();
        }
    }

    public T[] OldItemsOfT
    {
        get { return OldItems.Cast<T>().ToArray(); }
        set
        {
            if (value == null) return;
            OldItems = value.Cast<object>().ToArray();
        }
    }
}

#if DLL
}
#endif