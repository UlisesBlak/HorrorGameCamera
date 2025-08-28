using UnityEngine;

namespace Utils
{
    public static class EDebug
    {
        public static void LogGood(object msg)
        {
            #if UNITY_EDITOR
            Debug.Log(StringUtils.AddColorToString(msg.ToString(), CColors.Good));
            #endif
        }
        public static void LogGood(object msg, Object context)
        {
            #if UNITY_EDITOR
            string contextName = context ? $"<b>[{context.name}]</b> " : "";
            Debug.Log(StringUtils.AddColorToString(contextName + msg, CColors.Good), context);
            #endif
        }
        public static void Log(object msg)
        {
            #if UNITY_EDITOR
            Debug.Log(StringUtils.AddColorToString(msg.ToString(), CColors.Log));
            #endif
        }
        public static void Log(object msg, Object context)
        {
            #if UNITY_EDITOR
            string contextName = context ? $"<b>[{context.name}]</b> " : "";
            Debug.Log(StringUtils.AddColorToString(contextName + msg, CColors.Log), context);
            #endif
        }
        public static void LogWarning(object msg)
        {
            #if UNITY_EDITOR
            Debug.LogWarning(StringUtils.AddColorToString(msg.ToString(), CColors.Warning));
            #endif
        }
        public static void LogWarning(object msg, Object context)
        {
            #if UNITY_EDITOR
            string contextName = context ? $"<b>[{context.name}]</b> " : "";
            Debug.Log(StringUtils.AddColorToString(contextName + msg, CColors.Warning), context);
            #endif
        }
        public static void LogError(object msg)
        {
            #if UNITY_EDITOR
            Debug.LogError(StringUtils.AddColorToString(msg.ToString(), CColors.Error));
            #endif
        }
        public static void LogError(object msg, Object context)
        {
            #if UNITY_EDITOR
            string contextName = context ? $"<b>[{context.name}]</b> " : "";
            Debug.Log(StringUtils.AddColorToString(contextName + msg, CColors.Error), context);
            #endif
        }
    }
}
