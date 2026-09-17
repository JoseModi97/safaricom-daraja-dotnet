using System;
using System.Net.Http;
using Safaricom.Daraja.Auth;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Services;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja
{
    /// <summary>
    /// Master client for the Safaricom Daraja API. Provides strongly-typed access to every
    /// transactional area behind a single shared OAuth session.
    /// </summary>
    public class DarajaClient
    {
        private readonly DarajaTransport _transport;

        public DarajaConfig Config => _transport.Config;
        public DarajaAuthClient Auth => _transport.Auth;

        public StkPushClient StkPush { get; }
        public C2BClient C2B { get; }
        public B2CClient B2C { get; }
        public B2BClient B2B { get; }
        public ReversalClient Reversal { get; }
        public TransactionStatusClient TransactionStatus { get; }
        public AccountBalanceClient AccountBalance { get; }
        public RatibaClient Ratiba { get; }
        public LipaNaBongaClient LipaNaBonga { get; }
        public PullApiClient PullApi { get; }
        public QrCodeClient QrCode { get; }
        public BillManagerClient BillManager { get; }

        public DarajaClient(DarajaConfig config, HttpClient? httpClient = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            _transport = new DarajaTransport(config, httpClient);

            StkPush = new StkPushClient(_transport);
            C2B = new C2BClient(_transport);
            B2C = new B2CClient(_transport);
            B2B = new B2BClient(_transport);
            Reversal = new ReversalClient(_transport);
            TransactionStatus = new TransactionStatusClient(_transport);
            AccountBalance = new AccountBalanceClient(_transport);
            Ratiba = new RatibaClient(_transport);
            LipaNaBonga = new LipaNaBongaClient(_transport);
            PullApi = new PullApiClient(_transport);
            QrCode = new QrCodeClient(_transport);
            BillManager = new BillManagerClient(_transport);
        }

        /// <summary>Quick factory for a Sandbox client.</summary>
        public static DarajaClient CreateSandbox(string consumerKey, string consumerSecret)
        {
            return new DarajaClient(new DarajaConfig
            {
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Environment = DarajaEnvironment.Sandbox
            });
        }

        /// <summary>Quick factory for a Production client.</summary>
        public static DarajaClient CreateProduction(string consumerKey, string consumerSecret)
        {
            return new DarajaClient(new DarajaConfig
            {
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Environment = DarajaEnvironment.Production
            });
        }
    }
}
