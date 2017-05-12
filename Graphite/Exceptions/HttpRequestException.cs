using System;
using System.Net;
using System.Runtime.Serialization;

namespace ahd.Graphite.Exceptions
{
    [Serializable]
    public class HttpRequestException: System.Net.Http.HttpRequestException
    {
        public HttpRequestException(HttpStatusCode statuscode, string response):base()
        {
            StatusCode = statuscode;
            Response = response;
        }

        public HttpRequestException(string response, string message):base(message)
        {
            Response = response;
        }

        public HttpRequestException(string response, string message, Exception inner):base(message, inner)
        {
            Response = response;
        }

        public HttpStatusCode StatusCode { get; private set; }

        public string Response { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(Response), Response);
            info.AddValue(nameof(StatusCode), (int)StatusCode);

            base.GetObjectData(info, context);
        }

        protected HttpRequestException(SerializationInfo info, StreamingContext context)
        {
            Response = info.GetString(nameof(Response));
            StatusCode = (HttpStatusCode)info.GetInt32(nameof(StatusCode));
        }
    }
}
