using backend.Data;
using backend.Models.DTOs.Admin;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class LocationService : ILocationService
{
	private readonly AppDbContext _context;

	public LocationService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<IReadOnlyList<LocationResponse>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Locations
			.AsNoTracking()
			.OrderBy(x => x.State)
			.ThenBy(x => x.City)
			.ThenBy(x => x.Name)
			.Select(x => new LocationResponse
			{
				Id = x.Id,
				Name = x.Name,
				City = x.City,
				State = x.State
			})
			.ToListAsync(cancellationToken);
	}

	public async Task<LocationResponse> CreateAsync(int adminUserId, CreateLocationRequest request, CancellationToken cancellationToken = default)
	{
		var name = request.Name.Trim();
		var city = request.City.Trim();
		var state = request.State.Trim();

		if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(state))
		{
			throw new ArgumentException("Name, city and state are required.");
		}

		var normalizedName = name.ToLowerInvariant();
		var normalizedCity = city.ToLowerInvariant();
		var normalizedState = state.ToLowerInvariant();

		var exists = await _context.Locations.AnyAsync(
			x => x.Name.ToLower() == normalizedName
				 && x.City.ToLower() == normalizedCity
				 && x.State.ToLower() == normalizedState,
			cancellationToken);

		if (exists)
		{
			throw new InvalidOperationException("Location already exists.");
		}

		var entity = new Location
		{
			Name = name,
			City = city,
			State = state,
			CreatedBy = adminUserId,
			CreatedAt = DateTime.UtcNow
		};

		_context.Locations.Add(entity);
		await _context.SaveChangesAsync(cancellationToken);

		return new LocationResponse
		{
			Id = entity.Id,
			Name = entity.Name,
			City = entity.City,
			State = entity.State
		};
	}

	public async Task<LocationResponse> UpdateAsync(int id, UpdateLocationRequest request, CancellationToken cancellationToken = default)
	{
		var entity = await _context.Locations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
		if (entity == null)
		{
			throw new KeyNotFoundException("Location not found.");
		}

		var name = request.Name.Trim();
		var city = request.City.Trim();
		var state = request.State.Trim();

		if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(state))
		{
			throw new ArgumentException("Name, city and state are required.");
		}

		var normalizedName = name.ToLowerInvariant();
		var normalizedCity = city.ToLowerInvariant();
		var normalizedState = state.ToLowerInvariant();

		var duplicate = await _context.Locations.AnyAsync(
			x => x.Id != id
				 && x.Name.ToLower() == normalizedName
				 && x.City.ToLower() == normalizedCity
				 && x.State.ToLower() == normalizedState,
			cancellationToken);

		if (duplicate)
		{
			throw new InvalidOperationException("Another location with same name, city and state already exists.");
		}

		entity.Name = name;
		entity.City = city;
		entity.State = state;

		await _context.SaveChangesAsync(cancellationToken);

		return new LocationResponse
		{
			Id = entity.Id,
			Name = entity.Name,
			City = entity.City,
			State = entity.State
		};
	}
}
