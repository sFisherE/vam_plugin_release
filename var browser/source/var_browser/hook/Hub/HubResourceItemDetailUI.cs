using UnityEngine;
using UnityEngine.UI;

namespace var_browser
{
    public class HubResourceItemDetailUI : HubResourceItemUI
    {
        public void Init(MVR.Hub.HubResourceItemDetailUI ui)
        {
            this.closeDetailButton = ui.closeDetailButton;
            this.closeDetailButtonAlt = ui.closeDetailButtonAlt;
            this.hadErrorIndicator = ui.hadErrorIndicator;
            this.errorText = ui.errorText;
            this.navigateToOverviewButton = ui.navigateToOverviewButton;
            this.hasUpdatesIndicator = ui.hasUpdatesIndicator;
            this.updatesText = ui.updatesText;
            this.navigateToUpdatesButton = ui.navigateToUpdatesButton;
            this.hasReviewsIndicator = ui.hasReviewsIndicator;
            this.reviewsText = ui.reviewsText;
            this.navigateToReviewsButton = ui.navigateToReviewsButton;
            this.navigateToHistoryButton = ui.navigateToHistoryButton;
            this.navigateToDiscussionButton = ui.navigateToDiscussionButton;
            this.hasPromotionalLinkIndicator = ui.hasPromotionalLinkIndicator;
            this.promotionalLinkText = ui.promotionalLinkText;
            this.navigateToPromotionalLinkButton = ui.navigateToPromotionalLinkButton;
            this.promtionalLinkButtonEnterExitAction = ui.promtionalLinkButtonEnterExitAction;
            this.hasOtherCreatorsIndicator = ui.hasOtherCreatorsIndicator;
            this.creatorSupportContent = ui.creatorSupportContent;
            this.hubDownloadableIndicatorAlt = ui.hubDownloadableIndicatorAlt;
            this.hubDownloadableNegativeIndicatorAlt = ui.hubDownloadableNegativeIndicatorAlt;
            this.externalDownloadUrl = ui.externalDownloadUrl;
            this.goToExternalDownloadUrlButton = ui.goToExternalDownloadUrlButton;
            this.packageContent = ui.packageContent;
            this.downloadAllButton = ui.downloadAllButton;
            this.downloadAvailableIndicator = ui.downloadAvailableIndicator;
        }
        public new HubResourceItemDetail connectedItem;

        public Button closeDetailButton;
        public Button closeDetailButtonAlt;
        public GameObject hadErrorIndicator;
        public Text errorText;
        public Button navigateToOverviewButton;
        public GameObject hasUpdatesIndicator;
        public Text updatesText;
        public Button navigateToUpdatesButton;
        public GameObject hasReviewsIndicator;
        public Text reviewsText;
        public Button navigateToReviewsButton;
        public Button navigateToHistoryButton;
        public Button navigateToDiscussionButton;
        public GameObject hasPromotionalLinkIndicator;
        public Text promotionalLinkText;
        public Button navigateToPromotionalLinkButton;
        public PointerEnterExitAction promtionalLinkButtonEnterExitAction;
        public GameObject hasOtherCreatorsIndicator;
        public RectTransform creatorSupportContent;
        public GameObject hubDownloadableIndicatorAlt;
        public GameObject hubDownloadableNegativeIndicatorAlt;
        public Text externalDownloadUrl;
        public Button goToExternalDownloadUrlButton;
        public RectTransform packageContent;
        public Button downloadAllButton;
        public GameObject downloadAvailableIndicator;
    }

}
