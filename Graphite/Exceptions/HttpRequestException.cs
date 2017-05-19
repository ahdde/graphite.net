using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Serialization;

namespace ahd.Graphite.Exceptions
{
    /// <summary>
    /// A base class for exceptions thrown by the <see cref="System.Net.Http.HttpClient"/> and <see cref="System.Net.Http.HttpMessageHandler"/> classes.
    /// </summary>
    [Serializable]
    public class HttpRequestException: System.Net.Http.HttpRequestException
    {
        /// <summary>
        /// Initializes a new instance of the HttpRequestException class.
        /// </summary>
        /// <param name="statuscode">Statuscode of the response</param>
        /// <param name="response">response content</param>
        public HttpRequestException(HttpStatusCode statuscode, string response):base()
        {
            StatusCode = statuscode;
            Response = response;
        }

        /// <summary>
        /// Initializes a new instance of the HttpRequestException class with a specific message that describes the current exception
        /// </summary>
        /// <param name="response">response content</param>
        /// <param name="message">exception message</param>
        public HttpRequestException(string response, string message):base(message)
        {
            Response = response;
        }

        /// <summary>
        /// Initializes a new instance of the HttpRequestException class with a specific message that describes the current exception and an inner exception.
        /// </summary>
        /// <param name="response">response content</param>
        /// <param name="message">exception message</param>
        /// <param name="inner">inner exception</param>
        public HttpRequestException(string response, string message, Exception inner):base(message, inner)
        {
            Response = response;
        }

        /// <summary>
        /// statuscode of the HTTP response
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Content of the HTTP response
        /// </summary>
        public string Response { get; private set; }

        /// <summary>
        /// Security Critical. (Inherited from Exception.)
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the HttpRequestException class.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2236:CallBaseClassMethodsOnISerializableTypes", Justification = "base ctor is missing - blame Microsoft")]
        protected HttpRequestException(SerializationInfo info, StreamingContext context)
        {
            Response = info.GetString(nameof(Response));
            StatusCode = (HttpStatusCode)info.GetInt32(nameof(StatusCode));
        }
    }
}
