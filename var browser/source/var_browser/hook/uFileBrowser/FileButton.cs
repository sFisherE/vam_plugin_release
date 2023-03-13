using Prime31.MessageKit;
using System.Runtime.InteropServices;
//using MVR.FileManagement;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SimpleJSON;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace var_browser
{
    public class FileButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
    {
        public void InitUI(uFileBrowser.FileButton ui)
        {
            this.button=ui.button;
            this.button.onClick.RemoveAllListeners();
            this.buttonImage=ui.buttonImage;
            this.fileIcon=ui.fileIcon;
            this.altIcon=ui.altIcon;
            this.label=ui.label;
            this.selectedSprite = ui.selectedSprite;
            this.renameButton=ui.renameButton;
            this.deleteButton= ui.deleteButton;
            this.favoriteToggle=ui.favoriteToggle;
            this.hiddenToggle=ui.hiddenToggle;
            this.useFileAsTemplateToggle = ui.useFileAsTemplateToggle;
            this.fullPathLabel=ui.fullPathLabel;
            this.rectTransform=ui.rectTransform;
        }

        public Button button;
        public Image buttonImage;
        public Image fileIcon;
        public RawImage altIcon;
        public Text label;
        public Sprite selectedSprite;
        public Button renameButton;
        public Button deleteButton;
        public Toggle favoriteToggle;
        public Toggle hiddenToggle;
        public Toggle useFileAsTemplateToggle;
        public Text fullPathLabel;
        public RectTransform rectTransform;

        //[HideInInspector]
        public string text;

        //[HideInInspector]
        public string textLowerInvariant;

        //[HideInInspector]
        public string fullPath;

        //[HideInInspector]
        public string removedPrefix;

        //[HideInInspector]
        public bool isDir;

        //[HideInInspector]
        public string imgPath;

        private FileBrowser browser;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if ((bool)browser)
            {
                browser.OnFilePointerEnter(this);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if ((bool)browser)
            {
                browser.OnFilePointerExit(this);
            }
        }

        public void Select()
        {
            button.transition = Selectable.Transition.None;
            buttonImage.overrideSprite = selectedSprite;
        }

        public void Unselect()
        {
            button.transition = Selectable.Transition.SpriteSwap;
            buttonImage.overrideSprite = null;
        }

        void InstallInBackground()
        {
            LogUtil.Log("InstallInBackground "+fullPath);
            if (browser != null)
            {
                if (browser.inGame)
                {
                    EnsureInstalled();
                }
                else
                {
                    try
                    {
                        //有些var包里的dep不是全的，所有需要确保安装
                        if (fullPath.EndsWith(".json"))
                        {
                            using (FileEntryStream fileEntryStream = FileManager.OpenStream(fullPath))
                            {
                                using (StreamReader streamReader = new StreamReader(fileEntryStream.Stream))
                                {
                                    string aJSON = streamReader.ReadToEnd();
                                    bool dirty = EnsureInstalledByText(aJSON);
                                    if (dirty)
                                    {
                                        MVR.FileManagement.FileManager.Refresh();
                                        var_browser.FileManager.Refresh();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogUtil.Log(e.ToString());
                    }
                    OnInstalled(true);
                }
            }
        }
        public void OnClick()
        {
            LogUtil.Log("OnClick "+this.fullPath);
            if (browser!=null)
            {
                if (browser.inGame)
                {
                    EnsureInstalled();
                    browser.OnFileClick(this);
                }
                else
                {
                    try
                    {
                        if (fullPath.EndsWith(".json"))
                        {
                            using (FileEntryStream fileEntryStream = FileManager.OpenStream(fullPath))
                            {
                                using (StreamReader streamReader = new StreamReader(fileEntryStream.Stream))
                                {
                                    string aJSON = streamReader.ReadToEnd();
                                    bool dirty = EnsureInstalledByText(aJSON);
                                    if (dirty)
                                    {
                                        MVR.FileManagement.FileManager.Refresh();
                                        var_browser.FileManager.Refresh();
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        LogUtil.Log(e.ToString());
                    }
                    

                    OnInstalled(true);
                    browser.OnFileClick(this);
                }
            }
        }

        public void OnDeleteClick()
        {
            if ((bool)browser)
            {
                browser.OnDeleteClick(this);
            }
        }

        public void OnHiddenChange(bool b)
        {
            if ((bool)browser)
            {
                browser.OnHiddenChange(this, b);
            }
        }

        public void OnFavoriteChange(bool b)
        {
            if ((bool)browser)
            {
                browser.OnFavoriteChange(this, b);
                RefreshInstallStatus();
            }
        }
        public void OnSetAutoInstall(bool b)
        {
            bool flag = false;
            FileEntry fileEntry = var_browser.FileManager.GetFileEntry(fullPath, true);
            if (fileEntry != null && (fileEntry is VarFileEntry))
            {
                bool dirty=fileEntry.SetAutoInstall(b);
                if (dirty) flag = true;
            }
            else if (fileEntry != null && (fileEntry is SystemFileEntry))
            {
                bool dirty = fileEntry.SetAutoInstall(b);
                if (dirty) flag = true;
            }
            if (flag)
            {
                MVR.FileManagement.FileManager.Refresh();
                //刷新自己，这里会发事件出来
                var_browser.FileManager.Refresh();
            }
        }
        public void EnsureInstalled()
        {
            LogUtil.Log("EnsureInstalled " + fullPath);
            string text = File.ReadAllText(fullPath);
            EnsureInstalledInternal(text);
        }
        public static bool EnsureInstalledByText(string text)
        {
            Regex varInVapRegex = new Regex(@"""([^\\\/\:\*\?\""\<\>\.]+)\.([^\\\/\:\*\?\""\<\>\.]+)\.(\w+):");
            var ms = varInVapRegex.Matches(text);
            HashSet<string> set = new HashSet<string>();

            foreach (Match item in ms)
            {
                set.Add(string.Format("{0}.{1}.{2}", item.Groups[1], item.Groups[2], item.Groups[3]));
            }
            bool flag = false;
            foreach (var key in set)
            {
                LogUtil.Log("Try Install " + key);
                VarPackage package = FileManager.GetPackage(key);
                if (package != null)
                {
                    bool dirty=package.InstallRecursive();
                    if (dirty) flag = true;
                }
                else
                {
                    //如果是因为查某个具体version的var包失败的话，尝试安装最新包
                    if (!key.EndsWith(".latest"))
                    {
                        string newKey = key.Substring(0, key.LastIndexOf('.'))+ ".latest";
                        LogUtil.LogWarning("install try latest version:" + newKey);
                        VarPackage packageNewest = FileManager.GetPackage(newKey);
                        if (packageNewest != null)
                        {
                            bool dirty = packageNewest.InstallRecursive();
                            if (dirty) flag = true;
                        }
                    }
                }
            }
            if (flag)
                return true;
            return false;
        }

        public static void EnsureInstalledInternal(string text)
        {
            LogUtil.Log("FileButton.EnsureInstalledInternal");

            bool dirty=EnsureInstalledByText(text);
            if (dirty)
            {
                MVR.FileManagement.FileManager.Refresh();
                var_browser.FileManager.Refresh();
            }
        }

        public void OnInstalled(bool b)
        {
            LogUtil.Log("OnInstalled " + b+" "+ fullPath);
            bool flag = false;
            FileEntry fileEntry = FileManager.GetFileEntry(fullPath, true);//不带AllPackages路径的
            if (fileEntry != null && (fileEntry is VarFileEntry))
            {
                var entry = fileEntry as VarFileEntry;
                //卸载
                if (!b)
                {
                    bool dirty=entry.Package.UninstallSelf();
                    if (dirty) flag = true;
                }
                else
                {
                    bool dirty = entry.Package.InstallRecursive();
                    if (dirty) flag = true;
                }
            }
            else if (fileEntry != null && (fileEntry is SystemFileEntry))
            {
                SystemFileEntry entry = fileEntry as SystemFileEntry;
                LogUtil.Log("OnInstalled " + entry.Path+" isVar:"+ entry.isVar);
                if (entry.isVar)
                {
                    if (!b)
                    {
                        bool dirty = entry.Uninstall();
                    if (dirty) flag = true;
                    }

                    else
                    {
                        bool dirty = entry.Install();
                        if (dirty) flag = true;

                    }
                }
                else
                {
                    LogUtil.Log("impossible filebutton OnInstalled");
                }
            }
            if (flag)
            {
                MVR.FileManagement.FileManager.Refresh();
                //刷新自己，这里会发事件出来
                var_browser.FileManager.Refresh();
            }
        }


        void OnEnable()
        {
            MessageKit.addObserver(MessageDef.FileManagerRefresh, OnFileManagerRefresh);
        }
        void OnDisable()
        {
            MessageKit.removeObserver(MessageDef.FileManagerRefresh, OnFileManagerRefresh);
        }
        void OnFileManagerRefresh()
        {
            RefreshInstallStatus();
        }
        public void Set(FileBrowser b, string txt, string path, bool dir, bool hidden, bool hiddenModifiable, bool favorite,bool isAutoInstall, bool allowUseFileAsTemplateSelect, bool isTemplate, bool isTemplateModifiable)
        {
            altIcon.texture = null;
            rectTransform = GetComponent<RectTransform>();
            browser = b;
            text = txt;
            textLowerInvariant = txt.ToLowerInvariant();
            fullPath = path;//这个路径是uid:子路径，这种格式
            isDir = dir;
            label.text = text;

            this.button.onClick.RemoveAllListeners();
            this.button.onClick.AddListener(OnClick);

            if (fullPathLabel != null)
            {
                fullPathLabel.text = fullPath;
            }
            if (isDir)
            {
                fileIcon.sprite = b.folderIcon;
            }
            else
            {
                fileIcon.sprite = b.GetFileIcon(txt);
            }

            if (hiddenToggle != null)
            {
                hiddenToggle.isOn = hidden;
                if (hiddenModifiable)
                {
                    hiddenToggle.interactable = true;
                    hiddenToggle.onValueChanged.RemoveAllListeners();
                    hiddenToggle.onValueChanged.AddListener(OnHiddenChange);
                }
                else
                {
                    hiddenToggle.interactable = false;
                }
            }
            //deleteButton.transform.Find("Text").GetComponent<Text>().text = "Install In Background";
            renameButton.transform.Find("Text").GetComponent<Text>().text = "Install In Background";
            var rt = renameButton.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(250,40);
            rt.anchoredPosition = new Vector2(-130, 130);

            useFileAsTemplateToggle.transform.Find("Label").GetComponent<Text>().text = "Auto Install";

            this.buttonImage.color = Color.white;
            if (browser.inGame)
            {
                deleteButton.gameObject.SetActive(false);
                //deleteButton.onClick.RemoveAllListeners();
                //deleteButton.onClick.AddListener(EnsureInstalled);

                renameButton.gameObject.SetActive(true);
                renameButton.onClick.RemoveAllListeners();
                renameButton.onClick.AddListener(InstallInBackground);

                hiddenToggle.gameObject.SetActive(false);

                favoriteToggle.gameObject.SetActive(false);

                useFileAsTemplateToggle.gameObject.SetActive(false);
            }
            else
            {
                deleteButton.gameObject.SetActive(false);//ensureInstallButton
                //deleteButton.onClick.RemoveAllListeners();
                //deleteButton.onClick.AddListener(EnsureInstalled);

                renameButton.gameObject.SetActive(true);
                renameButton.onClick.RemoveAllListeners();
                renameButton.onClick.AddListener(InstallInBackground);

                hiddenToggle.gameObject.SetActive(false);
                //是否喜爱
                favoriteToggle.gameObject.SetActive(true);
                favoriteToggle.onValueChanged.RemoveAllListeners();
                favoriteToggle.isOn = favorite;
                favoriteToggle.onValueChanged.AddListener(OnFavoriteChange);
                //安装
                useFileAsTemplateToggle.gameObject.SetActive(true);
                useFileAsTemplateToggle.onValueChanged.RemoveAllListeners();
                useFileAsTemplateToggle.isOn = isAutoInstall;
                useFileAsTemplateToggle.onValueChanged.AddListener(OnSetAutoInstall);

                RefreshInstallStatus();

            }
        }
        void RefreshInstallStatus()
        {
            useFileAsTemplateToggle.onValueChanged.RemoveAllListeners();
                favoriteToggle.onValueChanged.RemoveAllListeners();
            bool isInstalled = false;
            bool isAutoInstall = false;
            bool isFavorite = false;
            FileEntry fileEntry = var_browser.FileManager.GetFileEntry(fullPath, true);
            if (fileEntry != null && fileEntry is VarFileEntry)
            {
                isInstalled = fileEntry.IsInstalled();
                isAutoInstall = fileEntry.IsAutoInstall();
                isFavorite = fileEntry.IsFavorite();
            }
            else if (fileEntry != null && fileEntry is SystemFileEntry)
            {
                isInstalled = fileEntry.IsInstalled();
                isAutoInstall = fileEntry.IsAutoInstall();
                isFavorite = fileEntry.IsFavorite();
            }
            else
            {
                //插件安装之后，路径变化了
                if (fullPath.StartsWith("AllPackages"))
                {
                    fullPath = "AddonPackages" + fullPath.Substring("AllPackages".Length);
                }
                else if (fullPath.StartsWith("AddonPackages"))
                {
                    fullPath = "AllPackages" + fullPath.Substring("AddonPackages".Length);
                }
                fileEntry = var_browser.FileManager.GetFileEntry(fullPath, true);
                if (fileEntry != null)
                {
                    isInstalled = fileEntry.IsInstalled();
                    isAutoInstall = fileEntry.IsAutoInstall();
                    isFavorite = fileEntry.IsFavorite();
                }
                else
                {
                    isInstalled = false;
                }
            }

            favoriteToggle.isOn = isFavorite;
            useFileAsTemplateToggle.isOn = isAutoInstall;
            favoriteToggle.onValueChanged.AddListener(OnFavoriteChange);
            useFileAsTemplateToggle.onValueChanged.AddListener(OnSetAutoInstall);

            UpdateButtonImageColor(isInstalled, isAutoInstall, isFavorite);
        }
        void UpdateButtonImageColor(bool isInstalled, bool isAutoInstall,bool isFavorite)
        {
            if (isAutoInstall)
            {
                this.buttonImage.color = new Color32(255, 150, 0, 255);
                return;
            }
            if (isInstalled)
            {
                this.buttonImage.color = new Color32(120, 220, 255, 255);
                return;
            }
            if (isFavorite)
            {
                this.buttonImage.color = new Color32(255, 125, 175, 255);
                return;
            }
            this.buttonImage.color = Color.white;
        }
    }
}
