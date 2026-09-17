using System;
using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    /// <summary>The transaction type encoded into a Dynamic QR code.</summary>
    public enum QrTransactionCode
    {
        /// <summary>Buy Goods. Wire value "BG".</summary>
        BuyGoods,

        /// <summary>Withdraw cash at an agent. Wire value "WA".</summary>
        WithdrawAtAgent,

        /// <summary>Paybill. Wire value "PB".</summary>
        PayBill,

        /// <summary>Send money to a phone number. Wire value "SM".</summary>
        SendMoney,

        /// <summary>Send to a business account. Wire value "SB".</summary>
        SendToBusiness
    }

    internal static class QrTransactionCodeMap
    {
        public static string ToWireValue(this QrTransactionCode code) => code switch
        {
            QrTransactionCode.BuyGoods => "BG",
            QrTransactionCode.WithdrawAtAgent => "WA",
            QrTransactionCode.PayBill => "PB",
            QrTransactionCode.SendMoney => "SM",
            QrTransactionCode.SendToBusiness => "SB",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };
    }

    /// <summary>Generates a scannable M-Pesa Dynamic QR code for a specific amount and merchant.</summary>
    public class QrCodeRequest
    {
        public string MerchantName { get; set; } = string.Empty;

        public string ReferenceNo { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public QrTransactionCode TransactionCode { get; set; } = QrTransactionCode.PayBill;

        /// <summary>Credit Party Identifier: the till/paybill/agent number credited by a scan.</summary>
        public string CreditPartyIdentifier { get; set; } = string.Empty;

        /// <summary>Rendered QR image size in pixels. Defaults to 300.</summary>
        public int Size { get; set; } = 300;
    }

    internal class QrCodeWireRequest
    {
        [JsonPropertyName("MerchantName")]
        public string MerchantName { get; set; } = string.Empty;

        [JsonPropertyName("RefNo")]
        public string RefNo { get; set; } = string.Empty;

        [JsonPropertyName("Amount")]
        public int Amount { get; set; }

        [JsonPropertyName("TrxCode")]
        public string TrxCode { get; set; } = string.Empty;

        [JsonPropertyName("CPI")]
        public string CPI { get; set; } = string.Empty;

        [JsonPropertyName("Size")]
        public string Size { get; set; } = "300";
    }

    public class QrCodeResponse
    {
        [JsonPropertyName("ResponseCode")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("ResponseDescription")]
        public string? ResponseDescription { get; set; }

        [JsonPropertyName("RequestID")]
        public string? RequestId { get; set; }

        /// <summary>Base64-encoded PNG of the generated QR code.</summary>
        [JsonPropertyName("QRCode")]
        public string? QrCodeBase64 { get; set; }

        /// <summary>Decodes <see cref="QrCodeBase64"/> into raw PNG bytes, ready to save or stream.</summary>
        public byte[] ToPngBytes() => string.IsNullOrEmpty(QrCodeBase64) ? Array.Empty<byte>() : Convert.FromBase64String(QrCodeBase64);
    }
}
