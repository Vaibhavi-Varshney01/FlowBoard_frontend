import React from 'react';
import { Droppable, Draggable } from 'react-beautiful-dnd';
import { Trash2, Plus, X, Archive } from 'lucide-react';
import { useDispatch } from 'react-redux';
import { archiveList } from '../store/slices/boardSlice';
import Card from './Card';

const List = ({
  list,
  listId,
  index,
  editingListId,
  setEditingListId,
  tempListTitle,
  setTempListTitle,
  handleUpdateListTitle,
  handleDeleteList,
  addingCardToListId,
  setAddingCardToListId,
  newCardTitle,
  setNewCardTitle,
  handleAddCard,
  setSelectedCard,
  setSelectedListTitle,
  user,
  boardId,
  isAuthenticated = true,
  filteredCards
}) => {
  const dispatch = useDispatch();
  const cardsToRender = filteredCards !== undefined ? filteredCards : list.cards;

  const handleArchiveList = () => {
    if (window.confirm('Archive this list?')) {
      dispatch(archiveList({ listId, boardId }));
    }
  };

  return (
    <Draggable draggableId={listId} index={index} isDragDisabled={!isAuthenticated}>
      {(provided) => (
        <div className="list-wrapper" ref={provided.innerRef} {...provided.draggableProps} {...provided.dragHandleProps}>
          <div className="list-content glass-panel">
            <div className="list-header">
              {editingListId === listId && isAuthenticated ? (
                <input
                  className="list-title-input"
                  value={tempListTitle}
                  onChange={e => setTempListTitle(e.target.value)}
                  onBlur={() => handleUpdateListTitle(listId)}
                  onKeyDown={e => e.key === 'Enter' && handleUpdateListTitle(listId)}
                  autoFocus
                />
              ) : (
                <h3 onClick={() => { if(isAuthenticated) { setEditingListId(listId); setTempListTitle(list.title); } }}>
                  {list.title}
                </h3>
              )}
              {isAuthenticated && (
                <div className="list-actions">
                  {/* FIX 13: Archive list button */}
                  <button className="list-menu-btn" onClick={handleArchiveList} title="Archive List">
                    <Archive size={14} />
                  </button>
                  <button className="list-menu-btn" onClick={() => handleDeleteList(listId)}>
                    <Trash2 size={14} />
                  </button>
                </div>
              )}
            </div>

            <Droppable droppableId={listId.toString()} type="card" isDropDisabled={!isAuthenticated} isCombineEnabled={false} ignoreContainerClipping={false}>
              {(provided) => (
                <div className="cards-list" {...provided.droppableProps} ref={provided.innerRef}>
                  {cardsToRender.map((card, idx) => (
                    <Draggable key={card.id} draggableId={card.id} index={idx} isDragDisabled={!isAuthenticated}>
                      {(provided) => (
                        <Card
                          card={card}
                          user={user}
                          provided={provided}
                          onClick={() => { setSelectedCard(card); setSelectedListTitle(list.title); }}
                        />
                      )}
                    </Draggable>
                  ))}
                  {provided.placeholder}
                </div>
              )}
            </Droppable>

            {isAuthenticated && (
              <div className="list-footer">
                {addingCardToListId === listId ? (
                  <div className="add-card-form">
                    <textarea
                      placeholder="Enter card title..."
                      autoFocus
                      value={newCardTitle}
                      onChange={(e) => setNewCardTitle(e.target.value)}
                      onKeyDown={(e) => e.key === 'Enter' && (e.preventDefault(), handleAddCard(listId))}
                    />
                    <div className="add-card-actions">
                      <button className="btn btn-primary btn-sm" onClick={() => handleAddCard(listId)}>Add Card</button>
                      <button className="btn-icon" onClick={() => setAddingCardToListId(null)}><X size={18} /></button>
                    </div>
                  </div>
                ) : (
                  <button className="add-card-btn" onClick={() => setAddingCardToListId(listId)}>
                    <Plus size={16} /> Add a card
                  </button>
                )}
              </div>
            )}
          </div>
        </div>
      )}
    </Draggable>
  );
};

export default List;
