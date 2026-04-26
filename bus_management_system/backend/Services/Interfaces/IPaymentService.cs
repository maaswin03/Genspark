using backend.Models.DTOs.Payment;

namespace backend.Services;

public interface IPaymentService
{
	Task<InitiatePaymentResponse> InitiateAsync(
		int userId,
		InitiatePaymentRequest request,
		CancellationToken cancellationToken = default);

	Task HandleWebhookAsync(
		int userId,
		PaymentWebhookRequest request,
		CancellationToken cancellationToken = default);

	Task<PaymentStatusResponse> GetStatusAsync(
		int userId,
		int bookingId,
		CancellationToken cancellationToken = default);
}
