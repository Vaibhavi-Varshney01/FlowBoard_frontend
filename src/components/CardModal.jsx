import React, { useState, useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { X, AlignLeft, CheckSquare, Paperclip, MessageSquare, Tag, User, Trash2, Calendar, Archive, Edit2, Reply, MoreHorizontal } from 'lucide-react';
import {
  deleteCard, updateCard, archiveCard, fetchCardHistory, fetchChecklists, fetchComments,
  addComment, replyToComment, updateComment, deleteComment,
  createChecklist, createChecklistItem, toggleChecklistItem, deleteChecklist,
  fetchBoardLabels, addLabelToCard, removeLabelFromCard,
  fetchBoardMembers, fetchAttachments, uploadAttachment, deleteAttachment
} from '../store/slices/boardSlice';
import './CardModal.css';

const COVER_COLORS = ['#ef4444','#f59e0b','#10b981','#3b82f6','#8b5cf6','#ec4899'];

const CardModal = ({ card, listTitle, boardId, onClose }) => {
  const dispatch = useDispatch();
  const { user } = useSelector(state => state.auth);
  const boardLabels = useSelector(state => state.board.boardLabels[boardId] || []);
  const boardMembers = useSelector(state => state.board.boardMembers[boardId] || []);
  const history = useSelector(state => state.board.cardHistory[card.id] || []);
  const checklists = useSelector(state => state.board.cardChecklists[card.id] || []);
  const comments = useSelector(state => state.board.cardComments[card.id] || []);
  const attachments = useSelector(state => state.board.cardAttachments[card.id] || []);

  const [title, setTitle] = useState(card.content);
  const [description, setDescription] = useState(card.description || '');
  const [isEditingDesc, setIsEditingDesc] = useState(false);
  const [commentText, setCommentText] = useState('');
  const [showLabels, setShowLabels] = useState(false);
  const [showMembers, setShowMembers] = useState(false);
  const [replyingTo, setReplyingTo] = useState(null);
  const [replyText, setReplyText] = useState('');
  const [editingCommentId, setEditingCommentId] = useState(null);
  const [editCommentText, setEditCommentText] = useState('');
  const fileInputRef = useRef();

  useEffect(() => {
    if (card?.id) {
      dispatch(fetchCardHistory(card.id));
      dispatch(fetchChecklists(card.id));
      dispatch(fetchComments(card.id));
      dispatch(fetchAttachments(card.id));
      dispatch(fetchBoardLabels(boardId));
      dispatch(fetchBoardMembers(boardId));
    }
  }, [dispatch, card.id, boardId]);

  const formatRelativeTime = (dateString) => {
    const now = new Date();
    const past = new Date(dateString);
    const diffInMins = Math.floor((now - past) / 60000);
    if (diffInMins < 1) return 'just now';
    if (diffInMins < 60) return `${diffInMins}m ago`;
    const diffInHours = Math.floor(diffInMins / 60);
    if (diffInHours < 24) return `${diffInHours}h ago`;
    return past.toLocaleDateString();
  };

  if (!card) return null;

  const handleDelete = () => {
    if (window.confirm('Delete this card?')) {
      dispatch(deleteCard({ cardId: card.id, boardId }));
      onClose();
    }
  };

  const handleArchive = () => {
    if (window.confirm('Archive this card?')) {
      dispatch(archiveCard({ cardId: card.id, boardId }));
      onClose();
    }
  };

  const handleUpdateTitle = () => {
    if (title.trim() && title !== card.content)
      dispatch(updateCard({ cardId: card.id, boardId, updates: { title } }));
  };

  const handleUpdateDesc = () => {
    dispatch(updateCard({ cardId: card.id, boardId, updates: { description } }));
    setIsEditingDesc(false);
  };

  const handleAddComment = () => {
    if (commentText.trim()) {
      dispatch(addComment({ cardId: card.id, text: commentText }));
      setCommentText('');
    }
  };

  const handleReply = (commentId) => {
    if (replyText.trim()) {
      dispatch(replyToComment({ cardId: card.id, parentCommentId: commentId, text: replyText }));
      setReplyingTo(null);
      setReplyText('');
    }
  };

  const handleEditComment = (comment) => {
    setEditingCommentId(comment.id);
    setEditCommentText(comment.text);
  };

  const handleSaveEdit = (commentId) => {
    if (editCommentText.trim()) {
      dispatch(updateComment({ commentId, cardId: card.id, text: editCommentText }));
      setEditingCommentId(null);
    }
  };

  const handleDeleteComment = (commentId) => {
    if (window.confirm('Delete this comment?'))
      dispatch(deleteComment({ commentId, cardId: card.id }));
  };

  const handleToggleLabel = (label) => {
    const hasLabel = card.labels?.some(l => l.id === label.id);
    if (hasLabel) dispatch(removeLabelFromCard({ cardId: card.id, labelId: label.id }));
    else dispatch(addLabelToCard({ cardId: card.id, labelId: label.id }));
  };

  const handleAssign = (memberId) => {
    dispatch(updateCard({ cardId: card.id, boardId, updates: { assigneeId: memberId } }));
    setShowMembers(false);
  };

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (file) dispatch(uploadAttachment({ cardId: card.id, file }));
  };

  const topComments = comments.filter(c => !c.parentCommentId);
  const getReplies = (parentId) => comments.filter(c => c.parentCommentId === parentId);

  const assignee = boardMembers.find(m => (m.userId || m.id) === card.assigneeId);

  const renderComment = (comment, isReply = false) => (
    <div key={comment.id} className={`history-item comment-item ${isReply ? 'reply-item' : ''}`} style={isReply ? { marginLeft: '2.5rem' } : {}}>
      <div className="avatar-sm">
        {comment.authorAvatar ? <img src={comment.authorAvatar} alt="U" /> : (comment.authorName?.charAt(0) || 'U')}
      </div>
      <div className="history-content" style={{ flex: 1 }}>
        <p><strong>{comment.authorName}</strong> <span className="history-time">{formatRelativeTime(comment.createdAt)}</span></p>
        {editingCommentId === comment.id ? (
          <div>
            <textarea className="description-textarea" value={editCommentText} onChange={e => setEditCommentText(e.target.value)} />
            <div className="edit-actions">
              <button className="btn btn-primary btn-sm" onClick={() => handleSaveEdit(comment.id)}>Save</button>
              <button className="btn btn-secondary btn-sm" onClick={() => setEditingCommentId(null)}>Cancel</button>
            </div>
          </div>
        ) : (
          <div className="comment-text glass-panel">{comment.text}</div>
        )}
        <div className="comment-meta-actions">
          <button className="btn-text-xs" onClick={() => { setReplyingTo(comment.id); setReplyText(''); }}>
            <Reply size={12} /> Reply
          </button>
          {comment.authorId === user?.id && (
            <>
              <button className="btn-text-xs" onClick={() => handleEditComment(comment)}><Edit2 size={12} /> Edit</button>
              <button className="btn-text-xs danger" onClick={() => handleDeleteComment(comment.id)}><Trash2 size={12} /> Delete</button>
            </>
          )}
        </div>
        {replyingTo === comment.id && (
          <div className="reply-form" style={{ marginTop: '0.5rem' }}>
            <textarea className="description-textarea" placeholder="Write a reply..." value={replyText} onChange={e => setReplyText(e.target.value)} />
            <div className="edit-actions">
              <button className="btn btn-primary btn-sm" onClick={() => handleReply(comment.id)}>Reply</button>
              <button className="btn btn-secondary btn-sm" onClick={() => setReplyingTo(null)}>Cancel</button>
            </div>
          </div>
        )}
      </div>
    </div>
  );

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content glass-panel" onClick={e => e.stopPropagation()}>
        <button className="close-btn" onClick={onClose}><X size={24} /></button>

        {/* Cover Color Bar */}
        {card.coverColor && <div style={{ height: '6px', background: card.coverColor, borderRadius: '12px 12px 0 0', marginBottom: '0.5rem' }} />}

        <div className="modal-header">
          <div className="card-title-icon"><AlignLeft size={24} /></div>
          <div style={{ flex: 1 }}>
            <input className="modal-title-input" value={title} onChange={e => setTitle(e.target.value)} onBlur={handleUpdateTitle} />
            <p className="modal-subtitle">in list <u>{listTitle}</u></p>
          </div>
        </div>

        <div className="modal-body">
          <div className="modal-main">
            {/* Description */}
            <div className="modal-section">
              <div className="section-header">
                <AlignLeft size={20} /><h3>Description</h3>
                {!isEditingDesc && description && <button className="btn btn-secondary btn-xs" onClick={() => setIsEditingDesc(true)}>Edit</button>}
              </div>
              {isEditingDesc || !description ? (
                <div className="description-edit">
                  <textarea className="description-textarea" placeholder="Add a more detailed description..." value={description} onChange={e => setDescription(e.target.value)} autoFocus={isEditingDesc} />
                  <div className="edit-actions">
                    <button className="btn btn-primary btn-sm" onClick={handleUpdateDesc}>Save</button>
                    <button className="btn btn-secondary btn-sm" onClick={() => { setIsEditingDesc(false); setDescription(card.description || ''); }}>Cancel</button>
                  </div>
                </div>
              ) : (
                <div className="description-display" onClick={() => setIsEditingDesc(true)}>{description}</div>
              )}
            </div>

            {/* Attachments Section */}
            {attachments.length > 0 && (
              <div className="modal-section">
                <div className="section-header"><Paperclip size={20} /><h3>Attachments</h3></div>
                {attachments.map(att => (
                  <div key={att.id} className="attachment-item" style={{ display:'flex', alignItems:'center', gap:'0.75rem', padding:'0.5rem', background:'var(--bg-tertiary)', borderRadius:'8px', marginBottom:'0.5rem' }}>
                    <Paperclip size={14} />
                    <div style={{ flex:1 }}>
                      <a href={att.url} target="_blank" rel="noreferrer" style={{ color:'var(--accent-primary)', fontWeight:500 }}>{att.fileName}</a>
                      <span style={{ display:'block', fontSize:'0.75rem', color:'var(--text-muted)' }}>{att.fileSize ? `${(att.fileSize/1024).toFixed(1)} KB` : ''}</span>
                    </div>
                    <button className="btn-icon" onClick={() => dispatch(deleteAttachment({ attachmentId: att.id, cardId: card.id }))} title="Remove"><Trash2 size={14} /></button>
                  </div>
                ))}
              </div>
            )}

            {/* Checklists */}
            {checklists.map(checklist => {
              const completedCount = checklist.items?.filter(i => i.isCompleted).length || 0;
              const totalCount = checklist.items?.length || 0;
              const progress = totalCount === 0 ? 0 : Math.round((completedCount / totalCount) * 100);
              return (
                <div key={checklist.id} className="modal-section">
                  <div className="section-header">
                    <CheckSquare size={20} /><h3>{checklist.title}</h3>
                    <button className="btn btn-secondary btn-xs" onClick={() => { if(window.confirm('Delete?')) dispatch(deleteChecklist({ checklistId: checklist.id, cardId: card.id })); }}>Delete</button>
                  </div>
                  <div className="progress-container">
                    <span className="progress-percent">{progress}%</span>
                    <div className="progress-bar"><div className="progress-fill" style={{ width: `${progress}%` }}></div></div>
                  </div>
                  <div className="checklist-items">
                    {checklist.items?.map(item => (
                      <div key={item.id} className="check-item">
                        <input type="checkbox" checked={item.isCompleted} onChange={() => dispatch(toggleChecklistItem({ checklistId: checklist.id, itemId: item.id, cardId: card.id, isCompleted: !item.isCompleted }))} />
                        <span className={item.isCompleted ? 'completed' : ''}>{item.text}</span>
                      </div>
                    ))}
                    <button className="btn btn-secondary btn-xs add-item-btn" onClick={() => { const t = window.prompt('Item text:'); if(t) dispatch(createChecklistItem({ checklistId: checklist.id, cardId: card.id, text: t })); }}>Add an item</button>
                  </div>
                </div>
              );
            })}

            {/* Activity & Comments */}
            <div className="modal-section">
              <div className="section-header"><MessageSquare size={20} /><h3>Activity</h3></div>
              <div className="comment-input-area">
                <div className="avatar-sm">{user?.avatarUrl ? <img src={user.avatarUrl} alt="U" /> : (user?.username?.charAt(0) || 'U')}</div>
                <div className="comment-box">
                  <textarea placeholder="Write a comment..." value={commentText} onChange={e => setCommentText(e.target.value)} />
                  <div className="comment-actions">
                    <button className="btn btn-primary btn-sm" disabled={!commentText.trim()} onClick={handleAddComment}>Save</button>
                  </div>
                </div>
              </div>
              <div className="activity-feed">
                {topComments.map(comment => (
                  <div key={comment.id}>
                    {renderComment(comment)}
                    {getReplies(comment.id).map(reply => renderComment(reply, true))}
                  </div>
                ))}
                {history.map((item, idx) => (
                  <div key={idx} className="history-item">
                    <div className="avatar-sm">{item.actorId === user?.id ? (user?.avatarUrl ? <img src={user.avatarUrl} alt="U" /> : user.username?.charAt(0)) : 'A'}</div>
                    <div className="history-content">
                      <p><strong>{item.actorId === user?.id ? 'You' : 'Someone'}</strong> {item.action?.toLowerCase()} {item.detail}</p>
                      <span className="history-time">{formatRelativeTime(item.occurredAt)}</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>

          <div className="modal-sidebar">
            {/* Status — FIX 6: correct enum values */}
            <div className="sidebar-module">
              <h4>Status</h4>
              <div className="status-selector glass-panel">
                <span className={`status-pill ${(card.status || 'TO_DO').toLowerCase().replace('_','-')}`}>{card.status || 'TO_DO'}</span>
                <select className="hidden-select" defaultValue={card.status || 'TO_DO'} onChange={e => dispatch(updateCard({ cardId: card.id, boardId, updates: { status: e.target.value } }))}>
                  <option value="TO_DO">To Do</option>
                  <option value="IN_PROGRESS">In Progress</option>
                  <option value="IN_REVIEW">In Review</option>
                  <option value="DONE">Done</option>
                </select>
              </div>
            </div>

            {/* Priority */}
            <div className="sidebar-module">
              <h4>Priority</h4>
              <div className="priority-selector glass-panel">
                <span className={`priority-pill ${(card.priority || 'LOW').toLowerCase()}`}>{card.priority || 'LOW'}</span>
                <select className="hidden-select" defaultValue={card.priority || 'LOW'} onChange={e => dispatch(updateCard({ cardId: card.id, boardId, updates: { priority: e.target.value } }))}>
                  <option value="LOW">Low</option>
                  <option value="MEDIUM">Medium</option>
                  <option value="HIGH">High</option>
                  <option value="CRITICAL">Critical</option>
                </select>
              </div>
            </div>

            {/* FIX 3: Assignee */}
            <div className="sidebar-module">
              <h4>Assignee</h4>
              {card.assigneeId && <div style={{ fontSize:'0.8rem', color:'var(--text-muted)', marginBottom:'0.5rem' }}>
                {assignee ? (assignee.fullName || assignee.username || assignee.userId) : card.assigneeName || 'Assigned'}
              </div>}
              <button className="sidebar-btn" onClick={() => setShowMembers(!showMembers)}><User size={16}/> {card.assigneeId ? 'Change' : 'Assign Member'}</button>
              {showMembers && (
                <div className="sidebar-dropdown glass-panel">
                  {boardMembers.length === 0 && <p style={{fontSize:'0.8rem',color:'var(--text-muted)',padding:'0.5rem'}}>No members found</p>}
                  {boardMembers.map(m => {
                    const mId = m.userId || m.id;
                    const mName = m.fullName || m.username || m.userName || mId;
                    return <button key={mId} className="dropdown-member-item" onClick={() => handleAssign(mId)}><User size={14}/> {mName}</button>;
                  })}
                  {card.assigneeId && <button className="dropdown-member-item danger" onClick={() => { dispatch(updateCard({ cardId: card.id, boardId, updates: { assigneeId: null } })); setShowMembers(false); }}>Remove Assignee</button>}
                </div>
              )}
            </div>

            {/* FIX 2: Labels */}
            <div className="sidebar-module">
              <h4>Labels</h4>
              <button className="sidebar-btn" onClick={() => setShowLabels(!showLabels)}><Tag size={16}/> Labels</button>
              {showLabels && (
                <div className="sidebar-dropdown glass-panel">
                  {boardLabels.length === 0 && <p style={{fontSize:'0.8rem',color:'var(--text-muted)',padding:'0.5rem'}}>No labels on this board</p>}
                  {boardLabels.map(label => {
                    const checked = card.labels?.some(l => l.id === label.id);
                    return (
                      <div key={label.id} className="label-toggle-item" onClick={() => handleToggleLabel(label)} style={{ cursor:'pointer', display:'flex', alignItems:'center', gap:'0.5rem', padding:'0.4rem 0.5rem', borderRadius:'6px' }}>
                        <span style={{ width:16, height:16, borderRadius:4, background: label.color, flexShrink:0, border: checked ? '2px solid #fff' : 'none' }} />
                        <span style={{ flex:1, fontSize:'0.85rem' }}>{label.name}</span>
                        {checked && <span style={{color:'var(--accent-success)'}}>✓</span>}
                      </div>
                    );
                  })}
                </div>
              )}
            </div>

            {/* FIX 4: Dates */}
            <div className="sidebar-module">
              <h4>Dates</h4>
              <label style={{fontSize:'0.75rem',color:'var(--text-muted)',display:'block',marginBottom:'0.25rem'}}>Start Date</label>
              <input type="date" className="form-control" style={{marginBottom:'0.5rem'}}
                defaultValue={card.startDate ? card.startDate.split('T')[0] : ''}
                onChange={e => dispatch(updateCard({ cardId: card.id, boardId, updates: { startDate: e.target.value } }))}
              />
              <label style={{fontSize:'0.75rem',color:'var(--text-muted)',display:'block',marginBottom:'0.25rem'}}>Due Date</label>
              <input type="date" className="form-control"
                defaultValue={card.dueDate ? card.dueDate.split('T')[0] : ''}
                onChange={e => dispatch(updateCard({ cardId: card.id, boardId, updates: { dueDate: e.target.value } }))}
              />
            </div>

            {/* FIX 5: Cover Color */}
            <div className="sidebar-module">
              <h4>Cover Color</h4>
              <div style={{ display:'flex', gap:'0.4rem', flexWrap:'wrap' }}>
                {COVER_COLORS.map(color => (
                  <div key={color} onClick={() => dispatch(updateCard({ cardId: card.id, boardId, updates: { coverColor: color } }))}
                    style={{ width:28, height:28, borderRadius:6, background:color, cursor:'pointer', border: card.coverColor===color ? '2px solid #fff' : '2px solid transparent' }} />
                ))}
                {card.coverColor && <div onClick={() => dispatch(updateCard({ cardId: card.id, boardId, updates: { coverColor: null } }))}
                  style={{ width:28, height:28, borderRadius:6, background:'var(--bg-tertiary)', cursor:'pointer', display:'flex', alignItems:'center', justifyContent:'center', fontSize:'0.7rem', color:'var(--text-muted)' }}>✕</div>}
              </div>
            </div>

            {/* Checklist */}
            <div className="sidebar-module">
              <h4>Add to card</h4>
              <button className="sidebar-btn" onClick={() => { const t = window.prompt('Checklist title:','Checklist'); if(t) dispatch(createChecklist({ cardId: card.id, title: t })); }}>
                <CheckSquare size={16}/> Checklist
              </button>
              {/* FIX 8: Attachment upload */}
              <button className="sidebar-btn" onClick={() => fileInputRef.current?.click()}><Paperclip size={16}/> Attachment</button>
              <input type="file" ref={fileInputRef} style={{display:'none'}} onChange={handleFileChange} />
            </div>

            <div className="sidebar-module">
              <h4>Actions</h4>
              {/* FIX 7: Archive button */}
              <button className="sidebar-btn" onClick={handleArchive}><Archive size={16}/> Archive Card</button>
              <button className="sidebar-btn danger" onClick={handleDelete}><Trash2 size={16}/> Delete Card</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CardModal;
