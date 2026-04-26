using backend.Models.DTOs.Admin;
using backend.Models.DTOs.Operator;

namespace backend.Services;

public interface IAdminService
{
	Task<IReadOnlyList<OperatorProfileResponse>> GetOperatorsAsync(CancellationToken cancellationToken = default);
	Task<OperatorDetailResponse> GetOperatorDetailAsync(int id, CancellationToken cancellationToken = default);
	Task<OperatorDetailResponse> ApproveOperatorAsync(int id, int adminUserId, ApproveOperatorRequest request, CancellationToken cancellationToken = default);
	Task<OperatorDetailResponse> RejectOperatorAsync(int id, RejectOperatorRequest request, CancellationToken cancellationToken = default);
	Task<OperatorDetailResponse> BlockOperatorAsync(int id, RejectOperatorRequest request, CancellationToken cancellationToken = default);
	Task<OperatorDetailResponse> UnblockOperatorAsync(int id, int adminUserId, CancellationToken cancellationToken = default);
	Task VerifyDocumentAsync(int id, int docId, int adminUserId, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<AdminUserResponse>> GetUsersAsync(CancellationToken cancellationToken = default);
	Task DeactivateUserAsync(int id, CancellationToken cancellationToken = default);
	Task<PlatformFeeResponse?> GetPlatformFeeAsync(CancellationToken cancellationToken = default);
	Task<PlatformFeeResponse> SetPlatformFeeAsync(int adminUserId, SetPlatformFeeRequest request, CancellationToken cancellationToken = default);
}
