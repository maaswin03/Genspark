using backend.Data;
using backend.Models.DTOs.Admin;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class RouteService : IRouteService
{
	private readonly AppDbContext _context;

	public RouteService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<IReadOnlyList<RouteResponse>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var routes = await _context.Routes
			.AsNoTracking()
			.Include(x => x.SourceLocation)
			.Include(x => x.DestinationLocation)
			.OrderBy(x => x.Id)
			.ToListAsync(cancellationToken);

		var routeIds = routes.Select(x => x.Id).ToList();

		var stops = await _context.RouteStops
			.AsNoTracking()
			.Where(x => routeIds.Contains(x.RouteId))
			.Include(x => x.Location)
			.OrderBy(x => x.RouteId)
			.ThenBy(x => x.StopOrder)
			.ToListAsync(cancellationToken);

		return routes.Select(route => new RouteResponse
		{
			Id = route.Id,
			SourceId = route.SourceId,
			SourceName = FormatLocation(route.SourceLocation),
			DestinationId = route.DestinationId,
			DestinationName = FormatLocation(route.DestinationLocation),
			DistanceKm = route.DistanceKm,
			IsActive = route.IsActive,
			Stops = stops
				.Where(x => x.RouteId == route.Id)
				.Select(x => new RouteStopResponse
				{
					Id = x.Id,
					LocationId = x.LocationId,
					LocationName = FormatLocation(x.Location),
					StopOrder = x.StopOrder,
					DistanceFromSource = x.DistanceFromSource
				})
				.ToList()
		}).ToList();
	}

	public async Task<RouteResponse> CreateAsync(int adminUserId, CreateRouteRequest request, CancellationToken cancellationToken = default)
	{
		if (request.SourceId == request.DestinationId)
		{
			throw new ArgumentException("Source and destination must be different.");
		}

		if (request.DistanceKm <= 0)
		{
			throw new ArgumentException("Distance must be greater than 0.");
		}

		var locations = await _context.Locations
			.AsNoTracking()
			.Where(x => x.Id == request.SourceId || x.Id == request.DestinationId)
			.ToListAsync(cancellationToken);

		if (locations.Count != 2)
		{
			throw new KeyNotFoundException("Source or destination location not found.");
		}

		var exists = await _context.Routes.AnyAsync(
			x => x.SourceId == request.SourceId && x.DestinationId == request.DestinationId,
			cancellationToken);

		if (exists)
		{
			throw new InvalidOperationException("Route already exists.");
		}

		var entity = new backend.Models.Entities.Route
		{
			SourceId = request.SourceId,
			DestinationId = request.DestinationId,
			DistanceKm = request.DistanceKm,
			CreatedBy = adminUserId,
			IsActive = true,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_context.Routes.Add(entity);
		await _context.SaveChangesAsync(cancellationToken);

		var source = locations.First(x => x.Id == request.SourceId);
		var destination = locations.First(x => x.Id == request.DestinationId);

		return new RouteResponse
		{
			Id = entity.Id,
			SourceId = entity.SourceId,
			SourceName = FormatLocation(source),
			DestinationId = entity.DestinationId,
			DestinationName = FormatLocation(destination),
			DistanceKm = entity.DistanceKm,
			IsActive = entity.IsActive
		};
	}

	public async Task<RouteResponse> AddStopsAsync(int routeId, AddRouteStopsRequest request, CancellationToken cancellationToken = default)
	{
		var route = await _context.Routes
			.Include(x => x.SourceLocation)
			.Include(x => x.DestinationLocation)
			.FirstOrDefaultAsync(x => x.Id == routeId, cancellationToken);

		if (route == null)
		{
			throw new KeyNotFoundException("Route not found.");
		}

		var inputStops = request.Stops
			.OrderBy(x => x.StopOrder)
			.ToList();

		if (inputStops.Count == 0)
		{
			throw new ArgumentException("At least one stop is required.");
		}

		if (inputStops.GroupBy(x => x.StopOrder).Any(g => g.Count() > 1))
		{
			throw new ArgumentException("Duplicate stop_order values are not allowed.");
		}

		if (inputStops.GroupBy(x => x.LocationId).Any(g => g.Count() > 1))
		{
			throw new ArgumentException("Duplicate location_id values are not allowed.");
		}

		var firstStop = inputStops.First();
		var lastStop = inputStops.Last();

		if (firstStop.LocationId != route.SourceId)
		{
			throw new ArgumentException("First stop must match route source location.");
		}

		if (lastStop.LocationId != route.DestinationId)
		{
			throw new ArgumentException("Last stop must match route destination location.");
		}

		var locationIds = inputStops.Select(x => x.LocationId).Distinct().ToList();
		var locationMap = await _context.Locations
			.AsNoTracking()
			.Where(x => locationIds.Contains(x.Id))
			.ToDictionaryAsync(x => x.Id, cancellationToken);

		if (locationMap.Count != locationIds.Count)
		{
			throw new KeyNotFoundException("One or more route stop locations were not found.");
		}

		// **FIX #5: ROUTE VALIDATION** - Validate distance is monotonically increasing
		for (int i = 1; i < inputStops.Count; i++)
		{
			if (inputStops[i].DistanceFromSource <= inputStops[i - 1].DistanceFromSource)
			{
				throw new ArgumentException(
					$"Distance from source must be strictly increasing. Stop {i} distance ({inputStops[i].DistanceFromSource}km) " +
					$"is not greater than stop {i - 1} distance ({inputStops[i - 1].DistanceFromSource}km).");
			}
		}

		// Also validate that the last stop distance doesn't exceed the route's total distance
		var lastStopDistance = inputStops.Last().DistanceFromSource;
		if (lastStopDistance > route.DistanceKm)
		{
			throw new ArgumentException(
				$"Last stop distance from source ({lastStopDistance}km) cannot exceed route total distance ({route.DistanceKm}km).");
		}

		var existingStops = await _context.RouteStops
			.Where(x => x.RouteId == routeId)
			.ToListAsync(cancellationToken);

		if (existingStops.Count > 0)
		{
			_context.RouteStops.RemoveRange(existingStops);
		}

		var entities = inputStops.Select(x => new RouteStop
		{
			RouteId = routeId,
			LocationId = x.LocationId,
			StopOrder = x.StopOrder,
			DistanceFromSource = x.DistanceFromSource
		}).ToList();

		_context.RouteStops.AddRange(entities);
		route.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync(cancellationToken);

		return new RouteResponse
		{
			Id = route.Id,
			SourceId = route.SourceId,
			SourceName = FormatLocation(route.SourceLocation),
			DestinationId = route.DestinationId,
			DestinationName = FormatLocation(route.DestinationLocation),
			DistanceKm = route.DistanceKm,
			IsActive = route.IsActive,
			Stops = entities
				.OrderBy(x => x.StopOrder)
				.Select(x => new RouteStopResponse
				{
					Id = x.Id,
					LocationId = x.LocationId,
					LocationName = FormatLocation(locationMap[x.LocationId]),
					StopOrder = x.StopOrder,
					DistanceFromSource = x.DistanceFromSource
				})
				.ToList()
		};
	}

	public async Task<RouteResponse> ToggleActiveAsync(int routeId, CancellationToken cancellationToken = default)
	{
		var route = await _context.Routes
			.Include(x => x.SourceLocation)
			.Include(x => x.DestinationLocation)
			.FirstOrDefaultAsync(x => x.Id == routeId, cancellationToken);

		if (route == null)
		{
			throw new KeyNotFoundException("Route not found.");
		}

		route.IsActive = !route.IsActive;
		route.UpdatedAt = DateTime.UtcNow;
		await _context.SaveChangesAsync(cancellationToken);

		var stops = await _context.RouteStops
			.AsNoTracking()
			.Where(x => x.RouteId == routeId)
			.Include(x => x.Location)
			.OrderBy(x => x.StopOrder)
			.ToListAsync(cancellationToken);

		return new RouteResponse
		{
			Id = route.Id,
			SourceId = route.SourceId,
			SourceName = FormatLocation(route.SourceLocation),
			DestinationId = route.DestinationId,
			DestinationName = FormatLocation(route.DestinationLocation),
			DistanceKm = route.DistanceKm,
			IsActive = route.IsActive,
			Stops = stops.Select(x => new RouteStopResponse
			{
				Id = x.Id,
				LocationId = x.LocationId,
				LocationName = FormatLocation(x.Location),
				StopOrder = x.StopOrder,
				DistanceFromSource = x.DistanceFromSource
			}).ToList()
		};
	}

	private static string FormatLocation(Location location)
	{
		return $"{location.Name}, {location.City}, {location.State}";
	}
}
