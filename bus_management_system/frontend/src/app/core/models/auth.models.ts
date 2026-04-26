export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  phone: string;
  password: string;
}

export interface OperatorRegisterRequest {
  name: string;
  email: string;
  phone: string;
  password: string;
  companyName: string;
  licenseNumber: string;
}

export interface UploadDocumentRequest {
  documentType: number;
  fileUrl: string;
}

export interface AuthResponse {
  token: string;
  name: string;
  email: string;
  role: string;
}
