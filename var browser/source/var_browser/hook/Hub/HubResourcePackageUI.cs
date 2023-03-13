using UnityEngine;
using UnityEngine.UI;

namespace var_browser
{
    public class HubResourcePackageUI : MonoBehaviour
    {
        public void Init(MVR.Hub.HubResourcePackageUI ui)
        {
            this.resourceButton = ui.resourceButton;
            this.nameText = ui.nameText;
            this.licenseTypeText = ui.licenseTypeText;
            this.fileSizeText = ui.fileSizeText;
            this.isDependencyIndicator = ui.isDependencyIndicator;
            this.notOnHubIndicator = ui.notOnHubIndicator;
            this.alreadyHaveIndicator = ui.alreadyHaveIndicator;
            this.openInPackageManagerButton = ui.openInPackageManagerButton;
            this.alreadyHaveSceneIndicator = ui.alreadyHaveSceneIndicator;
            this.openSceneButton = ui.openSceneButton;
            this.downloadButton = ui.downloadButton;
            this.updateAvailableIndicator = ui.updateAvailableIndicator;
            this.updateButton = ui.updateButton;
            this.updateMsgText = ui.updateMsgText;
            this.isDownloadQueuedIndicator = ui.isDownloadQueuedIndicator;
            this.isDownloadingIndicator = ui.isDownloadingIndicator;
            this.isDownloadedIndicator = ui.isDownloadedIndicator;
            this.progressSlider = ui.progressSlider;

        }
        public HubResourcePackage connectedItem;

        public Button resourceButton;
        public Text nameText;
        public Text licenseTypeText;
        public Text fileSizeText;
        public GameObject isDependencyIndicator;
        public GameObject notOnHubIndicator;
        public GameObject alreadyHaveIndicator;
        public Button openInPackageManagerButton;
        public GameObject alreadyHaveSceneIndicator;
        public Button openSceneButton;
        public Button downloadButton;
        public GameObject updateAvailableIndicator;
        public Button updateButton;
        public Text updateMsgText;
        public GameObject isDownloadQueuedIndicator;
        public GameObject isDownloadingIndicator;
        public GameObject isDownloadedIndicator;
        public Slider progressSlider;
    }

}
