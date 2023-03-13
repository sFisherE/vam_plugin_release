using UnityEngine;
using UnityEngine.UI;

namespace var_browser
{
    public class HubResourceCreatorSupportUI : MonoBehaviour
    {
        public void Init(MVR.Hub.HubResourceCreatorSupportUI ui)
        {
            this.linkButton = ui.linkButton;
            this.creatorNameText = ui.creatorNameText;
            this.pointerEnterExitAction = ui.pointerEnterExitAction;
        }
        public Button linkButton;
        public Text creatorNameText;
        public PointerEnterExitAction pointerEnterExitAction;
    }
}
