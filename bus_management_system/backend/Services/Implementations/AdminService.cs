using backend.Data;
using backend.Models.DTOs.Admin;
using backend.Models.DTOs.Operator;
using backend.Models.Entities;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AdminService : IAdminService
{
	private readonly AppDbContext _context;

	public AdminService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<IReadOnlyList<OperatorProfileResponse>> GetOperatorsAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Operators
			.AsNoTracking()
			.Include(x => x.User)
			.OrderByDescending(x => x.CreatedAt)
			.Select(x => new OperatorProfileResponse
			{
				Id = x.Id,
				UserId = x.UserId,
				Name = x.User.Name,
				Email = x.User.Email,
				Phone = x.User.Phone,
				CompanyName = x.CompanyName,
				LicenseNumber = x.LicenseNumber,
				ApprovalStatus = x.ApprovalStatus,
				CreatedAt = x.CreatedAt
			})
			.ToListAsync(cancellationToken);
	}

	public async Task<OperatorDetailResponse> GetOperatorDetailAsync(int id, CancellationToken cancellationToken = default)
	{
		var op = await _context.Operators
			.AsNoTracking()
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

		if (op == null)
		{
			throw new KeyNotFoundException("Operator not found.");
		}

		var docs = await _context.OperatorDocuments
			.AsNoTracking()
			.Where(x => x.OperatorId == id)
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

		return ToOperatorDetail(op, docs);
	}

	public async Task<OperatorDetailResponse> ApproveOperatorAsync(int id, int adminUserId, ApproveOperatorRequest request, CancellationToken cancellationToken = default)
	{
		var op = await _context.Operators
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

		if (op == null)
		{
			throw new KeyNotFoundException("Operator not found.");
		}

		if (op.ApprovalStatus == ApprovalStatus.Blocked)
		{
			throw new InvalidOperationException("Blocked operator must be unblocked instead of approved.");
		}

		if (op.ApprovalStatus == ApprovalStatus.Approved)
		{
			throw new InvalidOperationException("Operator is already approved.");
		}

		op.ApprovalStatus = ApprovalStatus.Approved;
		op.ApprovedBy = adminUserId;
		op.ApprovedAt = DateTime.UtcNow;
		op.RejectionReason = null;
		op.BlockedReason = null;
		op.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync(cancellationToken);
		return await GetOperatorDetailAsync(id, cancellationToken);
	}

	public async Task<OperatorDetailResponse> RejectOperatorAsync(int id, RejectOperatorRequest request, CancellationToken cancellationToken = default)
	{
		var reason = request.Reason.Trim();
		if (string.IsNullOrWhiteSpace(reason))
		{
			throw new ArgumentException("Rejection reason is required.");
		}

		var op = await _context.Operators
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

		if (op == null)
		{
			throw new KeyNotFoundException("Operator not found.");
		}

		if (op.ApprovalStatus == ApprovalStatus.Approved)
		{
			throw new InvalidOperationException("Approved operator cannot be rejected. Use block endpoint.");
		}

		if (op.ApprovalStatus == ApprovalStatus.Blocked)
		{
			throw new InvalidOperationException("Blocked operator cannot be rejected.");
		}

		op.ApprovalStatus = ApprovalStatus.Rejected;
		op.RejectionReason = reason;
		op.BlockedReason = null;
		op.ApprovedBy = null;
		op.ApprovedAt = null;
		op.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync(cancellationToken);
		return await GetOperatorDetailAsync(id, cancellationToken);
	}

	public async Task<OperatorDetailResponse> BlockOperatorAsync(int id, RejectOperatorRequest request, CancellationToken cancellationToken = default)
	{
		var reason = request.Reason.Trim();
		if (string.IsNullOrWhiteSpace(reason))
		{
			throw new ArgumentException("Blocked reason is required.");
		}

		var op = await _context.Operators
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

		if (op == null)
		{
			throw new KeyNotFoundException("Operator not found.");
		}

		if (op.ApprovalStatus != ApprovalStatus.Approved)
		{
			throw new InvalidOperationException("Only approved operators can be blocked.");
		}

		op.ApprovalStatus = ApprovalStatus.Blocked;
		op.BlockedReason = reason;
		op.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync(cancellationToken);
		return await GetOperatorDetailAsync(id, cancellationToken);
	}

	public async Task<OperatorDetailResponse> UnblockOperatorAsync(int id, int adminUserId, CancellationToken cancellationToken = default)
	{
		var op = await _context.Operators
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

		if (op == null)
		{
			throw new KeyNotFoundException("Operator not found.");
		}

		if (op.ApprovalStatus != ApprovalStatus.Blocked)
		{
			throw new InvalidOperationException("Only blocked operators can be unblocked.");
		}

		op.ApprovalStatus = ApprovalStatus.Approved;
		op.BlockedReason = null;
		op.ApprovedBy = adminUserId;
		op.ApprovedAt = DateTime.UtcNow;
		op.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync(cancellationToken);
		return await GetOperatorDetailAsync(id, cancellationToken);
	}

	public async Task VerifyDocumentAsync(int id, int docId, int adminUserId, CancellationToken cancellationToken = default)
	{
		var opExists = await _context.Operators
			.AsNoTracking()
			.AnyAsync(x => x.Id == id, cancellationToken);

		if (!opExists)
		{
			throw new KeyNotFoundException("Operator not found.");
		}

		var doc = await _context.OperatorDocuments
			.FirstOrDefaultAsync(x => x.Id == docId && x.OperatorId == id, cancellationToken);

		if (doc == null)
		{
			throw new KeyNotFoundException("Document not found.");
		}

		doc.VerifiedBy = adminUserId;
		doc.VerifiedAt = DateTime.UtcNow;
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<AdminUserResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Users
			.AsNoTracking()
			.Include(x => x.Role)
			.OrderByDescending(x => x.CreatedAt)
			.Select(x => new AdminUserResponse
			{
				Id = x.Id,
				Name = x.Name,
				Email = x.Email,
				Phone = x.Phone,
				Role = x.Role.Name,
				IsActive = x.IsActive
			})
			.ToListAsync(cancellationToken);
	}

	public async Task DeactivateUserAsync(int id, CancellationToken cancellationToken = default)
	{
		var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
		if (user == null)
		{
			throw new KeyNotFoundException("User not found.");
		}

		if (!user.IsActive)
		{
			return;
		}

		user.IsActive = false;
		user.UpdatedAt = DateTime.UtcNow;
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task<PlatformFeeResponse?> GetPlatformFeeAsync(CancellationToken cancellationToken = default)
	{
		var fee = await _context.PlatformFeeConfigs
			.AsNoTracking()
			.Where(x => x.IsActive)
			.OrderByDescending(x => x.UpdatedAt)
			.FirstOrDefaultAsync(cancellationToken);

		if (fee == null)
		{
			return null;
		}

		return new PlatformFeeResponse
		{
			FeeValue = fee.FeeValue,
			IsActive = fee.IsActive,
			UpdatedAt = fee.UpdatedAt
		};
	}

	public async Task<PlatformFeeResponse> SetPlatformFeeAsync(int adminUserId, SetPlatformFeeRequest request, CancellationToken cancellationToken = default)
	{
		if (request.FeeValue < 0 || request.FeeValue > 100)
		{
			throw new ArgumentException("Fee value must be between 0 and 100.");
		}

		var currentActiveFees = await _context.PlatformFeeConfigs
			.Where(x => x.IsActive)
			.ToListAsync(cancellationToken);

		foreach (var fee in currentActiveFees)
		{
			fee.IsActive = false;
			fee.UpdatedAt = DateTime.UtcNow;
		}

		var entity = new PlatformFeeConfig
		{
			Type = FeeType.Percentage,
			FeeValue = request.FeeValue,
			IsActive = true,
			CreatedBy = adminUserId,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_context.PlatformFeeConfigs.Add(entity);
		await _context.SaveChangesAsync(cancellationToken);

		return new PlatformFeeResponse
		{
			FeeValue = entity.FeeValue,
			IsActive = entity.IsActive,
			UpdatedAt = entity.UpdatedAt
		};
	}

	private static OperatorDetailResponse ToOperatorDetail(Operator op, List<OperatorDocumentResponse> docs)
	{
		return new OperatorDetailResponse
		{
			Id = op.Id,
			UserId = op.UserId,
			Name = op.User.Name,
			Email = op.User.Email,
			Phone = op.User.Phone,
			CompanyName = op.CompanyName,
			LicenseNumber = op.LicenseNumber,
			ApprovalStatus = op.ApprovalStatus,
			ApprovedAt = op.ApprovedAt,
			ApprovedBy = op.ApprovedBy,
			RejectionReason = op.RejectionReason,
			BlockedReason = op.BlockedReason,
			Documents = docs
		};
	}
}
