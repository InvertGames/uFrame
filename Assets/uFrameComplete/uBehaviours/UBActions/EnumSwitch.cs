using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

[UBCategory(" UBehaviours")]
public class EnumSwitch : UBAction
{
    [HideInInspector]
    public UBActionSheet[] _SwitchSheets = { };

    [UBRequireVariable]
    public UBEnum EnumVariable = new UBEnum();


    public override IEnumerable<UBActionSheetInfo> GetAvailableActionSheets(IUBehaviours behaviours)
    {
        var declareVariable = behaviours.FindDeclare(EnumVariable._ValueFromVariableName, ActionSheet.TriggerInfo);
        if (declareVariable == null) yield break;
        //base.FillAvailableActionSheets(obj, list);

        var field = GetType().GetField("_SwitchSheets");
        var names = Enum.GetNames(declareVariable.EnumType);
        var values = (int[])Enum.GetValues(declareVariable.EnumType);

        var maxValue = values.Max();
        if (_SwitchSheets.Length != maxValue + 1)
        {
            Array.Resize(ref _SwitchSheets, maxValue + 1);
        }

        for (int index = 0; index < names.Length; index++)
        {
            var name = names[index];
//            var value = values[index];
            yield return new UBActionSheetInfo()
            {
                Field = field,
                Name = name,
                Sheet = _SwitchSheets[index],
                ArrayIndex = index,
                Owner = this
            };
        }
    }

    public override string ToString()
    {
        return string.Format("Switch {0}", EnumVariable.ToString(RootContainer));
    }

    protected override void PerformExecute(IUBContext context)
    {
        var value = EnumVariable.GetObjectValue(context);
        var intValue = (int)value;
        if (intValue < _SwitchSheets.Length)
        {
            var sheet = _SwitchSheets[(int)value];
            if (sheet != null)
                sheet.Execute(context);
        }
    }
}
[UBCategory(" UBehaviours")]
public class StringFormat : UBAction
{
    public UBString[] _Args = new UBString[] { };
    public string _Format = "{0}";

    [UBRequireVariable]
    [UBRequired]
    public UBString _Result = new UBString();

    private const string EXTRACT_FORMAT_ARGS = @"\{(?<number>\d+).*?\}";

    public int GetNumberOfArgs()
    {
        var matches = Regex.Matches(_Format, EXTRACT_FORMAT_ARGS, RegexOptions.None);
        var list = new List<string>();
        foreach (Match match in matches)
        {
            list.Add(match.Groups["number"].Value);
        }
        return list.Distinct().Count();
    }

    protected override IEnumerable<IUBFieldInfo> GetFields()
    {
        return UBArrayFieldInfo("_Args", ref _Args, GetNumberOfArgs(), () => new UBString());
    }

    protected override void PerformExecute(IUBContext context)
    {
        if (_Result != null)
        {
            List<object> list = new List<object>();
            foreach (UBString p in _Args)
            {
                object o = p.GetValue(context).ToString(CultureInfo.InvariantCulture);
                list.Add(o);
            }
            _Result.SetValue(context,
                string.Format(_Format, list.ToArray())
                );
        }
    }
}