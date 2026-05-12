import React from 'react';
import { Navigate } from 'react-router-dom';
import { isLoggedIn, getUserRole } from '../utils/auth';

export const ProtectedRoute = ({ children }) => {
  if (!isLoggedIn()) {
    return <Navigate to="/login" replace />;
  }
  return children;
};

export const AdminRoute = ({ children }) => {
  if (!isLoggedIn()) {
    return <Navigate to="/login" replace />;
  }
  
  const role = getUserRole();
  if (role !== 'ADMIN' && role !== 'SUPER_ADMIN') {
    return <Navigate to="/dashboard" replace />;
  }
  
  return children;
};

export const SuperAdminRoute = ({ children }) => {
  if (!isLoggedIn()) {
    return <Navigate to="/login" replace />;
  }
  
  if (getUserRole() !== 'SUPER_ADMIN') {
    return <Navigate to="/dashboard" replace />;
  }
  
  return children;
};
