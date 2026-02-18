import { inject } from 'vue'
import type { ToastT, ToastToDismiss, ExternalToast } from 'vue-sonner'

interface ToastFunctions {
  success: (message: string, opts?: ExternalToast) => string | number
  error: (message: string, opts?: ExternalToast) => string | number
  warning: (message: string, opts?: ExternalToast) => string | number
  info: (message: string, opts?: ExternalToast) => string | number
}

export function useToast(): ToastFunctions {
  const toast = inject<ToastFunctions>('toast')
  
  if (!toast) {
    // Fallback to console logging if toast is not available
    return {
      success: (message: string) => {
        console.log('[SUCCESS]', message)
        return ''
      },
      error: (message: string) => {
        console.error('[ERROR]', message)
        return ''
      },
      warning: (message: string) => {
        console.warn('[WARNING]', message)
        return ''
      },
      info: (message: string) => {
        console.info('[INFO]', message)
        return ''
      }
    }
  }
  
  return toast
}