using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
namespace UI.Panels.Social
{
    public class TabController : MonoBehaviour
    {
        [SerializeField] private UITabManager _tabManager;
        [SerializeField] private GameObject _tab;
        private Toggle _toggle;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(SetTabActive);
            SetTabActive(_toggle.isOn);
        }

        private void SetTabActive(bool isOn)
        {
            if (_tab.activeSelf || !isOn)
                return;

            _tabManager.HideAllTabs();
            _tab.SetActive(true);
        }
    }
}