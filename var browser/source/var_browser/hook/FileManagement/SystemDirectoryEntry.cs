//using System;
//using System.Collections.Generic;
//using System.IO;


//namespace var_browser
//{
//	public class SystemDirectoryEntry : DirectoryEntry
//	{
//		public string FixedPath { get; protected set; }

//		public override DirectoryEntry Parent
//		{
//			get
//			{
//				DirectoryInfo parent = Directory.GetParent(FixedPath);
//				return new SystemDirectoryEntry(parent.FullName);
//			}
//			protected set
//			{
//				throw new NotImplementedException();
//			}
//		}

//		public override List<FileEntry> Files
//		{
//			get
//			{
//				List<FileEntry> list = new List<FileEntry>();
//				string[] directories = Directory.GetDirectories(FixedPath);
//				string[] array = directories;
//				foreach (string text in array)
//				{
//					if (FileManager.IsPackage(text))
//					{
//						SystemFileEntry item = new SystemFileEntry(text);
//						list.Add(item);
//					}
//				}
//				string[] files = Directory.GetFiles(FixedPath);
//				string[] array2 = files;
//				foreach (string path in array2)
//				{
//					SystemFileEntry item2 = new SystemFileEntry(path);
//					list.Add(item2);
//				}
//				return list;
//			}
//			protected set
//			{
//				throw new NotImplementedException();
//			}
//		}

//		public override List<DirectoryEntry> SubDirectories
//		{
//			get
//			{
//				List<DirectoryEntry> list = new List<DirectoryEntry>();
//				//string[] directories = Directory.GetDirectories(FixedPath);
//				//string[] array = directories;
//				//foreach (string text in array)
//				//{
//				//	VarPackage package = FileManager.GetPackage(text);
//				//	if (package != null)
//				//	{
//				//		list.Add(package.RootDirectory);
//				//		continue;
//				//	}
//				//	SystemDirectoryEntry item = new SystemDirectoryEntry(text);
//				//	list.Add(item);
//				//}
//				//string[] files = Directory.GetFiles(FixedPath);
//				//string[] array2 = files;
//				//foreach (string path in array2)
//				//{
//				//	VarDirectoryEntry varRootDirectoryEntryFromPath = FileManager.GetVarRootDirectoryEntryFromPath(path);
//				//	if (varRootDirectoryEntryFromPath != null)
//				//	{
//				//		list.Add(varRootDirectoryEntryFromPath);
//				//	}
//				//}
//				return list;
//			}
//			protected set
//			{
//				throw new NotImplementedException();
//			}
//		}

//		public SystemDirectoryEntry(string path)
//			: base(path)
//		{
//			if (Path.EndsWith(":"))
//			{
//				FixedPath = Path + "/";
//			}
//			else
//			{
//				FixedPath = Path;
//				//hidePath = Path + ".hide";
//			}
//			if (!Directory.Exists(FixedPath))
//			{
//				throw new Exception("Directory " + Path + " does not exist");
//			}
//			//FullPath = System.IO.Path.GetFullPath(FixedPath).Replace('\\', '/');
//			LastWriteTime = Directory.GetLastWriteTime(FixedPath);
//		}
//	}

//}
