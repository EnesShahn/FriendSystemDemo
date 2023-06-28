using System;

using EnesShahn.PanelSystem;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class InputPopupPanelData : BasePanelData
    {
        private string _infoText;
        private string _placeholder;
        private Action<string> _onYesClickedCallback;
        private Action _onNoClickedCallback;

        public string InfoText => _infoText;
        public string Placeholder => _placeholder;
        public Action<string> OnYesClickedCallback => _onYesClickedCallback;
        public Action OnNoClickedCallback => _onNoClickedCallback;

        public InputPopupPanelData(string infoText, string placeholder, Action<string> onYesClickedCallback, Action onNoClickedCallback)
        {
            _infoText = infoText;
            _placeholder = placeholder;
            _onYesClickedCallback = onYesClickedCallback;
            _onNoClickedCallback = onNoClickedCallback;
        }
    }
    public class InputPopupPanel : BasePanel
    {
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private TMP_Text _placeholderText;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;

        public override PanelType PanelType => PanelType.ConfirmationPopupPanel;

        private InputPopupPanelData _data;

        public override void ShowPanel(BasePanelData data)
        {
            _data = data as InputPopupPanelData;
            _yesButton.onClick.RemoveAllListeners();
            _yesButton.onClick.AddListener(() =>
            {
                _data.OnYesClickedCallback?.Invoke(_inputField.text);
                _inputField.text = "";
                HidePanel();
            });

            _noButton.onClick.RemoveAllListeners();
            _noButton.onClick.AddListener(() =>
            {
                _data.OnNoClickedCallback?.Invoke();
                _inputField.text = "";
                HidePanel();
            });

            _placeholderText.text = _data.Placeholder;
            _infoText.text = _data.InfoText;

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
    }
}