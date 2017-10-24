using System;
using System.Collections;
using UnityEngine;

static public class MonoBehaviourExtension
{
    /// <summary>
    /// Gets or add a component. Usage example:
    /// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
    /// </summary>
    public static T GetOrAddComponent<T>(this Component child) where T : Component
    {
        T result = child.GetComponent<T>();
        if (result == null)
        {
            result = child.gameObject.AddComponent<T>();
        }
        return result;
    }

    /// <summary>
    /// Gets or add a component. Usage example:
    /// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
    /// </summary>
    public static T GetOrAddComponent<T>(this GameObject child) where T : Component
    {
        T result = child.GetComponent<T>();
        if (result == null)
        {
            result = child.gameObject.AddComponent<T>();
        }
        return result;
    }


    /// <summary>
    /// This Ivoke is better to debug than default, becouse you can see reference to method.
    /// </summary>
    public static void Invoke(this MonoBehaviour mono, Action action, float delay)
    {
        //mono.Invoke(action.Method.Name, delay);
        mono.StartCoroutine(InvokeEnumerator(action, delay));
    }

    public static IEnumerator InvokeEnumerator(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
}