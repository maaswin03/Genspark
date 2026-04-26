using backend.Models.DTOs.Bus;

namespace backend.Services;

public interface IBusService
{
    Task<IReadOnlyList<BusResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BusDetailResponse> GetWithLayoutAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BusSeatResponse>> GetSeatsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BusResponse>> GetByOperatorAsync(int userId, CancellationToken cancellationToken = default);
    Task<BusDetailResponse> CreateAsync(int userId, CreateBusRequest request, CancellationToken cancellationToken = default);
    Task<BusDetailResponse> UpdateAsync(int userId, int id, UpdateBusRequest request, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int userId, int id, CancellationToken cancellationToken = default);
}
