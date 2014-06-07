using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using System.Reflection;
using UnityEngine;

/// <summary>
///  A data structure that contains information/data needed for a 'View'
/// </summary>
[Serializable]
public class ViewModel :  IJsonSerializable
{
    private Dictionary<string, ICommand> _commands;
    private Dictionary<string, ModelPropertyBase> _modelProperties;

    public Dictionary<string, ModelPropertyBase> Properties
    {
        get
        {
            LoadReflectedModelProperties();
            return _modelProperties;
        }
    }

    public Dictionary<string, ICommand> Commands
    {
        get
        {
            if (_commands == null)
            {
                var dictionary = new Dictionary<string, ICommand>();
                foreach (KeyValuePair<string, PropertyInfo> command in GetReflectedCommands(this.GetType()))
                    dictionary.Add(command.Key, (ICommand)command.Value.GetValue(this, null));
                _commands = dictionary;
            }
            return _commands;
        }
    }

    /// <summary>
    ///Access a model property via string.  This is optimized using a compiled delegate to
    ///access derived classes properties so use as needed
    /// </summary>
    /// <param name="bindingPropertyName">The name of the property/field to access</param>
    /// <returns>ModelPropertyBase The Model Property class.  Use value to get the value of the property</returns>
    public ModelPropertyBase this[string bindingPropertyName]
    {
        get
        {
            try
            {
                LoadReflectedModelProperties();
                return _modelProperties[bindingPropertyName];
            }
            catch (Exception ex)
            {
                throw new Exception(bindingPropertyName + " was not found on " + this.ToString() + ": " + ex.Message);
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    public static Dictionary<string, PropertyInfo> GetReflectedCommands(Type modelType)
    {
        var modelProperties = new Dictionary<string, PropertyInfo>();
        var fields = modelType.GetProperties();
        foreach (var field in fields)
        {
            if (typeof(ICommand).IsAssignableFrom(field.PropertyType))
            {
                modelProperties.Add(field.Name, field);
            }
        }
        return modelProperties;
    }

    public static Dictionary<string, FieldInfo> GetReflectedModelProperties(Type modelType)
    {
        var modelProperties = new Dictionary<string, FieldInfo>();
        var fields = modelType.GetFields();
        foreach (var field in fields.Where(p => p.IsPublic && p.IsInitOnly))
        {
            modelProperties.Add(field.Name, field);
        }
        return modelProperties;
    }

    public virtual void Deserialize(JSONNode node)
    {
        LoadReflectedModelProperties();
        foreach (var property in _modelProperties)
        {
            if (property.Value == null) continue;
            var val = node[property.Key];
            if (val == null) continue;
            property.Value.Deserialize(val);
        }
    }

    public ICommand ForwardThisTo<T>(ICommandWith<T> command) where T : ViewModel
    {
        if (command == null) return null;
        command.Sender = this as T;
        return new YieldCommandWith<ViewModel>(this, (vm) =>
        {
            command.Sender = this as T;
            return command.Execute();
        });
    }

    public ICommand ForwardThisTo<T>(Func<ICommandWith<T>> commandSelector) where T : ViewModel
    {
        if (commandSelector == null) return null;

        return new YieldCommandWith<ViewModel>(this, (vm) =>
        {
            var command = commandSelector();
            command.Sender = this as T;
            return command.Execute();
        });
    }

    /// <summary>
    /// Override this method to skip using reflection.  This can drastically improve performance especially IOS
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerable<ModelPropertyBase> GetProperties()
    {
        LoadReflectedModelProperties();
        return _modelProperties.Values.ToArray();
    }

    public virtual JSONNode Serialize()
    {
        LoadReflectedModelProperties();

        var node = new JSONClass();
        node.Add("TypeName", GetType().FullName);

        foreach (var property in _modelProperties)
        {
            if (property.Value == null) continue;
            node.Add(property.Key, property.Value.Serialize());
        }
        return node;
    }

    public override string ToString()
    {
        return Serialize().ToString();
    }

    protected ICommand Command(Action command)
    {
        return new Command(command);
    }

    protected ICommand Command(Func<IEnumerator> command)
    {
        return new YieldCommand(command);
    }

    private void LoadReflectedModelProperties()
    {
        if (_modelProperties != null) return;
        var dictionary = new Dictionary<string, ModelPropertyBase>();
        foreach (KeyValuePair<string, FieldInfo> property in GetReflectedModelProperties(this.GetType()))
            dictionary.Add(property.Key, (ModelPropertyBase)property.Value.GetValue(this));

        _modelProperties = dictionary;
    }
}