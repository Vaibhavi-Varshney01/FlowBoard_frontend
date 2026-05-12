import axios from 'axios';

const getBaseURL = () => {
  const base = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5050/api';
  // Ensure it ends with /api/
  let url = base.replace(/\/?$/, '');
  if (!url.endsWith('/api')) {
    url += '/api';
  }
  return url + '/';
};

const api = axios.create({
  baseURL: getBaseURL(),
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor — attach token from localStorage
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor — if 401, clear token and redirect to login
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      // Only redirect if not already on login/sso-callback page
      const currentPath = window.location.pathname;
      const isInitialSync = currentPath.includes('/sso-callback');
      
      if (!currentPath.includes('/login') && !isInitialSync) {
        // Only redirect if we've been on the page for more than 1 second 
        // to avoid race conditions during login sync
        setTimeout(() => {
          if (!localStorage.getItem('token')) {
            window.location.href = '/login';
          }
        }, 500);
      }
    }
    return Promise.reject(error);
  }
);

export default api;

