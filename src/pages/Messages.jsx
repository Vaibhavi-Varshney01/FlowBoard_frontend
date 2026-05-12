import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { fetchNotifications, markAsRead, markAllAsRead, deleteNotification } from '../store/slices/notificationSlice';
import {
  Mail, MessageSquare, Bell, CheckCircle, Trash2,
  Megaphone, Filter, Search, Clock, ChevronRight,
  Shield, User, ArrowLeft, MoreHorizontal, Inbox
} from 'lucide-react';
import './Messages.css';
import toast from 'react-hot-toast';

const formatTimeDistance = (date) => {
  const now = new Date();
  const diffInSeconds = Math.floor((now - new Date(date)) / 1000);
  if (diffInSeconds < 60) return 'just now';
  if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)}m ago`;
  if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)}h ago`;
  return `${Math.floor(diffInSeconds / 86400)}d ago`;
};

const Messages = () => {
  const dispatch = useDispatch();
  // FIX 19 — useNavigate for deep-link
  const navigate = useNavigate();
  const { items, loading, unreadCount } = useSelector(state => state.notifications);
  const [filter, setFilter] = useState('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedId, setSelectedId] = useState(null);

  useEffect(() => { dispatch(fetchNotifications()); }, [dispatch]);

  const handleMarkRead = (id) => { dispatch(markAsRead(id)); toast.success('Marked as read'); };
  const handleMarkAllRead = () => { if(window.confirm('Mark all as read?')) { dispatch(markAllAsRead()); toast.success('All marked as read'); } };

  // FIX 20 — Delete notification
  const handleDelete = (id) => {
    dispatch(deleteNotification(id));
    if (selectedId === id) setSelectedId(null);
    toast.success('Notification deleted');
  };

  const filteredItems = (items || []).filter(n => {
    if (!n || !n.message) return false;
    const matchesSearch = n.message.toLowerCase().includes(searchQuery.toLowerCase()) ||
                         (n.title && n.title.toLowerCase().includes(searchQuery.toLowerCase()));
    if (filter === 'broadcasts') return matchesSearch && (n.type === 'SYSTEM_BROADCAST' || n.recipientId === 'ALL');
    if (filter === 'personal') return matchesSearch && n.recipientId !== 'ALL';
    return matchesSearch;
  });

  const selectedMessage = (items || []).find(n => n.id === selectedId) || (filteredItems.length > 0 ? filteredItems[0] : null);

  const getIcon = (type) => {
    switch (type) {
      case 'SYSTEM_BROADCAST': return <Megaphone className="msg-icon-inner" size={20} />;
      case 'ASSIGNMENT': return <User className="msg-icon-inner" size={20} />;
      case 'COMMENT': return <MessageSquare className="msg-icon-inner" size={20} />;
      default: return <Bell className="msg-icon-inner" size={20} />;
    }
  };

  // FIX 19 — navigate based on relatedType
  const handleViewRelated = () => {
    if (!selectedMessage?.relatedId) return;
    const type = selectedMessage.relatedType;
    if (type === 'CARD' || type === 'BOARD') {
      navigate(`/b/${selectedMessage.relatedId}`);
    }
  };

  return (
    <div className="messages-container">
      <div className="messages-layout glass-panel">
        <div className="messages-sidebar">
          <div className="sidebar-header">
            <div className="header-top">
              <h1>Messages</h1>
              {unreadCount > 0 && <span className="unread-badge-large">{unreadCount} New</span>}
            </div>
            <div className="search-box">
              <Search size={16} />
              <input type="text" placeholder="Search messages..." value={searchQuery} onChange={e => setSearchQuery(e.target.value)} />
            </div>
            <div className="filter-chips">
              {['all','broadcasts','personal'].map(f => (
                <button key={f} className={`filter-chip ${filter === f ? 'active' : ''}`} onClick={() => setFilter(f)}>
                  {f === 'all' ? 'All' : f === 'broadcasts' ? 'Announcements' : 'Personal'}
                </button>
              ))}
            </div>
          </div>
          <div className="messages-list">
            {loading ? (
              <div className="list-loading">Loading your inbox...</div>
            ) : filteredItems.length === 0 ? (
              <div className="empty-inbox"><Inbox size={48} /><p>No messages found</p></div>
            ) : filteredItems.map((n, idx) => (
              <div key={n.id || `msg-${idx}`} className={`message-item ${!n.isRead ? 'unread' : ''} ${selectedId === n.id ? 'selected' : ''}`} onClick={() => setSelectedId(n.id)}>
                <div className={`msg-icon ${n.type?.toLowerCase()}`}>{getIcon(n.type)}</div>
                <div className="msg-preview">
                  <div className="msg-preview-header">
                    <span className="msg-title">{n.title}</span>
                    <span className="msg-date">{formatTimeDistance(n.createdAt)}</span>
                  </div>
                  <p className="msg-text">{n.message}</p>
                </div>
                {!n.isRead && <div className="unread-dot"></div>}
              </div>
            ))}
          </div>
          {items.length > 0 && (
            <div className="sidebar-footer">
              <button onClick={handleMarkAllRead} className="btn-link"><CheckCircle size={14} /> Mark all read</button>
            </div>
          )}
        </div>

        <div className="message-view">
          {selectedMessage ? (
            <div className="view-content animate-fade-in">
              <div className="view-header">
                <div className="sender-info">
                  <div className={`sender-avatar ${selectedMessage?.type?.toLowerCase()}`}>
                    {selectedMessage?.recipientId === 'ALL' ? <Shield size={24} /> : <User size={24} />}
                  </div>
                  <div className="sender-details">
                    <h2>{selectedMessage?.title}</h2>
                    <p>From: {selectedMessage?.recipientId === 'ALL' ? 'System Admin' : 'FlowBoard Notifications'}</p>
                  </div>
                </div>
                <div className="view-actions">
                  {!selectedMessage.isRead && (
                    <button className="btn btn-secondary btn-sm" onClick={() => handleMarkRead(selectedMessage.id)}>Mark as Read</button>
                  )}
                  {/* FIX 20 — Trash2 deletes notification */}
                  <button className="btn-icon" onClick={() => handleDelete(selectedMessage.id)} title="Delete notification">
                    <Trash2 size={18} />
                  </button>
                </div>
              </div>
              <div className="view-body">
                <div className="msg-meta">
                  <span><Clock size={14} /> {selectedMessage?.createdAt ? new Date(selectedMessage.createdAt).toLocaleString() : ''}</span>
                  <span><Filter size={14} /> {selectedMessage?.type}</span>
                </div>
                <div className="message-content-box"><p>{selectedMessage?.message}</p></div>
                {selectedMessage?.relatedId && (
                  <div className="related-action glass-panel">
                    <p>This message is related to a {selectedMessage?.relatedType}.</p>
                    {/* FIX 19 — navigate on click */}
                    <button className="btn btn-primary btn-sm" onClick={handleViewRelated}>
                      View {selectedMessage?.relatedType} <ChevronRight size={14} />
                    </button>
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="no-message-selected">
              <Mail size={64} /><h3>Your System Inbox</h3>
              <p>Select a message from the list to view its contents.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Messages;
