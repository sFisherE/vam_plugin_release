using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

namespace VarBrowser {
	public class VarBrowserSessionPlugin : MVRScript {

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
				m_Messager = GameObject.Find("var_browser_messager");
				if(m_Messager==null)
					CreateHeader("var browser not ready", false, Color.red);
				else
					CreateHeader("var browser is ready", false, Color.white);

				// put init code in here
				SuperController.LogMessage("Template Loaded");

				//CreateHeader("0000/0000",false,Color.white);

				CreateButton("Refresh").button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "Refresh");
				});

				CreateButton("Remove Invalid Vars").button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "RemoveInvalidVars");
				});

				CreateButton("Uninstall All").button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "UninstallAll");
				});
				CreateButton("Hub Browse").button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenHubBrowse");
				});

				CreateHeader("Custom",true, Color.white);

				CreateBigButton("Scene", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenCustomScene");
				});

				CreateButton("Saved Person", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenCustomSavedPerson");
				});
				CreateButton("Person Preset", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenPersonPreset");
				});

				CreateHeader("Category", true, Color.white);
				CreateBigButton("Scene", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenCategoryScene");
				});
				CreateButton("Clothing", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenCategoryClothing");
				});
				CreateButton("Hair", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenCategoryHair");
				});
				CreateButton("Pose", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenCategoryPose");
				});
				//CreateHeader("Plugin", true, Color.white);
				CreateHeader("Preset", true, Color.white);
				CreateButton("Person", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenPresetPerson");
				});
				CreateButton("Clothing", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenPresetClothing");
				});
				CreateButton("Hair", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenPresetHair");
				});
				CreateButton("Other", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenPresetOther");
				});
				CreateHeader("Misc", true, Color.white);
				CreateButton("AssetBundle", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenMiscCUA");
				});
				CreateButton("All", true).button.onClick.AddListener(() =>
				{
					m_Messager.SendMessage("Invoke", "OpenMiscAll");
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