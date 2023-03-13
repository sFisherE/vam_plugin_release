using System;
using System.IO;

namespace var_browser
{
	public abstract class FileEntryStream : IDisposable
	{
		public Stream Stream { get; protected set; }

		public FileEntryStream(FileEntry fe)
		{
		}

		public virtual void Dispose()
		{
			if (Stream != null)
			{
				Stream.Dispose();
				Stream = null;
			}
		}
	}

}
