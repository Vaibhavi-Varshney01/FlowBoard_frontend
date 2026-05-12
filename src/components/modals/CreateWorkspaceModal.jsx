import React, { useState } from 'react';
import { X } from 'lucide-react';
import { useDispatch } from 'react-redux';
import { createWorkspace } from '../../store/slices/boardSlice';
import './Modal.css';


const CreateWorkspaceModal = ({ onClose }) => {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [visibility, setVisibility] = useState('PUBLIC');
  const dispatch = useDispatch();

  const handleSubmit = (e) => {
    e.preventDefault();
    dispatch(createWorkspace({ name, description, visibility }));
    onClose();
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content glass-panel" style={{ width: '400px' }}>
        <div className="modal-header">
          <h3>Create Workspace</h3>
          <button className="close-btn" onClick={onClose}><X /></button>
        </div>
        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label>Workspace Name</label>
            <input 
              type="text" 
              className="form-control" 
              value={name} 
              onChange={(e) => setName(e.target.value)} 
              required 
            />
          </div>
          <div className="form-group">
            <label>Description (Optional)</label>
            <textarea 
              className="form-control" 
              value={description} 
              onChange={(e) => setDescription(e.target.value)} 
            />
          </div>
          <div className="form-group">
            <label>Visibility</label>
            <select 
              className="form-control" 
              value={visibility} 
              onChange={(e) => setVisibility(e.target.value)}
            >
              <option value="PUBLIC">Public</option>
              <option value="PRIVATE">Private</option>
            </select>
          </div>
          <button type="submit" className="btn btn-primary" style={{ width: '100%', marginTop: '1rem' }}>
            Create
          </button>
        </form>
      </div>
    </div>
  );
};

export default CreateWorkspaceModal;
