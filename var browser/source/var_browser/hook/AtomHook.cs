using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using HarmonyLib;
using SimpleJSON;
using System.Reflection;

namespace var_browser
{
    class AtomHook
    {
        //load look功能
        //prefab:TabControlAtom
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Atom), "LoadAppearancePreset", new Type[] { typeof(string) })]
        public static void PreLoadAppearancePreset(Atom __instance, string saveName = "savefile")
        {
            LogUtil.Log("[var browser hook]PreLoadAppearancePreset " + saveName);
            using (MVR.FileManagement.FileEntryStreamReader fileEntryStreamReader =MVR.FileManagement.FileManager.OpenStreamReader(saveName, true))
            {
                string aJSON = fileEntryStreamReader.ReadToEnd();
                FileButton.EnsureInstalledInternal(aJSON);
            }
        }

        //ky1001.PresetLoader 用这种方法加载的
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Atom), "LoadPreset", new Type[] { typeof(string) })]
        public static void PreLoadPreset(Atom __instance, string saveName = "savefile")
        {
            LogUtil.Log("[var browser hook]PreLoadPreset " + saveName);
            using (MVR.FileManagement.FileEntryStreamReader fileEntryStreamReader = MVR.FileManagement.FileManager.OpenStreamReader(saveName, true))
            {
                string aJSON = fileEntryStreamReader.ReadToEnd();
                FileButton.EnsureInstalledInternal(aJSON);
            }

        }
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(MeshVR.PresetManagerControl), "LoadPresetWithPath", new Type[] { typeof(string) })]
        //public static void PreLoadPresetWithPath(MeshVR.PresetManagerControl __instance, string p)
        //{
        //    Debug.Log("PreLoadPresetWithPath " + p);
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(MeshVR.PresetManagerControl), "MergeLoadPresetWithPath", new Type[] { typeof(string) })]
        //public static void PreMergeLoadPresetWithPath(MeshVR.PresetManagerControl __instance, string p)
        //{
        //    Debug.Log("PreMergeLoadPresetWithPath " + p);
        //}
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MeshVR.PresetManagerControl), "SyncPresetBrowsePath", new Type[] { typeof(string) })]
        protected static void PreSyncPresetBrowsePath(MeshVR.PresetManagerControl __instance, string url)
        {
            LogUtil.Log("[var browser hook]PreSyncPresetBrowsePath " + url);
            VarFileEntry varFileEntry = FileManager.GetVarFileEntry(url);
            if (varFileEntry != null)
            {
                bool dirty= varFileEntry.Package.InstallRecursive();
                if (dirty)
                {
                    MVR.FileManagement.FileManager.Refresh();
                    var_browser.FileManager.Refresh();
                }
            }
            else
            {
                if (File.Exists(url))
                {
                    string text = File.ReadAllText(url);
                    FileButton.EnsureInstalledInternal(text);
                }
            }
        }

        //prefab:TabPresetPerson
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(MeshVR.PresetManagerControl), "LoadPreset")]
        //protected static void PreLoadPreset(MeshVR.PresetManagerControl __instance)
        //{
        //    var pm = Traverse.Create(__instance).Field("pm").GetValue<MeshVR.PresetManager>();
        //    string storeFolderPath = pm.GetStoreFolderPath(true);
        //    var tra = Traverse.Create(pm);
        //    string storeName = (string)tra.Field("storeName").GetValue();
        //    string path = storeFolderPath + storeName + "_" + pm.presetName + ".vap";

        //    if (File.Exists(path))
        //    {
        //        string text = File.ReadAllText(path);
        //        FileButton.EnsureInstalledInternal(text);
        //    }
        //    Debug.Log("[var browser hook]PresetManagerControl PreLoadPreset " + path);
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(MeshVR.PresetManagerControl), "MergeLoadPreset")]
        //protected static void PreMergeLoadPreset(MeshVR.PresetManagerControl __instance)
        //{
        //    var pm = Traverse.Create(__instance).Field("pm").GetValue<MeshVR.PresetManager>();
        //    string storeFolderPath = pm.GetStoreFolderPath(true);
        //    var tra = Traverse.Create(pm);
        //    string storeName = (string)tra.Field("storeName").GetValue();
        //    string path = storeFolderPath + storeName + "_" + pm.presetName + ".vap";

        //    if (File.Exists(path))
        //    {
        //        string text = File.ReadAllText(path);
        //        FileButton.EnsureInstalledInternal(text);
        //    }
        //    Debug.Log("[var browser hook]PresetManagerControl MergeLoadPreset " + pm.presetName);
        //}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SubScene), "LoadSubSceneWithPath", new Type[] { typeof(string)})]
        public static void PreLoadSubSceneWithPath(SubScene __instance,string p)
        {
            LogUtil.Log("[var browser hook]PreLoadSubSceneWithPath " + p);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SubScene), "LoadSubScene")]
        public static void PreLoadSubScene(SubScene __instance)
        {
            MethodInfo getStorePathMethod = typeof(SubScene).GetMethod("GetStorePath", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            object ret= getStorePathMethod.Invoke(__instance, new object[1] {true });
            string path = (string)ret + ".json";
            LogUtil.Log("[var browser hook]PreLoadSubScene " + path);
            if (path.Contains(":"))
            {
                string packagename = path.Substring(0,path.IndexOf(":"));
                var package = FileManager.GetPackage(packagename);
                if (package != null)
                {
                    bool dirty = package.InstallRecursive();
                    if (dirty)
                    {
                        MVR.FileManagement.FileManager.Refresh();
                        var_browser.FileManager.Refresh();
                    }
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    //Debug.Log("Exists " + url);
                    string text = File.ReadAllText(path);
                    FileButton.EnsureInstalledInternal(text);
                }
            }
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(Atom), "Store", new Type[]{typeof(JSONArray),typeof(bool),typeof(bool)})]
        //public static void PreAtomStore(Atom __instance,
        //    JSONArray atoms, bool includePhysical, bool includeAppearance)
        //{
        //    Debug.Log("PreAtomStore ");
        //}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MeshVR.PresetManager), "LoadPresetPre",
            new Type[] { typeof(bool) })]
        public static void PreLoadPresetPre(MeshVR.PresetManager __instance,bool isMerge = false)
        {
            if (__instance.itemType != MeshVR.PresetManager.ItemType.None)
            {
                string storeFolderPath = __instance.GetStoreFolderPath(false);
                string storeName = __instance.storeName;

                var traverse = Traverse.Create(__instance);
                var _presetName = traverse.Field("_presetName").GetValue<string>();
                var presetPackagePath = traverse.Field("presetPackagePath").GetValue<string>();
                var presetSubPath = traverse.Field("presetSubPath").GetValue<string>();
                var presetSubName = traverse.Field("presetSubName").GetValue<string>();

                if (storeFolderPath != null && storeFolderPath != string.Empty
                    && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
                {
                    string text = presetPackagePath + storeFolderPath + presetSubPath + storeName + "_" + presetSubName + ".vap";
                    LogUtil.LogWarning("PresetManager PreLoadPresetPre " + text);
                    using (MVR.FileManagement.FileEntryStreamReader fileEntryStreamReader = MVR.FileManagement.FileManager.OpenStreamReader(text, true))
                    {
                        string aJSON = fileEntryStreamReader.ReadToEnd();
                        FileButton.EnsureInstalledInternal(aJSON);
                    }
                }
            }
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(MeshVR.PresetManager), "LoadPresetPreFromJSON", 
        //    new Type[] {typeof(JSONClass), typeof(bool) })]
        //protected void PreLoadPresetPreFromJSON(MeshVR.PresetManager __instance, 
        //    JSONClass inputJSON, 
        //    bool isMerge = false)
        //{
        //    string str = inputJSON.ToString();
        //    Debug.Log("PresetManager PreLoadPresetPreFromJSON " + __instance.presetName);
        //    FileButton.EnsureInstalledInternal(str);
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(MeshVR.PresetManager), "LoadPresetPre", new Type[] { typeof(bool) })]
        //public static void PreLoadPresetPre(MeshVR.PresetManager __instance, bool isMerge = false)
        //{


        //    //Debug.Log("PresetManager PreLoadPresetPre " + __instance.GetStorePathBase());
        //    //Debug.Log("PresetManager PreLoadPresetPre2 " + __instance.GetStoreFolderPath(true));
        //    //Debug.Log("PresetManager PreLoadPresetPre3 " + __instance.presetName);
        //    //Debug.Log("PresetManager PreLoadPresetPre4 " + Traverse.Create(__instance).Field("presetSubName").GetValue());
        //    //Debug.Log("PresetManager PreLoadPresetPre5 " + Traverse.Create(__instance).Field("presetSubPath").GetValue());
        //    //string storeFolderPath = __instance.GetStoreFolderPath(false);

        //    var tra = Traverse.Create(__instance);
        //    //string presetPackagePath= (string)tra.Field("presetPackagePath").GetValue();
        //    //string presetSubPath = (string)tra.Field("presetSubName").GetValue();
        //    string storeName = (string)tra.Field("storeName").GetValue();
        //    string presetSubName = (string)tra.Field("presetSubName").GetValue();
        //    //string _presetName = (string)tra.Field("_presetName").GetValue();
        //    //Debug.Log(storeFolderPath + " " + presetSubPath + " " + storeName + " " + presetSubName);
        //    string storeFolderPath = __instance.GetStoreFolderPath(true);
        //    string path = storeFolderPath + storeName + "_" + presetSubName + ".vap";
        //    Debug.Log("PresetManager PreLoadPresetPre6 " + path);
        //    //if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty 
        //    //    && _presetName != null && _presetName != string.Empty)
        //    //{
        //    //    Debug.Log("PresetManager PreLoadPresetPre7 " + path);
        //    //}

        //    if (path.Contains(":"))
        //    {

        //    }
        //    else
        //    {
        //        //if (File.Exists(path))
        //        //{
        //        //    //Debug.Log("Exists " + url);
        //        //    string text = File.ReadAllText(path);
        //        //    FileButton.EnsureInstalledInternal(text);
        //        //}
        //    }
        //}
    }
}
