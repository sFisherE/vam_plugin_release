using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using SimpleJSON;
using UnityEngine.UI;
using UnityEngine;

namespace var_browser
{
    public class HubResourcePackage
    {
        protected HubBrowse browser;

        private static readonly string[] SizeSuffixes = new string[9] { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        protected string package_id;

        protected string resource_id;

        protected string resolvedVarName;

        protected JSONStorableAction goToResourceAction;

        protected JSONStorableBool isDependencyJSON;

        protected JSONStorableString nameJSON;

        protected JSONStorableString licenseTypeJSON;

        protected JSONStorableString fileSizeJSON;

        protected JSONStorableBool alreadyHaveJSON;

        protected JSONStorableString updateMsgJSON;

        protected JSONStorableBool updateAvailableJSON;

        protected JSONStorableBool alreadyHaveSceneJSON;

        protected string alreadyHaveScenePath;

        protected JSONStorableBool notOnHubJSON;

        public string promotionalUrl;

        protected string downloadUrl;

        protected string latestUrl;

        protected JSONStorableFloat downloadProgressJSON;

        protected JSONStorableBool isDownloadQueuedJSON;

        protected JSONStorableBool isDownloadingJSON;

        protected JSONStorableBool isDownloadedJSON;

        protected JSONStorableAction downloadAction;

        protected JSONStorableAction updateAction;

        protected JSONStorableAction openInPackageManagerAction;

        protected JSONStorableAction openSceneAction;

        public string GroupName { get; protected set; }

        public string Creator { get; protected set; }

        public string Version { get; protected set; }

        public int LatestVersion { get; protected set; }

        public string Name
        {
            get
            {
                return nameJSON.val;
            }
        }

        public string LicenseType
        {
            get
            {
                return licenseTypeJSON.val;
            }
        }

        public int FileSize { get; protected set; }

        public bool NeedsDownload
        {
            get
            {
                return !alreadyHaveJSON.val || updateAvailableJSON.val;
            }
        }

        public bool IsDownloading
        {
            get
            {
                return isDownloadingJSON.val;
            }
        }

        public bool IsDownloadQueued
        {
            get
            {
                return isDownloadQueuedJSON.val;
            }
        }

        public HubResourcePackage(JSONClass package, HubBrowse hubBrowse, bool isDependency)
        {
            browser = hubBrowse;
            package_id = package["package_id"];
            resource_id = package["resource_id"];
            string input = package["filename"];
            input = Regex.Replace(input, ".var$", string.Empty);
            GroupName = Regex.Replace(input, "(.*)\\..*", "$1");
            Creator = Regex.Replace(GroupName, "(.*)\\..*", "$1");
            Version = package["version"];
            if (Version == null)
            {
                Version = Regex.Replace(input, ".*\\.([0-9]+)$", "$1");
            }
            resolvedVarName = GroupName + "." + Version + ".var";
            string text = package["latest_version"];
            if (text == null)
            {
                text = Version;
            }
            if (text != null)
            {
                int result;
                if (int.TryParse(text, out result))
                {
                    LatestVersion = result;
                }
                else
                {
                    LatestVersion = -1;
                }
            }
            string startingValue = package["licenseType"];
            string s = package["file_size"];
            SyncFileSize(s);
            string startingValue2 = SizeSuffix(FileSize);
            downloadUrl = package["downloadUrl"];
            if (downloadUrl == null)
            {
                downloadUrl = package["urlHosted"];
            }
            latestUrl = package["latestUrl"];
            if (latestUrl == null)
            {
                latestUrl = downloadUrl;
            }
            bool startingValue3 = downloadUrl == "null";
            promotionalUrl = package["promotional_link"];
            goToResourceAction = new JSONStorableAction("GoToResource", GoToResource);
            isDependencyJSON = new JSONStorableBool("isDependency", isDependency);
            nameJSON = new JSONStorableString("name", input);
            licenseTypeJSON = new JSONStorableString("licenseType", startingValue);
            fileSizeJSON = new JSONStorableString("fileSize", startingValue2);
            alreadyHaveJSON = new JSONStorableBool("alreadyHave", false);
            alreadyHaveSceneJSON = new JSONStorableBool("alreadyHaveScene", false);
            updateAvailableJSON = new JSONStorableBool("updateAvailable", false);
            updateMsgJSON = new JSONStorableString("updateMsg", "Update");
            updateAction = new JSONStorableAction("Update", Update);
            notOnHubJSON = new JSONStorableBool("notOnHub", startingValue3);
            downloadAction = new JSONStorableAction("Download", Download);
            isDownloadQueuedJSON = new JSONStorableBool("isDownloadQueued", false);
            isDownloadingJSON = new JSONStorableBool("isDownloading", false);
            isDownloadedJSON = new JSONStorableBool("isDownloaded", false);
            downloadProgressJSON = new JSONStorableFloat("downloadProgress", 0f, 0f, 1f, true, false);
            openInPackageManagerAction = new JSONStorableAction("OpenInPackageManager", OpenInPackageManager);
            openSceneAction = new JSONStorableAction("OpenScene", OpenScene);
            Refresh();
        }

        private static string SizeSuffix(int value, int decimalPlaces = 1)
        {
            if (value < 0)
            {
                return "-" + SizeSuffix(-value);
            }
            if (value == 0)
            {
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
            }
            int num = (int)Math.Log(value, 1024.0);
            decimal num2 = (decimal)value / (decimal)(1L << num * 10);
            if (Math.Round(num2, decimalPlaces) >= 1000m)
            {
                num++;
                num2 /= 1024m;
            }
            return string.Format("{0:n" + decimalPlaces + "} {1}", num2, SizeSuffixes[num]);
        }

        protected void GoToResource()
        {
            if (resource_id != "null")
            {
                browser.OpenDetail(resource_id);
            }
        }

        protected void SyncFileSize(string s)
        {
            int result;
            if (int.TryParse(s, out result))
            {
                FileSize = result;
            }
        }

        protected void DownloadStarted()
        {
            isDownloadQueuedJSON.val = false;
            isDownloadingJSON.val = true;
        }

        protected void DownloadProgress(float f,ulong downloadedBytes)
        {
            downloadProgressJSON.val = f;

            fileSizeJSON.val = string.Format("{0}/{1}", SizeSuffix((int)downloadedBytes,2), SizeSuffix(FileSize));
        }

        protected void DownloadComplete(byte[] data, Dictionary<string, string> responseHeaders)
        {
            fileSizeJSON.val =SizeSuffix(FileSize);

            isDownloadingJSON.val = false;
            isDownloadedJSON.val = true;
            string value;
            string text;
            if (responseHeaders.TryGetValue("Content-Disposition", out value))
            {
                value = Regex.Replace(value, ";$", string.Empty);
                text = Regex.Replace(value, ".*filename=\"?([^\"]+)\"?.*", "$1");
            }
            else
            {
                text = resolvedVarName;
            }
            try
            {
                FileManager.WriteAllBytes("AllPackages/" + text, data);
            }
            catch (Exception)
            {
                LogUtil.Log("Error while trying to save file AllPackages/" + text + " after download");
                isDownloadQueuedJSON.val = false;
                isDownloadingJSON.val = false;
            }
        }

        protected void DownloadError(string err)
        {
            isDownloadQueuedJSON.val = false;
            isDownloadingJSON.val = false;
            LogUtil.Log("Error while downloading " + Name + ": " + err);
        }
        HubBrowse.DownloadRequest request;
        public void Download()
        {
            if (browser != null && downloadUrl != null && downloadUrl != string.Empty && downloadUrl != "null" && !isDownloadQueuedJSON.val && (!alreadyHaveJSON.val || updateAvailableJSON.val))
            {
                if (!alreadyHaveJSON.val)
                {
                    isDownloadQueuedJSON.val = true;
                    request=browser.QueueDownload(downloadUrl, promotionalUrl, DownloadStarted, DownloadProgress, DownloadComplete, DownloadError);
                }
                else if (updateAvailableJSON.val && latestUrl != null && latestUrl != string.Empty && latestUrl != "null")
                {
                    isDownloadQueuedJSON.val = true;
                    request = browser.QueueDownload(latestUrl, promotionalUrl, DownloadStarted, DownloadProgress, DownloadComplete, DownloadError);
                }
            }
        }

        public void Update()
        {
            if (browser != null && latestUrl != null && latestUrl != string.Empty && !isDownloadQueuedJSON.val && updateAvailableJSON.val)
            {
                isDownloadQueuedJSON.val = true;
                request = browser.QueueDownload(latestUrl, promotionalUrl, DownloadStarted, DownloadProgress, DownloadComplete, DownloadError);
            }
        }

        public void OpenInPackageManager()
        {
            VarPackage package = FileManager.GetPackage(nameJSON.val);
            if (package != null)
            {
                //SuperController.singleton.OpenPackageInManager(nameJSON.val);
            }
        }

        protected void OpenScene()
        {
            if (alreadyHaveScenePath != null)
            {
                //SuperController.singleton.Load(alreadyHaveScenePath);
            }
        }

        public void Refresh()
        {
            isDownloadedJSON.val = false;
            VarPackage package = null;
            if (isDependencyJSON.val)
            {
                package = FileManager.GetPackage(nameJSON.val);
            }
            else
            {
                string text = FileManager.PackageIDToPackageGroupID(nameJSON.val);
                string packageUidOrPath = text + ".latest";
                package = FileManager.GetPackage(packageUidOrPath);
            }
            if (package != null)
            {
                alreadyHaveJSON.val = true;
                if ((Version == "latest" || !isDependencyJSON.val) && LatestVersion != -1)
                {
                    if (package.Version < LatestVersion)
                    {
                        updateAvailableJSON.val = true;
                        updateMsgJSON.val = "Update " + package.Version + " -> " + LatestVersion;
                    }
                    else
                    {
                        updateAvailableJSON.val = false;
                    }
                }
                else
                {
                    updateAvailableJSON.val = false;
                }
                List<FileEntry> list = new List<FileEntry>();
                package.FindFiles("Saves/scene", "*.json", list);
                if (list.Count > 0)
                {
                    FileEntry fileEntry = list[0];
                    alreadyHaveScenePath = fileEntry.Uid;
                    alreadyHaveSceneJSON.val = true;
                }
                else
                {
                    alreadyHaveScenePath = null;
                    alreadyHaveSceneJSON.val = false;
                }
            }
            else
            {
                alreadyHaveJSON.val = false;
                alreadyHaveScenePath = null;
                alreadyHaveSceneJSON.val = false;
            }
        }

        public void RegisterUI(HubResourcePackageUI ui)
        {
            if (ui != null)
            {
                ui.connectedItem = this;
                goToResourceAction.button = ui.resourceButton;
                if (ui.resourceButton != null)
                {
                    ui.resourceButton.interactable = !notOnHubJSON.val && isDependencyJSON.val;
                }
                isDependencyJSON.indicator = ui.isDependencyIndicator;
                nameJSON.text = ui.nameText;
                licenseTypeJSON.text = ui.licenseTypeText;
                fileSizeJSON.text = ui.fileSizeText;
                //显示范围大一点
                ui.fileSizeText.GetComponent<RectTransform>().sizeDelta = new Vector2(-20,0);

                notOnHubJSON.indicator = ui.notOnHubIndicator;
                alreadyHaveJSON.indicator = ui.alreadyHaveIndicator;
                alreadyHaveSceneJSON.indicator = ui.alreadyHaveSceneIndicator;
                updateAvailableJSON.indicator = ui.updateAvailableIndicator;
                updateMsgJSON.text = ui.updateMsgText;
                updateAction.button = ui.updateButton;
                downloadAction.button = ui.downloadButton;
                isDownloadQueuedJSON.indicator = ui.isDownloadQueuedIndicator;
                isDownloadingJSON.indicator = ui.isDownloadingIndicator;
                isDownloadedJSON.indicator = ui.isDownloadedIndicator;
                downloadProgressJSON.slider = ui.progressSlider;
                openInPackageManagerAction.button = ui.openInPackageManagerButton;
                openSceneAction.button = ui.openSceneButton;

                var parent= ui.progressSlider.GetComponent<RectTransform>();
                RectTransform newObj = UnityEngine.Object.Instantiate(ui.updateButton.transform, parent) as RectTransform;
                newObj.gameObject.SetActive(true);
                newObj.localScale = new Vector3(0.8f,1,1);
                newObj.sizeDelta = new Vector2(80,-10);
                newObj.anchoredPosition = new Vector2(-20, 0);
                newObj.Find("Text").GetComponent<Text>().text = "stop";
                newObj.GetComponent<Image>().color = new Color32(255, 122, 122, 255);
                var btn = newObj.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(StopDownloading);
            }
        }
        void StopDownloading()
        {
            if (request != null)
            {
                request.stop = true;
            }
        }
    }

}
