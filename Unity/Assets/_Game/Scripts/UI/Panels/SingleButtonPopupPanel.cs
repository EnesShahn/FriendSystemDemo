using System;

using EnesShahn.PanelSystem;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class SingleButtonPopupPanelData : BasePanelData
    {
        private string _infoText;
        private string _buttonText;
        private Action _onButtonClicked;

        public string InfoText => _infoText;
        public string ButtonText => _buttonText;
        public Action OnButtonClickedCallback => _onButtonClicked;

        public SingleButtonPopupPanelData(string infoText, string buttonText, Action onButtonClicked)
        {
            _infoText = infoText;
            _buttonText = buttonText;
            _onButtonClicked = onButtonClicked;
        }
    }
    public class SingleButtonPopupPanel : BasePanel
    {
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private TMP_Text _buttonText;
        [SerializeField] private Button _button;

        public override PanelType PanelType => PanelType.SingleButtonPopupPanel;

        private SingleButtonPopupPanelData _data;

        public override void ShowPanel(BasePanelData data)
        {
            _data = data as SingleButtonPopupPanelData;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                _data.OnButtonClickedCallback?.Invoke();
                HidePanel();
            });

            _infoText.text = _data.InfoText;
            _buttonText.text = _data.ButtonText;

            transform.SetAsLastSibling();

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