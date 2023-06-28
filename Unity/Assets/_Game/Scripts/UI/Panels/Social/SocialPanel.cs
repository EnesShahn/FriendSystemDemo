using EnesShahn.PanelSystem;
namespace UI.Panels.Social
{
    public class SocialPanel : BasePanel
    {
        public override PanelType PanelType => PanelType.SocialPanel;

        public override void ShowPanel(BasePanelData data)
        {
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