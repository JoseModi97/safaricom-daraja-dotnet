using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.IoT.Models;
using Safaricom.Daraja.IoT.Transport;
using Safaricom.Daraja.Models;

namespace Safaricom.Daraja.IoT.Services
{
    /// <summary>
    /// SIM-portal SIM/subscriber operations (<c>/simportal/v1/*</c>). Responses are returned as
    /// raw JSON (<see cref="DarajaRawResult"/>) — see <see cref="SimMessagingClient"/> for why.
    /// </summary>
    public class SimOperationsClient
    {
        private readonly IoTTransport _transport;

        public SimOperationsClient(IoTTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<DarajaRawResult> AllSimsAsync(AllSimsRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/simportal/v1/allsims", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> QueryLifeCycleStatusAsync(QueryLifeCycleStatusRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/simportal/v1/queryLifeCycleStatus", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> QueryCustomerInfoAsync(QueryCustomerInfoRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/simportal/v1/querycustomerinfo", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> ActivateAsync(SimActivationRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/simportal/v1/simactivation", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> GetActivationTrendsAsync(GetActivationTrendsRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/simportal/v1/getactivationtrends", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> RenameAssetAsync(RenameAssetRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/simportal/v1/renameasset", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> GetLocationInfoAsync(GetLocationInfoRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/simportal/v1/getlocationinfo", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        /// <summary>
        /// Suspends or unsuspends a subscriber. The reference collection includes an
        /// <c>X-Identity</c> header (the operator's email) for this call specifically — pass it
        /// via <paramref name="options"/>.
        /// </summary>
        public async Task<DarajaRawResult> SuspendOrUnsuspendAsync(SuspendUnsuspendSubRequest request, IoTCallOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync("/simportal/v1/suspend_unsuspend_sub", request, options, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }
    }
}
