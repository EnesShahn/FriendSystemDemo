using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UI.Panels.Social
{
    public class UITabManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] _tabs;

        public void HideAllTabs()
        {
            foreach (var tab in _tabs)
            {
                tab.gameObject.SetActive(false);
            }
        }
    }
}