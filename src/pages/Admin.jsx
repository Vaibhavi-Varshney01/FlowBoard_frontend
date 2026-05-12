import React, { useEffect, useState, useRef } from 'react';
import {
  Users, Layout, FileText, Send, ShieldCheck,
  Trash2, Lock, Unlock, Search, RefreshCw, Shield,
  User as UserIcon, CheckCircle, XCircle, Megaphone,
  Clock, AlertCircle, Wifi, WifiOff, Briefcase, BarChart2, ListChecks, BookOpen
} from 'lucide-react';
import api from '../api';
import './Admin.css';
import toast from 'react-hot-toast';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { getToken } from '../utils/auth';

const Admin = () => {
  // FIX 18 — real stats from API
  const [stats, setStats] = useState({ totalUsers: 0, totalBoards: 0, totalCards: 0, activeTeams: 0 });
  const [users, setUsers] = useState([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(null);
  const [broadcastMessage, setBroadcastMessage] = useState('');
  const [broadcastHistory, setBroadcastHistory] = useState([]);
  const [broadcastSending, setBroadcastSending] = useState(false);
  const [hubConnected, setHubConnected] = useState(false);
  const [activeTab, setActiveTab] = useState('users');
  const [error, setError] = useState(null);

  // FIX 15 — Workspaces & Boards tabs
  const [adminWorkspaces, setAdminWorkspaces] = useState([]);
  const [adminBoards, setAdminBoards] = useState([]);

  // FIX 16 — Audit logs
  const [auditLogs, setAuditLogs] = useState([]);
  const [auditLoading, setAuditLoading] = useState(false);

  // FIX 17 — Overdue cards
  const [overdueCards, setOverdueCards] = useState([]);
  const [overdueLoading, setOverdueLoading] = useState(false);

  const hubRef = useRef(null);

  useEffect(() => {
    const token = getToken();
    const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5050/api';
    const hubUrl = apiBase.replace('/api', '').replace(/\/$/, '') + '/hubs/notifications';
    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect().build();
    connection.start()
      .then(() => {
        setHubConnected(true);
        connection.on('ReceiveBroadcast', (data) => {
          setBroadcastHistory(prev => [{ id: Date.now(), message: data.message, sentAt: data.occurredAt || new Date().toISOString() }, ...prev]);
        });
      }).catch(() => setHubConnected(false));
    hubRef.current = connection;
    return () => connection.stop();
  }, []);

  useEffect(() => { fetchAdminData(); }, []);

  const fetchAdminData = async () => {
    try {
      setLoading(true); setError(null);
      const usersRes = await api.get('auth/users');
      const data = Array.isArray(usersRes.data) ? usersRes.data : [];
      setUsers(data);

      // FIX 18 — fetch real analytics
      try {
        const analyticsRes = await api.get('admin/analytics');
        const a = analyticsRes.data;
        setStats({ totalUsers: a.totalUsers ?? data.length, totalBoards: a.totalBoards ?? 0, totalCards: a.totalCards ?? 0, activeTeams: a.activeTeams ?? 0 });
      } catch {
        // fallback to user-derived stats
        setStats({ totalUsers: data.length, totalBoards: data.filter(u => u.isActive).length, totalCards: data.filter(u => u.role==='ADMIN'||u.role==='SUPER_ADMIN').length, activeTeams: data.filter(u => !u.isActive).length });
      }
    } catch (err) {
      const msg = err.response?.data?.detail || err.response?.data?.message || err.message || 'Network error';
      setError(`${err.response?.status || 'Error'}: ${msg}`);
      toast.error('Failed to load admin data');
    } finally { setLoading(false); }
  };

  // FIX 15 — Fetch all workspaces/boards
  const fetchAdminWorkspaces = async () => {
    try {
      const res = await api.get('workspaces/all');
      setAdminWorkspaces(Array.isArray(res.data) ? res.data : []);
    } catch { toast.error('Failed to load workspaces'); }
  };

  const fetchAdminBoards = async () => {
    try {
      const res = await api.get('boards/all');
      setAdminBoards(Array.isArray(res.data) ? res.data : []);
    } catch { toast.error('Failed to load boards'); }
  };

  // FIX 16 — Fetch audit logs
  const fetchAuditLogs = async () => {
    setAuditLoading(true);
    try {
      const res = await api.get('admin/audit-logs');
      const logs = Array.isArray(res.data) ? res.data : [];
      setAuditLogs(logs.sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp)));
    } catch { toast.error('Failed to load audit logs'); }
    finally { setAuditLoading(false); }
  };

  // FIX 17 — Fetch overdue cards
  const fetchOverdueCards = async () => {
    setOverdueLoading(true);
    try {
      const res = await api.get('cards/overdue');
      setOverdueCards(Array.isArray(res.data) ? res.data : []);
    } catch { toast.error('Failed to load overdue cards'); }
    finally { setOverdueLoading(false); }
  };

  // Tab change side-effects
  useEffect(() => {
    if (activeTab === 'workspaces') fetchAdminWorkspaces();
    if (activeTab === 'boards') fetchAdminBoards();
    if (activeTab === 'auditlogs') fetchAuditLogs();
    if (activeTab === 'overdue') fetchOverdueCards();
  }, [activeTab]);

  const handleToggleSuspend = async (userId, isCurrentlyActive) => {
    if (!window.confirm(`${isCurrentlyActive ? 'Suspend' : 'Reactivate'} this user?`)) return;
    try {
      setActionLoading(userId);
      const endpoint = isCurrentlyActive ? `auth/users/${userId}/suspend` : `auth/users/${userId}/reactivate`;
      await api.put(endpoint);
      setUsers(users.map(u => u.id === userId ? { ...u, isActive: !isCurrentlyActive } : u));
      toast.success(`User ${isCurrentlyActive ? 'suspended' : 'reactivated'}!`);
    } catch (err) { toast.error(`Failed: ${err.response?.data?.message || err.message}`);
    } finally { setActionLoading(null); }
  };

  const handleDeleteUser = async (userId) => {
    if (!window.confirm('PERMANENTLY delete this user?')) return;
    try {
      setActionLoading(userId);
      await api.delete(`auth/users/${userId}`);
      setUsers(users.filter(u => u.id !== userId));
      toast.success('User deleted');
    } catch (err) { toast.error(`Failed: ${err.response?.data?.message || err.message}`);
    } finally { setActionLoading(null); }
  };

  // FIX 15 — Delete workspace/board
  const handleDeleteAdminWorkspace = async (id) => {
    if (!window.confirm('Delete this workspace?')) return;
    try { await api.delete(`workspaces/${id}`); fetchAdminWorkspaces(); toast.success('Workspace deleted'); }
    catch (err) { toast.error(err.response?.data?.message || 'Failed'); }
  };

  const handleDeleteAdminBoard = async (id) => {
    if (!window.confirm('Delete this board?')) return;
    try { await api.delete(`boards/${id}`); fetchAdminBoards(); toast.success('Board deleted'); }
    catch (err) { toast.error(err.response?.data?.message || 'Failed'); }
  };

  const handleSendBroadcast = async () => {
    if (!broadcastMessage.trim()) return;
    try {
      setBroadcastSending(true);
      await api.post('notifications/broadcast', { message: broadcastMessage });
      toast.success('📢 Broadcast sent!');
      setBroadcastMessage('');
    } catch (err) {
      toast.error(`Failed: ${err.response?.data?.message || err.message}`);
    } finally { setBroadcastSending(false); }
  };

  const filteredUsers = users.filter(u => {
    const q = searchQuery.toLowerCase();
    return (u.fullName||'').toLowerCase().includes(q) || (u.email||'').toLowerCase().includes(q) ||
           (u.userName||u.username||'').toLowerCase().includes(q) || (u.role||'').toLowerCase().includes(q);
  });

  const formatTime = (iso) => { try { return new Date(iso).toLocaleString(); } catch { return iso; } };

  // FIX 17 — Days overdue
  const daysOverdue = (dueDate) => {
    const diff = new Date() - new Date(dueDate);
    return Math.max(0, Math.floor(diff / 86400000));
  };

  const TABS = [
    { id:'users', label:'User Management', icon:<Users size={16}/> },
    { id:'workspaces', label:'Workspaces', icon:<Briefcase size={16}/> },
    { id:'boards', label:'Boards', icon:<Layout size={16}/> },
    { id:'auditlogs', label:'Audit Logs', icon:<BookOpen size={16}/> },
    { id:'overdue', label:'Overdue Cards', icon:<ListChecks size={16}/> },
    { id:'broadcast', label:'Broadcast Center', icon:<Megaphone size={16}/>, badge: broadcastHistory.length },
  ];

  return (
    <div className="admin-container">
      <div className="admin-header">
        <div className="header-title">
          <h1>Admin Command Center</h1>
          <p>Manage system users, workspaces, boards, and analytics.</p>
        </div>
        <div className="header-actions">
          <div className={`hub-status ${hubConnected ? 'connected' : 'disconnected'}`}>
            {hubConnected ? <Wifi size={14}/> : <WifiOff size={14}/>} {hubConnected ? 'Live' : 'Offline'}
          </div>
          <button className="btn btn-secondary" onClick={fetchAdminData} disabled={loading}>
            <RefreshCw size={18} className={loading ? 'spin' : ''} /> Refresh
          </button>
        </div>
      </div>

      {/* FIX 18 — Real stats */}
      <div className="stats-grid">
        {[
          { label:'Total Users', value:stats.totalUsers, icon:<Users size={24}/>, cls:'users' },
          { label:'Total Boards', value:stats.totalBoards, icon:<Layout size={24}/>, cls:'boards' },
          { label:'Total Cards', value:stats.totalCards, icon:<BarChart2 size={24}/>, cls:'cards' },
          { label:'Active Teams', value:stats.activeTeams, icon:<ShieldCheck size={24}/>, cls:'teams' },
        ].map(s => (
          <div key={s.label} className="stat-card glass-panel">
            <div className={`stat-icon ${s.cls}`}>{s.icon}</div>
            <div className="stat-info"><h3>{s.label}</h3><span className="stat-value">{s.value}</span></div>
          </div>
        ))}
      </div>

      <div className="admin-tabs">
        {TABS.map(t => (
          <button key={t.id} className={`admin-tab ${activeTab===t.id?'active':''}`} onClick={() => setActiveTab(t.id)}>
            {t.icon} {t.label}
            {t.badge > 0 && <span className="tab-badge">{t.badge}</span>}
          </button>
        ))}
      </div>

      {/* User Management */}
      {activeTab === 'users' && (
        <div className="admin-section glass-panel">
          <div className="section-header">
            <div className="section-title"><Users size={20}/><h2>Member Management</h2></div>
            <div className="search-bar-admin">
              <Search size={18}/>
              <input type="text" placeholder="Search by name, email, role..." value={searchQuery} onChange={e => setSearchQuery(e.target.value)} />
              {searchQuery && <button className="clear-search" onClick={() => setSearchQuery('')}>✕</button>}
            </div>
          </div>
          {error && (
            <div className="admin-error-banner">
              <AlertCircle size={18}/><div><strong>Failed to load users</strong><p>{error}</p></div>
              <button className="btn btn-secondary btn-sm" onClick={fetchAdminData}>Retry</button>
            </div>
          )}
          <div className="admin-table-container">
            <table className="admin-table">
              <thead><tr><th>User Profile</th><th>Contact Info</th><th>Role</th><th>Status</th><th>Actions</th></tr></thead>
              <tbody>
                {loading ? (
                  <tr><td colSpan="5" className="table-loading"><RefreshCw size={20} className="spin"/> Fetching users...</td></tr>
                ) : filteredUsers.length === 0 ? (
                  <tr><td colSpan="5" className="table-empty">{searchQuery ? `No users matching "${searchQuery}"` : 'No users found.'}</td></tr>
                ) : filteredUsers.map(u => (
                  <tr key={u.id} className={actionLoading===u.id?'row-loading':''}>
                    <td><div className="user-profile-cell"><div className="admin-avatar">{(u.fullName||u.userName||u.username||'U')[0].toUpperCase()}</div><div className="user-name-stack"><span className="user-full-name">{u.fullName||'No Name'}</span><span className="user-handle">@{u.userName||u.username}</span></div></div></td>
                    <td><div className="user-email-cell"><span className="user-email">{u.email}</span><span className="user-meta">Joined {u.createdAt?new Date(u.createdAt).toLocaleDateString():'N/A'}</span></div></td>
                    <td><div className={`role-pill ${(u.role||'member').toLowerCase()}`}>{(u.role==='ADMIN'||u.role==='SUPER_ADMIN')?<Shield size={14}/>:<UserIcon size={14}/>}{u.role||'MEMBER'}</div></td>
                    <td><div className={`status-pill ${u.isActive?'active':'suspended'}`}>{u.isActive?<CheckCircle size={14}/>:<XCircle size={14}/>}{u.isActive?'Active':'Suspended'}</div></td>
                    <td><div className="table-actions">
                      <button className={`btn-icon-admin ${u.isActive?'suspend':'reactivate'}`} onClick={() => handleToggleSuspend(u.id,u.isActive)} disabled={actionLoading===u.id}>{u.isActive?<Lock size={16}/>:<Unlock size={16}/>}</button>
                      <button className="btn-icon-admin delete" onClick={() => handleDeleteUser(u.id)} disabled={actionLoading===u.id}><Trash2 size={16}/></button>
                    </div></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {!loading && !error && <div className="table-footer">Showing <strong>{filteredUsers.length}</strong> of <strong>{users.length}</strong> users</div>}
        </div>
      )}

      {/* FIX 15 — Workspaces tab */}
      {activeTab === 'workspaces' && (
        <div className="admin-section glass-panel">
          <div className="section-header"><div className="section-title"><Briefcase size={20}/><h2>All Workspaces</h2></div></div>
          <div className="admin-table-container">
            <table className="admin-table">
              <thead><tr><th>Name</th><th>Owner</th><th>Visibility</th><th>Actions</th></tr></thead>
              <tbody>
                {adminWorkspaces.length === 0 ? (
                  <tr><td colSpan="4" className="table-empty">No workspaces found.</td></tr>
                ) : adminWorkspaces.map(ws => {
                  const id = ws.workspaceId || ws.id;
                  return (
                    <tr key={id}>
                      <td><strong>{ws.name}</strong></td>
                      <td>{ws.ownerName || ws.ownerId || '—'}</td>
                      <td><span className={`role-pill ${ws.visibility?.toLowerCase()||'private'}`}>{ws.visibility||'PRIVATE'}</span></td>
                      <td><button className="btn-icon-admin delete" onClick={() => handleDeleteAdminWorkspace(id)}><Trash2 size={16}/></button></td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* FIX 15 — Boards tab */}
      {activeTab === 'boards' && (
        <div className="admin-section glass-panel">
          <div className="section-header"><div className="section-title"><Layout size={20}/><h2>All Boards</h2></div></div>
          <div className="admin-table-container">
            <table className="admin-table">
              <thead><tr><th>Name</th><th>Workspace</th><th>Visibility</th><th>Actions</th></tr></thead>
              <tbody>
                {adminBoards.length === 0 ? (
                  <tr><td colSpan="4" className="table-empty">No boards found.</td></tr>
                ) : adminBoards.map(board => {
                  const id = board.boardId || board.id;
                  return (
                    <tr key={id}>
                      <td><strong>{board.name}</strong></td>
                      <td>{board.workspaceName || board.workspaceId || '—'}</td>
                      <td><span className={`role-pill ${board.visibility?.toLowerCase()||'private'}`}>{board.visibility||'PRIVATE'}</span></td>
                      <td><button className="btn-icon-admin delete" onClick={() => handleDeleteAdminBoard(id)}><Trash2 size={16}/></button></td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* FIX 16 — Audit Logs tab */}
      {activeTab === 'auditlogs' && (
        <div className="admin-section glass-panel">
          <div className="section-header"><div className="section-title"><BookOpen size={20}/><h2>Audit Logs</h2></div>
            <button className="btn btn-secondary btn-sm" onClick={fetchAuditLogs} disabled={auditLoading}><RefreshCw size={14} className={auditLoading?'spin':''}/> Refresh</button>
          </div>
          <div className="admin-table-container">
            <table className="admin-table">
              <thead><tr><th>Timestamp</th><th>Actor</th><th>Action</th><th>Entity Type</th><th>Entity ID</th></tr></thead>
              <tbody>
                {auditLoading ? (
                  <tr><td colSpan="5" className="table-loading"><RefreshCw size={20} className="spin"/> Loading logs...</td></tr>
                ) : auditLogs.length === 0 ? (
                  <tr><td colSpan="5" className="table-empty">No audit logs found.</td></tr>
                ) : auditLogs.map((log, idx) => (
                  <tr key={log.id || idx}>
                    <td><span style={{fontSize:'0.82rem'}}>{formatTime(log.timestamp)}</span></td>
                    <td>{log.actor || log.actorId || log.userId || '—'}</td>
                    <td><span className="role-pill member" style={{fontSize:'0.78rem'}}>{log.action}</span></td>
                    <td>{log.entityType}</td>
                    <td style={{fontSize:'0.78rem', color:'var(--text-muted)'}}>{log.entityId}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* FIX 17 — Overdue Cards tab */}
      {activeTab === 'overdue' && (
        <div className="admin-section glass-panel">
          <div className="section-header"><div className="section-title"><ListChecks size={20}/><h2>Overdue Cards</h2></div>
            <button className="btn btn-secondary btn-sm" onClick={fetchOverdueCards} disabled={overdueLoading}><RefreshCw size={14} className={overdueLoading?'spin':''}/> Refresh</button>
          </div>
          <div className="admin-table-container">
            <table className="admin-table">
              <thead><tr><th>Card Title</th><th>Board</th><th>Assignee</th><th>Due Date</th><th>Days Overdue</th></tr></thead>
              <tbody>
                {overdueLoading ? (
                  <tr><td colSpan="5" className="table-loading"><RefreshCw size={20} className="spin"/> Loading...</td></tr>
                ) : overdueCards.length === 0 ? (
                  <tr><td colSpan="5" className="table-empty">No overdue cards 🎉</td></tr>
                ) : overdueCards.map(card => {
                  const id = card.cardId || card.id;
                  return (
                    <tr key={id}>
                      <td><strong>{card.title}</strong></td>
                      <td>{card.boardName || card.boardId || '—'}</td>
                      <td>{card.assigneeName || card.assigneeId || 'Unassigned'}</td>
                      <td style={{color:'var(--accent-danger)'}}>{card.dueDate ? new Date(card.dueDate).toLocaleDateString() : '—'}</td>
                      <td>
                        <span style={{background:'#ef444422', color:'#ef4444', borderRadius:6, padding:'2px 8px', fontSize:'0.8rem', fontWeight:600}}>
                          {card.dueDate ? daysOverdue(card.dueDate) : '—'} days
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Broadcast Center */}
      {activeTab === 'broadcast' && (
        <div className="admin-section glass-panel broadcast-section">
          <div className="section-header">
            <div className="section-title"><Megaphone size={20}/><h2>Broadcast Center</h2></div>
            <div className={`hub-status ${hubConnected?'connected':'disconnected'}`}>
              {hubConnected?<Wifi size={14}/>:<WifiOff size={14}/>} Real-time: {hubConnected?'Connected':'Disconnected'}
            </div>
          </div>
          <div className="broadcast-compose glass-panel">
            <h3>📢 Send System Announcement</h3>
            <p className="broadcast-hint">This message will appear as a popup for ALL connected users.</p>
            <textarea placeholder="Type your system-wide announcement here..." className="broadcast-input" value={broadcastMessage} onChange={e => setBroadcastMessage(e.target.value)} maxLength={500} />
            <div className="broadcast-footer">
              <span className="char-count">{broadcastMessage.length}/500 characters</span>
              <button className="btn btn-primary" onClick={handleSendBroadcast} disabled={!broadcastMessage.trim()||broadcastSending}>
                {broadcastSending?<RefreshCw size={18} className="spin"/>:<Send size={18}/>} {broadcastSending?'Sending...':'Broadcast Now'}
              </button>
            </div>
          </div>
          <div className="broadcast-history">
            <h3>📋 Broadcast History (this session)</h3>
            {broadcastHistory.length === 0 ? (
              <div className="empty-broadcast"><Megaphone size={40} style={{opacity:0.3}}/><p>No broadcasts sent yet.</p></div>
            ) : (
              <div className="broadcast-list">
                {broadcastHistory.map(b => (
                  <div key={b.id} className="broadcast-item glass-panel">
                    <div className="broadcast-icon"><Megaphone size={18}/></div>
                    <div className="broadcast-content"><p className="broadcast-msg">{b.message}</p><span className="broadcast-time"><Clock size={12}/> {formatTime(b.sentAt)}</span></div>
                    <span className="broadcast-status sent">Sent ✓</span>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default Admin;
