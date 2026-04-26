export type NotificationChannelValue = number | string;
export type ReferenceTypeValue = number | string;

export interface UserNotificationResponse {
  id: number;
  title: string;
  message: string;
  type: string;
  referenceType: ReferenceTypeValue | null;
  referenceId: number | null;
  channel: NotificationChannelValue;
  isRead: boolean;
  createdAt: string;
}

export interface UserProfileResponse {
  id: number;
  name: string;
  email: string;
  phone: string;
  role: string;
}

export interface UpdateUserProfileRequest {
  name: string;
  phone: string;
}

export interface ChangeUserPasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
