using System;

using SDK.Models;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Social
{
    public class MemberElement : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _lastOnlineText;
        [SerializeField] private Button _memberKickOrLeaveButton;
        [SerializeField] private UIAnimator _uiAnimator;

        private User _memberData;

        public User MemberData => _memberData;
        public Button MemberKickOrLeaveButton => _memberKickOrLeaveButton;
        public UIAnimator UIAnimator => _uiAnimator;

        public void Init(User member, string name, DateTime lastOnlineTime, bool enableKick)
        {
            _memberData = member;
            _nameText.text = name;
            UpdateLastOnlineTime(lastOnlineTime);
            _memberKickOrLeaveButton.gameObject.SetActive(enableKick);
        }

        public void UpdateLastOnlineTime(DateTime lastOnlineTime)
        {
            int minutesSinceOnline = (int)((DateTime.UtcNow - lastOnlineTime).TotalSeconds / 60);
            _lastOnlineText.text = minutesSinceOnline > 1 ? $"Last online {minutesSinceOnline} minutes ago" : "Online";
        }

    }
}