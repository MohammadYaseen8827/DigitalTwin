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
    const dev = typeof import.meta !== 'undefined' && import.meta.env?.DEV
    return {
      success: () => '',
      error: (message: string) => {
        if (dev) console.error('[ERROR]', message)
        return ''
      },
      warning: (message: string) => {
        if (dev) console.warn('[WARNING]', message)
        return ''
      },
      info: (message: string) => {
        if (dev) console.info('[INFO]', message)
        return ''
      }
    }
  }
  
  return toast
}