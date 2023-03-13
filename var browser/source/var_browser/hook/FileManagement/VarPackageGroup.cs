using System;
using System.Linq;
using System.Collections.Generic;
using SimpleJSON;


namespace var_browser
{
	public class VarPackageGroup
	{
		protected List<int> _versions;

		protected List<int> _enabledVersions;

		//protected string _userNotes = string.Empty;

		//protected Dictionary<string, bool> customOptions;

		public string Name { get; protected set; }

		public List<int> Versions
		{
			get
			{
				List<int> list = _versions.ToList();
				list.Sort();
				return list;
			}
		}

		public int NewestVersion
		{
			get
			{
				if (NewestPackage != null)
				{
					return NewestPackage.Version;
				}
				return 0;
			}
		}

		public int NewestEnabledVersion
		{
			get
			{
				if (NewestPackage != null)
				{
					return NewestEnabledPackage.Version;
				}
				return 0;
			}
		}

		public List<VarPackage> Packages { get; protected set; }

		public VarPackage NewestPackage { get; protected set; }

		public VarPackage NewestEnabledPackage { get; protected set; }

		public string UserNotes
		{
			get
			{
				//return _userNotes;
				return null;
			}
			//set
			//{
			//	if (_userNotes != value)
			//	{
			//		_userNotes = value;
			//		SaveUserPrefs();
			//	}
			//}
		}

		public VarPackageGroup(string name)
		{
			Name = name;
			_versions = new List<int>();
			_enabledVersions = new List<int>();
			Packages = new List<VarPackage>();
		}

		public VarPackage GetClosestMatchingPackageVersion(int requestVersion, bool onlyUseEnabledPackages = true, bool returnLatestOnMissing = true)
		{
			int num = -1;
			List<int> list = ((!onlyUseEnabledPackages) ? _versions : _enabledVersions);
			foreach (int item in list)
			{
				if (requestVersion <= item)
				{
					num = item;
					break;
				}
			}
			if (num == -1)
			{
				if (returnLatestOnMissing)
				{
					if (onlyUseEnabledPackages)
					{
						return NewestEnabledPackage;
					}
					return NewestPackage;
				}
			}
			else
			{
				foreach (VarPackage package in Packages)
				{
					if (package.Version == num)
					{
						return package;
					}
				}
			}
			return null;
		}

		protected void SyncNewestVersion()
		{
			NewestPackage = null;
			NewestEnabledPackage = null;
			if (_versions.Count <= 0)
			{
				return;
			}
			int num = _versions[_versions.Count - 1];
			foreach (VarPackage package in Packages)
			{
				if (package.Version == num)
				{
					package.isNewestVersion = true;
					NewestPackage = package;
				}
				else
				{
					package.isNewestVersion = false;
				}
				if (package.Enabled)
				{
					int num2 = _enabledVersions[_enabledVersions.Count - 1];
					if (package.Version == num2)
					{
						package.isNewestEnabledVersion = true;
						NewestEnabledPackage = package;
					}
					else
					{
						package.isNewestEnabledVersion = false;
					}
				}
				else
				{
					package.isNewestEnabledVersion = false;
				}
			}
		}

		//public List<string> GetCustomOptionNames()
		//{
		//	if (customOptions != null)
		//	{
		//		return customOptions.Keys.ToList();
		//	}
		//	return new List<string>();
		//}

		public bool GetCustomOption(string optionName)
		{
			bool value = false;
			//if (customOptions != null)
			//{
			//	customOptions.TryGetValue(optionName, out value);
			//}
			return value;
		}

		public void SetCustomOption(string optionName, bool optionValue)
		{
			//if (customOptions != null)
			//{
			//	if (customOptions.ContainsKey(optionName))
			//	{
			//		customOptions.Remove(optionName);
			//	}
			//	customOptions.Add(optionName, optionValue);
			//	SaveUserPrefs();
			//	FileManager.Refresh();
			//}
		}

		protected void LoadUserPrefs()
		{
			//string path = FileManager.UserPrefsFolder + "/" + Name + ".prefs";
			//if (FileManager.FileExists(path))
			//{
			//	using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(path))
			//	{
			//		string aJSON = fileEntryStreamReader.ReadToEnd();
			//		JSONClass asObject = JSON.Parse(aJSON).AsObject;
			//		if (asObject != null)
			//		{
			//			_userNotes = asObject["userNotes"];
			//			JSONClass asObject2 = asObject["customOptions"].AsObject;
			//			customOptions = new Dictionary<string, bool>();
			//			if (asObject2 != null)
			//			{
			//				foreach (string key in asObject2.Keys)
			//				{
			//					if (!customOptions.ContainsKey(key))
			//					{
			//						customOptions.Add(key, asObject2[key].AsBool);
			//					}
			//				}
			//				return;
			//			}
			//		}
			//		return;
			//	}
			//}
			//_userNotes = string.Empty;
			//customOptions = new Dictionary<string, bool>();
			//VarPackage newestPackage = NewestPackage;
			//if (newestPackage == null)
			//{
			//	return;
			//}
			//List<string> customOptionNames = newestPackage.GetCustomOptionNames();
			//foreach (string item in customOptionNames)
			//{
			//	customOptions.Add(item, newestPackage.GetCustomOption(item));
			//}
		}

		protected void SaveUserPrefs()
		{
			//string text = FileManager.UserPrefsFolder + "/" + Name + ".prefs";
			//JSONClass jSONClass = new JSONClass();
			//jSONClass["userNotes"] = UserNotes;
			//JSONClass jSONClass2 = (JSONClass)(jSONClass["customOptions"] = new JSONClass());
			//if (customOptions != null)
			//{
			//	foreach (KeyValuePair<string, bool> customOption in customOptions)
			//	{
			//		jSONClass2[customOption.Key].AsBool = customOption.Value;
			//	}
			//}
			//string text2 = jSONClass.ToString(string.Empty);
			//try
			//{
			//	FileManager.WriteAllText(text, text2);
			//}
			//catch (Exception ex)
			//{
			//	//SuperController.LogError("Error during save of prefs file " + text + ": " + ex.Message);
			//}
		}

		public void AddPackage(VarPackage vp)
		{
			Packages.Add(vp);
			if (_versions.Contains(vp.Version))
			{
				throw new Exception("Tried to add package to group " + Name + " with version " + vp.Version + " that was already added");
			}
			_versions.Add(vp.Version);
			_versions.Sort();
			if (vp.Enabled)
			{
				_enabledVersions.Add(vp.Version);
				_enabledVersions.Sort();
			}
			SyncNewestVersion();
		}

		public void RemovePackage(VarPackage vp)
		{
			Packages.Remove(vp);
			_versions.Remove(vp.Version);
			_versions.Sort();
			if (vp.Enabled)
			{
				_enabledVersions.Remove(vp.Version);
				_enabledVersions.Sort();
			}
			SyncNewestVersion();
		}

		public void Init()
		{
			//LoadUserPrefs();
		}
	}

}
