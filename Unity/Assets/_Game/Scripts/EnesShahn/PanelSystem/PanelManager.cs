using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace EnesShahn.PanelSystem
{
    public static class PanelManager
    {
        private static Dictionary<PanelType, BasePanel> panelsMap = new Dictionary<PanelType, BasePanel>();
        private static Dictionary<PanelType, BasePanel> prefabMap = new Dictionary<PanelType, BasePanel>();

        public static UnityEvent<PanelType> OnPanelShown = new UnityEvent<PanelType>();
        public static UnityEvent<PanelType> OnPanelHidden = new UnityEvent<PanelType>();

        private static PanelInfoCollection panelInfoCollection;
        private static string PanelInfoCollectionPath = "Configurations/PanelInfoCollection";

        static PanelManager()
        {
            Load();
        }

        private static void Load()
        {
            panelInfoCollection = Resources.Load<PanelInfoCollection>(PanelInfoCollectionPath);

            foreach (var item in panelInfoCollection.items)
            {
                if (prefabMap.ContainsKey(item.PanelType))
                {
                    Debug.Log($"Duplicate PanelType {item.PanelType} in the collection");
                    continue;
                }

                prefabMap[item.PanelType] = item.PanelPrefab;
            }
        }
        public static void Show(PanelType panelType, BasePanelData data)
        {
            CreatePanelIfNotExist(panelType);

            if (IsPanelVisable(panelType)) return;

            panelsMap[panelType].ShowPanel(data);
            OnPanelShown.Invoke(panelType);
            panelsMap[panelType].OnPanelShow();
        }
        public static void Hide(PanelType panelType)
        {
            CreatePanelIfNotExist(panelType);

            if (!IsPanelVisable(panelType)) return;

            panelsMap[panelType].HidePanel();
            OnPanelHidden.Invoke(panelType);
            panelsMap[panelType].OnPanelHide();
        }

        private static void CreatePanelIfNotExist(PanelType panelType)
        {
            if (!panelsMap.ContainsKey(panelType))
            {
                BasePanel panel = GameObject.Instantiate<BasePanel>(prefabMap[panelType], PanelCanvas.Instance.transform);
                panel.gameObject.SetActive(false);
                panelsMap.Add(panelType, panel);
            }
        }

        public static bool IsPanelVisable(PanelType panelType)
        {
            if (!panelsMap.ContainsKey(panelType)) return false;
            return panelsMap[panelType].enabled && panelsMap[panelType].gameObject.activeSelf;
        }
    }
}