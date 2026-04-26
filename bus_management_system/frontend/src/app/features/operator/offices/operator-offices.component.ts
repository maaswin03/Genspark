import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import {
  CreateOperatorOfficeRequest,
  OperatorOfficeResponse,
  UpdateOperatorOfficeRequest,
} from '../../../core/models/operator.models';
import { LocationOption, LocationService } from '../../../core/services/location.service';
import { OperatorService } from '../../../core/services/operator.service';
import { AdminBadgeComponent } from '../../admin/shared/admin-badge.component';
import { AdminModalComponent } from '../../admin/shared/admin-modal.component';
import { AdminTableShellComponent } from '../../admin/shared/admin-table-shell.component';

@Component({
  selector: 'app-operator-offices',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AdminTableShellComponent, AdminBadgeComponent, AdminModalComponent],
  templateUrl: './operator-offices.component.html',
  styleUrl: './operator-offices.component.css',
})
export class OperatorOfficesComponent implements OnInit {
  isLoading = true;
  isSubmitting = false;
  showOfficeModal = false;

  message = '';
  messageTone: 'success' | 'danger' = 'success';

  offices: OperatorOfficeResponse[] = [];
  locations: LocationOption[] = [];

  editingOfficeId: number | null = null;

  officeForm;

  constructor(
    private readonly fb: FormBuilder,
    private readonly operatorService: OperatorService,
    private readonly locationService: LocationService,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.officeForm = this.fb.nonNullable.group({
      locationId: [0, [Validators.required, Validators.min(1)]],
      address: ['', [Validators.required, Validators.maxLength(500)]],
      isHeadOffice: [false],
    });
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    forkJoin({
      offices: this.operatorService.getOffices().pipe(catchError(() => of([] as OperatorOfficeResponse[]))),
      locations: this.locationService.getLocations().pipe(catchError(() => of([] as LocationOption[]))),
    })
      .pipe(finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: ({ offices, locations }) => {
          this.offices = this.normalizeHeadOfficeOrder(offices);
          this.locations = locations;
          this.cdr.detectChanges();
        },
        error: () => {
          this.showMessage('Unable to load offices right now.', 'danger');
          this.cdr.detectChanges();
        },
      });
  }

  openCreateOffice(): void {
    this.editingOfficeId = null;
    this.officeForm.reset({
      locationId: 0,
      address: '',
      isHeadOffice: this.offices.length === 0,
    });
    this.showOfficeModal = true;
  }

  openEditOffice(office: OperatorOfficeResponse): void {
    this.editingOfficeId = office.id;
    this.officeForm.reset({
      locationId: office.locationId,
      address: office.address,
      isHeadOffice: office.isHeadOffice,
    });
    this.showOfficeModal = true;
  }

  closeModal(): void {
    this.showOfficeModal = false;
  }

  submitOffice(): void {
    if (this.officeForm.invalid || this.isSubmitting) {
      this.officeForm.markAllAsTouched();
      return;
    }

    const value = this.officeForm.getRawValue();
    const isCreate = this.editingOfficeId === null;

    this.isSubmitting = true;

    if (isCreate) {
      this.submitCreate(value);
      return;
    }

    this.submitUpdate(this.editingOfficeId!, value);
  }

  trackByOfficeId(_: number, row: OperatorOfficeResponse): number {
    return row.id;
  }

  private submitCreate(value: { locationId: number; address: string; isHeadOffice: boolean }): void {
    const selectedLocation = this.locations.find((location) => location.id === Number(value.locationId));
    const tempId = -Date.now();

    const optimisticRow: OperatorOfficeResponse = {
      id: tempId,
      locationId: Number(value.locationId),
      locationName: selectedLocation ? this.formatLocationName(selectedLocation) : `Location ${value.locationId}`,
      address: value.address.trim(),
      isHeadOffice: value.isHeadOffice,
    };

    const snapshot = [...this.offices];
    this.offices = this.applyOfficeUpsert(this.offices, optimisticRow);
    this.closeModal();
    this.cdr.detectChanges(); // close modal + show optimistic row immediately

    const payload: CreateOperatorOfficeRequest = {
      locationId: Number(value.locationId),
      address: value.address.trim(),
      isHeadOffice: value.isHeadOffice,
    };

    this.operatorService
      .createOffice(payload)
      .pipe(finalize(() => {
        this.isSubmitting = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (saved) => {
          this.offices = this.applyOfficeUpsert(this.offices.filter((office) => office.id !== tempId), saved);
          this.showMessage('Office added successfully.', 'success');
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Failed to add office', error);
          this.offices = snapshot;
          this.showMessage('Unable to add office. Please try again.', 'danger');
          this.cdr.detectChanges();
        },
      });
  }

  private submitUpdate(officeId: number, value: { locationId: number; address: string; isHeadOffice: boolean }): void {
    const snapshot = [...this.offices];
    const existing = this.offices.find((office) => office.id === officeId);

    if (!existing) {
      this.showMessage('Office not found.', 'danger');
      this.isSubmitting = false;
      this.cdr.detectChanges();
      return;
    }

    const optimisticRow: OperatorOfficeResponse = {
      ...existing,
      address: value.address.trim(),
      isHeadOffice: value.isHeadOffice,
    };

    this.offices = this.applyOfficeUpsert(this.offices, optimisticRow);
    this.closeModal();
    this.cdr.detectChanges(); // close modal + show optimistic row immediately

    const payload: UpdateOperatorOfficeRequest = {
      address: value.address.trim(),
      isHeadOffice: value.isHeadOffice,
    };

    this.operatorService
      .updateOffice(officeId, payload)
      .pipe(finalize(() => {
        this.isSubmitting = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (saved) => {
          this.offices = this.applyOfficeUpsert(this.offices, saved);
          this.showMessage('Office updated successfully.', 'success');
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Failed to update office', error);
          this.offices = snapshot;
          this.showMessage('Unable to update office. Please try again.', 'danger');
          this.cdr.detectChanges();
        },
      });
  }

  private normalizeHeadOfficeOrder(rows: OperatorOfficeResponse[]): OperatorOfficeResponse[] {
    return [...rows].sort((a, b) => Number(b.isHeadOffice) - Number(a.isHeadOffice));
  }

  private applyOfficeUpsert(current: OperatorOfficeResponse[], row: OperatorOfficeResponse): OperatorOfficeResponse[] {
    const updated = current
      .filter((item) => item.id !== row.id)
      .map((item) => ({
        ...item,
        isHeadOffice: row.isHeadOffice && item.id !== row.id ? false : item.isHeadOffice,
      }));

    updated.push(row);
    return this.normalizeHeadOfficeOrder(updated);
  }

  private formatLocationName(location: LocationOption): string {
    return `${location.name}, ${location.city}, ${location.state}`;
  }

  private showMessage(text: string, tone: 'success' | 'danger'): void {
    this.message = text;
    this.messageTone = tone;
  }
}
