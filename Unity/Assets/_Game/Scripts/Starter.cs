using EnesShahn.PanelSystem;

using UnityEngine;

public class Starter : MonoBehaviour
{
    private void Start()
    {
        PanelManager.Show(PanelType.LoginPanel, null);
    }
}
