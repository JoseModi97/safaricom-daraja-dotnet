using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.IoT.Models;
using Safaricom.Daraja.IoT.Transport;
using Safaricom.Daraja.Models;

namespace Safaricom.Daraja.IoT.Services
{
    /// <summary>IMSI CheckATI (Authentication Triplet Index) checks, v1 and v2 ("SWAP CheckATI").</summary>
    public class ImsiClient
    {
        private readonly IoTTransport _transport;

        public ImsiClient(IoTTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<DarajaRawResult> CheckAtiV1Async(CheckAtiRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/imsi/v1/checkATI", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        /// <summary>"SWAP CheckATI" in the reference collection — same operation, v2 endpoint.</summary>
        public async Task<DarajaRawResult> CheckAtiV2Async(CheckAtiRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/imsi/v2/checkATI", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }
    }
}
