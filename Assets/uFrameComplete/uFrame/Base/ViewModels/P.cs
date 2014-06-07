using System;
using System.Diagnostics;
/// <summary>
/// A typed ViewModel Property Class
/// </summary>
/// <typeparam name="T"></typeparam>
public class P<T> : ModelPropertyBase
{
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    public T Value
    {
        get
        {
            if (ObjectValue == null)
                return default(T);

            try
            {
                return (T)ObjectValue;
            }
            catch
            {
                //UnityEngine.Debug.LogError(ObjectValue.GetType().FullName + " TO " + typeof(T).FullName + " : " + ex.Message);
                return default(T);
            }
        }
        set
        {
            if (CanSetValue(value))
            {
                ObjectValue = value;
            }
        }
    }

    /// <summary>
    /// Gets the type of the value.
    /// </summary>
    /// <value>The type of the value.</value>
    public override Type ValueType
    {
        get
        {
            return typeof(T);
        }
    }

    public P()
    {
        Value = default(T);
    }

    public P(T value)
    {
        Value = value;
    }

    ///// <summary>
    ///// Bind the specified target to this property.
    ///// </summary>
    ///// <param name="target">Target.</param>
    ///// <typeparam name="TBindingType">The 1st type parameter.</typeparam>
    //public void Bind(Action<T> target, bool immediate = true)
    //{
    //    Action<object> bindMethod = delegate(object v)
    //    {
    //        target((T)v);
    //    };

    //    PropertyChanged += new ModelPropertyBase.PropertyChangedHandler(bindMethod);
    //    if (immediate)
    //        bindMethod(Value);
    //}

    public virtual bool CanSetValue(T value)
    {
        return true;
    }

    /// <summary>
    /// Deserialize the specified node into `Value`.
    /// </summary>
    /// <param name="node">Node.</param>
    public override void Deserialize(JSONNode node)
    {
        this.ObjectValue = DeserializeObject(ValueType, node);
    }

    //public override int GetHashCode()
    //{
    //    return Value.GetHashCode();
    //}

    #region Operator Overloads

    public override bool Equals(object obj)
    {
        if (obj is P<T>)
        {
            var tObj = (P<T>)obj;
            return Value.Equals(tObj.Value);
        }
        return Value.Equals(obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    //public static bool operator ==(P<T> a, P<T> b)
    //{
    //    return a.Value.Equals(b.Value);
    //}

    //public static bool operator !=(P<T> a, P<T> b)
    //{
    //    if (a == null || b == null)
    //    {
    //        return !(a == null && b == null);
    //    }
    //    return !(a.Value.Equals(b.Value));
    //}

    //public static bool operator ==(P<T> a, T b)
    //{
    //    return a.Value.Equals(b);
    //}

    //public static bool operator !=(P<T> a, T b)
    //{
    //    return !(a.Value.Equals(b));
    //}

    //public static implicit operator P<T>(T d)
    //{
    //    return new P<T> { Value = d };
    //}

    ////  User-defined conversion from double to Digit
    //public static implicit operator T(P<T> d)
    //{
    //    return d.Value;
    //}

    #endregion Operator Overloads

    /// <summary>
    /// Serializes this object
    /// </summary>
    public override JSONNode Serialize()
    {
        var value = ObjectValue;
        return SerializeObject(ValueType, value);
    }
}