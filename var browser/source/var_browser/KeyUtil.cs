using System.Collections.Generic;
using UnityEngine;

namespace var_browser
{
	public class KeyUtil
	{
		private List<KeyCode> supportKeys = new List<KeyCode>();

		private KeyCode key;
		public string keyPattern;

		public static KeyUtil Parse(string keyPattern)
		{
			string[] array = keyPattern.Split('+');
			List<KeyCode> list = new List<KeyCode>();
			//string text;
			KeyCode code=KeyCode.Home;
			if (array.Length == 1)
			{
				string text = array[0];
				code = (KeyCode)System.Enum.Parse(typeof(KeyCode), array[0], true);
			}
			else
			{
				for (int i = 0; i < array.Length - 1; i++)
				{
					string a = array[i].ToLower();
					if (a == "ctrl")
					{
						list.Add(KeyCode.LeftControl);
						list.Add(KeyCode.RightControl);
					}
					else if (a == "shift")
					{
						list.Add(KeyCode.LeftShift);
						list.Add(KeyCode.RightShift);
					}
					else if (a == "alt")
					{
						list.Add(KeyCode.LeftAlt);
						list.Add(KeyCode.RightAlt);
					}
                    else
                    {
						list.Add((KeyCode)System.Enum.Parse(typeof(KeyCode), array[i], true));
					}
				}
				string text = array[array.Length - 1];
				code = (KeyCode)System.Enum.Parse(typeof(KeyCode), text,true);
			}
			return new KeyUtil
			{
				supportKeys = list,
				key = code,
				keyPattern = keyPattern
		};
		}

		public bool TestKeyUp()
		{
			if (Input.GetKeyUp(key))
			{
				return TestSupports();
			}
			return false;
		}

		public bool TestKeyDown()
		{
			if (Input.GetKeyDown(key))
			{
				return TestSupports();
			}
			return false;
		}

		public bool TestSupports()
		{
			for (int i = 0; i < supportKeys.Count; i += 2)
			{
				if (!Input.GetKey(supportKeys[i]) && !Input.GetKey(supportKeys[i + 1]))
				{
					return false;
				}
			}
			return true;
		}
	}
}
