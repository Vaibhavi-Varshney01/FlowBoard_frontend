import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import api from '../../api';

export const fetchNotifications = createAsyncThunk('notifications/fetch', async (_, { rejectWithValue }) => {
  try {
    const response = await api.get('notifications/my');
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch notifications');
  }
});

export const markAsRead = createAsyncThunk('notifications/markAsRead', async (id, { rejectWithValue }) => {
  try {
    await api.put(`notifications/${id}/read`);
    return id;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to mark as read');
  }
});

export const markAllAsRead = createAsyncThunk('notifications/markAllAsRead', async (_, { rejectWithValue }) => {
  try {
    await api.put(`notifications/read-all`);
    return true;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to mark all as read');
  }
});

// FIX 20 — Delete notification
export const deleteNotification = createAsyncThunk('notifications/delete', async (id, { rejectWithValue, getState }) => {
  try {
    await api.delete(`notifications/${id}`);
    const item = getState().notifications.items.find(n => n.id === id);
    return { id, wasUnread: item && !item.isRead };
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to delete notification');
  }
});

const notificationSlice = createSlice({
  name: 'notifications',
  initialState: {
    items: [],
    unreadCount: 0,
    loading: false,
    error: null
  },
  reducers: {
    // FIX 21 — Deduplicate SignalR notifications
    addNotification: (state, action) => {
      const alreadyExists = state.items.some(n => n.id === action.payload.id);
      if (alreadyExists) return;
      state.items.unshift(action.payload);
      if (!action.payload.isRead) {
        state.unreadCount += 1;
      }
    },
    clearUnreadCount: (state) => {
      state.unreadCount = 0;
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchNotifications.pending, (state) => { state.loading = true; })
      .addCase(fetchNotifications.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload;
        state.unreadCount = action.payload.filter(n => !n.isRead).length;
      })
      .addCase(markAsRead.fulfilled, (state, action) => {
        const item = state.items.find(n => n.id === action.payload);
        if (item && !item.isRead) {
          item.isRead = true;
          state.unreadCount = Math.max(0, state.unreadCount - 1);
        }
      })
      .addCase(markAllAsRead.fulfilled, (state) => {
        state.items.forEach(n => n.isRead = true);
        state.unreadCount = 0;
      })
      // FIX 20
      .addCase(deleteNotification.fulfilled, (state, action) => {
        state.items = state.items.filter(n => n.id !== action.payload.id);
        if (action.payload.wasUnread) {
          state.unreadCount = Math.max(0, state.unreadCount - 1);
        }
      });
  }
});

export const { addNotification, clearUnreadCount } = notificationSlice.actions;
export default notificationSlice.reducer;
