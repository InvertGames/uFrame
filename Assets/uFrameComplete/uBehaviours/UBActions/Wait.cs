using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


[UBCategory(" UBehaviours")]
[UBHelp("Waits for a specified time in seconds.")]
public class Wait : UBAction
{
    [HideInInspector]
    [UBHelp("Executed when the time has elapsed.")]
    public UBActionSheet _Complete;

    [UBHelp("How long should we wait in seconds?")]
    public UBFloat _HowLong = 1.0f;

    public IEnumerator DelayExecute(IUBContext context, float variable)
    {
        yield return new WaitForSeconds(variable);
        if (_Complete != null)
        {
            if (context == null)
            {
                Debug.Log("Context is null");
            }
            _Complete.Execute(context);
        }
    }

    public override string ToString()
    {
        return string.Format("Wait {0} second(s).", _HowLong.ToString(RootContainer));
    }

    protected override void PerformExecute(IUBContext context)
    {
        context.StartCoroutine(DelayExecute(context, _HowLong.GetValue(context)));
    }
}

