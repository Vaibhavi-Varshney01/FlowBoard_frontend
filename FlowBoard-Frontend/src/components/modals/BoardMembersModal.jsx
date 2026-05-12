import React, { useEffect, useState } from 'react';
import { X, User, Crown, Plus, Loader, Trash2, Shield } from 'lucide-react';
import api from '../../api';
import toast from 'react-hot-toast';
import './Modal.css';

// FIX 10 — Board member management modal
const BoardMembersModal = ({ boardId, boardName, onClose }) => {
  const [members, setMembers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [addEmail, setAddEmail] = useState('');
  const [adding, setAdding] = useState(false);
  const [addError, setAddError] = useState('');
  const [showAddForm, setShowAddForm] = useState(false);

  const reload = async () => {
    setLoading(true);
    try {
      const res = await api.get(`boards/${boardId}/members`);
      setMembers(res.data);
    } catch (err) {
      toast.error('Failed to load board members');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { reload(); }, [boardId]);

  const handleAddMember = async (e) => {
    e.preventDefault();
    setAdding(true);
    setAddError('');
    try {
      // First find the user by email
      const searchRes = await api.get(`auth/search?query=${addEmail}`);
      const users = searchRes.data;
      
      if (!users || users.length === 0) {
        setAddError('No user found with that email.');
        return;
      }
      
      const userId = users[0].id || users[0].userId;
      
      // Then add to board using userId
      await api.post(`boards/${boardId}/members`, { userId, role: 'MEMBER' });
      
      setAddEmail('');
      setShowAddForm(false);
      reload();
      toast.success('Member added!');
    } catch (err) {
      setAddError(err.response?.data?.message || 'Failed to add member.');
    } finally {
      setAdding(false);
    }
  };

  const handleRemove = async (userId) => {
    if (!window.confirm('Remove this member from the board?')) return;
    try {
      await api.delete(`boards/${boardId}/members/${userId}`);
      reload();
      toast.success('Member removed');
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to remove member');
    }
  };

  const handleRoleChange = async (userId, newRole) => {
    try {
      await api.put(`boards/${boardId}/members/${userId}/role`, { role: newRole });
      reload();
      toast.success('Role updated');
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to update role');
    }
  };

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal-content glass-panel" style={{ width: '520px', maxWidth: '95vw' }}>
        <div className="modal-header">
          <h3>Board Members — {boardName}</h3>
          <button className="close-btn" onClick={onClose}><X size={18} /></button>
        </div>
        <div className="modal-body">
          {loading ? (
            <div style={{ display:'flex', justifyContent:'center', padding:'2rem' }}>
              <Loader size={24} style={{ animation:'spin 1s linear infinite', color:'var(--accent-primary)' }} />
            </div>
          ) : (
            <>
              <div className="members-list">
                {members.length === 0 ? (
                  <p style={{ color:'var(--text-muted)', textAlign:'center', padding:'1.5rem' }}>No members yet.</p>
                ) : members.map(member => {
                  const mId = member.userId || member.id;
                  const mName = member.fullName || member.username || member.userName || mId;
                  return (
                    <div key={mId} style={{ display:'flex', alignItems:'center', gap:'0.75rem', padding:'0.6rem 0', borderBottom:'1px solid var(--border-color)' }}>
                      <div className="member-avatar"><User size={18} /></div>
                      <div style={{ flex:1 }}>
                        <p className="member-name">{mName}</p>
                        <p className="member-joined" style={{ fontSize:'0.75rem', color:'var(--text-muted)' }}>{member.email || ''}</p>
                      </div>
                      <select
                        value={member.role || 'MEMBER'}
                        onChange={e => handleRoleChange(mId, e.target.value)}
                        style={{ background:'var(--bg-tertiary)', color:'var(--text-primary)', border:'1px solid var(--border-color)', borderRadius:6, padding:'0.25rem 0.5rem', fontSize:'0.8rem' }}
                      >
                        <option value="OBSERVER">Observer</option>
                        <option value="MEMBER">Member</option>
                        <option value="ADMIN">Admin</option>
                      </select>
                      <button className="btn-icon" title="Remove" onClick={() => handleRemove(mId)}>
                        <Trash2 size={14} style={{ color:'var(--accent-danger)' }} />
                      </button>
                    </div>
                  );
                })}
              </div>

              {showAddForm ? (
                <form onSubmit={handleAddMember} style={{ marginTop:'1rem' }}>
                  <input type="email" className="form-control" placeholder="Enter member's email..." value={addEmail}
                    onChange={e => setAddEmail(e.target.value)} required autoFocus />
                  {addError && <p style={{ color:'var(--accent-danger)', fontSize:'0.8rem', marginTop:'0.3rem' }}>{addError}</p>}
                  <div style={{ display:'flex', gap:'0.5rem', marginTop:'0.5rem' }}>
                    <button type="submit" className="btn btn-primary btn-sm" disabled={adding}>{adding ? 'Adding...' : 'Add'}</button>
                    <button type="button" className="btn btn-secondary btn-sm" onClick={() => { setShowAddForm(false); setAddError(''); }}>Cancel</button>
                  </div>
                </form>
              ) : (
                <button className="btn btn-secondary" style={{ width:'100%', marginTop:'1rem', justifyContent:'center' }} onClick={() => setShowAddForm(true)}>
                  <Plus size={16} /> Add Member by Email
                </button>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default BoardMembersModal;
