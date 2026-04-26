using backend.Data;
using backend.Helpers;
using backend.Models.DTOs.Operator;
using backend.Models.Entities;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class OperatorService : IOperatorService
{
	private readonly AppDbContext _context;

	public OperatorService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<OperatorProfileResponse> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
	{
		var operatorEntity = await _context.Operators
			.AsNoTracking()
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

		if (operatorEntity == null)
		{
			throw new KeyNotFoundException("Operator profile not found.");
		}

		return MapProfile(operatorEntity);
	}

	public async Task<OperatorProfileResponse> UpdateProfileAsync(int userId, UpdateOperatorProfileRequest request, CancellationToken cancellationToken = default)
	{
		var operatorEntity = await _context.Operators
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

		if (operatorEntity == null)
		{
			throw new KeyNotFoundException("Operator profile not found.");
		}

		operatorEntity.User.Name = request.Name.Trim();
		operatorEntity.User.Phone = request.Phone.Trim();
		operatorEntity.CompanyName = request.CompanyName.Trim();
		operatorEntity.UpdatedAt = DateTime.UtcNow;
		operatorEntity.User.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync(cancellationToken);
		return MapProfile(operatorEntity);
	}

	public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
	{
		var operatorEntity = await _context.Operators
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

		if (operatorEntity == null)
		{
			throw new KeyNotFoundException("Operator profile not found.");
		}

		if (!PasswordHasher.Verify(request.CurrentPassword, operatorEntity.User.Password))
		{
			throw new ArgumentException("Current password is incorrect.");
		}

		if (request.CurrentPassword == request.NewPassword)
		{
			throw new ArgumentException("New password must be different from current password.");
		}

		operatorEntity.User.Password = PasswordHasher.Hash(request.NewPassword);
		operatorEntity.User.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task<OperatorDocumentResponse> UploadDocumentAsync(int userId, UploadDocumentRequest request, CancellationToken cancellationToken = default)
	{
		var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

		var doc = new OperatorDocument
		{
			OperatorId = operatorId,
			DocumentType = request.DocumentType,
			FileUrl = request.FileUrl.Trim(),
			UploadedAt = DateTime.UtcNow
		};

		_context.OperatorDocuments.Add(doc);
		await _context.SaveChangesAsync(cancellationToken);

		return new OperatorDocumentResponse
		{
			Id = doc.Id,
			DocumentType = doc.DocumentType,
			FileUrl = doc.FileUrl,
			UploadedAt = doc.UploadedAt,
			VerifiedBy = doc.VerifiedBy,
			VerifiedAt = doc.VerifiedAt
		};
	}

	public async Task<IReadOnlyList<OperatorDocumentResponse>> GetDocumentsAsync(int userId, CancellationToken cancellationToken = default)
	{
		var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

		return await _context.OperatorDocuments
			.AsNoTracking()
			.Where(x => x.OperatorId == operatorId)
			.OrderByDescending(x => x.UploadedAt)
			.Select(x => new OperatorDocumentResponse
			{
				Id = x.Id,
				DocumentType = x.DocumentType,
				FileUrl = x.FileUrl,
				UploadedAt = x.UploadedAt,
				VerifiedBy = x.VerifiedBy,
				VerifiedAt = x.VerifiedAt
			})
			.ToListAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<OperatorOfficeResponse>> GetOfficesAsync(int userId, CancellationToken cancellationToken = default)
	{
		var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

		return await _context.OperatorOffices
			.AsNoTracking()
			.Include(x => x.Location)
			.Where(x => x.OperatorId == operatorId)
			.OrderByDescending(x => x.IsHeadOffice)
			.ThenBy(x => x.CreatedAt)
			.Select(x => new OperatorOfficeResponse
			{
				Id = x.Id,
				LocationId = x.LocationId,
				LocationName = x.Location.Name + ", " + x.Location.City + ", " + x.Location.State,
				Address = x.Address,
				IsHeadOffice = x.IsHeadOffice
			})
			.ToListAsync(cancellationToken);
	}

	public async Task<OperatorOfficeResponse> AddOfficeAsync(int userId, CreateOfficeRequest request, CancellationToken cancellationToken = default)
	{
		var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

		var location = await _context.Locations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.LocationId, cancellationToken);
		if (location == null)
		{
			throw new KeyNotFoundException("Location not found.");
		}

		// Prevent duplicate office in the same location for this operator
		var alreadyHasOfficeHere = await _context.OperatorOffices
			.AsNoTracking()
			.AnyAsync(x => x.OperatorId == operatorId && x.LocationId == request.LocationId, cancellationToken);
		if (alreadyHasOfficeHere)
		{
			throw new InvalidOperationException(
				$"You already have an office in {location.Name}, {location.City}. Each location can only have one office.");
		}

		if (request.IsHeadOffice)
		{
			var existingHeadOffice = await _context.OperatorOffices.Where(x => x.OperatorId == operatorId && x.IsHeadOffice).ToListAsync(cancellationToken);
			foreach (var headOffice in existingHeadOffice)
			{
				headOffice.IsHeadOffice = false;
			}
		}

		var office = new OperatorOffice
		{
			OperatorId = operatorId,
			LocationId = request.LocationId,
			Address = request.Address.Trim(),
			IsHeadOffice = request.IsHeadOffice,
			CreatedAt = DateTime.UtcNow
		};

		_context.OperatorOffices.Add(office);
		await _context.SaveChangesAsync(cancellationToken);

		return new OperatorOfficeResponse
		{
			Id = office.Id,
			LocationId = office.LocationId,
			LocationName = FormatLocation(location),
			Address = office.Address,
			IsHeadOffice = office.IsHeadOffice
		};
	}

	public async Task<OperatorOfficeResponse> UpdateOfficeAsync(int userId, int officeId, UpdateOfficeRequest request, CancellationToken cancellationToken = default)
	{
		var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

		var office = await _context.OperatorOffices
			.Include(x => x.Location)
			.FirstOrDefaultAsync(x => x.Id == officeId && x.OperatorId == operatorId, cancellationToken);

		if (office == null)
		{
			throw new KeyNotFoundException("Office not found.");
		}

		if (request.IsHeadOffice)
		{
			var existingHeadOffice = await _context.OperatorOffices.Where(x => x.OperatorId == operatorId && x.Id != officeId && x.IsHeadOffice).ToListAsync(cancellationToken);
			foreach (var current in existingHeadOffice)
			{
				current.IsHeadOffice = false;
			}
		}

		office.Address = request.Address.Trim();
		office.IsHeadOffice = request.IsHeadOffice;

		await _context.SaveChangesAsync(cancellationToken);

		return new OperatorOfficeResponse
		{
			Id = office.Id,
			LocationId = office.LocationId,
			LocationName = FormatLocation(office.Location),
			Address = office.Address,
			IsHeadOffice = office.IsHeadOffice
		};
	}

	public async Task<IReadOnlyList<OperatorBookingSummaryResponse>> GetOperatorBookingsAsync(int userId, CancellationToken cancellationToken = default)
	{
		var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

		return await _context.Bookings
			.AsNoTracking()
			.Include(x => x.User)
			.Include(x => x.Trip)
				.ThenInclude(x => x.Route)
			.Include(x => x.BookingSeats)
			.Where(x => x.Trip.OperatorId == operatorId)
			.OrderByDescending(x => x.BookingDate)
			.Select(x => new OperatorBookingSummaryResponse
			{
				BookingId = x.Id,
				TripId = x.TripId,
				UserName = x.User.Name,
				UserEmail = x.User.Email,
				RouteName = x.Trip.Route.SourceLocation.Name + " -> " + x.Trip.Route.DestinationLocation.Name,
				DepartureTime = x.Trip.DepartureTime,
				TotalAmount = x.TotalAmount > 0 ? x.TotalAmount - x.PlatformFee : 0,
				BookingStatus = x.Status,
				SeatCount = x.BookingSeats.Count(s => s.Status != BookingSeatStatus.Cancelled)
			})
			.ToListAsync(cancellationToken);
	}

	public async Task<OperatorBookingDetailResponse> GetBookingDetailAsync(int userId, int bookingId, CancellationToken cancellationToken = default)
	{
		var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

		var booking = await _context.Bookings
			.AsNoTracking()
			.Include(x => x.User)
			.Include(x => x.Trip)
				.ThenInclude(x => x.Route)
					.ThenInclude(x => x.SourceLocation)
			.Include(x => x.Trip)
				.ThenInclude(x => x.Route)
					.ThenInclude(x => x.DestinationLocation)
			.Include(x => x.BookingSeats)
				.ThenInclude(x => x.Seat)
			.Include(x => x.BookingSeats)
				.ThenInclude(x => x.Passenger)
			.FirstOrDefaultAsync(x => x.Id == bookingId && x.Trip.OperatorId == operatorId, cancellationToken);

		if (booking == null)
		{
			throw new KeyNotFoundException("Booking not found.");
		}

		return new OperatorBookingDetailResponse
		{
			BookingId = booking.Id,
			TripId = booking.TripId,
			UserName = booking.User.Name,
			UserEmail = booking.User.Email,
			UserPhone = booking.User.Phone,
			RouteName = booking.Trip.Route.SourceLocation.Name + " -> " + booking.Trip.Route.DestinationLocation.Name,
			BookingDate = booking.BookingDate,
			DepartureTime = booking.Trip.DepartureTime,
			ArrivalTime = booking.Trip.ArrivalTime,
			TotalAmount = booking.TotalAmount,
			PlatformFee = booking.PlatformFee,
			BookingStatus = booking.Status,
			CancelledAt = booking.CancelledAt,
			CancelReason = booking.CancelReason,
			Seats = booking.BookingSeats.Select(x => new OperatorBookingSeatResponse
			{
				BookingSeatId = x.Id,
				SeatId = x.SeatId,
				SeatNumber = x.Seat.SeatNumber,
				AmountPaid = x.AmountPaid,
				Status = x.Status,
				PassengerName = x.Passenger?.Name,
				PassengerAge = x.Passenger?.Age,
				PassengerGender = x.Passenger != null ? x.Passenger.Gender : null
			}).ToList()
		};
	}

	private async Task<int> GetOperatorIdAsync(int userId, CancellationToken cancellationToken)
	{
		var operatorId = await _context.Operators
			.AsNoTracking()
			.Where(x => x.UserId == userId)
			.Select(x => x.Id)
			.FirstOrDefaultAsync(cancellationToken);

		if (operatorId == 0)
		{
			throw new KeyNotFoundException("Operator profile not found.");
		}

		return operatorId;
	}

	private static string FormatLocation(Location location) => location.Name + ", " + location.City + ", " + location.State;

	private static OperatorProfileResponse MapProfile(Operator operatorEntity)
	{
		return new OperatorProfileResponse
		{
			Id = operatorEntity.Id,
			UserId = operatorEntity.UserId,
			Name = operatorEntity.User.Name,
			Email = operatorEntity.User.Email,
			Phone = operatorEntity.User.Phone,
			CompanyName = operatorEntity.CompanyName,
			LicenseNumber = operatorEntity.LicenseNumber,
			ApprovalStatus = operatorEntity.ApprovalStatus,
			CreatedAt = operatorEntity.CreatedAt
		};
	}
}
