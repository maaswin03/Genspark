import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, startWith, takeUntil } from 'rxjs';
import { LocationOption, LocationService } from '../../../core/services/location.service';

@Component({
  selector: 'app-trip-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './trip-search.component.html',
  styleUrl: './trip-search.component.css',
})
export class TripSearchComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  readonly form;

  allLocations: LocationOption[] = [];
  fromSuggestions: LocationOption[] = [];
  toSuggestions: LocationOption[] = [];
  selectedFrom: LocationOption | null = null;
  selectedTo: LocationOption | null = null;

  showFromDropdown = false;
  showToDropdown = false;
  loadingLocations = false;
  isSearching = false;
  errorMessage = '';

  constructor(
    private readonly fb: FormBuilder,
    private readonly router: Router,
    private readonly locationService: LocationService,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      fromQuery: ['', Validators.required],
      toQuery: ['', Validators.required],
      journeyDate: ['', Validators.required],
      returnJourneyEnabled: [false],
      returnJourneyDate: [''],
    });
  }

  ngOnInit(): void {
    this.loadingLocations = true;
    this.cdr.detectChanges();

    this.locationService
      .getLocations()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (locations) => {
          this.allLocations = locations;
          this.loadingLocations = false;
          this.updateFromSuggestions();
          this.updateToSuggestions();
          this.cdr.detectChanges();
        },
        error: () => {
          this.loadingLocations = false;
          this.errorMessage = 'Unable to load locations. Please refresh the page.';
          this.cdr.detectChanges();
        },
      });

    this.form.controls.fromQuery.valueChanges
      .pipe(startWith(''), debounceTime(200), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((value) => {
        if (this.selectedFrom && value !== this.formatLocation(this.selectedFrom)) {
          this.selectedFrom = null;
        }
        this.updateFromSuggestions(value ?? '');
      });

    this.form.controls.toQuery.valueChanges
      .pipe(startWith(''), debounceTime(200), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((value) => {
        if (this.selectedTo && value !== this.formatLocation(this.selectedTo)) {
          this.selectedTo = null;
        }
        this.updateToSuggestions(value ?? '');
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /** Minimum date (today) for the date picker */
  get todayIso(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  // ── Focus / Blur ──────────────────────────────────────────────────────────

  onFromFocus(): void {
    this.showFromDropdown = true;
    this.updateFromSuggestions(this.form.controls.fromQuery.value ?? '');
  }

  onToFocus(): void {
    this.showToDropdown = true;
    this.updateToSuggestions(this.form.controls.toQuery.value ?? '');
  }

  onFromBlur(): void {
    window.setTimeout(() => {
      this.showFromDropdown = false;
      // If user typed something but didn't pick a location, reset
      if (!this.selectedFrom && (this.form.controls.fromQuery.value || '').trim()) {
        this.form.controls.fromQuery.setValue('');
      }
    }, 180);
  }

  onToBlur(): void {
    window.setTimeout(() => {
      this.showToDropdown = false;
      if (!this.selectedTo && (this.form.controls.toQuery.value || '').trim()) {
        this.form.controls.toQuery.setValue('');
      }
    }, 180);
  }

  // ── Selection ─────────────────────────────────────────────────────────────

  selectFrom(location: LocationOption): void {
    this.selectedFrom = location;
    this.form.controls.fromQuery.setValue(this.formatLocation(location), { emitEvent: false });
    this.showFromDropdown = false;
    this.updateToSuggestions(this.form.controls.toQuery.value ?? '');
  }

  selectTo(location: LocationOption): void {
    this.selectedTo = location;
    this.form.controls.toQuery.setValue(this.formatLocation(location), { emitEvent: false });
    this.showToDropdown = false;
    this.updateFromSuggestions(this.form.controls.fromQuery.value ?? '');
  }

  clearFrom(): void {
    this.selectedFrom = null;
    this.form.controls.fromQuery.setValue('');
    this.updateToSuggestions(this.form.controls.toQuery.value ?? '');
  }

  clearTo(): void {
    this.selectedTo = null;
    this.form.controls.toQuery.setValue('');
    this.updateFromSuggestions(this.form.controls.fromQuery.value ?? '');
  }

  swapLocations(): void {
    const fromVal = this.form.controls.fromQuery.value ?? '';
    const toVal = this.form.controls.toQuery.value ?? '';
    const fromLoc = this.selectedFrom;
    const toLoc = this.selectedTo;

    this.selectedFrom = toLoc;
    this.selectedTo = fromLoc;

    this.form.patchValue(
      { fromQuery: toVal, toQuery: fromVal },
      { emitEvent: false }
    );

    this.updateFromSuggestions(toVal);
    this.updateToSuggestions(fromVal);
  }

  // ── Search ────────────────────────────────────────────────────────────────

  searchTrips(): void {
    this.errorMessage = '';
    this.form.markAllAsTouched();

    // Validation: must select a location from dropdown (not just type text)
    if (!this.selectedFrom) {
      this.errorMessage = 'Please select a valid departure city from the dropdown.';
      return;
    }

    if (!this.selectedTo) {
      this.errorMessage = 'Please select a valid destination city from the dropdown.';
      return;
    }

    if (this.selectedFrom.id === this.selectedTo.id) {
      this.errorMessage = 'Departure and destination cannot be the same city.';
      return;
    }

    if (!this.form.controls.journeyDate.value) {
      this.errorMessage = 'Please choose a journey date.';
      return;
    }

    const journeyDate = this.form.controls.journeyDate.value;
    if (journeyDate < this.todayIso) {
      this.errorMessage = 'Journey date cannot be in the past.';
      return;
    }

    const returnEnabled = this.form.controls.returnJourneyEnabled.value;
    const returnDate = this.form.controls.returnJourneyDate.value;

    if (returnEnabled && (!returnDate || returnDate < journeyDate)) {
      this.errorMessage = 'Return date must be on or after the journey date.';
      return;
    }

    this.router.navigate(['/trips/search'], {
      queryParams: {
        sourceId: this.selectedFrom.id,
        destinationId: this.selectedTo.id,
        date: journeyDate,
      },
    });
  }

  // ── Getters ───────────────────────────────────────────────────────────────

  get fromRequiredError(): boolean {
    const ctrl = this.form.controls.fromQuery;
    return ctrl.touched && (!this.selectedFrom);
  }

  get toRequiredError(): boolean {
    const ctrl = this.form.controls.toQuery;
    return ctrl.touched && (!this.selectedTo);
  }

  get dateRequiredError(): boolean {
    return this.form.controls.journeyDate.touched && this.form.controls.journeyDate.invalid;
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  formatLocation(location: LocationOption): string {
    return `${location.name}, ${location.city}, ${location.state}`;
  }

  trackByLocationId(_: number, location: LocationOption): number {
    return location.id;
  }

  private updateFromSuggestions(term: string = ''): void {
    const q = term.trim().toLowerCase();
    this.fromSuggestions = this.allLocations.filter((loc) => {
      if (this.selectedTo?.id === loc.id) return false;
      if (!q) return true;
      return this.formatLocation(loc).toLowerCase().includes(q);
    });
  }

  private updateToSuggestions(term: string = ''): void {
    const q = term.trim().toLowerCase();
    this.toSuggestions = this.allLocations.filter((loc) => {
      if (this.selectedFrom?.id === loc.id) return false;
      if (!q) return true;
      return this.formatLocation(loc).toLowerCase().includes(q);
    });
  }
}
