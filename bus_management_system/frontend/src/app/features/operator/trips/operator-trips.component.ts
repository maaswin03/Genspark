import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import { AdminBadgeComponent } from '../../admin/shared/admin-badge.component';
import { AdminModalComponent } from '../../admin/shared/admin-modal.component';
import { AdminTableShellComponent } from '../../admin/shared/admin-table-shell.component';
import {
  BusResponse,
  OperatorRouteOption,
  OperatorTripResponse,
  OperatorTripScheduleRequest,
  OperatorTripScheduleResponse,
} from '../../../core/models/operator.models';
import { OperatorService } from '../../../core/services/operator.service';

type TripsTab = 'schedules' | 'trips';

type ScheduleViewModel = {
  id: number;
  busId: number;
  routeId: number;
  busNumber: string;
  routeName: string;
  departureTime: string;
  arrivalTime: string;
  daysOfWeek: string;
  validFrom: string;
  validUntil?: string | null;
  baseFare: number;
  isActive: boolean;
};



// Cross-field validator: validUntil must be after validFrom (if provided)
function validUntilAfterValidFromValidator(): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const from = group.get('validFrom')?.value as string;
    const until = group.get('validUntil')?.value as string;
    if (!from || !until) return null;
    if (until <= from) {
      return { validUntilNotAfterValidFrom: true };
    }
    return null;
  };
}

@Component({
  selector: 'app-operator-trips',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, AdminTableShellComponent, AdminBadgeComponent, AdminModalComponent],
  templateUrl: './operator-trips.component.html',
  styleUrl: './operator-trips.component.css',
})
export class OperatorTripsComponent implements OnInit {
  private static readonly dayToNumber: Record<string, string> = {
    Mon: '1',
    Tue: '2',
    Wed: '3',
    Thu: '4',
    Fri: '5',
    Sat: '6',
    Sun: '7',
  };

  private static readonly numberToDay: Record<string, string> = {
    '1': 'Mon',
    '2': 'Tue',
    '3': 'Wed',
    '4': 'Thu',
    '5': 'Fri',
    '6': 'Sat',
    '7': 'Sun',
  };

  readonly dayOptions = [
    { label: 'Mon', value: 'Mon' },
    { label: 'Tue', value: 'Tue' },
    { label: 'Wed', value: 'Wed' },
    { label: 'Thu', value: 'Thu' },
    { label: 'Fri', value: 'Fri' },
    { label: 'Sat', value: 'Sat' },
    { label: 'Sun', value: 'Sun' },
  ];

  selectedTab: TripsTab = 'schedules';
  selectedDate = this.toDateInput(new Date());

  isLoading = true;
  isSaving = false;
  isGenerating = false;
  message = '';
  messageTone: 'success' | 'danger' = 'success';

  trips: OperatorTripResponse[] = [];
  scheduleList: ScheduleViewModel[] = [];
  buses: BusResponse[] = [];
  routeOptions: OperatorRouteOption[] = [];

  scheduleModalOpen = false;
  scheduleModalMode: 'create' | 'edit' = 'create';
  editingScheduleId: number | null = null;

  changeBusModalOpen = false;
  selectedTripForBusChange: OperatorTripResponse | null = null;

  cancelTripModalOpen = false;
  selectedTripForCancel: OperatorTripResponse | null = null;

  readonly scheduleForm;
  readonly changeBusForm;
  readonly cancelForm;

  constructor(
    private readonly operatorService: OperatorService,
    private readonly fb: FormBuilder,
    private readonly cdr: ChangeDetectorRef,
    private readonly router: Router
  ) {
    this.scheduleForm = this.fb.group(
      {
        busId: [null as number | null, [Validators.required]],
        routeId: [null as number | null, [Validators.required]],
        departureTime: ['', [Validators.required]],
        arrivalTime: ['', [Validators.required]],
        baseFare: [500, [Validators.required, Validators.min(1)]],
        validFrom: [this.toDateInput(new Date()), [Validators.required]],
        validUntil: [''],
        days: this.fb.group({
          Mon: [true],
          Tue: [true],
          Wed: [true],
          Thu: [true],
          Fri: [true],
          Sat: [false],
          Sun: [false],
        }),
        isActive: [true],
      },
      {
        validators: [validUntilAfterValidFromValidator()],
      }
    );

    this.changeBusForm = this.fb.group({
      newBusId: [null as number | null, [Validators.required]],
      changeType: ['temporary', [Validators.required]],
      reason: ['', [Validators.required, Validators.maxLength(500)]],
    });

    this.cancelForm = this.fb.group({
      reason: ['', [Validators.required, Validators.maxLength(500)]],
    });
  }

  ngOnInit(): void {
    this.loadData();
  }

  /** ISO date string for today — used as [min] on date inputs to prevent past dates. */
  get todayIso(): string {
    return this.toDateInput(new Date());
  }

  get filteredTrips(): OperatorTripResponse[] {
    const target = this.selectedDate;
    return this.trips.filter((trip) => this.toDateInput(new Date(trip.departureTime)) === target);
  }

  /** Whether the cross-field date range error is active. */
  get showValidUntilError(): boolean {
    const from = this.scheduleForm.get('validFrom');
    const until = this.scheduleForm.get('validUntil');
    return (
      !!this.scheduleForm.hasError('validUntilNotAfterValidFrom') &&
      (from?.touched ?? false) &&
      (until?.touched ?? false)
    );
  }

  setTab(tab: TripsTab): void {
    this.selectedTab = tab;
  }

  generateTrips(): void {
    this.isGenerating = true;
    this.message = '';
    this.cdr.detectChanges();

    this.operatorService
      .generateTodayTrips()
      .pipe(
        finalize(() => {
          this.isGenerating = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: () => {
          this.message = "Today's trips generated successfully.";
          this.messageTone = 'success';
          this.cdr.detectChanges();
          this.loadData();
        },
        error: () => {
          this.message = 'Failed to generate trips. Please try again.';
          this.messageTone = 'danger';
          this.cdr.detectChanges();
        },
      });
  }

  loadData(): void {
    this.isLoading = true;
    this.message = '';
    this.cdr.detectChanges();

    forkJoin({
      trips: this.operatorService.getTrips().pipe(catchError(() => of([] as OperatorTripResponse[]))),
      schedules: this.operatorService.getSchedules().pipe(catchError(() => of([] as OperatorTripScheduleResponse[]))),
      buses: this.operatorService.getBuses().pipe(catchError(() => of([] as BusResponse[]))),
      routes: this.operatorService.getRouteOptions().pipe(catchError(() => of([] as OperatorRouteOption[]))),
    })
      .pipe(
        finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: ({ trips, schedules, buses, routes }) => {
          this.trips = trips;
          this.buses = buses;
          this.routeOptions = routes;
          this.scheduleList = schedules.map((schedule) => this.mapScheduleResponse(schedule));
          this.cdr.detectChanges();
        },
        error: () => {
          this.message = 'Unable to load trips and schedules right now.';
          this.messageTone = 'danger';
          this.cdr.detectChanges();
        },
      });
  }

  openCreateSchedule(): void {
    this.scheduleModalMode = 'create';
    this.editingScheduleId = null;

    this.scheduleForm.reset({
      busId: this.buses[0]?.id ?? null,
      routeId: this.routeOptions[0]?.id ?? null,
      departureTime: '',
      arrivalTime: '',
      baseFare: 500,
      validFrom: this.toDateInput(new Date()),
      validUntil: '',
      days: {
        Mon: true,
        Tue: true,
        Wed: true,
        Thu: true,
        Fri: true,
        Sat: false,
        Sun: false,
      },
      isActive: true,
    });

    this.scheduleModalOpen = true;
  }

  openEditSchedule(schedule: ScheduleViewModel): void {
    this.scheduleModalMode = 'edit';
    this.editingScheduleId = schedule.id;

    const selectedDays = this.parseDays(schedule.daysOfWeek);

    this.scheduleForm.reset({
      busId: schedule.busId || this.findBusId(schedule.busNumber),
      routeId: schedule.routeId || this.findRouteId(schedule.routeName),
      departureTime: this.toTimeInput(schedule.departureTime),
      arrivalTime: this.toTimeInput(schedule.arrivalTime),
      baseFare: schedule.baseFare,
      validFrom: this.toDateInput(new Date(schedule.validFrom)),
      validUntil: schedule.validUntil ? this.toDateInput(new Date(schedule.validUntil)) : '',
      days: {
        Mon: selectedDays.includes('Mon'),
        Tue: selectedDays.includes('Tue'),
        Wed: selectedDays.includes('Wed'),
        Thu: selectedDays.includes('Thu'),
        Fri: selectedDays.includes('Fri'),
        Sat: selectedDays.includes('Sat'),
        Sun: selectedDays.includes('Sun'),
      },
      isActive: schedule.isActive,
    });

    this.scheduleModalOpen = true;
  }

  closeScheduleModal(): void {
    if (this.isSaving) {
      return;
    }

    this.scheduleModalOpen = false;
  }

  submitSchedule(): void {
    this.scheduleForm.markAllAsTouched();

    if (this.scheduleForm.invalid) {
      return;
    }

    const selectedDays = this.selectedDays;
    if (selectedDays.length === 0) {
      this.message = 'Select at least one day of week.';
      this.messageTone = 'danger';
      return;
    }

    const busId = Number(this.scheduleForm.controls.busId.value);
    const routeId = Number(this.scheduleForm.controls.routeId.value);
    const validFrom = this.scheduleForm.controls.validFrom.value || this.toDateInput(new Date());

    // Prevent past validFrom date
    if (validFrom < this.todayIso) {
      this.message = 'Valid From date cannot be in the past.';
      this.messageTone = 'danger';
      return;
    }

    // Issue #1 — Prevent same bus already active on the same route (duplicate schedule check)
    if (this.scheduleModalMode === 'create') {
      const duplicate = this.scheduleList.find(
        (s) => s.busId === busId && s.routeId === routeId && s.isActive && s.id !== this.editingScheduleId
      );
      if (duplicate) {
        this.message = `Bus "${duplicate.busNumber}" already has an active schedule for this route. Please choose a different bus or route.`;
        this.messageTone = 'danger';
        return;
      }
    }

    const payload: OperatorTripScheduleRequest = {
      busId,
      routeId,
      departureTime: `${this.scheduleForm.controls.departureTime.value}:00`,
      arrivalTime: `${this.scheduleForm.controls.arrivalTime.value}:00`,
      baseFare: Number(this.scheduleForm.controls.baseFare.value),
      daysOfWeek: this.serializeDays(selectedDays),
      validFrom,
      validUntil: this.scheduleForm.controls.validUntil.value || null,
    };

    const request$ = this.scheduleModalMode === 'create'
      ? this.operatorService.createSchedule(payload)
      : this.operatorService.updateSchedule(this.editingScheduleId ?? 0, {
          ...payload,
          isActive: Boolean(this.scheduleForm.controls.isActive.value),
        });

    this.isSaving = true;
    this.message = '';

    request$
      .pipe(
        finalize(() => {
          this.isSaving = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (saved) => {
          this.scheduleModalOpen = false;
          this.syncSavedSchedule(saved);
          this.message = this.scheduleModalMode === 'create' ? 'Schedule added successfully.' : 'Schedule updated successfully.';
          this.messageTone = 'success';
          this.cdr.detectChanges();
          this.loadData();
        },
        error: (error) => {
          if (error?.status === 0) {
            this.message = 'Cannot reach server. Check internet/VPN connection and confirm backend is running on http://localhost:5131.';
          } else if (error?.error && typeof error.error === 'string') {
            this.message = error.error;
          } else {
            this.message = 'Unable to save schedule. Please verify route and bus selections.';
          }
          this.messageTone = 'danger';
          this.cdr.detectChanges();
        },
      });
  }

  toggleSchedule(schedule: ScheduleViewModel): void {
    this.isSaving = true;
    this.message = '';
    this.cdr.detectChanges();

    this.operatorService
      .toggleSchedule(schedule.id)
      .pipe(
        finalize(() => {
          this.isSaving = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (updated) => {
          this.syncSavedSchedule(updated);
          this.message = updated.isActive ? 'Schedule resumed.' : 'Schedule paused.';
          this.messageTone = 'success';
          this.cdr.detectChanges();
        },
        error: () => {
          this.message = 'Unable to toggle schedule right now.';
          this.messageTone = 'danger';
          this.cdr.detectChanges();
        },
      });
  }

  openChangeBusModal(trip: OperatorTripResponse): void {
    this.selectedTripForBusChange = trip;
    this.changeBusForm.reset({
      newBusId: this.buses.find((bus) => bus.busNumber === trip.busNumber)?.id ?? this.buses[0]?.id ?? null,
      changeType: 'temporary',
      reason: '',
    });
    this.changeBusModalOpen = true;
  }

  closeChangeBusModal(): void {
    if (this.isSaving) {
      return;
    }

    this.changeBusModalOpen = false;
    this.selectedTripForBusChange = null;
  }

  submitBusChange(): void {
    if (!this.selectedTripForBusChange) {
      return;
    }

    if (this.changeBusForm.invalid) {
      this.changeBusForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.message = '';

    const tripId = this.selectedTripForBusChange.tripId;
    const changeType = String(this.changeBusForm.controls.changeType.value ?? 'temporary');
    const reason = String(this.changeBusForm.controls.reason.value ?? '').trim();

    this.operatorService
      .changeTripBus(tripId, {
        newBusId: Number(this.changeBusForm.controls.newBusId.value),
        reason: `[${changeType}] ${reason}`,
      })
      .pipe(
        finalize(() => {
          this.isSaving = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: () => {
          this.changeBusModalOpen = false;
          this.selectedTripForBusChange = null;
          this.message = 'Trip bus changed successfully.';
          this.messageTone = 'success';
          this.cdr.detectChanges();
          this.loadData();
        },
        error: () => {
          this.message = 'Unable to change trip bus right now.';
          this.messageTone = 'danger';
          this.cdr.detectChanges();
        },
      });
  }

  openCancelModal(trip: OperatorTripResponse): void {
    this.selectedTripForCancel = trip;
    this.cancelForm.reset({ reason: '' });
    this.cancelTripModalOpen = true;
  }

  closeCancelModal(): void {
    if (this.isSaving) {
      return;
    }

    this.cancelTripModalOpen = false;
    this.selectedTripForCancel = null;
  }

  submitCancelTrip(): void {
    if (!this.selectedTripForCancel) {
      return;
    }

    if (this.cancelForm.invalid) {
      this.cancelForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.message = '';

    this.operatorService
      .cancelTrip(this.selectedTripForCancel.tripId, {
        reason: String(this.cancelForm.controls.reason.value ?? '').trim(),
      })
      .pipe(
        finalize(() => {
          this.isSaving = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: () => {
          this.cancelTripModalOpen = false;
          this.selectedTripForCancel = null;
          this.message = 'Trip cancelled and passengers notified.';
          this.messageTone = 'success';
          this.cdr.detectChanges();
          this.loadData();
        },
        error: () => {
          this.message = 'Unable to cancel trip right now.';
          this.messageTone = 'danger';
          this.cdr.detectChanges();
        },
      });
  }

  goToTripDetail(tripId: number): void {
    this.router.navigate(['/operator/trips', tripId]);
  }

  private readonly tripStatusMap: Record<string, string> = {
    '0': 'Scheduled',
    '1': 'Active',
    '2': 'Completed',
    '3': 'Cancelled',
  };

  getTripStatusTone(status: number | string): 'success' | 'danger' | 'warning' | 'dark' {
    const value = this.getStatusLabel(status).toLowerCase();

    if (value.includes('active') || value.includes('scheduled')) {
      return 'success';
    }

    if (value.includes('cancel')) {
      return 'danger';
    }

    if (value.includes('complete')) {
      return 'dark';
    }

    return 'warning';
  }

  getStatusLabel(value: number | string): string {
    const key = String(value).trim().toLowerCase();
    if (this.tripStatusMap[key]) return this.tripStatusMap[key];

    return String(value)
      .replaceAll('_', ' ')
      .replace(/\b\w/g, (segment) => segment.toUpperCase());
  }

  trackByScheduleId(_: number, row: ScheduleViewModel): number {
    return row.id;
  }

  trackByTripId(_: number, row: OperatorTripResponse): number {
    return row.tripId;
  }

  get selectedDays(): string[] {
    const daysGroup = this.scheduleForm.controls.days.controls;
    return this.dayOptions
      .filter((day) => Boolean(daysGroup[day.value as keyof typeof daysGroup].value))
      .map((day) => day.value);
  }

  private syncSavedSchedule(saved: OperatorTripScheduleResponse): void {
    const mapped = this.mapScheduleResponse(saved);

    const index = this.scheduleList.findIndex((item) => item.id === saved.id);
    if (index >= 0) {
      this.scheduleList[index] = mapped;
      this.scheduleList = [...this.scheduleList];
      return;
    }

    this.scheduleList = [mapped, ...this.scheduleList];
  }

  private mapScheduleResponse(saved: OperatorTripScheduleResponse): ScheduleViewModel {
    const routeLabel = this.routeOptions.find((route) => route.id === saved.routeId);
    const bus = this.buses.find((item) => item.id === saved.busId);

    // Issue #2 — daysOfWeek from the backend is a TimeSpan for dep/arr, and a comma-separated
    // string of integers ("1,2,3") for days. parseDays handles both numeric and named day tokens.
    const parsedDays = this.parseDays(saved.daysOfWeek);

    return {
      id: saved.id,
      busId: saved.busId,
      routeId: saved.routeId,
      busNumber: bus?.busNumber ?? `Bus #${saved.busId}`,
      routeName: routeLabel ? `${routeLabel.sourceName} - ${routeLabel.destinationName}` : `Route #${saved.routeId}`,
      departureTime: saved.departureTime,
      arrivalTime: saved.arrivalTime,
      daysOfWeek: parsedDays.join(', '),
      validFrom: saved.validFrom,
      validUntil: saved.validUntil,
      baseFare: Number(saved.baseFare ?? 0),
      isActive: saved.isActive,
    };
  }

  private findBusId(busNumber: string): number {
    return this.buses.find((bus) => bus.busNumber === busNumber)?.id ?? 0;
  }

  private findRouteId(routeName: string): number {
    const normalized = routeName.trim().toLowerCase().replace('->', '-');
    return (
      this.routeOptions.find((route) => `${route.sourceName} - ${route.destinationName}`.trim().toLowerCase().replace('->', '-') === normalized)
        ?.id ??
      0
    );
  }

  // Issue #2 fix — parseDays handles: "1,2,3" (numeric), "Mon,Tue" (named), "Monday" (full), mixed
  private parseDays(days: string): string[] {
    return String(days || '')
      .split(',')
      .map((value) => value.trim())
      .filter((value) => value.length > 0)
      .map((value) => {
        // Numeric token e.g. "1" → "Mon"
        const numericMapped = OperatorTripsComponent.numberToDay[value];
        if (numericMapped) {
          return numericMapped;
        }
        // Already a 3-letter abbreviation e.g. "Mon"
        const knownDays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
        const title = value.charAt(0).toUpperCase() + value.slice(1).toLowerCase();
        const threeLetters = title.slice(0, 3);
        return knownDays.includes(threeLetters) ? threeLetters : value;
      });
  }

  private serializeDays(days: string[]): string {
    // Issue #2 fix — always serialize as numeric codes ("1,2,3") so the backend stores consistently
    return days.map((day) => OperatorTripsComponent.dayToNumber[day] ?? day).join(',');
  }

  private toDateInput(value: Date): string {
    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  // Issue #2 fix — toTimeInput must also handle bare TimeSpan strings like "08:30:00" from the backend
  private toTimeInput(value: string): string {
    if (!value) return '00:00';

    // TimeSpan format from C# e.g. "08:30:00" or "08:30:00.0000000"
    const timeSpanMatch = /^(\d{1,2}):(\d{2})(?::\d{2})?/.exec(value);
    if (timeSpanMatch) {
      return `${timeSpanMatch[1].padStart(2, '0')}:${timeSpanMatch[2]}`;
    }

    // Full ISO datetime string
    const date = new Date(value);
    if (!Number.isNaN(date.getTime())) {
      return `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;
    }

    const raw = String(value);
    return raw.length >= 5 ? raw.slice(0, 5) : '00:00';
  }
}
