using System;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace var_browser
{
    public class HubResourceItem
    {
        protected HubBrowse browser;

        protected string resource_id;

        protected string discussion_thread_id;

        protected JSONStorableString titleJSON;

        protected JSONStorableString tagLineJSON;

        protected JSONStorableString versionNumberJSON;

        protected JSONStorableString payTypeJSON;

        protected JSONStorableString categoryJSON;

        protected JSONStorableAction payTypeAndCategorySelectAction;

        protected JSONStorableString creatorJSON;

        protected JSONStorableAction creatorSelectAction;

        protected RawImage creatorIconImage;

        protected Texture2D creatorIconTexture;

        protected bool useQueueImmediate;

        protected var_browser.CustomImageLoaderThreaded.QueuedImage creatorIconQueuedImage;

        protected JSONStorableUrl creatorIconUrlJSON;

        protected RawImage thumbnailImage;

        protected Texture2D thumbnailTexture;

        protected var_browser.CustomImageLoaderThreaded.QueuedImage thumbnailQueuedImage;

        protected JSONStorableUrl thumbnailUrlJSON;

        protected JSONStorableBool hubDownloadableJSON;

        protected JSONStorableBool hubHostedJSON;

        protected JSONStorableString dependencyCountJSON;

        protected JSONStorableBool hasDependenciesJSON;

        protected JSONStorableString downloadCountJSON;

        protected JSONStorableString ratingsCountJSON;

        protected JSONStorableFloat ratingJSON;

        protected JSONStorableString lastUpdateJSON;

        protected JSONArray varFilesJSONArray;

        protected JSONStorableBool inLibraryJSON;

        protected JSONStorableBool updateAvailableJSON;

        protected JSONStorableString updateMsgJSON;

        protected JSONStorableAction openDetailAction;

        public string ResourceId
        {
            get
            {
                return resource_id;
            }
        }

        public string Title
        {
            get
            {
                return titleJSON.val;
            }
        }

        public string TagLine
        {
            get
            {
                return tagLineJSON.val;
            }
        }

        public string VersionNumber
        {
            get
            {
                return versionNumberJSON.val;
            }
        }

        public string PayType
        {
            get
            {
                return payTypeJSON.val;
            }
        }

        public string Category
        {
            get
            {
                return categoryJSON.val;
            }
        }

        public string Creator
        {
            get
            {
                return creatorJSON.val;
            }
        }

        public int DownloadCount
        {
            get
            {
                int result;
                if (int.TryParse(downloadCountJSON.val, out result))
                {
                    return result;
                }
                return 0;
            }
        }

        public int RatingsCount
        {
            get
            {
                int result;
                if (int.TryParse(ratingsCountJSON.val, out result))
                {
                    return result;
                }
                return 0;
            }
        }

        public float Rating
        {
            get
            {
                return ratingJSON.val;
            }
        }

        public DateTime LastUpdateTimestamp { get; protected set; }

        public HubResourceItem(JSONClass resource, HubBrowse hubBrowse, bool queueImagesImmediate = false)
        {
            useQueueImmediate = queueImagesImmediate;
            browser = hubBrowse;
            resource_id = resource["resource_id"];
            discussion_thread_id = resource["discussion_thread_id"];
            string startingValue = resource["title"];
            string startingValue2 = resource["tag_line"];
            string startingValue3 = resource["version_string"];
            string startingValue4 = resource["category"];
            string startingValue5 = resource["type"];
            string startingValue6 = resource["username"];
            string text = resource["icon_url"];
            string text2 = resource["image_url"];
            bool asBool = resource["hubDownloadable"].AsBool;
            bool asBool2 = resource["hubHosted"].AsBool;
            int asInt = resource["dependency_count"].AsInt;
            bool startingValue7 = asInt > 0;
            string startingValue8 = resource["download_count"];
            string startingValue9 = resource["rating_count"];
            float asFloat = resource["rating_avg"].AsFloat;
            int asInt2 = resource["last_update"].AsInt;
            LastUpdateTimestamp = UnixTimeStampToDateTime(asInt2);
            string startingValue10 = ((!((DateTime.Now - LastUpdateTimestamp).TotalDays > 7.0)) ? LastUpdateTimestamp.ToString("dddd h:mm tt") : LastUpdateTimestamp.ToString("MMM d, yyyy"));
            varFilesJSONArray = resource["hubFiles"].AsArray;
            titleJSON = new JSONStorableString("title", startingValue);
            tagLineJSON = new JSONStorableString("tagLine", startingValue2);
            versionNumberJSON = new JSONStorableString("versionNumber", startingValue3);
            payTypeJSON = new JSONStorableString("payType", startingValue4);
            categoryJSON = new JSONStorableString("category", startingValue5);
            payTypeAndCategorySelectAction = new JSONStorableAction("PayTypeAndCategorySelect", PayTypeAndCategorySelect);
            creatorJSON = new JSONStorableString("creator", startingValue6);
            creatorSelectAction = new JSONStorableAction("CreatorSelect", CreatorSelect);
            creatorIconUrlJSON = new JSONStorableUrl("creatorIconUrl", text, SyncCreatorIconUrl);
            SyncCreatorIconUrl(text);
            thumbnailUrlJSON = new JSONStorableUrl("thumbnailUrl", text2, SyncThumbnailUrl);
            SyncThumbnailUrl(text2);
            hubDownloadableJSON = new JSONStorableBool("hubDownloadable", asBool);
            hubHostedJSON = new JSONStorableBool("hubHosted", asBool2);
            hasDependenciesJSON = new JSONStorableBool("hasDependencies", startingValue7);
            dependencyCountJSON = new JSONStorableString("dependencyCount", asInt + " Hub-Hosted Dependencies");
            downloadCountJSON = new JSONStorableString("downloadCount", startingValue8);
            ratingsCountJSON = new JSONStorableString("ratingsCount", startingValue9);
            ratingJSON = new JSONStorableFloat("rating", asFloat, 0f, 5f, true, false);
            lastUpdateJSON = new JSONStorableString("lastUpdate", startingValue10);
            openDetailAction = new JSONStorableAction("OpenDetail", OpenDetail);
            inLibraryJSON = new JSONStorableBool("inLibrary", false);
            updateAvailableJSON = new JSONStorableBool("updateAvailable", false);
            updateMsgJSON = new JSONStorableString("updateMsg", "Update Available");
        }

        protected void PayTypeAndCategorySelect()
        {
            browser.SetPayTypeAndCategoryFilter(payTypeJSON.val, categoryJSON.val);
        }

        protected void CreatorSelect()
        {
            browser.CreatorFilterOnly = creatorJSON.val;
        }

        protected void SyncCreatorIconTexture(var_browser.CustomImageLoaderThreaded.QueuedImage qi)
        {
            creatorIconTexture = qi.tex;
            if (creatorIconImage != null && creatorIconTexture != null)
            {
                creatorIconImage.texture = creatorIconTexture;
            }
        }

        protected void SyncCreatorIconUrl(string url)
        {
            if (var_browser.CustomImageLoaderThreaded.singleton != null && url != null && url != string.Empty)
            {
                var_browser.CustomImageLoaderThreaded.QueuedImage queuedImage = new var_browser.CustomImageLoaderThreaded.QueuedImage();
                queuedImage.imgPath = url;
                queuedImage.callback = SyncCreatorIconTexture;
                creatorIconQueuedImage = queuedImage;
                if (useQueueImmediate)
                {
                    var_browser.CustomImageLoaderThreaded.singleton.QueueThumbnailImmediate(queuedImage);
                }
                else
                {
                    var_browser.CustomImageLoaderThreaded.singleton.QueueThumbnail(queuedImage);
                }
            }
        }

        protected void SyncThumbnailTexture(var_browser.CustomImageLoaderThreaded.QueuedImage qi)
        {
            thumbnailTexture = qi.tex;
            if (thumbnailImage != null && thumbnailTexture != null)
            {
                thumbnailImage.texture = thumbnailTexture;
            }
        }

        protected void SyncThumbnailUrl(string url)
        {
            if (var_browser.CustomImageLoaderThreaded.singleton != null && url != null && url != string.Empty)
            {
                var_browser.CustomImageLoaderThreaded.QueuedImage queuedImage = new var_browser.CustomImageLoaderThreaded.QueuedImage();
                queuedImage.imgPath = url;
                queuedImage.callback = SyncThumbnailTexture;
                thumbnailQueuedImage = queuedImage;
                if (useQueueImmediate)
                {
                    var_browser.CustomImageLoaderThreaded.singleton.QueueThumbnailImmediate(queuedImage);
                }
                else
                {
                    var_browser.CustomImageLoaderThreaded.singleton.QueueThumbnail(queuedImage);
                }
            }
        }
        protected DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();
        }

        public virtual void Refresh()
        {
            if (hubDownloadableJSON.val && varFilesJSONArray != null && varFilesJSONArray.Count > 0)
            {
                bool val = true;
                bool val2 = false;
                IEnumerator enumerator = varFilesJSONArray.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        JSONNode jSONNode = (JSONNode)enumerator.Current;
                        string text = jSONNode["filename"];
                        if (text == null)
                        {
                            continue;
                        }
                        text = Regex.Replace(text, ".var$", string.Empty);
                        string packageGroupUid = Regex.Replace(text, "(.*)\\..*", "$1");
                        string s = Regex.Replace(text, ".*\\.([0-9]+)$", "$1");
                        int result;
                        if (!int.TryParse(s, out result))
                        {
                            continue;
                        }
                        VarPackage package = FileManager.GetPackage(text);
                        if (package == null)
                        {
                            VarPackageGroup packageGroup = FileManager.GetPackageGroup(packageGroupUid);
                            if (packageGroup == null || packageGroup.NewestPackage == null)
                            {
                                val = false;
                                break;
                            }
                            if (packageGroup.NewestPackage.Version < result)
                            {
                                val2 = true;
                                updateMsgJSON.val = "Update Available " + packageGroup.NewestEnabledPackage.Version + " -> " + result;
                            }
                        }
                    }
                }
                finally
                {
                    IDisposable disposable;
                    if ((disposable = enumerator as IDisposable) != null)
                    {
                        disposable.Dispose();
                    }
                }
                inLibraryJSON.val = val;
                updateAvailableJSON.val = val2;
            }
            else
            {
                inLibraryJSON.val = false;
                updateAvailableJSON.val = false;
            }
        }

        public void OpenDetail()
        {
            browser.OpenDetail(resource_id);
        }

        public void Hide()
        {
            if (creatorIconQueuedImage != null)
            {
                creatorIconQueuedImage.cancel = true;
            }
            if (thumbnailQueuedImage != null)
            {
                thumbnailQueuedImage.cancel = true;
            }
        }

        public void Show()
        {
            if (creatorIconQueuedImage != null && !creatorIconQueuedImage.preprocessed)
            {
                creatorIconQueuedImage.cancel = false;
                if (useQueueImmediate)
                {
                    var_browser.CustomImageLoaderThreaded.singleton.QueueThumbnailImmediate(creatorIconQueuedImage);
                }
                else
                {
                    var_browser.CustomImageLoaderThreaded.singleton.QueueThumbnail(creatorIconQueuedImage);
                }
            }
            if (thumbnailQueuedImage != null && !thumbnailQueuedImage.preprocessed)
            {
                thumbnailQueuedImage.cancel = false;
                if (useQueueImmediate)
                {
                    var_browser.CustomImageLoaderThreaded.singleton.QueueThumbnailImmediate(thumbnailQueuedImage);
                }
                else
                {
                    var_browser.CustomImageLoaderThreaded.singleton.QueueThumbnail(thumbnailQueuedImage);
                }
            }
        }

        public void Destroy()
        {
            if (creatorIconQueuedImage != null)
            {
                creatorIconQueuedImage.cancel = true;
            }
            if (thumbnailQueuedImage != null)
            {
                thumbnailQueuedImage.cancel = true;
            }
        }

        public virtual void RegisterUI(HubResourceItemUI ui)
        {
            if (ui != null)
            {
                ui.connectedItem = this;

                titleJSON.text = ui.titleText;
                tagLineJSON.text = ui.tagLineText;
                versionNumberJSON.text = ui.versionText;
                payTypeJSON.text = ui.payTypeText;
                categoryJSON.text = ui.categoryText;
                payTypeAndCategorySelectAction.button = ui.payTypeAndCategorySelectButton;
                creatorSelectAction.button = ui.creatorSelectButton;
                creatorJSON.text = ui.creatorText;
                creatorIconImage = ui.creatorIconImage;
                if (creatorIconImage != null && creatorIconTexture != null)
                {
                    creatorIconImage.texture = creatorIconTexture;
                }
                thumbnailImage = ui.thumbnailImage;
                if (thumbnailImage != null && thumbnailTexture != null)
                {
                    thumbnailImage.texture = thumbnailTexture;
                }
                hubDownloadableJSON.indicator = ui.hubDownloadableIndicator;
                hubDownloadableJSON.negativeIndicator = ui.hubDownloadableNegativeIndicator;
                hubHostedJSON.indicator = ui.hubHostedIndicator;
                hubHostedJSON.negativeIndicator = ui.hubHostedNegativeIndicator;
                hasDependenciesJSON.indicator = ui.hasDependenciesIndicator;
                hasDependenciesJSON.negativeIndicator = ui.hasDependenciesNegativeIndicator;
                dependencyCountJSON.text = ui.dependencyCountText;
                downloadCountJSON.text = ui.downloadCountText;
                ratingsCountJSON.text = ui.ratingsCountText;
                ratingJSON.slider = ui.ratingSlider;
                lastUpdateJSON.text = ui.lastUpdateText;
                openDetailAction.button = ui.openDetailButton;

                inLibraryJSON.indicator = ui.inLibraryIndicator;
                updateAvailableJSON.indicator = ui.updateAvailableIndicator;

                updateMsgJSON.text = ui.updateMsgText;
            }
        }
    }

}
