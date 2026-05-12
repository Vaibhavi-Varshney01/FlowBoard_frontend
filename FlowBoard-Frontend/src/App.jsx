import React, { useEffect } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { addNotification } from './store/slices/notificationSlice';
import { getToken } from './utils/auth';
import Navbar from './components/Navbar';
import Home from './pages/Home';
import Dashboard from './pages/Dashboard';
import Board from './pages/Board';
import Login from './pages/Login';
import Register from './pages/Register';
import Admin from './pages/Admin';
import WorkspaceDetail from './pages/WorkspaceDetail';
import SsoCallback from './pages/SsoCallback';
import ClerkSignIn from './pages/ClerkSignIn';
import Messages from './pages/Messages';

// FIX 27 — BoardPage removed; all board navigation goes through Board.jsx at /b/:boardId

import { ProtectedRoute, AdminRoute, SuperAdminRoute } from './components/ProtectedRoute';
import { Toaster, toast } from 'react-hot-toast';
import './App.css';

function App() {
  const dispatch = useDispatch();
  const { isAuthenticated, loading: authLoading } = useSelector((state) => state.auth);

  useEffect(() => {
    if (isAuthenticated) {
      const token = getToken();
      const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5050/api';
      const hubUrl = apiBase.replace('/api', '') + '/hubs/notifications';

      const connection = new HubConnectionBuilder()
        .withUrl(hubUrl, { accessTokenFactory: () => token })
        .withAutomaticReconnect()
        .build();

      connection.start()
        .then(() => {
          console.log('Connected to SignalR Notification Hub');
          connection.on('ReceiveNotification', (notification) => {
            dispatch(addNotification(notification));
          });
          connection.on('ReceiveBroadcast', (data) => {
            dispatch(addNotification(data));
            toast.success(data.message, {
              duration: 10000, icon: '📢',
              style: { border: '1px solid #6366f1', padding: '16px', color: '#fff', background: '#1e1b4b' },
            });
          });
        })
        .catch(err => console.error('SignalR Connection Error: ', err));

      return () => connection.stop();
    }
  }, [isAuthenticated, dispatch]);

  const isSsoCallback = window.location.pathname === '/sso-callback';
  const hasToken = !!localStorage.getItem('token');
  
  // Only show global loading if we have a token but are still verifying (authLoading)
  // or if we are in the middle of an SSO callback.
  if (authLoading && hasToken && !isAuthenticated && !isSsoCallback) {
    return (
      <div className="app-loading" style={{ 
        display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100vh', 
        background: 'var(--bg-primary)', color: 'white', flexDirection: 'column', gap: '1rem' 
      }}>
        <div className="spinner large"></div>
        <span>Loading FlowBoard...</span>
      </div>
    );
  }

  return (
    <div className="app-container" style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <Toaster position="top-right" />
      <Navbar />
      <main className="main-content" style={{ flex: 1, padding: '2rem', display: 'flex', flexDirection: 'column' }}>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/sign-in/*" element={<ClerkSignIn />} />
          <Route path="/sso-callback" element={<SsoCallback />} />

          <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
          <Route path="/workspaces/:id" element={<ProtectedRoute><WorkspaceDetail /></ProtectedRoute>} />
          {/* FIX 24 — /b/:boardId is public (no ProtectedRoute); Board.jsx handles guest mode internally */}
          <Route path="/b/:boardId" element={<Board />} />
          {/* FIX 27 — /board-details redirects to /b/ */}
          <Route path="/board-details/:boardId" element={<Navigate to="/dashboard" replace />} />
          <Route path="/messages" element={<ProtectedRoute><Messages /></ProtectedRoute>} />

          <Route path="/admin" element={<AdminRoute><Admin /></AdminRoute>} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
