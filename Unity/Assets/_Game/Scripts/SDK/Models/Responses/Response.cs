namespace SDK.Models.Responses
{
    public class Response<T> : BaseResponse
    {
        public T? Data { get; set; }
    }
}
