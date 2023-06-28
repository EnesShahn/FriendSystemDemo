namespace Server.Models.Responses
{
    public class Response<T> : BaseResponse
    {
        public T? Data { get; set; }
    }
}
