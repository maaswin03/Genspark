using backend.Models.DTOs.Revenue;

namespace backend.Services;

public interface IRevenueService
{
	Task<AdminRevenueSummaryResponse> GetAdminRevenueAsync(CancellationToken cancellationToken = default);

	Task<IReadOnlyList<AdminOperatorRevenueResponse>> GetAdminOperatorRevenueAsync(CancellationToken cancellationToken = default);

	Task<OperatorRevenueSummaryResponse> GetOperatorRevenueAsync(int userId, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<OperatorTripRevenueResponse>> GetOperatorTripRevenueAsync(int userId, CancellationToken cancellationToken = default);
}
