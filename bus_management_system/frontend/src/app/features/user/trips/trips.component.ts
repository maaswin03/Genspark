import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ChangeDetectorRef, Component, Inject, OnDestroy, OnInit, PLATFORM_ID } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, finalize, takeUntil } from 'rxjs';
import { LocationOption, LocationService } from '../../../core/services/location.service';
import { BookingFlowStateService } from '../../../core/services/booking-flow-state.service';
import { TripBookingService } from '../../../core/services/trip-booking.service';
import { TripSearchResponse } from '../../../core/models/trip-booking.models';

@Component({
  selector: 'app-user-trips',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './trips.component.html',
  styleUrl: './trips.component.css',
})
export class TripsComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  readonly searchForm;
  readonly filterForm;

  allLocations: LocationOption[] = [];
  sourceSuggestions: LocationOption[] = [];
  destinationSuggestions: LocationOption[] = [];
  selectedSource: LocationOption | null = null;
  selectedDestination: LocationOption | null = null;
  sourceDropdownOpen = false;
  destinationDropdownOpen = false;

  sourceId = 0;
  destinationId = 0;
  travelDate = '';

  isLoading = false;
  errorMessage = '';
  filtersVisible = false;
  loadingLocations = false;

  trips: TripSearchResponse[] = [];
  filteredTrips: TripSearchResponse[] = [];
  minPrice = 0;
  maxPrice = 5000;

  readonly busTypeOptions = [
    { label: 'All Types', value: 'all' },
    { label: 'Seater', value: 'seater' },
    { label: 'Sleeper', value: 'sleeper' },
    { label: 'Semi-Sleeper', value: 'semi-sleeper' },
  ];

  readonly departureRanges = [
    { label: 'Any Time', value: 'all' },
    { label: 'Early Morning (12am–6am)', value: '0-6' },
    { label: 'Morning (6am–12pm)', value: '6-12' },
    { label: 'Afternoon (12pm–6pm)', value: '12-18' },
    { label: 'Evening (6pm–12am)', value: '18-24' },
  ];

  readonly sortOptions = [
    { label: 'Recommended', value: 'recommended' },
    { label: 'Price: Low to High', value: 'price' },
    { label: 'Departure Time', value: 'departure' },
    { label: 'Duration', value: 'duration' },
  ];

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly locationService: LocationService,
    private readonly bookingFlowState: BookingFlowStateService,
    private readonly tripBookingService: TripBookingService,
    private readonly cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) private readonly platformId: object
  ) {
    this.searchForm = this.fb.group({
      sourceQuery: ['', Validators.required],
      destinationQuery: ['', Validators.required],
      travelDate: ['', Validators.required],
    });

    this.filterForm = this.fb.group({
      busType: ['all'],
      departureRange: ['all'],
      maxPrice: [5000],
      sortBy: ['recommended'],
    });
  }

  ngOnInit(): void {
    this.loadingLocations = true;
    this.cdr.detectChanges();

    this.route.queryParamMap.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      this.sourceId = Number(params.get('sourceId') ?? 0);
      this.destinationId = Number(params.get('destinationId') ?? 0);
      this.travelDate = params.get('date') ?? '';

      if (this.travelDate) {
        this.searchForm.controls.travelDate.setValue(this.travelDate, { emitEvent: false });
      }

      this.loadTrips();
    });

    this.locationService
      .getLocations()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (locations) => {
          this.allLocations = locations;
          this.loadingLocations = false;
          this.syncSelectionsFromQuery();
          this.cdr.detectChanges();
        },
        error: () => {
          this.loadingLocations = false;
          this.errorMessage = 'Unable to load locations right now.';
          this.cdr.detectChanges();
        },
      });

    this.searchForm.controls.sourceQuery.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((value) => {
        if (this.selectedSource && value !== this.formatLocation(this.selectedSource)) {
          this.selectedSource = null;
        }
        this.sourceSuggestions = this.filterLocations(value ?? '', this.selectedDestination?.id ?? null);
      });

    this.searchForm.controls.destinationQuery.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((value) => {
        if (this.selectedDestination && value !== this.formatLocation(this.selectedDestination)) {
          this.selectedDestination = null;
        }
        this.destinationSuggestions = this.filterLocations(value ?? '', this.selectedSource?.id ?? null);
      });

    this.filterForm.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.applyFilters();
      this.cdr.detectChanges();
    });

    if (isPlatformBrowser(this.platformId)) {
      this.filtersVisible = window.innerWidth >= 992;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleFilters(): void {
    this.filtersVisible = !this.filtersVisible;
    this.cdr.detectChanges();
  }

  openSourceDropdown(): void {
    this.sourceDropdownOpen = true;
    this.sourceSuggestions = this.filterLocations(this.searchForm.controls.sourceQuery.value ?? '', this.selectedDestination?.id ?? null);
  }

  openDestinationDropdown(): void {
    this.destinationDropdownOpen = true;
    this.destinationSuggestions = this.filterLocations(this.searchForm.controls.destinationQuery.value ?? '', this.selectedSource?.id ?? null);
  }

  closeSourceDropdown(): void {
    window.setTimeout(() => {
      this.sourceDropdownOpen = false;
      this.cdr.detectChanges();
    }, 150);
  }

  closeDestinationDropdown(): void {
    window.setTimeout(() => {
      this.destinationDropdownOpen = false;
      this.cdr.detectChanges();
    }, 150);
  }

  selectSource(location: LocationOption): void {
    this.selectedSource = location;
    this.sourceId = location.id;
    this.searchForm.controls.sourceQuery.setValue(this.formatLocation(location), { emitEvent: false });
    this.sourceDropdownOpen = false;
    this.destinationSuggestions = this.filterLocations(this.searchForm.controls.destinationQuery.value ?? '', location.id);
    this.cdr.detectChanges();
  }

  selectDestination(location: LocationOption): void {
    this.selectedDestination = location;
    this.destinationId = location.id;
    this.searchForm.controls.destinationQuery.setValue(this.formatLocation(location), { emitEvent: false });
    this.destinationDropdownOpen = false;
    this.sourceSuggestions = this.filterLocations(this.searchForm.controls.sourceQuery.value ?? '', location.id);
    this.cdr.detectChanges();
  }

  swapLocations(): void {
    const source = this.selectedSource;
    const destination = this.selectedDestination;
    this.selectedSource = destination;
    this.selectedDestination = source;

    const sourceValue = this.searchForm.controls.sourceQuery.value ?? '';
    const destinationValue = this.searchForm.controls.destinationQuery.value ?? '';

    this.searchForm.patchValue(
      { sourceQuery: destinationValue, destinationQuery: sourceValue },
      { emitEvent: false }
    );

    this.sourceId = this.selectedSource?.id ?? 0;
    this.destinationId = this.selectedDestination?.id ?? 0;
    this.cdr.detectChanges();
  }

  searchTrips(): void {
    if (this.searchForm.invalid || !this.selectedSource || !this.selectedDestination) {
      this.searchForm.markAllAsTouched();
      this.errorMessage = 'Please select From, To, and Date.';
      this.cdr.detectChanges();
      return;
    }

    if (this.selectedSource.id === this.selectedDestination.id) {
      this.errorMessage = 'From and To cannot be the same location.';
      this.cdr.detectChanges();
      return;
    }

    this.router.navigate(['/trips/search'], {
      queryParams: {
        sourceId: this.selectedSource.id,
        destinationId: this.selectedDestination.id,
        date: this.searchForm.controls.travelDate.value,
      },
    });
  }

  selectTrip(trip: TripSearchResponse): void {
    this.bookingFlowState.setTrip(trip.tripId, {
      tripId: trip.tripId,
      operatorName: trip.operatorName,
      busNumber: trip.busNumber,
      busType: trip.busType,
      routeName: trip.routeName,
      departureTime: trip.departureTime,
      arrivalTime: trip.arrivalTime,
      baseFare: trip.baseFare,
      availableSeats: trip.availableSeats,
      status: trip.status,
      routeId: 0,
      sourceName: '',
      destinationName: '',
      seats: [],
    });

    this.router.navigate(['/trips', trip.tripId, 'seats']);
  }

  get searchTitle(): string {
    if (!this.selectedSource || !this.selectedDestination) return 'Available Trips';
    return `${this.selectedSource.name} → ${this.selectedDestination.name}`;
  }

  get currentPriceCap(): number {
    return Number(this.filterForm.controls.maxPrice.value ?? 5000);
  }

  onPriceChange(event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);
    this.filterForm.controls.maxPrice.setValue(value);
  }

  trackByTripId(_: number, trip: TripSearchResponse): number { return trip.tripId; }
  trackByLocationId(_: number, location: LocationOption): number { return location.id; }

  getBusTypeLabel(busType: string | number): string {
    if (typeof busType === 'number' || /^\d+$/.test(String(busType ?? ''))) {
      switch (Number(busType)) {
        case 0:
          return 'AC Sleeper';
        case 1:
          return 'Non-AC Sleeper';
        case 2:
          return 'AC Seater';
        case 3:
          return 'Non-AC Seater';
        case 4:
          return 'AC Semi-Sleeper';
        case 5:
          return 'Non-AC Semi-Sleeper';
        default:
          return `Bus Type ${busType}`;
      }
    }

    const n = String(busType ?? '').replace(/_/g, ' ').toLowerCase().trim();
    const isAc = n.startsWith('ac') || n.includes(' ac ');
    const prefix = isAc ? 'AC' : 'Non-AC';
    if (n.includes('semi') && n.includes('sleeper')) return `${prefix} Semi-Sleeper`;
    if (n.includes('sleeper')) return `${prefix} Sleeper`;
    if (n.includes('seater')) return `${prefix} Seater`;
    return String(busType).replace(/_/g, ' ');
  }

  getDurationLabel(trip: TripSearchResponse): string {
    const minutes = Math.max(0, Math.round((new Date(trip.arrivalTime).getTime() - new Date(trip.departureTime).getTime()) / 60000));
    return `${Math.floor(minutes / 60)}h ${String(minutes % 60).padStart(2, '0')}m`;
  }

  formatTripTime(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? value : date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  private loadTrips(): void {
    if (!this.sourceId || !this.destinationId || !this.travelDate) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.trips = [];
    this.filteredTrips = [];
    this.cdr.detectChanges();

    this.tripBookingService
      .searchTrips({ sourceId: this.sourceId, destinationId: this.destinationId, travelDate: this.travelDate })
      .pipe(
        finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (trips) => {
          this.trips = trips;
          this.maxPrice = Math.max(1000, ...trips.map((t) => t.baseFare || 0));
          this.filterForm.controls.maxPrice.setValue(this.maxPrice, { emitEvent: false });
          this.applyFilters();
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Unable to load trips right now. Please try again.';
          this.cdr.detectChanges();
        },
      });
  }

  private syncSelectionsFromQuery(): void {
    if (!this.sourceId || !this.destinationId) {
      this.sourceSuggestions = this.allLocations;
      this.destinationSuggestions = this.allLocations;
      return;
    }

    this.selectedSource = this.allLocations.find((l) => l.id === this.sourceId) ?? null;
    this.selectedDestination = this.allLocations.find((l) => l.id === this.destinationId) ?? null;

    if (this.selectedSource) {
      this.searchForm.controls.sourceQuery.setValue(this.formatLocation(this.selectedSource), { emitEvent: false });
    }
    if (this.selectedDestination) {
      this.searchForm.controls.destinationQuery.setValue(this.formatLocation(this.selectedDestination), { emitEvent: false });
    }

    this.sourceSuggestions = this.filterLocations(this.searchForm.controls.sourceQuery.value ?? '', this.selectedDestination?.id ?? null);
    this.destinationSuggestions = this.filterLocations(this.searchForm.controls.destinationQuery.value ?? '', this.selectedSource?.id ?? null);
  }

  private filterLocations(term: string, excludeId: number | null): LocationOption[] {
    const q = term.trim().toLowerCase();
    return this.allLocations.filter((l) => {
      if (excludeId && l.id === excludeId) return false;
      if (!q) return true;
      return this.formatLocation(l).toLowerCase().includes(q);
    });
  }

  private applyFilters(): void {
    const busType = String(this.filterForm.controls.busType.value ?? 'all').toLowerCase();
    const departureRange = String(this.filterForm.controls.departureRange.value ?? 'all');
    const maxPrice = Number(this.filterForm.controls.maxPrice.value ?? this.maxPrice);
    const sortBy = String(this.filterForm.controls.sortBy.value ?? 'recommended');
    const window = this.getDepartureWindow(departureRange);

    const filtered = this.trips.filter((trip) => {
      const type = this.getBusTypeLabel(trip.busType).toLowerCase();
      const hour = new Date(trip.departureTime).getHours();
      const matchType = busType === 'all' || type === busType;
      const matchPrice = Number(trip.baseFare || 0) <= maxPrice;
      const matchTime = window == null || (hour >= window.start && hour < window.end);
      return matchType && matchPrice && matchTime;
    });

    this.filteredTrips = [...filtered].sort((a, b) => this.sortTrips(a, b, sortBy));
  }

  private sortTrips(a: TripSearchResponse, b: TripSearchResponse, sortBy: string): number {
    if (sortBy === 'price') return a.baseFare - b.baseFare;
    if (sortBy === 'duration') return this.getDurationMinutes(a) - this.getDurationMinutes(b);
    if (sortBy === 'departure') return new Date(a.departureTime).getTime() - new Date(b.departureTime).getTime();
    return a.baseFare - b.baseFare;
  }

  private getDurationMinutes(trip: TripSearchResponse): number {
    return Math.max(0, Math.round((new Date(trip.arrivalTime).getTime() - new Date(trip.departureTime).getTime()) / 60000));
  }

  private getDepartureWindow(value: string): { start: number; end: number } | null {
    if (value === 'all') return null;
    const [start, end] = value.split('-').map(Number);
    return { start, end };
  }

  private formatLocation(location: LocationOption): string {
    return `${location.name}, ${location.city}, ${location.state}`;
  }
}
