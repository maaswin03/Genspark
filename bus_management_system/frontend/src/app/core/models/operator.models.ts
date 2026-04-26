export interface OperatorRevenueSummaryResponse {
  operatorId: number;
  companyName: string;
  totalRevenue: number;
  totalPlatformFee: number;
  totalOperatorEarning: number;
  totalTransactions: number;
}

export interface OperatorTripRevenueResponse {
  tripId: number;
  departureTime: string;
  arrivalTime: string;
  routeName: string;
  totalRevenue: number;
  totalPlatformFee: number;
  operatorEarning: number;
  totalBookings: number;
}

export interface OperatorTripResponse {
  tripId: number;
  scheduleId: number;
  busNumber: string;
  routeName: string;
  departureTime: string;
  arrivalTime: string;
  baseFare: number;
  status: number | string;
  availableSeats: number;
  bookedSeats: number;
}

export interface OperatorTripScheduleResponse {
  id: number;
  busId: number;
  routeId: number;
  operatorId: number;
  departureTime: string;
  arrivalTime: string;
  baseFare: number;
  daysOfWeek: string;
  validFrom: string;
  validUntil?: string | null;
  isActive: boolean;
}

export interface OperatorTripScheduleRequest {
  busId: number;
  routeId: number;
  departureTime: string;
  arrivalTime: string;
  baseFare: number;
  daysOfWeek: string;
  validFrom: string;
  validUntil?: string | null;
}

export interface ChangeBusRequest {
  newBusId: number;
  reason: string;
}

export interface CancelTripRequest {
  reason: string;
}

export interface SeatPricingResponse {
  seatId: number;
  seatNumber: string;
  price: number;
}

export interface SeatPricingRequest {
  seatId: number;
  price: number;
}

export interface SetSeatPricingRequest {
  prices: SeatPricingRequest[];
}

export interface TripSeatResponse {
  tripSeatId: number;
  seatId: number;
  seatNumber: string;
  row: number;
  columnNumber: number;
  deck: number | string;
  seatType: number | string;
  status: number | string;
  lockedUntil?: string | null;
  price: number;
}

export interface TripDetailResponse {
  tripId: number;
  operatorName: string;
  busNumber: string;
  busType: number | string;
  routeName: string;
  departureTime: string;
  arrivalTime: string;
  baseFare: number;
  availableSeats: number;
  status: number | string;
  routeId: number;
  sourceName: string;
  destinationName: string;
  seats: TripSeatResponse[];
}

export interface OperatorRouteOption {
  id: number;
  sourceName: string;
  destinationName: string;
  isActive: boolean;
}

export interface OperatorProfileResponse {
  id: number;
  userId: number;
  name: string;
  email: string;
  phone: string;
  companyName: string;
  licenseNumber: string;
  approvalStatus: number | string;
  createdAt: string;
}

export interface UpdateOperatorProfileRequest {
  name: string;
  phone: string;
  companyName: string;
}

export interface ChangeOperatorPasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface OperatorDocumentResponse {
  id: number;
  documentType: number | string;
  fileUrl: string;
  uploadedAt: string;
  verifiedBy?: number | null;
  verifiedAt?: string | null;
}

export interface UploadOperatorDocumentRequest {
  documentType: number;
  fileUrl: string;
}

export interface OperatorOfficeResponse {
  id: number;
  locationId: number;
  locationName: string;
  address: string;
  isHeadOffice: boolean;
}

export interface CreateOperatorOfficeRequest {
  locationId: number;
  address: string;
  isHeadOffice: boolean;
}

export interface UpdateOperatorOfficeRequest {
  address: string;
  isHeadOffice: boolean;
}

export interface OperatorBookingSummaryResponse {
  bookingId: number;
  tripId: number;
  userName: string;
  userEmail: string;
  routeName: string;
  departureTime: string;
  totalAmount: number;
  bookingStatus: number | string;
  seatCount: number;
}

export interface OperatorBookingSeatResponse {
  bookingSeatId: number;
  seatId: number;
  seatNumber: string;
  amountPaid: number;
  status: number | string;
  passengerName?: string | null;
  passengerAge?: number | null;
  passengerGender?: number | string | null;
}

export interface OperatorBookingDetailResponse {
  bookingId: number;
  tripId: number;
  userName: string;
  userEmail: string;
  userPhone: string;
  routeName: string;
  bookingDate: string;
  departureTime: string;
  arrivalTime: string;
  totalAmount: number;
  platformFee: number;
  bookingStatus: number | string;
  cancelledAt?: string | null;
  cancelReason?: string | null;
  seats: OperatorBookingSeatResponse[];
}

export interface PassengerManifestRow {
  seatNumber: string;
  passengerName: string;
  age?: number | null;
  gender?: number | string | null;
  bookingStatus: number | string;
}

export interface BusSeatResponse {
  id: number;
  busId: number;
  seatNumber: string;
  row: number;
  columnNumber: number;
  deck: number | string;
  seatType: number | string;
  isActive: boolean;
}

export interface BusResponse {
  id: number;
  operatorId: number;
  operatorName: string;
  busNumber: string;
  busType: number | string;
  totalSeats: number;
  isActive: boolean;
  createdAt: string;
}

export interface BusDetailResponse extends BusResponse {
  seats: BusSeatResponse[];
}

export interface CreateBusRequest {
  busNumber: string;
  busType: number | string;
  totalSeats: number;
  seats?: SeatLayoutRequest[];
}

export interface UpdateBusRequest extends CreateBusRequest {}

export interface SeatLayoutRequest {
  seatNumber: string;
  row: number;
  columnNumber: number;
  deck: number | string;
  seatType: number | string;
  isActive: boolean;
}

export type SeatDeck = 'Lower' | 'Upper' | 'Single';
export type SeatSide = 'Left' | 'Right';
export type SeatKind = 'Window' | 'Aisle' | 'Middle';

export interface SeatBlueprintCell {
  seatNumber: string;
  row: number;
  side: SeatSide;
  sideIndex: number;
  deck: SeatDeck;
  displayType: string;
  isActive: boolean;
}

export interface SeatBlueprintSection {
  deck: SeatDeck;
  seats: SeatBlueprintCell[];
}
