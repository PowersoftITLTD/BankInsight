// src/app/store/meta-reducers/local-storage-meta.reducer.ts
import { ActionReducer, MetaReducer } from '@ngrx/store';

// Meta-reducer for syncing state to localStorage
export function localStorageSyncReducer(reducer: ActionReducer<any>): ActionReducer<any> {
  return function (state, action) {
    const nextState = reducer(state, action);

    // Specify the slices of state to persist
    const persistState = { auth: nextState.auth }; 
    localStorage.setItem('appStateBank', JSON.stringify(persistState));

    return nextState;
  };
}

// Rehydrate the state from localStorage
export function rehydrateState(): any {
  const storedState = localStorage.getItem('appStateBank');
  return storedState ? JSON.parse(storedState) : undefined;
}
