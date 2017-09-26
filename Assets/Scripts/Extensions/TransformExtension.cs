using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class TransformExtension 
{
    /// <summary>
    /// Removes childs.
    /// </summary>
    public static void RemoveChilds(this Transform transform)
    {
        List<GameObject> childs = new List<GameObject>();

        int lenght = transform.childCount;
        for(int i = 0; i < lenght; i++)
        {
            childs.Add(transform.GetChild(i).gameObject);
        }

        foreach(GameObject child in childs)
        {
            #if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Object.Destroy(child);
            }
            else
            {
                EditorApplication.delayCall += () =>
                {
                    Object.DestroyImmediate(child);
                };
            }
            #else
            Object.Destroy(child);
            #endif
        }
    }
}