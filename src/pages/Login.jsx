import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { useSignIn, useAuth } from '@clerk/clerk-react';
import { Layout, Mail, Lock, AlertCircle } from 'lucide-react';
import { loginUser } from '../store/slices/authSlice';
import './Auth.css';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [oauthLoading, setOauthLoading] = useState('');

  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { loading, error, isAuthenticated } = useSelector((state) => state.auth);
  const { signIn, isLoaded } = useSignIn();
  const { isSignedIn } = useAuth();

  const [hasTriedSync, setHasTriedSync] = useState(false);

  // Redirect logic
  React.useEffect(() => {
    if (isLoaded && isSignedIn) {
      if (isAuthenticated) {
        navigate('/dashboard');
      } else if (!error && !hasTriedSync) {
        // If signed in to Clerk but not Redux, we need to sync
        // We only do this ONCE to prevent infinite loops
        setHasTriedSync(true);
        navigate('/sso-callback');
      }
    }
  }, [isLoaded, isSignedIn, isAuthenticated, error, hasTriedSync, navigate]);

  // ── Email/Password login ──
  const handleLogin = async (e) => {
    e.preventDefault();
    const result = await dispatch(loginUser({ email, password }));
    if (loginUser.fulfilled.match(result)) navigate('/dashboard');
  };

  // ── Direct Google/GitHub OAuth ──
  const handleOAuth = async (provider) => {
    if (!isLoaded || !signIn) return;
    if (isSignedIn) {
      navigate('/dashboard');
      return;
    }
    
    setOauthLoading(provider);
    try {
      await signIn.authenticateWithRedirect({
        strategy: provider === 'Google' ? 'oauth_google' : 'oauth_github',
        redirectUrl: `${window.location.origin}/sso-callback`,
        redirectUrlComplete: `${window.location.origin}/dashboard`,
      });
    } catch (err) {
      console.error(`${provider} login error:`, err);
      setOauthLoading('');
    }
  };

  const isSyncing = isSignedIn && !isAuthenticated && !error;

  return (
    <div className="auth-container">
      {(oauthLoading || isSyncing) && (
        <div className="oauth-overlay glass-panel">
          <div className="spinner large"></div>
          <p>{isSyncing ? 'Session found!' : `Connecting to ${oauthLoading}...`}</p>
          <span>{isSyncing ? 'Finalizing your secure session...' : `Redirecting to ${oauthLoading} for account selection`}</span>
        </div>
      )}

      <div className="glass-panel auth-card" style={{ opacity: isSyncing ? 0.7 : 1, pointerEvents: isSyncing ? 'none' : 'auto' }}>
        <div className="auth-header">
          <Layout className="brand-icon" size={32} />
          <h2>Welcome back</h2>
          <p>Log in to your FlowBoard account</p>
        </div>

        {error && (
          <div className="alert alert-danger" style={{ marginBottom: '1.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--accent-danger)' }}>
            <AlertCircle size={18} />
            <span>{error}</span>
          </div>
        )}

        <form className="auth-form" onSubmit={handleLogin}>
          <div className="form-group">
            <label>Email Address</label>
            <div className="input-with-icon">
              <Mail size={18} />
              <input
                type="email"
                className="input-field"
                placeholder="name@company.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                disabled={isSyncing}
              />
            </div>
          </div>

          <div className="form-group">
            <label>Password</label>
            <div className="input-with-icon">
              <Lock size={18} />
              <input
                type="password"
                className="input-field"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                disabled={isSyncing}
              />
            </div>
          </div>

          <button type="submit" className="btn btn-primary btn-block" disabled={loading || isSyncing}>
            {loading ? 'Logging in...' : 'Log In'}
          </button>
        </form>

        <div className="auth-divider">
          <span>OR CONTINUE WITH</span>
        </div>

        <div className="oauth-buttons">
          <button 
            className="btn btn-secondary oauth-btn" 
            type="button" 
            onClick={() => handleOAuth('Google')}
            disabled={!isLoaded || isSyncing}
          >
            <img src="https://www.svgrepo.com/show/475656/google-color.svg" alt="Google" width="20" /> 
            Google
          </button>
          <button 
            className="btn btn-secondary oauth-btn" 
            type="button" 
            onClick={() => handleOAuth('GitHub')}
            disabled={!isLoaded || isSyncing}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 0C5.37 0 0 5.37 0 12c0 5.3 3.44 9.8 8.2 11.38.6.11.82-.26.82-.58v-2.03c-3.34.73-4.04-1.61-4.04-1.61-.55-1.39-1.34-1.76-1.34-1.76-1.09-.75.08-.73.08-.73 1.2.08 1.84 1.24 1.84 1.24 1.07 1.83 2.81 1.3 3.5 1 .1-.78.42-1.3.76-1.6-2.67-.3-5.47-1.33-5.47-5.93 0-1.31.47-2.38 1.24-3.22-.14-.3-.54-1.52.1-3.18 0 0 1.01-.32 3.3 1.23a11.5 11.5 0 0 1 3-.4c1.02 0 2.04.14 3 .4 2.28-1.55 3.29-1.23 3.29-1.23.64 1.66.24 2.88.12 3.18.77.84 1.23 1.91 1.23 3.22 0 4.61-2.81 5.63-5.48 5.92.43.37.81 1.1.81 2.22v3.29c0 .32.21.7.82.58C20.56 21.8 24 17.3 24 12c0-6.63-5.37-12-12-12z"/>
            </svg>
            GitHub
          </button>
        </div>

        <p className="auth-footer">
          Don't have an account? <Link to="/register">Sign up</Link>
        </p>
      </div>
    </div>
  );
};

export default Login;
