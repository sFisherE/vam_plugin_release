using System;
using System.Collections.Generic;
using UnityEngine;

namespace var_browser
{
    class LogUtil
    {
        public static void Log(string log)
        {
            Debug.Log(DateTime.Now.ToString("HH:mm:ss")+"【var browser】" + log);
        }
        public static void LogError(string log)
        {
            Debug.LogError(DateTime.Now.ToString("HH:mm:ss") + "【var browser】" + log);
        }
        public static void LogWarning(string log)
        {
            Debug.LogWarning(DateTime.Now.ToString("HH:mm:ss") + "【var browser】" + log);
        }
    }
}
