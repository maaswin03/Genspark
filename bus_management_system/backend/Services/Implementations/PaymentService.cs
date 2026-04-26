using System.Text.Json;
using backend.Data;
using backend.Models.DTOs.Payment;
using backend.Models.Entities;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class PaymentService : IPaymentService
{
	private readonly AppDbContext _context;

	public PaymentService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<InitiatePaymentResponse> InitiateAsync(
		int userId,
		InitiatePaymentRequest request,
		CancellationToken cancellationToken = default)
	{
		var paymentMethod = request.PaymentMethod.Trim().ToLowerInvariant();
		var allowedMethods = new[] { "upi", "card", "netbanking", "wallet" };
		if (!allowedMethods.Contains(paymentMethod))
		{
			throw new ArgumentException("Unsupported payment method.");
		}

		var booking = await _context.Bookings
			.Include(x => x.Payments)
			.FirstOrDefaultAsync(x => x.Id == request.BookingId && x.UserId == userId, cancellationToken);

		if (booking == null)
		{
			throw new KeyNotFoundException("Booking not found.");
		}

		if (booking.Status == BookingStatus.Cancelled)
		{
			throw new InvalidOperationException("Cannot initiate payment for a cancelled booking.");
		}

		if (booking.Payments.Any(x => x.PaymentStatus == PaymentStatus.Success))
		{
			throw new InvalidOperationException("Payment already completed for this booking.");
		}

		var payment = new Payment
		{
			BookingId = booking.Id,
			Amount = booking.TotalAmount,
			PaymentMethod = paymentMethod,
			PaymentStatus = PaymentStatus.Pending,
			GatewayResponse = JsonSerializer.Serialize(new
			{
				mock = true,
				booking_id = booking.Id,
				amount = booking.TotalAmount,
				redirect_url = "https://mock-gateway/pay"
			}),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_context.Payments.Add(payment);
		await _context.SaveChangesAsync(cancellationToken);

		return new InitiatePaymentResponse
		{
			PaymentId = payment.Id,
			BookingId = booking.Id,
			PaymentStatus = payment.PaymentStatus,
			GatewayPayload = new
			{
				payment_id = payment.Id,
				booking_id = booking.Id,
				amount = payment.Amount,
				payment_method = payment.PaymentMethod,
				mock_signature = "mock-signature"
			}
		};
	}

	public async Task HandleWebhookAsync(
		int userId,
		PaymentWebhookRequest request,
		CancellationToken cancellationToken = default)
	{
		if (request.Signature != "mock-signature")
		{
			throw new UnauthorizedAccessException("Invalid webhook signature.");
		}

		var payment = await _context.Payments
			.Include(x => x.Booking)
				.ThenInclude(x => x.Trip)
			.Include(x => x.Booking)
				.ThenInclude(x => x.BookingSeats)
					.ThenInclude(x => x.TripSeat)
			.FirstOrDefaultAsync(x => x.Id == request.PaymentId, cancellationToken);

		if (payment == null)
		{
			throw new KeyNotFoundException("Payment not found.");
		}

		if (payment.Booking.UserId != userId)
		{
			throw new UnauthorizedAccessException("You are not allowed to update this payment.");
		}

		var now = DateTime.UtcNow;
		var isSuccess = string.Equals(request.Status, "success", StringComparison.OrdinalIgnoreCase);

		payment.PaymentStatus = isSuccess ? PaymentStatus.Success : PaymentStatus.Failed;
		payment.TransactionId = request.GatewayTransactionId ?? Guid.NewGuid().ToString("N");
		payment.UpdatedAt = now;
		payment.PaidAt = isSuccess ? now : null;

		if (isSuccess)
		{
			var seatSubtotal = payment.Booking.BookingSeats
				.Where(x => x.Status == BookingSeatStatus.Confirmed)
				.Sum(x => x.AmountPaid);

			payment.Booking.Status = BookingStatus.Confirmed;
			payment.Booking.UpdatedAt = now;

			foreach (var bookingSeat in payment.Booking.BookingSeats.Where(x => x.Status == BookingSeatStatus.Confirmed))
			{
				bookingSeat.TripSeat.Status = SeatStatus.Booked;
				bookingSeat.TripSeat.LockedBy = null;
				bookingSeat.TripSeat.LockedUntil = null;
			}

			var activeFee = await _context.PlatformFeeConfigs
				.AsNoTracking()
				.Where(x => x.IsActive)
				.OrderByDescending(x => x.UpdatedAt)
				.FirstOrDefaultAsync(cancellationToken);

			var platformFee = activeFee == null
				? 0
				: Math.Round(seatSubtotal * activeFee.FeeValue / 100m, 2);

			payment.Booking.PlatformFee = platformFee;
			payment.Booking.TotalAmount = seatSubtotal + platformFee;

			var existingTransaction = await _context.Transactions
				.AnyAsync(x => x.BookingId == payment.BookingId, cancellationToken);

			if (!existingTransaction)
			{
				_context.Transactions.Add(new Transaction
				{
					BookingId = payment.BookingId,
					OperatorId = payment.Booking.Trip.OperatorId,
					TotalAmount = payment.Booking.TotalAmount,
					PlatformFee = platformFee,
					OperatorEarning = seatSubtotal,
					CreatedAt = now,
					UpdatedAt = now
				});
			}

			_context.Notifications.Add(new Notification
			{
				UserId = payment.Booking.UserId,
				Title = "Payment Successful",
				Message = $"Payment for booking #{payment.BookingId} completed successfully.",
				Type = "payment_success",
				ReferenceType = ReferenceType.Booking,
				ReferenceId = payment.BookingId,
				Channel = NotificationChannel.InApp,
				IsRead = false,
				CreatedAt = now
			});
		}

		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task<PaymentStatusResponse> GetStatusAsync(
		int userId,
		int bookingId,
		CancellationToken cancellationToken = default)
	{
		var booking = await _context.Bookings
			.AsNoTracking()
			.Include(x => x.Payments)
			.FirstOrDefaultAsync(x => x.Id == bookingId && x.UserId == userId, cancellationToken);

		if (booking == null)
		{
			throw new KeyNotFoundException("Booking not found.");
		}

		var latestPayment = booking.Payments
			.OrderByDescending(x => x.CreatedAt)
			.FirstOrDefault();

		return new PaymentStatusResponse
		{
			BookingId = bookingId,
			PaymentId = latestPayment?.Id,
			PaymentStatus = latestPayment?.PaymentStatus ?? PaymentStatus.Pending,
			BookingStatus = booking.Status,
			Amount = latestPayment?.Amount ?? booking.TotalAmount,
			TransactionId = latestPayment?.TransactionId,
			UpdatedAt = latestPayment?.UpdatedAt ?? booking.UpdatedAt
		};
	}
}
