using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

namespace VarBrowser {
	public class Mmd2TimelineAtomPlugin : MVRScript {

		// IMPORTANT - DO NOT make custom enums. The dynamic C# complier crashes Unity when it encounters these for
		// some reason

		// IMPORTANT - DO NOT OVERRIDE Awake() as it is used internally by MVRScript - instead use Init() function which
		// is called right after creation

		public GameObject m_Messager;
		void CreateHeader(string v,bool rightSide,Color color)
        {
			var header = CreateSpacer(rightSide);
			header.height = 40;
			var text = header.gameObject.AddComponent<Text>();
			text.text = v;
			text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
			text.fontSize = 30;
			text.fontStyle = FontStyle.Bold;
			text.color = color;
		}
		UIDynamicButton CreateBigButton(string label, bool rightSide = false)
        {
			var btn = CreateButton(label,rightSide);
			btn.height = 80;
			return btn;
		}
		public override void Init() {
			try {
				m_Messager = GameObject.Find("mmd2timeline_messager");
				if(m_Messager==null)
					CreateHeader("mmd2timeline not ready", false, Color.red);
				else
					CreateHeader("mmd2timeline is ready", false, Color.white);

				// put init code in here
				//SuperController.LogMessage("Template Loaded");

				//CreateHeader("0000/0000",false,Color.white);

				CreateButton("1. InitAtom").button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("InitAtom", containingAtom);
				});

				CreateButton("2. Import Vmd").button.onClick.AddListener(() =>
				{
					try
					{
						SuperController.singleton.GetMediaPathDialog(path=>
						{
							if (string.IsNullOrEmpty(path)) return;
							m_Messager.SendMessage("ImportVmd", path);
						}, "vmd", "Saves\\PluginData\\animations");
					}
					catch (Exception exc)
					{
						SuperController.LogError($"Timeline: Failed to open file dialog: {exc}");
					}
				});

				CreateButton("3. Sample").button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "Sample");
				});
				CreateButton("4. Export").button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "Export");
				});
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Start is called once before Update or FixedUpdate is called and after Init()
		void Start() {
			try {
				// put code in here
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Update is called with each rendered frame by Unity
		void Update() {
			try {
				// put code in here
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// FixedUpdate is called with each physics simulation frame by Unity
		void FixedUpdate() {
			try {
				// put code in here
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// OnDestroy is where you should put any cleanup
		// if you registered objects to supercontroller or atom, you should unregister them here
		void OnDestroy() {
		}
	}
}