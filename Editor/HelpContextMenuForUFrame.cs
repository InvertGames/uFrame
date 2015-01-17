using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Implements a context menu item that links to web help of uFrame components.
/// </summary>
static class HelpContextMenuForUFrame
{
	const string UFrameSourceRoot = "uFrameComplete/uFrame";
	const string UFrameAssembly = "Assembly-CSharp";
	const string UFrameURL = "http://invertgamestudios.com/uFrameAPI/Default/webframe.html#{1}~{0}.html";
	const string UniRxURL = "https://github.com/neuecc/UniRx/search?q={0}";
	const string UniRxNamespace = "UniRx";
	const string ContextMenuItemName = "CONTEXT/MonoBehaviour/uFrame Help";

	[MenuItem(ContextMenuItemName, true)]
	static bool IsExtraHelpAvailable(MenuCommand command)
	{
		System.Type contextType, uniRxType, uFrameType;
		GetContextData(command, out contextType, out uniRxType, out uFrameType);
		bool isExtraHelpAvailable = ((uniRxType ?? uFrameType) != null);
		return isExtraHelpAvailable;
	}

	[MenuItem(ContextMenuItemName, false)]
	static void GetExtraHelp(MenuCommand command)
	{
		System.Type contextType, uniRxType, uFrameType;
		UnityEngine.Object context = GetContextData(command, out contextType, out uniRxType, out uFrameType);
		if (uniRxType != null)
		{
			Help.BrowseURL(string.Format(UniRxURL, uniRxType.Name));
		}
		else if (uFrameType != null)
		{
			Help.BrowseURL(string.Format(UFrameURL, uFrameType.Name, UFrameAssembly));
		}
		else if (Help.HasHelpForObject(context))
		{
			Help.ShowHelpForObject(context);
		}
		else
		{
			Debug.LogWarning(string.Format("Sorry, no extra help available for {0}.", context), context);
		}
	}

	private static UnityEngine.Object GetContextData(
		MenuCommand command,
		out System.Type contextType,
		out System.Type uniRxType,
		out System.Type uFrameType)
	{
		UnityEngine.Object context = ((command != null) ? command.context : null);
		contextType = ((context != null) ? context.GetType() : null);
		uniRxType = FindUniRxType(contextType);
		uFrameType = ((uniRxType == null) ? FindUFrameType(contextType) : null);

		//Debug.Log(string.Format("Extra Help: {0} => {1}", contextType, (uniRxType ?? uFrameType)), context);

		return context;
	}

	private static System.Type FindUFrameType(System.Type contextType)
	{
		System.Type uFrameType = null;
		string uFrameSourcePath = Path.GetFullPath(Path.Combine(Application.dataPath, UFrameSourceRoot));
		while (contextType != null)
		{
			string contextTypeAssemblyName = (((contextType.Assembly != null) && (contextType.Assembly.GetName() != null)) ? contextType.Assembly.GetName().Name : null);
			bool isUFrameAssembly = ((contextTypeAssemblyName != null) && (0 == string.CompareOrdinal(contextTypeAssemblyName, UFrameAssembly)));
			if (isUFrameAssembly)
			{
				string[] allSceneFiles = Directory.GetFiles(
					uFrameSourcePath,
					string.Format("{0}.cs", contextType.Name),
					SearchOption.AllDirectories);
				if (allSceneFiles.Length > 0)
				{
					uFrameType = contextType;
					break;
				}
			}

			contextType = contextType.BaseType;
		}

		return uFrameType;
	}

	private static System.Type FindUniRxType(System.Type contextType)
	{
		System.Type uniRxType = null;
		while (contextType != null)
		{
			string contextTypeNamespace = contextType.Namespace;
			bool isUniRxNamespace = ((contextTypeNamespace != null) && ((0 == string.CompareOrdinal(contextTypeNamespace, UniRxNamespace)) || contextTypeNamespace.StartsWith((UniRxNamespace + "."), System.StringComparison.Ordinal)));
			if (isUniRxNamespace)
			{
				uniRxType = contextType;
				break;
			}

			contextType = contextType.BaseType;
		}

		return uniRxType;
	}
}
