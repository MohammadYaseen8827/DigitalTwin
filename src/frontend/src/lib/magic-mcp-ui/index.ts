import { createApp, defineComponent, h, TransitionGroup, ref } from 'vue'

type ToastIntent = 'success' | 'error' | 'warning' | 'info'

export interface ToastOptions {
  duration?: number
  id?: string
}

interface ToastItem {
  id: string
  message: string
  intent: ToastIntent
  duration: number
  count: number
}

const DEFAULT_DURATION = 6000
const MIN_DURATION = 5000
const MAX_DURATION = 7000
const toasts = ref<ToastItem[]>([])
let hostMounted = false
let styleInjected = false
const toastTimeouts = new Map<string, number>()

const removeToast = (id: string) => {
  const timeout = toastTimeouts.get(id)
  if (timeout) {
    window.clearTimeout(timeout)
    toastTimeouts.delete(id)
  }
  toasts.value = toasts.value.filter(toast => toast.id !== id)
}

const scheduleRemoval = (id: string, duration: number) => {
  if (typeof window === 'undefined') {
    return
  }

  const existing = toastTimeouts.get(id)
  if (existing) {
    window.clearTimeout(existing)
  }

  const timeoutId = window.setTimeout(() => {
    toastTimeouts.delete(id)
    removeToast(id)
  }, duration)

  toastTimeouts.set(id, timeoutId)
}

const mountHost = () => {
  if (hostMounted || typeof window === 'undefined') {
    return
  }

  hostMounted = true

  const container = document.createElement('div')
  container.id = 'magic-mcp-toast-host'
  document.body.appendChild(container)

  const ToastHost = defineComponent(() => () =>
    h(
      TransitionGroup,
      {
        name: 'magic-toast',
        tag: 'div',
        class: 'magic-toast-stack',
      },
      () =>
        toasts.value.map(toast =>
          h(
            'div',
            {
              key: toast.id,
              class: ['magic-toast', `magic-toast--${toast.intent}`],
              role: 'status',
            },
            [
              h('div', { class: 'magic-toast__content' }, [
                h('span', { class: 'magic-toast__message' }, toast.message),
                toast.count > 1
                  ? h('span', { class: 'magic-toast__badge' }, `×${toast.count}`)
                  : null,
              ]),
            ],
          ),
        ),
    ),
  )

  createApp(ToastHost).mount(container)
  injectStyles()
}

const injectStyles = () => {
  if (styleInjected || typeof document === 'undefined') {
    return
  }

  styleInjected = true

  const style = document.createElement('style')
  style.setAttribute('data-magic-mcp-ui', '')
  style.textContent = `
    .magic-toast-stack {
      position: fixed;
      top: 24px;
      right: 24px;
      display: flex;
      flex-direction: column;
      gap: 12px;
      z-index: 9999;
      pointer-events: none;
    }

    .magic-toast {
      min-width: 240px;
      max-width: 360px;
      padding: 14px 18px;
      border-radius: 14px;
      box-shadow: 0 12px 30px rgba(15, 23, 42, 0.16);
      background: rgba(15, 23, 42, 0.94);
      color: #f8fafc;
      font-size: 0.95rem;
      font-weight: 500;
      pointer-events: auto;
      border: 1px solid rgba(148, 163, 184, 0.25);
      backdrop-filter: blur(18px);
    }

    .magic-toast--success {
      border-color: rgba(34, 197, 94, 0.45);
      box-shadow: 0 12px 30px rgba(34, 197, 94, 0.2);
    }

    .magic-toast--error {
      border-color: rgba(239, 68, 68, 0.45);
      box-shadow: 0 12px 30px rgba(239, 68, 68, 0.2);
    }

    .magic-toast--warning {
      border-color: rgba(234, 179, 8, 0.45);
      box-shadow: 0 12px 30px rgba(234, 179, 8, 0.2);
    }

    .magic-toast--info {
      border-color: rgba(59, 130, 246, 0.45);
      box-shadow: 0 12px 30px rgba(59, 130, 246, 0.2);
    }

    .magic-toast__content {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 12px;
    }

    .magic-toast__message {
      flex: 1;
    }

    .magic-toast__badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 32px;
      padding: 2px 10px;
      border-radius: 999px;
      background: rgba(241, 245, 249, 0.12);
      color: #e2e8f0;
      font-size: 0.82rem;
      font-weight: 600;
    }

    .magic-toast-enter-active,
    .magic-toast-leave-active {
      transition: all 0.26s cubic-bezier(0.4, 0, 0.2, 1);
    }

    .magic-toast-enter-from {
      opacity: 0;
      transform: translateY(-16px) scale(0.96);
    }

    .magic-toast-leave-to {
      opacity: 0;
      transform: translateY(-8px) scale(0.96);
    }
  `

  document.head.appendChild(style)
}

const enqueueToast = (intent: ToastIntent, message: string, options: ToastOptions = {}) => {
  if (typeof window === 'undefined') {
    if (typeof import.meta !== 'undefined' && import.meta.env?.DEV) {
      console.debug(`[${intent.toUpperCase()}]`, message)
    }
    return
  }

  mountHost()

  const id = options.id ?? `toast-${Date.now()}-${Math.random().toString(16).slice(2)}`
  const duration = Math.min(MAX_DURATION, Math.max(MIN_DURATION, options.duration ?? DEFAULT_DURATION))

  const existingToast = toasts.value.find(toast => toast.intent === intent && toast.message === message)
  if (existingToast) {
    existingToast.count += 1
    existingToast.duration = duration
    toasts.value = [...toasts.value]
    scheduleRemoval(existingToast.id, duration)
    return
  }

  removeToast(id)

  toasts.value = [...toasts.value, { id, intent, message, duration, count: 1 }]

  if (toasts.value.length > 3) {
    const overflow = toasts.value.length - 3
    const toRemove = toasts.value.slice(0, overflow)
    toRemove.forEach(toast => removeToast(toast.id))
  }

  scheduleRemoval(id, duration)
}

export function useToast() {
  return {
    success(message: string, options?: ToastOptions) {
      enqueueToast('success', message, options)
    },
    error(message: string, options?: ToastOptions) {
      enqueueToast('error', message, options)
    },
    warning(message: string, options?: ToastOptions) {
      enqueueToast('warning', message, options)
    },
    info(message: string, options?: ToastOptions) {
      enqueueToast('info', message, options)
    },
  }
}
