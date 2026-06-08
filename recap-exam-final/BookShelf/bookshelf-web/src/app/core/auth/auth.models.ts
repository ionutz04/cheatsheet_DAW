export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

// BookShelf AuthController returns { token, expiresIn }.
export interface AuthResponse {
  token: string;
  expiresIn: number;
}
