using backend.Models.DTOs.Operator;

namespace backend.Services;

public interface IOperatorService
{
    Task<OperatorProfileResponse> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<OperatorProfileResponse> UpdateProfileAsync(int userId, UpdateOperatorProfileRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<OperatorDocumentResponse> UploadDocumentAsync(int userId, UploadDocumentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OperatorDocumentResponse>> GetDocumentsAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OperatorOfficeResponse>> GetOfficesAsync(int userId, CancellationToken cancellationToken = default);
    Task<OperatorOfficeResponse> AddOfficeAsync(int userId, CreateOfficeRequest request, CancellationToken cancellationToken = default);
    Task<OperatorOfficeResponse> UpdateOfficeAsync(int userId, int officeId, UpdateOfficeRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OperatorBookingSummaryResponse>> GetOperatorBookingsAsync(int userId, CancellationToken cancellationToken = default);
    Task<OperatorBookingDetailResponse> GetBookingDetailAsync(int userId, int bookingId, CancellationToken cancellationToken = default);
}
