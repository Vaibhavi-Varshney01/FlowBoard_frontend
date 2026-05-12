import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Layout, Bell, User, Settings, Sun, Moon, LogOut, Search, X } from 'lucide-react';
import { useClerk } from '@clerk/clerk-react';
import { logout } from '../store/slices/authSlice';
import { fetchNotifications, markAllAsRead, markAsRead } from '../store/slices/notificationSlice';
import { getUserRole } from '../utils/auth';
import api from '../api';
import './Navbar.css';

// FIX 26 — debounce helper
function useDebounce(value, delay) {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const t = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(t);
  }, [value, delay]);
  return debounced;
}

const Navbar = () => {
  const [theme, setTheme] = useState('dark');
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useSelector((state) => state.auth);
  const { unreadCount, items: notifications } = useSelector((state) => state.notifications);
  const { signOut } = useClerk();
  const [searchQuery, setSearchQuery] = useState('');
  const [showNotifications, setShowNotifications] = useState(false);

  // FIX 26 — Card search state
  const [searchResults, setSearchResults] = useState([]);
  const [searchLoading, setSearchLoading] = useState(false);
  const [showResults, setShowResults] = useState(false);
  const searchRef = useRef(null);

  const debouncedQuery = useDebounce(searchQuery, 300);

  // FIX 26 — Fetch card search results when query ≥ 3 chars
  useEffect(() => {
    if (debouncedQuery.length >= 3) {
      setSearchLoading(true);
      api.get(`cards/search?q=${encodeURIComponent(debouncedQuery)}`)
        .then(res => { setSearchResults(Array.isArray(res.data) ? res.data : []); setShowResults(true); })
        .catch(() => setSearchResults([]))
        .finally(() => setSearchLoading(false));
    } else {
      setSearchResults([]);
      setShowResults(false);
    }
  }, [debouncedQuery]);

  // Close dropdown on outside click
  useEffect(() => {
    const handler = (e) => { if (searchRef.current && !searchRef.current.contains(e.target)) setShowResults(false); };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  useEffect(() => {
    if (isAuthenticated) dispatch(fetchNotifications());
  }, [dispatch, isAuthenticated]);

  const handleSearch = (e) => {
    e.preventDefault();
    setShowResults(false);
    if (searchQuery.trim()) navigate(`/dashboard?search=${encodeURIComponent(searchQuery)}`);
  };

  useEffect(() => { document.documentElement.setAttribute('data-theme', theme); }, [theme]);

  const handleLogout = async () => {
    try { await signOut(); } catch (err) { console.error('Clerk logout error:', err); }
    dispatch(logout());
    navigate('/login');
  };

  return (
    <nav className="glass-panel navbar">
      <div className="navbar-brand">
        <Layout className="brand-icon" />
        <Link to="/" className="brand-name">FlowBoard</Link>
      </div>

      {/* FIX 26 — Search with dropdown results */}
      <div ref={searchRef} style={{ position: 'relative' }}>
        <form className="navbar-search" onSubmit={handleSearch}>
          <Search size={18} />
          <input
            type="text"
            placeholder="Search cards or boards..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onFocus={() => searchResults.length > 0 && setShowResults(true)}
          />
          {searchQuery && (
            <button type="button" style={{ background:'none', border:'none', cursor:'pointer', color:'var(--text-muted)', padding:0 }} onClick={() => { setSearchQuery(''); setShowResults(false); }}>
              <X size={14} />
            </button>
          )}
        </form>

        {showResults && (
          <div className="notification-dropdown glass-panel" style={{ top: '110%', left: 0, right: 0, maxHeight: 300, overflowY: 'auto' }}>
            <div className="dropdown-header"><h3>Cards</h3></div>
            {searchLoading ? (
              <div style={{ padding: '0.75rem', color: 'var(--text-muted)', fontSize: '0.85rem' }}>Searching...</div>
            ) : searchResults.length === 0 ? (
              <div style={{ padding: '0.75rem', color: 'var(--text-muted)', fontSize: '0.85rem' }}>No results found</div>
            ) : searchResults.map(card => {
              const boardId = card.boardId || card.board?.boardId;
              const cardId = card.cardId || card.id;
              return (
                <div key={cardId} className="notif-item" style={{ cursor: 'pointer' }}
                  onClick={() => { navigate(`/b/${boardId}`); setShowResults(false); setSearchQuery(''); }}>
                  <div className="notif-content">
                    <p style={{ margin: 0 }}><strong>{card.title}</strong></p>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{card.boardName || card.board?.name || `Board ${boardId}`}</span>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      <div className="navbar-actions">
        <Link to="/dashboard" className="nav-link">Dashboard</Link>
        <Link to="/messages" className="nav-link">Messages</Link>
        {(getUserRole() === 'SUPER_ADMIN' || getUserRole() === 'ADMIN') && <Link to="/admin" className="nav-link admin-link">Admin</Link>}
        <button className="icon-btn" onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')} title="Toggle Theme">
          {theme === 'dark' ? <Sun size={20} /> : <Moon size={20} />}
        </button>

        {isAuthenticated ? (
          <>
            <div className="notification-wrapper">
              <button className="icon-btn" title="Notifications" onClick={() => setShowNotifications(!showNotifications)}>
                <Bell size={20} />
                {unreadCount > 0 && <span className="badge">{unreadCount}</span>}
              </button>
              {showNotifications && (
                <div className="notification-dropdown glass-panel">
                  <div className="dropdown-header">
                    <h3>Notifications</h3>
                    <button className="btn-text" onClick={() => dispatch(markAllAsRead())}>Mark all as read</button>
                  </div>
                  <div className="notification-list">
                    {notifications.length === 0 ? (
                      <div className="empty-notif">No notifications</div>
                    ) : notifications.slice(0, 5).map((n, idx) => (
                      <div key={n.id || `notif-${idx}`} className={`notif-item ${!n.isRead ? 'unread' : ''}`} onClick={() => dispatch(markAsRead(n.id))}>
                        <div className="notif-content">
                          <p>{n.message}</p>
                          <span className="notif-time">{new Date(n.createdAt).toLocaleTimeString()}</span>
                        </div>
                        {!n.isRead && <div className="unread-dot"></div>}
                      </div>
                    ))}
                  </div>
                  <Link to="/messages" className="view-all" onClick={() => setShowNotifications(false)}>View all messages</Link>
                </div>
              )}
            </div>
            <div className="user-profile-nav">
              <div className="avatar" title={user?.username}>
                {user?.avatarUrl ? <img src={user.avatarUrl} alt={user.username} /> : <User size={20} />}
              </div>
              <div className="user-info-text">
                <span className="username-display">{user?.username}</span>
                <span className="user-role-badge">{getUserRole()}</span>
              </div>
            </div>
            <button className="icon-btn logout-btn" onClick={handleLogout} title="Logout"><LogOut size={20} /></button>
          </>
        ) : (
          <Link to="/login" className="btn btn-secondary" style={{ padding: '0.4rem 0.8rem', fontSize: '0.85rem' }}>Login</Link>
        )}
      </div>
    </nav>
  );
};

export default Navbar;
