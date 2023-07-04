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

namespace UI.Panels.Social
{
    public class GroupsTab : MonoBehaviour
    {
        [SerializeField] private GroupElement _groupElementPrefab;
        [SerializeField] private GameObject _scrollRectContent;
        [SerializeField] private Button _createNewGroupButton;

        private Dictionary<string, GroupElement> _groupElementMap = new Dictionary<string, GroupElement>();
        private ComponentPool<GroupElement> _elementPool;

        private void Awake()
        {
            _elementPool = new ComponentPool<GroupElement>(_groupElementPrefab, 10, 2);
            _createNewGroupButton.onClick.AddListener(OnCreateNewGroupButtonClicked);
        }
        private void OnEnable()
        {
            Client.GetModule<GroupModule>().AddOnGroupAddedListener(OnGroupAdded);
            Client.GetModule<GroupModule>().AddOnGroupRemovedListener(OnGroupRemoved);

            var storedGroups = Client.GetModule<GroupModule>().GetStoredGroups();
            InitList(storedGroups);

            Client.GetModule<GroupModule>().FetchGroups();
        }
        private void OnDisable()
        {
            Client.GetModule<GroupModule>().RemoveOnGroupAddedListener(OnGroupAdded);
            Client.GetModule<GroupModule>().RemoveOnGroupRemovedListener(OnGroupRemoved);
        }

        private void InitList(List<Group> groups)
        {
            foreach (var groupElement in _groupElementMap.Values.ToList())
            {
                RemoveGroupElement(groupElement.GroupData, false);
            }
            _groupElementMap.Clear();

            foreach (var group in groups)
            {
                AddGroupElement(group, false);
            }
        }
        private void OnGroupAdded(Group group)
        {
            AddGroupElement(group, true);
        }
        private void OnGroupRemoved(Group group)
        {
            RemoveGroupElement(group, true);
        }
        private void AddGroupElement(Group group, bool animate)
        {
            var groupElement = _elementPool.RequestObject();

            groupElement.OpenChatButton.onClick.RemoveAllListeners();
            groupElement.OpenChatButton.onClick.AddListener(() => { OnAnyOpenChatButtonClicked(group); });

            groupElement.Init(group, group.Name, null);
            groupElement.transform.SetParent(_scrollRectContent.transform);
            groupElement.transform.localScale = Vector3.one;
            if (animate)
                groupElement.UIAnimator.ShowUIItems();

            _groupElementMap.Add(group.GroupID, groupElement);
        }
        private void RemoveGroupElement(Group group, bool animate)
        {
            if (!_groupElementMap.ContainsKey(group.GroupID)) return;

            var groupElement = _groupElementMap[group.GroupID];
            groupElement.OpenChatButton.onClick.RemoveAllListeners();

            _groupElementMap.Remove(group.GroupID);
            if (animate)
            {
                groupElement.UIAnimator.HideUIItems(() =>
                {
                    _elementPool.ReturnObject(groupElement);
                });
            }
            else
            {
                _elementPool.ReturnObject(groupElement);
            }
        }
        private void OnAnyOpenChatButtonClicked(Group group)
        {
            if (PanelManager.IsPanelVisable(PanelType.ChatPanel))
                PanelManager.Hide(PanelType.ChatPanel);

            var chatPanelData = new GroupPanelData(group);
            PanelManager.Show(PanelType.GroupPanel, chatPanelData);
        }
        private void OnCreateNewGroupButtonClicked()
        {
            InputPopupPanelData panelData = new InputPopupPanelData("Create new group", "Enter group name..",
                async (input) =>
                {
                    var response = await Client.GetModule<GroupModule>().CreateGroup(input);
                    if (!response.Success)
                    {
                        GameDebugger.Log("Create Group error: " + response.Message);
                        var errorSBPanelData = new SingleButtonPopupPanelData("Create Group error: " + response.Message, "Ok", null);
                        PanelManager.Show(PanelType.SingleButtonPopupPanel, errorSBPanelData);
                        return;
                    }

                    var sbPanelData = new SingleButtonPopupPanelData("Group created.", "Ok", null);
                    PanelManager.Show(PanelType.SingleButtonPopupPanel, sbPanelData);

                    AddGroupElement(response.Data, true);
                },
                () =>
                {
                });
            PanelManager.Show(PanelType.InputPopupPanel, panelData);
        }
    }
}