
using System;

namespace SDK.Models
{
    public class User : IEquatable<User>
    {
        public string UserID { get; set; }
        public string Email { get; set; }
        public DateTime LastOnlineTime { get; set; }


        public bool Equals(User other) => UserID == other.UserID;
        public override bool Equals(object obj) => Equals(obj as User);
        public override int GetHashCode() => (UserID, Email).GetHashCode();
    }
}
