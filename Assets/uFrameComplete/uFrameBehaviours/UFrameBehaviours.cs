using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class UFrameBehaviours : UBSharedBehaviour, IBindingProvider
{
    public static void CreateUBehaviour()
    {

    }
    [SerializeField]
    private string _ViewModelType;
    
    public Type ViewModelType
    {
        get
        {
            return UBHelper.GetType(ViewModelTypeString);
        }
        set { ViewModelTypeString = value.FullName; }
    }

    [SerializeField]
    [HideInInspector]
    private List<UBActionSheet> _commandActionSheets = new List<UBActionSheet>();

    [SerializeField]
    [HideInInspector]
    private List<UBActionSheet> _propertyActionSheets = new List<UBActionSheet>();

    private IUBContext _instance;

    public List<UBActionSheet> CommandActionSheets
    {
        get { return _commandActionSheets; }
        set { _commandActionSheets = value; }
    }

    public List<UBActionSheet> PropertyActionSheets
    {
        get { return _propertyActionSheets; }
        set { _propertyActionSheets = value; }
    }

    public string ViewModelTypeString
    {
        get { return _ViewModelType; }
        set { _ViewModelType = value; }
    }

    public override void Initialize(IUBContext instance)
    {
        _instance = instance;
        base.Initialize(instance);
        var _View = instance.GameObject.GetComponent<ViewBase>();
        var viewModelType = _View.ViewModelType;
        if (!ViewModelType.IsAssignableFrom(viewModelType))
        {
            Debug.LogError(string.Format("The {0} Behaviour could not find View with the type {1} on this instance.", name, ViewModelType.Name), instance.GameObject);
        }
        _View.BindingProviders.Add(this);
    }

    public void Bind(ViewBase view)
    {
        _instance.Variables.RemoveAll(p => p is UBViewModelProperty);
        foreach (var property in view.ViewModelObject.Properties)
        {
            _instance.Variables.Add(new UBViewModelProperty(GetPropertyName(property.Key), property.Value));
        }
    }

    public sealed override void FillIncludedTriggers(List<TriggerInfo> list)
    {
        base.FillIncludedTriggers(list);
        if (ViewModelType == null) return;
        var viewModel = Activator.CreateInstance(ViewModelType) as ViewModel;
        var names = viewModel.Properties.Select(p => p.Key)
            .Concat(viewModel.Commands.Select(p => p.Key)).ToArray();
        Triggers.RemoveAll(p => typeof(IBindingProvider).IsAssignableFrom(p.TriggerType) && !names.Contains(p.Data));

        foreach (var property in viewModel.Properties)
        {
            if (property.Value is IModelCollection)
            {
                if (typeof (ViewModel).IsAssignableFrom(property.Value.ValueType)) continue;

                var currentTrigger = Triggers.FirstOrDefault(p => p.Data.ToString(CultureInfo.InvariantCulture) == property.Key && p.TriggerTypeName == typeof(ViewModelCollectionItemAddedTrigger).AssemblyQualifiedName);
                if (currentTrigger == null)
                {
                    list.Add(new TriggerInfo()
                    {
                        IsStatic = true,
                        Guid = Guid.NewGuid().ToString(),
                        DisplayName = GetPropertyName(property.Key) + " Item Added",
                        Data = property.Key,
                        TriggerTypeName = typeof(ViewModelCollectionItemAddedTrigger).AssemblyQualifiedName
                    });
                }
                else
                {
                    list.Add(currentTrigger);

                }

                currentTrigger = Triggers.FirstOrDefault(p => p.Data.ToString(CultureInfo.InvariantCulture) == property.Key && p.TriggerTypeName == typeof(ViewModelCollectionItemRemovedTrigger).AssemblyQualifiedName);
                if (currentTrigger == null)
                {
                    list.Add(new TriggerInfo()
                    {
                        IsStatic = true,
                        Guid = Guid.NewGuid().ToString(),
                        DisplayName = GetPropertyName(property.Key) + " Item Removed",
                        Data = property.Key,
                        TriggerTypeName = typeof(ViewModelCollectionItemRemovedTrigger).AssemblyQualifiedName
                    });
                }
                else
                {
                    list.Add(currentTrigger);
                }
              
            }
            else
            {
                var currentTrigger =
                Triggers.FirstOrDefault(p => p.Data.ToString(CultureInfo.InvariantCulture) == property.Key);
                if (currentTrigger != null)
                {
                    list.Add(currentTrigger);
                }
                else
                {
                    list.Add(new TriggerInfo
                    {
                        IsStatic = true,
                        Guid = Guid.NewGuid().ToString(),
                        DisplayName = GetPropertyName(property.Key) + " Changed",
                        Data = property.Key,
                        TriggerTypeName = typeof(ViewModelPropertyTrigger).AssemblyQualifiedName
                    });
                }
                
            }

        }
        foreach (var command in viewModel.Commands)
        {
            var currentTrigger = Triggers.FirstOrDefault(p => p.Data.ToString(CultureInfo.InvariantCulture) == command.Key);
            if (currentTrigger != null)
            {
                list.Add(currentTrigger);
            }
            else
            {
                list.Add(new TriggerInfo
                {
                    IsStatic = true,
                    Guid = Guid.NewGuid().ToString(),
                    DisplayName = command.Key.Replace("Command", "") + " Executed",
                    Data = command.Key,
                    TriggerTypeName = typeof(ViewModelCommandTrigger).AssemblyQualifiedName,
                });
            }
        }
    }

    public override void FillIncludedDeclares(List<IUBVariableDeclare> list)
    {
        if (Globals != null)
        {
            foreach (var item in Globals.Declares)
            {
                list.Add(item);
            }
        }

        if (ViewModelType == null) return;
        var viewModel = Activator.CreateInstance(ViewModelType) as ViewModel;

        foreach (var property in viewModel.Properties)
        {
            list.Add(new UBStaticVariableDeclare()
            {
                Guid = GetPropertyName(property.Key),
                Name = GetPropertyName(property.Key),
                ValueType = property.Value.ValueType,
                EnumType = property.Value.ValueType,
                ObjectType = property.Value.ValueType,
                DefaultValue = property.Value.ObjectValue
            });
        }
    }

    public void Unbind(ViewBase viewBase)
    {
        
    }

    private static string GetPropertyName(string name)
    {
        return name.Replace("_", "").Replace("Property", "").Trim();
    }
}