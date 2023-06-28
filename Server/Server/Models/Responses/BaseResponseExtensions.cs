namespace Server.Models.Responses
{
    public class BaseResponseDefaults
    {
        public static BaseResponse InvalidParametersResponse
        {
            get
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = "Invalid parameters"
                };
            }
        }
    }
}
