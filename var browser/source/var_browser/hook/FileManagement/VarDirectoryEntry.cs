using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;

namespace var_browser
{
	public class VarDirectoryEntry : DirectoryEntry
	{
		protected HashSet<VarDirectoryEntry> varSubDirectories;

		protected List<VarFileEntry> varFileEntries;

		public VarPackage Package { get; protected set; }

		public string InternalPath { get; protected set; }

		//public string InternalSlashPath { get; protected set; }

		public override List<FileEntry> Files
		{
			get
			{
				List<FileEntry> list = new List<FileEntry>();
				foreach (VarFileEntry varFileEntry in varFileEntries)
				{
					list.Add(varFileEntry);
				}
				return list;
			}
			protected set
			{
				throw new NotImplementedException();
			}
		}

		public override List<DirectoryEntry> SubDirectories
		{
			get
			{
				List<DirectoryEntry> list = new List<DirectoryEntry>();
				foreach (VarDirectoryEntry varSubDirectory in varSubDirectories)
				{
					list.Add(varSubDirectory);
				}
				return list;
			}
			protected set
			{
				throw new NotImplementedException();
			}
		}

		public List<VarDirectoryEntry> VarSubDirectories
		{
			get
			{
				return varSubDirectories.ToList();
			}
			protected set
			{
				throw new NotImplementedException();
			}
		}

		public VarDirectoryEntry(VarPackage vp, string entryName, VarDirectoryEntry parent = null)
		{
			Package = vp;
			//InternalSlashPath = entryName;
			//hidePath = "AddonPackagesFilePrefs/" + vp.Uid + "/" + InternalSlashPath + ".hide";
			bool flag = false;
			if (entryName == string.Empty)
			{
				flag = true;
				Name = vp.Uid + ".var:";
			}
			InternalPath = entryName;// InternalSlashPath.Replace("/", "\\");
			if (flag)
			{
				Uid = vp.Uid + ":";
				Path = vp.Path + ":";
				//SlashPath = Path.Replace('\\', '/');
				//FullPath = vp.FullPath + ":";
				//FullSlashPath = FullPath.Replace('\\', '/');
			}
			else
			{
				Uid = vp.Uid + ":/" + InternalPath;
				Path = vp.Path + ":/" + InternalPath;
				//SlashPath = Path.Replace('\\', '/');
				//FullPath = vp.FullPath + ":\\" + InternalPath;
				//FullSlashPath = FullPath.Replace('\\', '/');
			}
			Name = Regex.Replace(Path, ".*/", string.Empty);
			UidLowerInvariant = Uid.ToLowerInvariant();
			LastWriteTime = vp.LastWriteTime;
			Parent = parent;
			varSubDirectories = new HashSet<VarDirectoryEntry>();
			varFileEntries = new List<VarFileEntry>();
			if (FileManager.debug)
			{
				//Debug.Log("New var directory entry\n Uid: " + Uid + "\n Path: " + Path + "\n FullPath: " + FullPath + "\n SlashPath: " + SlashPath + "\n Name: " + Name + "\n InternalSlashPath: " + InternalSlashPath);
			}
		}

		public void AddSubDirectory(VarDirectoryEntry subDir)
		{
			varSubDirectories.Add(subDir);
		}

		public void AddFileEntry(VarFileEntry varFileEntry)
		{
			varFileEntries.Add(varFileEntry);
		}
	}

}
