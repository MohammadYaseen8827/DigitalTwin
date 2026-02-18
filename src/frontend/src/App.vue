<script setup lang="ts">
import { onMounted, onUnmounted, computed } from 'vue'
import { RouterView, RouterLink, useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import SessionTimeoutWarning from '@/components/auth/SessionTimeoutWarning.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// Check if current route is auth layout
const isAuthRoute = computed(() => {
    return route.meta.layout === 'auth'
})

// Mobile menu state
const isMobileMenuOpen = ref(false)
const isProfileDropdownOpen = ref(false)

// Navigation items
const navItems = [
    { name: 'Dashboard', path: '/dashboard', icon: 'home' },
    { name: 'Machines', path: '/machines', icon: 'cpu' },
    { name: 'Predictions', path: '/predictions', icon: 'chart' },
    { name: 'Alerts', path: '/alerts', icon: 'bell' },
    { name: 'Settings', path: '/settings', icon: 'settings' },
]

// Get icon path
function getIconPath(icon: string) {
    const icons: Record<string, string> = {
        home: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6',
        cpu: 'M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z',
        chart: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z',
        bell: 'M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9',
        settings: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z',
    }
    return icons[icon] || icons.home
}

// Toggle mobile menu
function toggleMobileMenu() {
    isMobileMenuOpen.value = !isMobileMenuOpen.value
}

// Toggle profile dropdown
function toggleProfileDropdown() {
    isProfileDropdownOpen.value = !isProfileDropdownOpen.value
}

// Close dropdowns on outside click
function handleClickOutside(event: MouseEvent) {
    const target = event.target as HTMLElement
    if (!target.closest('[data-profile-dropdown]')) {
        isProfileDropdownOpen.value = false
    }
}

// Handle logout
function handleLogout() {
    authStore.logout()
    isProfileDropdownOpen.value = false
}

// Initialize auth store
onMounted(() => {
    authStore.init()
    document.addEventListener('click', handleClickOutside)
})

// Cleanup
onUnmounted(() => {
    authStore.cleanup()
    document.removeEventListener('click', handleClickOutside)
})
</script>

<template>
    <div class="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors duration-200">
        <!-- Session Timeout Warning -->
        <SessionTimeoutWarning />

        <!-- Auth Layout -->
        <template v-if="isAuthRoute">
            <RouterView />
        </template>

        <!-- App Layout -->
        <template v-else>
            <div class="flex h-screen overflow-hidden">
                <!-- Mobile Header -->
                <header class="lg:hidden fixed top-0 left-0 right-0 z-40 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    <div class="flex items-center justify-between px-4 h-16">
                        <button 
                            @click="toggleMobileMenu" 
                            class="p-2 rounded-lg text-gray-600 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700"
                            aria-label="Toggle menu"
                        >
                            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                <path v-if="!isMobileMenuOpen" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                                <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>
                        
                        <RouterLink to="/dashboard" class="flex items-center gap-2">
                            <div class="h-8 w-8 bg-primary-600 rounded-lg flex items-center justify-center">
                                <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z" />
                                </svg>
                            </div>
                            <span class="text-lg font-bold text-gray-900 dark:text-white hidden sm:block">Digital Twin</span>
                        </RouterLink>

                        <!-- Profile Button -->
                        <div class="relative" data-profile-dropdown>
                            <button 
                                @click="toggleProfileDropdown"
                                class="flex items-center gap-2 p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700"
                            >
                                <div class="h-8 w-8 rounded-full bg-primary-100 dark:bg-primary-900 flex items-center justify-center text-primary-600 dark:text-primary-400 text-sm font-semibold">
                                    {{ authStore.userInitials }}
                                </div>
                            </button>
                        </div>
                    </div>
                </header>

                <!-- Sidebar -->
                <aside 
                    :class="[
                        'fixed inset-y-0 left-0 z-50 w-64 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 transform transition-transform duration-300 lg:sticky lg:top-0 lg:h-screen lg:transform-none',
                        isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
                    ]"
                >
                    <div class="flex flex-col h-full">
                        <!-- Logo -->
                        <div class="flex items-center gap-3 px-6 py-5 border-b border-gray-200 dark:border-gray-700">
                            <div class="h-10 w-10 bg-primary-600 rounded-xl flex items-center justify-center shadow-lg shadow-primary-600/30">
                                <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z" />
                                </svg>
                            </div>
                            <div>
                                <h1 class="text-lg font-bold text-gray-900 dark:text-white">Digital Twin</h1>
                                <p class="text-xs text-gray-500 dark:text-gray-400">Predictive Maintenance</p>
                            </div>
                        </div>

                        <!-- Navigation -->
                        <nav class="flex-1 px-4 py-4 space-y-1 overflow-y-auto">
                            <RouterLink
                                v-for="item in navItems"
                                :key="item.path"
                                :to="item.path"
                                @click="isMobileMenuOpen = false"
                                :class="[
                                    'flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors',
                                    route.path === item.path || route.path.startsWith(item.path + '/')
                                        ? 'bg-primary-50 dark:bg-primary-900/50 text-primary-700 dark:text-primary-400'
                                        : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                                ]"
                            >
                                <svg 
                                    class="w-5 h-5" 
                                    :class="route.path === item.path ? 'text-primary-600 dark:text-primary-400' : 'text-gray-400'"
                                    fill="none" 
                                    stroke="currentColor" 
                                    viewBox="0 0 24 24"
                                    aria-hidden="true"
                                >
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="getIconPath(item.icon)" />
                                </svg>
                                {{ item.name }}
                                <span 
                                    v-if="item.name === 'Alerts' && authStore.sessionWarning.show"
                                    class="ml-auto h-2 w-2 rounded-full bg-danger-500 animate-pulse"
                                ></span>
                            </RouterLink>
                        </nav>

                        <!-- User Section -->
                        <div class="p-4 border-t border-gray-200 dark:border-gray-700">
                            <div 
                                class="flex items-center gap-3 p-3 rounded-lg bg-gray-50 dark:bg-gray-700/50"
                                data-profile-dropdown
                            >
                                <div class="h-10 w-10 rounded-full bg-primary-100 dark:bg-primary-900 flex items-center justify-center text-primary-600 dark:text-primary-400 text-sm font-semibold flex-shrink-0">
                                    {{ authStore.userInitials }}
                                </div>
                                <div class="flex-1 min-w-0">
                                    <p class="text-sm font-medium text-gray-900 dark:text-white truncate">
                                        {{ authStore.userFullName }}
                                    </p>
                                    <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
                                        {{ authStore.user?.email }}
                                    </p>
                                </div>
                                <button 
                                    @click="toggleProfileDropdown"
                                    class="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-200 dark:hover:bg-gray-600 dark:hover:text-gray-200"
                                >
                                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                                    </svg>
                                </button>
                            </div>

                            <!-- Profile Dropdown -->
                            <Transition
                                enter-active-class="transition-all duration-200"
                                enter-from-class="opacity-0 -translate-y-2"
                                enter-to-class="opacity-100 translate-y-0"
                                leave-active-class="transition-all duration-150"
                                leave-from-class="opacity-100 translate-y-0"
                                leave-to-class="opacity-0 -translate-y-2"
                            >
                                <div 
                                    v-if="isProfileDropdownOpen" 
                                    class="mt-2 py-2 bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700"
                                >
                                    <RouterLink 
                                        to="/settings"
                                        @click="isProfileDropdownOpen = false"
                                        class="flex items-center gap-2 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700"
                                    >
                                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                                        </svg>
                                        Your Profile
                                    </RouterLink>
                                    <button 
                                        @click="handleLogout"
                                        class="w-full flex items-center gap-2 px-4 py-2 text-sm text-danger-600 dark:text-danger-400 hover:bg-danger-50 dark:hover:bg-danger-900/20"
                                    >
                                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                                        </svg>
                                        Sign Out
                                    </button>
                                </div>
                            </Transition>
                        </div>
                    </div>
                </aside>

                <!-- Mobile Overlay -->
                <Transition
                    enter-active-class="transition-opacity duration-300"
                    enter-from-class="opacity-0"
                    enter-to-class="opacity-100"
                    leave-active-class="transition-opacity duration-200"
                    leave-from-class="opacity-100"
                    leave-to-class="opacity-0"
                >
                    <div 
                        v-if="isMobileMenuOpen" 
                        @click="isMobileMenuOpen = false"
                        class="lg:hidden fixed inset-0 bg-gray-900/50 z-40"
                    ></div>
                </Transition>

                <!-- Main Content -->
                <main class="flex-1 overflow-y-auto">
                    <RouterView />
                </main>
            </div>
        </template>
    </div>
</template>

<script lang="ts">
import { ref } from 'vue'
export default {
    name: 'App'
}
</script>
