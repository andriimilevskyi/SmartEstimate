import { create } from 'zustand';

interface UiState {
  currencyFormat: string;
  dateFormat: string;
  isSidebarCollapsed: boolean;
  setCurrencyFormat: (currencyFormat: string) => void;
  setDateFormat: (dateFormat: string) => void;
  toggleSidebar: () => void;
}

export const useUiStore = create<UiState>((set) => ({
  currencyFormat: 'UAH',
  dateFormat: 'medium',
  isSidebarCollapsed: false,
  setCurrencyFormat: (currencyFormat) => set({ currencyFormat }),
  setDateFormat: (dateFormat) => set({ dateFormat }),
  toggleSidebar: () => set((state) => ({ isSidebarCollapsed: !state.isSidebarCollapsed })),
}));
