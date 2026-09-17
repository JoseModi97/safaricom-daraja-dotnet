using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.IoT.Models;
using Safaricom.Daraja.IoT.Transport;
using Safaricom.Daraja.Models;

namespace Safaricom.Daraja.IoT.Services
{
    /// <summary>
    /// SIM-portal messaging (<c>/simportal/v1/*messages*</c>). This is an internal/enterprise
    /// Safaricom product with no public documentation, so responses are returned as raw JSON
    /// (<see cref="DarajaRawResult"/>) rather than a typed shape this SDK can't verify.
    /// </summary>
    public class SimMessagingClient
    {
        private readonly IoTTransport _transport;

        public SimMessagingClient(IoTTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<DarajaRawResult> SearchMessagesAsync(SearchMessagesRequest request, int pageNo = 1, int pageSize = 10, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var query = PagingQuery(pageNo, pageSize);
            var json = await _transport.PostAsync("/simportal/v1/searchmessages", query, request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> FilterMessagesAsync(FilterMessagesRequest request, int pageNo = 1, int pageSize = 10, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var query = PagingQuery(pageNo, pageSize);
            var json = await _transport.PostAsync("/simportal/v1/filtermessages", query, request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> GetAllMessagesAsync(GetAllMessagesRequest request, int pageNo = 1, int pageSize = 10, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var query = PagingQuery(pageNo, pageSize);
            var json = await _transport.PostAsync("/simportal/v1/getallmessages", query, request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> SendSingleMessageAsync(SendSingleMessageRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/simportal/v1/sendsinglemessage", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> DeleteMessageAsync(DeleteMessageRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/simportal/v1/deletemessage", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> DeleteMessageThreadAsync(DeleteMessageThreadRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/simportal/v1/deleteMessageThread", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        private static Dictionary<string, string> PagingQuery(int pageNo, int pageSize) => new Dictionary<string, string>
        {
            ["pageNo"] = pageNo.ToString(),
            ["pageSize"] = pageSize.ToString()
        };
    }
}
