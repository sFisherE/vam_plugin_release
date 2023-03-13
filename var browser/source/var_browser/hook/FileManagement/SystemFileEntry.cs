using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace var_browser
{
	public class SystemFileEntry : FileEntry
	{
		public bool isVar = false;
		public VarPackage package;
		//public bool isPlugin = false;
		public SystemFileEntry(string path)
			: base(path)
		{
			Exists = File.Exists(Path);
			if (Exists)
			{
				FileInfo fileInfo = new FileInfo(Path);
				LastWriteTime = fileInfo.LastWriteTime;
				Size = fileInfo.Length;
			}

			//isPlugin = false;
			package = FileManager.GetPackage(System.IO.Path.GetFileNameWithoutExtension(Path));
            //if (package != null && package.Scripts != null && package.Scripts.Length > 0)
            //{
            //    isPlugin = true;
            //}
            if (package != null)
            {
				isVar = true;
            }
			
        }

		public override FileEntryStream OpenStream()
		{
			return new SystemFileEntryStream(this);
		}

		public override FileEntryStreamReader OpenStreamReader()
		{
			return new SystemFileEntryStreamReader(this);
		}
        public override bool IsInstalled()
        {
            if (isVar)
            {
                if (Path.StartsWith("AllPackages"))
                {
					string path="AddonPackages" + Path.Substring("AllPackages".Length);
					return File.Exists(path);
				}
				else if (Path.StartsWith("AddonPackages"))
                {
					return File.Exists(Path);
                }
			}
			return false;
		}
		public override bool IsFavorite()
		{

			string key = null;
            if (isVar)
				key = System.IO.Path.GetFileNameWithoutExtension(Path);
            else
				key = Path;
			if (FavoriteLookup.Contains(key))
				return true;
			return false;
		}
		public override void SetFavorite(bool b)
		{
			string key = null;
			if (isVar)
				key = System.IO.Path.GetFileNameWithoutExtension(Path);
			else
				key = Path;
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
			foreach (var item in FavoriteLookup)
			{
				list.Add(item);
			}
			sf.FavoriteNames = list.ToArray();
			File.WriteAllText(GlobalInfo.FavoritePath, JsonUtility.ToJson(sf));
		}
		public override bool IsAutoInstall()
		{
            string key = System.IO.Path.GetFileNameWithoutExtension(Path);

            if (AutoInstallLookup.Contains(key))
                return true;
            return false;
        }
		public override bool SetAutoInstall(bool b)
        {
			LogUtil.Log("SetAutoInstall " + b+" "+Path);
			if (isVar)
            {
				string key = System.IO.Path.GetFileNameWithoutExtension(Path);
				SetAutoInstallInternal(key, b);
                if (b)
                {
					bool dirty=Install();
					return dirty;
				}
			}
			return false;
		}

        public bool Install()
        {
            if (isVar)
            {
				string installPath = null;
				string repoPath = null;
				if (Path.StartsWith("AddonPackages/"))
				{
					installPath = Path;
					repoPath = "AllPackages" + Path.Substring("AddonPackages".Length);
				}
				else if (Path.StartsWith("AllPackages/"))
				{
					installPath = "AddonPackages" + Path.Substring("AllPackages".Length);
					repoPath = Path;
				}
				//卸载
				if (File.Exists(repoPath))
				{
					if (!File.Exists(installPath))
					{
						string dir = System.IO.Path.GetDirectoryName(installPath);
						if (!Directory.Exists(dir))
							Directory.CreateDirectory(dir);

						File.Move(repoPath, installPath);
						return true;
					}
					else
					{
						LogUtil.Log(installPath + " uninstall failed because there is a file with same name in AllPackages");
					}
				}
			}
            else
            {
				//就是var包了
            }
			return false;
        }
		public bool Uninstall()
        {
			if (isVar)
			{
				string installPath = null;
				string repoPath = null;
                if (Path.StartsWith("AddonPackages/"))
                {
					installPath = Path;
					repoPath = "AllPackages" + Path.Substring("AddonPackages".Length);
				}
                else if(Path.StartsWith("AllPackages/"))
                {
					installPath = "AddonPackages" + Path.Substring("AllPackages".Length);
					repoPath = Path;
				}

                //卸载
                if (File.Exists(installPath))
                {
                    if (!File.Exists(repoPath))
                    {
                        string dir = System.IO.Path.GetDirectoryName(repoPath);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        File.Move(installPath, repoPath);
						return true;
                    }
                    else
                    {
						LogUtil.Log(installPath + " uninstall failed because there is a file with same name in AllPackages");
                    }
                }
            }
				return false;
		}
	}

}
