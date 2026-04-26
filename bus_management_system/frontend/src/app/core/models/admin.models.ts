export type ApprovalStatusLabel = 'Pending' | 'Approved' | 'Rejected' | 'Blocked';

export interface AdminRevenueSummaryResponse {
  totalRevenue: number;
  totalPlatformFee: number;
  totalOperatorEarning: number;
  totalTransactions: number;
}

export interface AdminOperatorRevenueResponse {
  operatorId: number;
  companyName: string;
  totalRevenue: number;
  totalPlatformFee: number;
  totalOperatorEarning: number;
  totalTransactions: number;
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

export interface OperatorDocumentResponse {
  id: number;
  documentType: number | string;
  fileUrl: string;
  uploadedAt: string;
  verifiedBy?: number | null;
  verifiedAt?: string | null;
}

export interface OperatorDetailResponse {
  id: number;
  userId: number;
  name: string;
  email: string;
  phone: string;
  companyName: string;
  licenseNumber: string;
  approvalStatus: number | string;
  approvedAt?: string | null;
  approvedBy?: number | null;
  rejectionReason?: string | null;
  blockedReason?: string | null;
  documents: OperatorDocumentResponse[];
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

export interface LocationResponse {
  id: number;
  name: string;
  city: string;
  state: string;
  createdAt?: string | null;
  usedInCount?: number;
}

export interface CreateLocationRequest {
  name: string;
  city: string;
  state: string;
}

export interface RouteStopResponse {
  id: number;
  locationId: number;
  locationName: string;
  stopOrder: number;
  distanceFromSource: number;
}

export interface RouteResponse {
  id: number;
  sourceId: number;
  sourceName: string;
  destinationId: number;
  destinationName: string;
  distanceKm: number;
  isActive: boolean;
  stops: RouteStopResponse[];
}

export interface CreateRouteRequest {
  sourceId: number;
  destinationId: number;
  distanceKm: number;
}

export interface RouteStopInput {
  locationId: number;
  stopOrder: number;
  distanceFromSource: number;
}

export interface AddRouteStopsRequest {
  stops: RouteStopInput[];
}

export interface AdminUserResponse {
  id: number;
  name: string;
  email: string;
  phone: string;
  role: string;
  isActive: boolean;
  createdAt?: string | null;
}

export interface PlatformFeeResponse {
  feeValue: number;
  isActive: boolean;
  updatedAt: string;
}

export interface SetPlatformFeeRequest {
  feeValue: number;
}

const approvalStatusMap: Record<number, ApprovalStatusLabel> = {
  0: 'Pending',
  1: 'Approved',
  2: 'Rejected',
  3: 'Blocked',
};

export function normalizeApprovalStatus(value: number | string | null | undefined): ApprovalStatusLabel {
  if (typeof value === 'number') {
    return approvalStatusMap[value] ?? 'Pending';
  }

  const normalized = String(value ?? '')
    .trim()
    .toLowerCase();

  switch (normalized) {
    case 'approved':
      return 'Approved';
    case 'rejected':
      return 'Rejected';
    case 'blocked':
      return 'Blocked';
    default:
      return 'Pending';
  }
}

export function statusBadgeClass(status: ApprovalStatusLabel): string {
  switch (status) {
    case 'Approved':
      return 'badge text-bg-success';
    case 'Rejected':
      return 'badge text-bg-danger';
    case 'Blocked':
      return 'badge text-bg-dark';
    default:
      return 'badge text-bg-warning';
  }
}

export function roleBadgeTone(role: string): 'success' | 'warning' | 'danger' | 'info' | 'neutral' {
  const normalized = role.trim().toLowerCase();
  if (normalized === 'admin') {
    return 'danger';
  }

  if (normalized === 'operator') {
    return 'warning';
  }

  if (normalized === 'user') {
    return 'info';
  }

  return 'neutral';
}
