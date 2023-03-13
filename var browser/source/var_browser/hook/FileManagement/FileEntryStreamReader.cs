using System;
using System.IO;

namespace var_browser
{
	public abstract class FileEntryStreamReader : IDisposable
	{
		public virtual StreamReader StreamReader { get; protected set; }

		public FileEntryStreamReader(FileEntry fe)
		{
		}

		public virtual string ReadToEnd()
		{
			if (StreamReader != null)
			{
				return StreamReader.ReadToEnd();
			}
			return null;
		}

		public virtual void Dispose()
		{
			if (StreamReader != null)
			{
				StreamReader.Dispose();
				StreamReader = null;
			}
		}
	}

}
