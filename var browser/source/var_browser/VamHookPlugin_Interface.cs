using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace var_browser
{
    public partial class VamHookPlugin
    {
        void OpenFileBrowser(string msg)
        {
            LogUtil.Log("receive OpenFileBrowser "+ msg);
        }

        public void Refresh()
        {
            FileManager.Refresh(true);
            MVR.FileManagement.FileManager.Refresh();
            RemoveEmptyFolder("AllPackages");
        }
        public void RemoveInvalidVars()
        {
            FileManager.Refresh(true, true);
            MVR.FileManagement.FileManager.Refresh();
        }
        public void RemoveOldVersion()
        {
            FileManager.Refresh(true, true, true);
            MVR.FileManagement.FileManager.Refresh();
        }
        //https://stackoverflow.com/questions/2811509/c-sharp-remove-all-empty-subdirectories
        private static void RemoveEmptyFolder(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                RemoveEmptyFolder(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }
        public void UninstallAll()
        {
            string[] addonVarPaths = Directory.GetFiles("AddonPackages", "*.var", SearchOption.AllDirectories);
            foreach (var item in addonVarPaths)
            {
                string name = Path.GetFileNameWithoutExtension(item);
                if (FileEntry.AutoInstallLookup.Contains(name)) continue;
                if (item.StartsWith("AddonPackages"))
                {
                    string targetPath = "AllPackages" + item.Substring("AddonPackages".Length);
                    string dir = Path.GetDirectoryName(targetPath);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    if (File.Exists(targetPath)) continue;
                    File.Move(item, targetPath);
                }
            }
            MVR.FileManagement.FileManager.Refresh();
            var_browser.FileManager.Refresh();
            RemoveEmptyFolder("AddonPackages");
        }
        public void OpenHubBrowse()
        {
            SuperController.singleton.ActivateWorldUI();
            m_HubBrowse.Show();
        }
        public void OpenCustomScene()
        {
            //自定义的不需要安装
            m_FileBrowser.onlyInstalled = false;
            ShowFileBrowser("Custom Scene", "json", "Saves/scene", true);
        }
        public void OpenCustomSavedPerson()
        {
            //自定义的不需要安装
            m_FileBrowser.onlyInstalled = false;
            ShowFileBrowser("Custom Saved Person", "json", "Saves/Person", true);
        }
        public void OpenPersonPreset()
        {
            //自定义的不需要安装
            m_FileBrowser.onlyInstalled = false;
            ShowFileBrowser("Custom Person Preset", "vap", "Custom/Atom/Person", true, false);
        }
        public void OpenCategoryScene()
        {
            ShowFileBrowser("Category Scene", "json", "Saves/scene");
        }
        public void OpenCategoryClothing()
        {
            ShowFileBrowser("Category Clothing", "vam", "Custom/Clothing", false, false);
        }
        public void OpenCategoryHair()
        {
            ShowFileBrowser("Category Hair", "vam", "Custom/Hair", false, false);
        }
        public void OpenCategoryPose()
        {
            ShowFileBrowser("Category Pose", "json|vap", "Custom/Atom/Person/Pose", false, false);
        }
        public void OpenPresetPerson()
        {
            ShowFileBrowser("Preset Person", "vap", "Custom/Atom/Person", false, false);
        }
        public void OpenPresetClothing()
        {
            ShowFileBrowser("Preset Clothing", "vap", "Custom/Clothing", false, false);
        }
        public void OpenPresetHair()
        {
            ShowFileBrowser("Preset Hair", "vap", "Custom/Hair", false, false);
        }
        public void OpenPresetOther()
        {
            ShowFileBrowser("Preset Other", "vap", "Custom", false, false);
        }
        public void OpenMiscCUA()
        {
            ShowFileBrowser("AssetBundle", "assetbundle", "Custom", false, false);
        }
        public void OpenMiscAll()
        {
            ShowFileBrowser("All", "var", "", false, false);
        }
    }
}
