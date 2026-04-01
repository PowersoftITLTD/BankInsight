// src/app/store/auth/auth.reducer.ts
import { createReducer, on } from '@ngrx/store';
import { loginSuccess, logout,} from './auth.actions';
import { AuthState } from './auth.interface';

export const initialState: AuthState = {
  token: '',
  user: '',
//   userName: '',
  // password: '',
};

export const authReducer = createReducer(
  initialState,
  on(loginSuccess, (state, { authData }) => ({
    ...state,
    ...authData,
  })),
  on(logout, () => initialState),
);
