import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AuthState } from './auth.interface';

export const selectAuthState = createFeatureSelector<AuthState>('auth');

export const selectToken = createSelector(
    selectAuthState,
    (state: AuthState) => state.token
);

export const storedDetails = createSelector(
    selectAuthState,
    (state: AuthState) => state
);

export const userDetails = createSelector(
    selectAuthState,
    (state: AuthState) => ({
        token:state.token,
        user: state.user,
        // userName: state.userName,
        // password: state.password, 
    })
);

