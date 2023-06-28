using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance
    {
        get
        {
            if (instance == null || instance.gameObject == null)
            {
                instance = GameObject.FindObjectOfType<T>();
            }
            if (instance == null || instance.gameObject == null)
            {
                var go = GameObject.Find(typeof(T).ToString());
                if (go != null)
                {
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<T>();
                }
            }
            if (instance == null || instance.gameObject == null)
            {
                var newObject = new GameObject(typeof(T).ToString());
                DontDestroyOnLoad(newObject);
                instance = newObject.AddComponent<T>();
            }
            return instance;
        }
    }

    private static T instance;

    public static bool IsAvailable()
    {
        return instance != null;
    }

}
