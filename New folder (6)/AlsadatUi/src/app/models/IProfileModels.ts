/**
 * Profile domain models (frontend).
 * Mirror the backend DTOs in Application/DTOs/Profile/*.cs
 */

/** GET /api/Profile response payload. */
export interface ProfileDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  userName: string;
  phoneNumber?: string | null;
  defaultCurrency: string;
}

/**
 * PUT /api/Profile request body.
 * Property names use lowercase-first to match the backend DTO convention.
 */
export interface UpdateProfileRequest {
  phoneNumber?: string | null;
  email: string;
  userName: string;
}

/**
 * PUT /api/Profile response payload.
 * The frontend uses `usernameChanged` to decide whether to force re-login.
 */
export interface UpdateProfileResponse {
  usernameChanged: boolean;
  emailChanged: boolean;
  phoneChanged: boolean;
}

/**
 * PUT /api/Profile/change-password request body.
 * Property names are PascalCase here because the backend DTO uses PascalCase
 * (kept as-is to avoid touching ChangePasswordRequest.cs).
 */
export interface ChangePasswordRequest {
  OldPassword: string;
  NewPassword: string;
  ConfirmPassword: string;
}
