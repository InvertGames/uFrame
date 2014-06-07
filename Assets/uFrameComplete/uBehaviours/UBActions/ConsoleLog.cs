using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
[UBCategory(" UBehaviours")]
public class ConsoleLog : UBAction
{
    public UBString _Message = new UBString();

    public override string ToString()
    {
        return string.Format("Log {0}", _Message.ToString(RootContainer));
    }

    public override void WriteCode(IUBCSharpGenerator sb)
    {
        sb.AppendFormat("Debug.Log({0})", sb.VariableName(_Message, "_Message"));
    }

    protected override void PerformExecute(IUBContext context)
    {
        Debug.Log(_Message.GetValue(context));
    }
}