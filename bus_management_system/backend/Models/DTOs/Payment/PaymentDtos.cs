using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.Models.DTOs.Payment;

public class InitiatePaymentRequest
{
    [Required]
    public int BookingId { get; set; }

    [Required]
    [RegularExpression("^(upi|card|netbanking|wallet)$", ErrorMessage = "Payment method must be one of: upi, card, netbanking, wallet")]
    public required string PaymentMethod { get; set; }
}

public class InitiatePaymentResponse
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public required object GatewayPayload { get; set; }
}

public class PaymentWebhookRequest
{
    [Required]
    public int PaymentId { get; set; }

    [Required]
    public required string Status { get; set; }

    [Required]
    public required string Signature { get; set; }

    public string? GatewayTransactionId { get; set; }
}

public class PaymentStatusResponse
{
    public int BookingId { get; set; }
    public int? PaymentId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
