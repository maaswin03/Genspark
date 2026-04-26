import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TripSeatResponse } from '../../../core/models/trip-booking.models';

type SeatDeck = 'lower' | 'upper';

@Component({
  selector: 'app-seat-map',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './seat-map.component.html',
  styleUrl: './seat-map.component.css',
})
export class SeatMapComponent {
  @Input() seats: TripSeatResponse[] = [];
  @Input() selectedTripSeatIds: number[] = [];
  @Input() activeDeck: SeatDeck = 'lower';
  @Input() disabled = false;
  @Input() isSleeper = false;

  @Output() deckChange = new EventEmitter<SeatDeck>();
  @Output() seatToggle = new EventEmitter<TripSeatResponse>();

  readonly deckOptions: Array<{ label: string; value: SeatDeck }> = [
    { label: 'Lower Deck', value: 'lower' },
    { label: 'Upper Deck', value: 'upper' },
  ];

  changeDeck(deck: SeatDeck): void {
    this.deckChange.emit(deck);
  }

  toggleSeat(seat: TripSeatResponse): void {
    if (this.disabled || !this.canSelect(seat)) return;
    this.seatToggle.emit(seat);
  }

  /** Only show deck tabs when the bus has both lower & upper seats */
  get hasBothDecks(): boolean {
    return this.seats.some(s => this.getSeatDeck(s) === 'upper');
  }

  get visibleSeats(): TripSeatResponse[] {
    return this.seats.filter((seat) => this.getSeatDeck(seat) === this.activeDeck);
  }

  get maxColumns(): number {
    return Math.max(2, ...this.visibleSeats.map((seat) => seat.columnNumber || 0));
  }

  get rows(): number[] {
    return Array.from(new Set(this.visibleSeats.map((seat) => seat.row))).sort((a, b) => a - b);
  }

  get columnIndexes(): number[] {
    return Array.from({ length: this.maxColumns }, (_, i) => i + 1);
  }

  seatAt(row: number, col: number): TripSeatResponse | null {
    return this.visibleSeats.find((s) => s.row === row && s.columnNumber === col) ?? null;
  }

  isSelected(seat: TripSeatResponse): boolean {
    return this.selectedTripSeatIds.includes(seat.tripSeatId);
  }

  canSelect(seat: TripSeatResponse): boolean {
    const status = this.normalize(seat.status);
    return status === 'available' || status === '0';
  }

  seatClass(seat: TripSeatResponse): Record<string, boolean> {
    const selected = this.isSelected(seat);
    const status = this.normalize(seat.status);
    // SeatStatus enum: Available=0, Reserved=1, Booked=2
    const available = status === 'available' || status === '0';
    const reserved  = status === 'reserved'  || status === '1';
    const booked    = status === 'booked'    || status === '2';
    return {
      'seat-available': !selected && available,
      'seat-selected':  selected,
      'seat-booked':    !selected && (booked || (!available && !reserved)),
      'seat-reserved':  !selected && reserved,
      'seat-berth':     this.isSleeper,
    };
  }

  formatPrice(seat: TripSeatResponse): string {
    return '₹' + Math.round(seat.price ?? 0).toLocaleString('en-IN');
  }

  private getSeatDeck(seat: TripSeatResponse): SeatDeck {
    const deck = this.normalize(seat.deck);
    return deck === 'upper' || deck === '1' ? 'upper' : 'lower';
  }

  private normalize(value: unknown): string {
    return String(value ?? '').trim().toLowerCase();
  }
}
