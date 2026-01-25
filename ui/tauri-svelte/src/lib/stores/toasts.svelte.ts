/**
 * Toast notification store using Svelte 5 runes.
 * Provides a simple way to show toast notifications throughout the app.
 */

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
  duration: number;
  dismissible: boolean;
}

interface ToastOptions {
  type?: ToastType;
  duration?: number;
  dismissible?: boolean;
}

// State
let toasts = $state<Toast[]>([]);

// Generate unique ID
function generateId(): string {
  return `toast-${Date.now()}-${Math.random().toString(36).slice(2, 9)}`;
}

/**
 * Add a new toast notification
 */
function addToast(message: string, options: ToastOptions = {}): string {
  const id = generateId();
  const toast: Toast = {
    id,
    message,
    type: options.type ?? 'info',
    duration: options.duration ?? 3000,
    dismissible: options.dismissible ?? true,
  };

  toasts = [...toasts, toast];

  // Auto-dismiss after duration
  if (toast.duration > 0) {
    setTimeout(() => {
      removeToast(id);
    }, toast.duration);
  }

  return id;
}

/**
 * Remove a toast by ID
 */
function removeToast(id: string): void {
  toasts = toasts.filter(t => t.id !== id);
}

/**
 * Clear all toasts
 */
function clearToasts(): void {
  toasts = [];
}

// Convenience methods
function success(message: string, duration?: number): string {
  return addToast(message, { type: 'success', duration });
}

function error(message: string, duration?: number): string {
  return addToast(message, { type: 'error', duration: duration ?? 5000 });
}

function info(message: string, duration?: number): string {
  return addToast(message, { type: 'info', duration });
}

function warning(message: string, duration?: number): string {
  return addToast(message, { type: 'warning', duration: duration ?? 4000 });
}

// Export the store
export function useToasts() {
  return {
    // Getters
    get toasts() { return toasts; },
    
    // Actions
    add: addToast,
    remove: removeToast,
    clear: clearToasts,
    
    // Convenience methods
    success,
    error,
    info,
    warning,
  };
}

// Create a singleton instance
const toastStore = useToasts();
export default toastStore;
