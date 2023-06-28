using System.Collections;
using System.Collections.Generic;

using SDK.Models;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Social
{
    public class MessageElement : MonoBehaviour
    {
        [SerializeField] private Image _profileImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private UIAnimator _uiAnimator;

        private Message _messageData;

        public Message MessageData => _messageData;
        public UIAnimator UIAnimator => _uiAnimator;

        public void Init(Message messageData, string senderName, string messageContent)
        {
            _messageData = messageData;
            _nameText.text = senderName;
            _messageText.text = messageContent;
        }
    }
}