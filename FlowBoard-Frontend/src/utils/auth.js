import { jwtDecode } from 'jwt-decode';

export const getToken = () => localStorage.getItem('token');

export const getUserRole = () => {
  const token = getToken();
  if (!token) return null;
  try {
    const decoded = jwtDecode(token);
    // Standard role claims are usually 'role' or 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
    return decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'MEMBER';
  } catch (error) {
    console.error('Error decoding token:', error);
    return null;
  }
};

export const isLoggedIn = () => !!getToken();
