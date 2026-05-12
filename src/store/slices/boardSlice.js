import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import api from '../../api';

// Fetch all workspaces and their boards for the Dashboard
export const fetchWorkspaces = createAsyncThunk('board/fetchWorkspaces', async (_, { rejectWithValue }) => {
  try {
    const response = await api.get('workspaces');
    const workspaces = response.data;

    // Aggregate boards for each workspace (Microservices sync)
    const workspacesWithBoards = await Promise.all(workspaces.map(async (ws) => {
      try {
        const wsId = ws.workspaceId || ws.id;
        const boardsRes = await api.get(`boards/workspace/${wsId}`);
        return { ...ws, boards: boardsRes.data };
      } catch (e) {
        return { ...ws, boards: [] };
      }
    }));

    return workspacesWithBoards;
  } catch (error) {
    if (error.response?.status === 401) {
      return rejectWithValue('UNAUTHORIZED');
    }
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch workspaces');
  }
});

// FIX 12 — Edit and delete workspace
export const updateWorkspace = createAsyncThunk('board/updateWorkspace', async ({ workspaceId, updates }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.put(`workspaces/${workspaceId}`, updates);
    dispatch(fetchWorkspaces());
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to update workspace');
  }
});

export const deleteWorkspace = createAsyncThunk('board/deleteWorkspace', async (workspaceId, { rejectWithValue, dispatch }) => {
  try {
    await api.delete(`workspaces/${workspaceId}`);
    dispatch(fetchWorkspaces());
    return workspaceId;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to delete workspace');
  }
});

// Fetch full board details including lists and cards
export const fetchBoardDetails = createAsyncThunk('board/fetchBoardDetails', async (boardId, { rejectWithValue }) => {
  try {
    const [boardRes, listsRes, cardsRes] = await Promise.all([
      api.get(`boards/${boardId}`),
      api.get(`lists/board/${boardId}`),
      api.get(`cards/board/${boardId}`)
    ]);
    
    return {
      board: boardRes.data,
      lists: listsRes.data,
      cards: cardsRes.data
    };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch board details');
  }
});

// Move a card (drag and drop)
export const moveList = createAsyncThunk('board/moveList', async ({ boardId, listId, position }, { rejectWithValue }) => {
  try {
    const response = await api.put(`boards/${boardId}/lists/${listId}/reorder`, { position });
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to reorder list');
  }
});

export const updateBoard = createAsyncThunk('board/updateBoard', async ({ boardId, updates }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.put(`boards/${boardId}`, updates);
    dispatch(fetchBoardDetails(boardId));
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to update board');
  }
});

export const updateList = createAsyncThunk('board/updateList', async ({ boardId, listId, updates }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.put(`lists/${listId}`, updates);
    dispatch(fetchBoardDetails(boardId));
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to update list');
  }
});

export const moveCard = createAsyncThunk('board/moveCard', async ({ cardId, targetListId, position }, { rejectWithValue }) => {
  try {
    const response = await api.put(`cards/${cardId}/move`, { listId: targetListId, position });
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to move card');
  }
});

// Create a new workspace
export const createWorkspace = createAsyncThunk('board/createWorkspace', async (workspaceData, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.post('workspaces', workspaceData);
    dispatch(fetchWorkspaces()); // Refresh workspaces after creation
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to create workspace');
  }
});

// Fetch members for a specific workspace
export const fetchWorkspaceMembers = createAsyncThunk('board/fetchMembers', async (workspaceId, { rejectWithValue }) => {
  try {
    const response = await api.get(`workspaces/${workspaceId}/members`);
    return { workspaceId, members: response.data };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch members');
  }
});

// FIX 3 — Fetch board members
export const fetchBoardMembers = createAsyncThunk('board/fetchBoardMembers', async (boardId, { rejectWithValue }) => {
  try {
    const response = await api.get(`boards/${boardId}/members`);
    return { boardId, members: response.data };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch board members');
  }
});

// FIX 2 — Fetch board labels
export const fetchBoardLabels = createAsyncThunk('board/fetchBoardLabels', async (boardId, { rejectWithValue }) => {
  try {
    const response = await api.get(`labels/board/${boardId}`);
    return { boardId, labels: response.data };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch labels');
  }
});

export const addLabelToCard = createAsyncThunk('board/addLabelToCard', async ({ cardId, labelId }, { rejectWithValue }) => {
  try {
    const response = await api.post(`labels/card/${cardId}/${labelId}`);
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to add label');
  }
});

export const removeLabelFromCard = createAsyncThunk('board/removeLabelFromCard', async ({ cardId, labelId }, { rejectWithValue }) => {
  try {
    await api.delete(`labels/card/${cardId}/${labelId}`);
    return { cardId, labelId };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to remove label');
  }
});

// Create a new board in a workspace — FIX 14: accept visibility param
export const createBoard = createAsyncThunk('board/createBoard', async ({ workspaceId, name, background, visibility }, { rejectWithValue, dispatch, getState }) => {
  try {
    const { user } = getState().auth;
    const response = await api.post('boards', { 
      workspaceId: parseInt(workspaceId), 
      name: name, 
      description: '',
      background: background,
      visibility: visibility || 'PRIVATE',
      createdById: user?.id || '00000000-0000-0000-0000-000000000000'
    });
    dispatch(fetchWorkspaces()); // Refresh dashboard
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to create board');
  }
});

// Create a new list (column) in a board
export const createList = createAsyncThunk('board/createList', async ({ boardId, name, position }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.post('lists', { boardId, name, position });
    dispatch(fetchBoardDetails(boardId)); // Refresh board details
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to create list');
  }
});

// FIX 13 — Archive and unarchive list
export const archiveList = createAsyncThunk('board/archiveList', async ({ listId, boardId }, { rejectWithValue, dispatch }) => {
  try {
    await api.post(`lists/${listId}/archive`);
    dispatch(fetchBoardDetails(boardId));
    return { listId, boardId };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to archive list');
  }
});

export const unarchiveList = createAsyncThunk('board/unarchiveList', async ({ listId, boardId }, { rejectWithValue, dispatch }) => {
  try {
    await api.post(`lists/${listId}/unarchive`);
    dispatch(fetchBoardDetails(boardId));
    return { listId, boardId };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to unarchive list');
  }
});

export const createCard = createAsyncThunk('board/createCard', async ({ listId, boardId, title, position }, { rejectWithValue, dispatch, getState }) => {
  try {
    const { user } = getState().auth;
    const response = await api.post('cards', { 
      listId: parseInt(listId), 
      boardId: parseInt(boardId),
      title, 
      position,
      priority: 'LOW',
      status: 'TO_DO',
      createdById: user?.id
    });
    dispatch(fetchBoardDetails(boardId)); // Refresh board details
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to create card');
  }
});

// Update card details
export const updateCard = createAsyncThunk('board/updateCard', async ({ cardId, boardId, updates }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.put(`cards/${cardId}`, updates);
    dispatch(fetchBoardDetails(boardId));
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to update card');
  }
});

// Delete a card
export const deleteCard = createAsyncThunk('board/deleteCard', async ({ cardId, boardId }, { rejectWithValue, dispatch }) => {
  try {
    await api.delete(`cards/${cardId}`);
    dispatch(fetchBoardDetails(boardId));
    return cardId;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to delete card');
  }
});

// FIX 7 — Archive / Unarchive card
export const archiveCard = createAsyncThunk('board/archiveCard', async ({ cardId, boardId }, { rejectWithValue, dispatch }) => {
  try {
    await api.post(`cards/${cardId}/archive`);
    dispatch(fetchBoardDetails(boardId));
    return { cardId, boardId };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to archive card');
  }
});

export const unarchiveCard = createAsyncThunk('board/unarchiveCard', async ({ cardId, boardId }, { rejectWithValue, dispatch }) => {
  try {
    await api.post(`cards/${cardId}/unarchive`);
    dispatch(fetchBoardDetails(boardId));
    return { cardId, boardId };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to unarchive card');
  }
});

// Delete a list
export const deleteList = createAsyncThunk('board/deleteList', async ({ listId, boardId }, { rejectWithValue, dispatch }) => {
  try {
    await api.delete(`lists/${listId}`);
    dispatch(fetchBoardDetails(boardId));
    return listId;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to delete list');
  }
});

// Fetch card history/activities
export const fetchCardHistory = createAsyncThunk('board/fetchCardHistory', async (cardId, { rejectWithValue }) => {
  try {
    const response = await api.get(`cards/${cardId}/activity`);
    return { cardId, history: response.data };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch card history');
  }
});

// Fetch checklists for a card
export const fetchChecklists = createAsyncThunk('board/fetchChecklists', async (cardId, { rejectWithValue }) => {
  try {
    const response = await api.get(`checklists?cardId=${cardId}`);
    return { cardId, checklists: response.data };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch checklists');
  }
});

// Comments
export const fetchComments = createAsyncThunk('board/fetchComments', async (cardId, { rejectWithValue }) => {
  try {
    const response = await api.get(`comments?cardId=${cardId}`);
    return { cardId, comments: response.data };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch comments');
  }
});

export const addComment = createAsyncThunk('board/addComment', async ({ cardId, text }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.post('comments', { cardId, text });
    dispatch(fetchComments(cardId));
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to add comment');
  }
});

// FIX 9 — Comment reply, edit, delete
export const replyToComment = createAsyncThunk('board/replyToComment', async ({ cardId, parentCommentId, text }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.post('comments', { cardId, text, parentCommentId });
    dispatch(fetchComments(cardId));
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to reply to comment');
  }
});

export const updateComment = createAsyncThunk('board/updateComment', async ({ commentId, cardId, text }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.put(`comments/${commentId}`, { text });
    dispatch(fetchComments(cardId));
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to update comment');
  }
});

export const deleteComment = createAsyncThunk('board/deleteComment', async ({ commentId, cardId }, { rejectWithValue, dispatch }) => {
  try {
    await api.delete(`comments/${commentId}`);
    dispatch(fetchComments(cardId));
    return { commentId, cardId };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to delete comment');
  }
});

export const createChecklist = createAsyncThunk('board/createChecklist', async ({ cardId, title }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.post('checklists', { cardId, title });
    dispatch(fetchChecklists(cardId));
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to create checklist');
  }
});

export const createChecklistItem = createAsyncThunk('board/createChecklistItem', async ({ checklistId, cardId, text }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.post(`checklists/${checklistId}/items`, { text });
    dispatch(fetchChecklists(cardId));
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to add checklist item');
  }
});

export const toggleChecklistItem = createAsyncThunk('board/toggleChecklistItem', async ({ checklistId, itemId, cardId, isCompleted }, { rejectWithValue, dispatch }) => {
  try {
    const response = await api.put(`checklists/${checklistId}/items/${itemId}`, { isCompleted });
    dispatch(fetchChecklists(cardId));
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to toggle item');
  }
});

export const deleteChecklist = createAsyncThunk('board/deleteChecklist', async ({ checklistId, cardId }, { rejectWithValue, dispatch }) => {
  try {
    await api.delete(`checklists/${checklistId}`);
    dispatch(fetchChecklists(cardId));
    return checklistId;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to delete checklist');
  }
});

// FIX 8 — Attachments
export const fetchAttachments = createAsyncThunk('board/fetchAttachments', async (cardId, { rejectWithValue }) => {
  try {
    const response = await api.get(`attachments/card/${cardId}`);
    return { cardId, attachments: response.data };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch attachments');
  }
});

export const uploadAttachment = createAsyncThunk('board/uploadAttachment', async ({ cardId, file }, { rejectWithValue, dispatch }) => {
  try {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('cardId', cardId);
    const response = await api.post('attachments', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    dispatch(fetchAttachments(cardId));
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to upload attachment');
  }
});

export const deleteAttachment = createAsyncThunk('board/deleteAttachment', async ({ attachmentId, cardId }, { rejectWithValue, dispatch }) => {
  try {
    await api.delete(`attachments/${attachmentId}`);
    dispatch(fetchAttachments(cardId));
    return { attachmentId, cardId };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to delete attachment');
  }
});

const initialState = {
  workspaces: [],
  workspaceMembers: {},  // Map of workspaceId -> members list
  boardMembers: {},      // Map of boardId -> members list (FIX 3)
  boardLabels: {},       // Map of boardId -> labels list (FIX 2)
  cardHistory: {},       // Map of cardId -> activities list
  cardChecklists: {},    // Map of cardId -> checklists list
  cardComments: {},      // Map of cardId -> comments list
  cardAttachments: {},   // Map of cardId -> attachments list (FIX 8)
  currentBoard: null,
  loading: false,
  error: null,
};

const boardSlice = createSlice({
  name: 'board',
  initialState,
  reducers: {
    // Optimistic UI updates for drag and drop
    optimisticMoveList: (state, action) => {
      const { listId, newPosition } = action.payload;
      const { listOrder } = state.currentBoard;
      const oldIndex = listOrder.indexOf(listId);
      listOrder.splice(oldIndex, 1);
      listOrder.splice(newPosition, 0, listId);
    },
    optimisticMoveCard: (state, action) => {
      const { cardId, sourceListId, destinationListId, newPosition } = action.payload;
      const sourceList = state.currentBoard.lists[sourceListId];
      const destList = state.currentBoard.lists[destinationListId];
      
      const cardIndex = sourceList.cards.findIndex(c => c.id === cardId);
      const [card] = sourceList.cards.splice(cardIndex, 1);
      
      destList.cards.splice(newPosition, 0, card);
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchWorkspaces.pending, (state) => { state.loading = true; })
      .addCase(fetchWorkspaces.fulfilled, (state, action) => {
        state.loading = false;
        state.workspaces = action.payload;
      })
      .addCase(fetchWorkspaces.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchBoardDetails.pending, (state) => { state.loading = true; })
      .addCase(fetchBoardDetails.fulfilled, (state, action) => {
        state.loading = false;
        
        // Transform flat lists and cards into structured format for react-beautiful-dnd
        const listsMap = {};
        const listOrder = [];
        
        action.payload.lists.sort((a,b) => a.position - b.position).forEach(list => {
          listsMap[list.listId] = {
            id: list.listId.toString(),
            title: list.name,
            cards: []
          };
          listOrder.push(list.listId.toString());
        });

        action.payload.cards.sort((a,b) => a.position - b.position).forEach(card => {
          if (listsMap[card.listId]) {
            listsMap[card.listId].cards.push({
              id: card.cardId.toString(),
              content: card.title,
              description: card.description,
              labels: card.labels || [],
              date: card.dueDate,
              startDate: card.startDate,
              dueDate: card.dueDate,
              status: card.status,
              priority: card.priority,
              assigneeId: card.assigneeId,
              assigneeName: card.assigneeName,
              coverColor: card.coverColor,
              hasDesc: !!card.description,
              hasChecklist: card.checklists?.length > 0
            });
          }
        });

        state.currentBoard = {
          info: action.payload.board,
          lists: listsMap,
          listOrder: listOrder
        };
      })
      .addCase(fetchBoardDetails.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchWorkspaceMembers.fulfilled, (state, action) => {
        state.workspaceMembers[action.payload.workspaceId] = action.payload.members;
      })
      .addCase(fetchBoardMembers.fulfilled, (state, action) => {
        state.boardMembers[action.payload.boardId] = action.payload.members;
      })
      .addCase(fetchBoardLabels.fulfilled, (state, action) => {
        state.boardLabels[action.payload.boardId] = action.payload.labels;
      })
      .addCase(fetchCardHistory.fulfilled, (state, action) => {
        state.cardHistory[action.payload.cardId] = action.payload.history;
      })
      .addCase(fetchChecklists.fulfilled, (state, action) => {
        state.cardChecklists[action.payload.cardId] = action.payload.checklists;
      })
      .addCase(fetchComments.fulfilled, (state, action) => {
        state.cardComments[action.payload.cardId] = action.payload.comments;
      })
      .addCase(fetchAttachments.fulfilled, (state, action) => {
        state.cardAttachments[action.payload.cardId] = action.payload.attachments;
      })
      .addCase(moveList.fulfilled, () => {
        // Handled optimistically
      });
  }
});

export const { optimisticMoveList, optimisticMoveCard } = boardSlice.actions;
export default boardSlice.reducer;
