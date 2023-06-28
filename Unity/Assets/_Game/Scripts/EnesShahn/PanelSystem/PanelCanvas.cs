using UnityEngine;

namespace EnesShahn.PanelSystem
{
    public class PanelCanvas : MonoBehaviour
    {
        public static PanelCanvas Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}