using System;
using System.Diagnostics;

[UBCategory("UFrame")]
public class ExecuteCommand : UBAction
{
    [ViewModelCommands]
    public string _Command;

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(_Command))
        {
            return string.Format("Execute {0}", _Command);
        }
        return base.ToString();
    }

    protected override void PerformExecute(IUBContext context)
    {
        var view = context.GameObject.GetView();
        if (view != null)
        {
            var command = view.ViewModelObject.Commands[_Command];
            if (command == null)
            {
                UnityEngine.Debug.LogError(
                    string.Format("The command {0} was not found on TriggerCommand",
                    _Command), context.GameObject);
            }

            view.ExecuteCommand(command);
        }
        else
        {
            throw new Exception("View was not found on TriggerCommand");
        }
    }
}