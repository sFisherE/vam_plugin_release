using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

namespace VarBrowser
{
    public class VarBrowserSessionPlugin : MVRScript
    {

        // IMPORTANT - DO NOT make custom enums. The dynamic C# complier crashes Unity when it encounters these for
        // some reason

        // IMPORTANT - DO NOT OVERRIDE Awake() as it is used internally by MVRScript - instead use Init() function which
        // is called right after creation

        public GameObject m_Messager;
        void CreateHeader(string v, bool rightSide, Color color)
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
            var btn = CreateButton(label, rightSide);
            btn.height = 80;
            return btn;
        }
        public override void Init()
        {
            try
            {
                m_Messager = GameObject.Find("var_browser_messager");
                if (m_Messager == null)
                    CreateHeader("var browser not ready", false, Color.red);
                else
                    CreateHeader("var browser is ready", false, Color.white);

                CreateButton("Refresh").button.onClick.AddListener(Refresh);
                RegisterAction(new JSONStorableAction("Refresh", Refresh));
                CreateButton("Remove Invalid Vars").button.onClick.AddListener(RemoveInvalidVars);
                RegisterAction(new JSONStorableAction("RemoveInvalidVars", RemoveInvalidVars));

                CreateButton("Uninstall All").button.onClick.AddListener(UninstallAll);
                RegisterAction(new JSONStorableAction("UninstallAll", UninstallAll));

                CreateButton("Hub Browse").button.onClick.AddListener(OpenHubBrowse);
                RegisterAction(new JSONStorableAction("OpenHubBrowse", OpenHubBrowse));

                CreateHeader("Custom", true, Color.white);
                CreateBigButton("Scene", true).button.onClick.AddListener(OpenCustomScene);
                RegisterAction(new JSONStorableAction("OpenCustomScene", OpenCustomScene));
                CreateButton("Saved Person", true).button.onClick.AddListener(OpenCustomSavedPerson);
                RegisterAction(new JSONStorableAction("OpenCustomSavedPerson", OpenCustomSavedPerson));
                CreateButton("Person Preset", true).button.onClick.AddListener(OpenPersonPreset);
                RegisterAction(new JSONStorableAction("OpenPersonPreset", OpenPersonPreset));

                CreateHeader("Category", true, Color.white);
                CreateBigButton("Scene", true).button.onClick.AddListener(OpenCategoryScene);
                RegisterAction(new JSONStorableAction("OpenCategoryScene", OpenCategoryScene));
                CreateButton("Clothing", true).button.onClick.AddListener(OpenCategoryClothing);
                RegisterAction(new JSONStorableAction("OpenCategoryClothing", OpenCategoryClothing));
                CreateButton("Hair", true).button.onClick.AddListener(OpenCategoryHair);
                RegisterAction(new JSONStorableAction("OpenCategoryHair", OpenCategoryHair));
                CreateButton("Pose", true).button.onClick.AddListener(OpenCategoryPose);
                RegisterAction(new JSONStorableAction("OpenCategoryPose", OpenCategoryPose));

                //CreateHeader("Plugin", true, Color.white);
                CreateHeader("Preset", true, Color.white);
                CreateButton("Person", true).button.onClick.AddListener(OpenPresetPerson);
                RegisterAction(new JSONStorableAction("OpenPresetPerson", OpenPresetPerson));
                CreateButton("Clothing", true).button.onClick.AddListener(OpenPresetClothing);
                RegisterAction(new JSONStorableAction("OpenPresetClothing", OpenPresetClothing));
                CreateButton("Hair", true).button.onClick.AddListener(OpenPresetHair);
                RegisterAction(new JSONStorableAction("OpenPresetHair", OpenPresetHair));
                CreateButton("Other", true).button.onClick.AddListener(OpenPresetOther);
                RegisterAction(new JSONStorableAction("OpenPresetOther", OpenPresetOther));

                CreateHeader("Misc", true, Color.white);
                CreateButton("AssetBundle", true).button.onClick.AddListener(OpenMiscCUA);
                RegisterAction(new JSONStorableAction("OpenMiscCUA", OpenMiscCUA));
                CreateButton("All", true).button.onClick.AddListener(OpenMiscAll);
                RegisterAction(new JSONStorableAction("OpenMiscAll", OpenMiscAll));
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        void Refresh()
        {
            m_Messager.SendMessage("Invoke", "Refresh");
        }
        void RemoveInvalidVars()
        {
            m_Messager.SendMessage("Invoke", "RemoveInvalidVars");
        }
        void UninstallAll()
        {
            m_Messager.SendMessage("Invoke", "UninstallAll");
        }
        void OpenHubBrowse()
        {
            m_Messager.SendMessage("Invoke", "OpenHubBrowse");
        }
        void OpenCustomScene()
        {
            m_Messager.SendMessage("Invoke", "OpenCustomScene");
        }
        void OpenCustomSavedPerson()
        {
            m_Messager.SendMessage("Invoke", "OpenCustomSavedPerson");
        }
        void OpenPersonPreset()
        {
            m_Messager.SendMessage("Invoke", "OpenPersonPreset");
        }
        void OpenCategoryScene()
        {
            m_Messager.SendMessage("Invoke", "OpenCategoryScene");
        }
        void OpenCategoryClothing()
        {
            m_Messager.SendMessage("Invoke", "OpenCategoryClothing");
        }
        void OpenCategoryHair()
        {
            m_Messager.SendMessage("Invoke", "OpenCategoryHair");
        }
        void OpenCategoryPose()
        {
            m_Messager.SendMessage("Invoke", "OpenCategoryPose");
        }
        void OpenPresetPerson()
        {
            m_Messager.SendMessage("Invoke", "OpenPresetPerson");
        }
        void OpenPresetClothing()
        {
            m_Messager.SendMessage("Invoke", "OpenPresetClothing");
        }
        void OpenPresetHair()
        {
            m_Messager.SendMessage("Invoke", "OpenPresetHair");
        }
        void OpenPresetOther()
        {
            m_Messager.SendMessage("Invoke", "OpenPresetOther");
        }
        void OpenMiscCUA()
        {
            m_Messager.SendMessage("Invoke", "OpenMiscCUA");
        }
        void OpenMiscAll()
        {
            m_Messager.SendMessage("Invoke", "OpenMiscAll");
        }

        // Start is called once before Update or FixedUpdate is called and after Init()
        void Start()
        {
            try
            {
                // put code in here
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        // Update is called with each rendered frame by Unity
        void Update()
        {
            try
            {
                // put code in here
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        // FixedUpdate is called with each physics simulation frame by Unity
        void FixedUpdate()
        {
            try
            {
                // put code in here
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        // OnDestroy is where you should put any cleanup
        // if you registered objects to supercontroller or atom, you should unregister them here
        void OnDestroy()
        {
        }
    }
}