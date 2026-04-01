// src/app/store/auth/auth.actions.ts
import { createAction, props } from '@ngrx/store';
import { AuthState } from './auth.interface';

export const loginSuccess = createAction(
    '[Auth] Login Success',
    props<{ authData: AuthState }>()
);

export const logout = createAction('[Auth] Logout');