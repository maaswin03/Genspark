import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { AdminBadgeComponent } from '../../admin/shared/admin-badge.component';
import { AdminModalComponent } from '../../admin/shared/admin-modal.component';
import {
  BusDetailResponse,
  BusResponse,
  BusSeatResponse,
  CreateBusRequest,
  SeatLayoutRequest,
  SeatBlueprintCell,
  SeatDeck,
  SeatKind,
  UpdateBusRequest,
} from '../../../core/models/operator.models';
import { OperatorService } from '../../../core/services/operator.service';

type BusWizardMode = 'create' | 'edit';
type WizardStep = 1 | 2 | 3;

type BusTypeOption = {
  value: number;
  label: string;
};

type BusLayoutPreset = {
  rowsPerDeck: number;
  leftCount: number;
  rightCount: number;
  decks: SeatDeck[];
  seatDisplayType: string;
  notes: string;
};

type SeaterPattern = '2+3' | '2+2' | '1+2';

@Component({
  selector: 'app-operator-buses',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AdminModalComponent, AdminBadgeComponent],
  templateUrl: './operator-buses.component.html',
  styleUrl: './operator-buses.component.css',
})
export class OperatorBusesComponent implements OnInit {
  private readonly busNumberPattern = /^[A-Z]{2}\s\d{2}\s[A-Z]{2}\s\d{4}$/;

  isLoading = true;
  isSaving = false;
  isBusDetailLoading = false;
  message = '';
  messageTone: 'success' | 'danger' = 'success';

  buses: BusResponse[] = [];
  wizardOpen = false;
  wizardMode: BusWizardMode = 'create';
  wizardStep: WizardStep = 1;
  editingBusId: number | null = null;
  deactivateTarget: BusResponse | null = null;
  layoutBus: BusDetailResponse | null = null;
  layoutSeatLayout: SeatBlueprintCell[] = [];
  layoutOpen = false;
  selectedLayoutDeck: SeatDeck = 'Lower';

  readonly busTypeOptions: BusTypeOption[] = [
    { value: 0, label: 'AC Sleeper' },
    { value: 1, label: 'Non AC Sleeper' },
    { value: 2, label: 'AC Seater' },
    { value: 3, label: 'Non AC Seater' },
    { value: 4, label: 'AC Semi Sleeper' },
    { value: 5, label: 'Non AC Semi Sleeper' },
  ];

  readonly seatTypeOptions: SeatKind[] = ['Window', 'Aisle', 'Middle'];
  readonly deckRenderOrder: SeatDeck[] = ['Lower', 'Upper', 'Single'];

  readonly seaterPatternOptions: Array<{ value: SeaterPattern; label: string; leftCount: number; rightCount: number }> = [
    { value: '2+3', label: '2 + 3', leftCount: 2, rightCount: 3 },
    { value: '2+2', label: '2 + 2', leftCount: 2, rightCount: 2 },
    { value: '1+2', label: '1 + 2', leftCount: 1, rightCount: 2 },
  ];

  readonly layoutPresets: Record<number, BusLayoutPreset> = {
    0: { rowsPerDeck: 4, leftCount: 1, rightCount: 2, decks: ['Lower', 'Upper'], seatDisplayType: 'Berth', notes: 'Sleeper: 1 berth on the left, 2 berths on the right for each deck.' },
    1: { rowsPerDeck: 4, leftCount: 1, rightCount: 2, decks: ['Lower', 'Upper'], seatDisplayType: 'Berth', notes: 'Sleeper: 1 berth on the left, 2 berths on the right for each deck.' },
    2: { rowsPerDeck: 4, leftCount: 2, rightCount: 3, decks: ['Single'], seatDisplayType: 'Seat', notes: 'Seater: supports 2 + 3, 2 + 2, and 1 + 2 configurations.' },
    3: { rowsPerDeck: 4, leftCount: 2, rightCount: 3, decks: ['Single'], seatDisplayType: 'Seat', notes: 'Seater: supports 2 + 3, 2 + 2, and 1 + 2 configurations.' },
    4: { rowsPerDeck: 4, leftCount: 1, rightCount: 2, decks: ['Lower', 'Upper'], seatDisplayType: 'Berth', notes: 'Semi-sleeper: upper deck uses berths, lower deck uses seats.' },
    5: { rowsPerDeck: 4, leftCount: 1, rightCount: 2, decks: ['Lower', 'Upper'], seatDisplayType: 'Berth', notes: 'Semi-sleeper: upper deck uses berths, lower deck uses seats.' },
  };

  readonly busForm;

  seatLayout: SeatBlueprintCell[] = [];

  constructor(
    private readonly operatorService: OperatorService,
    private readonly fb: FormBuilder,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.busForm = this.fb.group({
      busNumber: ['', [Validators.required, Validators.maxLength(50), Validators.pattern(this.busNumberPattern)]],
      busType: [0, [Validators.required]],
      seaterPattern: ['2+3' as SeaterPattern, [Validators.required]],
      rowsPerDeck: [4, [Validators.required, Validators.min(1), Validators.max(20)]],
    });
  }

  ngOnInit(): void {
    this.loadBuses();
  }

  loadBuses(): void {
    this.isLoading = true;
    this.message = '';

    this.operatorService
      .getBuses()
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isLoading = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: (response) => {
          this.zone.run(() => {
            this.buses = response;
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to load buses right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  openCreateBus(): void {
    this.wizardMode = 'create';
    this.editingBusId = null;
    this.wizardStep = 1;
    this.seatLayout = [];
    this.layoutBus = null;
    this.busForm.reset({
      busNumber: '',
      busType: 0,
      seaterPattern: '2+3',
      rowsPerDeck: this.getLayoutPreset(0, '2+3').rowsPerDeck,
    });
    this.wizardOpen = true;
  }

  openEditBus(bus: BusResponse): void {
    this.isBusDetailLoading = true;
    this.operatorService
      .getBusDetail(bus.id)
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isBusDetailLoading = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: (response) => {
          this.zone.run(() => {
            this.wizardMode = 'edit';
            this.editingBusId = bus.id;
            this.wizardStep = 1;
            const busType = this.normalizeBusTypeValue(response.busType);
            this.busForm.reset({
              busNumber: response.busNumber,
              busType,
              seaterPattern: this.inferSeaterPattern(response.busType, response.seats),
              rowsPerDeck: Math.max(1, this.estimateRowsPerDeck(response.seats, busType)),
            });
            this.seatLayout = this.mapSeatsToBlueprint(response.seats, response.busType);
            this.wizardOpen = true;
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to load bus details right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  closeWizard(): void {
    if (this.isSaving) {
      return;
    }

    this.wizardOpen = false;
  }

  nextStep(): void {
    if (this.wizardStep === 1) {
      this.formatBusNumber();
      this.busForm.controls.busNumber.markAsTouched();
      this.busForm.controls.busType.markAsTouched();

      if (this.busForm.controls.busNumber.invalid || this.busForm.controls.busType.invalid) {
        return;
      }

      if (this.isDuplicateBusNumber()) {
        this.message = 'Bus number already exists.';
        this.messageTone = 'danger';
        return;
      }

      this.generateSeatLayout();
      this.wizardStep = 2;
      return;
    }

    if (this.wizardStep === 2) {
      if (this.seatLayout.length === 0) {
        this.generateSeatLayout();
      }

      this.wizardStep = 3;
    }
  }

  previousStep(): void {
    if (this.wizardStep > 1) {
      this.wizardStep = (this.wizardStep - 1) as WizardStep;
    }
  }

  regenerateLayout(): void {
    this.generateSeatLayout();
  }

  submitBus(): void {
    this.formatBusNumber();

    if (this.busForm.invalid) {
      this.busForm.markAllAsTouched();
      return;
    }

    if (this.isDuplicateBusNumber()) {
      this.message = 'Bus number already exists.';
      this.messageTone = 'danger';
      return;
    }

    if (this.seatLayout.length === 0) {
      this.generateSeatLayout();
    }

    const payload: CreateBusRequest = {
      busNumber: this.normalizedBusNumber(),
      busType: Number(this.busForm.controls.busType.value ?? 0),
      totalSeats: this.seatLayout.length,
      seats: this.toSeatLayoutRequest(),
    };

    const request$ = this.wizardMode === 'create'
      ? this.operatorService.createBus(payload)
      : this.operatorService.updateBus(this.editingBusId ?? 0, payload as UpdateBusRequest);

    this.isSaving = true;
    this.message = '';

    request$
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
            this.message = this.wizardMode === 'create' ? 'Bus created successfully.' : 'Bus updated successfully.';
            this.messageTone = 'success';
            this.wizardOpen = false;
            this.loadBuses();
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to save bus right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  openDeactivate(bus: BusResponse): void {
    this.deactivateTarget = bus;
  }

  closeDeactivate(): void {
    if (this.isSaving) {
      return;
    }

    this.deactivateTarget = null;
  }

  confirmDeactivate(): void {
    if (!this.deactivateTarget) {
      return;
    }

    this.isSaving = true;
    this.message = '';

    this.operatorService
      .deactivateBus(this.deactivateTarget.id)
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
            this.message = 'Bus deactivated successfully.';
            this.messageTone = 'success';
            this.deactivateTarget = null;
            this.loadBuses();
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to deactivate bus right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  viewSeatLayout(bus: BusResponse): void {
    this.isBusDetailLoading = true;
    this.operatorService
      .getBusDetail(bus.id)
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isBusDetailLoading = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: (response) => {
          this.zone.run(() => {
            this.layoutBus = response;
            this.layoutSeatLayout = this.mapSeatsToBlueprint(response.seats, response.busType);
            this.selectedLayoutDeck = this.availableDecks(response.seats)[0] ?? 'Single';
            this.layoutOpen = true;
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to load seat layout right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  closeLayout(): void {
    this.layoutOpen = false;
    this.layoutSeatLayout = [];
  }

  formatBusNumber(): void {
    const normalized = this.normalizedBusNumber();
    this.busForm.controls.busNumber.setValue(normalized, { emitEvent: false });
  }

  getActiveTone(bus: BusResponse): 'success' | 'dark' {
    return bus.isActive ? 'success' : 'dark';
  }

  getBusTypeLabel(value: number | string): string {
    return this.busTypeOptions.find((option) => option.value === Number(value))?.label ?? 'Bus';
  }

  getSeatTypeLabel(value: number | string): SeatKind {
    // Handle integer enum values returned by the backend (SeatType: 0=Window, 1=Aisle, 2=Middle)
    const num = Number(value);
    if (!isNaN(num) && String(value).trim() !== '') {
      if (num === 1) return 'Aisle';
      if (num === 2) return 'Middle';
      if (num === 0) return 'Window';
    }

    // Fallback: handle string names
    const normalized = String(value).trim().toLowerCase();
    if (normalized === 'aisle') return 'Aisle';
    if (normalized === 'middle') return 'Middle';
    return 'Window';
  }

  getDeckLabel(value: number | string): SeatDeck {
    // Handle integer enum values returned by the backend (DeckType: 0=Lower, 1=Upper, 2=Single)
    const num = Number(value);
    if (!isNaN(num) && String(value).trim() !== '') {
      if (num === 1) return 'Upper';
      if (num === 2) return 'Single';
      if (num === 0) return 'Lower';
    }

    // Fallback: handle string names
    const normalized = String(value).trim().toLowerCase();
    if (normalized === 'upper') return 'Upper';
    if (normalized === 'single') return 'Single';
    return 'Lower';
  }

  getSeatSummary(): { deck: SeatDeck; count: number }[] {
    return this.deckRenderOrder.map((deck) => ({
      deck,
      count: this.seatLayout.filter((seat) => seat.deck === deck).length,
    }));
  }

  getSeatDeckLabel(value: number | string): SeatDeck {
    return this.getDeckLabel(value);
  }

  get selectedLayoutPreset(): BusLayoutPreset {
    return this.getLayoutPreset(Number(this.busForm.controls.busType.value ?? 0), String(this.busForm.controls.seaterPattern.value ?? '2+3') as SeaterPattern);
  }

  deckLabel(deck: SeatDeck): string {
    return deck === 'Single' ? 'Single Deck' : `${deck} Deck`;
  }

  getLayoutNote(): string {
    return this.selectedLayoutPreset.notes;
  }

  get isSeaterBus(): boolean {
    const busType = Number(this.busForm.controls.busType.value ?? 0);
    return busType === 2 || busType === 3;
  }

  layoutDecks(): SeatDeck[] {
    if (!this.layoutSeatLayout.length) {
      return [];
    }

    const decks = new Set<SeatDeck>();
    this.layoutSeatLayout.forEach((seat) => decks.add(seat.deck));
    return Array.from(decks);
  }

  getLayoutDeckRows(deck: SeatDeck): Array<{ row: number; left: SeatBlueprintCell[]; right: SeatBlueprintCell[] }> {
    return this.getDeckRowsFromSource(this.layoutSeatLayout, deck);
  }

  availableDecks(seats: BusSeatResponse[]): SeatDeck[] {
    const decks = new Set<SeatDeck>();
    seats.forEach((seat) => decks.add(this.getDeckLabel(seat.deck)));
    return Array.from(decks);
  }

  trackByLayoutSeat(_: number, seat: BusSeatResponse): string {
    return `${seat.row}-${seat.columnNumber}-${seat.seatNumber}`;
  }

  getDeckRows(deck: SeatDeck): Array<{ row: number; left: SeatBlueprintCell[]; right: SeatBlueprintCell[] }> {
    return this.getDeckRowsFromSource(this.seatLayout, deck);
  }

  private getDeckRowsFromSource(source: SeatBlueprintCell[], deck: SeatDeck): Array<{ row: number; left: SeatBlueprintCell[]; right: SeatBlueprintCell[] }> {
    const seats = source.filter((seat) => seat.deck === deck);
    const rows = new Map<number, { row: number; left: SeatBlueprintCell[]; right: SeatBlueprintCell[] }>();

    seats.forEach((seat) => {
      const existing = rows.get(seat.row) ?? { row: seat.row, left: [], right: [] };
      existing[seat.side.toLowerCase() as 'left' | 'right'].push(seat);
      rows.set(seat.row, existing);
    });

    return Array.from(rows.values()).sort((a, b) => a.row - b.row).map((row) => ({
      row: row.row,
      left: row.left.sort((a, b) => a.sideIndex - b.sideIndex),
      right: row.right.sort((a, b) => a.sideIndex - b.sideIndex),
    }));
  }

  getDeckColumnClass(deck: SeatDeck): string {
    if (this.selectedLayoutPreset.decks.length > 1 && deck !== 'Single') {
      return 'col-12 col-xl-6';
    }

    return 'col-12';
  }

  getSeatTileClass(displayType: string): string {
    return displayType === 'Berth' ? 'seat-tile berth' : 'seat-tile seat';
  }

  trackBySeatNumber(_: number, seat: SeatBlueprintCell): string {
    return seat.seatNumber;
  }

  trackByDeckRow(_: number, row: { row: number }): number {
    return row.row;
  }

  private generateSeatLayout(): void {
    const layout: SeatBlueprintCell[] = [];
    const busType = Number(this.busForm.controls.busType.value ?? 0);
    const pattern = String(this.busForm.controls.seaterPattern.value ?? '2+3') as SeaterPattern;
    const preset = this.getLayoutPreset(busType, pattern);
    const rowsPerDeck = Math.max(1, Number(this.busForm.controls.rowsPerDeck.value ?? preset.rowsPerDeck));
    const leftCount = preset.leftCount;
    const rightCount = preset.rightCount;

    preset.decks.forEach((deck) => {
      const rowLimit = this.getDeckRowLimit(busType, deck, rowsPerDeck);

      for (let row = 1; row <= rowLimit; row += 1) {
        for (let sideIndex = 1; sideIndex <= leftCount; sideIndex += 1) {
          layout.push({
            seatNumber: `${deck[0]}${row}-L${sideIndex}`,
            row,
            side: 'Left',
            sideIndex,
            deck,
            displayType: this.getDisplayType(deck, busType),
            isActive: true,
          });
        }

        for (let sideIndex = 1; sideIndex <= rightCount; sideIndex += 1) {
          layout.push({
            seatNumber: `${deck[0]}${row}-R${sideIndex}`,
            row,
            side: 'Right',
            sideIndex,
            deck,
            displayType: this.getDisplayType(deck, busType),
            isActive: true,
          });
        }
      }
    });

    this.seatLayout = layout;
  }

  private getLayoutPreset(busType: number, pattern: SeaterPattern): BusLayoutPreset {
    const base = this.layoutPresets[busType] ?? this.layoutPresets[0];

    if (busType === 2 || busType === 3) {
      const selected = this.seaterPatternOptions.find((option) => option.value === pattern) ?? this.seaterPatternOptions[0];

      return {
        ...base,
        leftCount: selected.leftCount,
        rightCount: selected.rightCount,
      };
    }

    return base;
  }

  private getDisplayType(deck: SeatDeck, busType: number): string {
    if (busType === 0 || busType === 1) {
      return 'Berth';
    }

    if (busType === 4 || busType === 5) {
      return deck === 'Upper' ? 'Berth' : 'Seat';
    }

    return 'Seat';
  }

  private getDeckRowLimit(busType: number, deck: SeatDeck, rowsPerDeck: number): number {
    if ((busType === 4 || busType === 5) && deck === 'Lower') {
      // Semi-sleeper mapping: one upper berth row corresponds to two lower seat rows.
      return rowsPerDeck * 2;
    }

    return rowsPerDeck;
  }

  private inferSeaterPattern(busType: number | string, seats: BusSeatResponse[]): SeaterPattern {
    const normalized = Number(busType);
    if (normalized !== 2 && normalized !== 3) {
      return '2+3';
    }

    const rowCounts = seats.reduce((accumulator, seat) => {
      if (seat.deck === 'Single' || String(seat.deck).toLowerCase() === 'single') {
        accumulator.add(seat.row);
      }

      return accumulator;
    }, new Set<number>());

    const totalPerRow = seats.length / Math.max(rowCounts.size || 1, 1);
    if (totalPerRow <= 3) {
      return '1+2';
    }

    if (totalPerRow <= 4) {
      return '2+2';
    }

    return '2+3';
  }

  getSeatTotalsByDeck(): { deck: SeatDeck; total: number }[] {
    return this.selectedLayoutPreset.decks.map((deck) => ({
      deck,
      total: this.getDeckRows(deck).length * (this.selectedLayoutPreset.leftCount + this.selectedLayoutPreset.rightCount),
    }));
  }

  get rowsPerDeckValue(): number {
    return Number(this.busForm.controls.rowsPerDeck.value ?? this.selectedLayoutPreset.rowsPerDeck);
  }

  get isDeckManagedBus(): boolean {
    return Number(this.busForm.controls.busType.value ?? 0) !== 2 && Number(this.busForm.controls.busType.value ?? 0) !== 3;
  }

  private estimateRowsPerDeck(seats: BusSeatResponse[], busType: number): number {
    const rowCountByDeck = new Map<string, Set<number>>();

    seats.forEach((seat) => {
      const deck = this.getDeckLabel(seat.deck);
      const key = rowCountByDeck.get(deck) ?? new Set<number>();
      key.add(seat.row);
      rowCountByDeck.set(deck, key);
    });

    if (busType === 4 || busType === 5) {
      const upperRows = rowCountByDeck.get('Upper')?.size ?? 0;
      if (upperRows > 0) {
        return upperRows;
      }

      const lowerRows = rowCountByDeck.get('Lower')?.size ?? 0;
      if (lowerRows > 0) {
        return Math.max(1, Math.ceil(lowerRows / 2));
      }
    }

    const firstDeck = Array.from(rowCountByDeck.values())[0];
    return firstDeck ? firstDeck.size : 4;
  }

  private mapSeatsToBlueprint(seats: BusSeatResponse[], busType: number | string): SeatBlueprintCell[] {
    const grouped = new Map<string, BusSeatResponse[]>();

    seats.forEach((seat) => {
      const deck = this.getDeckLabel(seat.deck);
      const key = `${deck}-${seat.row}`;
      const existing = grouped.get(key) ?? [];
      existing.push(seat);
      grouped.set(key, existing);
    });

    const blueprint: SeatBlueprintCell[] = [];
    grouped.forEach((rowSeats, key) => {
      const [deckRaw] = key.split('-');
      const deck = deckRaw as SeatDeck;
      const sorted = [...rowSeats].sort((a, b) => a.columnNumber - b.columnNumber);
      const splitIndex = this.findSplitIndex(sorted.map((seat) => seat.columnNumber));

      const leftSeats = sorted.filter((_, index) => index <= splitIndex);
      const rightSeats = sorted.filter((_, index) => index > splitIndex);

      leftSeats.forEach((seat, index) => {
        blueprint.push({
          seatNumber: seat.seatNumber,
          row: seat.row,
          side: 'Left',
          sideIndex: index + 1,
          deck,
          displayType: this.getDisplayType(deck, Number(busType)),
          isActive: seat.isActive,
        });
      });

      rightSeats.forEach((seat, index) => {
        blueprint.push({
          seatNumber: seat.seatNumber,
          row: seat.row,
          side: 'Right',
          sideIndex: index + 1,
          deck,
          displayType: this.getDisplayType(deck, Number(busType)),
          isActive: seat.isActive,
        });
      });
    });

    return blueprint.sort((a, b) => {
      if (a.deck !== b.deck) {
        return this.deckRenderOrder.indexOf(a.deck) - this.deckRenderOrder.indexOf(b.deck);
      }

      if (a.row !== b.row) {
        return a.row - b.row;
      }

      if (a.side !== b.side) {
        return a.side === 'Left' ? -1 : 1;
      }

      return a.sideIndex - b.sideIndex;
    });
  }

  private findSplitIndex(columns: number[]): number {
    if (columns.length <= 1) {
      return 0;
    }

    let largestGap = 0;
    let splitIndex = Math.ceil(columns.length / 2) - 1;

    for (let index = 0; index < columns.length - 1; index += 1) {
      const gap = columns[index + 1] - columns[index];
      if (gap > largestGap) {
        largestGap = gap;
        splitIndex = index;
      }
    }

    return splitIndex;
  }

  private normalizedBusNumber(): string {
    const raw = String(this.busForm.controls.busNumber.value ?? '');
    const compact = raw.toUpperCase().replace(/[^A-Z0-9]/g, '');
    if (compact.length !== 10) {
      return raw.trim().toUpperCase().replace(/\s+/g, ' ');
    }

    return `${compact.slice(0, 2)} ${compact.slice(2, 4)} ${compact.slice(4, 6)} ${compact.slice(6, 10)}`;
  }

  private isDuplicateBusNumber(): boolean {
    const candidate = this.normalizedBusNumber().replace(/\s+/g, '').toUpperCase();
    if (!candidate) {
      return false;
    }

    return this.buses.some((bus) => {
      if (this.wizardMode === 'edit' && bus.id === this.editingBusId) {
        return false;
      }

      return bus.busNumber.replace(/\s+/g, '').toUpperCase() === candidate;
    });
  }

  private toSeatLayoutRequest(): SeatLayoutRequest[] {
    const rows = new Map<string, { leftCount: number }>();

    this.seatLayout.forEach((seat) => {
      const key = `${seat.deck}-${seat.row}`;
      const entry = rows.get(key) ?? { leftCount: 0 };
      if (seat.side === 'Left') {
        entry.leftCount = Math.max(entry.leftCount, seat.sideIndex);
      }

      rows.set(key, entry);
    });

    return this.seatLayout.map((seat) => {
      const key = `${seat.deck}-${seat.row}`;
      const leftCount = rows.get(key)?.leftCount ?? 0;
      const columnNumber = seat.side === 'Left' ? seat.sideIndex : leftCount + 1 + seat.sideIndex;

      return {
        seatNumber: seat.seatNumber,
        row: seat.row,
        columnNumber,
        deck: this.deckToInt(seat.deck),
        seatType: this.seatKindToInt(this.resolveSeatType(seat)),
        isActive: seat.isActive,
      };
    });
  }

  private deckToInt(deck: SeatDeck): number {
    const map: Record<SeatDeck, number> = { Lower: 0, Upper: 1, Single: 2 };
    return map[deck] ?? 0;
  }

  private seatKindToInt(kind: SeatKind): number {
    const map: Record<SeatKind, number> = { Window: 0, Aisle: 1, Middle: 2 };
    return map[kind] ?? 0;
  }

  private resolveSeatType(seat: SeatBlueprintCell): SeatKind {
    if (seat.side === 'Left') {
      return seat.sideIndex === 1 ? 'Window' : 'Aisle';
    }

    const maxRight = this.seatLayout
      .filter((item) => item.deck === seat.deck && item.row === seat.row && item.side === 'Right')
      .reduce((max, item) => Math.max(max, item.sideIndex), 1);

    if (seat.sideIndex === 1) {
      return 'Aisle';
    }

    if (seat.sideIndex === maxRight) {
      return 'Window';
    }

    return 'Middle';
  }

  private normalizeBusTypeValue(value: number | string): number {
    return Number(value ?? 0);
  }
}
