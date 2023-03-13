using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using HarmonyLib;
namespace var_browser
{
    class HubResourcePackageHook
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MVR.Hub.HubResourcePackage), "DownloadComplete", 
            new Type[] { typeof(byte[]), typeof(Dictionary<string, string>) })]
        static void PostDownloadComplete(MVR.Hub.HubResourcePackage __instance, 
            byte[] data, Dictionary<string, string> responseHeaders)
        {
            string value;
            string str;
            if (responseHeaders.TryGetValue("Content-Disposition", out value))
            {
                value = Regex.Replace(value, ";$", string.Empty);
                str = Regex.Replace(value, ".*filename=\"?([^\"]+)\"?.*", "$1");
            }
            else
            {
                str = Traverse.Create(__instance).Field("resolvedVarName").GetValue<string>();
            }
            LogUtil.Log("Hook DownloadComplete "+ str);
            //移动到仓库目录，然后再link过来
            var_browser.FileManager.Refresh();
        }

    }
}
