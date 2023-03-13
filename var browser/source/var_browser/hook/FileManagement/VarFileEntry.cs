using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace var_browser
{
	[System.Serializable]
	public class SerializableFavorite
	{
		public string[] FavoriteNames;
	}
	[System.Serializable]
	public class SerializableNames
	{
		public string[] Names;
	}

	public class VarFileEntry : FileEntry
	{
		public VarPackage Package { get; protected set; }

		public string InternalPath { get; protected set; }

		public VarFileEntry(VarPackage vp, string entryName, DateTime lastWriteTime, long size, bool simulated = false)
		{
			Package = vp;
			InternalPath = entryName;
			Uid = vp.Uid + ":/" + InternalPath;
			Path = vp.Path + ":/" + InternalPath;
			Name = Regex.Replace(Path, ".*/", string.Empty);
			Exists = true;
			LastWriteTime = lastWriteTime;
			Size = size;
		}

		public override FileEntryStream OpenStream()
		{
			return new VarFileEntryStream(this);
		}

		public List<string> ClothingTags;// = new List<string>();
		public List<string> HairTags;// = new List<string>();

		public override FileEntryStreamReader OpenStreamReader()
		{
			return new VarFileEntryStreamReader(this);
		}

		public override bool HasFlagFile(string flagName)
		{
			return false;
		}

		public bool IsFlagFileModifiable(string flagName)
		{
			return false;
		}

		public override bool IsHidden()
		{
			return false;
		}

		public bool IsHiddenModifiable()
		{
			return false;
		}


		public override bool IsInstalled()
		{
			if(Package.Path.StartsWith("AddonPackages/"))
            {
				return File.Exists(Package.Path);
            }
			else if (Package.Path.StartsWith("AllPackages/"))
            {
				return File.Exists("AddonPackages" + Package.Path.Substring("AllPackages".Length));
            }
			return false;
		}
		public override bool IsFavorite()
		{
			string key = this.Package.Creator + "." + this.Package.Name + ":" + InternalPath;

			if (FavoriteLookup.Contains(key))
				return true;
			return false;
		}
		public override bool IsAutoInstall()
		{
			string key = this.Package.Uid;

			if (AutoInstallLookup.Contains(key))
				return true;
			return false;
		}
		public override void SetFavorite(bool b)
		{
			//string key = this.Package.Uid + ":" + InternalPath;
			string key = this.Package.Creator+"."+this.Package.Name+ ":" + InternalPath;
			if (b)
			{
				FavoriteLookup.Add(key);
			}
			else
			{
				FavoriteLookup.Remove(key);
			}

			if (!Directory.Exists(GlobalInfo.FavoriteDirectory))
			{
				Directory.CreateDirectory(GlobalInfo.FavoriteDirectory);
			}

			SerializableFavorite sf = new SerializableFavorite();
			var list = new List<string>();
			foreach(var item in FavoriteLookup)
            {
				list.Add(item);
            }
			sf.FavoriteNames = list.ToArray();
			File.WriteAllText(GlobalInfo.FavoritePath, JsonUtility.ToJson(sf));
		}

        public override bool SetAutoInstall(bool b)
        {
			string key = this.Package.Uid;
			SetAutoInstallInternal(key, b);

			if(b)
            {
				bool dirty=this.Package.InstallSelf();
				return dirty;
			}
			//设置false的时候不会自动卸载

			return false;
		}

	}

}
