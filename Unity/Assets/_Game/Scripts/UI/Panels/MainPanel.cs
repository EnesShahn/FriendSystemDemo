using EnesShahn.PanelSystem;

using SDK;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class MainPanel : BasePanel
    {
        [SerializeField] private Button _openSocialPanelButton;
        [SerializeField] private TMP_InputField _playerIdInputField;

        public override PanelType PanelType => PanelType.MainPanel;

        public override void ShowPanel(BasePanelData data)
        {
            gameObject.SetActive(true);
            GetComponent<UIAnimator>().ShowUIItems();
            _openSocialPanelButton.onClick.AddListener(OpenSocialPanel);

            Client.OnUserDataFetched += UpdatePlayerID;
        }
        public override void HidePanel()
        {
            Client.OnUserDataFetched -= UpdatePlayerID;
            GetComponent<UIAnimator>().HideUIItems(() =>
            {
                gameObject.SetActive(false);
            });
            _openSocialPanelButton.onClick.AddListener(OpenSocialPanel);
        }

        private void UpdatePlayerID()
        {
            _playerIdInputField.text = Client.UserData.UserID;
        }
        private void OpenSocialPanel()
        {
            _openSocialPanelButton.gameObject.SetActive(false);
            PanelManager.Show(PanelType.SocialPanel, null);
        }
    }
}