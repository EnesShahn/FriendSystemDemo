using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SDK;
using SDK.Models;
using SDK.Modules;

using EnesShahn.Debugger;
using EnesShahn.ObjectPool;
using EnesShahn.PanelSystem;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Social
{
    public class ChatPanelData : BasePanelData
    {
        private User _otherUser;
        public User OtherUser => _otherUser;

        public ChatPanelData(User otherUser)
        {
            _otherUser = otherUser;
        }
    }

    public class ChatPanel : BasePanel
    {
        [SerializeField] private MessageElement _messageElementPrefab;

        [SerializeField] private GameObject _messagesScrollRectContent;
        [SerializeField] private RectTransform _messagesScrollRectContentRT;

        [SerializeField] private Button _closePanelButton;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _submitButton;

        public override PanelType PanelType => PanelType.ChatPanel;

        private Dictionary<string, MessageElement> _messageElementMap = new Dictionary<string, MessageElement>();
        private ComponentPool<MessageElement> _messageElementPool;
        private ChatPanelData _chatPanelData;

        public override async void ShowPanel(BasePanelData data)
        {
            _chatPanelData = data as ChatPanelData;

            _closePanelButton.onClick.RemoveAllListeners();
            _closePanelButton.onClick.AddListener(HidePanel);

            _submitButton.onClick.RemoveAllListeners();
            _submitButton.onClick.AddListener(SubmitMessage);

            if (_messageElementPool == null)
                _messageElementPool = new ComponentPool<MessageElement>(_messageElementPrefab, 10, 2);

            var storedMessaged = Client.GetModule<FriendshipModule>().GetStoredMessages(_chatPanelData.OtherUser.UserID);
            InitMessageList(storedMessaged);

            Client.GetModule<FriendshipModule>().AddMessageChangeListener(_chatPanelData.OtherUser.UserID, OnMessageAdded);
            Client.GetModule<FriendshipModule>().FetchMessages(_chatPanelData.OtherUser.UserID);

            gameObject.SetActive(true);
            GetComponent<UIAnimator>().ShowUIItems();

            _messagesScrollRectContentRT.anchoredPosition = Vector2.zero;
        }
        public override void HidePanel()
        {
            Client.GetModule<FriendshipModule>().RemoveMessageChangeListener(_chatPanelData.OtherUser.UserID, OnMessageAdded);

            GetComponent<UIAnimator>().HideUIItems(() =>
            {
                gameObject.SetActive(false);
            });
        }

        private void InitMessageList(List<Message> messages)
        {
            foreach (var message in _messageElementMap.Values.ToList())
            {
                RemoveMessageElement(message.MessageData, false);
            }
            _messageElementMap.Clear();

            foreach (var message in messages)
            {
                AddMessageElement(message, false);
            }
        }
        private void OnMessageAdded(Message message)
        {
            AddMessageElement(message, true);
        }
        private void OnMessageRemoved(Message message)
        {
            RemoveMessageElement(message, true);
        }
        private void AddMessageElement(Message message, bool animate)
        {
            var newMessage = _messageElementPool.RequestObject();

            newMessage.Init(message, message.SenderID, message.Content);
            newMessage.transform.SetParent(_messagesScrollRectContent.transform);
            newMessage.transform.localScale = Vector3.one;
            if (animate)
                newMessage.UIAnimator.ShowUIItems();

            _messageElementMap.Add(message.MessageID, newMessage);
        }
        private void RemoveMessageElement(Message message, bool animate)
        {
            if (!_messageElementMap.ContainsKey(message.MessageID)) return;

            var messageElement = _messageElementMap[message.MessageID];

            _messageElementMap.Remove(message.MessageID);
            if (animate)
            {
                messageElement.UIAnimator.HideUIItems(() =>
                {
                    _messageElementPool.ReturnObject(messageElement);
                });
            }
            else
            {
                _messageElementPool.ReturnObject(messageElement);
            }
        }


        private async void SubmitMessage()
        {
            string inputTextCleaned = _inputField.text.Trim();
            if (String.IsNullOrEmpty(_inputField.text)) return;
            _inputField.text = "";

            var tempMessage = new Message
            {
                SenderID = Client.UserData.UserID,
                Content = inputTextCleaned,
            };
            tempMessage.MessageID = tempMessage.GetHashCode().ToString();

            AddMessageElement(tempMessage, true);

            var resposne = await Client.GetModule<FriendshipModule>().SendMessage(_chatPanelData.OtherUser.UserID, inputTextCleaned, true);
            if (!resposne.Success)
            {
                GameDebugger.Log("Submit message error: " + resposne.Message);
                RemoveMessageElement(tempMessage, true);
                return;
            }

            var messageElement = _messageElementMap[tempMessage.MessageID];
            _messageElementMap.Remove(tempMessage.MessageID);
            tempMessage.MessageID = resposne.Data.MessageID;
            _messageElementMap.Add(tempMessage.MessageID, messageElement);
        }

    }
}