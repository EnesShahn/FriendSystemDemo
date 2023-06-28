using SDK.Models;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Social
{
    public class GroupElement : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Button _openChatButton;
        [SerializeField] private UIAnimator _uiAnimator;

        private Group _groupData;

        public Group GroupData => _groupData;
        public Button OpenChatButton => _openChatButton;
        public UIAnimator UIAnimator => _uiAnimator;

        public void Init(Group friend, string name, string lastOnlineTime)
        {
            _groupData = friend;
            _nameText.text = name;
        }

    }
}