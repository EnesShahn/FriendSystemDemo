using UnityEngine;

namespace EnesShahn.PanelSystem
{
    public abstract class BasePanelData { }

    public abstract class BasePanel : MonoBehaviour
    {
        public abstract PanelType PanelType { get; }

        public virtual void ShowPanel(BasePanelData data) { }
        public virtual void HidePanel() { }

        public virtual void OnPanelShow() { }
        public virtual void OnPanelHide() { }
    }
}