import React, { useEffect, useState, useRef } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { DragDropContext, Droppable } from 'react-beautiful-dnd';
import List from '../components/List';
import {
  Plus, AlertCircle, X, ChevronLeft, Star, Filter,
  Settings, Users, Share2, RotateCcw, ChevronDown, ChevronRight, User
} from 'lucide-react';
import CardModal from '../components/CardModal';
import BoardMembersModal from '../components/modals/BoardMembersModal';
import {
  fetchBoardDetails, moveCard, moveList, optimisticMoveCard, optimisticMoveList,
  createList, createCard, deleteList, updateBoard, updateList,
  unarchiveCard, unarchiveList, fetchBoardLabels, fetchBoardMembers
} from '../store/slices/boardSlice';
import api from '../api';
import './Board.css';

const Board = () => {
  const { boardId } = useParams();
  const dispatch = useDispatch();
  const { currentBoard, loading, error, boardLabels, boardMembers } = useSelector(state => state.board);
  const { user, isAuthenticated } = useSelector(state => state.auth);

  const [selectedCard, setSelectedCard] = useState(null);
  const [selectedListTitle, setSelectedListTitle] = useState('');
  const [addingList, setAddingList] = useState(false);
  const [newListTitle, setNewListTitle] = useState('');
  const [addingCardToListId, setAddingCardToListId] = useState(null);
  const [newCardTitle, setNewCardTitle] = useState('');
  const [showBoardMenu, setShowBoardMenu] = useState(false);
  const [showMembersModal, setShowMembersModal] = useState(false);
  const [editingBoardTitle, setEditingBoardTitle] = useState(false);
  const [tempBoardTitle, setTempBoardTitle] = useState('');
  const [editingListId, setEditingListId] = useState(null);
  const [tempListTitle, setTempListTitle] = useState('');

  // FIX 7 — Archived cards
  const [archivedCards, setArchivedCards] = useState([]);
  const [showArchivedCards, setShowArchivedCards] = useState(false);
  const [archivedCardsLoading, setArchivedCardsLoading] = useState(false);

  // FIX 13 — Archived lists
  const [archivedLists, setArchivedLists] = useState([]);
  const [showArchivedLists, setShowArchivedLists] = useState(false);

  // FIX 25 — Filter bar
  const [showFilter, setShowFilter] = useState(false);
  const [filterAssignee, setFilterAssignee] = useState('');
  const [filterLabels, setFilterLabels] = useState([]);
  const [filterPriority, setFilterPriority] = useState('');
  const [filterDueFrom, setFilterDueFrom] = useState('');
  const [filterDueTo, setFilterDueTo] = useState('');

  const members = boardMembers[boardId] || [];
  const labels = boardLabels[boardId] || [];

  useEffect(() => {
    if (boardId) {
      dispatch(fetchBoardDetails(boardId));
      dispatch(fetchBoardLabels(boardId));
      dispatch(fetchBoardMembers(boardId));
    }
  }, [dispatch, boardId]);

  useEffect(() => {
    if (currentBoard?.info) setTempBoardTitle(currentBoard.info.name);
  }, [currentBoard]);

  const fetchArchivedCards = async () => {
    setArchivedCardsLoading(true);
    try {
      const res = await api.get(`cards/board/${boardId}?archived=true`);
      setArchivedCards(res.data || []);
    } catch { setArchivedCards([]); }
    finally { setArchivedCardsLoading(false); }
  };

  const fetchArchivedLists = async () => {
    try {
      const res = await api.get(`lists/board/${boardId}?archived=true`);
      setArchivedLists(res.data || []);
    } catch { setArchivedLists([]); }
  };

  const handleToggleArchivedCards = () => {
    if (!showArchivedCards) fetchArchivedCards();
    setShowArchivedCards(v => !v);
  };

  const handleToggleArchivedLists = () => {
    if (!showArchivedLists) fetchArchivedLists();
    setShowArchivedLists(v => !v);
  };

  const handleRestoreCard = (cardId) => {
    dispatch(unarchiveCard({ cardId, boardId }));
    setArchivedCards(prev => prev.filter(c => (c.cardId || c.id).toString() !== cardId.toString()));
  };

  const handleRestoreList = (listId) => {
    dispatch(unarchiveList({ listId, boardId }));
    setArchivedLists(prev => prev.filter(l => (l.listId || l.id).toString() !== listId.toString()));
  };

  const handleUpdateBoardTitle = () => {
    if (tempBoardTitle.trim() && tempBoardTitle !== currentBoard.info.name)
      dispatch(updateBoard({ boardId, updates: { name: tempBoardTitle } }));
    setEditingBoardTitle(false);
  };

  const handleUpdateListTitle = (listId) => {
    if (tempListTitle.trim()) dispatch(updateList({ boardId, listId, updates: { name: tempListTitle } }));
    setEditingListId(null);
  };

  const handleAddList = (e) => {
    e.preventDefault();
    if (newListTitle.trim()) {
      dispatch(createList({ boardId, name: newListTitle, position: currentBoard.listOrder.length }));
      setNewListTitle(''); setAddingList(false);
    }
  };

  const handleAddCard = (listId) => {
    if (newCardTitle.trim()) {
      const list = currentBoard.lists[listId];
      dispatch(createCard({ listId, boardId, title: newCardTitle, position: list.cards.length }));
      setNewCardTitle(''); setAddingCardToListId(null);
    }
  };

  const handleDeleteList = (listId) => {
    if (window.confirm('Delete this list and all its cards?')) dispatch(deleteList({ listId, boardId }));
  };

  const onDragEnd = result => {
    if (!isAuthenticated) return;
    const { destination, source, draggableId, type } = result;
    if (!destination) return;
    if (destination.droppableId === source.droppableId && destination.index === source.index) return;
    if (type === 'list') {
      dispatch(optimisticMoveList({ listId: draggableId, newPosition: destination.index }));
      dispatch(moveList({ boardId, listId: draggableId, position: destination.index }));
      return;
    }
    dispatch(optimisticMoveCard({ cardId: draggableId, sourceListId: source.droppableId, destinationListId: destination.droppableId, newPosition: destination.index }));
    dispatch(moveCard({ cardId: draggableId, targetListId: destination.droppableId, position: destination.index }));
  };

  // FIX 25 — Client-side filter
  const clearFilters = () => { setFilterAssignee(''); setFilterLabels([]); setFilterPriority(''); setFilterDueFrom(''); setFilterDueTo(''); };
  const hasFilters = filterAssignee || filterLabels.length || filterPriority || filterDueFrom || filterDueTo;

  const getFilteredCards = (cards) => {
    if (!hasFilters) return undefined; // use default
    return cards.filter(card => {
      if (filterAssignee && card.assigneeId?.toString() !== filterAssignee) return false;
      if (filterPriority && card.priority !== filterPriority) return false;
      if (filterLabels.length && !filterLabels.every(lId => card.labels?.some(l => l.id?.toString() === lId))) return false;
      if (filterDueFrom && card.dueDate && new Date(card.dueDate) < new Date(filterDueFrom)) return false;
      if (filterDueTo && card.dueDate && new Date(card.dueDate) > new Date(filterDueTo)) return false;
      return true;
    });
  };

  if (loading && !currentBoard) return <div className="board-loading"><div className="spinner"></div>Loading Board...</div>;
  if (error) return <div className="board-error"><AlertCircle size={48} /><h3>{error}</h3><Link to="/dashboard" className="btn btn-primary">Back to Dashboard</Link></div>;
  if (!currentBoard) return <div className="board-not-found">Board not found</div>;

  return (
    <div className="board-view" style={{ background: currentBoard.info?.background || 'var(--bg-primary)' }}>
      {/* FIX 24 — Guest banner */}
      {!isAuthenticated && (
        <div style={{ background:'#f59e0b', color:'#1e1e1e', textAlign:'center', padding:'0.6rem 1rem', fontWeight:600, fontSize:'0.9rem', position:'sticky', top:0, zIndex:100 }}>
          You are viewing a public board. <Link to="/login" style={{ color:'#1e293b', textDecoration:'underline' }}>Log in to collaborate.</Link>
        </div>
      )}

      <div className="board-navbar glass-panel">
        <div className="board-nav-left">
          <Link to="/dashboard" className="btn-icon"><ChevronLeft size={20} /></Link>
          <div className="board-nav-title">
            {editingBoardTitle && isAuthenticated ? (
              <input className="board-title-input" value={tempBoardTitle} onChange={e => setTempBoardTitle(e.target.value)} onBlur={handleUpdateBoardTitle} onKeyDown={e => e.key === 'Enter' && handleUpdateBoardTitle()} autoFocus />
            ) : (
              <h2 onClick={() => isAuthenticated && setEditingBoardTitle(true)}>{currentBoard.info?.name}</h2>
            )}
            <button className="btn-icon star-btn"><Star size={18} /></button>
          </div>
          <div className="board-nav-sep"></div>
          <div className="workspace-badge">{currentBoard.info?.workspaceName || 'Workspace'}</div>
        </div>
        <div className="board-nav-right">
          {/* FIX 10 — Members button opens BoardMembersModal */}
          {isAuthenticated && (
            <button className="btn btn-secondary btn-sm" onClick={() => setShowMembersModal(true)}><Users size={16} /> Members</button>
          )}
          {/* FIX 25 — Filter toggle */}
          <button className={`btn btn-secondary btn-sm ${showFilter ? 'active' : ''}`} onClick={() => setShowFilter(v => !v)}><Filter size={16} /> Filter{hasFilters ? ' ●' : ''}</button>
          <button className="btn btn-secondary btn-sm" onClick={() => setShowBoardMenu(true)}><Settings size={16} /> Board Menu</button>
        </div>
      </div>

      {/* FIX 25 — Filter bar */}
      {showFilter && (
        <div className="filter-bar glass-panel" style={{ display:'flex', gap:'0.75rem', alignItems:'center', padding:'0.75rem 1.25rem', flexWrap:'wrap', margin:'0.5rem 1rem', borderRadius:10 }}>
          <select value={filterAssignee} onChange={e => setFilterAssignee(e.target.value)} style={{ background:'var(--bg-tertiary)', color:'var(--text-primary)', border:'1px solid var(--border-color)', borderRadius:6, padding:'0.3rem 0.5rem', fontSize:'0.82rem' }}>
            <option value="">All Assignees</option>
            {members.map(m => { const id = m.userId||m.id; return <option key={id} value={id}>{m.fullName||m.username||id}</option>; })}
          </select>
          <select value={filterPriority} onChange={e => setFilterPriority(e.target.value)} style={{ background:'var(--bg-tertiary)', color:'var(--text-primary)', border:'1px solid var(--border-color)', borderRadius:6, padding:'0.3rem 0.5rem', fontSize:'0.82rem' }}>
            <option value="">All Priorities</option>
            {['LOW','MEDIUM','HIGH','CRITICAL'].map(p => <option key={p} value={p}>{p}</option>)}
          </select>
          <div style={{ display:'flex', gap:'0.4rem', alignItems:'center' }}>
            {labels.map(lbl => (
              <label key={lbl.id} style={{ display:'flex', alignItems:'center', gap:'0.25rem', cursor:'pointer', fontSize:'0.8rem' }}>
                <input type="checkbox" checked={filterLabels.includes(lbl.id?.toString())}
                  onChange={e => setFilterLabels(prev => e.target.checked ? [...prev, lbl.id?.toString()] : prev.filter(x => x !== lbl.id?.toString()))} />
                <span style={{ width:10, height:10, borderRadius:3, background:lbl.color }} />
                {lbl.name}
              </label>
            ))}
          </div>
          <input type="date" placeholder="Due from" value={filterDueFrom} onChange={e => setFilterDueFrom(e.target.value)} style={{ background:'var(--bg-tertiary)', color:'var(--text-primary)', border:'1px solid var(--border-color)', borderRadius:6, padding:'0.3rem 0.5rem', fontSize:'0.82rem' }} />
          <input type="date" placeholder="Due to" value={filterDueTo} onChange={e => setFilterDueTo(e.target.value)} style={{ background:'var(--bg-tertiary)', color:'var(--text-primary)', border:'1px solid var(--border-color)', borderRadius:6, padding:'0.3rem 0.5rem', fontSize:'0.82rem' }} />
          {hasFilters && <button className="btn btn-secondary btn-sm" onClick={clearFilters}>Clear Filters</button>}
        </div>
      )}

      {/* Board Sidebar Menu */}
      {showBoardMenu && (
        <div className="board-sidebar-overlay" onClick={() => setShowBoardMenu(false)}>
          <div className="board-sidebar-menu glass-panel" onClick={e => e.stopPropagation()}>
            <div className="sidebar-menu-header">
              <h3>Menu</h3>
              <button className="btn-icon" onClick={() => setShowBoardMenu(false)}><X size={20} /></button>
            </div>
            <div className="sidebar-menu-content">
              <div className="sidebar-section">
                <h4>About this board</h4>
                <div className="admin-info-item"><User size={16} /><span>Created by <strong>{currentBoard.info?.creatorName || 'Admin'}</strong></span></div>
              </div>
              {isAuthenticated && (
                <div className="sidebar-section">
                  <h4>Change Background</h4>
                  <div className="background-grid">
                    {['#6366f1','#ec4899','#f59e0b','#10b981','#3b82f6','#ef4444'].map(color => (
                      <div key={color} className={`bg-option ${currentBoard.info?.background === color ? 'active' : ''}`} style={{ background: color }} onClick={() => dispatch(updateBoard({ boardId, updates: { background: color } }))}></div>
                    ))}
                  </div>
                </div>
              )}
              {/* FIX 7 — Archived cards toggle */}
              <div className="sidebar-section">
                <button className="btn btn-secondary btn-sm" onClick={handleToggleArchivedCards} style={{ width:'100%', justifyContent:'space-between' }}>
                  Archived Cards {showArchivedCards ? <ChevronDown size={14}/> : <ChevronRight size={14}/>}
                </button>
                {showArchivedCards && (
                  archivedCardsLoading ? <p style={{color:'var(--text-muted)',fontSize:'0.8rem',padding:'0.5rem'}}>Loading...</p> :
                  archivedCards.length === 0 ? <p style={{color:'var(--text-muted)',fontSize:'0.8rem',padding:'0.5rem'}}>No archived cards</p> :
                  archivedCards.map(c => {
                    const id = (c.cardId || c.id).toString();
                    return (
                      <div key={id} style={{ display:'flex', justifyContent:'space-between', alignItems:'center', padding:'0.4rem 0', borderBottom:'1px solid var(--border-color)', fontSize:'0.82rem' }}>
                        <span>{c.title}</span>
                        {isAuthenticated && <button className="btn btn-secondary btn-xs" onClick={() => handleRestoreCard(id)}><RotateCcw size={12}/> Restore</button>}
                      </div>
                    );
                  })
                )}
              </div>
              {/* FIX 13 — Archived lists toggle */}
              <div className="sidebar-section">
                <button className="btn btn-secondary btn-sm" onClick={handleToggleArchivedLists} style={{ width:'100%', justifyContent:'space-between' }}>
                  Archived Lists {showArchivedLists ? <ChevronDown size={14}/> : <ChevronRight size={14}/>}
                </button>
                {showArchivedLists && (
                  archivedLists.length === 0 ? <p style={{color:'var(--text-muted)',fontSize:'0.8rem',padding:'0.5rem'}}>No archived lists</p> :
                  archivedLists.map(l => {
                    const id = (l.listId || l.id).toString();
                    return (
                      <div key={id} style={{ display:'flex', justifyContent:'space-between', alignItems:'center', padding:'0.4rem 0', borderBottom:'1px solid var(--border-color)', fontSize:'0.82rem' }}>
                        <span>{l.name || l.title}</span>
                        {isAuthenticated && <button className="btn btn-secondary btn-xs" onClick={() => handleRestoreList(id)}><RotateCcw size={12}/> Restore</button>}
                      </div>
                    );
                  })
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      <div className="board-canvas">
        <DragDropContext onDragEnd={onDragEnd}>
          <Droppable droppableId="board" type="list" direction="horizontal" isDropDisabled={!isAuthenticated} isCombineEnabled={false} ignoreContainerClipping={false}>
            {(provided) => (
              <div className="lists-container" {...provided.droppableProps} ref={provided.innerRef}>
                {currentBoard.listOrder.map((listId, index) => {
                  const list = currentBoard.lists[listId];
                  const filteredCards = getFilteredCards(list.cards);
                  return (
                    <List
                      key={listId}
                      list={list}
                      listId={listId}
                      index={index}
                      boardId={boardId}
                      editingListId={editingListId}
                      setEditingListId={setEditingListId}
                      tempListTitle={tempListTitle}
                      setTempListTitle={setTempListTitle}
                      handleUpdateListTitle={handleUpdateListTitle}
                      handleDeleteList={handleDeleteList}
                      addingCardToListId={addingCardToListId}
                      setAddingCardToListId={setAddingCardToListId}
                      newCardTitle={newCardTitle}
                      setNewCardTitle={setNewCardTitle}
                      handleAddCard={handleAddCard}
                      setSelectedCard={setSelectedCard}
                      setSelectedListTitle={setSelectedListTitle}
                      user={user}
                      isAuthenticated={isAuthenticated}
                      filteredCards={filteredCards}
                    />
                  );
                })}
                {provided.placeholder}

                {isAuthenticated && (
                  <div className="add-list-wrapper">
                    {addingList ? (
                      <form className="add-list-form glass-panel" onSubmit={handleAddList}>
                        <input placeholder="Enter list title..." autoFocus value={newListTitle} onChange={e => setNewListTitle(e.target.value)} />
                        <div className="add-list-actions">
                          <button type="submit" className="btn btn-primary btn-sm">Add List</button>
                          <button type="button" className="btn-icon" onClick={() => setAddingList(false)}><X size={18} /></button>
                        </div>
                      </form>
                    ) : (
                      <button className="add-list-btn glass-panel" onClick={() => setAddingList(true)}><Plus size={18} /> Add another list</button>
                    )}
                  </div>
                )}
              </div>
            )}
          </Droppable>
        </DragDropContext>
      </div>

      {selectedCard && (
        <CardModal card={selectedCard} listTitle={selectedListTitle} boardId={boardId} onClose={() => setSelectedCard(null)} />
      )}
      {/* FIX 10 — BoardMembersModal */}
      {showMembersModal && (
        <BoardMembersModal boardId={boardId} boardName={currentBoard.info?.name} onClose={() => setShowMembersModal(false)} />
      )}
    </div>
  );
};

export default Board;
