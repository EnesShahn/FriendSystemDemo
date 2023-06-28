using System.ComponentModel.DataAnnotations;

namespace Server.Models.Requests
{
    public class InvitationRequestParameters
    {
        [Required]
        public string InvitationID { get; set;}

    }
}
