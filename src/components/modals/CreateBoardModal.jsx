import React, { useState } from 'react';
import { X } from 'lucide-react';
import { useDispatch } from 'react-redux';
import { createBoard } from '../../store/slices/boardSlice';
import './Modal.css';

const CreateBoardModal = ({ workspaceId, onClose }) => {
  const [name, setName] = useState('');
  const [background, setBackground] = useState('linear-gradient(135deg, #3b82f6, #8b5cf6)');
  // FIX 14: visibility selector
  const [visibility, setVisibility] = useState('PRIVATE');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [localError, setLocalError] = useState(null);
  const dispatch = useDispatch();

  const backgrounds = [
    'linear-gradient(135deg, #3b82f6, #8b5cf6)',
    'linear-gradient(135deg, #10b981, #3b82f6)',
    'linear-gradient(135deg, #f59e0b, #ef4444)',
    'linear-gradient(135deg, #6366f1, #a855f7)',
    'linear-gradient(135deg, #ec4899, #f43f5e)',
    '#1e293b'
  ];

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!name.trim()) return;
    setIsSubmitting(true);
    setLocalError(null);
    try {
      const result = await dispatch(createBoard({ workspaceId, name, background, visibility }));
      if (createBoard.fulfilled.match(result)) {
        onClose();
      } else {
        setLocalError(result.payload || 'Failed to create board');
      }
    } catch {
      setLocalError('An unexpected error occurred');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content glass-panel" style={{ width: '400px' }}>
        <div className="modal-header">
          <h3>Create Board</h3>
          <button className="close-btn" onClick={onClose}><X /></button>
        </div>
        <form onSubmit={handleSubmit} className="modal-form">
          {localError && <div style={{ color: '#ef4444', fontSize: '0.8rem', marginBottom: '0.5rem' }}>{localError}</div>}
          <div className="form-group">
            <label>Board Title</label>
            <input type="text" className="form-control" placeholder="Enter board title..." value={name} onChange={e => setName(e.target.value)} required />
          </div>
          {/* FIX 14: Visibility */}
          <div className="form-group">
            <label>Visibility</label>
            <select className="form-control" value={visibility} onChange={e => setVisibility(e.target.value)}>
              <option value="PRIVATE">Private</option>
              <option value="PUBLIC">Public</option>
            </select>
          </div>
          <div className="form-group">
            <label>Background</label>
            <div className="bg-selector">
              {backgrounds.map((bg, idx) => (
                <div key={idx} className={`bg-option ${background === bg ? 'active' : ''}`} style={{ background: bg }} onClick={() => setBackground(bg)} />
              ))}
            </div>
          </div>
          <button type="submit" className="btn btn-primary" style={{ width: '100%', marginTop: '1rem' }} disabled={isSubmitting}>
            {isSubmitting ? 'Creating...' : 'Create Board'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default CreateBoardModal;
