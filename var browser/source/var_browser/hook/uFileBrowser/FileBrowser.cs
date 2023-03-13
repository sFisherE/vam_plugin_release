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
		protected bool _onlyAutoInstall;
		public Toggle showAutoInstallToggle;
		public void InitUI(uFileBrowser.FileBrowser ui)
        {
			this.sortByPopup = ui.sortByPopup;
			this.overlay = ui.overlay;
			this.window = ui.window;
			this.fileButtonPrefab = ui.fileButtonPrefab;
			this.titleText = ui.titleText;
			this.fileContent = ui.fileContent;
			this.filesScrollRect = ui.filesScrollRect;
			this.showHiddenToggle = ui.showHiddenToggle;


            var newgo = GameObject.Instantiate(ui.showHiddenToggle.gameObject, ui.showHiddenToggle.transform.parent);
            Vector3 oldPos = ui.showHiddenToggle.transform.localPosition;
            newgo.transform.localPosition = new Vector3(oldPos.x - 280, oldPos.y, oldPos.z);
            this.showAutoInstallToggle = newgo.GetComponent<Toggle>();


            this.onlyFavoritesToggle = ui.onlyFavoritesToggle;
			this.limitSlider = ui.limitSlider;
			this.limitValueText = ui.limitValueText;

			this.showingCountText = ui.showingCountText;
			this.searchField = ui.searchField;
			this.searchField.onValueChanged.RemoveAllListeners();
			this.searchField.onValueChanged.AddListener(v =>
			{
				this.SearchChanged();
			});


			this.searchCancelButton = ui.searchCancelButton;
			this.searchCancelButton.onClick.RemoveAllListeners();
			this.searchCancelButton.onClick.AddListener(() =>
			{
				this.SearchCancelClick();
			});

			this.cancelButton = ui.cancelButton;
			this.cancelButton.onClick.RemoveAllListeners();
			this.cancelButton.onClick.AddListener(() =>
			{
				//this.CancelButtonClicked();
				Hide();
				//把top界面关掉
				SuperController.singleton.DeactivateWorldUI();
			});
			//this.renameContainer = ui.renameContainer;

			var firstPageButton = this.transform.Find("MainPanel/ShowingGroup/FirstPageButton").GetComponent<Button>();
			firstPageButton.onClick.RemoveAllListeners();
			firstPageButton.onClick.AddListener(() =>
			{
				FirstPage();
			});
			var leftButton = this.transform.Find("MainPanel/ShowingGroup/LeftButton").GetComponent<Button>();
			leftButton.onClick.RemoveAllListeners();
			leftButton.onClick.AddListener(() =>
			{
				PrevPage();
			});

			var rightButton = this.transform.Find("MainPanel/ShowingGroup/RightButton").GetComponent<Button>();
			rightButton.onClick.RemoveAllListeners();
			rightButton.onClick.AddListener(() =>
			{
				NextPage();
			});

			this.fileHighlightField = ui.fileHighlightField;
			this.folderIcon = ui.folderIcon;
			this.defaultIcon = ui.defaultIcon;

        }

		public class FileAndDirInfo
		{
			protected bool _isWriteable;
			protected bool _isDirectory;
			public FileButton button;
			public bool isWriteable
			{
				get
				{
					return _isWriteable;
				}
			}

			public bool isTemplate
			{
				get
				{
					if (FileEntry != null)
					{
						return FileEntry.HasFlagFile("template");
					}
					return false;
				}
			}
			public bool isTemplateModifiable
			{
				get
				{
					if (FileEntry != null)
					{
						VarFileEntry varFileEntry = FileEntry as VarFileEntry;
						if (varFileEntry != null)
						{
							return varFileEntry.IsFlagFileModifiable("template");
						}
						return true;
					}
					return true;
				}
			}
			public bool isFavorite
			{
				get
				{
					if (FileEntry != null)
					{
						return FileEntry.IsFavorite();
					}
					return false;
				}
			}
			public bool isInstalled
			{
				get
				{
					if (FileEntry != null)
					{
						return FileEntry.IsInstalled();
					}
					return false;
				}
			}
			public bool isAutoInstall
            {
                get
                {
					if (FileEntry != null)
					{
						return FileEntry.IsAutoInstall();
					}
					return false;
				}
            }
			public bool isHidden
			{
				get
				{
					if (FileEntry != null)
					{
						return FileEntry.IsHidden();
					}
					if (DirectoryEntry != null)
					{
						return DirectoryEntry.IsHidden();
					}
					return false;
				}
			}
			public bool isHiddenModifiable
			{
				get
				{
					if (FileEntry != null)
					{
						VarFileEntry varFileEntry = FileEntry as VarFileEntry;
						if (varFileEntry != null)
						{
							return varFileEntry.IsHiddenModifiable();
						}
						return true;
					}
					return true;
				}
			}
			public bool isDirectory
			{
				get
				{
					return _isDirectory;
				}
			}
			public FileEntry FileEntry { get; protected set; }
			public DirectoryEntry DirectoryEntry { get; protected set; }
			public string Name { get; protected set; }
			public string FullName { get; protected set; }
			public DateTime LastWriteTime { get; protected set; }
			public DateTime LastWriteTimePackage { get; protected set; }

			public FileAndDirInfo(FileEntry fEntry)
			{
				FileEntry = fEntry;
				_isDirectory = false;
				_isWriteable = !(fEntry is VarFileEntry);// && FileManager.IsSecureWritePath(fEntry.FullPath);
				Name = fEntry.Name;
				FullName = fEntry.Uid;
				LastWriteTime = fEntry.LastWriteTime;
				VarFileEntry varFileEntry = fEntry as VarFileEntry;
				if (varFileEntry != null)
				{
					LastWriteTimePackage = varFileEntry.Package.LastWriteTime;
				}
				else
				{
					LastWriteTimePackage = LastWriteTime;
				}
			}

			//public void SetTemplate(bool b)
			//{
			//	if (_isWriteable && FileEntry != null)
			//	{
			//		FileEntry.SetFlagFile("template", b);
			//	}
			//}

			public void SetFavorite(bool b)
			{
				if (_isWriteable && FileEntry != null)
				{
					FileEntry.SetFavorite(b);
				}
			}

			public void SetHidden(bool b)
			{
				if (_isWriteable && FileEntry != null)
				{
					FileEntry.SetHidden(b);
				}
			}
		}


		public string defaultPath = string.Empty;

		public bool selectDirectory;

		public bool canCancel = true;

		public bool selectOnClick = true;

		public bool browseVarFilesAsDirectories = true;

		public bool showInstallFolderInDirectoryList;

		//只显示模板
		public bool forceOnlyShowTemplates;

		public bool allowUseFileAsTemplateSelect;

		//场景的肯定都是json文件
		public string fileFormat = string.Empty;

		public bool hideExtension;

		public string fileRemovePrefix;

		[SerializeField]
		[HideInInspector]
		private string currentPath;

		[SerializeField]
		[HideInInspector]
		private string search;

		[HideInInspector]
		private string searchLower;

		[SerializeField]
		[HideInInspector]
		private string slash;

		[SerializeField]
		//[HideInInspector]
		private List<string> drives;

		private List<DirectoryButton> dirButtons;

		private List<ShortCutButton> shortCutButtons;

		private List<GameObject> dirSpacers;

		private FileButton selected;

		private FileBrowserCallback callback;

		private FileBrowserFullCallback fullCallback;

		//public List<ShortCut> shortCuts;

		public bool manageContentTransform = true;

		public Vector2 cellSize=new Vector2(370,420);

		public Vector2 cellSpacing=new Vector2(5,5);

		public int columnCount=6;

		public UIPopup directoryOptionPopup;

		protected UserPreferences.DirectoryOption _directoryOption;

		public UIPopup sortByPopup;

		protected UserPreferences.SortBy _sortBy = UserPreferences.SortBy.NewToOld;

		public GameObject overlay;

		public GameObject window;

		public GameObject fileButtonPrefab;

		public GameObject directoryButtonPrefab;

		public GameObject directorySpacerPrefab;

		public Text titleText;

		public RectTransform fileContent;

		public ScrollRect filesScrollRect;

		public RectTransform dirContent;

		public RectTransform dirOption;

		public GameObject shortCutButtonPrefab;

		public RectTransform shortCutContent;

		public Button openPackageButton;

		public Button openOnHubButton;

		public Button promotionalButton;

		public Text promotionalButtonText;

		public Toggle keepOpenToggle;

		[SerializeField]
		protected bool _keepOpen;

		public Toggle onlyShowLatestToggle;

		protected bool _onlyShowLatest = true;

		public Toggle showHiddenToggle;

		protected bool _showHidden;

		public Toggle onlyFavoritesToggle;

		protected bool _onlyFavorites;

		public Toggle onlyInstalledToggle;

		protected bool _onlyInstalled;

		public Toggle onlyTemplatesToggle;

		protected bool _onlyTemplates;

		protected int _totalPages;

		protected int _page = 1;

		public Slider limitSlider;

		public Text limitValueText;

		[SerializeField]
		protected int _limit = 25;

		public int limitMultiple = 18;

		protected int _limitXMultiple = 500;

		public Text showingCountText;

		public InputField currentPathField;

		public InputField searchField;

		public Button searchCancelButton;

		public Button cancelButton;

		public Button selectButton;

		public Text selectButtonText;

		public Transform renameContainer;

		public InputField renameField;

		public InputFieldAction renameFieldAction;

		protected FileButton renameFileButton;

		public Transform deleteContainer;

		public InputField deleteField;

		protected FileButton deleteFileButton;

		public Text statusField;

		public Text fileHighlightField;

		public InputField fileEntryField;

		public Sprite folderIcon;

		public Sprite defaultIcon;

		public List<FileIcon> fileIcons = new List<FileIcon>();

		private HashSet<var_browser.CustomImageLoaderThreaded.QueuedImage> queuedThumbnails;

		public bool clearCurrentPathOnHide = true;

		protected Dictionary<string, float> directoryScrollPositions;
		protected Dictionary<string, int> browserPage;

		protected string currentPackageUid;

		protected string currentPackageFilter;

		protected bool useFlatten;

		protected bool includeRegularDirsInFlatten;

		protected bool ignoreSearchChange;

		protected List<FileAndDirInfo> sortedFilesAndDirs;

		protected HashSet<FileButton> displayedFileButtons;

		protected bool cacheDirty = true;

		protected string lastCacheDir;

		public bool lastCacheInGame = false;

		protected bool lastCacheUseFlatten;

		protected bool lastCacheIncludeRegularDirsInFlatten;

		//protected bool lastCacheShowDirs;

		protected bool lastCacheForceOnlyShowTemplates;

		protected DateTime lastCacheTime;

		protected string lastCacheFileFormat;

		protected string lastCacheFileRemovePrefix;

		protected string lastCachePackageFilter;

		protected List<FileAndDirInfo> cachedFiles;

		//protected List<FileAndDirInfo> cachedDirs;

		protected bool threadHadException;

		protected string threadException;

		public bool inGame;

		public string SelectedPath
		{
			get
			{
				if (selected != null)
				{
					return selected.fullPath;
				}
				return null;
			}
		}

		public UserPreferences.DirectoryOption directoryOption
		{
			get
			{
				return _directoryOption;
			}
			set
			{
				if (_directoryOption != value)
				{
					_directoryOption = value;
					if (directoryOptionPopup != null)
					{
						directoryOptionPopup.currentValueNoCallback = _directoryOption.ToString();
					}
					SyncSort();
				}
			}
		}

		protected UserPreferences.DirectoryOption directoryOptionNoSync
		{
			get
			{
				return _directoryOption;
			}
			set
			{
				if (_directoryOption != value)
				{
					_directoryOption = value;
					if (directoryOptionPopup != null)
					{
						directoryOptionPopup.currentValueNoCallback = _directoryOption.ToString();
					}
				}
			}
		}

		public UserPreferences.SortBy sortBy
		{
			get
			{
				return _sortBy;
			}
			set
			{
				if (_sortBy != value)
				{
					_sortBy = value;
					if (sortByPopup != null)
					{
						sortByPopup.currentValueNoCallback = _sortBy.ToString();
					}
					SyncSort();
				}
			}
		}

		protected UserPreferences.SortBy sortByNoSync
		{
			get
			{
				return _sortBy;
			}
			set
			{
				if (_sortBy != value)
				{
					_sortBy = value;
					if (sortByPopup != null)
					{
						sortByPopup.currentValueNoCallback = _sortBy.ToString();
					}
				}
			}
		}

		public bool keepOpen
		{
			get
			{
				return _keepOpen;
			}
			set
			{
				if (_keepOpen != value)
				{
					_keepOpen = value;
					if (keepOpenToggle != null)
					{
						keepOpenToggle.isOn = _keepOpen;
					}
				}
			}
		}

		public bool onlyShowLatest
		{
			get
			{
				return _onlyShowLatest;
			}
			set
			{
				if (_onlyShowLatest != value)
				{
					_onlyShowLatest = value;
					if (onlyShowLatestToggle != null)
					{
						onlyShowLatestToggle.isOn = _onlyShowLatest;
					}
					UpdateDirectoryList();
				}
			}
		}

		public bool showHidden
		{
			get
			{
				return _showHidden;
			}
			set
			{
				if (_showHidden != value)
				{
					_showHidden = value;
					if (showHiddenToggle != null)
					{
						showHiddenToggle.isOn = _showHidden;
					}
					//UpdateDirectoryList();
					ResetDisplayedPage();
				}
			}
		}

		public bool onlyFavorites
		{
			get
			{
				return _onlyFavorites;
			}
			set
			{
				if (_onlyFavorites != value)
				{
					_onlyFavorites = value;
					if (onlyFavoritesToggle != null)
					{
						onlyFavoritesToggle.isOn = _onlyFavorites;
					}
					ResetDisplayedPage();
				}
			}
		}

		public bool onlyInstalled
		{
			get
			{
				return _onlyInstalled;
			}
			set
			{
				if (_onlyInstalled != value)
				{
					_onlyInstalled = value;
					if (onlyInstalledToggle != null)
					{
						onlyInstalledToggle.isOn = _onlyInstalled;
					}
					ResetDisplayedPage();
				}
			}
		}
		public bool onlyAutoInstall
		{
			get
			{
				return _onlyAutoInstall;
			}
			set
			{
				if (_onlyAutoInstall != value)
				{
					_onlyAutoInstall = value;
					if (showAutoInstallToggle != null)
					{
						showAutoInstallToggle.isOn = _onlyAutoInstall;
					}
					ResetDisplayedPage();
				}
			}
		}

		public bool onlyTemplates
		{
			get
			{
				return _onlyTemplates;
			}
			set
			{
				if (_onlyTemplates != value)
				{
					_onlyTemplates = value;
					if (onlyTemplatesToggle != null)
					{
						onlyTemplatesToggle.isOn = _onlyTemplates;
					}
					ResetDisplayedPage();
				}
			}
		}

		public int page
		{
			get
			{
				return _page;
			}
			set
			{
				if (_page != value && value <= _totalPages && value > 0)
				{
					_page = value;
					StartCoroutine(DelaySetScroll(1f));
					SyncDisplayed();
				}
			}
		}

		public int limit
		{
			get
			{
				return _limit;
			}
			set
			{
				if (_limit != value)
				{
					_limit = value;
					_limitXMultiple = _limit * limitMultiple;
					if (limitValueText != null)
					{
						limitValueText.text = _limitXMultiple.ToString("F0");
					}
					if (limitSlider != null)
					{
						limitSlider.value = _limit;
					}
					ResetDisplayedPage();
				}
			}
		}

		public void ClearCacheImage(string imgPath)
		{
			if (var_browser.CustomImageLoaderThreaded.singleton != null)
			{
				var_browser.CustomImageLoaderThreaded.singleton.ClearCacheThumbnail(imgPath);
			}
		}

		public void ClearImageQueue()
		{
			foreach (var_browser.CustomImageLoaderThreaded.QueuedImage queuedThumbnail in queuedThumbnails)
			{
				queuedThumbnail.cancel = true;
			}
			queuedThumbnails.Clear();
		}


		public void SetDirectoryOption(string dirOptionString)
		{
		}

		public void SetSortBy(string sortByString)
		{
			try
			{
				UserPreferences.SortBy fileBrowserSortBy = this.sortBy = (UserPreferences.SortBy)Enum.Parse(typeof(UserPreferences.SortBy), sortByString);
				if (UserPreferences.singleton != null)
				{
					//UserPreferences.singleton.fileBrowserSortBy = fileBrowserSortBy;
				}
			}
			catch (ArgumentException)
			{
				LogUtil.LogError("Attempted to set sort by to " + sortByString + " which is not a valid type");
			}
		}

		private void SortFilesAndDirs(List<FileAndDirInfo> fdlist)
		{
			switch (sortBy)
			{
				case UserPreferences.SortBy.AtoZ:
					fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => a.Name.CompareTo(b.Name));
					break;
				case UserPreferences.SortBy.ZtoA:
					fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => b.Name.CompareTo(a.Name));
					break;
				case UserPreferences.SortBy.NewToOld:
					fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => b.LastWriteTime.CompareTo(a.LastWriteTime));
					break;
				case UserPreferences.SortBy.OldToNew:
					fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => a.LastWriteTime.CompareTo(b.LastWriteTime));
					break;
				case UserPreferences.SortBy.NewToOldPackage:
					fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => b.LastWriteTimePackage.CompareTo(a.LastWriteTimePackage));
					break;
				case UserPreferences.SortBy.OldToNewPackage:
					fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => a.LastWriteTimePackage.CompareTo(b.LastWriteTimePackage));
					break;
			}
		}

		protected void SetShowHidden(bool b)
		{
			showHidden = b;
		}

		protected void SetOnlyFavorites(bool b)
		{
			onlyFavorites = b;
		}
		protected void SetOnlyInstalled(bool b)
		{
			onlyInstalled = b;
		}
		protected void SetOnlyAutoInstall(bool b)
        {
			onlyAutoInstall = b;
        }

		protected void SetOnlyTemplates(bool b)
		{
			onlyTemplates = b;
		}

		protected void ResetDisplayedPage()
		{
			_page = 1;
			_totalPages = 1;
			StartCoroutine(DelaySetScroll(1f));
			SyncDisplayed();
		}

		public void FirstPage()
		{
			page = 1;
		}

		public void NextPage()
		{
			page++;
		}

		public void PrevPage()
		{
			page--;
		}

		protected void SetLimit(float f)
		{
			limit = Mathf.FloorToInt(f);
		}

		protected FileButton CreateFileButton(string text, string path, bool dir, bool writeable, bool hidden, bool hiddenModifiable, bool favorite,bool isAutoInstall, bool isTemplate, bool isTemplateModifiable)
		{
			FileButton component = null;
			GameObject gameObject = PoolManager.SpawnObject(fileButtonPrefab);
			//GameObject gameObject = UnityEngine.Object.Instantiate(fileButtonPrefab, Vector3.zero, Quaternion.identity);
			var component2 = gameObject.GetComponent<uFileBrowser.FileButton>();
            if (component2 != null)
            {
				component = gameObject.AddComponent<FileButton>();
				component.InitUI(component2);
				Component.DestroyImmediate(component2);
			}
			component = gameObject.GetComponent<FileButton>();

			string text2 = text;
			if (hideExtension)
			{
				text2 = Regex.Replace(text2, "\\.[^\\.]*$", string.Empty);
			}
			component.Set(this, text2, path, dir, hidden, hiddenModifiable, favorite, isAutoInstall, allowUseFileAsTemplateSelect, allowUseFileAsTemplateSelect && isTemplate, isTemplateModifiable);
			if (CustomImageLoaderThreaded.singleton != null)
			{
				Transform transform = null;
				if (component.fileIcon != null)
				{
					transform = component.fileIcon.transform;
				}
				Transform transform2 = null;
				if (component.altIcon != null)
				{
					transform2 = component.altIcon.transform;
				}
				if (transform != null)
				{
					transform.gameObject.SetActive(true);
					if (transform2 != null)
					{
						transform2.gameObject.SetActive(false);
						RawImage altIcon = component.altIcon;
						if (altIcon != null)
						{
							FileEntry fileEntry = FileManager.GetFileEntry(path);
							if (fileEntry != null)
							{
								string text4 = Path.GetExtension(fileEntry.Path);
                                string imgPath;
                                switch (text4)
                                {
                                    case ".duf":
                                        imgPath = fileEntry.Path + ".png";
                                        text4 = ".png";
                                        break;
									//case ".vmi"://角色变形
									case ".json":
                                    case ".vac":
                                    case ".vap":
                                    case ".vam":
                                    case ".scene":
                                    case ".assetbundle":
                                        imgPath = Regex.Replace(fileEntry.Path, "\\.(json|vac|vap|vam|scene|assetbundle)$", ".jpg");
                                        text4 = ".jpg";
                                        break;
                                    default:
                                        imgPath = fileEntry.Path;
                                        break;
                                }
                                string text6 = text4.ToLower();
								if (FileManager.FileExists(imgPath))
								{
                                    switch (text6)
                                    {
                                        case ".jpg":
                                        case ".jpeg":
                                        case ".png":
                                        case ".tif":
                                            {
                                                if (string.IsNullOrEmpty(imgPath))
                                                {
													LogUtil.Log(fileEntry.Path);
                                                }
                                                component.imgPath = imgPath;
                                                transform.gameObject.SetActive(false);
                                                transform2.gameObject.SetActive(true);
                                                Texture2D cachedThumbnail = CustomImageLoaderThreaded.singleton.GetCachedThumbnail(imgPath);
                                                if (cachedThumbnail != null)
                                                {
                                                    altIcon.texture = cachedThumbnail;
                                                }
                                                break;
                                            }
                                    }
                                }
							}
						}
					}
				}
			}
			return component;
		}

        //private void SyncFileButtonImages()
        //{
        //    foreach (FileButton displayedFileButton in displayedFileButtons)
        //    {
        //        SyncFileButtonImage(displayedFileButton);
        //    }
        //}

        public void SyncFileButtonImage(FileButton fb)
		{
			if (fb.imgPath != null && fb.altIcon != null)
			{
				Texture2D cachedThumbnail = CustomImageLoaderThreaded.singleton.GetCachedThumbnail(fb.imgPath);
				if (cachedThumbnail != null)
				{
					fb.altIcon.texture = cachedThumbnail;
					return;
				}

				fb.altIcon.texture = null;

				CustomImageLoaderThreaded.QueuedImage queuedImage = new CustomImageLoaderThreaded.QueuedImage();
				queuedImage.imgPath = fb.imgPath;
				queuedImage.width = 512;
				queuedImage.height = 512;
				queuedImage.setSize = true;
				queuedImage.fillBackground = true;
				queuedImage.rawImageToLoad = fb.altIcon;
				CustomImageLoaderThreaded.singleton.QueueThumbnail(queuedImage);
				queuedThumbnails.Add(queuedImage);
			}
		}


		public void SetTitle(string title)
		{
			if (titleText != null)
			{
				titleText.text = title;
			}
		}

		protected void ShowInternal(bool changeDirectory = true)
		{
			if (statusField != null)
			{
				statusField.text = string.Empty;
			}
			if (fileEntryField != null)
			{
				fileEntryField.text = string.Empty;
			}
			if ((bool)overlay)
			{
				overlay.SetActive(true);
			}
			window.SetActive(true);

			if (changeDirectory)
			{
				GotoDirectory(defaultPath);
			}
			UpdateUI();

		}

		public void Show(string _fileFormat,string _defaultPath,FileBrowserCallback callback, bool changeDirectory = true,bool inGame=false)
		{
			//是否显示cloth tag过滤
			if(_fileFormat== "vam" && _defaultPath == "Custom/Clothing")
            {
				SetClothTagsActive(true);
            }
            else
            {
				SetClothTagsActive(false);
            }
			if (_fileFormat == "vam" && _defaultPath == "Custom/Hair")
			{
				SetHairTagsActive(true);
			}
			else
			{
				SetHairTagsActive(false);
			}

			SaveDirectoryScrollPos();

			fileFormat = _fileFormat;
			defaultPath = _defaultPath;
			window.SetActive(true);
			this.callback = callback;
			fullCallback = null;
			this.inGame = inGame;
            if (this.inGame)
            {
                creatorPopup.gameObject.SetActive(false);
				//把弹出的popup隐藏掉
				creatorPopup.GetComponent<UIPopup>().visible = false;
			}
            else
            {
				creatorPopup.gameObject.SetActive(true);
                creatorPopup.GetComponent<UIPopup>().visible = false;
			}
			ShowInternal(changeDirectory);
        }
		public void ClearCurrentPath()
		{
			ClearDirectoryScrollPos();
			currentPath = string.Empty;
		}
		public void Hide()
		{
			LogUtil.LogWarning("FileBrowser Hide");
			if (window.activeSelf)
			{
				if (selected != null)
				{
					selected.Unselect();
				}
				ClearImageQueue();
				SaveDirectoryScrollPos();
				if (clearCurrentPathOnHide)
				{
					currentPath = string.Empty;
				}
				selected = null;
				if ((bool)overlay)
				{
					overlay.SetActive(false);
				}
				window.SetActive(false);
			}
		}

		public bool IsHidden()
		{
			return !window.activeSelf;
		}

		public void UpdateUI()
		{
			if ((bool)cancelButton)
			{
				cancelButton.gameObject.SetActive(canCancel);
			}
			if (currentPathField != null)
			{
				currentPathField.text = currentPath;
			}
			if (searchField != null)
			{
				searchField.text = search;
			}
		}

		public Sprite GetFileIcon(string path)
		{
			string empty = string.Empty;
			if (path.Contains("."))
			{
				empty = path.Substring(path.LastIndexOf('.') + 1);
				for (int i = 0; i < fileIcons.Count; i++)
				{
					if (fileIcons[i].extension == empty)
					{
						return fileIcons[i].icon;
					}
				}
				return defaultIcon;
			}
			return defaultIcon;
		}

		public void OnHiddenChange(FileButton fb, bool b)
		{
			string fullPath = fb.fullPath;
			FileEntry fileEntry = FileManager.GetFileEntry(fullPath, true);
			if (fileEntry != null)
			{
				fileEntry.SetHidden(b);
			}
		}

		public void OnFavoriteChange(FileButton fb, bool b)
		{
			string fullPath = fb.fullPath;
			FileEntry fileEntry = FileManager.GetFileEntry(fullPath, true);
			if (fileEntry != null)
			{
				fileEntry.SetFavorite(b);
			}
		}

		private IEnumerator RenameProcess()
		{
			yield return null;
			//LookInputModule.SelectGameObject(renameField.gameObject);
			renameField.ActivateInputField();
		}

		public void OnRenameClick(FileButton fb)
		{
			if (statusField != null)
			{
				statusField.text = string.Empty;
			}
			renameFileButton = fb;
			OpenRenameDialog();
			if (renameField != null)
			{
				string text = fb.text;
				if (text.EndsWith(".json"))
				{
					text = text.Replace(".json", string.Empty);
				}
				else if (text.EndsWith(".vac"))
				{
					text = text.Replace(".vac", string.Empty);
				}
				else if (text.EndsWith(".vap"))
				{
					text = text.Replace(".vap", string.Empty);
				}
				renameField.text = text;
				StartCoroutine(RenameProcess());
			}
		}

		protected void OpenRenameDialog()
		{
			if (renameContainer != null)
			{
				renameContainer.gameObject.SetActive(true);
			}
		}

		public void OnRenameConfirm()
		{
			if (renameField != null && renameField.text != string.Empty && renameFileButton != null)
			{
				string text = ((renameFileButton.removedPrefix == null) ? (currentPath + slash + renameField.text) : (currentPath + slash + renameFileButton.removedPrefix + renameField.text));
				if (renameFileButton.isDir)
				{
					string fullPath = renameFileButton.fullPath;
					try
					{
						FileManager.AssertNotCalledFromPlugin();
						FileManager.MoveDirectory(fullPath, text);
					}
					catch (Exception ex)
					{
						LogUtil.LogError("Could not move directory " + fullPath + " to " + text + " Exception: " + ex.Message);
						if (statusField != null)
						{
							statusField.text = ex.Message;
						}
						OnRenameCancel();
						return;
					}
				}
				else
				{
					string fullPath2 = renameFileButton.fullPath;
					bool flag = false;
					string oldValue = string.Empty;
					if (fullPath2.EndsWith(".json"))
					{
						flag = true;
						oldValue = ".json";
						if (!text.EndsWith(".json"))
						{
							text += ".json";
						}
					}
					else if (fullPath2.EndsWith(".vac"))
					{
						flag = true;
						oldValue = ".vac";
						if (!text.EndsWith(".vac"))
						{
							text += ".vac";
						}
					}
					else if (fullPath2.EndsWith(".vap"))
					{
						flag = true;
						oldValue = ".vap";
						if (!text.EndsWith(".vap"))
						{
							text += ".vap";
						}
					}
					LogUtil.Log("Rename file " + fullPath2 + " to " + text);
					try
					{
						FileManager.AssertNotCalledFromPlugin();
						FileManager.MoveFile(fullPath2, text, false);
					}
					catch (Exception ex2)
					{
						LogUtil.LogError("Could not move file " + fullPath2 + " to " + text + " Exception: " + ex2.Message);
						if (statusField != null)
						{
							statusField.text = ex2.Message;
						}
						OnRenameCancel();
						return;
					}
					if (flag)
					{
						string text2 = fullPath2.Replace(oldValue, ".jpg");
						string text3 = text.Replace(oldValue, ".jpg");
						if (FileManager.FileExists(text2))
						{
							try
							{
								FileManager.MoveFile(text2, text3);
							}
							catch (Exception ex3)
							{
								LogUtil.LogError("Could not move file " + text2 + " to " + text3 + " Exception: " + ex3.Message);
								if (statusField != null)
								{
									statusField.text = ex3.Message;
								}
							}
						}
						string text4 = fullPath2 + ".fav";
						string text5 = text + ".fav";
						if (FileManager.FileExists(text4))
						{
							try
							{
								FileManager.MoveFile(text4, text5);
							}
							catch (Exception ex4)
							{
								LogUtil.LogError("Could not move file " + text4 + " to " + text5 + " Exception: " + ex4.Message);
								if (statusField != null)
								{
									statusField.text = ex4.Message;
								}
							}
						}
					}
				}
				UpdateFileList();
			}
			OnRenameCancel();
		}

		public void OnRenameCancel()
		{
			if (renameContainer != null)
			{
				renameContainer.gameObject.SetActive(false);
			}
			renameFileButton = null;
		}

		public void OnDeleteClick(FileButton fb)
		{
			if (statusField != null)
			{
				statusField.text = string.Empty;
			}
			deleteFileButton = fb;
			OpenDeleteDialog();
			if (deleteField != null)
			{
				string text = fb.text;
				if (text.EndsWith(".json"))
				{
					text = text.Replace(".json", string.Empty);
				}
				else if (text.EndsWith(".vac"))
				{
					text = text.Replace(".vac", string.Empty);
				}
				else if (text.EndsWith(".vap"))
				{
					text = text.Replace(".vap", string.Empty);
				}
				deleteField.text = text;
			}
		}

		protected void OpenDeleteDialog()
		{
			if (deleteContainer != null)
			{
				deleteContainer.gameObject.SetActive(true);
			}
		}

		public void OnDeleteConfirm()
		{
			if (deleteFileButton != null)
			{
				string fullPath = deleteFileButton.fullPath;
				if (deleteFileButton.isDir)
				{
					if (FileManager.DirectoryExists(fullPath))
					{
						try
						{
							FileManager.AssertNotCalledFromPlugin();
							FileManager.DeleteDirectory(fullPath, true);
						}
						catch (Exception ex)
						{
							LogUtil.LogError("Could not delete directory " + fullPath + " Exception: " + ex.Message);
							if (statusField != null)
							{
								statusField.text = ex.Message;
							}
							OnDeleteCancel();
							return;
						}
					}
				}
				else
				{
					if (FileManager.FileExists(fullPath))
					{
						try
						{
							FileManager.AssertNotCalledFromPlugin();
							FileManager.DeleteFile(fullPath);
						}
						catch (Exception ex2)
						{
							LogUtil.LogError("Could not delete file " + fullPath + " Exception: " + ex2.Message);
							if (statusField != null)
							{
								statusField.text = ex2.Message;
							}
							OnDeleteCancel();
							return;
						}
					}
					string text = string.Empty;
					if (fullPath.EndsWith(".json"))
					{
						text = ".json";
					}
					else if (fullPath.EndsWith(".vac"))
					{
						text = ".vac";
					}
					else if (fullPath.EndsWith(".vap"))
					{
						text = ".vap";
					}
					if (text != string.Empty)
					{
						string text2 = fullPath.Replace(text, ".jpg");
						if (FileManager.FileExists(text2))
						{
							try
							{
								FileManager.DeleteFile(text2);
							}
							catch (Exception ex3)
							{
								LogUtil.LogError("Could not delete file " + text2 + " Exception: " + ex3.Message);
								if (statusField != null)
								{
									statusField.text = ex3.Message;
								}
							}
						}
					}
					string text3 = fullPath + ".fav";
					if (FileManager.FileExists(text3))
					{
						try
						{
							FileManager.DeleteFile(text3);
						}
						catch (Exception ex4)
						{
							LogUtil.LogError("Could not delete file " + text3 + " Exception: " + ex4.Message);
							if (statusField != null)
							{
								statusField.text = ex4.Message;
							}
						}
					}
				}
				UpdateFileList();
				UpdateDirectoryList();
			}
			OnDeleteCancel();
		}

		public void OnDeleteCancel()
		{
			if (deleteContainer != null)
			{
				deleteContainer.gameObject.SetActive(false);
			}
			deleteFileButton = null;
		}

		//protected string DeterminePathToGoTo(string pathToGoTo)
		//{
		//	DirectoryEntry directoryEntry = FileManager.GetDirectoryEntry(pathToGoTo);
		//	if (!selectDirectory && directoryEntry != null && directoryEntry is VarDirectoryEntry)
		//	{
		//		DirectoryEntry directoryEntry2 = directoryEntry.FindFirstDirectoryWithFiles();
		//		string uid = directoryEntry.Uid;
		//		string uid2 = directoryEntry2.Uid;
		//		if (uid2 != uid)
		//		{
		//			string text = uid2.Replace(uid, string.Empty);
		//			//text = text.Replace('/', '\\');
		//			text = text.Replace('\\', '/');
		//			pathToGoTo += text;
		//		}
		//	}
		//	return pathToGoTo;
		//}

		public void OnFileClick(FileButton fb)
		{
			SelectFile(fb);
		}

		public void OnFilePointerEnter(FileButton fb)
		{
			if (fileHighlightField != null)
			{
				fileHighlightField.text = fb.fullPath;
			}
		}

		public void OnFilePointerExit(FileButton fb)
		{
			if (fileHighlightField != null)
			{
				fileHighlightField.text = string.Empty;
			}
		}

		//public void OnDirectoryClick(DirectoryButton db)
		//{
		//	GotoDirectory(db.fullPath, db.packageFilter);
		//}

		//public void OnShortCutClick(int i)
		//{
		//	if (i >= shortCutButtons.Count)
		//	{
		//		Debug.LogError("uFileBrowser: Button index is bigger than array, something went wrong.");
		//	}
		//	else
		//	{
		//		GotoDirectory(DeterminePathToGoTo(shortCutButtons[i].fullPath), shortCutButtons[i].packageFilter, shortCutButtons[i].flatten, shortCutButtons[i].includeRegularDirsInFlatten);
		//	}
		//}

		private IEnumerator DelaySetScroll(float scrollPos)
		{
			yield return null;
			filesScrollRect.verticalNormalizedPosition = scrollPos;
		}

		private void SaveDirectoryScrollPos()
		{
			if (filesScrollRect != null)
			{
				if (directoryScrollPositions == null)
				{
					directoryScrollPositions = new Dictionary<string, float>();
				}
				if (browserPage == null)
					browserPage = new Dictionary<string, int>();


				string key = SavedKey;
                float value;
                if (directoryScrollPositions.TryGetValue(key, out value))
                {
                    directoryScrollPositions.Remove(key);
                }
                float verticalNormalizedPosition = filesScrollRect.verticalNormalizedPosition;
                directoryScrollPositions.Add(key, verticalNormalizedPosition);

				if (browserPage.ContainsKey(key))
					browserPage.Remove(key);
				browserPage.Add(key, page);
            }
		}

		private void ClearDirectoryScrollPos()
		{
			if (currentPath != null && fileFormat != null && directoryScrollPositions != null)
			{
				string text = currentPath;
				if (!text.EndsWith("\\"))
				{
					text += "\\";
				}
				string key = fileFormat + ":" + text;
				float value;
				if (directoryScrollPositions.TryGetValue(key, out value))
				{
					directoryScrollPositions.Remove(key);
				}
			}
		}

		private void GoToPromotionalLink()
		{
			if (promotionalButtonText != null)
			{
				//SuperController.singleton.OpenLinkInBrowser(promotionalButtonText.text);
			}
		}

		private void OpenPackageInManager()
		{
			if (currentPackageUid != null && currentPackageUid != string.Empty)
			{
				//SuperController.singleton.OpenPackageInManager(currentPackageUid);
			}
		}

		private void OpenOnHub()
		{
			if (currentPackageUid != null && currentPackageUid != string.Empty)
			{
				VarPackage package = FileManager.GetPackage(currentPackageUid);
				if (package != null)
				{
					package.OpenOnHub();
				}
			}
		}

		public void GotoDirectory(string path, string pkgFilter = null, bool flatten = true, bool includeRegularDirs = false)
		{
    //        if (path == currentPath
				////&&lastCacheFileFormat==fileFormat
				////&&lastCacheInGame== inGame
				//&& path != string.Empty
    //            && pkgFilter == currentPackageFilter
    //            && useFlatten == flatten
    //            && includeRegularDirsInFlatten == includeRegularDirs)
    //        {
    //            SyncDisplayed();
    //            return;
    //        }
            currentPackageFilter = pkgFilter;
			useFlatten = flatten;
			includeRegularDirsInFlatten = includeRegularDirs;
			//SaveDirectoryScrollPos(currentPath);
			if (string.IsNullOrEmpty(path))
			{
				currentPath = string.Empty;
			}
			else if (!FileManager.DirectoryExists(path) && !flatten)
			{
				LogUtil.LogError("uFileBrowser: Directory doesn't exist:\n" + path);
				currentPath = string.Empty;
			}
			else
			{
				currentPath = path;
			}
			if ((bool)currentPathField)
			{
				currentPathField.text = currentPath;
			}
			if (selectDirectory && fileEntryField != null)
			{
				fileEntryField.text = string.Empty;
			}
			selected = null;
			UpdateFileList();
			if (!(filesScrollRect != null))
			{
				return;
			}
			float value = 1f;
			if (directoryScrollPositions != null)
			{
				if (!directoryScrollPositions.TryGetValue(SavedKey, out value))
				{
					value = 1f;
				}
				LogUtil.Log("directoryScrollPositions " + SavedKey + " " + value);
			}
            if (browserPage != null)
            {
				int p = 0;
				if (!browserPage.TryGetValue(SavedKey, out p))
				{
					p = 1;
				}
				page = p;
			}

			StartCoroutine(DelaySetScroll(value));
		}
		string SavedKey
        {
            get
            {
				string key = fileFormat + ":" + defaultPath+ ":" + inGame.ToString();
                return key;
			}
        }

		private void SelectFile(FileButton fb)
		{
			//if (fb == selected && selectDirectory && fb.isDir)
			//{
			//	GotoDirectory(fb.fullPath, currentPackageFilter);
			//}
			//else
			{
				//if (!fb.isDir && selectDirectory)
				//{
				//	return;
				//}
				if (selected != null)
				{
					selected.Unselect();
				}
				selected = fb;
				fb.Select();
				if (fileEntryField != null)
				{
					fileEntryField.text = selected.text;
					if (fileEntryField.text.EndsWith(".json"))
					{
						fileEntryField.text = fileEntryField.text.Replace(".json", string.Empty);
					}
					else if (fileEntryField.text.EndsWith(".vac"))
					{
						fileEntryField.text = fileEntryField.text.Replace(".vac", string.Empty);
					}
				}
				if (selectOnClick)
				{
					SelectButtonClicked();
				}
			}
		}

		public void PathFieldEndEdit()
		{
			if (currentPathField != null)
			{
				if (FileManager.DirectoryExists(currentPathField.text))
				{
					GotoDirectory(currentPathField.text);
				}
				else
				{
					currentPathField.text = currentPath;
				}
			}
		}

		public void SearchChanged()
		{
			if ((bool)searchField && !ignoreSearchChange)
			{
				search = searchField.text.Trim();
				searchLower = search.ToLowerInvariant();
				ResetDisplayedPage();
			}
		}

		public void SearchCancelClick()
		{
			searchField.text = string.Empty;
		}

		protected void ClearSearch()
		{
			search = string.Empty;
			searchLower = string.Empty;
			ignoreSearchChange = true;
			searchField.text = string.Empty;
			ignoreSearchChange = false;
		}

		public void SetTextEntry(bool b)
		{
			if (b)
			{
				selectOnClick = false;
				if (selectButton != null)
				{
					selectButton.gameObject.SetActive(true);
				}
				if (fileEntryField != null)
				{
					fileEntryField.gameObject.SetActive(true);
				}
				if (keepOpenToggle != null)
				{
					keepOpenToggle.gameObject.SetActive(false);
				}
			}
			else
			{
				selectOnClick = true;
				if (selectButton != null)
				{
					selectButton.gameObject.SetActive(false);
				}
				if (fileEntryField != null)
				{
					fileEntryField.gameObject.SetActive(false);
				}
				if (keepOpenToggle != null)
				{
					keepOpenToggle.gameObject.SetActive(true);
				}
			}
		}

		private IEnumerator ActivateFileNameFieldProcess()
		{
			yield return null;
			//LookInputModule.SelectGameObject(fileEntryField.gameObject);
			fileEntryField.ActivateInputField();
		}

		public void ActivateFileNameField()
		{
			if (fileEntryField != null)
			{
				StartCoroutine(ActivateFileNameFieldProcess());
			}
		}

		public void SelectButtonClicked()
		{
			LogUtil.Log("SelectButtonClicked");
			if (!selectOnClick && fileEntryField != null)
			{
				if (fileEntryField.text != string.Empty)
				{
					string path = currentPath + slash + fileEntryField.text;
					if (!_keepOpen)
					{
						Hide();
					}
					if (callback != null)
					{
						callback(path);
					}
					if (fullCallback != null)
					{
						fullCallback(path, !_keepOpen);
					}
				}
			}
			else if (selected != null && ((selected.isDir && selectDirectory) || (!selected.isDir && !selectDirectory)))
			{
				string fullPath = selected.fullPath;
				if (!_keepOpen)
				{
					Hide();
				}
				if (callback != null)
				{
					callback(fullPath);
				}
				if (fullCallback != null)
				{
					fullCallback(fullPath, !_keepOpen);
				}
			}
		}

		public void CancelButtonClicked()
		{
			if (canCancel)
			{
				Hide();
				if (callback != null)
				{
					callback(string.Empty);
				}
				if (fullCallback != null)
				{
					fullCallback(string.Empty, true);
				}
			}
		}

		private void SyncSort()
		{
			if (cachedFiles != null)
			{
				if (lastCacheUseFlatten)
				{
					sortedFilesAndDirs = cachedFiles;
					SortFilesAndDirs(sortedFilesAndDirs);
				}
				else
				{
					switch (directoryOption)
					{
					case UserPreferences.DirectoryOption.Hide:
						sortedFilesAndDirs = cachedFiles;
						SortFilesAndDirs(sortedFilesAndDirs);
						break;
					case UserPreferences.DirectoryOption.Intermix:
						sortedFilesAndDirs = new List<FileAndDirInfo>();
						sortedFilesAndDirs.AddRange(cachedFiles);
						SortFilesAndDirs(sortedFilesAndDirs);
						break;
					case UserPreferences.DirectoryOption.ShowFirst:
						sortedFilesAndDirs = new List<FileAndDirInfo>();
						SortFilesAndDirs(cachedFiles);
						sortedFilesAndDirs.AddRange(cachedFiles);
						break;
					case UserPreferences.DirectoryOption.ShowLast:
						sortedFilesAndDirs = new List<FileAndDirInfo>();
						SortFilesAndDirs(cachedFiles);
						sortedFilesAndDirs.AddRange(cachedFiles);
						break;
					}
				}
			}
			ResetDisplayedPage();
		}

		protected void HideButton(FileButton fb)
		{
			//manageContentTransform这里是true
			if (manageContentTransform && displayedFileButtons.Contains(fb))
			{
				fb.gameObject.SetActive(false);
				displayedFileButtons.Remove(fb);
			}
		}
		void HideButton(FileAndDirInfo info)
        {
            if (info.button != null)
            {
				if (displayedFileButtons.Contains(info.button))
				{
					displayedFileButtons.Remove(info.button);
				}
				info.button.gameObject.SetActive(false);
				PoolManager.ReleaseObject(info.button.gameObject);
				info.button = null;
            }
        }
		int lastSyncFrame = 0;
		private void SyncDisplayed()
		{
#if DEBUG
			string stackTrace = new System.Diagnostics.StackTrace().ToString();
			LogUtil.LogWarning("SyncDisplayed "+ stackTrace);
#endif
			LogUtil.LogWarning("SyncDisplayed");

			if (sortedFilesAndDirs == null)
			{
				return;
			}
			ClearImageQueue();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			Vector2 vector = cellSize + cellSpacing;
			if (!manageContentTransform)
			{
				foreach (FileButton displayedFileButton in displayedFileButtons)
				{
					displayedFileButton.gameObject.SetActive(false);
					displayedFileButton.transform.SetParent(null, false);
				}
				displayedFileButtons.Clear();
			}
			int num5 = (_page - 1) * _limitXMultiple + 1;
			int num6 = _page * _limitXMultiple;

			HashSet<string> clothTag = null;
			if (this.needFilterClothing)
            {
				clothTag = GetClothingFilter();
			}
			HashSet<string> hairTag = null;
			if (this.needFilterHair)
			{
				hairTag = GetHairFilter();
			}

			foreach (FileAndDirInfo sortedFilesAndDir in sortedFilesAndDirs)
			{
				FileEntry fileEntry = sortedFilesAndDir.FileEntry;
				//FileButton button = sortedFilesAndDir.button;
				//if (button != null)
				{
					if (fileEntry != null)
					{
                        if (clothTag != null && clothTag.Count > 0)
                        {
							bool includeNoTag = clothTag.Contains("no tag");
							bool includeUnknownTag = clothTag.Contains("unknown");

							VarFileEntry varFileEntry = fileEntry as VarFileEntry;
                            if (varFileEntry != null)
                            {
								bool pass = false;
                                if (varFileEntry.ClothingTags != null)
                                {
                                    foreach (var item in varFileEntry.ClothingTags)
                                    {
                                        if (clothTag.Contains(item))
                                        {
                                            pass = true;
                                            break;
                                        }
                                    }
                                }
                                if (!pass)
                                {
									//如果包含了no tag，则特殊处理
                                    if (includeNoTag)
                                    {
										if (varFileEntry.ClothingTags == null||varFileEntry.ClothingTags.Count==0)
                                        {
											pass = true;
										}
									}
								}

                                if (!pass && includeUnknownTag)
                                {
                                    if (varFileEntry.ClothingTags != null)
                                    {
                                        foreach (var item in varFileEntry.ClothingTags)
                                        {
											if (unknownClothingTagFilterChooser.val == item)
                                            {
                                                pass = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (!pass)
                                {
                                    HideButton(sortedFilesAndDir);
                                    continue;
                                }
                            }
                        }
						if (hairTag != null && hairTag.Count > 0)
						{
							bool includeNoTag = hairTag.Contains("no tag");
							bool includeUnknownTag = hairTag.Contains("unknown");

							VarFileEntry varFileEntry = fileEntry as VarFileEntry;
							if (varFileEntry != null)
							{
								bool pass = false;
								if (varFileEntry.HairTags != null)
								{
									foreach (var item in varFileEntry.HairTags)
									{
										if (hairTag.Contains(item))
										{
											pass = true;
											break;
										}
									}
								}
								if (!pass)
								{
									//如果包含了no tag，则特殊处理
									if (includeNoTag)
									{
										if (varFileEntry.HairTags == null || varFileEntry.HairTags.Count == 0)
										{
											pass = true;
										}
									}
								}
								if (!pass&& includeUnknownTag)
								{
									if (varFileEntry.HairTags != null)
									{
										foreach (var item in varFileEntry.HairTags)
										{
											if (unknownHairTagFilterChooser.val==(item))
											{
												pass = true;
												break;
											}
										}
									}
								}
								if (!pass)
								{
									HideButton(sortedFilesAndDir);
									continue;
								}
							}
						}

						if (_onlyFavorites && !sortedFilesAndDir.isFavorite)
						{
							//HideButton(button);
							HideButton(sortedFilesAndDir);
							continue;
						}
						if (_onlyInstalled && !sortedFilesAndDir.isInstalled)
						{
                            //HideButton(button);
							HideButton(sortedFilesAndDir);
							continue;
						}
						if (_onlyAutoInstall && !sortedFilesAndDir.isAutoInstall)
						{
							//HideButton(button);
							HideButton(sortedFilesAndDir);
							continue;
						}
						//if (_onlyTemplates && !sortedFilesAndDir.isTemplate)
						//{
						//	HideButton(button);
						//	continue;
						//}
						if (!string.IsNullOrEmpty(searchLower) && !fileEntry.UidLowerInvariant.Contains(searchLower))
						{
							VarFileEntry varFileEntry = fileEntry as VarFileEntry;
                            if (varFileEntry == null)
                            {
                                //HideButton(button);
							HideButton(sortedFilesAndDir);
                                continue;
							}
                            if (!varFileEntry.Package.UidLowerInvariant.Contains(searchLower))
							{
								//HideButton(button);
							HideButton(sortedFilesAndDir);
								continue;
							}
                        }
						//需要过滤
						if (!inGame && _creatorFilter != "All")
                        {
                            string creator = _creatorFilter.Substring(0, _creatorFilter.IndexOf('('));
                            if (fileEntry is VarFileEntry)
                            {
                                VarFileEntry varFileEntry = fileEntry as VarFileEntry;
                                if (!varFileEntry.Package.Uid.StartsWith(creator + "."))
                                {
                                    //HideButton(button);
							HideButton(sortedFilesAndDir);
                                    continue;
								}
                            }
                            else if (fileEntry is SystemFileEntry)
                            {
                                var systemFileEntry = fileEntry as SystemFileEntry;
                                if (systemFileEntry.package != null)
                                {
                                    if (systemFileEntry.package.Creator != creator)
                                    {
                                        //HideButton(button);
							HideButton(sortedFilesAndDir);
                                        continue;
									}
                                }
                            }
                        }
                    }
                    num++;
					if (num < num5 || num > num6)
					{
						//HideButton(button);
							HideButton(sortedFilesAndDir);
					}
					else
					{
						var cachedFile2 = sortedFilesAndDir;
                        if (cachedFile2.button == null)
                        {
							cachedFile2.button = CreateFileButton(cachedFile2.Name,
								cachedFile2.FullName, false,
								cachedFile2.isWriteable, cachedFile2.isHidden,
								cachedFile2.isHiddenModifiable, cachedFile2.isFavorite,
								cachedFile2.isAutoInstall, cachedFile2.isTemplate,
								cachedFile2.isTemplateModifiable);
						}
                        cachedFile2.button.gameObject.SetActive(false);
                        cachedFile2.button.transform.SetParent(fileContent, false);

                        FileButton button = sortedFilesAndDir.button;

						if (manageContentTransform)
						{
							button.gameObject.SetActive(true);
							RectTransform rectTransform = button.rectTransform;
							if (rectTransform != null)
							{
								rectTransform.anchorMin = new Vector2(0, 1);
								rectTransform.anchorMax = new Vector2(0, 1);
								rectTransform.pivot = new Vector2(0, 1);
								rectTransform.sizeDelta = new Vector2(370,420);
								rectTransform.localRotation = Quaternion.identity;
								rectTransform.localScale = Vector3.one;


								Vector2 anchoredPosition = default(Vector2);
								anchoredPosition.x = (float)num4 * vector.x;
								anchoredPosition.y = (float)(-num3) * vector.y;
								rectTransform.anchoredPosition = anchoredPosition;
								num4++;
								if (num4 == columnCount)
								{
									num4 = 0;
									num3++;
								}
								num2++;
							}
						}
						//else
						//{
						//	button.gameObject.SetActive(true);
						//	button.transform.SetParent(fileContent, false);
						//	num2++;
						//}
						displayedFileButtons.Add(button);
						SyncFileButtonImage(button);
					}
				}
			}
			if (manageContentTransform)
			{
				float y = (float)(num3 + 1) * vector.y;
				Vector2 sizeDelta = fileContent.sizeDelta;
				sizeDelta.y = y;
				fileContent.sizeDelta = sizeDelta;
			}
			if (showingCountText != null)
			{
				if (num6 > num)
				{
					num6 = num;
				}
				showingCountText.text = num5 + "-" + num6 + " of " + num;
			}
			_totalPages = (num - 1) / _limitXMultiple + 1;
		}

		protected List<FileAndDirInfo> FilterFormat(List<FileAndDirInfo> files, bool skipFileFormatCheck = false)
		{
			List<FileAndDirInfo> list = files;
			if (!string.IsNullOrEmpty(fileFormat) && !skipFileFormatCheck)
			{
				List<FileAndDirInfo> list3 = new List<FileAndDirInfo>();
				string[] array = fileFormat.Split('|');
				for (int j = 0; j < list.Count; j++)
				{
					string text = string.Empty;
					if (list[j].Name.Contains("."))
					{
						text = list[j].Name.Substring(list[j].Name.LastIndexOf('.') + 1).ToLowerInvariant();
					}
					for (int k = 0; k < array.Length; k++)
					{
						if (text == array[k].Trim().ToLowerInvariant())
						{
							list3.Add(list[j]);
						}
					}
				}
				list = list3;
			}
			return list;
		}
		
		protected void UpdateFileListCacheThreadSafe()
		{
			LogUtil.Log("UpdateFileListCacheThreadSafe");
			List<FileAndDirInfo> list = new List<FileAndDirInfo>();
			if (useFlatten)
			{
				List<FileEntry> list3 = new List<FileEntry>();
				try
				{
					if (fileFormat == "var")
                    {
						var vars = FileManager.singleton.GetAllVars();
						foreach (var item in vars)
						{
							var entry = new SystemFileEntry(item);
							list3.Add(entry);
						}
					}
					else if (inGame)
                    {
						string[] files= Directory.GetFiles(defaultPath, "*.*",SearchOption.AllDirectories);
						foreach(var item in files)
                        {
							//暂时写死 外观预设
							if (fileFormat == "vap")
                            {
								if (item.EndsWith(fileFormat))//.vap
								{
									if (File.Exists(item.Substring(0, item.Length - 3)+"jpg"))
                                    {
										list3.Add(new SystemFileEntry(item));
									}
								}
							}
							else if (fileFormat == "json")
                            {
								if (item.EndsWith(fileFormat))
								{
									if (File.Exists(item.Substring(0, item.Length - 4) + "jpg"))
									{
										list3.Add(new SystemFileEntry(item));
									}
								}
							}
						}
                    }
                    else
                    {
						if (fileFormat != null)
						{
							string regex = "\\.(" + fileFormat + ")$";
							FileManager.FindVarFilesRegex(currentPath, regex, list3);
						}
						else
						{
							FileManager.FindVarFiles(currentPath, "*", list3);
						}
					}

					//在这里进行筛选
					bool sceneFilterOther = defaultPath == "Saves" && fileFormat == "json";
					bool presetFilterOther = defaultPath == "Custom" && fileFormat == "vap";
					bool presetFilterPerson = defaultPath == "Custom/Atom/Person" && fileFormat == "vap";
					foreach (FileEntry item7 in list3)
					{
						if (item7 is VarFileEntry)
						{
							VarFileEntry varFileEntry = item7 as VarFileEntry;
							if (!varFileEntry.Package.isNewestEnabledVersion)
							{
								continue;
							}
                            if (sceneFilterOther)
                            {
								if (varFileEntry.InternalPath.StartsWith("Saves/scene/"))
								{
									continue;
								}
							}
							else if (presetFilterOther)
							{
								//所有的预设
								if (varFileEntry.InternalPath.StartsWith("Custom/Atom/Person/Pose/"))
									continue;//pose忽略
								else if (varFileEntry.InternalPath.StartsWith("Custom/Hair/"))
									continue;
								else if (varFileEntry.InternalPath.StartsWith("Custom/Clothing/"))
									continue;
								else if (varFileEntry.InternalPath.StartsWith("Custom/Atom/Person/"))
									continue;
							}
							else if (presetFilterPerson)
							{
								if (varFileEntry.InternalPath.StartsWith("Custom/Atom/Person/Pose/"))
									continue;//pose忽略
								if (varFileEntry.InternalPath.StartsWith("Custom/Atom/Person/Hair/"))
									continue;
								if (varFileEntry.InternalPath.StartsWith("Custom/Atom/Person/Clothing/"))
									continue;
							}
                        }
                        if (item7.Exists || FileManager.IsPackage(item7.Path))
                        {
                            FileAndDirInfo item2 = new FileAndDirInfo(item7);
                            list.Add(item2);
                        }
                        else
						{
							LogUtil.LogError("Unable to read file " + item7.Path);
							threadHadException = true;
							threadException = "Unable to read file " + item7.Path;
						}
					}
				}
				catch (Exception ex2)
				{
					LogUtil.LogError("uFileBrowser: " + ex2);
					threadHadException = true;
					threadException = ex2.Message;
				}
				list = FilterFormat(list, true);
			}

			//这里是初始化这个页面所有的作者信息
			//过滤创作者
			if (!inGame)
            {
				string lastCreator = null;
                if (_creatorFilter != "All")
                {
					lastCreator = _creatorFilter.Substring(0, _creatorFilter.IndexOf('('));
				}
                Dictionary<string, int> dic = new Dictionary<string, int>();
                List<string> ret = new List<string>();
				bool keepCreator = false;
                foreach (var item in list)
                {
					string creator = null;
					if (item.FileEntry is VarFileEntry)
                    {
						string uid = item.FileEntry.Uid;
						creator = uid.Substring(0, uid.IndexOf('.'));
					}
                    else if (item.FileEntry is SystemFileEntry)
                    {
                        var package = (item.FileEntry as SystemFileEntry).package;
                        if (package != null)
                        {
                            creator = package.Creator;
                        }
                    }
                    if (creator == null)
                    {
						LogUtil.LogError("no creator:" + item.FileEntry.Path);
                    }
                    else
                    {
						if (!dic.ContainsKey(creator))
							dic.Add(creator, 0);
						dic[creator]++;

						if (creator == lastCreator)
						{
							keepCreator = true;
						}
					}
                    
                }
                foreach (var item in dic)
                {
                    ret.Add(item.Key);
                }
                ret.Sort((a, b) =>
                {
                    if (dic[a] > dic[b])
                        return -1;
                    else
                        return 1;
                });
				string choice = "All";
                for (int i = 0; i < ret.Count; i++)
                {
					string creator = ret[i];
                    ret[i] += "(" + dic[ret[i]] + ")";
					if (keepCreator)
					{
						if (creator == lastCreator)
						{
							choice = ret[i];
						}
					}
				}
                ret.Insert(0, "All");
                creatorFilterChooser.choices = ret;
				//这里会调用回调函数，会有问题。只设置显示
				creatorFilterChooser.valNoCallback=choice;
				_creatorFilter = choice;
			}

            cachedFiles = list;
			lastCacheFileFormat = fileFormat;
			lastCacheFileRemovePrefix = fileRemovePrefix;
			lastCacheUseFlatten = useFlatten;
			lastCacheIncludeRegularDirsInFlatten = includeRegularDirsInFlatten;
			lastCachePackageFilter = currentPackageFilter;
			lastCacheTime = DateTime.Now;
			lastCacheForceOnlyShowTemplates = forceOnlyShowTemplates;
			lastCacheDir = currentPath;
			lastCacheInGame = inGame;
			cacheDirty = false;
		}

		private void UpdateFileList()
		{
			//Debug.Log("UpdateFileList");
            if (!cacheDirty)
            {
                if (currentPath != lastCacheDir)
                {
                    cacheDirty = true;
                }
                else if (useFlatten != lastCacheUseFlatten)
                {
                    cacheDirty = true;
                }
                else if (fileFormat != lastCacheFileFormat)
                {
                    cacheDirty = true;
                }
                else if (fileRemovePrefix != lastCacheFileRemovePrefix)
                {
                    cacheDirty = true;
                }
                else if (FileManager.lastPackageRefreshTime > lastCacheTime)
                {
                    cacheDirty = true;
                }
                else if (lastCachePackageFilter != currentPackageFilter)
                {
                    cacheDirty = true;
                }
                else if (useFlatten && FileManager.CheckIfDirectoryChanged(currentPath, lastCacheTime))
                {
                    cacheDirty = true;
                }
                else if (useFlatten && includeRegularDirsInFlatten != lastCacheIncludeRegularDirsInFlatten)
                {
                    cacheDirty = true;
                }
                else if (!useFlatten && FileManager.CheckIfDirectoryChanged(currentPath, lastCacheTime, false))
                {
                    cacheDirty = true;
                }
                else if (forceOnlyShowTemplates != lastCacheForceOnlyShowTemplates)
                {
                    cacheDirty = true;
                }
				else if (lastCacheInGame != inGame)
                {
					cacheDirty = true;
                }
			}
				LogUtil.Log("cacheDirty:"+ cacheDirty);
			if (cacheDirty)
			{
				int num = 0;
				ClearSearch();
				if (cachedFiles != null)
				{
					num += cachedFiles.Count;
					foreach (FileAndDirInfo cachedFile in cachedFiles)
					{
						if (cachedFile.button != null)
						{
							//UnityEngine.Object.Destroy(cachedFile.button.gameObject);
							PoolManager.ReleaseObject(cachedFile.button.gameObject);
						}
					}
				}
				displayedFileButtons.Clear();
				threadHadException = false;

				UpdateFileListCacheThreadSafe();
				if (threadHadException && statusField != null)
				{
					statusField.text = threadException;
				}
				ClearImageQueue();
				int num2 = 0;
                foreach (FileAndDirInfo cachedFile2 in cachedFiles)
                {
					//所有的列表项都需要创建button，太卡了
					if (!s_OptimzeFileButton)
                    {
						cachedFile2.button = CreateFileButton(cachedFile2.Name,
							cachedFile2.FullName, false,
							cachedFile2.isWriteable, cachedFile2.isHidden,
							cachedFile2.isHiddenModifiable, cachedFile2.isFavorite,
							cachedFile2.isAutoInstall, cachedFile2.isTemplate,
							cachedFile2.isTemplateModifiable);

						cachedFile2.button.gameObject.SetActive(false);
						if (manageContentTransform)
						{
							cachedFile2.button.transform.SetParent(fileContent, false);
						}
					}
                    
					num2++;
                }
            }
			SyncSort();
		}
		static bool s_OptimzeFileButton = true;

		private void UpdateDirectoryList()
		{
		}

		private void Awake()
		{
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			slash = directorySeparatorChar.ToString();
			drives = new List<string>(Directory.GetLogicalDrives());
			displayedFileButtons = new HashSet<FileButton>();
			queuedThumbnails = new HashSet<var_browser.CustomImageLoaderThreaded.QueuedImage>();
		}
		void Start()
		{ 
			if (renameFieldAction != null)
			{
				InputFieldAction inputFieldAction = renameFieldAction;
				inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(OnRenameConfirm));
			}
			if (onlyShowLatestToggle != null)
			{
				onlyShowLatestToggle.isOn = _onlyShowLatest;
			}
			if (showHiddenToggle != null)//onlyInstalled
			{
				showHiddenToggle.isOn = _onlyInstalled;
				showHiddenToggle.transform.Find("Label").GetComponent<Text>().text = "Only Installed";
				showHiddenToggle.onValueChanged.AddListener(SetOnlyInstalled);
			}

			if (showAutoInstallToggle != null)//onlyInstalled
			{
				showAutoInstallToggle.isOn = _onlyAutoInstall;
				showAutoInstallToggle.transform.Find("Label").GetComponent<Text>().text = "Only AutoInstall";
				showAutoInstallToggle.onValueChanged.AddListener(SetOnlyAutoInstall);
			}

			if (onlyFavoritesToggle != null)
			{
				onlyFavoritesToggle.isOn = _onlyFavorites;
				onlyFavoritesToggle.onValueChanged.AddListener(SetOnlyFavorites);
			}

			if (onlyTemplatesToggle != null)
			{
				onlyTemplatesToggle.isOn = _onlyTemplates;
				onlyTemplatesToggle.onValueChanged.AddListener(SetOnlyTemplates);
			}
			if (limitSlider != null)
			{
				limitSlider.value = _limit;
				limitSlider.onValueChanged.AddListener(SetLimit);
			}
			_limitXMultiple = _limit * limitMultiple;
			if (limitValueText != null)
			{
				limitValueText.text = _limitXMultiple.ToString("F0");
			}
			if (openPackageButton != null)
			{
				openPackageButton.onClick.AddListener(OpenPackageInManager);
			}
			if (openOnHubButton != null)
			{
				openOnHubButton.onClick.AddListener(OpenOnHub);
			}
			if (promotionalButton != null)
			{
				promotionalButton.onClick.AddListener(GoToPromotionalLink);
			}
			if (sortByPopup != null)
			{
				sortByPopup.currentValueNoCallback = _sortBy.ToString();
				UIPopup uIPopup = sortByPopup;
				uIPopup.onValueChangeHandlers = SetSortBy;// (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetSortBy));
			}
			if (directoryOptionPopup != null)
			{
				directoryOptionPopup.currentValueNoCallback = _directoryOption.ToString();
				UIPopup uIPopup2 = directoryOptionPopup;
				uIPopup2.onValueChangeHandlers = SetDirectoryOption;// (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetDirectoryOption));
			}



			CreateRightHeader("Custom", 0-10, Color.black);
			CreateRightButton("Scene", -50).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenCustomScene();
			});

			CreateRightButton("Saved Person", -100).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenCustomSavedPerson();
			});
			CreateRightButton("Person Preset", -150).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenPersonPreset();
			});
			CreateRightHeader("Category", -200 - 10, Color.black);
			CreateRightButton("Scene", -250).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenCategoryScene();
			});
			CreateRightButton("Clothing", -300).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenCategoryClothing();
			});
			CreateRightButton("Hair", -350).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenCategoryHair();
			});
			CreateRightButton("Pose", -400).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenCategoryPose();
			});

			CreateRightHeader("Preset", -450 - 10, Color.black);
			CreateRightButton("Person", -500).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenPresetPerson();
			});
			CreateRightButton("Clothing", -550).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenPresetClothing();
			});
			CreateRightButton("Hair",-600).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenPresetHair();
			});
			CreateRightButton("Other",-650).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenPresetOther();
			});
			CreateRightHeader("Misc", -700 - 10, Color.black);
			CreateRightButton("AssetBundle", -750).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenMiscCUA();
			});
			CreateRightButton("All", -800).button.onClick.AddListener(() =>
			{
				VamHookPlugin.singleton.OpenMiscAll();
			});


			//left
			InitTags();

#region 头发
            {

				var container = CreateUIContainer(-420, -240-120, 200, 1460 - 120);//整个高度是1700
				HairTagsUIList.Add(container);
				var container2 = CreateUIContainer(-210, -240 - 120, 210, 1460 - 120);
				HairTagsUIList.Add(container2);
				var container0 = CreateUIContainer(-420, -130, 420, 100 + 120);
				HairTagsUIList.Add(container0);

				CreateLabel(container0, "Hair Tags Filter", Color.black, true);//高度40
				hairOnlyAllowSingleFilter = new JSONStorableBool("Only Allow Single Filter", false, SetHairOnlyAllowSingleFilter);
				CreateToggle(container0, hairOnlyAllowSingleFilter);//高度50

				var list = new List<string>();
				foreach (var item in TagFilter.HairUnknownTags)
				{
					list.Add(item);
				}
				unknownHairTagFilterChooser = new JSONStorableStringChooser("Unknown Tags", list, list[0], "Unknown Tags", SyncUnknownHairFilter);
				CreateFilterablePopup(container0, unknownHairTagFilterChooser);//高度120

				CreateLabel(container, "Region Tags", Color.black, true);
				for (int i = 0; i < TagFilter.HairRegionTags.Count; i++)
				{
					CreateToggle(container, HairRegionTagsJsonStorable[i]);
					HairRegionTagsJsonStorable[i].setJSONCallbackFunction = OnHairTagChange;
				}
				CreateLabel(container, "Other Tags", Color.black, true);
				//CreateToggle(container, HairNoTagJsonStorable);
				//HairNoTagJsonStorable.setJSONCallbackFunction = OnHairTagChange;
				for (int i = 0; i < TagFilter.HairOtherTags.Count; i++)
				{
					CreateToggle(container, HairOtherTagsJsonStorable[i]);
					HairOtherTagsJsonStorable[i].setJSONCallbackFunction = OnHairTagChange;
				}

				CreateLabel(container2, "Type Tags", Color.black, true);
				for (int i = 0; i < TagFilter.HairTypeTags.Count; i++)
				{
					CreateToggle(container2, HairTypeTagsJsonStorable[i]);
					HairTypeTagsJsonStorable[i].setJSONCallbackFunction = OnHairTagChange;
				}

				
			}
#endregion

#region 衣服
			{

                //界面左侧离filebrowser的距离，越小越远。
                var container = CreateUIContainer(-420, -240 - 120, 200, 1460 - 120);//整个高度是1700
                ClothingTagsUIList.Add(container);
				var container2 = CreateUIContainer(-210, -240 - 120, 210, 1460 - 120);
				ClothingTagsUIList.Add(container2);
				var container0 = CreateUIContainer(-420, -130, 420, 100 + 120);
				ClothingTagsUIList.Add(container0);

				CreateLabel(container0, "Clothing Tags Filter", Color.black, true);//高度40
				onlyAllowSingleFilter = new JSONStorableBool("Only Allow Single Filter", false, SetClothingOnlyAllowSingleFilter);
				CreateToggle(container0, onlyAllowSingleFilter);//高度50

				var list = new List<string>();
				foreach (var item in TagFilter.ClothingUnknownTags)
				{
					list.Add(item);
				}
				unknownClothingTagFilterChooser = new JSONStorableStringChooser("Unknown Tags", list, list[0], "Unknown Tags", SyncUnknownClothingFilter);
				CreateFilterablePopup(container0, unknownClothingTagFilterChooser);//高度120

				CreateLabel(container, "Region Tags", Color.black, true);
                for (int i = 0; i < TagFilter.ClothingRegionTags.Count; i++)
                {
                    CreateToggle(container, ClothingRegionTagsJsonStorable[i]);
                    ClothingRegionTagsJsonStorable[i].setJSONCallbackFunction = OnClothingTagChange;
                }
                CreateLabel(container, "Other Tags", Color.black, true);
                for (int i = 0; i < TagFilter.ClothingOtherTags.Count; i++)
                {
                    CreateToggle(container, ClothingOtherTagsJsonStorable[i]);
                    ClothingOtherTagsJsonStorable[i].setJSONCallbackFunction = OnClothingTagChange;
                }
                //CreateLabel(container, "Extra Tags", Color.black, true);
                //CreateToggle(container, ClothingNoTagJsonStorable);
                //ClothingNoTagJsonStorable.setJSONCallbackFunction = OnClothingTagChange;


                CreateLabel(container2, "Type Tags", Color.black, true);
                for (int i = 0; i < TagFilter.ClothingTypeTags.Count; i++)
                {
                    CreateToggle(container2, ClothingTypeTagsJsonStorable[i]);
                    ClothingTypeTagsJsonStorable[i].setJSONCallbackFunction = OnClothingTagChange;
                }


				
			}

#endregion


            {
				//这个放最后，因为会弹出popup窗口，否则会被挡住
				//创作者过滤
				var createrContainter = CreateUIContainer(-420, 0, 420, 120);
				var list = new List<string>();
				list.Add("All");
				List<string> choicesList4 = list;
				creatorFilterChooser = new JSONStorableStringChooser("creator", choicesList4, _creatorFilter, "Creator", SyncCreatorFilter);
				creatorFilterChooser.isStorable = false;
				creatorFilterChooser.isRestorable = false;
				creatorPopup = CreateFilterablePopup(createrContainter, creatorFilterChooser);//高度120

			}

		}
		JSONStorableStringChooser unknownClothingTagFilterChooser;
		JSONStorableStringChooser unknownHairTagFilterChooser;
		void SyncUnknownClothingFilter(string s)
		{
			ResetDisplayedPage();
		}
		void SyncUnknownHairFilter(string s)
		{
			ResetDisplayedPage();
		}

		void OnHairTagChange(JSONStorableBool jsb)
        {
            if (hairOnlyAllowSingleFilter.val && jsb.val)
            {
                //把其他的都去掉掉
                foreach (var item in HairRegionTagsJsonStorable)
                {
                    if (item != jsb) item.valNoCallback = false;
                }
                foreach (var item in HairTypeTagsJsonStorable)
                {
                    if (item != jsb) item.valNoCallback = false;
                }
				foreach (var item in HairOtherTagsJsonStorable)
				{
					if (item != jsb) item.valNoCallback = false;
				}
				//if (ClothingNoTagJsonStorable != jsb) ClothingNoTagJsonStorable.valNoCallback = false;
			}
            ResetDisplayedPage();
		}
		void OnClothingTagChange(JSONStorableBool jsb)
        {
            if (onlyAllowSingleFilter.val && jsb.val)
            {
                //把其他的都去掉掉
                foreach (var item in ClothingRegionTagsJsonStorable)
                {
                    if (item != jsb) item.valNoCallback = false;
                }
                foreach (var item in ClothingOtherTagsJsonStorable)
                {
                    if (item != jsb) item.valNoCallback = false;
                }
                foreach (var item in ClothingTypeTagsJsonStorable)
                {
                    if (item != jsb) item.valNoCallback = false;
                }
            }
            ResetDisplayedPage();
		}
		JSONStorableBool onlyAllowSingleFilter;
		JSONStorableBool hairOnlyAllowSingleFilter;
		void SetClothingOnlyAllowSingleFilter(bool val)
		{
            if (val)
            {
				bool have = false;
                //把其他的都去掉掉
                foreach (var item in ClothingRegionTagsJsonStorable)
                {
                    if (!have)
                    {
                        if (item.val) have = true;
                    }
                    else
                    {
                        item.valNoCallback = false;
                    }
                }
                foreach (var item in ClothingTypeTagsJsonStorable)
                {
                    if (!have)
                    {
                        if (item.val) have = true;
                    }
                    else
                    {
                        item.valNoCallback = false;
                    }
                }
                foreach (var item in ClothingOtherTagsJsonStorable)
                {
                    if (!have)
                    {
                        if (item.val) have = true;
                    }
                    else
                    {
                        item.valNoCallback = false;
                    }
                }
                // no tag优先级最低
                //if (!have)
                //{
                //    if (ClothingNoTagJsonStorable.val) ClothingNoTagJsonStorable.valNoCallback = true;
                //}
                //else
                //{
                //    ClothingNoTagJsonStorable.valNoCallback = false;
                //}
            }
            ResetDisplayedPage();
		}
		void SetHairOnlyAllowSingleFilter(bool val)
		{
            if (val)
            {
                bool have = false;
                //把其他的都去掉掉
                foreach (var item in HairRegionTagsJsonStorable)
                {
                    if (!have)
                    {
                        if (item.val) have = true;
                    }
                    else
                    {
                        item.valNoCallback = false;
                    }
                }
                foreach (var item in HairTypeTagsJsonStorable)
                {
                    if (!have)
                    {
                        if (item.val) have = true;
                    }
                    else
                    {
                        item.valNoCallback = false;
                    }
                }
				foreach (var item in HairOtherTagsJsonStorable)
				{
					if (!have)
					{
						if (item.val) have = true;
					}
					else
					{
						item.valNoCallback = false;
					}
				}
            }
            ResetDisplayedPage();
		}

		protected string _creatorFilter = "All";
		JSONStorableStringChooser creatorFilterChooser;
		public UIDynamicPopup creatorPopup = null;
		protected void SyncCreatorFilter(string s)
		{
			LogUtil.Log("SyncCreatorFilter "+s);
			_creatorFilter = s;
			ResetDisplayedPage();
		}
	}


}
