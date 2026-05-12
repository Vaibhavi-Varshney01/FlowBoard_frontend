import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { useUser } from '@clerk/clerk-react';
import { oauthLogin } from '../store/slices/authSlice';
import { AlertCircle, RefreshCw } from 'lucide-react';

const SsoCallback = () => {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { user, isLoaded, isSignedIn } = useUser();
  const [error, setError] = useState(null);

  const { isAuthenticated } = useSelector((state) => state.auth);

  const performSync = async () => {
    if (!user || isAuthenticated) {
      if (isAuthenticated) navigate('/dashboard', { replace: true });
      return;
    }
    
    setError(null);
    const email = user.primaryEmailAddress?.emailAddress || '';
    const providerAccount = user.externalAccounts[0];
    let providerName = providerAccount ? providerAccount.provider.toUpperCase() : 'GOOGLE';
    if (providerName.startsWith('OAUTH_')) {
      providerName = providerName.replace('OAUTH_', '');
    }
    
    const usernameBase = email ? email.split('@')[0] : `user_${Math.floor(Math.random() * 10000)}`;

    if (!email) {
      console.error('No email found in Clerk user object');
      setError('Your Google/GitHub account must have a primary email address to log in.');
      return;
    }

    console.log('Attempting sync for:', email);
    
    try {
      const result = await dispatch(oauthLogin({
        email,
        fullName: user.fullName || user.firstName || usernameBase,
        provider: providerName,
        avatarUrl: user.imageUrl || ''
      }));

      if (oauthLogin.fulfilled.match(result)) {
        console.log('Sync successful! Navigating to dashboard...');
        navigate('/dashboard', { replace: true });
      } else {
        console.error('Sync rejected:', result.payload);
        const detailedError = typeof result.payload === 'object' 
          ? JSON.stringify(result.payload) 
          : result.payload;
        setError(detailedError || 'The backend refused the login request.');
      }
    } catch (err) {
      console.error('Sync exception:', err);
      setError('A network error occurred while connecting to the server.');
    }
  };

  useEffect(() => {
    if (isLoaded && isSignedIn && user) {
      if (isAuthenticated) {
        navigate('/dashboard', { replace: true });
      } else {
        performSync();
      }
    } else if (isLoaded && !isSignedIn) {
      console.warn("Clerk says you are not signed in. Redirecting to login.");
      navigate('/login', { replace: true });
    }
  }, [isLoaded, isSignedIn, user, isAuthenticated]);

  if (error) {
    return (
      <div className="auth-container">
        <div className="glass-panel auth-card" style={{ textAlign: 'center', padding: '3rem' }}>
          <AlertCircle size={48} color="var(--accent-danger)" style={{ marginBottom: '1.5rem' }} />
          <h2 style={{ marginBottom: '0.5rem' }}>Sync Failed</h2>
          <div style={{ color: 'var(--text-secondary)', marginBottom: '2rem' }}>
             <p>{typeof error === 'string' ? error : 'An error occurred during synchronization'}</p>
          </div>
          <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
            <button className="btn btn-primary" onClick={performSync}>
              <RefreshCw size={18} /> Try Again
            </button>
            <button className="btn btn-secondary" onClick={() => navigate('/login')}>
              Back to Login
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      minHeight: '80vh',
      flexDirection: 'column',
      gap: '1.5rem'
    }}>
      <div style={{
        width: 44,
        height: 44,
        border: '3px solid rgba(99,102,241,0.15)',
        borderTop: '3px solid #6366f1',
        borderRadius: '50%',
        animation: 'spin 0.8s linear infinite'
      }}></div>
      <p style={{ color: 'var(--text-secondary)', fontSize: '1rem', margin: 0 }}>
        Finalizing your secure session...
      </p>
      <span style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>
        Verifying account details with the backend
      </span>
    </div>
  );
};

export default SsoCallback;
