using System;

namespace Safaricom.Daraja.Exceptions
{
    /// <summary>
    /// Thrown when OAuth token acquisition against Daraja fails.
    /// </summary>
    public class DarajaAuthException : DarajaApiException
    {
        public DarajaAuthException(string message, int statusCode = 0, string? rawResponse = null, Exception? innerException = null)
            : base(message, statusCode, rawResponse: rawResponse, innerException: innerException)
        {
        }
    }
}
