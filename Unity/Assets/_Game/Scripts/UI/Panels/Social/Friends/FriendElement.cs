using System;

using SDK.Models;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Social
{
    public class FriendElement : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _lastOnlineText;
        [SerializeField] private Button _openChatButton;
        [SerializeField] private Button _removeFriendButton;
        [SerializeField] private UIAnimator _uiAnimator;

        private User _friendData;

        public User FriendData => _friendData;
        public Button OpenChatButton => _openChatButton;
        public Button RemoveFriendButton => _removeFriendButton;
        public UIAnimator UIAnimator => _uiAnimator;

        public void Init(User friend, string name, DateTime lastOnlineTime)
        {
            _friendData = friend;
            _nameText.text = name;
            UpdateLastOnlineTime(lastOnlineTime);
        }

        public void UpdateLastOnlineTime(DateTime lastOnlineTime)
        {
            int minutesSinceOnline = (int)((DateTime.UtcNow - lastOnlineTime).TotalSeconds / 60);
            _lastOnlineText.text = minutesSinceOnline > 1 ? $"Last online {minutesSinceOnline} minutes ago" : "Online";
        }
    }
}