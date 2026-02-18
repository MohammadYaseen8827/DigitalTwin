<script setup lang="ts">
import { ref } from 'vue'

interface Props {
  title?: string
  subtitle?: string
}

withDefaults(defineProps<Props>(), {
  title: 'Digital Twin Platform',
  subtitle: ''
})

const isSearchOpen = ref(false)
const searchQuery = ref('')
const showNotifications = ref(false)
const notificationCount = ref(3)

const notifications = ref([
  { id: 1, type: 'warning', message: 'Machine MT-003 temperature high', time: '5 min ago' },
  { id: 2, type: 'critical', message: 'Machine MT-007 vibration critical', time: '12 min ago' },
  { id: 3, type: 'info', message: 'Prediction model updated', time: '1 hour ago' }
])

const toggleNotifications = () => {
  showNotifications.value = !showNotifications.value
  if (showNotifications.value) {
    notificationCount.value = 0
  }
}
</script>

<template>
  <header class="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6 sticky top-0 z-30">
    <div class="flex items-center gap-4">
      <div>
        <h1 class="text-xl font-semibold text-gray-900">{{ title }}</h1>
        <p v-if="subtitle" class="text-sm text-gray-500">{{ subtitle }}</p>
      </div>
    </div>

    <div class="flex items-center gap-4">
      <div class="relative">
        <button class="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors" @click="isSearchOpen = !isSearchOpen" aria-label="Search">
          <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </button>
        <Transition name="search">
          <div v-if="isSearchOpen" class="absolute right-0 top-full mt-2 w-72">
            <input v-model="searchQuery" type="text" placeholder="Search machines, alerts..." class="w-full px-4 py-2 pl-10 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent" @keydown.esc="isSearchOpen = false" />
            <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
        </Transition>
      </div>

      <div class="relative">
        <button class="relative p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors" @click="toggleNotifications" aria-label="Notifications">
          <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
          </svg>
          <span v-if="notificationCount > 0" class="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs font-medium rounded-full flex items-center justify-center">
            {{ notificationCount }}
          </span>
        </button>

        <Transition name="dropdown">
          <div v-if="showNotifications" class="absolute right-0 top-full mt-2 w-80 bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
            <div class="px-4 py-3 border-b border-gray-200">
              <h3 class="font-semibold text-gray-900">Notifications</h3>
            </div>
            <div class="max-h-80 overflow-y-auto">
              <div v-for="notification in notifications" :key="notification.id" class="px-4 py-3 hover:bg-gray-50 border-b border-gray-100 last:border-0">
                <div class="flex items-start gap-3">
                  <span :class="['w-2 h-2 rounded-full mt-2 flex-shrink-0', notification.type === 'critical' ? 'bg-red-500' : notification.type === 'warning' ? 'bg-yellow-500' : 'bg-blue-500']" />
                  <div class="flex-1 min-w-0">
                    <p class="text-sm text-gray-900">{{ notification.message }}</p>
                    <p class="text-xs text-gray-500 mt-1">{{ notification.time }}</p>
                  </div>
                </div>
              </div>
            </div>
            <div class="px-4 py-3 bg-gray-50 border-t border-gray-200">
              <router-link to="/alerts" class="text-sm text-primary-600 hover:text-primary-700 font-medium">View all notifications</router-link>
            </div>
          </div>
        </Transition>
      </div>

      <div class="relative">
        <button class="flex items-center gap-3 p-1.5 hover:bg-gray-100 rounded-lg transition-colors" aria-label="User menu">
          <div class="w-8 h-8 bg-gray-200 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
          <span class="text-sm font-medium text-gray-700 hidden lg:block">Admin</span>
        </button>
      </div>
    </div>
  </header>
</template>

<style scoped>
.search-enter-active, .search-leave-active { transition: opacity 0.2s, transform 0.2s; }
.search-enter-from, .search-leave-to { opacity: 0; transform: translateY(-10px); }
.dropdown-enter-active, .dropdown-leave-active { transition: opacity 0.2s, transform 0.2s; }
.dropdown-enter-from, .dropdown-leave-to { opacity: 0; transform: translateY(-10px); }
</style>
