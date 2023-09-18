using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using ZenFulcrum.EmbeddedBrowser;
//using MVR.Hub;

namespace var_browser
{
    public class HubBrowse : JSONStorable
    {
        public delegate void BinaryRequestStartedCallback();

        public delegate void BinaryRequestSuccessCallback(byte[] data, Dictionary<string, string> responseHeaders);

        public delegate void RequestSuccessCallback(SimpleJSON.JSONNode jsonNode);

        public delegate void RequestErrorCallback(string err);

        public delegate void RequestProgressCallback(float progress,ulong downloadedBytes);

        public delegate void EnableHubCallback();

        public delegate void EnableWebBrowserCallback();

        public delegate void PreShowCallback();

        public class DownloadRequest
        {
            public string url;

            public string promotionalUrl;

            public BinaryRequestStartedCallback startedCallback;

            public RequestProgressCallback progressCallback;

            public BinaryRequestSuccessCallback successCallback;

            public RequestErrorCallback errorCallback;

            public bool stop = false;
        }

        public static HubBrowse singleton;

        public string cookieHost = "hub.virtamate.com";

        public string apiUrl = "https://hub.virtamate.com/citizenx/api.php";

        public string packagesJSONUrl = "https://s3cdn.virtamate.com/data/packages.json";

        protected bool _hubEnabled;

        protected JSONStorableBool hubEnabledJSON;

        public EnableHubCallback enableHubCallbacks;

        protected JSONStorableAction enableHubAction;

        protected bool _webBrowserEnabled;

        protected JSONStorableBool webBrowserEnabledJSON;

        public EnableWebBrowserCallback enableWebBrowserCallbacks;

        protected JSONStorableAction enableWebBrowserAction;

        protected MVR.Hub.HubBrowseUI hubBrowseUI;

        public RectTransform itemPrefab;

        protected RectTransform itemContainer;

        protected ScrollRect itemScrollRect;

        protected List<HubResourceItemUI> items;

        public RectTransform resourceDetailPrefab;

        protected GameObject detailPanel;

        public RectTransform packageDownloadPrefab;

        public RectTransform creatorSupportButtonPrefab;

        protected RectTransform resourceDetailContainer;

        protected Browser browser;

        protected VRWebBrowser webBrowser;

        protected GameObject isWebLoadingIndicator;

        protected GameObject refreshIndicator;

        protected bool _isShowing;

        public PreShowCallback preShowCallbacks;

        protected bool _hasBeenRefreshed;

        protected Coroutine refreshResourcesRoutine;

        protected JSONStorableAction refreshResourcesAction;

        protected JSONStorableString numResourcesJSON;

        protected JSONStorableString pageInfoJSON;

        protected int _numPagesInt;

        protected JSONStorableString numPagesJSON;

        protected int _numPerPageInt = 48;

        protected JSONStorableFloat numPerPageJSON;

        protected string _currentPageString = "1";

        protected int _currentPageInt = 1;

        protected JSONStorableString currentPageJSON;

        protected JSONStorableAction firstPageAction;

        protected JSONStorableAction previousPageAction;

        protected JSONStorableAction nextPageAction;

        protected JSONStorableAction clearFiltersAction;

        protected string _hostedOption = "Hub And Dependencies";

        protected JSONStorableStringChooser hostedOptionChooser;

        protected string _payTypeFilter = "Free";

        protected JSONStorableStringChooser payTypeFilterChooser;

        protected const float _triggerDelay = 0.5f;

        protected float triggerCountdown;

        protected Coroutine triggerResetRefreshRoutine;

        protected string _minLengthSearchFilter = string.Empty;

        protected string _searchFilter = string.Empty;

        protected JSONStorableString searchFilterJSON;

        protected string _categoryFilter = "All";

        protected JSONStorableStringChooser categoryFilterChooser;

        protected string _creatorFilter = "All";

        protected JSONStorableStringChooser creatorFilterChooser;

        protected string _tagsFilter = "All";

        protected JSONStorableStringChooser tagsFilterChooser;

        protected string _sortPrimary = "Latest Update";

        protected JSONStorableStringChooser sortPrimaryChooser;

        protected string _sortSecondary = "None";

        protected JSONStorableStringChooser sortSecondaryChooser;

        protected Dictionary<string, HubResourceItemDetailUI> savedResourceDetailsPanels;

        protected Stack<HubResourceItemDetailUI> resourceDetailStack;

        //public PackageBuilder packageManager;

        protected GameObject missingPackagesPanel;

        protected RectTransform missingPackagesContainer;

        protected List<string> checkMissingPackageNames;

        protected List<HubResourcePackageUI> missingPackages;

        protected JSONStorableAction openMissingPackagesPanelAction;

        protected JSONStorableAction closeMissingPackagesPanelAction;

        protected JSONStorableAction downloadAllMissingPackagesAction;

        protected GameObject updatesPanel;

        protected RectTransform updatesContainer;

        protected List<string> checkUpdateNames;

        protected List<HubResourcePackageUI> updates;

        protected Dictionary<string, int> packageGroupToLatestVersion;

        protected Dictionary<string, string> packageIdToResourceId;

        protected JSONStorableAction openUpdatesPanelAction;

        protected JSONStorableAction closeUpdatesPanelAction;

        protected JSONStorableAction downloadAllUpdatesAction;

        protected JSONStorableBool isDownloadingJSON;

        protected JSONStorableString downloadQueuedCountJSON;

        protected Queue<DownloadRequest> downloadQueue;

        protected List<string> hubCookies;

        protected Coroutine GetBrowserCookiesRoutine;

        protected JSONStorableAction openDownloadingAction;

        protected RectTransform refreshingGetInfoPanel;

        protected RectTransform failedGetInfoPanel;

        protected Text getInfoErrorText;

        protected bool hubInfoSuccess;

        protected bool hubInfoCompleted;

        protected bool hubInfoRefreshing;

        protected Coroutine hubInfoCoroutine;

        protected JSONStorableAction cancelGetHubInfoAction;

        protected JSONStorableAction retryGetHubInfoAction;

        public bool HubEnabled
        {
            get
            {
                return _hubEnabled;
            }
            set
            {
                if (hubEnabledJSON != null)
                {
                    hubEnabledJSON.val = value;
                }
                else
                {
                    _hubEnabled = value;
                }
            }
        }

        public bool WebBrowserEnabled
        {
            get
            {
                return _webBrowserEnabled;
            }
            set
            {
                if (webBrowserEnabledJSON != null)
                {
                    webBrowserEnabledJSON.val = value;
                }
                else
                {
                    _webBrowserEnabled = value;
                }
            }
        }

        public string HostedOption
        {
            get
            {
                return _hostedOption;
            }
            set
            {
                hostedOptionChooser.val = value;
            }
        }

        public string PayTypeFilter
        {
            get
            {
                return _payTypeFilter;
            }
            set
            {
                payTypeFilterChooser.val = value;
            }
        }

        public string SearchFilter
        {
            get
            {
                return _searchFilter;
            }
            set
            {
                searchFilterJSON.val = value;
            }
        }

        public string CategoryFilter
        {
            get
            {
                return _categoryFilter;
            }
            set
            {
                categoryFilterChooser.val = value;
            }
        }

        public string CreatorFilter
        {
            get
            {
                return _creatorFilter;
            }
            set
            {
                _hostedOption = "All";
                hostedOptionChooser.valNoCallback = "All";
                creatorFilterChooser.val = value;
            }
        }

        public string CreatorFilterOnly
        {
            get
            {
                return _creatorFilter;
            }
            set
            {
                CloseAllDetails();
                ResetFilters();
                _hostedOption = "All";
                hostedOptionChooser.valNoCallback = "All";
                creatorFilterChooser.val = value;
            }
        }

        public string TagsFilter
        {
            get
            {
                return _tagsFilter;
            }
            set
            {
                tagsFilterChooser.val = value;
            }
        }

        public string TagsFilterOnly
        {
            get
            {
                return _tagsFilter;
            }
            set
            {
                ResetFilters();
                tagsFilterChooser.val = value;
            }
        }

        private IEnumerator GetRequest(string uri, RequestSuccessCallback callback, RequestErrorCallback errorCallback)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                webRequest.SendWebRequest();
                while (!webRequest.isDone)
                {
                    yield return null;
                }
                if (webRequest.isNetworkError)
                {
                    LogUtil.LogError(uri + ": Error: " + webRequest.error);
                    if (errorCallback != null)
                    {
                        errorCallback(webRequest.error);
                    }
                }
                else
                {
                    SimpleJSON.JSONNode jsonNode = JSON.Parse(webRequest.downloadHandler.text);
                    if (callback != null)
                    {
                        callback(jsonNode);
                    }
                }
            }
        }

        private IEnumerator BinaryGetRequest(DownloadRequest request,string uri, BinaryRequestStartedCallback startedCallback, BinaryRequestSuccessCallback successCallback, RequestErrorCallback errorCallback, RequestProgressCallback progressCallback, List<string> cookies = null)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                string cookieHeader = "vamhubconsent=1";
                if (cookies != null)
                {
                    foreach (string cookie in cookies)
                    {
                        cookieHeader = cookieHeader + ";" + cookie;
                    }
                }
                webRequest.SetRequestHeader("Cookie", cookieHeader);
                webRequest.SendWebRequest();
                if (startedCallback != null)
                {
                    startedCallback();
                }
                while (!webRequest.isDone)
                {
                    
                    if (progressCallback != null)
                    {
                        progressCallback(webRequest.downloadProgress, webRequest.downloadedBytes);
                    }
                    if (request.stop)
                    {
                        LogUtil.Log("stop request " + uri);
                        break;
                    }
                    yield return null;
                }
                if (request.stop || webRequest.isNetworkError)
                {
                    LogUtil.LogError(uri + ": Error: " + webRequest.error);
                    if (errorCallback != null)
                    {
                        errorCallback(webRequest.error);
                    }
                }
                else
                {
                    Dictionary<string, string> responseHeaders = webRequest.GetResponseHeaders();
                    if (successCallback != null)
                    {
                        successCallback(webRequest.downloadHandler.data, responseHeaders);
                    }
                }
            }
        }

        private IEnumerator PostRequest(string uri, string postData, RequestSuccessCallback callback, RequestErrorCallback errorCallback)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, postData))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(postData));
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                yield return webRequest.SendWebRequest();
                string[] pages = uri.Split('/');
                int page = pages.Length - 1;
                if (webRequest.isNetworkError)
                {
                    LogUtil.LogError(pages[page] + ": Error: " + webRequest.error);
                    if (errorCallback != null)
                    {
                        errorCallback(webRequest.error);
                    }
                    yield break;
                }
                //Debug.Log(webRequest.downloadHandler.text);
                SimpleJSON.JSONNode jSONNode = JSON.Parse(webRequest.downloadHandler.text);
                if (jSONNode == null)
                {
                    string text = "Error - Invalid JSON response: " + webRequest.downloadHandler.text;
                    //Debug.LogError(pages[page] + ": " + text);
                    if (errorCallback != null)
                    {
                        errorCallback(text);
                    }
                }
                else if (callback != null)
                {
                    callback(jSONNode);
                }
            }
        }

        protected void SyncHubEnabled(bool b)
        {
            _hubEnabled = b;
            if (_hubEnabled)
            {
                GetHubInfo();
                if (_isShowing)
                {
                    RefreshResources();
                }
            }
        }

        protected void EnableHub()
        {
            if (enableHubCallbacks != null)
            {
                enableHubCallbacks();
            }
        }

        protected void SyncWebBrowserEnabled(bool b)
        {
            _webBrowserEnabled = b;
            if (_webBrowserEnabled && resourceDetailStack != null && resourceDetailStack.Count > 0)
            {
                HubResourceItemDetailUI hubResourceItemDetailUI = resourceDetailStack.Peek();
                if (hubResourceItemDetailUI.connectedItem != null)
                {
                    hubResourceItemDetailUI.gameObject.SetActive(true);//sf
                    hubResourceItemDetailUI.connectedItem.NavigateToOverview();
                }
            }
        }

        protected void EnableWebBrowser()
        {
            if (enableWebBrowserCallbacks != null)
            {
                enableWebBrowserCallbacks();
            }
        }

        public void Show()
        {
            LogUtil.Log("HubBrowse Show");
            if (preShowCallbacks != null)
            {
                preShowCallbacks();
            }
            _isShowing = true;
            if (hubBrowseUI != null)
            {
                hubBrowseUI.gameObject.SetActive(true);
            }
            else if (UITransform != null)
            {
                UITransform.gameObject.SetActive(true);
            }
            if (!_hubEnabled)
            {
                return;
            }
            if (_hasBeenRefreshed)
            {
                if (items == null)
                {
                    return;
                }
                {
                    foreach (HubResourceItemUI item in items)
                    {
                        if (item.connectedItem != null)
                        {
                            item.connectedItem.Show();
                        }
                    }
                    return;
                }
            }
            RefreshResources();
        }

        public void Hide()
        {
            LogUtil.Log("HubBrowse Hide");
            _isShowing = false;
            if (hubBrowseUI != null)
            {
                hubBrowseUI.gameObject.SetActive(false);
            }
            if (items == null)
            {
                return;
            }
            foreach (HubResourceItemUI item in items)
            {
                if (item.connectedItem != null)
                {
                    item.connectedItem.Hide();
                }
            }
        }

        protected void RefreshErrorCallback(string err)
        {
            if (refreshIndicator != null)
            {
                refreshIndicator.SetActive(false);
            }
            SuperController.LogError("Error during hub request " + err);
        }

        protected void RefreshCallback(SimpleJSON.JSONNode jsonNode)
        {
            if (refreshIndicator != null)
            {
                refreshIndicator.SetActive(false);
            }
            if (!(jsonNode != null))
            {
                return;
            }
            JSONClass asObject = jsonNode.AsObject;
            if (!(asObject != null))
            {
                return;
            }
            string text = asObject["status"];
            if (text == "success")
            {
                JSONClass asObject2 = asObject["pagination"].AsObject;
                if (!(asObject2 != null))
                {
                    return;
                }
                numResourcesJSON.val = "Total: " + asObject2["total_found"];
                numPagesJSON.val = asObject2["total_pages"];
                if (items != null)
                {
                    foreach (HubResourceItemUI item in items)
                    {
                        if (item.connectedItem != null)
                        {
                            item.connectedItem.Destroy();
                        }
                        UnityEngine.Object.Destroy(item.gameObject);
                    }
                    items.Clear();
                }
                else
                {
                    items = new List<HubResourceItemUI>();
                }
                if (itemScrollRect != null)
                {
                    itemScrollRect.verticalNormalizedPosition = 1f;
                }
                JSONArray asArray = asObject["resources"].AsArray;
                if (!(itemContainer != null) || !(itemPrefab != null) || !(asArray != null))
                {
                    return;
                }
                IEnumerator enumerator2 = asArray.GetEnumerator();
                try
                {
                    while (enumerator2.MoveNext())
                    {
                        JSONClass resource = (JSONClass)enumerator2.Current;
                        HubResourceItem hubResourceItem = new HubResourceItem(resource, this);
                        hubResourceItem.Refresh();

                        RectTransform rectTransform = UnityEngine.Object.Instantiate(itemPrefab);
                        rectTransform.SetParent(itemContainer, false);
                        HubResourceItemUI component = rectTransform.GetComponent<HubResourceItemUI>();
                        if (component != null)
                        {
                            hubResourceItem.RegisterUI(component);
                            items.Add(component);
                        }
                    }
                    return;
                }
                finally
                {
                    IDisposable disposable;
                    if ((disposable = enumerator2 as IDisposable) != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            string text2 = jsonNode["error"];
            LogUtil.Log("Refresh returned error " + text2);
        }

        public void RefreshResources()
        {
            _hasBeenRefreshed = true;
            if (_hubEnabled)
            {
                if (refreshResourcesRoutine != null)
                {
                    StopCoroutine(refreshResourcesRoutine);
                }
                JSONClass jSONClass = new JSONClass();
                jSONClass["source"] = "VaM";
                jSONClass["action"] = "getResources";
                jSONClass["latest_image"] = "Y";
                jSONClass["perpage"] = _numPerPageInt.ToString();
                jSONClass["page"] = _currentPageString;
                if (_hostedOption != "All")
                {
                    jSONClass["location"] = _hostedOption;
                }
                if (_searchFilter != string.Empty)
                {
                    jSONClass["search"] = _searchFilter;
                    jSONClass["searchall"] = "true";
                }
                if (_payTypeFilter != "All")
                {
                    jSONClass["category"] = _payTypeFilter;
                }
                if (_categoryFilter != "All")
                {
                    jSONClass["type"] = _categoryFilter;
                }
                if (_creatorFilter != "All")
                {
                    jSONClass["username"] = _creatorFilter;
                }
                if (_tagsFilter != "All")
                {
                    jSONClass["tags"] = _tagsFilter;
                }
                string text = _sortPrimary;
                if (_sortSecondary != null && _sortSecondary != string.Empty && _sortSecondary != "None")
                {
                    text = text + "," + _sortSecondary;
                }
                jSONClass["sort"] = text;
                string postData = jSONClass.ToString();
                refreshResourcesRoutine = StartCoroutine(PostRequest(apiUrl, postData, RefreshCallback, RefreshErrorCallback));
                if (refreshIndicator != null)
                {
                    refreshIndicator.SetActive(true);
                }
            }
        }

        protected void SyncNumResources(string s)
        {
        }

        protected void SetPageInfo()
        {
            pageInfoJSON.val = "Page " + currentPageJSON.val + " of " + numPagesJSON.val;
        }

        protected void SyncNumPages(string s)
        {
            int result;
            if (int.TryParse(s, out result))
            {
                _numPagesInt = result;
            }
            SetPageInfo();
        }

        protected void SyncNumPerPage(float f)
        {
            _numPerPageInt = (int)f;
            ResetRefresh();
        }

        protected void ResetRefresh()
        {
            _currentPageString = "1";
            _currentPageInt = 1;
            currentPageJSON.valNoCallback = _currentPageString;
            SetPageInfo();
            RefreshResources();
        }

        protected void SyncCurrentPage(string s)
        {
            _currentPageString = s;
            int result;
            if (int.TryParse(s, out result))
            {
                _currentPageInt = result;
            }
            SetPageInfo();
            RefreshResources();
        }

        protected void FirstPage()
        {
            currentPageJSON.val = "1";
        }

        protected void PreviousPage()
        {
            if (_currentPageInt > 1)
            {
                currentPageJSON.val = (_currentPageInt - 1).ToString();
            }
        }

        protected void NextPage()
        {
            if (_currentPageInt < _numPagesInt)
            {
                currentPageJSON.val = (_currentPageInt + 1).ToString();
            }
        }

        protected void ResetFilters()
        {
            _payTypeFilter = "All";
            payTypeFilterChooser.valNoCallback = "All";
            _searchFilter = string.Empty;
            searchFilterJSON.valNoCallback = string.Empty;
            _categoryFilter = "All";
            categoryFilterChooser.valNoCallback = "All";
            _creatorFilter = "All";
            creatorFilterChooser.valNoCallback = "All";
            _tagsFilter = "All";
            tagsFilterChooser.valNoCallback = "All";
        }

        protected void ResetFiltersAndRefresh()
        {
            ResetFilters();
            ResetRefresh();
        }

        protected void SyncHostedOption(string s)
        {
            _hostedOption = s;
            ResetRefresh();
        }

        protected void SyncPayTypeFilter(string s)
        {
            _payTypeFilter = s;
            if (_payTypeFilter != "Free" && _hostedOption != "All")
            {
                hostedOptionChooser.val = "All";
            }
            else
            {
                ResetRefresh();
            }
        }

        protected IEnumerator TriggerResetRefesh()
        {
            while (triggerCountdown > 0f)
            {
                triggerCountdown -= Time.unscaledDeltaTime;
                yield return null;
            }
            triggerResetRefreshRoutine = null;
            ResetRefresh();
        }

        protected void SyncSearchFilter(string s)
        {
            _searchFilter = s;
            bool flag = false;
            if (_searchFilter.Length > 2)
            {
                if (_minLengthSearchFilter != _searchFilter)
                {
                    _minLengthSearchFilter = _searchFilter;
                    flag = true;
                }
            }
            else if (_minLengthSearchFilter != string.Empty)
            {
                _minLengthSearchFilter = string.Empty;
                flag = true;
            }
            if (flag)
            {
                triggerCountdown = 0.5f;
                if (triggerResetRefreshRoutine == null)
                {
                    triggerResetRefreshRoutine = StartCoroutine(TriggerResetRefesh());
                }
            }
        }

        protected void SyncCategoryFilter(string s)
        {
            _categoryFilter = s;
            ResetRefresh();
        }

        public void SetPayTypeAndCategoryFilter(string payType, string category, bool onlyTheseFilters = true)
        {
            if (onlyTheseFilters)
            {
                CloseAllDetails();
                ResetFilters();
            }
            _payTypeFilter = payType;
            payTypeFilterChooser.valNoCallback = payType;
            _categoryFilter = category;
            categoryFilterChooser.valNoCallback = category;
            ResetRefresh();
        }

        protected void SyncCreatorFilter(string s)
        {
            _creatorFilter = s;
            ResetRefresh();
        }

        protected void SyncTagsFilter(string s)
        {
            _tagsFilter = s;
            ResetRefresh();
        }

        protected void SyncSortPrimary(string s)
        {
            _sortPrimary = s;
            ResetRefresh();
        }

        protected void SyncSortSecondary(string s)
        {
            _sortSecondary = s;
            ResetRefresh();
        }

        public void NavigateWebPanel(string url)
        {
            if (webBrowser != null && webBrowser.url != url && _webBrowserEnabled)
            {
                if (isWebLoadingIndicator != null)
                {
                    isWebLoadingIndicator.SetActive(true);
                }
                webBrowser.url = url;
            }
        }

        public void ShowHoverUrl(string url)
        {
            if (webBrowser != null)
            {
                webBrowser.HoveredURL = url;
            }
        }

        protected void GetResourceDetailErrorCallback(string err, HubResourceItemDetailUI hridui)
        {
            LogUtil.Log("Error during fetch of resource detail from Hub");
            CloseDetail(null);
        }

        protected void GetResourceDetailCallback(SimpleJSON.JSONNode jsonNode, HubResourceItemDetailUI hridui)
        {
            if (jsonNode != null && hridui != null)
            {
                JSONClass asObject = jsonNode.AsObject;
                if (asObject != null)
                {
                    HubResourceItemDetail hubResourceItemDetail = new HubResourceItemDetail(asObject, this);
                    hubResourceItemDetail.Refresh();
                    hubResourceItemDetail.RegisterUI(hridui);
                }
            }
        }

        public void OpenDetail(string resource_id, bool isPackageName = false)
        {
            if (_hubEnabled)
            {
                if (!(resourceDetailPrefab != null) || !(resourceDetailContainer != null))
                {
                    return;
                }
                Show();

                HubResourceItemDetailUI hridui;
                //所有不在栈里的detailpanel都放在savedResourceDetailsPanels
                if (savedResourceDetailsPanels.TryGetValue(resource_id, out hridui))
                {
                    savedResourceDetailsPanels.Remove(resource_id);
                    hridui.gameObject.SetActive(true);
                    resourceDetailStack.Push(hridui);
                    hridui.transform.SetAsLastSibling();//移动到最后的位置，保证显示是正确的
                }
                else
                {
                    RectTransform rectTransform = UnityEngine.Object.Instantiate(resourceDetailPrefab);
                    rectTransform.SetParent(resourceDetailContainer, false);
                    hridui = rectTransform.GetComponent<HubResourceItemDetailUI>();
                    resourceDetailStack.Push(hridui);
                    hridui.transform.SetAsLastSibling();//移动到最后的位置，保证显示是正确的

                    JSONClass jSONClass = new JSONClass();
                    jSONClass["source"] = "VaM";
                    jSONClass["action"] = "getResourceDetail";
                    jSONClass["latest_image"] = "Y";
                    if (isPackageName)
                    {
                        jSONClass["package_name"] = resource_id;
                    }
                    else
                    {
                        jSONClass["resource_id"] = resource_id;
                    }
                    string postData = jSONClass.ToString();
                    StartCoroutine(PostRequest(apiUrl, postData,
                        jsonNode => { GetResourceDetailCallback(jsonNode, hridui); },
                        err => { this.GetResourceDetailErrorCallback(err, hridui); }));
                }
                if (detailPanel != null)
                {
                    detailPanel.SetActive(true);
                }
            }
            else
            {
                LogUtil.LogError("Cannot perform action. Hub is disabled in User Preferences");
            }
        }

        public void CloseDetail(string resource_id)
        {
            //关闭的时候，发现栈中还有数据的时候
            if (resourceDetailStack.Count > 0)
            {
                HubResourceItemDetailUI hubResourceItemDetailUI = resourceDetailStack.Pop();
                if (hubResourceItemDetailUI.connectedItem != null && hubResourceItemDetailUI.connectedItem.IsDownloading)
                {
                    hubResourceItemDetailUI.gameObject.SetActive(false);
                    if (!savedResourceDetailsPanels.ContainsKey(resource_id))
                        savedResourceDetailsPanels.Add(resource_id, hubResourceItemDetailUI);
                }
                else
                {
                    //如果下完了，直接移除
                    if (resource_id != null)
                    {
                        savedResourceDetailsPanels.Remove(resource_id);
                    }
                    UnityEngine.Object.Destroy(hubResourceItemDetailUI.gameObject);
                }
            }
            if (resourceDetailStack.Count == 0)
            {
                if (detailPanel != null)
                {
                    detailPanel.SetActive(false);
                }
            }
            else
            {
                HubResourceItemDetailUI hubResourceItemDetailUI2 = resourceDetailStack.Peek();
                if (hubResourceItemDetailUI2.connectedItem != null)
                {
                    //显示栈中的下一个
                    hubResourceItemDetailUI2.gameObject.SetActive(true);
                    hubResourceItemDetailUI2.connectedItem.NavigateToOverview();
                }
            }

            //所有没有在下载的detailpanel都应该被删掉
            List<string> removes = new List<string>();
            foreach (string key in savedResourceDetailsPanels.Keys)
            {
                var hubResourceItemDetailUI = savedResourceDetailsPanels[key];
                if (hubResourceItemDetailUI.connectedItem != null && hubResourceItemDetailUI.connectedItem.IsDownloading)
                {
                    //保留
                }
                else
                {
                    //清除
                    removes.Add(key);
                }
            }
            //没在下载的都移除
            foreach (var key in removes)
            {
                var hubResourceItemDetailUI = savedResourceDetailsPanels[key];
                savedResourceDetailsPanels.Remove(key);
                UnityEngine.Object.Destroy(hubResourceItemDetailUI.gameObject);
            }

        }

        protected void CloseAllDetails()
        {
            while (resourceDetailStack.Count > 0)
            {
                HubResourceItemDetailUI hubResourceItemDetailUI = resourceDetailStack.Pop();
                if (hubResourceItemDetailUI.connectedItem != null && hubResourceItemDetailUI.connectedItem.IsDownloading)
                {
                    hubResourceItemDetailUI.gameObject.SetActive(false);
                    savedResourceDetailsPanels.Add(hubResourceItemDetailUI.connectedItem.ResourceId, hubResourceItemDetailUI);
                    continue;
                }
                if (hubResourceItemDetailUI.connectedItem != null)
                {
                    savedResourceDetailsPanels.Remove(hubResourceItemDetailUI.connectedItem.ResourceId);
                }
                UnityEngine.Object.Destroy(hubResourceItemDetailUI.gameObject);
            }
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
        }

        public RectTransform CreateDownloadPrefabInstance()
        {
            RectTransform result = null;
            if (packageDownloadPrefab != null)
            {
                result = UnityEngine.Object.Instantiate(packageDownloadPrefab);
            }
            return result;
        }

        public RectTransform CreateCreatorSupportButtonPrefabInstance()
        {
            RectTransform result = null;
            if (creatorSupportButtonPrefab != null)
            {
                result = UnityEngine.Object.Instantiate(creatorSupportButtonPrefab);
            }
            return result;
        }

        protected void FindMissingPackagesErrorCallback(string err)
        {
            //SuperController.LogError("Error during hub request " + err);
        }

        protected void FindMissingPackagesCallback(SimpleJSON.JSONNode jsonNode)
        {
            if (!(jsonNode != null))
            {
                return;
            }
            JSONClass asObject = jsonNode.AsObject;
            if (!(asObject != null))
            {
                return;
            }
            string text = asObject["status"];
            if (text != null && text == "error")
            {
                string text2 = jsonNode["error"];
                LogUtil.LogError("findPackages returned error " + text2);
                return;
            }
            JSONClass asObject2 = jsonNode["packages"].AsObject;
            if (!(asObject2 != null))
            {
                return;
            }
            if (missingPackages != null)
            {
                foreach (HubResourcePackageUI missingPackage in missingPackages)
                {
                    UnityEngine.Object.Destroy(missingPackage.gameObject);
                }
                missingPackages.Clear();
            }
            else
            {
                missingPackages = new List<HubResourcePackageUI>();
            }
            foreach (string checkMissingPackageName in checkMissingPackageNames)
            {
                JSONClass jSONClass = asObject2[checkMissingPackageName].AsObject;
                if (jSONClass == null)
                {
                    jSONClass = new JSONClass();
                    jSONClass["filename"] = checkMissingPackageName;
                    jSONClass["downloadUrl"] = "null";
                }
                else
                {
                    if (Regex.IsMatch(checkMissingPackageName, "[0-9]+$"))
                    {
                        string text3 = jSONClass["filename"];
                        if (text3 == null || text3 == "null" || text3 != checkMissingPackageName + ".var")
                        {
                            LogUtil.LogError("Missing file name " + text3 + " does not match missing package name " + checkMissingPackageName);
                            jSONClass["filename"] = checkMissingPackageName;
                            jSONClass["file_size"] = "null";
                            jSONClass["licenseType"] = "null";
                            jSONClass["downloadUrl"] = "null";
                        }
                    }
                    else
                    {
                        string text4 = jSONClass["filename"];
                        if (text4 == null || text4 == "null")
                        {
                            jSONClass["filename"] = checkMissingPackageName;
                        }
                    }
                    string text5 = jSONClass["resource_id"];
                    if (text5 == null || text5 == "null")
                    {
                        jSONClass["downloadUrl"] = "null";
                    }
                }
                HubResourcePackage hubResourcePackage = new HubResourcePackage(jSONClass, this, true);
                RectTransform rectTransform = CreateDownloadPrefabInstance();
                if (rectTransform != null)
                {
                    rectTransform.SetParent(missingPackagesContainer, false);
                    HubResourcePackageUI component = rectTransform.GetComponent<HubResourcePackageUI>();
                    if (component != null)
                    {
                        missingPackages.Add(component);
                        hubResourcePackage.RegisterUI(component);
                    }
                }
            }
        }

        public void OpenMissingPackagesPanel()
        {
            if (_hubEnabled)
            {
                if ((missingPackagesPanel == null) || (missingPackagesContainer == null))
                {
                    return;
                }

                Show();
                if (missingPackagesPanel != null)
                {
                    missingPackagesPanel.SetActive(true);
                }
                if (downloadQueue.Count != 0)
                {
                    return;
                }


                List<string> missingPackageNames = FileManager.singleton.GetMissingDependenciesNames();
                if (missingPackageNames.Count > 0)
                {
                    JSONClass jSONClass = new JSONClass();
                    jSONClass["source"] = "VaM";
                    jSONClass["action"] = "findPackages";
                    checkMissingPackageNames = missingPackageNames;
                    jSONClass["packages"] = string.Join(",", missingPackageNames.ToArray());
                    string postData = jSONClass.ToString();
                    StartCoroutine(PostRequest(apiUrl, postData, FindMissingPackagesCallback, FindMissingPackagesErrorCallback));
                }
                else if (missingPackages != null)
                {
                    foreach (HubResourcePackageUI missingPackage in missingPackages)
                    {
                        UnityEngine.Object.Destroy(missingPackage.gameObject);
                    }
                    missingPackages.Clear();
                }
                else
                {
                    missingPackages = new List<HubResourcePackageUI>();
                }
            }
            else
            {
                LogUtil.LogWarning("[var browser]Cannot perform action. Hub is disabled in User Preferences");
            }
        }

        public void CloseMissingPackagesPanel()
        {
            if (missingPackagesPanel != null)
            {
                missingPackagesPanel.SetActive(false);
            }
        }

        public void DownloadAllMissingPackages()
        {
            if (missingPackages == null)
            {
                return;
            }
            foreach (HubResourcePackageUI missingPackage in missingPackages)
            {
                missingPackage.connectedItem.Download();
            }
        }

        public string GetPackageHubResourceId(string packageId)
        {
            string value = null;
            if (packageIdToResourceId != null)
            {
                packageIdToResourceId.TryGetValue(packageId, out value);
            }
            return value;
        }

        protected void GetPackagesJSONErrorCallback(string err)
        {
            //SuperController.LogError("Error during hub request for packages.json " + err);
        }

        protected void GetPackagesJSONCallback(SimpleJSON.JSONNode jsonNode)
        {
            if (!(jsonNode != null))
            {
                return;
            }
            JSONClass asObject = jsonNode.AsObject;
            if (!(asObject != null))
            {
                return;
            }
            packageGroupToLatestVersion = new Dictionary<string, int>();
            packageIdToResourceId = new Dictionary<string, string>();
            foreach (string key2 in asObject.Keys)
            {
                string text = Regex.Replace(key2, "\\.var$", string.Empty);
                string text2 = FileManager.PackageIDToPackageVersion(text);
                int result;
                if (text2 == null || !int.TryParse(text2, out result))
                {
                    continue;
                }
                string value = asObject[key2];
                packageIdToResourceId.Add(text, value);
                string key = FileManager.PackageIDToPackageGroupID(text);
                int value2;
                if (packageGroupToLatestVersion.TryGetValue(key, out value2))
                {
                    if (result > value2)
                    {
                        packageGroupToLatestVersion.Remove(key);
                        packageGroupToLatestVersion.Add(key, result);
                    }
                }
                else
                {
                    packageGroupToLatestVersion.Add(key, result);
                }
            }
        }

        protected void FindUpdatesErrorCallback(string err)
        {
            LogUtil.Log("Error during hub request " + err);
        }

        protected void FindUpdatesCallback(SimpleJSON.JSONNode jsonNode)
        {
            if (!(jsonNode != null))
            {
                return;
            }
            JSONClass asObject = jsonNode.AsObject;
            if (!(asObject != null))
            {
                return;
            }
            string text = asObject["status"];
            if (text != null && text == "error")
            {
                string text2 = jsonNode["error"];
                LogUtil.Log("findPackages returned error " + text2);
                return;
            }
            JSONClass asObject2 = jsonNode["packages"].AsObject;
            if (!(asObject2 != null))
            {
                return;
            }
            if (updates != null)
            {
                foreach (HubResourcePackageUI update in updates)
                {
                    UnityEngine.Object.Destroy(update.gameObject);
                }
                updates.Clear();
            }
            else
            {
                updates = new List<HubResourcePackageUI>();
            }
            foreach (string checkUpdateName in checkUpdateNames)
            {
                JSONClass jSONClass = asObject2[checkUpdateName].AsObject;
                if (jSONClass == null)
                {
                    jSONClass = new JSONClass();
                    jSONClass["filename"] = checkUpdateName;
                    jSONClass["downloadUrl"] = "null";
                }
                else
                {
                    string text3 = jSONClass["filename"];
                    if (text3 == null || text3 == "null")
                    {
                        jSONClass["filename"] = checkUpdateName;
                    }
                }
                HubResourcePackage hubResourcePackage = new HubResourcePackage(jSONClass, this, false);
                RectTransform rectTransform = CreateDownloadPrefabInstance();
                if (rectTransform != null)
                {
                    rectTransform.SetParent(updatesContainer, false);
                    HubResourcePackageUI component = rectTransform.GetComponent<HubResourcePackageUI>();
                    if (component != null)
                    {
                        updates.Add(component);
                        hubResourcePackage.RegisterUI(component);
                    }
                }
            }
        }

        public void OpenUpdatesPanel()
        {
            if (_hubEnabled)
            {
                if (!(updatesPanel != null) || !(updatesContainer != null))
                {
                    return;
                }
                Show();
                if (updatesPanel != null)
                {
                    updatesPanel.SetActive(true);
                }
                if (downloadQueue.Count != 0)
                {
                    return;
                }
                checkUpdateNames = new List<string>();
                if (packageGroupToLatestVersion != null)
                {
                    foreach (VarPackageGroup packageGroup in FileManager.GetPackageGroups())
                    {
                        int value;
                        if (packageGroupToLatestVersion.TryGetValue(packageGroup.Name, out value) && packageGroup.NewestVersion < value)
                        {
                            checkUpdateNames.Add(packageGroup.Name + ".latest");
                        }
                    }
                }
                if (checkUpdateNames.Count > 0)
                {
                    JSONClass jSONClass = new JSONClass();
                    jSONClass["source"] = "VaM";
                    jSONClass["action"] = "findPackages";
                    jSONClass["packages"] = string.Join(",", checkUpdateNames.ToArray());
                    string postData = jSONClass.ToString();
                    StartCoroutine(PostRequest(apiUrl, postData, FindUpdatesCallback, FindUpdatesErrorCallback));
                }
                else if (updates != null)
                {
                    foreach (HubResourcePackageUI update in updates)
                    {
                        UnityEngine.Object.Destroy(update.gameObject);
                    }
                    updates.Clear();
                }
                else
                {
                    updates = new List<HubResourcePackageUI>();
                }
            }
            else
            {
                LogUtil.Log("Cannot perform action. Hub is disabled in User Preferences");
            }
        }

        public void CloseUpdatesPanel()
        {
            if (updatesPanel != null)
            {
                updatesPanel.SetActive(false);
            }
        }

        public void DownloadAllUpdates()
        {
            if (updates == null)
            {
                return;
            }
            foreach (HubResourcePackageUI update in updates)
            {
                update.connectedItem.Download();
            }
        }

        protected void RefreshCookies()
        {
            if (GetBrowserCookiesRoutine == null && browser != null)
            {
                StartCoroutine(GetBrowserCookies());
            }
        }

        protected IEnumerator GetBrowserCookies()
        {
            if (hubCookies == null)
            {
                hubCookies = new List<string>();
            }
            while (!browser.IsReady)
            {
                yield return null;
            }
            IPromise<List<Cookie>> promise = browser.CookieManager.GetCookies();
            yield return promise.ToWaitFor(true);
            hubCookies.Clear();
            foreach (Cookie item in promise.Value)
            {
                if (item.domain == cookieHost)
                {
                    hubCookies.Add(string.Format("{0}={1}", item.name, item.value));
                }
            }
            GetBrowserCookiesRoutine = null;
        }

        protected IEnumerator DownloadRoutine()
        {
            while (true)
            {
                if (downloadQueue.Count > 0)
                {
                    isDownloadingJSON.val = true;
                    downloadQueuedCountJSON.val = "Queued: " + downloadQueue.Count;
                    DownloadRequest request = downloadQueue.Dequeue();
                    yield return BinaryGetRequest(request,request.url, request.startedCallback, request.successCallback, request.errorCallback, request.progressCallback, hubCookies);
                    if (downloadQueue.Count == 0)
                    {
                        var_browser.FileManager.Refresh();
                    }
                }
                else
                {
                    isDownloadingJSON.val = false;
                }
                yield return null;
            }
        }

        protected void OnPackageRefresh()
        {
            if (items != null)
            {
                foreach (HubResourceItemUI item in items)
                {
                    item.connectedItem.Refresh();
                }
            }
            if (missingPackages != null)
            {
                foreach (HubResourcePackageUI missingPackage in missingPackages)
                {
                    missingPackage.connectedItem.Refresh();
                }
            }
            if (updates != null)
            {
                foreach (HubResourcePackageUI update in updates)
                {
                    update.connectedItem.Refresh();
                }
            }
            if (resourceDetailStack == null)
            {
                return;
            }
            foreach (HubResourceItemDetailUI item2 in resourceDetailStack)
            {
                item2.connectedItem.Refresh();
            }
        }

        public DownloadRequest QueueDownload(string url, string promotionalUrl, BinaryRequestStartedCallback startedCallback, RequestProgressCallback progressCallback, BinaryRequestSuccessCallback successCallback, RequestErrorCallback errorCallback)
        {
            LogUtil.Log("QueueDownload "+url);
            DownloadRequest downloadRequest = new DownloadRequest();
            downloadRequest.url = url;
            if (downloadQueue.Count == 0)
            {
                downloadRequest.promotionalUrl = promotionalUrl;
            }
            downloadRequest.startedCallback = startedCallback;
            downloadRequest.progressCallback = progressCallback;
            downloadRequest.successCallback = successCallback;
            downloadRequest.errorCallback = errorCallback;
            downloadQueue.Enqueue(downloadRequest);
            return downloadRequest;
        }

        protected void OpenDownloading()
        {
            if (savedResourceDetailsPanels == null)
            {
                return;
            }


            List<string> shows = new List<string>();
            foreach (string key in savedResourceDetailsPanels.Keys)
            {
                shows.Add(key);
            }
            foreach (string key in shows)
            {
                OpenDetail(key);
            }
        }

        protected void GetInfoCallback(SimpleJSON.JSONNode jsonNode)
        {
            if (refreshingGetInfoPanel != null)
            {
                refreshingGetInfoPanel.gameObject.SetActive(false);
            }
            if (failedGetInfoPanel != null)
            {
                failedGetInfoPanel.gameObject.SetActive(false);
            }
            hubInfoCoroutine = null;
            hubInfoRefreshing = false;
            hubInfoSuccess = true;
            JSONClass asObject = jsonNode.AsObject;
            if (!(asObject != null))
            {
                return;
            }
            if (asObject["location"] != null)
            {
                JSONArray asArray = asObject["location"].AsArray;
                if (asArray != null)
                {
                    List<string> list = new List<string>();
                    list.Add("All");
                    IEnumerator enumerator = asArray.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            SimpleJSON.JSONNode jSONNode = (SimpleJSON.JSONNode)enumerator.Current;
                            list.Add(jSONNode);
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
                    hostedOptionChooser.choices = list;
                }
            }
            if (asObject["category"] != null)
            {
                JSONArray asArray2 = asObject["category"].AsArray;
                if (asArray2 != null)
                {
                    List<string> list2 = new List<string>();
                    list2.Add("All");
                    IEnumerator enumerator2 = asArray2.GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            SimpleJSON.JSONNode jSONNode2 = (SimpleJSON.JSONNode)enumerator2.Current;
                            list2.Add(jSONNode2);
                        }
                    }
                    finally
                    {
                        IDisposable disposable2;
                        if ((disposable2 = enumerator2 as IDisposable) != null)
                        {
                            disposable2.Dispose();
                        }
                    }
                    payTypeFilterChooser.choices = list2;
                }
            }
            if (asObject["type"] != null)
            {
                JSONArray asArray3 = asObject["type"].AsArray;
                if (asArray3 != null)
                {
                    List<string> list3 = new List<string>();
                    list3.Add("All");
                    IEnumerator enumerator3 = asArray3.GetEnumerator();
                    try
                    {
                        while (enumerator3.MoveNext())
                        {
                            SimpleJSON.JSONNode jSONNode3 = (SimpleJSON.JSONNode)enumerator3.Current;
                            list3.Add(jSONNode3);
                        }
                    }
                    finally
                    {
                        IDisposable disposable3;
                        if ((disposable3 = enumerator3 as IDisposable) != null)
                        {
                            disposable3.Dispose();
                        }
                    }
                    categoryFilterChooser.choices = list3;
                }
            }
            if (asObject["users"] != null)
            {
                JSONClass asObject2 = asObject["users"].AsObject;
                if (asObject2 != null)
                {
                    List<string> list4 = new List<string>();
                    list4.Add("All");
                    foreach (string key in asObject2.Keys)
                    {
                        list4.Add(key);
                    }
                    creatorFilterChooser.choices = list4;
                }
            }
            if (asObject["tags"] != null)
            {
                JSONClass asObject3 = asObject["tags"].AsObject;
                if (asObject3 != null)
                {
                    List<string> list5 = new List<string>();
                    list5.Add("All");
                    foreach (string key2 in asObject3.Keys)
                    {
                        list5.Add(key2);
                    }
                    tagsFilterChooser.choices = list5;
                }
            }
            if (asObject["sort"] != null)
            {
                JSONArray asArray4 = asObject["sort"].AsArray;
                if (asArray4 != null)
                {
                    List<string> list6 = new List<string>();
                    List<string> list7 = new List<string>();
                    list7.Add("None");
                    IEnumerator enumerator6 = asArray4.GetEnumerator();
                    try
                    {
                        while (enumerator6.MoveNext())
                        {
                            SimpleJSON.JSONNode jSONNode4 = (SimpleJSON.JSONNode)enumerator6.Current;
                            list6.Add(jSONNode4);
                            list7.Add(jSONNode4);
                        }
                    }
                    finally
                    {
                        IDisposable disposable4;
                        if ((disposable4 = enumerator6 as IDisposable) != null)
                        {
                            disposable4.Dispose();
                        }
                    }
                    sortPrimaryChooser.choices = list6;
                    sortSecondaryChooser.choices = list7;
                }
            }
            string text = asObject["last_update"];
            if (packagesJSONUrl != null && packagesJSONUrl != string.Empty && text != null)
            {
                string uri = packagesJSONUrl + "?" + text;
                StartCoroutine(GetRequest(uri, GetPackagesJSONCallback, GetPackagesJSONErrorCallback));
            }
        }

        protected void GetInfoErrorCallback(string err)
        {
            if (refreshingGetInfoPanel != null)
            {
                refreshingGetInfoPanel.gameObject.SetActive(false);
            }
            if (failedGetInfoPanel != null)
            {
                failedGetInfoPanel.gameObject.SetActive(true);
            }
            if (getInfoErrorText != null)
            {
                getInfoErrorText.text = err;
            }
            hubInfoCoroutine = null;
            hubInfoRefreshing = false;
            hubInfoSuccess = false;
        }

        protected void GetHubInfo()
        {
            if (!hubInfoRefreshing)
            {
                if (failedGetInfoPanel != null)
                {
                    failedGetInfoPanel.gameObject.SetActive(false);
                }
                JSONClass jSONClass = new JSONClass();
                jSONClass["source"] = "VaM";
                jSONClass["action"] = "getInfo";
                string postData = jSONClass.ToString();
                hubInfoRefreshing = true;
                if (refreshingGetInfoPanel != null)
                {
                    refreshingGetInfoPanel.gameObject.SetActive(true);
                }
                hubInfoCoroutine = StartCoroutine(PostRequest(apiUrl, postData, GetInfoCallback, GetInfoErrorCallback));
            }
        }

        protected void CancelGetHubInfo()
        {
            if (hubInfoRefreshing && hubInfoCoroutine != null)
            {
                StopCoroutine(hubInfoCoroutine);
                GetInfoErrorCallback("Cancelled");
            }
        }

        protected void Init()
        {
            LogUtil.Log("HubBrowse Init");
            hubEnabledJSON = new JSONStorableBool("hubEnabled", _hubEnabled, SyncHubEnabled);
            enableHubAction = new JSONStorableAction("EnableHub", EnableHub);
            webBrowserEnabledJSON = new JSONStorableBool("webBrowserEnabled", _webBrowserEnabled, SyncWebBrowserEnabled);
            enableWebBrowserAction = new JSONStorableAction("EnableWebBrowser", EnableWebBrowser);
            cancelGetHubInfoAction = new JSONStorableAction("CancelGetHubInfo", CancelGetHubInfo);
            retryGetHubInfoAction = new JSONStorableAction("RetryGetHubInfo", GetHubInfo);
            numResourcesJSON = new JSONStorableString("numResources", "0", SyncNumResources);
            pageInfoJSON = new JSONStorableString("pageInfo", "Page 0 of 0");
            numPagesJSON = new JSONStorableString("numPages", "0", SyncNumPages);
            currentPageJSON = new JSONStorableString("currentPage", "1", SyncCurrentPage);
            firstPageAction = new JSONStorableAction("FirstPage", FirstPage);
            previousPageAction = new JSONStorableAction("PreviousPage", PreviousPage);
            RegisterAction(previousPageAction);
            nextPageAction = new JSONStorableAction("NextPage", NextPage);
            RegisterAction(nextPageAction);
            refreshResourcesAction = new JSONStorableAction("RefreshResources", ResetRefresh);
            RegisterAction(refreshResourcesAction);
            clearFiltersAction = new JSONStorableAction("ResetFilters", ResetFiltersAndRefresh);
            RegisterAction(clearFiltersAction);

            List<string> list = new List<string>();
            list.Add("All");
            List<string> choicesList = list;
            hostedOptionChooser = new JSONStorableStringChooser("hostedOption", choicesList, _hostedOption, "Hosted Option", SyncHostedOption);
            hostedOptionChooser.isStorable = false;
            hostedOptionChooser.isRestorable = false;
            RegisterStringChooser(hostedOptionChooser);

            searchFilterJSON = new JSONStorableString("searchFilter", string.Empty, SyncSearchFilter);
            searchFilterJSON.enableOnChange = true;
            searchFilterJSON.isStorable = false;
            searchFilterJSON.isRestorable = false;
            RegisterString(searchFilterJSON);

            list = new List<string>();
            list.Add("All");
            List<string> choicesList2 = list;
            payTypeFilterChooser = new JSONStorableStringChooser("payType", choicesList2, _payTypeFilter, "Pay Type", SyncPayTypeFilter);
            payTypeFilterChooser.isStorable = false;
            payTypeFilterChooser.isRestorable = false;
            RegisterStringChooser(payTypeFilterChooser);
            list = new List<string>();
            list.Add("All");
            List<string> choicesList3 = list;
            categoryFilterChooser = new JSONStorableStringChooser("category", choicesList3, _categoryFilter, "Category", SyncCategoryFilter);
            categoryFilterChooser.isStorable = false;
            categoryFilterChooser.isRestorable = false;
            RegisterStringChooser(categoryFilterChooser);
            list = new List<string>();
            list.Add("All");
            List<string> choicesList4 = list;
            creatorFilterChooser = new JSONStorableStringChooser("creator", choicesList4, _creatorFilter, "Creator", SyncCreatorFilter);
            creatorFilterChooser.isStorable = false;
            creatorFilterChooser.isRestorable = false;
            RegisterStringChooser(creatorFilterChooser);
            list = new List<string>();
            list.Add("All");
            List<string> choicesList5 = list;
            tagsFilterChooser = new JSONStorableStringChooser("tags", choicesList5, _tagsFilter, "Tags", SyncTagsFilter);
            tagsFilterChooser.isStorable = false;
            tagsFilterChooser.isRestorable = false;
            RegisterStringChooser(tagsFilterChooser);
            list = new List<string>();
            list.Add("Latest Update");
            List<string> choicesList6 = list;
            sortPrimaryChooser = new JSONStorableStringChooser("sortPrimary", choicesList6, _sortPrimary, "Primary Sort", SyncSortPrimary);
            sortPrimaryChooser.isStorable = false;
            sortPrimaryChooser.isRestorable = false;
            RegisterStringChooser(sortPrimaryChooser);
            list = new List<string>();
            list.Add("None");
            List<string> choicesList7 = list;
            sortSecondaryChooser = new JSONStorableStringChooser("sortSecondary", choicesList7, _sortSecondary, "Secondary Sort", SyncSortSecondary);
            sortSecondaryChooser.isStorable = false;
            sortSecondaryChooser.isRestorable = false;
            RegisterStringChooser(sortSecondaryChooser);
            openMissingPackagesPanelAction = new JSONStorableAction("OpenMissingPackagesPanel", OpenMissingPackagesPanel);
            RegisterAction(openMissingPackagesPanelAction);
            closeMissingPackagesPanelAction = new JSONStorableAction("CloseMissingPackagesPanel", CloseMissingPackagesPanel);
            RegisterAction(closeMissingPackagesPanelAction);
            downloadAllMissingPackagesAction = new JSONStorableAction("DownloadAllMissingPackages", DownloadAllMissingPackages);
            RegisterAction(downloadAllMissingPackagesAction);
            openUpdatesPanelAction = new JSONStorableAction("OpenUpdatesPanel", OpenUpdatesPanel);
            RegisterAction(openUpdatesPanelAction);
            closeUpdatesPanelAction = new JSONStorableAction("CloseUpdatesPanel", CloseUpdatesPanel);
            RegisterAction(closeUpdatesPanelAction);
            downloadAllUpdatesAction = new JSONStorableAction("DownloadAllUpdates", DownloadAllUpdates);
            RegisterAction(downloadAllUpdatesAction);
            isDownloadingJSON = new JSONStorableBool("isDownloading", false);
            downloadQueuedCountJSON = new JSONStorableString("downloadQueuedCount", "Queued: 0");
            openDownloadingAction = new JSONStorableAction("OpenDownloading", OpenDownloading);
            RegisterAction(openDownloadingAction);
            resourceDetailStack = new Stack<HubResourceItemDetailUI>();
            savedResourceDetailsPanels = new Dictionary<string, HubResourceItemDetailUI>();
            downloadQueue = new Queue<DownloadRequest>();
            StartCoroutine(DownloadRoutine());

        }

        protected override void InitUI(Transform t, bool isAlt)
        {
            LogUtil.LogWarning("HubBrowse InitUI");
            if (t == null) return;
            MVR.Hub.HubBrowseUI componentInChildren = t.GetComponentInChildren<MVR.Hub.HubBrowseUI>(true);
            if (componentInChildren == null) return;
            if (!isAlt)
            {
                hubBrowseUI = componentInChildren;

                itemContainer = componentInChildren.itemContainer;
                itemScrollRect = componentInChildren.itemScrollRect;
                refreshingGetInfoPanel = componentInChildren.refreshingGetInfoPanel;
                if (refreshingGetInfoPanel != null && hubInfoRefreshing)
                {
                    refreshingGetInfoPanel.gameObject.SetActive(true);
                }
                failedGetInfoPanel = componentInChildren.failedGetInfoPanel;
                if (failedGetInfoPanel != null && !hubInfoSuccess && !hubInfoRefreshing)
                {
                    failedGetInfoPanel.gameObject.SetActive(true);
                }
                getInfoErrorText = componentInChildren.getInfoErrorText;
                detailPanel = componentInChildren.detailPanel;
                resourceDetailContainer = componentInChildren.resourceDetailContainer;
                browser = componentInChildren.browser;
                webBrowser = componentInChildren.webBrowser;
                isWebLoadingIndicator = componentInChildren.isWebLoadingIndicator;
                refreshIndicator = componentInChildren.refreshIndicator;
                missingPackagesPanel = componentInChildren.missingPackagesPanel;
                missingPackagesContainer = componentInChildren.missingPackagesContainer;
                updatesPanel = componentInChildren.updatesPanel;
                updatesContainer = componentInChildren.updatesContainer;
            }
            //LogUtil.LogWarning("hubEnabledJSON RegisterNegativeIndicator");
            hubEnabledJSON.RegisterNegativeIndicator(componentInChildren.hubEnabledNegativeIndicator, isAlt);
            //LogUtil.LogWarning("enableHubAction RegisterButton");
            enableHubAction.RegisterButton(componentInChildren.enableHubButton, isAlt);
            //LogUtil.LogWarning("webBrowserEnabledJSON.RegisterNegativeIndicator");
            webBrowserEnabledJSON.RegisterNegativeIndicator(componentInChildren.webBrowserEnabledNegativeIndicator, isAlt);
            //LogUtil.LogWarning("enableWebBrowserAction.RegisterButton");
            enableWebBrowserAction.RegisterButton(componentInChildren.enabledWebBrowserButton, isAlt);
            //LogUtil.LogWarning("cancelGetHubInfoAction.RegisterButton");
            cancelGetHubInfoAction.RegisterButton(componentInChildren.cancelGetHubInfoButton, isAlt);
            //LogUtil.LogWarning("retryGetHubInfoAction.RegisterButton");
            retryGetHubInfoAction.RegisterButton(componentInChildren.retryGetHubInfoButton, isAlt);
            pageInfoJSON.RegisterText(componentInChildren.pageInfoText, isAlt);
            numResourcesJSON.RegisterText(componentInChildren.numResourcesText, isAlt);
            firstPageAction.RegisterButton(componentInChildren.firstPageButton, isAlt);
            previousPageAction.RegisterButton(componentInChildren.previousPageButton, isAlt);
            nextPageAction.RegisterButton(componentInChildren.nextPageButton, isAlt);
            refreshResourcesAction.RegisterButton(componentInChildren.refreshButton, isAlt);
            clearFiltersAction.RegisterButton(componentInChildren.clearFiltersButton, isAlt);

            //LogUtil.LogWarning("hostedOptionPopup RegisterPopup");
            try
            {
                componentInChildren.hostedOptionPopup.useFiltering = false;
                componentInChildren.hostedOptionPopup.numPopupValues = 0;
                hostedOptionChooser.RegisterPopup(componentInChildren.hostedOptionPopup, isAlt);
            }
            catch(Exception e)
            {
                //LogUtil.LogError("hostedOptionPopup " + e.ToString());
            }


            searchFilterJSON.RegisterInputField(componentInChildren.searchInputField, isAlt);

            //LogUtil.LogWarning("payTypeFilterChooser RegisterPopup");
            try
            {
                componentInChildren.payTypeFilterPopup.useFiltering = false;
                componentInChildren.payTypeFilterPopup.numPopupValues = 0;
                payTypeFilterChooser.RegisterPopup(componentInChildren.payTypeFilterPopup, isAlt);
            }
            catch(Exception e)
            {
                LogUtil.LogError("payTypeFilterPopup " + e.ToString());
            }

            //LogUtil.LogWarning("categoryFilterChooser RegisterPopup");
            try
            {
                componentInChildren.categoryFilterPopup.useFiltering = false;
                componentInChildren.categoryFilterPopup.numPopupValues = 0;
                categoryFilterChooser.RegisterPopup(componentInChildren.categoryFilterPopup, isAlt);
            }
            catch (Exception e)
            {
                LogUtil.LogError("categoryFilterPopup " + e.ToString());
            }
            //LogUtil.LogWarning("creatorFilterChooser RegisterPopup");
            try
            {
                componentInChildren.creatorFilterPopup.useFiltering = false;
                componentInChildren.creatorFilterPopup.numPopupValues = 0;
                creatorFilterChooser.RegisterPopup(componentInChildren.creatorFilterPopup, isAlt);
            }
            catch (Exception e)
            {
                LogUtil.LogError("creatorFilterPopup " + e.ToString());
            }
            //LogUtil.LogWarning("tagsFilterChooser RegisterPopup");
            try
            {
                componentInChildren.tagsFilterPopup.useFiltering = false;
                componentInChildren.tagsFilterPopup.numPopupValues = 0;
                tagsFilterChooser.RegisterPopup(componentInChildren.tagsFilterPopup, isAlt);

            }
            catch (Exception e)
            {
                LogUtil.LogError("tagsFilterPopup " + e.ToString());
            }
            //LogUtil.LogWarning("sortPrimaryChooser RegisterPopup");
            try
            {
                componentInChildren.sortPrimaryPopup.useFiltering = false;
                componentInChildren.sortPrimaryPopup.numPopupValues = 0;
                sortPrimaryChooser.RegisterPopup(componentInChildren.sortPrimaryPopup, isAlt);
            }
            catch (Exception e)
            {
                LogUtil.LogError("sortPrimaryPopup " + e.ToString());
            }
            //LogUtil.LogWarning("sortSecondaryChooser RegisterPopup");
            try
            {
                componentInChildren.sortSecondaryPopup.useFiltering = false;
                componentInChildren.sortSecondaryPopup.numPopupValues = 0;
                sortSecondaryChooser.RegisterPopup(componentInChildren.sortSecondaryPopup, isAlt);
            }
            catch (Exception e)
            {
                LogUtil.LogError("sortSecondaryPopup " + e.ToString());
            }
            openMissingPackagesPanelAction.RegisterButton(componentInChildren.openMissingPackagesPanelButton, isAlt);
            closeMissingPackagesPanelAction.RegisterButton(componentInChildren.closeMissingPackagesPanelButton, isAlt);
            downloadAllMissingPackagesAction.RegisterButton(componentInChildren.downloadAllMissingPackagesButton, isAlt);
            openUpdatesPanelAction.RegisterButton(componentInChildren.openUpdatesPanelButton, isAlt);
            closeUpdatesPanelAction.RegisterButton(componentInChildren.closeUpdatesPanelButton, isAlt);
            downloadAllUpdatesAction.RegisterButton(componentInChildren.downloadAllUpdatesButton, isAlt);
            isDownloadingJSON.RegisterIndicator(componentInChildren.isDownloadingIndicator, isAlt);
            downloadQueuedCountJSON.RegisterText(componentInChildren.downloadQueuedCountText, isAlt);
            openDownloadingAction.RegisterButton(componentInChildren.openDownloadingButton, isAlt);

            LogUtil.Log("HubBrowse Init End");
        }

        protected void OnLoad(ZenFulcrum.EmbeddedBrowser.JSONNode loadData)
        {
            browser.EvalJS("\r\n\t\t\t\twindow.scrollTo(0,0);\r\n\t\t\t");
            RefreshCookies();
        }

        protected override void Awake()
        {
            if (!awakecalled)
            {
                singleton = this;
                base.Awake();
                Init();
                InitUI();
                InitUIAlt();
                if (browser != null)
                {
                    browser.onLoad += OnLoad;
                }
            }
        }
        void OnDestroy()
        {
            singleton = null;
        }
        public void Prepare()
        {
            Init();
            InitUI();
            InitUIAlt();
        }

        protected void Update()
        {
            if (browser != null && isWebLoadingIndicator != null)
            {
                isWebLoadingIndicator.SetActive(browser.IsLoadingRaw);
            }
        }
    }
}
