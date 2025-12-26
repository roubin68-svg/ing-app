// src/core/auth/useAuth.js
import { useAuthContext } from "./AuthContext";

export const useAuth = () => {
  return useAuthContext();
};
