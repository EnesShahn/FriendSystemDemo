using EnesShahn.PanelSystem;

using SDK;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class LoginPanel : BasePanel
    {
        [SerializeField] private TMP_InputField emailInputField;
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;

        public override PanelType PanelType => PanelType.LoginPanel;

        private void Awake()
        {
            loginButton.onClick.AddListener(OnLoginButtonClicked);
            registerButton.onClick.AddListener(OnRegisterButtonClicked);
        }

        public override void ShowPanel(BasePanelData data)
        {
            gameObject.SetActive(true);
            GetComponent<UIAnimator>().ShowUIItems();
        }
        public override void HidePanel()
        {
            GetComponent<UIAnimator>().HideUIItems(() =>
            {
                gameObject.SetActive(false);
            });
        }

        private async void OnLoginButtonClicked()
        {
            var response = await Client.Login(emailInputField.text, passwordInputField.text);
            if (response.Success)
            {
                PanelManager.Show(PanelType.MainPanel, null);
                HidePanel();
            }
        }
        private async void OnRegisterButtonClicked()
        {
            var response = await Client.Register(emailInputField.text, passwordInputField.text);
            if (response.Success)
            {
                PanelManager.Show(PanelType.MainPanel, null);
                HidePanel();
            }
        }
    }
}