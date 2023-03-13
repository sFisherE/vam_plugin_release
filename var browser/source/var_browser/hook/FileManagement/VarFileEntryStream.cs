using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace var_browser
{
	public class VarFileEntryStream : FileEntryStream
	{
		public VarFileEntryStream(VarFileEntry entry)
			: base(entry)
		{
			ZipFile zipFile = entry.Package.ZipFile;
			if (zipFile == null)
			{
				throw new Exception("Could not get ZipFile for package " + entry.Package.Uid);
			}
			ZipEntry entry2 = zipFile.GetEntry(entry.InternalPath);
			if (entry2 == null)
			{
				Dispose();
				throw new Exception("Could not find entry " + entry.InternalPath + " in zip file " + entry.Package.Path);
			}
			base.Stream = zipFile.GetInputStream(entry2);
		}

		public override void Dispose()
		{
			base.Dispose();
		}
	}

}
