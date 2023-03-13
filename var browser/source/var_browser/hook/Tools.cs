using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace var_browser
{
    class Tools
    {

		public static string GetGameObjectPath(GameObject obj)
		{
			string path = "/" + obj.name;
			while (obj.transform.parent != null)
			{
				obj = obj.transform.parent.gameObject;
				path = "/" + obj.name + path;
			}
			return path;
		}


		/// <summary>
		/// Add a new child game object.
		/// </summary>

		static public GameObject AddChild(GameObject parent) { return AddChild(parent, true); }

		/// <summary>
		/// Add a new child game object.
		/// </summary>

		static public GameObject AddChild(GameObject parent, bool undo)
		{
			GameObject go = new GameObject();
#if UNITY_EDITOR
		if (undo) UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
			if (parent != null)
			{
				Transform t = go.transform;
				t.parent = parent.transform;
				t.localPosition = Vector3.zero;
				t.localRotation = Quaternion.identity;
				t.localScale = Vector3.one;
				go.layer = parent.layer;
			}
			return go;
		}

		/// <summary>
		/// Instantiate an object and add it to the specified parent.
		/// </summary>

		static public GameObject AddChild(GameObject parent, GameObject prefab)
		{
			GameObject go = GameObject.Instantiate(prefab) as GameObject;
#if UNITY_EDITOR
		UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
			if (go != null && parent != null)
			{
				Transform t = go.transform;
				t.parent = parent.transform;
				t.localPosition = Vector3.zero;
				t.localRotation = Quaternion.identity;
				t.localScale = Vector3.one;
				go.layer = parent.layer;
			}
			return go;
		}
		public static Queue<Transform> cachedQueue = new Queue<Transform>();
		public static Transform GetChild(Transform parent, string target)
		{
			cachedQueue.Clear();
			cachedQueue.Enqueue(parent);
			return GetChildBFS(cachedQueue, target, 0);
		}

		public static UnityEngine.Object GetChildWithType(Transform obj, string path, Type type)
		{
			var tar = obj.transform.Find(path);
			if (tar != null)
			{
				if (type == typeof(GameObject))
					return tar.gameObject;
				else if (type.IsSubclassOf(typeof(Component)))
					return tar.GetComponent(type);
			}
			return null;
		}

		private static Transform GetChildBFS(Queue<Transform> queue, string target, int generation)
		{
			int parents = queue.Count;
			for (var i = 0; i < parents; i++)
			{
				Transform parent = queue.Dequeue();
				if (parent.name == target)
					return parent;
				for (int j = 0; j < parent.childCount; j++)
				{
					queue.Enqueue(parent.GetChild(j));
				}
			}
			if (queue.Count == 0)
			{
				return null;
			}
			return GetChildBFS(queue, target, generation++);
		}

	}
}
