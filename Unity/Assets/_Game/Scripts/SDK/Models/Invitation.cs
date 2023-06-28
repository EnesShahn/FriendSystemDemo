using System;

using SDK.Models.Enums;

namespace SDK.Models
{
    public class Invitation : IEquatable<Invitation>
    {
        public string InvitationID { get; set; }
        public InvitationType InvitationType { get; set; }
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }

        public bool Equals(Invitation other) => InvitationID == other.InvitationID;
        public override bool Equals(object obj) => Equals(obj as Invitation);
        public override int GetHashCode() => (InvitationID, SenderID, ReceiverID).GetHashCode();
    }

}
