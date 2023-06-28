using UnityEngine;

using EnesShahn.Configurations;

namespace EnesShahn.Debugger
{
    public class GameDebugger : MonoBehaviour
    {
        public static void Log(object message)
        {
            if (GameConfig.DefaultConfig.EnableDebug)
                Debug.Log(message);
        }
        public static void Log(object message, GameObject context)
        {
            if (GameConfig.DefaultConfig.EnableDebug)
                Debug.Log(message, context);
        }
        public static void LogError(object message)
        {
            if (GameConfig.DefaultConfig.EnableDebug)
                Debug.LogError(message);
        }
        public static void LogError(object message, GameObject context)
        {
            if (GameConfig.DefaultConfig.EnableDebug)
                Debug.LogError(message, context);
        }
    }
}