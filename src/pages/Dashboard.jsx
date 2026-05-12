import React, { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Plus, Layout, Users, Settings, AlertCircle, RefreshCw, Briefcase, History, CheckSquare, LayoutGrid, Clock, Menu, X as CloseIcon, Edit2, Trash2, Eye, EyeOff } from 'lucide-react';
import { fetchWorkspaces, updateWorkspace, deleteWorkspace } from '../store/slices/boardSlice';
import { deactivateAccount, updateProfile, changePassword } from '../store/slices/authSlice';
import CreateWorkspaceModal from '../components/modals/CreateWorkspaceModal';
import MembersModal from '../components/modals/MembersModal';
import CreateBoardModal from '../components/modals/CreateBoardModal';
import toast from 'react-hot-toast';
import './Dashboard.css';

const Dashboard = () => {
  const [activeTab, setActiveTab] = useState('Boards');
  const [showCreateWorkspace, setShowCreateWorkspace] = useState(false);
  const [selectedWorkspaceForMembers, setSelectedWorkspaceForMembers] = useState(null);
  const [selectedWorkspaceForBoard, setSelectedWorkspaceForBoard] = useState(null);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

  // FIX 12 — Workspace edit state
  const [editingWorkspaceId, setEditingWorkspaceId] = useState(null);
  const [editWsName, setEditWsName] = useState('');
  const [editWsDesc, setEditWsDesc] = useState('');
  const [editWsVisibility, setEditWsVisibility] = useState('PRIVATE');

  // FIX 23 — Controlled full name input
  const [fullNameInput, setFullNameInput] = useState('');
  const [fullNameInitialized, setFullNameInitialized] = useState(false);

  // FIX 22 — Password change state
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPasswords, setShowPasswords] = useState(false);

  const dispatch = useDispatch();
  const { workspaces, loading, error, cardHistory } = useSelector(state => state.board);
  const { user } = useSelector(state => state.auth);
  const location = useLocation();

  const queryParams = new URLSearchParams(location.search);
  const searchTerm = queryParams.get('search')?.toLowerCase() || '';

  useEffect(() => { dispatch(fetchWorkspaces()); }, [dispatch]);

  useEffect(() => {
    if (user && !fullNameInitialized) {
      setFullNameInput(user.username || '');
      setFullNameInitialized(true);
    }
  }, [user, fullNameInitialized]);

  const filteredWorkspaces = workspaces?.map(ws => {
    const matchedBoards = ws.boards?.filter(b => b.name.toLowerCase().includes(searchTerm)) || [];
    const wsMatches = ws.name.toLowerCase().includes(searchTerm);
    if (wsMatches || matchedBoards.length > 0) return { ...ws, boards: searchTerm ? matchedBoards : ws.boards };
    return null;
  }).filter(Boolean);

  const handleDeleteAccount = () => {
    if (window.confirm('PERMANENTLY delete your account?')) dispatch(deactivateAccount());
  };

  const handleUpdateAvatar = (url) => {
    dispatch(updateProfile({ avatarUrl: url, fullName: user.username, username: user.username }));
  };

  // FIX 23 — Update full name
  const handleUpdateFullName = async () => {
    if (!fullNameInput.trim()) { toast.error('Full name cannot be empty'); return; }
    const result = await dispatch(updateProfile({ fullName: fullNameInput, username: user.username, avatarUrl: user.avatarUrl }));
    if (updateProfile.fulfilled.match(result)) toast.success('Name updated!');
    else toast.error(result.payload || 'Update failed');
  };

  // FIX 22 — Change password
  const handleChangePassword = async () => {
    if (!currentPassword || !newPassword) { toast.error('Fill in all password fields'); return; }
    if (newPassword !== confirmPassword) { toast.error('New passwords do not match'); return; }
    const result = await dispatch(changePassword({ currentPassword, newPassword }));
    if (changePassword.fulfilled.match(result)) {
      toast.success('Password changed!');
      setCurrentPassword(''); setNewPassword(''); setConfirmPassword('');
    } else {
      toast.error(result.payload || 'Password change failed');
    }
  };

  // FIX 12 — Start editing workspace
  const startEditWorkspace = (ws) => {
    setEditingWorkspaceId(ws.workspaceId || ws.id);
    setEditWsName(ws.name);
    setEditWsDesc(ws.description || '');
    setEditWsVisibility(ws.visibility || 'PRIVATE');
  };

  const handleSaveWorkspace = async (wsId) => {
    const result = await dispatch(updateWorkspace({ workspaceId: wsId, updates: { name: editWsName, description: editWsDesc, visibility: editWsVisibility } }));
    if (updateWorkspace.fulfilled.match(result)) { toast.success('Workspace updated!'); setEditingWorkspaceId(null); }
    else toast.error(result.payload || 'Update failed');
  };

  const handleDeleteWorkspace = async (wsId) => {
    if (!window.confirm('Delete this workspace and all its boards?')) return;
    const result = await dispatch(deleteWorkspace(wsId));
    if (deleteWorkspace.fulfilled.match(result)) toast.success('Workspace deleted');
    else toast.error(result.payload || 'Delete failed');
  };

  const renderBoards = () => (
    <>
      <div className="dashboard-header">
        <h2>Your Workspaces</h2>
        <button className="btn btn-primary" onClick={() => setShowCreateWorkspace(true)}><Plus size={18} /> Create Workspace</button>
      </div>
      {loading && <div className="loading-state"><div className="spinner" /> Loading workspaces...</div>}
      {error && (
        <div className="error-state glass-panel">
          <AlertCircle color="var(--accent-danger)" size={32} /><h3>Could Not Load Workspaces</h3><p className="error-details">{error}</p>
          <button className="btn btn-secondary" onClick={() => dispatch(fetchWorkspaces())} style={{ marginTop:'1rem' }}><RefreshCw size={16} /> Try Again</button>
        </div>
      )}
      {!loading && !error && filteredWorkspaces?.length === 0 && (
        <div className="empty-state glass-panel">
          <Briefcase size={48} style={{ color:'var(--text-muted)', marginBottom:'1rem' }} />
          <h3>{searchTerm ? 'No Boards Found' : 'No Workspaces Yet'}</h3>
          <p>{searchTerm ? `No boards matching "${searchTerm}"` : 'Create your first workspace to get started.'}</p>
        </div>
      )}
      {!loading && !error && filteredWorkspaces?.map(workspace => {
        const wsId = workspace.workspaceId || workspace.id;
        const isEditing = editingWorkspaceId === wsId;
        return (
          <div key={wsId} className="workspace-section">
            <div className="workspace-header">
              <div className="workspace-title">
                <div className="workspace-icon">{(workspace.name || 'W').charAt(0).toUpperCase()}</div>
                <div>
                  {isEditing ? (
                    <div style={{ display:'flex', flexDirection:'column', gap:'0.4rem' }}>
                      <input className="form-control" value={editWsName} onChange={e => setEditWsName(e.target.value)} placeholder="Workspace name" />
                      <input className="form-control" value={editWsDesc} onChange={e => setEditWsDesc(e.target.value)} placeholder="Description" />
                      <select className="form-control" value={editWsVisibility} onChange={e => setEditWsVisibility(e.target.value)}>
                        <option value="PRIVATE">Private</option>
                        <option value="PUBLIC">Public</option>
                      </select>
                      <div style={{ display:'flex', gap:'0.5rem' }}>
                        <button className="btn btn-primary btn-sm" onClick={() => handleSaveWorkspace(wsId)}>Save</button>
                        <button className="btn btn-secondary btn-sm" onClick={() => setEditingWorkspaceId(null)}>Cancel</button>
                      </div>
                    </div>
                  ) : (
                    <>
                      <h3>{workspace.name}</h3>
                      {workspace.description && <p className="ws-desc">{workspace.description}</p>}
                    </>
                  )}
                </div>
              </div>
              <div className="workspace-actions">
                {/* FIX 12 — Edit icon */}
                {!isEditing && (
                  <button className="btn-icon" title="Edit workspace" onClick={() => startEditWorkspace(workspace)}><Edit2 size={15} /></button>
                )}
                <button className="btn-icon" title="Delete workspace" onClick={() => handleDeleteWorkspace(wsId)}><Trash2 size={15} style={{ color:'var(--accent-danger)' }} /></button>
                {(user?.role === 'ADMIN' || user?.role === 'SUPER_ADMIN') && (
                  <button className="btn btn-secondary btn-sm" onClick={() => setSelectedWorkspaceForMembers(workspace)}><Users size={14} /> Members</button>
                )}
                <button className="btn btn-secondary btn-sm" onClick={() => setActiveTab('Settings')}><Settings size={14} /> Settings</button>
              </div>
            </div>
            <div className="boards-grid">
              {workspace.boards?.map(board => {
                const boardId = board.boardId || board.id;
                return (
                  <Link to={`/b/${boardId}`} key={boardId} className="board-card" style={{ background: board.background || 'var(--bg-tertiary)' }}>
                    <div className="board-card-overlay" /><h4>{board.name}</h4>
                  </Link>
                );
              })}
              <div className="board-card create-board glass-panel" onClick={() => setSelectedWorkspaceForBoard(workspace)}>
                <Plus size={24} /><span>Create new board</span>
              </div>
            </div>
          </div>
        );
      })}
    </>
  );

  const renderMembers = () => (
    <>
      <div className="dashboard-header"><h2>Workspace Members</h2></div>
      {workspaces?.length === 0 ? (
        <div className="empty-state glass-panel"><p>Create a workspace first to manage members.</p></div>
      ) : (
        <div className="members-view">
          {workspaces?.map(workspace => {
            const wsId = workspace.workspaceId || workspace.id;
            return (
              <div key={wsId} className="workspace-members-card glass-panel">
                <div className="ws-mem-header">
                  <div className="workspace-icon">{(workspace.name || 'W').charAt(0).toUpperCase()}</div>
                  <div><h4>{workspace.name}</h4><span style={{ fontSize:'0.8rem', color:'var(--text-muted)' }}>{workspace.visibility}</span></div>
                </div>
                {(user?.role === 'ADMIN' || user?.role === 'SUPER_ADMIN') && (
                  <button className="btn btn-secondary btn-sm" onClick={() => setSelectedWorkspaceForMembers(workspace)}><Users size={14} /> Manage Members</button>
                )}
              </div>
            );
          })}
        </div>
      )}
    </>
  );

  const renderHistory = () => {
    const events = [];
    workspaces?.forEach(ws => {
      if (ws.createdAt) events.push({ id:`ws-${ws.workspaceId||ws.id}`, icon:<LayoutGrid size={16}/>, color:'#6366f1', message:<>You created workspace <strong>{ws.name}</strong></>, time:ws.createdAt });
      ws.boards?.forEach(board => {
        if (board.createdAt) events.push({ id:`board-${board.boardId||board.id}`, icon:<Layout size={16}/>, color:'#3b82f6', message:<>You created board <strong>{board.name}</strong></>, time:board.createdAt });
      });
    });
    Object.values(cardHistory || {}).flat().forEach((item, idx) => events.push({ id:`ch-${idx}`, icon:<CheckSquare size={16}/>, color:'#10b981', message:<><strong>You</strong> {item.action?.toLowerCase()} {item.detail}</>, time:item.occurredAt }));
    events.sort((a,b) => new Date(b.time)-new Date(a.time));
    const fmt = (d) => { const mins = Math.floor((new Date()-new Date(d))/60000); if(mins<1) return 'just now'; if(mins<60) return `${mins}m ago`; const h=Math.floor(mins/60); if(h<24) return `${h}h ago`; return `${Math.floor(h/24)}d ago`; };
    return (
      <>
        <div className="dashboard-header"><h2>Activity History</h2></div>
        {loading && <div className="loading-state"><div className="spinner" /> Loading...</div>}
        {!loading && events.length === 0 && <div className="empty-state glass-panel"><History size={48} style={{ color:'var(--text-muted)', marginBottom:'1rem' }} /><h3>No Activity Yet</h3></div>}
        {!loading && events.length > 0 && (
          <div className="history-timeline glass-panel" style={{ padding:'1.5rem', borderRadius:16 }}>
            {events.map((ev,idx) => (
              <div key={ev.id} style={{ display:'flex', gap:'1rem', padding:'0.85rem 0', borderBottom: idx<events.length-1?'1px solid var(--border-color)':'none', alignItems:'flex-start' }}>
                <div style={{ width:36, height:36, borderRadius:'50%', background:`${ev.color}22`, border:`1.5px solid ${ev.color}55`, display:'flex', alignItems:'center', justifyContent:'center', color:ev.color, flexShrink:0 }}>{ev.icon}</div>
                <div style={{ flex:1 }}>
                  <p style={{ margin:0, fontSize:'0.9rem' }}>{ev.message}</p>
                  <span style={{ fontSize:'0.75rem', color:'var(--text-muted)', display:'flex', alignItems:'center', gap:4, marginTop:4 }}><Clock size={11}/> {fmt(ev.time)}</span>
                </div>
              </div>
            ))}
          </div>
        )}
      </>
    );
  };

  const renderSettings = () => (
    <>
      <div className="dashboard-header"><h2>Settings</h2></div>
      <div className="settings-view glass-panel">
        <div className="settings-section">
          <h3>Profile</h3>
          <div className="setting-item"><div className="setting-info"><label>Username</label><span>{user?.username || 'Not set'}</span></div></div>
          <div className="setting-item"><div className="setting-info"><label>Email</label><span>{user?.email || 'Not set'}</span></div></div>
          <div className="setting-item"><div className="setting-info"><label>Role</label><span className="badge-role">{user?.role || 'MEMBER'}</span></div></div>
        </div>

        <div className="settings-section">
          <h3>Profile Settings</h3>
          <div className="setting-item">
            <div className="setting-info">
              <label>Profile Picture</label>
              <div className="avatar-grid">
                {['Felix','Aneka','Jack','Molly','Spooky'].map(seed => {
                  const url = `https://api.dicebear.com/7.x/avataaars/svg?seed=${seed}`;
                  return <div key={seed} className={`avatar-option ${user?.avatarUrl===url?'selected':''}`} onClick={() => handleUpdateAvatar(url)}><img src={url} alt="Avatar" /></div>;
                })}
              </div>
            </div>
          </div>
          {/* FIX 23 — Controlled full name input */}
          <div className="setting-item">
            <div className="setting-info">
              <label>Full Name</label>
              <input type="text" className="setting-input" value={fullNameInput} onChange={e => setFullNameInput(e.target.value)} />
            </div>
            <button className="btn btn-secondary btn-sm" onClick={handleUpdateFullName}>Update</button>
          </div>
        </div>

        {/* FIX 22 — Change Password section */}
        <div className="settings-section">
          <h3>Change Password</h3>
          <div className="setting-item" style={{ flexDirection:'column', alignItems:'stretch', gap:'0.5rem' }}>
            <input type={showPasswords ? 'text' : 'password'} className="setting-input" placeholder="Current password" value={currentPassword} onChange={e => setCurrentPassword(e.target.value)} />
            <input type={showPasswords ? 'text' : 'password'} className="setting-input" placeholder="New password" value={newPassword} onChange={e => setNewPassword(e.target.value)} />
            <input type={showPasswords ? 'text' : 'password'} className="setting-input" placeholder="Confirm new password" value={confirmPassword} onChange={e => setConfirmPassword(e.target.value)} />
            <div style={{ display:'flex', gap:'0.5rem', marginTop:'0.25rem' }}>
              <button className="btn btn-primary btn-sm" onClick={handleChangePassword}>Change Password</button>
              <button className="btn btn-secondary btn-sm" onClick={() => setShowPasswords(v => !v)}>{showPasswords ? <EyeOff size={14}/> : <Eye size={14}/>} {showPasswords ? 'Hide' : 'Show'}</button>
            </div>
          </div>
        </div>

        <div className="settings-section">
          <h3>Danger Zone</h3>
          <div className="setting-item danger-zone">
            <div className="setting-info"><label>Delete Account</label><span>This action is permanent and cannot be undone.</span></div>
            <button className="btn btn-danger btn-sm" onClick={handleDeleteAccount}>Delete Account</button>
          </div>
        </div>
      </div>
    </>
  );

  return (
    <div className={`dashboard-container ${sidebarCollapsed ? 'sidebar-hidden' : ''}`}>
      <button className="sidebar-toggle-btn glass-panel" onClick={() => setSidebarCollapsed(!sidebarCollapsed)} title={sidebarCollapsed ? 'Expand' : 'Collapse'}>
        {sidebarCollapsed ? <Menu size={20} /> : <CloseIcon size={20} />}
      </button>
      <div className={`dashboard-sidebar glass-panel ${sidebarCollapsed ? 'collapsed' : ''}`}>
        <div className="sidebar-user">
          <div className="sidebar-avatar" title={user?.username}>
            {user?.avatarUrl ? <img src={user.avatarUrl} alt={user.username} /> : (user?.username || 'U').charAt(0).toUpperCase()}
          </div>
          <div className="user-details-sidebar">
            <span className="sidebar-username">{user?.username || 'User'}</span>
            <span className={`role-badge-sidebar ${user?.role?.toLowerCase() || 'member'}`}>{user?.role || 'MEMBER'}</span>
          </div>
        </div>
        <ul className="sidebar-menu">
          {[['Boards',<Layout size={18}/>],['Members',<Users size={18}/>],['History',<History size={18}/>],['Settings',<Settings size={18}/>]].map(([tab, icon]) => (
            <li key={tab} className={activeTab===tab?'active':''} onClick={() => setActiveTab(tab)}>{icon} {tab}</li>
          ))}
        </ul>
      </div>
      <div className="dashboard-content">
        {activeTab === 'Boards' && renderBoards()}
        {activeTab === 'Members' && renderMembers()}
        {activeTab === 'History' && renderHistory()}
        {activeTab === 'Settings' && renderSettings()}
      </div>
      {showCreateWorkspace && <CreateWorkspaceModal onClose={() => setShowCreateWorkspace(false)} />}
      {selectedWorkspaceForBoard && <CreateBoardModal workspaceId={selectedWorkspaceForBoard.workspaceId||selectedWorkspaceForBoard.id} onClose={() => setSelectedWorkspaceForBoard(null)} />}
      {selectedWorkspaceForMembers && <MembersModal workspaceId={selectedWorkspaceForMembers.workspaceId||selectedWorkspaceForMembers.id} workspaceName={selectedWorkspaceForMembers.name} onClose={() => setSelectedWorkspaceForMembers(null)} />}
    </div>
  );
};

export default Dashboard;
