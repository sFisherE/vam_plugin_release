using ICSharpCode.SharpZipLib.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using Prime31.MessageKit;
namespace var_browser
{
	public class FileManager : MonoBehaviour
	{
		public delegate void OnRefresh();

		public static bool debug;

		public static FileManager singleton;

		protected static Dictionary<string, VarPackage> packagesByUid;
		public static Dictionary<string, VarPackage> PackagesByUid
        {
            get
            {
				return packagesByUid;

			}
        }

		protected static Dictionary<string, VarPackage> packagesByPath;

		protected static Dictionary<string, VarPackageGroup> packageGroups;
		protected static HashSet<VarFileEntry> allVarFileEntries;
		protected static Dictionary<string, VarFileEntry> uidToVarFileEntry;
		protected static Dictionary<string, VarFileEntry> pathToVarFileEntry;
		protected static OnRefresh onRefreshHandlers;
		protected static HashSet<string> restrictedReadPaths;

		protected static HashSet<string> secureReadPaths;

		protected static HashSet<string> secureInternalWritePaths;

		protected static HashSet<string> securePluginWritePaths;

		protected static HashSet<string> pluginWritePathsThatDoNotNeedConfirm;

		public Transform userConfirmContainer;

		public Transform userConfirmPrefab;

		public Transform userConfirmPluginActionPrefab;

		protected static Dictionary<string, string> pluginHashToPluginPath;

		//protected AsyncFlag userConfirmFlag;

		//protected static HashSet<string> userConfirmedPlugins;

		//protected static HashSet<string> userDeniedPlugins;

		protected static LinkedList<string> loadDirStack;
		public static DateTime lastPackageRefreshTime
		{
			get;
			protected set;
		}

		public static string CurrentLoadDir
		{
			get
			{
				if (loadDirStack != null && loadDirStack.Count > 0)
				{
					return loadDirStack.Last.Value;
				}
				return null;
			}
		}

		public static string CurrentPackageUid
		{
			get
			{
				string currentLoadDir = CurrentLoadDir;
				if (currentLoadDir != null)
				{
					VarDirectoryEntry varDirectoryEntry = GetVarDirectoryEntry(currentLoadDir);
					if (varDirectoryEntry != null)
					{
						return varDirectoryEntry.Package.Uid;
					}
				}
				return null;
			}
		}

		public static string TopLoadDir
		{
			get
			{
				if (loadDirStack != null && loadDirStack.Count > 0)
				{
					return loadDirStack.First.Value;
				}
				return null;
			}
		}

		public static string TopPackageUid
		{
			get
			{
				string topLoadDir = TopLoadDir;
				if (topLoadDir != null)
				{
					VarDirectoryEntry varDirectoryEntry = GetVarDirectoryEntry(topLoadDir);
					if (varDirectoryEntry != null)
					{
						return varDirectoryEntry.Package.Uid;
					}
				}
				return null;
			}
		}

		public static string CurrentSaveDir
		{
			get;
			protected set;
		}

		protected static string packagePathToUid(string vpath)
		{
			string input = vpath.Replace('\\', '/');
			input = Regex.Replace(input, "\\.(var|zip)$", string.Empty);
			return Regex.Replace(input, ".*/", string.Empty);
		}

		protected static VarPackage RegisterPackage(string vpath,bool clean=false)
		{
            if (debug)
            {
				LogUtil.Log("RegisterPackage " + vpath);
			}
			string text = packagePathToUid(vpath).Trim();
			string[] array = text.Split('.');

			bool isDuplicated = false;
			if (array.Length == 3)
			{
				string text2 = array[0];
				string text3 = array[1];
				string shortName = text2 + "." + text3;
				string s = array[2];
				try
				{
					int version = int.Parse(s);
					if (!packagesByUid.ContainsKey(text))
					{
						VarPackageGroup value;
						if (!packageGroups.TryGetValue(shortName, out value))
						{
							value = new VarPackageGroup(shortName);
							packageGroups.Add(shortName, value);
						}
						VarPackage varPackage = new VarPackage(text, vpath, value, text2, text3, version);
						packagesByUid.Add(text, varPackage);

						packagesByPath.Add(varPackage.Path, varPackage);
						value.AddPackage(varPackage);

						//var包disable，就是在同路径下新建一个disable的文件
						if (varPackage.Enabled)
						{
							if (varPackage.FileEntries != null)
							{
								foreach (VarFileEntry fileEntry in varPackage.FileEntries)
								{
									allVarFileEntries.Add(fileEntry);
									uidToVarFileEntry.Add(fileEntry.Uid, fileEntry);
									pathToVarFileEntry.Add(fileEntry.Path, fileEntry);
								}
							}
						}
						return varPackage;
					}
					isDuplicated = true;
					LogUtil.LogError("Duplicate package uid " + text + ". Cannot register");
				}
				catch (Exception)
				{
					LogUtil.LogError("VAR file " + vpath + " does not use integer version field in name <creator>.<name>.<version>");
				}
			}
			else
			{
				LogUtil.LogError("VAR file " + vpath + " is not named with convention <creator>.<name>.<version>");
			}

            //到这里说明不合法
            if (clean)
            {
                if (isDuplicated)
                {
					RemoveToInvalid(vpath, "Duplicated");
				}
				else
					RemoveToInvalid(vpath,"InvalidName");
			}
            return null;
        }
		static void RemoveToInvalid(string vpath,string subPath=null)
        {
			if (!Directory.Exists("InvalidPackages"))
				Directory.CreateDirectory("InvalidPackages");

            if (!string.IsNullOrEmpty(subPath))
            {
				if (!Directory.Exists("InvalidPackages/"+ subPath))
					Directory.CreateDirectory("InvalidPackages/" + subPath);
			}

			string moveToPath = null;
            if (vpath.StartsWith("AllPackages"))
            {
				moveToPath = "InvalidPackages" + vpath.Substring("AllPackages".Length);
				if (!string.IsNullOrEmpty(subPath))
                {
					moveToPath = "InvalidPackages/"+subPath+"/" + vpath.Substring("AllPackages".Length);
				}
			}
			else if (vpath.StartsWith("AddonPackages"))
            {
				moveToPath = "InvalidPackages" + vpath.Substring("AddonPackages".Length);
				if (!string.IsNullOrEmpty(subPath))
				{
					moveToPath = "InvalidPackages/" + subPath + "/" + vpath.Substring("AddonPackages".Length);
				}
			}
			//UnityEngine.Debug.Log(moveToPath);
			string dir = Path.GetDirectoryName(moveToPath);
			if(!Directory.Exists(dir))
            {
				Directory.CreateDirectory(dir);
            }
			while (File.Exists(moveToPath))
			{
				moveToPath += "(clone)";
			}
			File.Move(vpath, moveToPath);
		}

		public static void UnregisterPackage(VarPackage vp)
		{
			LogUtil.Log("UnregisterPackage " + vp.Path);
			if (vp != null)
			{
				if (vp.Group != null)
				{
					vp.Group.RemovePackage(vp);
				}
				packagesByUid.Remove(vp.Uid);
				packagesByPath.Remove(vp.Path);
                if (vp.FileEntries != null)
                {
					foreach (VarFileEntry fileEntry in vp.FileEntries)
					{
						allVarFileEntries.Remove(fileEntry);
						uidToVarFileEntry.Remove(fileEntry.Uid);
						pathToVarFileEntry.Remove(fileEntry.Path);
					}
				}
				vp.Dispose();
			}
		}

		public static void RegisterRefreshHandler(OnRefresh refreshHandler)
		{
			onRefreshHandlers = (OnRefresh)Delegate.Combine(onRefreshHandlers, refreshHandler);
		}

		public static void UnregisterRefreshHandler(OnRefresh refreshHandler)
		{
			onRefreshHandlers = (OnRefresh)Delegate.Remove(onRefreshHandlers, refreshHandler);
		}
		//public static void DoFixVarNameLog(string log)
		//{
		//	File.AppendAllText(GlobalInfo.FixVarNamePath, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : " + log + "\n");
		//}
		protected static void ClearAll()
		{
			foreach (VarPackage value in packagesByUid.Values)
			{
				value.Dispose();
			}
			if (packagesByUid != null)
			{
				packagesByUid.Clear();
			}
			if (packagesByPath != null)
			{
				packagesByPath.Clear();
			}
			if (packageGroups != null)
			{
				packageGroups.Clear();
			}
			if (allVarFileEntries != null)
			{
				allVarFileEntries.Clear();
			}
			if (uidToVarFileEntry != null)
			{
				uidToVarFileEntry.Clear();
			}
			if (pathToVarFileEntry != null)
			{
				pathToVarFileEntry.Clear();
			}
		}

		public static void Refresh(bool init = false,bool clean=false,bool removeOldVersion=false)
		{
			//if (debug)
			{
				LogUtil.Log("FileManager Refresh()");
			}
			if (packagesByUid == null)
			{
				packagesByUid = new Dictionary<string, VarPackage>();
			}
			if (packagesByPath == null)
			{
				packagesByPath = new Dictionary<string, VarPackage>();
			}
			if (packageGroups == null)
			{
				packageGroups = new Dictionary<string, VarPackageGroup>();
			}
			if (allVarFileEntries == null)
			{
				allVarFileEntries = new HashSet<VarFileEntry>();
			}
			if (uidToVarFileEntry == null)
			{
				uidToVarFileEntry = new Dictionary<string, VarFileEntry>();
			}
			if (pathToVarFileEntry == null)
			{
				pathToVarFileEntry = new Dictionary<string, VarFileEntry>();
			}

			bool flag = false;
			try
			{
                if (!Directory.Exists("Cache/AllPackagesJSON"))
                {
					Directory.CreateDirectory("Cache/AllPackagesJSON");
                }
				if (!Directory.Exists("AddonPackages"))
				{
					CreateDirectory("AddonPackages");
				}
				if (!Directory.Exists("AllPackages"))
				{
					CreateDirectory("AllPackages");
				}
				if (Directory.Exists("AllPackages"))
				{
                    string[] addonVarPaths = Directory.GetFiles("AddonPackages", "*.var", SearchOption.AllDirectories);
                    string[] allVarPaths = Directory.GetFiles("AllPackages", "*.var", SearchOption.AllDirectories);
                    string[] varPaths = new string[addonVarPaths.Length + allVarPaths.Length];
                    Array.Copy(addonVarPaths, 0, varPaths, 0, addonVarPaths.Length);
                    Array.Copy(allVarPaths, 0, varPaths, addonVarPaths.Length, allVarPaths.Length);

                    HashSet<string> hashSet = new HashSet<string>();
					HashSet<string> addSet = new HashSet<string>();
					if (varPaths != null)
					{
						string[] _varPaths = varPaths;
						foreach (string _varPath in _varPaths)
						{
							string varPath = CleanFilePath(_varPath);
							hashSet.Add(varPath);

							VarPackage value2;
							if (packagesByPath.TryGetValue(varPath, out value2))
							{
							}
							else
							{
								//没有，登记一下
								addSet.Add(varPath);
							}
						}
					}

					HashSet<VarPackage> removeSet = new HashSet<VarPackage>();
					foreach (VarPackage value3 in packagesByUid.Values)
					{
						if (!hashSet.Contains(value3.Path))
						{
							removeSet.Add(value3);
						}
					}
                    HashSet<string> oldVersion = new HashSet<string>();
                    if (removeOldVersion)
                    {
						HashSet<string> referenced = GetReferencedPackage();
						foreach (var item in packageGroups)
                        {
                            var group = item.Value;
                            foreach (var item2 in group.Packages)
                            {
                                if (item2.Version != group.NewestVersion)
                                {
                                    if (!referenced.Contains(item2.Uid))
                                    {
										removeSet.Add(item2);
										oldVersion.Add(item2.Path);
									}
                                    else
                                    {
#if DEBUG
										LogUtil.Log("keep old version:" + item2.Uid);
#endif
									}
                                }
                            }
                        }
                    }

                    foreach (VarPackage item2 in removeSet)
                    {
                        UnregisterPackage(item2);
                        flag = true;
                    }
                    foreach (string item3 in addSet)
                    {
                        RegisterPackage(item3, clean);
                        flag = true;
                    }
                    if (removeOldVersion)
                    {
                        //移除旧版本
                        foreach (var item in oldVersion)
                        {
                            RemoveToInvalid(item, "OldVersion");
                        }
                    }
                }
				if (flag)
				{
					//foreach (VarPackage value4 in packagesByUid.Values)
					//{
					//	UnityEngine.Profiling.Profiler.BeginSample("VarPackage LoadMetaData");
					//	//value4.LoadMetaData();
					//	UnityEngine.Profiling.Profiler.EndSample();
					//}
					//foreach (VarPackageGroup value5 in packageGroups.Values)
					//{
					//	UnityEngine.Profiling.Profiler.BeginSample("VarPackageGroup Init");
					//	value5.Init();
					//	UnityEngine.Profiling.Profiler.EndSample();
					//}
				}
                if (init)
                    FileManager.singleton.StartScan(flag, clean, true);
                else
                    FileManager.singleton.StartScan(flag, clean, false);

            }
            catch (Exception arg)
			{
				LogUtil.LogError("Exception during package refresh " + arg);
			}
			lastPackageRefreshTime = DateTime.Now;
		}

		static void ScanAndRegister(VarPackage varPackage)
		{
			varPackage.Scan();
            
			if (varPackage.invalid)
            {
				//最后移除
			}
			//else if (varPackage.fixUid)
			//{
			//	//UnityEngine.Debug.Log("need fix name1:" + varPackage.Path);
			//	string dir = Path.GetDirectoryName(varPackage.Path);
			//	string targetPath = dir + "/" + varPackage.Uid + ".var";
			//	//UnityEngine.Debug.Log("need fix name2:" + targetPath);
   //             if (!File.Exists(targetPath))
   //             {
   //                 //File.Move(varPackage.Path, targetPath);
   //                 DoFixVarNameLog("move "+varPackage.Path + " -> " + targetPath);
   //             }
   //             else
			//	{
			//		//RemoveToInvalid(varPackage.Path);
   //                 DoFixVarNameLog("remove "+varPackage.Path);
			//	}
			//}
			else
				RegisterFileEntry(varPackage);
		}

		static void RegisterFileEntry(VarPackage varPackage)
		{
			foreach (VarFileEntry fileEntry in varPackage.FileEntries)
			{
				if (!allVarFileEntries.Contains(fileEntry))
					allVarFileEntries.Add(fileEntry);
				if (!uidToVarFileEntry.ContainsKey(fileEntry.Uid))
					uidToVarFileEntry.Add(fileEntry.Uid, fileEntry);
				if (!pathToVarFileEntry.ContainsKey(fileEntry.Path))
					pathToVarFileEntry.Add(fileEntry.Path, fileEntry);
			}
		}
		Coroutine m_StartScanCo = null;
		IEnumerator StartScanCo(bool flag, bool clean, bool runCo)
        {
			List<VarPackage> invalid = new List<VarPackage>();
			if (runCo)
			{
				if (m_Co != null)
				{
					StopCoroutine(m_Co);
					m_Co = null;
				}
				m_Co = StartCoroutine(ScanVarPackage(clean,invalid));
				yield return m_Co;
			}
            else
            {

				foreach (var item in packagesByUid)
				{
					ScanAndRegister(item.Value);
					if (item.Value.invalid)
					{
						invalid.Add(item.Value);
					}
				}
				
			}
			if (clean)
			{
				foreach (var item in invalid)
				{
					string path = item.Path;
					UnregisterPackage(item);
					RemoveToInvalid(path, "InvalidZip");
				}
			}
			if (flag && onRefreshHandlers != null)
			{
				onRefreshHandlers();
			}
			//不管有没有变化都刷新一下，因为可能文件没有移动，只是favorite或者autoinstall状态变了
			MessageKit.post(MessageDef.FileManagerRefresh);
		}
		public void StartScan(bool flag,bool clean,bool runCo)
		{
			if (m_StartScanCo != null)
			{
				StopCoroutine(m_StartScanCo);
				m_StartScanCo = null;
			}
			m_StartScanCo = StartCoroutine(StartScanCo(flag, clean, runCo));
   //         if (runCo)
   //         {
			//	if (m_Co != null)
			//	{
			//		StopCoroutine(m_Co);
			//		m_Co = null;
			//	}
			//	m_Co = StartCoroutine(ScanVarPackage(clean));
			//}
   //         else
   //         {
			//	List<VarPackage> invalid = new List<VarPackage>();

			//	foreach (var item in packagesByUid)
			//	{
			//		ScanAndRegister(item.Value);
			//		if (item.Value.invalid)
			//		{
			//			invalid.Add(item.Value);
			//		}
			//	}
   //             if (clean)
   //             {
			//		foreach (var item in invalid)
			//		{
			//			string path = item.Path;
			//			UnregisterPackage(item);
			//			RemoveToInvalid(path, "CorruptedZip");
			//		}
			//	}
			//	if (flag && onRefreshHandlers != null)
			//	{
			//		onRefreshHandlers();
			//	}
			//	//不管有没有变化都刷新一下，因为可能文件没有移动，只是favorite或者autoinstall状态变了
			//	MessageKit.post(MessageDef.FileManagerRefresh);
			//}
		}
		Coroutine m_Co = null;
		IEnumerator ScanVarPackage(bool clean, List<VarPackage> invalid)
		{
			int cnt = 0;
			int allCount = packagesByUid.Count;
			int idx = 0;
			List<string> list = new List<string>();
			foreach (var item in packagesByUid)
			{
				list.Add(item.Key);
			}
			int step = 20;
            if (VarPackageMgr.singleton.existCache)
            {
				step = 100;
            }
			for (int i = 0; i < list.Count; i++)
            {
				string uid = list[i];
                if (packagesByUid.ContainsKey(uid))
                {
					var pkg = packagesByUid[uid];
					if (cnt > step)
					{
						yield return null;
						cnt = 0;
					}
					ScanAndRegister(pkg);
					if (pkg.invalid)
					{
						invalid.Add(pkg);
					}
				}
				idx++;
				MessageKit<string>.post(MessageDef.UpdateLoading, idx + "/" + allCount);

				cnt++;
			}
   //         if (clean)
   //         {
			//	foreach (var item in invalid)
			//	{
			//		string path = item.Path;
			//		UnregisterPackage(item);
			//		RemoveToInvalid(path, "CorruptedZip");
			//	}
			//}

   //         if (onRefreshHandlers != null)
   //         {
   //             onRefreshHandlers();
   //         }
			//	MessageKit.post(MessageDef.FileManagerInit);
		}

		public List<string> GetAllVars()
        {
            List<string> ret = new List<string>();
            foreach (VarPackage value4 in packagesByUid.Values)
            {
                ret.Add(value4.Path);
            }
            return ret;
        }
		//在移除老包的时候，查看所有的包引用的情况，带版本号的包需要特别记录一下，这部分包就算是老版本，也不能删掉
		public static HashSet<string> GetReferencedPackage()
        {
			HashSet<string> hashSet = new HashSet<string>();
			foreach (var item in packagesByUid)
			{
				var var = item.Value;
				if (var.RecursivePackageDependencies != null)
				{
					foreach (var key in var.RecursivePackageDependencies)
					{
                        if (!key.EndsWith(".latest"))
                        {
							hashSet.Add(key);
						}
					}
				}
			}
			return hashSet;
		}

		public List<string> GetMissingDependenciesNames()
		{
			HashSet<string> hashSet = new HashSet<string>();

			foreach (var item in packagesByUid)
			{
				var var = item.Value;
                if (var.RecursivePackageDependencies != null)
                {
					foreach (var key in var.RecursivePackageDependencies)
					{
						VarPackage package = FileManager.GetPackage(key);
						if (package == null)
						{
							hashSet.Add(key);
						}
					}
				}
			}
			LogUtil.Log("GetMissingDependenciesNames " + hashSet.Count);
			return hashSet.ToList();
		}


		public static void RegisterRestrictedReadPath(string path)
		{
			if (restrictedReadPaths == null)
			{
				restrictedReadPaths = new HashSet<string>();
			}
			restrictedReadPaths.Add(Path.GetFullPath(path));
		}

		public static void RegisterSecureReadPath(string path)
		{
			if (secureReadPaths == null)
			{
				secureReadPaths = new HashSet<string>();
			}
			secureReadPaths.Add(Path.GetFullPath(path));
		}

		public static void ClearSecureReadPaths()
		{
			if (secureReadPaths == null)
			{
				secureReadPaths = new HashSet<string>();
			}
			else
			{
				secureReadPaths.Clear();
			}
		}

		public static bool IsSecureReadPath(string path)
		{
			return true;
		}

		public static void ClearSecureWritePaths()
		{
			if (secureInternalWritePaths == null)
			{
				secureInternalWritePaths = new HashSet<string>();
			}
			else
			{
				secureInternalWritePaths.Clear();
			}
			if (securePluginWritePaths == null)
			{
				securePluginWritePaths = new HashSet<string>();
			}
			else
			{
				securePluginWritePaths.Clear();
			}
			if (pluginWritePathsThatDoNotNeedConfirm == null)
			{
				pluginWritePathsThatDoNotNeedConfirm = new HashSet<string>();
			}
			else
			{
				pluginWritePathsThatDoNotNeedConfirm.Clear();
			}
		}

		public static void RegisterInternalSecureWritePath(string path)
		{
			if (secureInternalWritePaths == null)
			{
				secureInternalWritePaths = new HashSet<string>();
			}
			secureInternalWritePaths.Add(Path.GetFullPath(path));
		}

		public static void RegisterPluginSecureWritePath(string path, bool doesNotNeedConfirm)
		{
			if (securePluginWritePaths == null)
			{
				securePluginWritePaths = new HashSet<string>();
			}
			if (pluginWritePathsThatDoNotNeedConfirm == null)
			{
				pluginWritePathsThatDoNotNeedConfirm = new HashSet<string>();
			}
			string fullPath = Path.GetFullPath(path);
			securePluginWritePaths.Add(fullPath);
			if (doesNotNeedConfirm)
			{
				pluginWritePathsThatDoNotNeedConfirm.Add(fullPath);
			}
		}

		public static bool IsSecureWritePath(string path)
		{
			//if (secureInternalWritePaths == null)
			//{
			//	secureInternalWritePaths = new HashSet<string>();
			//}
			//string fullPath = GetFullPath(path);
			//bool result = false;
			//foreach (string secureInternalWritePath in secureInternalWritePaths)
			//{
			//	if (fullPath.StartsWith(secureInternalWritePath))
			//	{
			//		return true;
			//	}
			//}
			//return result;
			return true;
		}

		public static bool IsSecurePluginWritePath(string path)
		{
			if (securePluginWritePaths == null)
			{
				securePluginWritePaths = new HashSet<string>();
			}
			string fullPath = GetFullPath(path);
			bool result = false;
			foreach (string securePluginWritePath in securePluginWritePaths)
			{
				if (fullPath.StartsWith(securePluginWritePath))
				{
					return true;
				}
			}
			return result;
		}

		public static bool IsPluginWritePathThatNeedsConfirm(string path)
		{
			if (pluginWritePathsThatDoNotNeedConfirm == null)
			{
				pluginWritePathsThatDoNotNeedConfirm = new HashSet<string>();
			}
			string fullPath = GetFullPath(path);
			bool result = true;
			foreach (string item in pluginWritePathsThatDoNotNeedConfirm)
			{
				if (fullPath.StartsWith(item))
				{
					return false;
				}
			}
			return result;
		}

		public static void RegisterPluginHashToPluginPath(string hash, string path)
		{
			if (pluginHashToPluginPath == null)
			{
				pluginHashToPluginPath = new Dictionary<string, string>();
			}
			pluginHashToPluginPath.Remove(hash);
			pluginHashToPluginPath.Add(hash, path);
		}

		protected static string GetPluginHash()
		{
			StackTrace stackTrace = new StackTrace();
			string result = null;
			for (int i = 0; i < stackTrace.FrameCount; i++)
			{
				StackFrame frame = stackTrace.GetFrame(i);
				MethodBase method = frame.GetMethod();
				AssemblyName name = method.DeclaringType.Assembly.GetName();
				string name2 = name.Name;
				if (name2.StartsWith("MVRPlugin_"))
				{
					result = Regex.Replace(name2, "_[0-9]+$", string.Empty);
					break;
				}
			}
			return result;
		}

		public static void AssertNotCalledFromPlugin()
		{
			string pluginHash = GetPluginHash();
			if (pluginHash != null)
			{
				throw new Exception("Plugin with signature " + pluginHash + " tried to execute forbidden operation");
			}
		}

		public static string GetFullPath(string path)
		{
			string path2 = Regex.Replace(path, "^file:///", string.Empty);
			return Path.GetFullPath(path2);
		}

		public static bool IsPackagePath(string path)
		{
			string input = path.Replace('\\', '/');
			string packageUidOrPath = Regex.Replace(input, ":/.*", string.Empty);
			VarPackage package = GetPackage(packageUidOrPath);
			return package != null;
		}

		public static bool IsSimulatedPackagePath(string path)
		{
			string input = path.Replace('\\', '/');
			string packageUidOrPath = Regex.Replace(input, ":/.*", string.Empty);
			return GetPackage(packageUidOrPath)?.IsSimulated ?? false;
		}

		public static string ConvertSimulatedPackagePathToNormalPath(string path)
		{
			string text = path.Replace('\\', '/');
			if (text.Contains(":/"))
			{
				string packageUidOrPath = Regex.Replace(text, ":/.*", string.Empty);
				VarPackage package = GetPackage(packageUidOrPath);
				if (package != null && package.IsSimulated)
				{
					string str = Regex.Replace(text, ".*:/", string.Empty);
					path = package.Path + "/" + str;
				}
			}
			return path;
		}

		public static string RemovePackageFromPath(string path)
		{
			string input = Regex.Replace(path, ".*:/", string.Empty);
			return Regex.Replace(input, ".*:\\\\", string.Empty);
		}

		public static string NormalizePath(string path)
		{
			string text = path;
			VarFileEntry varFileEntry = GetVarFileEntry(path);
			if (varFileEntry == null)
			{
				string fullPath = GetFullPath(path);
				string oldValue = Path.GetFullPath(".") + "\\";
				string text2 = fullPath.Replace(oldValue, string.Empty);
				if (text2 != fullPath)
				{
					text = text2;
				}
				return text.Replace('\\', '/');
			}
			return varFileEntry.Uid;
		}

		public static string GetDirectoryName(string path, bool returnSlashPath = false)
		{
			VarFileEntry value;
			string path2 = (uidToVarFileEntry != null && uidToVarFileEntry.TryGetValue(path, out value)) ?
				//((!returnSlashPath) ? value.Path : value.SlashPath) : 
				//((!returnSlashPath) ? path.Replace('/', '\\') : path.Replace('\\', '/'));
				value.Path : path.Replace('\\', '/');
			return Path.GetDirectoryName(path2);
		}

		public static string GetSuggestedBrowserDirectoryFromDirectoryPath(string suggestedDir, string currentDir, bool allowPackagePath = true)
		{
			if (currentDir == null || currentDir == string.Empty)
			{
				return suggestedDir;
			}
			string input = suggestedDir.Replace('\\', '/');
			input = Regex.Replace(input, "/$", string.Empty);
			string text = currentDir.Replace('\\', '/');
			VarDirectoryEntry varDirectoryEntry = GetVarDirectoryEntry(text);
			if (varDirectoryEntry != null)
			{
				if (!allowPackagePath)
				{
					return null;
				}
				string text2 = varDirectoryEntry.InternalPath.Replace(input, string.Empty);
				if (varDirectoryEntry.InternalPath != text2)
				{
					//text2 = text2.Replace('/', '\\');
					return varDirectoryEntry.Package.Path + ":/" + input + text2;
				}
			}
			else
			{
				string text3 = text.Replace(input, string.Empty);
				if (text != text3)
				{
					//text3 = text3.Replace('/', '\\');
					return suggestedDir + text3;
				}
			}
			return null;
		}

		public static void SetLoadDir(string dir, bool restrictPath = false)
		{
			if (loadDirStack != null)
			{
				loadDirStack.Clear();
			}
			PushLoadDir(dir, restrictPath);
		}

		public static void PushLoadDir(string dir, bool restrictPath = false)
		{
			string text = dir.Replace('\\', '/');
			if (text != "/")
			{
				text = Regex.Replace(text, "/$", string.Empty);
			}
			if (restrictPath && !IsSecureReadPath(text))
			{
				throw new Exception("Attempted to push load dir for non-secure dir " + text);
			}
			if (loadDirStack == null)
			{
				loadDirStack = new LinkedList<string>();
			}
			loadDirStack.AddLast(text);
		}

		public static string PopLoadDir()
		{
			string result = null;
			if (loadDirStack != null && loadDirStack.Count > 0)
			{
				result = loadDirStack.Last.Value;
				loadDirStack.RemoveLast();
			}
			return result;
		}

		public static void SetLoadDirFromFilePath(string path, bool restrictPath = false)
		{
			if (loadDirStack != null)
			{
				loadDirStack.Clear();
			}
			PushLoadDirFromFilePath(path, restrictPath);
		}

		public static void PushLoadDirFromFilePath(string path, bool restrictPath = false)
		{
			if (restrictPath && !IsSecureReadPath(path))
			{
				throw new Exception("Attempted to set load dir from non-secure path " + path);
			}
			FileEntry fileEntry = GetFileEntry(path);
			string dir;
			if (fileEntry != null)
			{
				if (fileEntry is VarFileEntry)
				{
					dir = Path.GetDirectoryName(fileEntry.Uid);
				}
				else
				{
					dir = Path.GetDirectoryName(fileEntry.Path);
					string oldValue = Path.GetFullPath(".") + "\\";
					dir = dir.Replace(oldValue, string.Empty);
				}
			}
			else
			{
				dir = Path.GetDirectoryName(GetFullPath(path));
				string oldValue2 = Path.GetFullPath(".") + "\\";
				dir = dir.Replace(oldValue2, string.Empty);
			}
			PushLoadDir(dir, restrictPath);
		}

		public static string PackageIDToPackageGroupID(string packageId)
		{
			string input = Regex.Replace(packageId, "\\.[0-9]+$", string.Empty);
			input = Regex.Replace(input, "\\.latest$", string.Empty);
			return Regex.Replace(input, "\\.min[0-9]+$", string.Empty);
		}

		public static string PackageIDToPackageVersion(string packageId)
		{
			Match match = Regex.Match(packageId, "[0-9]+$");
			if (match.Success)
			{
				return match.Value;
			}
			return null;
		}

		public static string NormalizeID(string id)
		{
			if (id.StartsWith("SELF:"))
			{
				string currentPackageUid = CurrentPackageUid;
				if (currentPackageUid != null)
				{
					return id.Replace("SELF:", currentPackageUid + ":");
				}
				return id.Replace("SELF:", string.Empty);
			}
			return NormalizeCommon(id);
		}

		protected static string NormalizeCommon(string path)
		{
			string text = path;
			Match match;
			if ((match = Regex.Match(text, "^(([^\\.]+\\.[^\\.]+)\\.latest):")).Success)
			{
				string value = match.Groups[1].Value;
				string value2 = match.Groups[2].Value;
				VarPackageGroup packageGroup = GetPackageGroup(value2);
				if (packageGroup != null)
				{
					VarPackage newestEnabledPackage = packageGroup.NewestEnabledPackage;
					if (newestEnabledPackage != null)
					{
						text = text.Replace(value, newestEnabledPackage.Uid);
					}
				}
			}
			else if ((match = Regex.Match(text, "^(([^\\.]+\\.[^\\.]+)\\.min([0-9]+)):")).Success)
			{
				string value3 = match.Groups[1].Value;
				string value4 = match.Groups[2].Value;
				int requestVersion = int.Parse(match.Groups[3].Value);
				VarPackageGroup packageGroup2 = GetPackageGroup(value4);
				if (packageGroup2 != null)
				{
					VarPackage closestMatchingPackageVersion = packageGroup2.GetClosestMatchingPackageVersion(requestVersion, true, false);
					if (closestMatchingPackageVersion != null)
					{
						text = text.Replace(value3, closestMatchingPackageVersion.Uid);
					}
				}
			}
			else if ((match = Regex.Match(text, "^([^\\.]+\\.[^\\.]+\\.[0-9]+):")).Success)
			{
				string value5 = match.Groups[1].Value;
				VarPackage package = GetPackage(value5);
				if (package == null || !package.Enabled)
				{
					string packageGroupUid = PackageIDToPackageGroupID(value5);
					VarPackageGroup packageGroup3 = GetPackageGroup(packageGroupUid);
					if (packageGroup3 != null)
					{
						package = packageGroup3.NewestEnabledPackage;
						if (package != null)
						{
							text = text.Replace(value5, package.Uid);
						}
					}
				}
			}
			return text;
		}

		public static string NormalizeLoadPath(string path)
		{
			string result = path;
			if (path != null && path != string.Empty && path != "/" && path != "NULL")
			{
				result = path.Replace('\\', '/');
				string currentLoadDir = CurrentLoadDir;
				if (currentLoadDir != null && currentLoadDir != string.Empty)
				{
					if (!result.Contains("/"))
					{
						result = currentLoadDir + "/" + result;
					}
					else if (Regex.IsMatch(result, "^\\./"))
					{
						result = Regex.Replace(result, "^\\./", currentLoadDir + "/");
					}
				}
				if (result.StartsWith("SELF:/"))
				{
					string currentPackageUid = CurrentPackageUid;
					result = ((currentPackageUid == null) ? result.Replace("SELF:/", string.Empty) : result.Replace("SELF:/", currentPackageUid + ":/"));
				}
				else
				{
					result = NormalizeCommon(result);
				}
			}
			return result;
		}

		public static void SetSaveDir(string path, bool restrictPath = true)
		{
			if (path == null || path == string.Empty)
			{
				CurrentSaveDir = string.Empty;
				return;
			}
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsPackagePath(path))
			{
				if (restrictPath && !IsSecureWritePath(path))
				{
					throw new Exception("Attempted to set save dir from non-secure path " + path);
				}
				string fullPath = GetFullPath(path);
				string oldValue = Path.GetFullPath(".") + "\\";
				fullPath = fullPath.Replace(oldValue, string.Empty);
				CurrentSaveDir = fullPath.Replace('\\', '/');
			}
		}

		public static void SetSaveDirFromFilePath(string path, bool restrictPath = true)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsPackagePath(path))
			{
				if (restrictPath && !IsSecureWritePath(path))
				{
					throw new Exception("Attempted to set save dir from non-secure path " + path);
				}
				string directoryName = Path.GetDirectoryName(GetFullPath(path));
				string oldValue = Path.GetFullPath(".") + "\\";
				directoryName = directoryName.Replace(oldValue, string.Empty);
				CurrentSaveDir = directoryName.Replace('\\', '/');
			}
		}

		public static void SetNullSaveDir()
		{
			CurrentSaveDir = null;
		}

		public static string NormalizeSavePath(string path)
		{
			string text = path;
			if (path != null && path != string.Empty && path != "/" && path != "NULL")
			{
				string path2 = Regex.Replace(path, "^file:///", string.Empty);
				string fullPath = Path.GetFullPath(path2);
				string oldValue = Path.GetFullPath(".") + "\\";
				string text2 = fullPath.Replace(oldValue, string.Empty);
				if (text2 != fullPath)
				{
					text = text2;
				}
				text = text.Replace('\\', '/');
				string fileName = Path.GetFileName(text2);
				string text3 = Path.GetDirectoryName(text2);
				if (text3 != null)
				{
					text3 = text3.Replace('\\', '/');
				}
				if (CurrentSaveDir == text3)
				{
					text = fileName;
				}
				else if (CurrentSaveDir != null && CurrentSaveDir != string.Empty && Regex.IsMatch(text3, "^" + CurrentSaveDir + "/"))
				{
					text = text3.Replace(CurrentSaveDir, ".") + "/" + fileName;
				}
			}
			return text;
		}

		public static List<VarPackage> GetPackages()
		{
			if (packagesByUid != null)
			{
				return packagesByUid.Values.ToList();
			}
			return new List<VarPackage>();
		}

		public static List<string> GetPackageUids()
		{
			List<string> list;
			if (packagesByUid != null)
			{
				list = packagesByUid.Keys.ToList();
				list.Sort();
			}
			else
			{
				list = new List<string>();
			}
			return list;
		}

		public static bool IsPackage(string packageUidOrPath)
		{
			if (packagesByUid != null && packagesByUid.ContainsKey(packageUidOrPath))
			{
				return true;
			}
			if (packagesByPath != null && packagesByPath.ContainsKey(packageUidOrPath))
			{
				return true;
			}
			return false;
		}

		public static VarPackage GetPackage(string packageUidOrPath)
		{
			VarPackage value = null;
			Match match;
			if ((match = Regex.Match(packageUidOrPath, "^([^\\.]+\\.[^\\.]+)\\.latest$")).Success)
			{
				string value2 = match.Groups[1].Value;
				VarPackageGroup packageGroup = GetPackageGroup(value2);
				if (packageGroup != null)
				{
					value = packageGroup.NewestPackage;
				}
			}
			else if ((match = Regex.Match(packageUidOrPath, "^([^\\.]+\\.[^\\.]+)\\.min([0-9]+)$")).Success)
			{
				string value3 = match.Groups[1].Value;
				int requestVersion = int.Parse(match.Groups[2].Value);
				VarPackageGroup packageGroup2 = GetPackageGroup(value3);
				if (packageGroup2 != null)
				{
					value = packageGroup2.GetClosestMatchingPackageVersion(requestVersion, false, false);
				}
			}
			else if (packagesByUid != null && packagesByUid.ContainsKey(packageUidOrPath))
			{
				packagesByUid.TryGetValue(packageUidOrPath, out value);
			}
			else if (packagesByPath != null && packagesByPath.ContainsKey(packageUidOrPath))
			{
				packagesByPath.TryGetValue(packageUidOrPath, out value);
			}
			return value;
		}

		public static List<VarPackageGroup> GetPackageGroups()
		{
			if (packageGroups != null)
			{
				return packageGroups.Values.ToList();
			}
			return new List<VarPackageGroup>();
		}

		public static VarPackageGroup GetPackageGroup(string packageGroupUid)
		{
			VarPackageGroup value = null;
			if (packageGroups != null)
			{
				packageGroups.TryGetValue(packageGroupUid, out value);
			}
			return value;
		}

		public static string CleanFilePath(string path)
		{
			return path?.Replace('\\', '/');
		}

		//public static void FindAllFiles(string dir, string pattern, List<FileEntry> foundFiles, bool restrictPath = false)
		//{
		//	FindRegularFiles(dir, pattern, foundFiles, restrictPath);
		//	FindVarFiles(dir, pattern, foundFiles);
		//}

		//public static void FindAllFilesRegex(string dir, string regex, List<FileEntry> foundFiles, bool restrictPath = false)
		//{
		//	FindRegularFilesRegex(dir, regex, foundFiles, restrictPath);
		//	FindVarFilesRegex(dir, regex, foundFiles);
		//}

		//public static void FindRegularFiles(string dir, string pattern, List<FileEntry> foundFiles, bool restrictPath = false)
		//{
		//	if (Directory.Exists(dir))
		//	{
		//		if (restrictPath && !IsSecureReadPath(dir))
		//		{
		//			throw new Exception("Attempted to find files for non-secure path " + dir);
		//		}
		//		string regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
		//		FindRegularFilesRegex(dir, regex, foundFiles, restrictPath);
		//	}
		//}

		public static bool CheckIfDirectoryChanged(string dir, DateTime previousCheckTime, bool recurse = true)
		{
			if (Directory.Exists(dir))
			{
				DateTime lastWriteTime = Directory.GetLastWriteTime(dir);
				if (lastWriteTime > previousCheckTime)
				{
					return true;
				}
				if (recurse)
				{
					string[] directories = Directory.GetDirectories(dir);
					foreach (string dir2 in directories)
					{
						if (CheckIfDirectoryChanged(dir2, previousCheckTime, recurse))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		//public static void FindRegularFilesRegex(string dir, string regex, List<FileEntry> foundFiles, bool restrictPath = false)
		//{
		//	dir = CleanDirectoryPath(dir);
		//	if (!Directory.Exists(dir))
		//	{
		//		return;
		//	}
		//	if (restrictPath && !IsSecureReadPath(dir))
		//	{
		//		throw new Exception("Attempted to find files for non-secure path " + dir);
		//	}
		//	string[] files = Directory.GetFiles(dir);
		//	foreach (string text in files)
		//	{
		//		if (Regex.IsMatch(text, regex, RegexOptions.IgnoreCase))
		//		{
		//			SystemFileEntry systemFileEntry = new SystemFileEntry(text);
		//			if (systemFileEntry.Exists)
		//			{
		//				foundFiles.Add(systemFileEntry);
		//			}
		//			else
		//			{
		//				UnityEngine.Debug.LogError("Error in lookup SystemFileEntry for " + text);
		//			}
		//		}
		//	}
		//	string[] directories = Directory.GetDirectories(dir);
		//	foreach (string dir2 in directories)
		//	{
		//		FindRegularFilesRegex(dir2, regex, foundFiles);
		//	}
		//}

		public static void FindVarFiles(string dir, string pattern, List<FileEntry> foundFiles)
		{
			if (allVarFileEntries != null)
			{
				string regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
				FindVarFilesRegex(dir, regex, foundFiles);
			}
		}

		public static void FindVarFilesRegex(string dir, string regex, List<FileEntry> foundFiles)
		{
			dir = CleanDirectoryPath(dir);
			if (allVarFileEntries != null)
			{
				foreach (VarFileEntry allVarFileEntry in allVarFileEntries)
				{
					if (allVarFileEntry.InternalPath.StartsWith(dir) && Regex.IsMatch(allVarFileEntry.Name, regex, RegexOptions.IgnoreCase))
					{
						foundFiles.Add(allVarFileEntry);
					}
				}
			}
		}

		public static bool FileExists(string path, bool onlySystemFiles = false, bool restrictPath = false)
		{
			if (path != null && path != string.Empty)
			{
				if (!onlySystemFiles)
				{
					string key = CleanFilePath(path);
					if (uidToVarFileEntry != null && uidToVarFileEntry.ContainsKey(path))
					{
						return true;
					}
					if (pathToVarFileEntry != null && pathToVarFileEntry.ContainsKey(key))
					{
						return true;
					}
				}
				if (File.Exists(path))
				{
					if (restrictPath && !IsSecureReadPath(path))
					{
						throw new Exception("Attempted to check file existence for non-secure path " + path);
					}
					return true;
				}
			}
			return false;
		}

		public static bool IsFileInPackage(string path)
		{
			string key = CleanFilePath(path);
			if (uidToVarFileEntry != null && uidToVarFileEntry.ContainsKey(key))
			{
				return true;
			}
			if (pathToVarFileEntry != null && pathToVarFileEntry.ContainsKey(key))
			{
				return true;
			}
			return false;
		}

		//public static bool IsFavorite(string path, bool restrictPath = false)
		//{
		//	FileEntry fileEntry = GetVarFileEntry(path);
		//	if (fileEntry == null)
		//	{
		//		fileEntry = GetSystemFileEntry(path, restrictPath);
		//	}
		//	return fileEntry?.IsFavorite() ?? false;
		//}

		//public static void SetFavorite(string path, bool fav, bool restrictPath = false)
		//{
		//	FileEntry fileEntry = GetVarFileEntry(path);
		//	if (fileEntry == null)
		//	{
		//		fileEntry = GetSystemFileEntry(path, restrictPath);
		//	}
		//	fileEntry?.SetFavorite(fav);
		//}

		//public static bool IsHidden(string path, bool restrictPath = false)
		//{
		//	FileEntry fileEntry = GetVarFileEntry(path);
		//	if (fileEntry == null)
		//	{
		//		fileEntry = GetSystemFileEntry(path, restrictPath);
		//	}
		//	return fileEntry?.IsHidden() ?? false;
		//}

		//public static void SetHidden(string path, bool hide, bool restrictPath = false)
		//{
		//	FileEntry fileEntry = GetVarFileEntry(path);
		//	if (fileEntry == null)
		//	{
		//		fileEntry = GetSystemFileEntry(path, restrictPath);
		//	}
		//	fileEntry?.SetHidden(hide);
		//}

		public static FileEntry GetFileEntry(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = GetVarFileEntry(path);
			if (fileEntry == null)
			{
				fileEntry = GetSystemFileEntry(path, restrictPath);
			}
			return fileEntry;
		}

		public static SystemFileEntry GetSystemFileEntry(string path, bool restrictPath = false)
		{
			SystemFileEntry result = null;
			if (File.Exists(path))
			{
				if (restrictPath && !IsSecureReadPath(path))
				{
					throw new Exception("Attempted to get file entry for non-secure path " + path);
				}
				result = new SystemFileEntry(path);
			}
			return result;
		}

		public static VarFileEntry GetVarFileEntry(string path)
		{
			VarFileEntry value = null;
			string key = CleanFilePath(path);
			if ((uidToVarFileEntry != null && uidToVarFileEntry.TryGetValue(key, out value))
				|| pathToVarFileEntry == null || pathToVarFileEntry.TryGetValue(key, out value))
			{
			}
			return value;
		}

		public static void SortFileEntriesByLastWriteTime(List<FileEntry> fileEntries)
		{
			fileEntries.Sort((FileEntry e1, FileEntry e2) => e1.LastWriteTime.CompareTo(e2.LastWriteTime));
		}

		public static string CleanDirectoryPath(string path)
		{
			if (path != null)
			{
				string input = path.Replace('\\', '/');
				return Regex.Replace(input, "/$", string.Empty);
			}
			return null;
		}

		public static int FolderContentsCount(string path)
		{
			int num = Directory.GetFiles(path).Length;
			string[] directories = Directory.GetDirectories(path);
			string[] array = directories;
			foreach (string path2 in array)
			{
				num += FolderContentsCount(path2);
			}
			return num;
		}

		public static bool DirectoryExists(string path, bool onlySystemDirectories = false, bool restrictPath = false)
		{
			return false;
		}

		public static bool IsDirectoryInPackage(string path)
		{
			//string key = CleanDirectoryPath(path);
			//if (uidToVarDirectoryEntry != null && uidToVarDirectoryEntry.ContainsKey(key))
			//{
			//	return true;
			//}
			//if (pathToVarDirectoryEntry != null && pathToVarDirectoryEntry.ContainsKey(key))
			//{
			//	return true;
			//}
			return false;
		}

		//public static DirectoryEntry GetDirectoryEntry(string path, bool restrictPath = false)
		//{
		//	string path2 = Regex.Replace(path, "(/|\\\\)$", string.Empty);
		//	DirectoryEntry directoryEntry = GetVarDirectoryEntry(path2);
		//	if (directoryEntry == null)
		//	{
		//		directoryEntry = GetSystemDirectoryEntry(path2, restrictPath);
		//	}
		//	return directoryEntry;
		//}

		//public static SystemDirectoryEntry GetSystemDirectoryEntry(string path, bool restrictPath = false)
		//{
		//	SystemDirectoryEntry result = null;
		//	if (Directory.Exists(path))
		//	{
		//		if (restrictPath && !IsSecureReadPath(path))
		//		{
		//			throw new Exception("Attempted to get directory entry for non-secure path " + path);
		//		}
		//		result = new SystemDirectoryEntry(path);
		//	}
		//	return result;
		//}

		public static VarDirectoryEntry GetVarDirectoryEntry(string path)
		{
			VarDirectoryEntry value = null;
			//string key = CleanDirectoryPath(path);
			//if ((uidToVarDirectoryEntry != null && uidToVarDirectoryEntry.TryGetValue(key, out value)) 
			//	|| pathToVarDirectoryEntry == null || pathToVarDirectoryEntry.TryGetValue(key, out value))
			//{
			//}
			return value;
		}

		public static VarDirectoryEntry GetVarRootDirectoryEntryFromPath(string path)
		{
			VarDirectoryEntry value = null;
			//if (varPackagePathToRootVarDirectory != null)
			//{
			//	varPackagePathToRootVarDirectory.TryGetValue(path, out value);
			//}
			return value;
		}

		//public static string[] GetDirectories(string dir, string pattern = null, bool restrictPath = false)
		//{
		//	if (restrictPath && !IsSecureReadPath(dir))
		//	{
		//		throw new Exception("Attempted to get directories at non-secure path " + dir);
		//	}
		//	List<string> list = new List<string>();
		//	DirectoryEntry directoryEntry = GetDirectoryEntry(dir, restrictPath);
		//	if (directoryEntry == null)
		//	{
		//		throw new Exception("Attempted to get directories at non-existent path " + dir);
		//	}
		//	string text = null;
		//	if (pattern != null)
		//	{
		//		text = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
		//	}
		//	foreach (DirectoryEntry subDirectory in directoryEntry.SubDirectories)
		//	{
		//		if (text == null || Regex.IsMatch(subDirectory.Name, text))
		//		{
		//			list.Add(dir + "\\" + subDirectory.Name);
		//		}
		//	}
		//	return list.ToArray();
		//}

		//public static string[] GetFiles(string dir, string pattern = null, bool restrictPath = false)
		//{
		//	if (restrictPath && !IsSecureReadPath(dir))
		//	{
		//		throw new Exception("Attempted to get files at non-secure path " + dir);
		//	}
		//	List<string> list = new List<string>();
		//	DirectoryEntry directoryEntry = GetDirectoryEntry(dir, restrictPath);
		//	if (directoryEntry == null)
		//	{
		//		throw new Exception("Attempted to get files at non-existent path " + dir);
		//	}
		//	foreach (FileEntry file in directoryEntry.GetFiles(pattern))
		//	{
		//		list.Add(dir + "\\" + file.Name);
		//	}
		//	return list.ToArray();
		//}

		public static void CreateDirectory(string path)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!DirectoryExists(path))
			{
				//if (!IsSecureWritePath(path))
				//{
				//	throw new Exception("Attempted to create directory at non-secure path " + path);
				//}
				Directory.CreateDirectory(path);
			}
		}
		public static void DeleteDirectory(string path, bool recursive = false)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (DirectoryExists(path))
			{
				if (!IsSecureWritePath(path))
				{
					throw new Exception("Attempted to delete file at non-secure path " + path);
				}
				Directory.Delete(path, recursive);
			}
		}
		public static void MoveDirectory(string oldPath, string newPath)
		{
			oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
			if (!IsSecureWritePath(oldPath))
			{
				throw new Exception("Attempted to move directory from non-secure path " + oldPath);
			}
			newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
			if (!IsSecureWritePath(newPath))
			{
				throw new Exception("Attempted to move directory to non-secure path " + newPath);
			}
			Directory.Move(oldPath, newPath);
		}
		public static FileEntryStream OpenStream(FileEntry fe)
		{
			if (fe == null)
			{
				throw new Exception("Null FileEntry passed to OpenStreamReader");
			}
			if (fe is VarFileEntry)
			{
				return new VarFileEntryStream(fe as VarFileEntry);
			}
			if (fe is SystemFileEntry)
			{
				return new SystemFileEntryStream(fe as SystemFileEntry);
			}
			throw new Exception("Unknown FileEntry class passed to OpenStreamReader");
		}

		public static FileEntryStream OpenStream(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = GetFileEntry(path, restrictPath);
			if (fileEntry == null)
			{
				throw new Exception("Path " + path + " not found");
			}
			return OpenStream(fileEntry);
		}

		public static FileEntryStreamReader OpenStreamReader(FileEntry fe)
		{
			if (fe == null)
			{
				throw new Exception("Null FileEntry passed to OpenStreamReader");
			}
			if (fe is VarFileEntry)
			{
				return new VarFileEntryStreamReader(fe as VarFileEntry);
			}
			if (fe is SystemFileEntry)
			{
				return new SystemFileEntryStreamReader(fe as SystemFileEntry);
			}
			throw new Exception("Unknown FileEntry class passed to OpenStreamReader");
		}

		public static FileEntryStreamReader OpenStreamReader(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = GetFileEntry(path, restrictPath);
			if (fileEntry == null)
			{
				throw new Exception("Path " + path + " not found");
			}
			return OpenStreamReader(fileEntry);
		}

		public static IEnumerator ReadAllBytesCoroutine(FileEntry fe, byte[] result)
		{
			Thread loadThread = new Thread((ThreadStart)delegate
			{
				byte[] buffer = new byte[32768];
				using (FileEntryStream fileEntryStream = OpenStream(fe))
				{
					using (MemoryStream destination = new MemoryStream(result))
					{
						StreamUtils.Copy(fileEntryStream.Stream, destination, buffer);
					}
				}
			});
			loadThread.Start();
			while (loadThread.IsAlive)
			{
				yield return null;
			}
		}

		public static byte[] ReadAllBytes(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = GetFileEntry(path, restrictPath);
			if (fileEntry == null)
			{
				throw new Exception("Path " + path + " not found");
			}
			return ReadAllBytes(fileEntry);
		}

		public static byte[] ReadAllBytes(FileEntry fe)
		{
			if (fe is VarFileEntry)
			{
				byte[] buffer = new byte[32768];
				using (FileEntryStream fileEntryStream = OpenStream(fe))
				{
					byte[] array = new byte[fe.Size];
					using (MemoryStream destination = new MemoryStream(array))
					{
						StreamUtils.Copy(fileEntryStream.Stream, destination, buffer);
					}
					return array;
				}
			}
			return File.ReadAllBytes(fe.Path);
		}

		public static string ReadAllText(string path, bool restrictPath = false)
		{
			FileEntry fileEntry = GetFileEntry(path, restrictPath);
			if (fileEntry == null)
			{
				throw new Exception("Path " + path + " not found");
			}
			return ReadAllText(fileEntry);
		}

		public static string ReadAllText(FileEntry fe)
		{
			using (FileEntryStreamReader fileEntryStreamReader = OpenStreamReader(fe))
			{
				return fileEntryStreamReader.ReadToEnd();
			}
		}

		public static FileStream OpenStreamForCreate(string path)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecureWritePath(path))
			{
				throw new Exception("Attempted to open stream for create at non-secure path " + path);
			}
			return File.Open(path, FileMode.Create);
		}

		public static StreamWriter OpenStreamWriter(string path)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecureWritePath(path))
			{
				throw new Exception("Attempted to open stream writer at non-secure path " + path);
			}
			return new StreamWriter(path);
		}

		public static void WriteAllText(string path, string text)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecureWritePath(path))
			{
				throw new Exception("Attempted to write all text at non-secure path " + path);
			}
			File.WriteAllText(path, text);
		}

		public static void WriteAllBytes(string path, byte[] bytes)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecureWritePath(path))
			{
				throw new Exception("Attempted to write all bytes at non-secure path " + path);
			}
			File.WriteAllBytes(path, bytes);
		}

		public static void SetFileAttributes(string path, FileAttributes attrs)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (!IsSecureWritePath(path))
			{
				throw new Exception("Attempted to set file attributes at non-secure path " + path);
			}
			File.SetAttributes(path, attrs);
		}

		public static void DeleteFile(string path)
		{
			path = ConvertSimulatedPackagePathToNormalPath(path);
			if (File.Exists(path))
			{
				if (!IsSecureWritePath(path))
				{
					throw new Exception("Attempted to delete file at non-secure path " + path);
				}
				File.Delete(path);
			}
		}

		protected static void DoFileCopy(string oldPath, string newPath)
		{
			FileEntry fileEntry = GetFileEntry(oldPath);
			if (fileEntry != null && fileEntry is VarFileEntry)
			{
				byte[] buffer = new byte[4096];
				using (FileEntryStream fileEntryStream = OpenStream(fileEntry))
				{
					using (FileStream destination = OpenStreamForCreate(newPath))
					{
						StreamUtils.Copy(fileEntryStream.Stream, destination, buffer);
					}
				}
			}
			else
			{
				File.Copy(oldPath, newPath);
			}
		}

		public static void CopyFile(string oldPath, string newPath, bool restrictPath = false)
		{
			oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
			if (restrictPath && !IsSecureReadPath(oldPath))
			{
				throw new Exception("Attempted to copy file from non-secure path " + oldPath);
			}
			newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
			if (!IsSecureWritePath(newPath))
			{
				throw new Exception("Attempted to copy file to non-secure path " + newPath);
			}
			DoFileCopy(oldPath, newPath);
		}


		protected static void DoFileMove(string oldPath, string newPath, bool overwrite = true)
		{
			if (File.Exists(newPath))
			{
				if (!overwrite)
				{
					throw new Exception("File " + newPath + " exists. Cannot move into");
				}
				File.Delete(newPath);
			}
			File.Move(oldPath, newPath);
		}

		public static void MoveFile(string oldPath, string newPath, bool overwrite = true)
		{
			oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
			if (!IsSecureWritePath(oldPath))
			{
				throw new Exception("Attempted to move file from non-secure path " + oldPath);
			}
			newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
			if (!IsSecureWritePath(newPath))
			{
				throw new Exception("Attempted to move file to non-secure path " + newPath);
			}
			DoFileMove(oldPath, newPath, overwrite);
		}

		private void Awake()
		{
			singleton = this;
		}

		private void OnDestroy()
		{
			//UnityEngine.Debug.Log("Hook FileManager OnDestroy");
			ClearAll();
		}
	}

}
