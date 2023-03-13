using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
//using MVR.FileManagement;
//using MVR.FileManagementSecure;
using UnityEngine;
using UnityEngine.UI;

namespace var_browser
{
    public partial class FileBrowser : MonoBehaviour
    {
		//public UIDynamicPopup CreateFilterablePopup(JSONStorableStringChooser jsc, int yOffset)
		//{
		//	UIDynamicPopup uIDynamicPopup = null;

		//	var manager = SuperController.singleton.transform.Find("ScenePluginManager").GetComponent<MVRPluginManager>();

		//	if (manager != null && manager.configurableFilterablePopupPrefab != null && jsc.popup == null)
		//	{
		//		Transform transform = CreateUIElement(manager.configurableFilterablePopupPrefab.transform, yOffset);
		//		if (transform != null)
		//		{
		//			uIDynamicPopup = transform.GetComponent<UIDynamicPopup>();
		//			if (uIDynamicPopup != null)
		//			{
		//				uIDynamicPopup.label = jsc.name;
		//				jsc.popup = uIDynamicPopup.popup;
		//			}
		//		}
		//	}
		//	return uIDynamicPopup;
		//}
        protected RectTransform CreateUIContainer(int xOffset, int yOffset, int width, int height)
        {
			var manager = SuperController.singleton.transform.Find("ScenePluginManager").GetComponent<MVRPluginManager>();
			if (manager != null && manager.configurableScrollablePopupPrefab != null)
            {
				RectTransform backgroundTransform = manager.configurableScrollablePopupPrefab.transform.Find("Background") as RectTransform;
				RectTransform rectTransform = UnityEngine.Object.Instantiate(backgroundTransform, this.window.transform);

				//RectTransform rectTransform = transform.GetComponent<RectTransform>();
				rectTransform.localRotation = Quaternion.identity;
				rectTransform.localPosition = new Vector3(-1725, 850, 0);
				rectTransform.anchorMin = new Vector2(0, 1);
				rectTransform.anchorMax = new Vector2(0, 1);
				rectTransform.pivot = new Vector2(0, 1);
				//yOffset往负值增长
				rectTransform.anchoredPosition = new Vector2(xOffset, yOffset);
				rectTransform.sizeDelta = new Vector2(width, height);//大小
				rectTransform.localScale = Vector3.one;

				var layout=	rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
				layout.padding = new RectOffset(2, 2, 2, 2);
				layout.spacing = 2;
				layout.childAlignment = TextAnchor.UpperLeft;
				layout.childControlHeight = true;
				layout.childControlWidth = true;
				layout.childForceExpandHeight = false;
				layout.childForceExpandWidth = false;
				return rectTransform;
			}
			return null;
        }

		RectTransform CreateLabel(RectTransform parent, string v,Color color,bool bold=false)
        {
			var manager = SuperController.singleton.transform.Find("ScenePluginManager").GetComponent<MVRPluginManager>();
			if (manager != null && manager.configurableSpacerPrefab != null)
			{
				RectTransform transform = UnityEngine.Object.Instantiate(manager.configurableSpacerPrefab, parent) as RectTransform;
				transform.gameObject.SetActive(true);

                UIDynamic header = transform.GetComponent<UIDynamic>();
                header.height = 40;
                var text = header.gameObject.AddComponent<Text>();
                text.text = v;
                text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
                text.fontSize = 30;
                text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
                text.color = color;
                return transform;
            }
			return null;
		}

		UIDynamicPopup CreateFilterablePopup(RectTransform parent, JSONStorableStringChooser jsc)
        {
			var manager = SuperController.singleton.transform.Find("ScenePluginManager").GetComponent<MVRPluginManager>();
			if (manager != null && manager.configurableFilterablePopupPrefab != null)
			{
				RectTransform transform = UnityEngine.Object.Instantiate(manager.configurableFilterablePopupPrefab, parent) as RectTransform;
				transform.gameObject.SetActive(true);
				var uIDynamicPopup = transform.GetComponent<UIDynamicPopup>();
				if (uIDynamicPopup != null)
				{
					uIDynamicPopup.label = jsc.name;
					jsc.popup = uIDynamicPopup.popup;
				}

				//var uIDynamicToggle = transform.GetComponent<UIDynamicToggle>();
				//if (uIDynamicToggle != null)
				//{
				//	//toggleToJSONStorableBool.Add(uIDynamicToggle, jsb);
				//	uIDynamicToggle.label = jsb.name;
				//	jsb.toggle = uIDynamicToggle.toggle;
				//}
				return uIDynamicPopup;
			}
			return null;
		}

		UIDynamicToggle CreateToggle(RectTransform parent, JSONStorableBool jsb)
        {
			var manager = SuperController.singleton.transform.Find("ScenePluginManager").GetComponent<MVRPluginManager>();
			if (manager != null && manager.configurableTogglePrefab != null)
            {
				RectTransform transform = UnityEngine.Object.Instantiate(manager.configurableTogglePrefab, parent) as RectTransform;
				transform.gameObject.SetActive(true);
				var uIDynamicToggle = transform.GetComponent<UIDynamicToggle>();
				if (uIDynamicToggle != null)
				{
					//toggleToJSONStorableBool.Add(uIDynamicToggle, jsb);
					uIDynamicToggle.label = jsb.name;
					jsb.toggle = uIDynamicToggle.toggle;
				}
				return uIDynamicToggle;
			}
			return null;
		}


        protected Transform CreateUIElement(Transform prefab, int yOffset)
		{
			Transform transform = null;
			if (prefab != null)
			{
				transform = UnityEngine.Object.Instantiate(prefab);
				transform.SetParent(this.window.transform);
				transform.gameObject.SetActive(true);

				RectTransform rectTransform = transform.GetComponent<RectTransform>();
				rectTransform.localRotation = Quaternion.identity;
				rectTransform.localPosition = new Vector3(-1725, 850, 0);
				rectTransform.anchorMin = new Vector2(0, 1);
				rectTransform.anchorMax = new Vector2(0, 1);
				rectTransform.pivot = new Vector2(0, 1);
				//yOffset往负值增长
				rectTransform.anchoredPosition = new Vector2(-500, yOffset);
				rectTransform.sizeDelta = new Vector2(500, 120);
				rectTransform.localScale = Vector3.one;
			}
			return transform;
		}

		public UIDynamicButton CreateRightButton(string label, int yOffset)
		{
			UIDynamicButton uIDynamicButton = null;
			var manager = SuperController.singleton.transform.Find("ScenePluginManager").GetComponent<MVRPluginManager>();
			if (manager != null && manager.configurableButtonPrefab != null)
			{
				Transform transform = CreateRightUIElement(manager.configurableButtonPrefab.transform, yOffset);
				if (transform != null)
				{
					uIDynamicButton = transform.GetComponent<UIDynamicButton>();
					if (uIDynamicButton != null)
					{
						uIDynamicButton.label = label;
					}
				}
			}
			return uIDynamicButton;
		}
		protected Transform CreateRightUIElement(Transform prefab, int yOffset)
		{
			Transform transform = null;
			if (prefab != null)
			{
				transform = UnityEngine.Object.Instantiate(prefab);
				transform.SetParent(this.window.transform);
				transform.gameObject.SetActive(true);

				RectTransform rectTransform = transform.GetComponent<RectTransform>();
				rectTransform.localRotation = Quaternion.identity;
				rectTransform.localPosition = new Vector3(1225, 850, 0);
				rectTransform.anchorMin = new Vector2(1, 1);
				rectTransform.anchorMax = new Vector2(1, 1);
				rectTransform.pivot = new Vector2(0, 1);
				//yOffset往负值增长
				rectTransform.anchoredPosition = new Vector2(0, yOffset);
				rectTransform.sizeDelta = new Vector2(200, 50);
				rectTransform.localScale = Vector3.one;

			}
			return transform;
		}

		public UIDynamic CreateRightSpacer(int yOffset)
		{
			UIDynamic result = null;
			var manager = SuperController.singleton.transform.Find("ScenePluginManager").GetComponent<MVRPluginManager>();
			if (manager != null && manager.configurableSpacerPrefab != null)
			{
				Transform transform = CreateRightUIElement(manager.configurableSpacerPrefab.transform, yOffset);
				if (transform != null)
				{
					result = transform.GetComponent<UIDynamic>();
				}
			}
			return result;
		}
		public UIDynamic CreateLeftSpacer(int yOffset)
		{
			UIDynamic result = null;
			var manager = SuperController.singleton.transform.Find("ScenePluginManager").GetComponent<MVRPluginManager>();
			if (manager != null && manager.configurableSpacerPrefab != null)
			{
				Transform transform = CreateRightUIElement(manager.configurableSpacerPrefab.transform, yOffset);
				LogUtil.Log("CreateLeftSpacer "+transform.localScale);
				if (transform != null)
				{
					result = transform.GetComponent<UIDynamic>();
				}
			}
			return result;
		}
		void CreateRightHeader(string v, int yOffset, Color color)
		{
			var header = CreateRightSpacer(yOffset);
			header.height = 40;
			var text = header.gameObject.AddComponent<Text>();
			text.text = v;
			text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
			text.fontSize = 30;
			text.fontStyle = FontStyle.Bold;
			text.color = color;
		}
		void CreateLeftHeader(string v, int yOffset, Color color)
		{
			var header = CreateLeftSpacer(yOffset);
			header.height = 40;
			var text = header.gameObject.AddComponent<Text>();
			text.text = v;
			text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
			text.fontSize = 30;
			text.fontStyle = FontStyle.Bold;
			text.color = color;
		}


	}
}
