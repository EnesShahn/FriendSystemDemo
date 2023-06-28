using System;
using System.Collections.Generic;
using System.Linq;

using SDK.Models;
using SDK.Modules;

using EnesShahn.Debugger;
using EnesShahn.ObjectPool;
using EnesShahn.PanelSystem;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using SDK;

namespace UI.Panels.Social
{
    public class GroupPanelData : BasePanelData
    {
        private Group _group;
        public Group Group => _group;

        public GroupPanelData(Group otherUser)
        {
            _group = otherUser;
        }
    }

    public class GroupPanel : BasePanel
    {
        [SerializeField] private MemberElement _memberElementPrefab;
        [SerializeField] private MessageElement _messageElementPrefab;

        [SerializeField] private GameObject _membersScrollRect;
        [SerializeField] private GameObject _membersScrollRectContent;
        [SerializeField] private RectTransform _membersScrollRectContentRT;

        [SerializeField] private GameObject _searchedMembersScrollRect;
        [SerializeField] private GameObject _searchedMembersScrollRectContent;
        [SerializeField] private RectTransform _searchedMembersScrollRectContentRT;

        [SerializeField] private GameObject _messagesScrollRectContent;
        [SerializeField] private RectTransform _messagesScrollRectContentRT;
        
        [SerializeField] private Button _closePanelButton;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _submitButton;
        [SerializeField] private Button _inviteMemberButton;
        [SerializeField] private TMP_InputField _searchMembersInputField;

        public override PanelType PanelType => PanelType.ChatPanel;

        private Dictionary<string, MemberElement> _memberElementMap = new Dictionary<string, MemberElement>();
        private Dictionary<string, MemberElement> _searchedMemberElementMap = new Dictionary<string, MemberElement>();
        private Dictionary<string, MessageElement> _messageElementMap = new Dictionary<string, MessageElement>();
        private ComponentPool<MemberElement> _memberElementPool;
        private ComponentPool<MessageElement> _messageElementPool;
        private GroupPanelData _chatPanelData;

        private void Awake()
        {
            _closePanelButton.onClick.AddListener(HidePanel);
            _submitButton.onClick.AddListener(SubmitMessage);
            _inviteMemberButton.onClick.AddListener(OnInviteButtonClicked);
            _searchMembersInputField.onValueChanged.AddListener(OnSearchFriendInputFieldValueChanged);
        }
        public override async void ShowPanel(BasePanelData data)
        {
            _chatPanelData = data as GroupPanelData;

            if (_messageElementPool == null)
                _messageElementPool = new ComponentPool<MessageElement>(_messageElementPrefab, 10, 2);
            if (_memberElementPool == null)
                _memberElementPool = new ComponentPool<MemberElement>(_memberElementPrefab, 10, 2);

            var storedMessaged = Client.GetModule<GroupModule>().GetStoredMessages(_chatPanelData.Group.GroupID);
            var storedMembers = Client.GetModule<GroupModule>().GetStoredMembers(_chatPanelData.Group.GroupID);
            InitMessageList(storedMessaged);
            InitMemberList(storedMembers);

            Client.GetModule<GroupModule>().AddOnMessageAddedListener(_chatPanelData.Group.GroupID, OnMessageAdded);
            Client.GetModule<GroupModule>().AddOnMemberLastOnlineTimeUpdated(_chatPanelData.Group.GroupID, UpdateMemberLastOnlineTime);

            Client.GetModule<GroupModule>().AddOnMemberAddedListener(_chatPanelData.Group.GroupID, OnMemberAdded);
            Client.GetModule<GroupModule>().AddOnMemberRemovedListener(_chatPanelData.Group.GroupID, OnMemberRemoved);

            Client.GetModule<GroupModule>().FetchGroupMembers(_chatPanelData.Group.GroupID);
            Client.GetModule<GroupModule>().FetchGroupMessages(_chatPanelData.Group.GroupID);

            _searchedMembersScrollRect.gameObject.SetActive(false);
            _membersScrollRect.gameObject.SetActive(true);

            gameObject.SetActive(true);
            GetComponent<UIAnimator>().ShowUIItems();

            _membersScrollRectContentRT.anchoredPosition = Vector2.zero;
            _searchedMembersScrollRectContentRT.anchoredPosition = Vector2.zero;
            _messagesScrollRectContentRT.anchoredPosition = Vector2.zero;
        }
        public override void HidePanel()
        {
            Client.GetModule<GroupModule>().RemoveOnMessageAddedListener(_chatPanelData.Group.GroupID, OnMessageAdded);
            Client.GetModule<GroupModule>().RemoveOnMemberLastOnlineTimeUpdated(_chatPanelData.Group.GroupID, UpdateMemberLastOnlineTime);

            Client.GetModule<GroupModule>().RemoveOnMemberAddedListener(_chatPanelData.Group.GroupID, OnMemberAdded);
            Client.GetModule<GroupModule>().RemoveOnMemberRemovedListener(_chatPanelData.Group.GroupID, OnMemberRemoved);

            GetComponent<UIAnimator>().HideUIItems(() =>
            {
                gameObject.SetActive(false);
            });
        }

        private void InitMessageList(List<Message> messages)
        {
            foreach (var messageElement in _messageElementMap.Values.ToList())
            {
                RemoveMessageElement(messageElement.MessageData, false);
            }
            _messageElementMap.Clear();

            foreach (var message in messages)
            {
                AddMessageElement(message, false);
            }
        }
        private void InitMemberList(List<User> members)
        {
            foreach (var memberElement in _memberElementMap.Values.ToList())
            {
                RemoveMemberElement(memberElement.MemberData, false);
            }
            _memberElementMap.Clear();

            foreach (var member in members)
            {
                AddMemberElement(member, false);
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
            var newMessageElement = _messageElementPool.RequestObject();
            newMessageElement.Init(message, message.SenderID, message.Content);
            newMessageElement.transform.SetParent(_messagesScrollRectContent.transform);
            newMessageElement.transform.localScale = Vector3.one;
            if (animate)
                newMessageElement.UIAnimator.ShowUIItems();

            _messageElementMap.Add(message.MessageID, newMessageElement);
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
        private void OnMemberAdded(User member)
        {
            AddMemberElement(member, true);
        }
        private void OnMemberRemoved(User member)
        {
            RemoveMemberElement(member, true);
        }
        private void AddMemberElement(User member, bool animate)
        {
            bool isSelf = member.UserID == Client.UserData.UserID;
            bool isCreator = _chatPanelData.Group.CreatorUID == Client.UserData.UserID;

            var newMemberElement = _memberElementPool.RequestObject();

            newMemberElement.Init(member, member.UserID, member.LastOnlineTime, isCreator || isSelf);
            newMemberElement.MemberKickOrLeaveButton.onClick.RemoveAllListeners();
            newMemberElement.MemberKickOrLeaveButton.onClick.AddListener(() => { OnAnyMemberElementKickOrLeaveButtonClicked(member.UserID); });
            newMemberElement.transform.SetParent(_membersScrollRectContent.transform);
            newMemberElement.transform.localScale = Vector3.one;
            if (animate)
                newMemberElement.UIAnimator.ShowUIItems();

            _memberElementMap.Add(member.UserID, newMemberElement);
        }
        private void RemoveMemberElement(User member, bool animate)
        {
            if (!_memberElementMap.ContainsKey(member.UserID)) return;

            var memberElement = _memberElementMap[member.UserID];
            memberElement.MemberKickOrLeaveButton.onClick.RemoveAllListeners();

            _memberElementMap.Remove(member.UserID);
            if (animate)
            {
                memberElement.UIAnimator.HideUIItems(() =>
                {
                    _memberElementPool.ReturnObject(memberElement);
                });
            }
            else
            {
                _memberElementPool.ReturnObject(memberElement);
            }
        }
        private void UpdateMemberLastOnlineTime(User member, DateTime time)
        {
            if (!_memberElementMap.ContainsKey(member.UserID)) return;
            var memberElement = _memberElementMap[member.UserID];
            memberElement.UpdateLastOnlineTime(time);
        }

        #region Searching
        private void OnSearchFriendInputFieldValueChanged(string newInput)
        {
            var cleanedInput = newInput.Trim().ToLower();
            if (string.IsNullOrEmpty(cleanedInput))
            {
                _searchedMembersScrollRect.gameObject.SetActive(false);
                _membersScrollRect.gameObject.SetActive(true);
            }
            else
            {
                _searchedMembersScrollRect.gameObject.SetActive(true);
                _membersScrollRect.gameObject.SetActive(false);

                // Optimize
                foreach (var memberElement in _searchedMemberElementMap.Values.ToList())
                {
                    RemoveSearchedFriendElement(memberElement.MemberData);
                }
                _searchedMemberElementMap.Clear();

                var members = Client.GetModule<GroupModule>().GetStoredMembers(_chatPanelData.Group.GroupID);
                var searchedFriends = members.Where(f => f.UserID.ToLower().StartsWith(cleanedInput));
                foreach (var foundFriend in searchedFriends)
                {
                    AddSearchedFriendElement(foundFriend);
                }
            }
        }
        private void AddSearchedFriendElement(User member)
        {
            var memberElement = _memberElementPool.RequestObject();
            memberElement.MemberKickOrLeaveButton.onClick.RemoveAllListeners();

            memberElement.Init(member, member.UserID, member.LastOnlineTime, false);
            memberElement.transform.SetParent(_searchedMembersScrollRectContent.transform);
            memberElement.transform.localScale = Vector3.one;

            _searchedMemberElementMap.Add(member.UserID, memberElement);
        }
        private void RemoveSearchedFriendElement(User member)
        {
            var memberElement = _searchedMemberElementMap[member.UserID];
            memberElement.MemberKickOrLeaveButton.onClick.RemoveAllListeners();

            _searchedMemberElementMap.Remove(member.UserID);
            _memberElementPool.ReturnObject(memberElement);
        }
        #endregion

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

            var resposne = await Client.GetModule<GroupModule>().SendMessage(_chatPanelData.Group.GroupID, inputTextCleaned, true);
            if (!resposne.Success)
            {
                GameDebugger.Log("Submit Group message error: " + resposne.Message);
                RemoveMessageElement(tempMessage, true);
                return;
            }

            var messageElement = _messageElementMap[tempMessage.MessageID];
            _messageElementMap.Remove(tempMessage.MessageID);
            tempMessage.MessageID = resposne.Data.MessageID;
            _messageElementMap.Add(tempMessage.MessageID, messageElement);
        }
        private async void OnInviteButtonClicked()
        {
            InputPopupPanelData panelData = new InputPopupPanelData("Send invite to player", "Enter player ID..",
                async (input) =>
                {
                    var response = await Client.GetModule<GroupModule>().SendGroupInvitation(_chatPanelData.Group.GroupID, input);
                    if (!response.Success)
                        GameDebugger.Log(response.Message);
                    // TODO: Show another popup saying invitation sent
                },
                () =>
                {
                });
            PanelManager.Show(PanelType.InputPopupPanel, panelData);
        }
        private void OnAnyMemberElementKickOrLeaveButtonClicked(string memberId)
        {
            bool isSelf = memberId == Client.UserData.UserID;
            bool isCreator = _chatPanelData.Group.CreatorUID == Client.UserData.UserID;

            string confirmationText = "";

            if (isCreator && !isSelf)
            {
                confirmationText = $"Kick Member {memberId}?";
            }
            else
            {
                confirmationText = $"Leave group?";
            }

            ConfirmationPopupPanelData panelData = new ConfirmationPopupPanelData(confirmationText,
                async () =>
                {
                    if (isCreator && !isSelf)
                    {
                        var response = await Client.GetModule<GroupModule>().KickMember(_chatPanelData.Group.GroupID, memberId);
                        if (!response.Success)
                        {
                            GameDebugger.Log(response.Message);
                        }
                    }
                    else
                    {
                        var response = await Client.GetModule<GroupModule>().LeaveGroup(_chatPanelData.Group.GroupID);
                        if (!response.Success)
                        {
                            GameDebugger.Log(response.Message);
                        }
                        HidePanel();
                    }
                },
                () =>
                {
                });
            PanelManager.Show(PanelType.ConfirmationPopupPanel, panelData);
        }
    }
}