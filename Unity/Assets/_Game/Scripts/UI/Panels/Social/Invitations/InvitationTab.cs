using System.Collections.Generic;
using System.Linq;

using SDK.Models;
using SDK.Modules;

using EnesShahn.ObjectPool;
using EnesShahn.Debugger;
using EnesShahn.PanelSystem;

using UnityEngine;
using SDK;

namespace UI.Panels.Social
{
    public class InvitationTab : MonoBehaviour
    {
        [SerializeField] private InvitationElement _invitationElementPrefab;
        [SerializeField] private GameObject _scrollRectContent;

        // Invitation ID -> Invitation Element
        private Dictionary<string, InvitationElement> _invitationElements = new Dictionary<string, InvitationElement>();
        private ComponentPool<InvitationElement> _elementPool;

        private void Awake()
        {
            _elementPool = new ComponentPool<InvitationElement>(_invitationElementPrefab, 10, 2);
        }
        private void OnEnable()
        {
            Client.GetModule<InvitationModule>().AddOnInvitationAddedListener(OnInvitationAdded);
            Client.GetModule<InvitationModule>().AddOnInvitationRemovedListener(OnInvitationRemoved);

            var storedInvitation = Client.GetModule<InvitationModule>().GetStoredInvitations();
            InitList(storedInvitation);

            Client.GetModule<InvitationModule>().FetchInvitations();
        }
        private void OnDisable()
        {
            Client.GetModule<InvitationModule>().RemoveOnInvitationAddedListener(OnInvitationAdded);
            Client.GetModule<InvitationModule>().RemoveOnInvitationRemovedListener(OnInvitationRemoved);
        }

        private void InitList(List<Invitation> invitations)
        {
            foreach (var invitationElement in _invitationElements.Values.ToList())
            {
                RemoveInvitationElement(invitationElement.InvitationData, false);
            }
            _invitationElements.Clear();

            foreach (var invitation in invitations)
            {
                AddInvitationElement(invitation, false);
            }
        }

        private void OnInvitationAdded(Invitation invitation)
        {
            AddInvitationElement(invitation, true);
        }
        private void OnInvitationRemoved(Invitation invitation)
        {
            RemoveInvitationElement(invitation, true);
        }
        private void AddInvitationElement(Invitation invitation, bool animate)
        {
            var invitationElement = _elementPool.RequestObject();

            invitationElement.AcceptButton.onClick.RemoveAllListeners();
            invitationElement.AcceptButton.onClick.AddListener(() => { OnAnyAcceptInvitation(invitation); });
            invitationElement.RejectButton.onClick.RemoveAllListeners();
            invitationElement.RejectButton.onClick.AddListener(() => { OnAnyRejectInvitation(invitation); });

            invitationElement.Init(invitation, invitation.SenderID, invitation.InvitationType);
            invitationElement.transform.SetParent(_scrollRectContent.transform);
            invitationElement.transform.localScale = Vector3.one;
            if (animate)
                invitationElement.UIAnimator.ShowUIItems();

            _invitationElements.Add(invitation.InvitationID, invitationElement);
        }
        private void RemoveInvitationElement(Invitation invitation, bool animate)
        {
            if (!_invitationElements.ContainsKey(invitation.InvitationID)) return;

            var invitationElement = _invitationElements[invitation.InvitationID];
            invitationElement.AcceptButton.onClick.RemoveAllListeners();
            invitationElement.RejectButton.onClick.RemoveAllListeners();

            _invitationElements.Remove(invitation.InvitationID);
            if (animate)
            {
                invitationElement.UIAnimator.HideUIItems(() =>
                {
                    _elementPool.ReturnObject(invitationElement);
                });
            }
            else
            {
                _elementPool.ReturnObject(invitationElement);
            }
        }

        private async void OnAnyAcceptInvitation(Invitation invitation)
        {
            var response = await Client.GetModule<InvitationModule>().AcceptInvitation(invitation.InvitationID);

            if (!response.Success)
            {
                GameDebugger.Log("Accept Invitation error: " + response.Message);
                var errorSBPanelData = new SingleButtonPopupPanelData("Accept Invitation error: " + response.Message, "Ok", null);
                PanelManager.Show(PanelType.SingleButtonPopupPanel, errorSBPanelData);
                return;
            }

            RemoveInvitationElement(invitation, true);
        }
        private async void OnAnyRejectInvitation(Invitation invitation)
        {
            var response = await Client.GetModule<InvitationModule>().RejectInvitation(invitation.InvitationID);
            if (!response.Success)
            {
                GameDebugger.Log("Reject Invitation error: " + response.Message);
                var errorSBPanelData = new SingleButtonPopupPanelData("Reject Invitation error: " + response.Message, "Ok", null);
                PanelManager.Show(PanelType.SingleButtonPopupPanel, errorSBPanelData);
                return;
            }

            RemoveInvitationElement(invitation, true);
        }
    }
}