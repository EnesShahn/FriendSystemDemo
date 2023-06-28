using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SDK.Models;
using SDK.Modules;

using EnesShahn.Debugger;
using EnesShahn.ObjectPool;
using EnesShahn.PanelSystem;

using UnityEngine;
using UnityEngine.UI;
using SDK;
using DG.Tweening;
using TMPro;

namespace UI.Panels.Social
{
    public class FriendsTab : MonoBehaviour
    {
        [SerializeField] private FriendElement _friendElementPrefab;
        [SerializeField] private GameObject _friendScrollRect;
        [SerializeField] private GameObject _searchedFriendsScrollRect;
        [SerializeField] private GameObject _friendScrollRectContent;
        [SerializeField] private GameObject _searchedFriendsScrollRectContent;
        [SerializeField] private Button _addNewFriendButton;
        [SerializeField] private TMP_InputField _searchFriendsInputField;

        private Dictionary<string, FriendElement> _friendElementsMap = new Dictionary<string, FriendElement>();
        private Dictionary<string, FriendElement> _searchedFriendElementsMap = new Dictionary<string, FriendElement>();
        private ComponentPool<FriendElement> _elementPool;

        private void Awake()
        {
            _elementPool = new ComponentPool<FriendElement>(_friendElementPrefab, 10, 2);
            _addNewFriendButton.onClick.AddListener(OnAddNewFriendButtonClicked);
            _searchFriendsInputField.onValueChanged.AddListener(OnSearchFriendInputFieldValueChanged);
        }
        private void OnEnable()
        {
            Client.GetModule<FriendshipModule>().AddOnFriendAddedListener(OnFriendAdded);
            Client.GetModule<FriendshipModule>().AddOnFriendRemovedListener(OnFriendRemoved);
            Client.GetModule<FriendshipModule>().AddOnFriendLastOnlineTimeUpdated(UpdateFriendLastOnlineTime);

            var storedFriends = Client.GetModule<FriendshipModule>().GetStoredFriends();
            InitList(storedFriends);

            _searchedFriendsScrollRect.gameObject.SetActive(false);
            _friendScrollRect.gameObject.SetActive(true);

            Client.GetModule<FriendshipModule>().FetchFriends();
        }
        private void OnDisable()
        {
            Client.GetModule<FriendshipModule>().RemoveOnFriendAddedListener(OnFriendAdded);
            Client.GetModule<FriendshipModule>().RemoveOnFriendRemovedListener(OnFriendRemoved);
            Client.GetModule<FriendshipModule>().RemoveOnFriendLastOnlineTimeUpdated(UpdateFriendLastOnlineTime);
        }

        private void InitList(List<User> friends)
        {
            foreach (var friendElement in _friendElementsMap.Values.ToList())
            {
                RemoveFriendElement(friendElement.FriendData, false);
            }
            _friendElementsMap.Clear();

            foreach (var friend in friends)
            {
                AddFriendElement(friend, false);
            }
        }
        private void OnFriendAdded(User friend)
        {
            AddFriendElement(friend, true);
        }
        private void OnFriendRemoved(User friend)
        {
            RemoveFriendElement(friend, true);
        }
        private void AddFriendElement(User friend, bool animate)
        {
            var friendElement = _elementPool.RequestObject();
            friendElement.OpenChatButton.onClick.RemoveAllListeners();
            friendElement.OpenChatButton.onClick.AddListener(() => { OnAnyOpenChatButtonClicked(friend); });
            friendElement.RemoveFriendButton.onClick.RemoveAllListeners();
            friendElement.RemoveFriendButton.onClick.AddListener(() => { OnAnyRemoveFriendButtonClicked(friend); });

            friendElement.Init(friend, friend.UserID, friend.LastOnlineTime);
            friendElement.transform.SetParent(_friendScrollRectContent.transform);
            friendElement.transform.localScale = Vector3.one;
            if (animate)
                friendElement.UIAnimator.ShowUIItems();

            _friendElementsMap.Add(friend.UserID, friendElement);
        }
        private void RemoveFriendElement(User friend, bool animate)
        {
            if (!_friendElementsMap.ContainsKey(friend.UserID)) return;

            var friendElement = _friendElementsMap[friend.UserID];
            friendElement.OpenChatButton.onClick.RemoveAllListeners();
            friendElement.RemoveFriendButton.onClick.RemoveAllListeners();

            _friendElementsMap.Remove(friend.UserID);
            if (animate)
            {
                friendElement.UIAnimator.HideUIItems(() =>
                {
                    _elementPool.ReturnObject(friendElement);
                });
            }
            else
            {
                _elementPool.ReturnObject(friendElement);
            }
        }
        private void UpdateFriendLastOnlineTime(User friend, DateTime time)
        {
            if (!_friendElementsMap.ContainsKey(friend.UserID)) return;
            var friendElement = _friendElementsMap[friend.UserID];
            friendElement.UpdateLastOnlineTime(time);
        }

        #region Searching
        private void OnSearchFriendInputFieldValueChanged(string newInput)
        {
            var cleanedInput = newInput.Trim().ToLower();
            if (string.IsNullOrEmpty(cleanedInput))
            {
                _searchedFriendsScrollRect.gameObject.SetActive(false);
                _friendScrollRect.gameObject.SetActive(true);
            }
            else
            {
                _searchedFriendsScrollRect.gameObject.SetActive(true);
                _friendScrollRect.gameObject.SetActive(false);

                // Optimize
                foreach (var friendElement in _searchedFriendElementsMap.Values.ToList())
                {
                    RemoveSearchedFriendElement(friendElement.FriendData);
                }
                _searchedFriendElementsMap.Clear();

                var friends = Client.GetModule<FriendshipModule>().GetStoredFriends();
                var searchedFriends = friends.Where(f => f.UserID.ToLower().StartsWith(cleanedInput));
                foreach (var foundFriend in searchedFriends)
                {
                    AddSearchedFriendElement(foundFriend);
                }
            }
        }
        private void AddSearchedFriendElement(User friend)
        {
            var friendElement = _elementPool.RequestObject();
            friendElement.OpenChatButton.onClick.RemoveAllListeners();
            friendElement.OpenChatButton.onClick.AddListener(() => { OnAnyOpenChatButtonClicked(friend); });
            friendElement.RemoveFriendButton.onClick.RemoveAllListeners();
            friendElement.RemoveFriendButton.onClick.AddListener(() => { OnAnyRemoveFriendButtonClicked(friend); });

            friendElement.Init(friend, friend.UserID, friend.LastOnlineTime);
            friendElement.transform.SetParent(_searchedFriendsScrollRectContent.transform);
            friendElement.transform.localScale = Vector3.one;

            _searchedFriendElementsMap.Add(friend.UserID, friendElement);
        }
        private void RemoveSearchedFriendElement(User friend)
        {
            var friendElement = _searchedFriendElementsMap[friend.UserID];
            friendElement.OpenChatButton.onClick.RemoveAllListeners();
            friendElement.RemoveFriendButton.onClick.RemoveAllListeners();

            _searchedFriendElementsMap.Remove(friend.UserID);
            _elementPool.ReturnObject(friendElement);
        }
        #endregion

        private void OnAnyOpenChatButtonClicked(User user)
        {
            if(PanelManager.IsPanelVisable(PanelType.GroupPanel))
                PanelManager.Hide(PanelType.GroupPanel);

            var chatPanelData = new ChatPanelData(user);
            PanelManager.Show(PanelType.ChatPanel, chatPanelData);
        }
        private void OnAnyRemoveFriendButtonClicked(User user)
        {
            ConfirmationPopupPanelData panelData = new ConfirmationPopupPanelData("Remove friend?",
                async () =>
                {
                    var response = await Client.GetModule<FriendshipModule>().RemoveFriend(user.UserID);

                    GameDebugger.Log(response.Message);
                    // TODO: Show another popup saying friend removed or something...
                },
                () =>
                {
                });
            PanelManager.Show(PanelType.ConfirmationPopupPanel, panelData);
        }
        private void OnAddNewFriendButtonClicked()
        {
            InputPopupPanelData panelData = new InputPopupPanelData("Send friend request", "Enter player ID..",
                async (input) =>
                {
                    var response = await Client.GetModule<FriendshipModule>().SendFriendInvitation(input);
                    if (!response.Success)
                        GameDebugger.Log(response.Message);
                    // TODO: Show another popup saying invitation sent
                },
                () =>
                {
                });
            PanelManager.Show(PanelType.InputPopupPanel, panelData);
        }
    }
}