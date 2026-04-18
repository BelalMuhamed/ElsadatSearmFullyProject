export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  userName:string;
  userMail:string;
  roles:string[];
}
export interface loginDto
{
  email:string|null,
  password:string|null
}
export interface RefreshTokenDto
{
  token:string|null,
  ipAddress:string|null
}

export interface RoleDTO {
  roleID: string;
  roleName: string;
  createdAt: string | null;
  isDeleted?: boolean;
}

export interface CreateRoleRequestDTO {
  roleName: string;
}
