using UnityEngine;
using UnityEngine.UI;

namespace var_browser
{
    public class HubResourceItemUI : MonoBehaviour
    {
        public void Init(MVR.Hub.HubResourceItemUI ui)
        {
            this.titleText = ui.titleText;
            this.tagLineText = ui.tagLineText;
            this.versionText = ui.versionText;
            this.payTypeText = ui.payTypeText;
            this.categoryText = ui.categoryText;
            this.payTypeAndCategorySelectButton = ui.payTypeAndCategorySelectButton;
            this.creatorSelectButton = ui.creatorSelectButton;
            this.creatorText = ui.creatorText;

            this.creatorIconImage = ui.creatorIconImage;
            this.thumbnailImage = ui.thumbnailImage;
            this.hubDownloadableIndicator = ui.hubDownloadableIndicator;
            this.hubDownloadableNegativeIndicator = ui.hubDownloadableNegativeIndicator;
            this.hubHostedIndicator = ui.hubHostedIndicator;
            this.hubHostedNegativeIndicator = ui.hubHostedNegativeIndicator;


            this.hasDependenciesIndicator = ui.hasDependenciesIndicator;
            this.hasDependenciesNegativeIndicator = ui.hasDependenciesNegativeIndicator;
            this.inLibraryIndicator = ui.inLibraryIndicator;
            this.updateAvailableIndicator = ui.updateAvailableIndicator;
            this.updateMsgText = ui.updateMsgText;
            this.dependencyCountText = ui.dependencyCountText;
            this.downloadCountText = ui.downloadCountText;
            this.ratingsCountText = ui.ratingsCountText;
            this.ratingSlider = ui.ratingSlider;
            this.lastUpdateText = ui.lastUpdateText;
            this.openDetailButton = ui.openDetailButton;
        }

        public HubResourceItem connectedItem;

        public Text titleText;
        public Text tagLineText;
        public Text versionText;
        public Text payTypeText;
        public Text categoryText;
        public Button payTypeAndCategorySelectButton;
        public Button creatorSelectButton;
        public Text creatorText;

        public RawImage creatorIconImage;
        public RawImage thumbnailImage;
        public GameObject hubDownloadableIndicator;
        public GameObject hubDownloadableNegativeIndicator;
        public GameObject hubHostedIndicator;
        public GameObject hubHostedNegativeIndicator;

        public GameObject hasDependenciesIndicator;
        public GameObject hasDependenciesNegativeIndicator;
        public GameObject inLibraryIndicator;
        public GameObject updateAvailableIndicator;
        public Text updateMsgText;
        public Text dependencyCountText;
        public Text downloadCountText;
        public Text ratingsCountText;
        public Slider ratingSlider;
        public Text lastUpdateText;
        public Button openDetailButton;
    }

}
