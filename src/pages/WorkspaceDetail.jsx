import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { 
  Plus, 
  Settings, 
  Users, 
  Trash2, 
  AlertCircle, 
  Layout, 
  Shield, 
  UserPlus,
  ChevronLeft
} from 'lucide-react';
import { fetchWorkspaces } from '../store/slices/boardSlice';
import CreateBoardModal from '../components/modals/CreateBoardModal';
import MembersModal from '../components/modals/MembersModal';
import './Dashboard.css';

const WorkspaceDetail = () => {
  const { id } = useParams();
  const dispatch = useDispatch();
  const { workspaces, loading } = useSelector(state => state.board);
  const [selectedWorkspace, setSelectedWorkspace] = useState(null);
  const [showCreateBoard, setShowCreateBoard] = useState(false);
  const [showMembers, setShowMembers] = useState(false);

  useEffect(() => {
    if (workspaces.length === 0) {
      dispatch(fetchWorkspaces());
    }
  }, [dispatch, workspaces.length]);

  useEffect(() => {
    const ws = workspaces.find(w => (w.workspaceId || w.id).toString() === id);
    setSelectedWorkspace(ws);
  }, [workspaces, id]);

  if (loading && !selectedWorkspace) return <div className="loading-state"><div className="spinner" /> Loading Workspace...</div>;
  if (!selectedWorkspace) return <div className="error-state"><AlertCircle size={48} /> <h3>Workspace not found</h3> <Link to="/dashboard">Back to Dashboard</Link></div>;

  return (
    <div className="workspace-detail">
      <div className="workspace-detail-header glass-panel">
        <div className="header-left">
          <Link to="/dashboard" className="btn-icon"><ChevronLeft /></Link>
          <div className="ws-logo">{(selectedWorkspace.name || 'W').charAt(0).toUpperCase()}</div>
          <div className="ws-info">
            <h1>{selectedWorkspace.name}</h1>
            <div className="ws-meta">
              <span className="badge">{selectedWorkspace.visibility}</span>
              <span>{selectedWorkspace.boards?.length || 0} boards</span>
            </div>
          </div>
        </div>
        <div className="header-actions">
          <button className="btn btn-secondary" onClick={() => setShowMembers(true)}>
            <Users size={18} /> Members
          </button>
          <button className="btn btn-secondary">
            <Settings size={18} /> Settings
          </button>
        </div>
      </div>

      <div className="boards-section">
        <div className="section-title">
          <Layout size={20} />
          <h2>Boards</h2>
        </div>
        <div className="boards-grid">
          {selectedWorkspace.boards?.map(board => (
            <Link 
              to={`/b/${board.boardId || board.id}`} 
              key={board.boardId || board.id} 
              className="board-card" 
              style={{ background: board.background || 'var(--bg-tertiary)' }}
            >
              <div className="board-card-overlay" />
              <h4>{board.name}</h4>
            </Link>
          ))}
          <div className="board-card create-board glass-panel" onClick={() => setShowCreateBoard(true)}>
            <Plus size={24} />
            <span>Create new board</span>
          </div>
        </div>
      </div>

      {showCreateBoard && (
        <CreateBoardModal 
          workspaceId={selectedWorkspace.workspaceId || selectedWorkspace.id} 
          onClose={() => setShowCreateBoard(false)} 
        />
      )}
      {showMembers && (
        <MembersModal 
          workspaceId={selectedWorkspace.workspaceId || selectedWorkspace.id} 
          workspaceName={selectedWorkspace.name}
          onClose={() => setShowMembers(false)} 
        />
      )}
    </div>
  );
};

export default WorkspaceDetail;
