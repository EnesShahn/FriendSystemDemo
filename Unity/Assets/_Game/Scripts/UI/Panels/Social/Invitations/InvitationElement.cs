using SDK.Models;
using SDK.Models.Enums;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Social
{
    public class InvitationElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _invitationTypeText;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _rejectButton;
        [SerializeField] private UIAnimator _uiAnimator;

        private Invitation _invitationData;

        public Invitation InvitationData => _invitationData;
        public Button AcceptButton => _acceptButton;
        public Button RejectButton => _rejectButton;
        public UIAnimator UIAnimator => _uiAnimator;

        public void Init(Invitation invitation, string name, InvitationType invitationType)
        {
            _invitationData = invitation;
            _nameText.text = name;
            _invitationTypeText.text = invitationType == InvitationType.Friendship ? "Friend request" : "Group invite";
        }
    }
}