using System;

using EnesShahn.PanelSystem;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class ConfirmationPopupPanelData : BasePanelData
    {
        private string _infoText;
        private Action _onYesClickedCallback;
        private Action _onNoClickedCallback;

        public string InfoText => _infoText;
        public Action OnYesClickedCallback => _onYesClickedCallback;
        public Action OnNoClickedCallback => _onNoClickedCallback;

        public ConfirmationPopupPanelData(string infoText, Action onYesClickedCallback, Action onNoClickedCallback)
        {
            _infoText = infoText;
            _onYesClickedCallback = onYesClickedCallback;
            _onNoClickedCallback = onNoClickedCallback;
        }
    }
    public class ConfirmationPopupPanel : BasePanel
    {
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;

        public override PanelType PanelType => PanelType.ConfirmationPopupPanel;

        private ConfirmationPopupPanelData _data;

        public override void ShowPanel(BasePanelData data)
        {
            _data = data as ConfirmationPopupPanelData;
            _yesButton.onClick.RemoveAllListeners();
            _yesButton.onClick.AddListener(() =>
            {
                _data.OnYesClickedCallback?.Invoke();
                HidePanel();
            });

            _noButton.onClick.RemoveAllListeners();
            _noButton.onClick.AddListener(() =>
            {
                _data.OnNoClickedCallback?.Invoke();
                HidePanel();
            });

            _infoText.text = _data.InfoText;

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