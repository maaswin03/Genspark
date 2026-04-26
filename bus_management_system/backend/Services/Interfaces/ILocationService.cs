using backend.Models.DTOs.Admin;

namespace backend.Services;

public interface ILocationService
{
	Task<IReadOnlyList<LocationResponse>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<LocationResponse> CreateAsync(int adminUserId, CreateLocationRequest request, CancellationToken cancellationToken = default);
	Task<LocationResponse> UpdateAsync(int id, UpdateLocationRequest request, CancellationToken cancellationToken = default);
}
