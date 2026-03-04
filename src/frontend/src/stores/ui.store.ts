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
  const sidebarCollapsed = ref(false)
  const sidebarOpen = ref(true) // mobile overlay open
  const loading = ref(false)
  const toasts = ref<ToastMessage[]>([])
  const activeModal = ref<string | null>(null)
  const theme = ref<'light' | 'dark'>('dark')
  const density = ref<'compact' | 'comfortable'>('comfortable')
  const commandPaletteOpen = ref(false)

  // Actions
  function toggleSidebarCollapsed() {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  function setSidebarCollapsed(collapsed: boolean) {
    sidebarCollapsed.value = collapsed
  }

  function toggleSidebar() {
    sidebarOpen.value = !sidebarOpen.value
  }

  function setSidebarOpen(open: boolean) {
    sidebarOpen.value = open
  }

  function openCommandPalette() {
    commandPaletteOpen.value = true
  }

  function closeCommandPalette() {
    commandPaletteOpen.value = false
  }

  function toggleCommandPalette() {
    commandPaletteOpen.value = !commandPaletteOpen.value
  }

  function setLoading(isLoading: boolean) {
    loading.value = isLoading
  }

  function addToast(toast: Omit<ToastMessage, 'id'>) {
    const id = `toast-${Date.now()}-${Math.random()}`
    toasts.value.push({ ...toast, id })
    const duration = toast.duration || 5000
    setTimeout(() => { removeToast(id) }, duration)
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

  function setDensity(newDensity: 'compact' | 'comfortable') {
    density.value = newDensity
    document.documentElement.setAttribute('data-density', newDensity)
  }

  function toggleDensity() {
    setDensity(density.value === 'compact' ? 'comfortable' : 'compact')
  }

  function $reset() {
    sidebarCollapsed.value = false
    sidebarOpen.value = true
    loading.value = false
    toasts.value = []
    activeModal.value = null
    theme.value = 'dark'
    density.value = 'comfortable'
    document.documentElement.setAttribute('data-density', 'comfortable')
    commandPaletteOpen.value = false
  }

  return {
    sidebarCollapsed,
    sidebarOpen,
    loading,
    toasts,
    activeModal,
    theme,
    density,
    commandPaletteOpen,
    toggleSidebarCollapsed,
    setSidebarCollapsed,
    toggleSidebar,
    setSidebarOpen,
    openCommandPalette,
    closeCommandPalette,
    toggleCommandPalette,
    setLoading,
    addToast,
    removeToast,
    openModal,
    closeModal,
    setTheme,
    toggleTheme,
    setDensity,
    toggleDensity,
    $reset
  }
})
