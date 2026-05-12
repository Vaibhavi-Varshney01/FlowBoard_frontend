import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { jwtDecode } from 'jwt-decode';
import api from '../../api';

export const loginUser = createAsyncThunk('auth/login', async (credentials, { rejectWithValue }) => {
  try {
    const response = await api.post('auth/login', credentials);
    localStorage.setItem('token', response.data.token);
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Login failed');
  }
});

export const registerUser = createAsyncThunk('auth/register', async (userData, { rejectWithValue }) => {
  try {
    const response = await api.post('auth/register', userData);
    localStorage.setItem('token', response.data.token);
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Registration failed');
  }
});

export const oauthLogin = createAsyncThunk('auth/oauth', async (oauthData, { rejectWithValue }) => {
  try {
    const pendingRole = localStorage.getItem('pendingRole');
    const response = await api.post('auth/oauth', { ...oauthData, role: pendingRole });
    localStorage.removeItem('pendingRole');
    localStorage.setItem('token', response.data.token);
    return response.data;
  } catch (error) {
    console.error('OAuth sync error:', error);
    const message = error.response?.data?.message || error.response?.data || error.message || 'OAuth login failed';
    return rejectWithValue(message);
  }
});

export const fetchUsers = createAsyncThunk('auth/fetchUsers', async (_, { rejectWithValue }) => {
  try {
    const response = await api.get('auth/users');
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to fetch users');
  }
});

export const deactivateAccount = createAsyncThunk('auth/deactivate', async (_, { rejectWithValue, dispatch }) => {
  try {
    await api.post('auth/deactivate');
    dispatch(logout());
    return true;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Failed to deactivate account');
  }
});

export const updateProfile = createAsyncThunk('auth/updateProfile', async (profileData, { rejectWithValue }) => {
  try {
    const response = await api.put('auth/profile', profileData);
    return response.data;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Update failed');
  }
});

// FIX 22 — Change password
export const changePassword = createAsyncThunk('auth/changePassword', async ({ currentPassword, newPassword }, { rejectWithValue }) => {
  try {
    await api.post('auth/password', { currentPassword, newPassword });
    return true;
  } catch (error) {
    return rejectWithValue(error.response?.data?.message || 'Password change failed');
  }
});

const initialState = {
  user: null,
  users: [],
  isAuthenticated: !!localStorage.getItem('token'),
  token: localStorage.getItem('token') || null,
  loading: false,
  error: null,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout: (state) => {
      localStorage.removeItem('token');
      state.user = null;
      state.token = null;
      state.isAuthenticated = false;
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(loginUser.pending, (state) => { state.loading = true; state.error = null; })
      .addCase(loginUser.fulfilled, (state, action) => {
        state.loading = false;
        state.token = action.payload.token;
        
        let userId = action.payload.id;
        let decodedRole = action.payload.role;
        try {
          const decoded = jwtDecode(action.payload.token);
          userId = userId || decoded.sub || decoded.userId || decoded.nameid || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
          decodedRole = decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decodedRole;
        } catch (e) {}

        state.user = {
          id: userId,
          username: action.payload.userName || action.payload.username,
          email: action.payload.email,
          role: decodedRole || 'MEMBER',
          avatarUrl: action.payload.avatarUrl
        };
        state.isAuthenticated = true;
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(registerUser.pending, (state) => { state.loading = true; state.error = null; })
      .addCase(registerUser.fulfilled, (state, action) => {
        state.loading = false;
        state.token = action.payload.token;
        
        let userId = action.payload.id;
        let decodedRole = action.payload.role;
        try {
          const decoded = jwtDecode(action.payload.token);
          userId = userId || decoded.sub || decoded.userId || decoded.nameid || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
          decodedRole = decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decodedRole;
        } catch (e) {}

        state.user = {
          id: userId,
          username: action.payload.userName || action.payload.username,
          email: action.payload.email,
          role: decodedRole || 'MEMBER',
          avatarUrl: action.payload.avatarUrl
        };
        state.isAuthenticated = true;
      })
      .addCase(registerUser.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(oauthLogin.pending, (state) => { state.loading = true; state.error = null; })
      .addCase(oauthLogin.fulfilled, (state, action) => {
        state.loading = false;
        state.token = action.payload.token;
        
        let userId = action.payload.id;
        let decodedRole = action.payload.role;
        try {
          const decoded = jwtDecode(action.payload.token);
          userId = userId || decoded.sub || decoded.userId || decoded.nameid || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
          decodedRole = decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decodedRole;
        } catch (e) {}

        state.user = {
          id: userId,
          username: action.payload.userName || action.payload.username,
          email: action.payload.email,
          role: decodedRole || 'MEMBER',
          avatarUrl: action.payload.avatarUrl
        };
        state.isAuthenticated = true;
      })
      .addCase(oauthLogin.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      })
      .addCase(fetchUsers.fulfilled, (state, action) => {
        state.users = action.payload;
      })
      .addCase(updateProfile.fulfilled, (state, action) => {
        state.user = {
          ...state.user,
          username: action.payload.userName || action.payload.username,
          avatarUrl: action.payload.avatarUrl
        };
      });
  }
});

export const { logout } = authSlice.actions;
export default authSlice.reducer;
