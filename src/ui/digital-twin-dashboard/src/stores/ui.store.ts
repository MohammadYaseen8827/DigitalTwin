import { defineStore } from 'pinia'
import { ref } from 'vue'

export interface ToastMessage {
  id: string
  type: 'success' | 'error' | 'warning' | 'info'
  message: string
  duration?: number
}

export const useUIStore = defineStore('ui', () => {
  // State
  const sidebarOpen = ref(true)
  const loading = ref(false)
  const toasts = ref<ToastMessage[]>([])
  const activeModal = ref<string | null>(null)
  const theme = ref<'light' | 'dark'>('dark')

  // Actions
  function toggleSidebar() {
    sidebarOpen.value = !sidebarOpen.value
  }

  function setSidebarOpen(open: boolean) {
    sidebarOpen.value = open
  }

  function setLoading(isLoading: boolean) {
    loading.value = isLoading
  }

  function addToast(toast: Omit<ToastMessage, 'id'>) {
    const id = `toast-${Date.now()}-${Math.random()}`
    toasts.value.push({ ...toast, id })
    
    // Auto-remove after duration
    const duration = toast.duration || 5000
    setTimeout(() => {
      removeToast(id)
    }, duration)
  }

  function removeToast(id: string) {
    toasts.value = toasts.value.filter((t: ToastMessage) => t.id !== id)
  }

  function openModal(modalId: string) {
    activeModal.value = modalId
  }

  function closeModal() {
    activeModal.value = null
  }

  function setTheme(newTheme: 'light' | 'dark') {
    theme.value = newTheme
    document.documentElement.setAttribute('data-theme', newTheme)
  }

  function toggleTheme() {
    setTheme(theme.value === 'light' ? 'dark' : 'light')
  }

  function $reset() {
    sidebarOpen.value = true
    loading.value = false
    toasts.value = []
    activeModal.value = null
    theme.value = 'dark'
  }

  return {
    // State
    sidebarOpen,
    loading,
    toasts,
    activeModal,
    theme,
    
    // Actions
    toggleSidebar,
    setSidebarOpen,
    setLoading,
    addToast,
    removeToast,
    openModal,
    closeModal,
    setTheme,
    toggleTheme,
    $reset
  }
})
