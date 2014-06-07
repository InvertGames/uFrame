using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

//[UBCategory("UBehaviours")]
//public class ExecuteEvent : UBAction
//{
//    [InstanceTemplateList]
//    public string _EventName;

//    [NonSerialized]
//    private TriggerInfo _cachedTrigger = null;


//    public override void WriteCode(IUBCSharpGenerator sb)
//    {
//        sb.Append(sb.InvokeTemplateSheet(_EventName));
//    }

//    public override string ToString()
//    {
//        return "Execute Event " + _EventName;
//    }

//    protected override void PerformExecute(IUBContext context)
//    {
        
//        //var ctx = context as UBSharedBehaviour;
//        //if (ctx != null)
//        //{
//        //    _cachedTrigger = ctx.FindTemplateTriggerByName(_EventName);
//        //    if (_cachedTrigger != null)
//        //        _cachedTrigger.Sheet.Execute(context);
//        //}
//    }
//}
[UBCategory(" UBehaviours")]
public class ActionGroup : UBAction
{
    public string _Name = "Group Name";

    public UBActionSheet _Actions;

    protected override void PerformExecute(IUBContext context)
    {
        context.ExecuteSheet(_Actions);
    }

    public override string ToString()
    {
        if (_Actions != null && _Actions.IsForward)
        {
            return string.Format("Forward To: {0}", _Actions.ForwardTo.DisplayName);
        }
        return _Name;
    }

    public override IEnumerable<UBActionSheetInfo> GetAvailableActionSheets(IUBehaviours behaviours)
    {
        yield return new UBActionSheetInfo()
        {
            Field = GetType().GetField("_Actions"),
            Name = _Name,
            Owner = this,
            Sheet = _Actions
        };
    }
}
[UBCategory(" UBehaviours")]
public class ThrowError : UBAction
{
    public UBString _Message = new UBString(string.Empty);
    protected override void PerformExecute(IUBContext context)
    {
        throw new Exception(_Message.GetValue(context));
    }
}
[UBCategory(" UBehaviours")]
public class ForwardTo : UBAction
{

    //public UBObject _Behaviour = new UBObject(typeof(UBInstanceBehaviour));

    [TriggerList]
    public string _TriggerName;

    public override string ToString()
    {

        var trigger = RootContainer.FindTriggerById(_TriggerName);
        if (trigger == null)
        {
            return "Forward To";
        }
        return "Forward To: " + trigger.DisplayName;
    }

    protected override void PerformExecute(IUBContext context)
    {
        //var c = _Behaviour.GetValue(context) as IUBContext ?? context;

        context.GoTo(_TriggerName);
    }
}