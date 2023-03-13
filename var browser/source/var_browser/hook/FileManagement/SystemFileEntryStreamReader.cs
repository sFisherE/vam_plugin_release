using System.IO;
namespace var_browser
{
	public class SystemFileEntryStreamReader : FileEntryStreamReader
	{
		public SystemFileEntryStreamReader(SystemFileEntry entry)
			: base(entry)
		{
			StreamReader = new StreamReader(entry.Path);
		}
	}
}
