using System.ComponentModel.DataAnnotations;

namespace Server.Models.Requests
{
    public class FriendshipRequestParameters
    {
        [Required]
        public string FriendID { get; set;}
        public string? Message { get; set;}

    }
}
