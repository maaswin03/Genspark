export type BusTypeValue = number | string;
export type TripStatusValue = number | string;
export type SeatStatusValue = number | string;
export type DeckValue = number | string;
export type SeatTypeValue = number | string;


export interface InitiatePaymentResponse {
  paymentId: number;
  bookingId: number;
  paymentStatus: string;
  gatewayPayload: unknown;
}

export interface TripSearchRequest {
  sourceId: number;
  destinationId: number;
  travelDate: string;
  busType?: BusTypeValue | null;
}

export interface TripSearchResponse {
  tripId: number;
  operatorName: string;
  busNumber: string;
  busType: BusTypeValue;
  routeName: string;
  departureTime: string;
  arrivalTime: string;
  baseFare: number;
  availableSeats: number;
  status: TripStatusValue;
}

export interface TripSeatResponse {
  tripSeatId: number;
  seatId: number;
  seatNumber: string;
  row: number;
  columnNumber: number;
  deck: DeckValue;
  seatType: SeatTypeValue;
  status: SeatStatusValue;
  lockedUntil: string | null;
  price: number;
}

export interface TripDetailResponse extends TripSearchResponse {
  routeId: number;
  sourceName: string;
  destinationName: string;
  seats: TripSeatResponse[];
}

export interface TripPointResponse {
  id: number;
  locationId: number;
  locationName: string;
  name: string;
  timeOffset: number;
  isDefault: boolean;
}

export interface TripPointsResponse {
  boardingPoints: TripPointResponse[];
  droppingPoints: TripPointResponse[];
}

export interface ReserveSeatRequest {
  tripId: number;
  seatIds: number[];
}

export interface ReserveSeatResponse {
  tripId: number;
  lockedUntil: string;
  reservedTripSeatIds: number[];
}

export interface BookingPassengerRequest {
  tripSeatId: number;
  name: string;
  age: number;
  gender: 'male' | 'female' | 'other';
}

export interface CreateBookingRequest {
  tripId: number;
  boardingPointId: number;
  droppingPointId: number;
  passengers: BookingPassengerRequest[];
}

export interface BookingResponse {
  bookingId: number;
  tripDetails: string;
  totalAmount: number;
  status: number | string;
  bookingDate: string;
}

export interface InitiatePaymentRequest {
  bookingId: number;
  paymentMethod: 'upi' | 'card' | 'netbanking' | 'wallet';
}

export interface PaymentStatusResponse {
  bookingId: number;
  paymentId: number | null;
  paymentStatus: number | string;
  bookingStatus: number | string;
  amount: number;
  transactionId: string | null;
  updatedAt: string;
}

export interface UserBookingResponse {
  bookingId: number;
  tripId: number;
  routeName: string;
  departureTime: string;
  arrivalTime: string;
  totalAmount: number;
  status: number | string;
  bookingDate: string;
  seatCount: number;
}

export interface UserBookingSeatResponse {
  bookingSeatId: number;
  seatId: number;
  seatNumber: string;
  amountPaid: number;
  status: number | string;
  passengerName: string | null;
  passengerAge: number | null;
  passengerGender: number | string | null;
}

export interface UserBookingDetailResponse {
  bookingId: number;
  tripId: number;
  routeName: string;
  departureTime: string;
  arrivalTime: string;
  totalAmount: number;
  platformFee: number;
  status: number | string;
  bookingDate: string;
  cancelledAt: string | null;
  cancelReason: string | null;
  paymentStatus: number | string;
  seats: UserBookingSeatResponse[];
}

export interface CancelBookingRequest {
  cancelReason: string;
}

export interface CancelSeatResponse {
  bookingId: number;
  bookingSeatId: number;
  bookingStatus: number | string;
  updatedTotalAmount: number;
}

export interface BookingFlowReservation {
  tripId: number;
  lockedUntil: string;
  reservedTripSeatIds: number[];
}

export interface BookingFlowState {
  tripId: number | null;
  trip: TripDetailResponse | null;
  tripPoints: TripPointsResponse | null;
  selectedTripSeatIds: number[];
  boardingPointId: number | null;
  droppingPointId: number | null;
  reservation: BookingFlowReservation | null;
}


