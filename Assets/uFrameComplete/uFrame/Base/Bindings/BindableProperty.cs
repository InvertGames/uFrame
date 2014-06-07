using System;
using System.Reflection;

/// <summary>
/// A wrapper for any class property so it can easily be bound to.
/// </summary>
public sealed class BindableProperty
{
    private Func<object> _getDelegate;

    public MemberInfo BindableMember { get; set; }

    public object BindableObject { get; set; }

    public Func<object> GetDelegate
    {
        get
        {
            return _getDelegate ?? (_getDelegate = CreateGetDelegate());
        }
    }


    /// <summary>
    /// Get the value of the property
    /// </summary>
    public object Value
    {
        get
        {
            return GetDelegate();
        }
        set
        {
            var info = BindableMember as FieldInfo;
            if (info != null)
            {
                if (info.FieldType == typeof(string))
                {
                    info.SetValue(BindableObject, value.ToString());
                }
                else
                {
                    info.SetValue(BindableObject, value);
                }
            }
            else
            {
                var pInfo = BindableMember as PropertyInfo;
                if (pInfo != null)
                {
                    if (pInfo.PropertyType == typeof(string))
                    {
                        pInfo.SetValue(BindableObject, value.ToString(), null);
                    }
                    else
                    {
                        pInfo.SetValue(BindableObject, value, null);
                    }
                }
            }
        }
    }

    public BindableProperty(object bindableObject, MemberInfo bindableMember)
    {
        BindableObject = bindableObject;
        BindableMember = bindableMember;
    }

    private Func<object> CreateGetDelegate()
    {
        return () =>
        {
            var fieldInfo = BindableMember as FieldInfo;
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(BindableObject);
            }
            else
            {
                var propertyInfo = BindableMember as PropertyInfo;
                if (propertyInfo != null)
                {
                    propertyInfo.GetValue(BindableObject, null);
                }
            }

            return null;
        };
        //var fieldExp = Expression.PropertyOrField(Expression.Constant(BindableObject), BindableMember.Name);
        //return Expression.Lambda<Func<object>>(fieldExp).Compile();
    }
}