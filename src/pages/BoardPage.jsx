import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import api from '../api';
import { useSelector } from 'react-redux';
import { 
  Plus, MoreHorizontal, X, User, Clock, 
  Tag, AlertTriangle, Shield, Settings, Trash2, Lock
} from 'lucide-react';
import './BoardPage.css';

// --- Sub-Components ---

const Card = ({ card, onClick }) => (
  <div className="simple-card glass-panel" onClick={() => onClick(card)}>
    <div className="card-labels">
      {card.priority === 2 && <span className="priority-badge high">High</span>}
      {card.priority === 1 && <span className="priority-badge med">Medium</span>}
    </div>
    <p className="card-title">{card.title}</p>
    {card.dueDate && (
      <div className="card-footer">
        <Clock size={12} />
        <span>{new Date(card.dueDate).toLocaleDateString()}</span>
      </div>
    )}
  </div>
);

const List = ({ list, cards, onAddCard, onEditCard, onDeleteList, canManageLists }) => {
  const [isAdding, setIsAdding] = useState(false);
  const [title, setTitle] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    if (title.trim()) {
      onAddCard(list.listId, title);
      setTitle('');
      setIsAdding(false);
    }
  };

  return (
    <div className="simple-list glass-panel">
      <div className="list-header">
        <h4>{list.name}</h4>
        {canManageLists && (
          <button className="delete-list-btn" onClick={() => onDeleteList(list.listId)}>
            <Trash2 size={14} />
          </button>
        )}
      </div>
      
      <div className="list-cards">
        {cards?.map(card => (
          <Card key={card.cardId} card={card} onClick={onEditCard} />
        ))}
      </div>

      {isAdding ? (
        <form onSubmit={handleSubmit} className="add-card-form">
          <textarea 
            autoFocus
            placeholder="Enter card title..."
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
          <div className="form-actions">
            <button type="submit" className="btn btn-primary btn-sm">Add Card</button>
            <button type="button" onClick={() => setIsAdding(false)}><X size={16} /></button>
          </div>
        </form>
      ) : (
        <button className="add-card-btn" onClick={() => setIsAdding(true)}>
          <Plus size={16} /> Add a card
        </button>
      )}
    </div>
  );
};

// --- Main Page Component ---

const BoardPage = () => {
  const { boardId } = useParams();
  const { user } = useSelector(state => state.auth);
  const [board, setBoard] = useState(null);
  const [lists, setLists] = useState([]);
  const [cards, setCards] = useState({}); // Mapping listId -> Array of cards
  const [members, setMembers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedCard, setSelectedCard] = useState(null);
  const [showAddList, setShowAddList] = useState(false);
  const [newListTitle, setNewListTitle] = useState('');

  // Permissions
  const canManageLists = user?.role === 'ADMIN' || user?.role === 'SUPER_ADMIN';
  const canEditCards = user?.role === 'MEMBER' || canManageLists;

  useEffect(() => {
    fetchBoardData();
  }, [boardId]);

  const fetchBoardData = async () => {
    setLoading(true);
    try {
      // 1. Fetch Board Info
      const boardRes = await api.get(`boards/${boardId}`);
      setBoard(boardRes.data);

      // 2. Fetch Lists - Fixed Endpoint
      const actualListsRes = await api.get(`lists/board/${boardId}`);
      setLists(actualListsRes.data);

      // 3. Fetch Cards for each list
      const cardsData = {};
      await Promise.all(actualListsRes.data.map(async (list) => {
        const res = await api.get(`cards/list/${list.listId}`);
        cardsData[list.listId] = res.data;
      }));
      setCards(cardsData);

      // 4. Fetch Members
      const membersRes = await api.get(`boards/${boardId}/members`);
      setMembers(membersRes.data);

    } catch (err) {
      console.error("Failed to fetch board data", err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddList = async (e) => {
    e.preventDefault();
    if (!newListTitle.trim()) return;
    try {
      const res = await api.post('lists', { boardId: parseInt(boardId), name: newListTitle, position: lists.length });
      setLists([...lists, res.data]);
      setCards({ ...cards, [res.data.listId]: [] });
      setNewListTitle('');
      setShowAddList(false);
    } catch (err) {
      alert("Failed to add list");
    }
  };

  const handleAddCard = async (listId, title) => {
    try {
      const listCards = cards[listId] || [];
      const res = await api.post('cards', { 
        listId: parseInt(listId), 
        boardId: parseInt(boardId), 
        title,
        position: listCards.length,
        priority: 'LOW',
        status: 'TO_DO',
        createdById: user.id
      });
      setCards({
        ...cards,
        [listId]: [...(cards[listId] || []), res.data]
      });
    } catch (err) {
      alert("Failed to add card");
    }
  };

  const handleDeleteList = async (listId) => {
    if (!window.confirm("Delete this list?")) return;
    try {
      await api.delete(`lists/${listId}`);
      setLists(lists.filter(l => l.listId !== listId));
    } catch (err) {
      alert("Failed to delete list");
    }
  };

  if (loading) return <div className="loading-screen">Loading Board Details...</div>;

  return (
    <div className="board-page-container">
      {/* Top Section */}
      <header className="board-header glass-panel">
        <div className="board-info">
          <div className="title-row">
             <Link to="/dashboard" className="back-link"><ChevronLeft size={20} /></Link>
             <h2>{board?.name}</h2>
          </div>
          <p>{board?.description || 'No description provided'}</p>
          <span className={`visibility-tag ${board?.visibility?.toLowerCase()}`}>
            {board?.visibility === 'PUBLIC' ? <Shield size={14} /> : <Lock size={14} />}
            {board?.visibility}
          </span>
        </div>
        <div className="board-members">
          <div className="member-avatars">
            {members.slice(0, 5).map(m => (
              <div key={m.userId} className="member-avatar" title={m.fullName}>
                {m.fullName?.[0] || '?'}
              </div>
            ))}
            {members.length > 5 && <div className="member-more">+{members.length - 5}</div>}
          </div>
          {canManageLists && <button className="btn btn-secondary btn-sm"><Settings size={14} /> Manage</button>}
        </div>
      </header>

      {/* Main Section - Horizontal Scroll */}
      <main className="board-main">
        <div className="lists-wrapper">
          {lists.map(list => (
            <List 
              key={list.listId} 
              list={list} 
              cards={cards[list.listId]} 
              onAddCard={handleAddCard}
              onEditCard={setSelectedCard}
              onDeleteList={handleDeleteList}
              canManageLists={canManageLists}
            />
          ))}

          {canManageLists && (
            <div className="add-list-container">
              {showAddList ? (
                <form onSubmit={handleAddList} className="add-list-form glass-panel">
                  <input 
                    autoFocus
                    placeholder="List title..."
                    value={newListTitle}
                    onChange={(e) => setNewListTitle(e.target.value)}
                  />
                  <div className="form-actions">
                    <button type="submit" className="btn btn-primary btn-sm">Add</button>
                    <button type="button" onClick={() => setShowAddList(false)}><X size={18} /></button>
                  </div>
                </form>
              ) : (
                <button className="btn-add-list glass-panel" onClick={() => setShowAddList(true)}>
                  <Plus size={18} /> Add another list
                </button>
              )}
            </div>
          )}
        </div>
      </main>

      {/* Card Details Modal */}
      {selectedCard && (
        <div className="simple-modal-overlay" onClick={() => setSelectedCard(null)}>
          <div className="simple-modal glass-panel" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3>{selectedCard.title}</h3>
              <button onClick={() => setSelectedCard(null)}><X /></button>
            </div>
            <div className="modal-body">
              <div className="modal-section">
                <label>Description</label>
                <p>{selectedCard.description || 'No description'}</p>
              </div>
              <div className="modal-row">
                <div className="modal-section">
                  <label>Priority</label>
                  <span className={`priority-badge ${selectedCard.priority === 2 ? 'high' : 'normal'}`}>
                    {selectedCard.priority === 2 ? 'High' : 'Normal'}
                  </span>
                </div>
                <div className="modal-section">
                  <label>Due Date</label>
                  <p>{selectedCard.dueDate ? new Date(selectedCard.dueDate).toLocaleDateString() : 'None'}</p>
                </div>
              </div>
            </div>
            <div className="modal-footer">
              {canEditCards && <button className="btn btn-primary">Edit Card</button>}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

const ChevronLeft = ({ size }) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="m15 18-6-6 6-6"/>
  </svg>
);

export default BoardPage;
