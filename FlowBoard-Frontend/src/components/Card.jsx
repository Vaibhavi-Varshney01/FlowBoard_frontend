import React from 'react';
import { AlignLeft, CheckSquare, Calendar, AlertCircle } from 'lucide-react';

const Card = ({ card, user, provided, onClick }) => {
  const today = new Date();
  today.setHours(0,0,0,0);
  const dueDate = card.dueDate ? new Date(card.dueDate) : null;
  const isOverdue = dueDate && dueDate < today && card.status !== 'DONE';

  return (
    <div
      className="card-item glass-panel"
      ref={provided.innerRef}
      {...provided.draggableProps}
      {...provided.dragHandleProps}
      onClick={onClick}
    >
      {/* FIX 5: Cover color bar */}
      {card.coverColor && (
        <div style={{ height: '5px', background: card.coverColor, borderRadius: '6px 6px 0 0', margin: '-0.75rem -0.75rem 0.5rem -0.75rem' }} />
      )}

      <div className="card-labels">
        {card.labels?.map((label, lidx) => (
          <span key={lidx} className="label-pill" style={{ background: label.color }}></span>
        ))}
      </div>
      <p className="card-text">{card.content}</p>
      <div className="card-footer">
        <div className="card-meta">
          {card.hasDesc && <AlignLeft size={12} />}
          {card.hasChecklist && <CheckSquare size={12} />}
          {/* FIX 4: Due date on card face */}
          {dueDate && (
            <div className={`card-date-badge ${isOverdue ? 'overdue' : ''}`}>
              <Calendar size={10} />
              {dueDate.toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}
            </div>
          )}
          {/* FIX 4: Overdue badge */}
          {isOverdue && (
            <div className="overdue-badge">
              <AlertCircle size={10} /> Overdue
            </div>
          )}
        </div>
        <div className="card-members">
          <div className="avatar-xs">{(user?.username?.charAt(0) || 'U')}</div>
        </div>
      </div>
    </div>
  );
};

export default Card;
