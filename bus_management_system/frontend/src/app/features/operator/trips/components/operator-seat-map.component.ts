import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { TripSeatResponse } from '../../../../core/models/operator.models';

type SeatTone = 'available' | 'booked' | 'reserved' | 'locked' | 'inactive';

@Component({
  selector: 'app-operator-seat-map',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="seat-map-wrap">
      <div class="seat-map-legend">
        <span class="legend-item"><i class="dot available"></i> Available</span>
        <span class="legend-item"><i class="dot booked"></i> Booked</span>
        <span class="legend-item"><i class="dot reserved"></i> Reserved</span>
        <span class="legend-item"><i class="dot locked"></i> Locked</span>
      </div>

      <div class="seat-map-scroll">
        <section class="deck-block" *ngFor="let deck of deckOrder">
          <h4 class="h6 fw-bold mb-2" *ngIf="rowsByDeck(deck).length > 0">{{ deckLabel(deck) }}</h4>

          <div class="row-stack" *ngIf="rowsByDeck(deck).length > 0">
            <article class="row-strip" *ngFor="let row of rowsByDeck(deck)">
              <div class="row-label">R{{ row.row }}</div>
              <div class="seat-flow">
                <button type="button" class="seat-chip" [class]="seatClass(seat)" *ngFor="let seat of row.seats" disabled>
                  <span class="num">{{ seat.seatNumber }}</span>
                </button>
              </div>
            </article>
          </div>
        </section>
      </div>
    </div>
  `,
  styles: [
    `
      .seat-map-wrap {
        border: 1px solid #e2e8f0;
        border-radius: 6px;
        padding: 0.85rem;
        background: #fff;
      }

      .seat-map-legend {
        display: flex;
        flex-wrap: wrap;
        gap: 0.75rem;
        margin-bottom: 0.75rem;
      }

      .legend-item {
        display: inline-flex;
        align-items: center;
        gap: 0.35rem;
        font-size: 0.78rem;
        color: #475569;
      }

      .dot {
        width: 10px;
        height: 10px;
        border-radius: 999px;
        display: inline-block;
      }

      .dot.available {
        background: #22c55e;
      }

      .dot.booked {
        background: #ef4444;
      }

      .dot.reserved {
        background: #f59e0b;
      }

      .dot.locked {
        background: #64748b;
      }

      .seat-map-scroll {
        overflow: auto;
      }

      .deck-block + .deck-block {
        margin-top: 1rem;
      }

      .row-stack {
        display: grid;
        gap: 0.45rem;
      }

      .row-strip {
        display: grid;
        grid-template-columns: 44px minmax(560px, 1fr);
        gap: 0.5rem;
        align-items: center;
      }

      .row-label {
        font-size: 0.75rem;
        color: #64748b;
        font-weight: 700;
      }

      .seat-flow {
        display: flex;
        flex-wrap: wrap;
        gap: 0.35rem;
      }

      .seat-chip {
        border: 1px solid #e2e8f0;
        border-radius: 4px;
        background: #fff;
        color: #0f172a;
        font-size: 0.74rem;
        width: 68px;
        min-width: 68px;
        height: 34px;
      }

      .seat-chip.booked {
        background: #fee2e2;
        border-color: #fecaca;
      }

      .seat-chip.available {
        background: #dcfce7;
        border-color: #bbf7d0;
      }

      .seat-chip.reserved {
        background: #fef3c7;
        border-color: #fde68a;
      }

      .seat-chip.locked,
      .seat-chip.inactive {
        background: #e2e8f0;
        border-color: #cbd5e1;
      }

      @media (max-width: 768px) {
        .row-strip {
          grid-template-columns: 1fr;
        }

        .seat-flow {
          min-width: 0;
        }

        .seat-chip {
          width: 58px;
          min-width: 58px;
        }
      }
    `,
  ],
})
export class OperatorSeatMapComponent {
  @Input() seats: TripSeatResponse[] = [];
  @Input() bookedSeatNumbers: string[] = [];

  readonly deckOrder = ['Lower', 'Upper', 'Single'];

  deckLabel(deck: string): string {
    return deck === 'Single' ? 'Single Deck' : `${deck} Deck`;
  }

  rowsByDeck(deck: string): Array<{ row: number; seats: TripSeatResponse[] }> {
    const rows = new Map<number, TripSeatResponse[]>();

    this.seats
      .filter((seat) => this.normalizeDeck(seat.deck) === deck)
      .sort((a, b) => (a.row - b.row) || (a.columnNumber - b.columnNumber))
      .forEach((seat) => {
        const row = rows.get(seat.row) ?? [];
        row.push(seat);
        rows.set(seat.row, row);
      });

    return Array.from(rows.entries())
      .sort(([left], [right]) => left - right)
      .map(([row, rowSeats]) => ({ row, seats: rowSeats }));
  }

  seatClass(seat: TripSeatResponse): string {
    return `seat-chip ${this.seatTone(seat)}`;
  }

  private seatTone(seat: TripSeatResponse): SeatTone {
    if (this.bookedSeatNumbers.includes(seat.seatNumber)) {
      return 'booked';
    }

    const status = String(seat.status).trim().toLowerCase();

    if (status.includes('booked')) {
      return 'booked';
    }

    if (status.includes('reserved')) {
      return 'reserved';
    }

    if (status.includes('locked')) {
      return 'locked';
    }

    if (status.includes('inactive') || status.includes('disabled')) {
      return 'inactive';
    }

    return 'available';
  }

  private normalizeDeck(deck: number | string): string {
    const normalized = String(deck).trim().toLowerCase();

    if (normalized === '1' || normalized === 'upper') {
      return 'Upper';
    }

    if (normalized === '2' || normalized === 'single') {
      return 'Single';
    }

    return 'Lower';
  }
}
