import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Layout, Mail, Lock, User, AtSign, AlertCircle } from 'lucide-react';
import { useSignUp, useAuth } from '@clerk/clerk-react';
import { registerUser, oauthLogin } from '../store/slices/authSlice';
import './Auth.css';

const Register = () => {
  const [fullName, setFullName] = useState('');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState('MEMBER');
  
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { loading, error, isAuthenticated } = useSelector((state) => state.auth);
  const { signUp, isLoaded } = useSignUp();
  const { isSignedIn } = useAuth();

  // Redirect logic
  React.useEffect(() => {
    if (isLoaded && isSignedIn) {
      if (isAuthenticated) {
        navigate('/dashboard');
      } else {
        // If signed in to Clerk but not Redux, we need to sync
        // But don't redirect if we just had an error (to prevent loops)
        if (!error) {
          navigate('/sso-callback');
        }
      }
    }
  }, [isLoaded, isSignedIn, isAuthenticated, error, navigate]);

  const handleRegister = async (e) => {
    e.preventDefault();
    const result = await dispatch(registerUser({ fullName, username, email, password, role }));
    if (registerUser.fulfilled.match(result)) {
      navigate('/dashboard');
    }
  };

  const handleOAuth = async (provider) => {
    if (!isLoaded || !signUp) return;
    if (isSignedIn) {
      navigate('/dashboard');
      return;
    }

    try {
      // Save selected role for sync after redirect
      localStorage.setItem('pendingRole', role);

      await signUp.authenticateWithRedirect({
        strategy: provider === 'Google' ? 'oauth_google' : 'oauth_github',
        redirectUrl: `${window.location.origin}/sso-callback`,
        redirectUrlComplete: `${window.location.origin}/dashboard`,
      });
    } catch (err) {
      console.error(`${provider} signup error:`, err);
    }
  };

  const isSyncing = isSignedIn && !isAuthenticated && !error;

  return (
    <div className="auth-container">
      {isSyncing && (
        <div className="oauth-overlay glass-panel">
          <div className="spinner large"></div>
          <p>Session found!</p>
          <span>Finalizing your secure session...</span>
        </div>
      )}

      <div className="glass-panel auth-card" style={{ opacity: isSyncing ? 0.7 : 1, pointerEvents: isSyncing ? 'none' : 'auto' }}>
        <div className="auth-header">
          <Layout className="brand-icon" size={32} />
          <h2>Create an account</h2>
          <p>Start organizing your work today</p>
        </div>

        {error && (
          <div className="alert alert-danger" style={{ marginBottom: '1.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--accent-danger)' }}>
            <AlertCircle size={18} />
            <span>{error}</span>
          </div>
        )}

        <form className="auth-form" onSubmit={handleRegister}>
          <div className="form-group">
            <label>Full Name</label>
            <div className="input-with-icon">
              <User size={18} />
              <input 
                type="text" 
                className="input-field" 
                placeholder="John Doe" 
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                required 
                disabled={isSyncing}
              />
            </div>
          </div>

          <div className="form-group">
            <label>Username</label>
            <div className="input-with-icon">
              <AtSign size={18} />
              <input 
                type="text" 
                className="input-field" 
                placeholder="johndoe123" 
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required 
                disabled={isSyncing}
              />
            </div>
          </div>

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

          <div className="form-group">
            <label>I am joining as a...</label>
            <div className="role-selector">
              <div 
                className={`role-option ${role === 'MEMBER' ? 'active' : ''}`} 
                onClick={() => !isSyncing && setRole('MEMBER')}
                style={{ cursor: isSyncing ? 'default' : 'pointer' }}
              >
                Member
              </div>
              <div 
                className={`role-option ${role === 'ADMIN' ? 'active' : ''}`} 
                onClick={() => !isSyncing && setRole('ADMIN')}
                style={{ cursor: isSyncing ? 'default' : 'pointer' }}
              >
                Admin
              </div>
            </div>
          </div>

          <button type="submit" className="btn btn-primary btn-block" disabled={loading || isSyncing}>
            {loading ? 'Creating account...' : 'Sign Up'}
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
          Already have an account? <Link to="/login">Log in</Link>
        </p>
      </div>
    </div>
  );
};

export default Register;
