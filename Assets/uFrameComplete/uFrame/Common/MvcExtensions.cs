using System.Collections.Generic;
using UnityEngine;

public static class MvcExtensions
{
    public static T GetComponentFromInterface<T>(this MonoBehaviour behaviour) where T : class
    {
        return behaviour.gameObject.GetComponentFromInterface<T>();
    }

    public static T GetComponentFromInterface<T>(this GameObject gameObj) where T : class
    {
        foreach (Component component in gameObj.GetComponents<Component>())
        {
            if (component is T)
            {
                T __522720665 = component as T;
                return __522720665;
            }
        }
        return default(T);
    }

    public static IEnumerable<T> GetComponentsInDirectChildren<T>(this Transform tfm) where T : Component
    {
        for (int i = 0; i < tfm.childCount; i++)
        {
            var cmp = tfm.GetChild(i).GetComponent<T>();
            if (cmp == null) continue;
            yield return cmp;
        }
    }
}