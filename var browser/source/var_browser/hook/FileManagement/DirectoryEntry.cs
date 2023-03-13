using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
namespace var_browser
{
	public abstract class DirectoryEntry
	{
		public virtual string Uid { get; protected set; }
		public virtual string UidLowerInvariant { get; protected set; }
		public virtual string Path { get; protected set; }
		public virtual string Name { get; protected set; }

		public virtual DateTime LastWriteTime { get; protected set; }

		public virtual DirectoryEntry Parent { get; protected set; }

		public abstract List<FileEntry> Files { get; protected set; }

		public abstract List<DirectoryEntry> SubDirectories { get; protected set; }

		public DirectoryEntry()
		{
		}

		public DirectoryEntry(string path)
		{
			if (path == null)
			{
				throw new Exception("Null path in DirectoryEntry constructor");
			}
			Path = path.Replace('\\', '/'); //path.Replace('/', '\\');
											//FullPath = Path;
			Uid = Path;// SlashPath;
			UidLowerInvariant = Uid.ToLowerInvariant();
			Name = Regex.Replace(Path, ".*/", string.Empty);
		}

		public override string ToString()
		{
			return Path;
		}

		public virtual List<FileEntry> GetFiles(string pattern)
		{
			if (pattern == null)
			{
				return Files;
			}
			List<FileEntry> list = new List<FileEntry>();
			string pattern2 = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
			foreach (FileEntry file in Files)
			{
				if (Regex.IsMatch(file.Name, pattern2))
				{
					list.Add(file);
				}
			}
			return list;
		}

		protected virtual DirectoryEntry FindFirstDirectoryWithFilesRecursive(DirectoryEntry startEntry)
		{
			if (startEntry.Files.Count != 0 || startEntry.SubDirectories.Count > 1)
			{
				return startEntry;
			}
			foreach (DirectoryEntry subDirectory in startEntry.SubDirectories)
			{
				DirectoryEntry directoryEntry = FindFirstDirectoryWithFilesRecursive(subDirectory);
				if (directoryEntry != null)
				{
					return directoryEntry;
				}
			}
			return null;
		}

		public virtual DirectoryEntry FindFirstDirectoryWithFiles()
		{
			return FindFirstDirectoryWithFilesRecursive(this);
		}

		public virtual bool IsHidden()
		{
			//return hidePath != null && File.Exists(hidePath);
			return false;
		}

		public virtual void SetHidden(bool b)
		{
			//if (hidePath == null)
			//{
			//	return;
			//}
			//if (File.Exists(hidePath))
			//{
			//	if (!b)
			//	{
			//		FileManager.DeleteFile(hidePath);
			//	}
			//}
			//else if (b)//隐藏
			//{
			//	string directoryName = FileManager.GetDirectoryName(hidePath);
			//	if (!FileManager.DirectoryExists(directoryName))
			//	{
			//		FileManager.CreateDirectory(directoryName);
			//	}
			//	FileManager.WriteAllText(hidePath, string.Empty);
			//}
		}
	}

}
