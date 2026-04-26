import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import { LocationResponse, RouteResponse } from '../../../core/models/admin.models';
import { AdminService } from '../../../core/services/admin.service';
import { AdminModalComponent } from '../shared/admin-modal.component';
import { AdminTableShellComponent } from '../shared/admin-table-shell.component';

@Component({
  selector: 'app-admin-locations',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, AdminModalComponent, AdminTableShellComponent],
  templateUrl: './admin-locations.component.html',
  styleUrl: './admin-locations.component.css',
})
export class AdminLocationsComponent {
  isLoading = true;
  isSaving = false;
  searchTerm = '';
  message = '';
  messageTone: 'success' | 'danger' = 'success';

  locations: LocationResponse[] = [];
  filteredLocations: LocationResponse[] = [];

  isModalOpen = false;
  editingLocationId: number | null = null;

  readonly locationForm;

  constructor(
    private readonly adminService: AdminService,
    private readonly fb: FormBuilder,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.locationForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      city: ['', [Validators.required, Validators.maxLength(100)]],
      state: ['', [Validators.required, Validators.maxLength(100)]],
    });
  }

  ngOnInit(): void {
    this.loadLocations();
  }

  loadLocations(): void {
    this.isLoading = true;
    this.message = '';

    forkJoin({
      locations: this.adminService.getLocations(),
      routes: this.adminService.getRoutes(),
    })
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isLoading = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: ({ locations, routes }) => {
          this.zone.run(() => {
            this.locations = this.mergeUsageCounts(locations, routes);
            this.applyFilter();
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to load locations right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  openAddModal(): void {
    this.editingLocationId = null;
    this.locationForm.reset({ name: '', city: '', state: '' });
    this.isModalOpen = true;
  }

  openEditModal(location: LocationResponse): void {
    this.editingLocationId = location.id;
    this.locationForm.reset({
      name: location.name,
      city: location.city,
      state: location.state,
    });
    this.isModalOpen = true;
  }

  closeModal(): void {
    if (this.isSaving) {
      return;
    }

    this.isModalOpen = false;
    this.locationForm.reset({ name: '', city: '', state: '' });
  }

  submitLocation(): void {
    if (this.locationForm.invalid) {
      this.locationForm.markAllAsTouched();
      return;
    }

    const payload = {
      name: this.locationForm.controls.name.value?.trim() ?? '',
      city: this.locationForm.controls.city.value?.trim() ?? '',
      state: this.locationForm.controls.state.value?.trim() ?? '',
    };

    if (!payload.name || !payload.city || !payload.state) {
      this.locationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.message = '';

    const request = this.editingLocationId
      ? this.adminService.updateLocation(this.editingLocationId, payload)
      : this.adminService.createLocation(payload);

    request
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isSaving = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: () => {
          this.zone.run(() => {
            this.isModalOpen = false;
            this.message = this.editingLocationId ? 'Location updated successfully.' : 'Location added successfully.';
            this.messageTone = 'success';
            this.loadLocations();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = this.editingLocationId
              ? 'Unable to update location right now.'
              : 'Unable to add location right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  applyFilter(): void {
    const term = this.searchTerm.trim().toLowerCase();
    this.filteredLocations = this.locations.filter(
      (location) =>
        !term ||
        location.name.toLowerCase().includes(term) ||
        location.city.toLowerCase().includes(term)
    );
  }

  private mergeUsageCounts(locations: LocationResponse[], routes: RouteResponse[]): LocationResponse[] {
    const usageMap = new Map<number, number>();

    for (const route of routes) {
      const locationIds = new Set<number>();
      locationIds.add(route.sourceId);
      locationIds.add(route.destinationId);
      for (const stop of route.stops) {
        locationIds.add(stop.locationId);
      }

      for (const locationId of locationIds) {
        usageMap.set(locationId, (usageMap.get(locationId) ?? 0) + 1);
      }
    }

    return locations
      .map((location) => ({
        ...location,
        usedInCount: usageMap.get(location.id) ?? 0,
      }))
      .sort((a, b) => a.name.localeCompare(b.name));
  }
}
