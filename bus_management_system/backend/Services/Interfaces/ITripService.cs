using backend.Models.DTOs.Trip;

namespace backend.Services;

public interface ITripService
{
	Task<IReadOnlyList<TripResponse>> SearchAsync(TripSearchRequest request, CancellationToken cancellationToken = default);
	Task<TripDetailResponse> GetDetailAsync(int id, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<TripSeatResponse>> GetAvailableSeatsAsync(int id, CancellationToken cancellationToken = default);
	Task<TripPointsResponse> GetPointsAsync(int id, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<OperatorTripResponse>> GetByOperatorAsync(int userId, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<TripScheduleResponse>> GetSchedulesByOperatorAsync(int userId, CancellationToken cancellationToken = default);
	Task<TripScheduleResponse> CreateScheduleAsync(int userId, CreateScheduleRequest request, CancellationToken cancellationToken = default);
	Task<TripScheduleResponse> UpdateScheduleAsync(int userId, int id, UpdateScheduleRequest request, CancellationToken cancellationToken = default);
	Task<TripScheduleResponse> ToggleScheduleAsync(int userId, int id, CancellationToken cancellationToken = default);
	Task<TripDetailResponse> ChangeBusAsync(int userId, int id, ChangeBusRequest request, CancellationToken cancellationToken = default);
	Task<TripDetailResponse> CancelAsync(int userId, int id, CancelTripRequest request, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<SeatPricingResponse>> GetSeatPricingAsync(int userId, int id, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<SeatPricingResponse>> SetSeatPricingAsync(int userId, int id, SetSeatPricingRequest request, CancellationToken cancellationToken = default);
}
