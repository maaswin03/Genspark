import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import {
  AddRouteStopsRequest,
  LocationResponse,
  RouteResponse,
  RouteStopInput,
  RouteStopResponse,
} from '../../../core/models/admin.models';
import { AdminService } from '../../../core/services/admin.service';
import { AdminBadgeComponent } from '../shared/admin-badge.component';
import { AdminModalComponent } from '../shared/admin-modal.component';
import { AdminTableShellComponent } from '../shared/admin-table-shell.component';

@Component({
  selector: 'app-admin-routes',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    AdminModalComponent,
    AdminTableShellComponent,
    AdminBadgeComponent,
  ],
  templateUrl: './admin-routes.component.html',
  styleUrl: './admin-routes.component.css',
})
export class AdminRoutesComponent {
  isLoading = true;
  isSavingRoute = false;
  isSavingStops = false;

  message = '';
  messageTone: 'success' | 'danger' = 'success';

  activeFilter: 'all' | 'active' | 'inactive' = 'all';

  routes: RouteResponse[] = [];
  filteredRoutes: RouteResponse[] = [];
  locations: LocationResponse[] = [];

  expandedRouteIds = new Set<number>();

  isRouteModalOpen = false;
  isStopsModalOpen = false;
  selectedRouteForStops: RouteResponse | null = null;

  readonly routeForm;
  readonly stopsForm;

  constructor(
    private readonly adminService: AdminService,
    private readonly fb: FormBuilder,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.routeForm = this.fb.group({
      sourceId: [null as number | null, [Validators.required]],
      destinationId: [null as number | null, [Validators.required]],
      distanceKm: [null as number | null, [Validators.required, Validators.min(1)]],
    });

    this.stopsForm = this.fb.group({
      stops: this.fb.array([]),
    });
  }

  ngOnInit(): void {
    this.loadData();
  }

  get stopsArray(): FormArray {
    return this.stopsForm.controls.stops as FormArray;
  }

  loadData(): void {
    this.isLoading = true;
    this.message = '';

    forkJoin({
      routes: this.adminService.getRoutes(),
      locations: this.adminService.getLocations(),
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
        next: ({ routes, locations }) => {
          this.zone.run(() => {
            this.routes = routes;
            this.locations = locations.sort((a, b) => a.name.localeCompare(b.name));
            this.applyFilter(this.activeFilter);
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to load routes right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  applyFilter(filter: 'all' | 'active' | 'inactive'): void {
    this.activeFilter = filter;
    this.filteredRoutes = this.routes.filter((route) => {
      if (filter === 'active') {
        return route.isActive;
      }

      if (filter === 'inactive') {
        return !route.isActive;
      }

      return true;
    });
  }

  toggleExpand(routeId: number): void {
    if (this.expandedRouteIds.has(routeId)) {
      this.expandedRouteIds.delete(routeId);
    } else {
      this.expandedRouteIds.add(routeId);
    }
  }

  isExpanded(routeId: number): boolean {
    return this.expandedRouteIds.has(routeId);
  }

  openRouteModal(): void {
    this.routeForm.reset({ sourceId: null, destinationId: null, distanceKm: null });
    this.isRouteModalOpen = true;
  }

  closeRouteModal(): void {
    if (this.isSavingRoute) {
      return;
    }

    this.isRouteModalOpen = false;
  }

  submitRoute(): void {
    if (this.routeForm.invalid) {
      this.routeForm.markAllAsTouched();
      return;
    }

    const sourceId = Number(this.routeForm.controls.sourceId.value);
    const destinationId = Number(this.routeForm.controls.destinationId.value);
    const distanceKm = Number(this.routeForm.controls.distanceKm.value);

    if (!sourceId || !destinationId || !distanceKm) {
      this.routeForm.markAllAsTouched();
      return;
    }

    if (sourceId === destinationId) {
      this.message = 'Source and destination must be different.';
      this.messageTone = 'danger';
      return;
    }

    this.isSavingRoute = true;
    this.message = '';

    this.adminService
      .createRoute({ sourceId, destinationId, distanceKm })
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isSavingRoute = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: (created) => {
          this.zone.run(() => {
            this.routes = [created, ...this.routes];
            this.applyFilter(this.activeFilter);
            this.isRouteModalOpen = false;
            this.message = 'Route added successfully.';
            this.messageTone = 'success';
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to add route right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  openStopsModal(route: RouteResponse): void {
    this.selectedRouteForStops = route;
    this.stopsArray.clear();

    const sortedStops = [...route.stops].sort((a, b) => a.stopOrder - b.stopOrder);
    const sourceStop: RouteStopResponse = {
      id: 0,
      locationId: route.sourceId,
      locationName: route.sourceName,
      stopOrder: 1,
      distanceFromSource: 0,
    };
    const destinationStop: RouteStopResponse = {
      id: 0,
      locationId: route.destinationId,
      locationName: route.destinationName,
      stopOrder: sortedStops.length > 0 ? sortedStops.length : 2,
      distanceFromSource: route.distanceKm,
    };

    const stopRows = sortedStops.length > 0 ? sortedStops : [sourceStop, destinationStop];
    for (const stop of stopRows) {
      this.stopsArray.push(
        this.fb.group({
          locationId: [stop.locationId, [Validators.required]],
          stopOrder: [stop.stopOrder, [Validators.required, Validators.min(1)]],
          distanceFromSource: [stop.distanceFromSource, [Validators.required, Validators.min(0)]],
        })
      );
    }

    this.reindexStops();
    this.isStopsModalOpen = true;
  }

  closeStopsModal(): void {
    if (this.isSavingStops) {
      return;
    }

    this.isStopsModalOpen = false;
    this.selectedRouteForStops = null;
    this.stopsArray.clear();
  }

  addStopRow(): void {
    this.stopsArray.push(
      this.fb.group({
        locationId: [null as number | null, [Validators.required]],
        stopOrder: [this.stopsArray.length + 1, [Validators.required, Validators.min(1)]],
        distanceFromSource: [0, [Validators.required, Validators.min(0)]],
      })
    );
    this.reindexStops();
  }

  removeStopRow(index: number): void {
    if (this.stopsArray.length <= 2) {
      return;
    }

    this.stopsArray.removeAt(index);
    this.reindexStops();
  }

  submitStops(): void {
    if (!this.selectedRouteForStops) {
      return;
    }

    if (this.stopsForm.invalid) {
      this.stopsForm.markAllAsTouched();
      return;
    }

    const stops = this.stopsArray.controls
      .map((control) => ({
        locationId: Number(control.value.locationId),
        stopOrder: Number(control.value.stopOrder),
        distanceFromSource: Number(control.value.distanceFromSource),
      }))
      .sort((a, b) => a.stopOrder - b.stopOrder);

    const validationError = this.validateStops(stops, this.selectedRouteForStops);
    if (validationError) {
      this.message = validationError;
      this.messageTone = 'danger';
      return;
    }

    this.isSavingStops = true;
    this.message = '';

    const payload: AddRouteStopsRequest = { stops: stops as RouteStopInput[] };
    this.adminService
      .addRouteStops(this.selectedRouteForStops.id, payload)
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isSavingStops = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: (updated) => {
          this.zone.run(() => {
            this.routes = this.routes.map((route) => (route.id === updated.id ? updated : route));
            this.applyFilter(this.activeFilter);
            this.message = 'Stops updated successfully.';
            this.messageTone = 'success';
            this.closeStopsModal();
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to update stops right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  toggleRoute(routeId: number): void {
    this.adminService.toggleRoute(routeId).subscribe({
      next: (updated) => {
        this.zone.run(() => {
          this.routes = this.routes.map((route) => (route.id === routeId ? updated : route));
          this.applyFilter(this.activeFilter);
          this.message = `Route is now ${updated.isActive ? 'active' : 'inactive'}.`;
          this.messageTone = 'success';
          this.cdr.markForCheck();
        });
      },
      error: () => {
        this.zone.run(() => {
          this.message = 'Unable to toggle route status right now.';
          this.messageTone = 'danger';
          this.cdr.markForCheck();
        });
      },
    });
  }

  locationLabel(locationId: number): string {
    const match = this.locations.find((location) => location.id === locationId);
    return match ? `${match.name}, ${match.city}, ${match.state}` : 'Select location';
  }

  private reindexStops(): void {
    this.stopsArray.controls.forEach((control, index) => {
      control.patchValue({ stopOrder: index + 1 }, { emitEvent: false });
    });
  }

  private validateStops(
    stops: Array<{ locationId: number; stopOrder: number; distanceFromSource: number }>,
    route: RouteResponse
  ): string | null {
    if (stops.length < 2) {
      return 'At least source and destination stops are required.';
    }

    if (stops[0].locationId !== route.sourceId) {
      return 'First stop must match route source.';
    }

    if (stops[stops.length - 1].locationId !== route.destinationId) {
      return 'Last stop must match route destination.';
    }

    for (let index = 1; index < stops.length; index += 1) {
      const previous = stops[index - 1];
      const current = stops[index];
      if (current.distanceFromSource <= previous.distanceFromSource) {
        return 'Stop distances must be strictly increasing.';
      }
    }

    if (stops[stops.length - 1].distanceFromSource > route.distanceKm) {
      return 'Last stop distance cannot exceed route distance.';
    }

    return null;
  }
}
