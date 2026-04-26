using backend.Models.DTOs.Admin;

namespace backend.Services;

public interface IRouteService
{
	Task<IReadOnlyList<RouteResponse>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<RouteResponse> CreateAsync(int adminUserId, CreateRouteRequest request, CancellationToken cancellationToken = default);
	Task<RouteResponse> AddStopsAsync(int routeId, AddRouteStopsRequest request, CancellationToken cancellationToken = default);
	Task<RouteResponse> ToggleActiveAsync(int routeId, CancellationToken cancellationToken = default);
}
