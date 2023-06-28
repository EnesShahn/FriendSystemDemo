using UnityEngine;

namespace EnesShahn.PanelSystem
{
    [CreateAssetMenu(fileName = "PanelInfoCollection", menuName = "Game/PanelInfoCollection")]

    public class PanelInfoCollection : ScriptableObject
    {
        public PanelInfo[] items;
    }
}