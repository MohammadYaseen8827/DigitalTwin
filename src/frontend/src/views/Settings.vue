<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import UserProfileSettings from '@/components/settings/UserProfileSettings.vue'
import NotificationSettings from '@/components/settings/NotificationSettings.vue'
import DisplaySettings from '@/components/settings/DisplaySettings.vue'
import SystemSettings from '@/components/settings/Settings.vue'

const authStore = useAuthStore()

// Settings tabs
const tabs = [
    { id: 'profile', name: 'Profile', icon: 'user', component: UserProfileSettings },
    { id: 'notifications', name: 'Notifications', icon: 'bell', component: NotificationSettings },
    { id: 'display', name: 'Display', icon: 'palette', component: DisplaySettings },
]

// Add system settings for admins
const allTabs = ref([...tabs])
const activeTab = ref('profile')
const isSidebarOpen = ref(false)

// Check if user is admin (add system tab)
onMounted(() => {
    if (authStore.isAdmin) {
        allTabs.value.push({ id: 'system', name: 'System', icon: 'cog', component: SystemSettings })
    }
})

// Tab icons
function getTabIcon(icon: string) {
    const icons: Record<string, string> = {
        user: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z',
        bell: 'M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9',
        palette: 'M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zm0 0h12a2 2 0 002-2v-4a2 2 0 00-2-2h-2.343M11 7.343l1.657-1.657a2 2 0 012.828 0l2.829 2.829a2 2 0 010 2.828l-8.486 8.485M7 17h.01',
        cog: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z'
    }
    return icons[icon] || icons.cog
}

// Toggle sidebar on mobile
function toggleSidebar() {
    isSidebarOpen.value = !isSidebarOpen.value
}
</script>

<template>
    <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
        <div class="flex">
            <!-- Mobile Header -->
            <div class="lg:hidden fixed top-0 left-0 right-0 z-40 bg-white dark:bg-gray-800 shadow-sm">
                <div class="flex items-center justify-between px-4 py-3">
                    <button @click="toggleSidebar" class="p-2 rounded-lg text-gray-600 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700">
                        <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
                        </svg>
                    </button>
                    <h1 class="text-lg font-semibold text-gray-900 dark:text-white">Settings</h1>
                    <div class="w-10"></div>
                </div>
            </div>

            <!-- Mobile Sidebar Overlay -->
            <Transition
                enter-active-class="transition-opacity duration-300"
                enter-from-class="opacity-0"
                enter-to-class="opacity-100"
                leave-active-class="transition-opacity duration-200"
                leave-from-class="opacity-100"
                leave-to-class="opacity-0"
            >
                <div 
                    v-if="isSidebarOpen" 
                    @click="isSidebarOpen = false"
                    class="lg:hidden fixed inset-0 bg-gray-900/50 z-40"
                ></div>
            </Transition>

            <!-- Sidebar -->
            <aside 
                :class="[
                    'lg:sticky lg:top-0 lg:h-screen fixed left-0 top-0 z-50 w-64 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 transform transition-transform duration-300 lg:transform-none overflow-y-auto',
                    isSidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
                ]"
            >
                <div class="p-6 pt-20 lg:pt-6">
                    <!-- User Info -->
                    <div class="flex items-center gap-3 mb-6 pb-6 border-b border-gray-200 dark:border-gray-700">
                        <div class="h-12 w-12 rounded-full bg-primary-100 dark:bg-primary-900 flex items-center justify-center text-primary-600 dark:text-primary-400 text-lg font-semibold">
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
                    </div>

                    <!-- Navigation -->
                    <nav class="space-y-1">
                        <button
                            v-for="tab in allTabs"
                            :key="tab.id"
                            @click="activeTab = tab.id; isSidebarOpen = false"
                            :class="[
                                'w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors',
                                activeTab === tab.id
                                    ? 'bg-primary-50 dark:bg-primary-900/50 text-primary-700 dark:text-primary-400'
                                    : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                            ]"
                        >
                            <svg 
                                class="w-5 h-5 flex-shrink-0" 
                                :class="activeTab === tab.id ? 'text-primary-600 dark:text-primary-400' : 'text-gray-400'"
                                fill="none" 
                                stroke="currentColor" 
                                viewBox="0 0 24 24" 
                                aria-hidden="true"
                            >
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="getTabIcon(tab.icon)" />
                            </svg>
                            {{ tab.name }}
                            <span 
                                v-if="tab.id === 'notifications' && authStore.sessionWarning.show"
                                class="ml-auto h-2 w-2 rounded-full bg-danger-500 animate-pulse"
                            ></span>
                        </button>
                    </nav>

                    <!-- Logout Button -->
                    <div class="mt-6 pt-6 border-t border-gray-200 dark:border-gray-700">
                        <button
                            @click="authStore.logout()"
                            class="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                        >
                            <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                            </svg>
                            Sign Out
                        </button>
                    </div>
                </div>
            </aside>

            <!-- Main Content -->
            <main class="flex-1 p-4 lg:p-8 pt-20 lg:pt-8 min-h-screen">
                <div class="max-w-4xl mx-auto">
                    <!-- Desktop Header -->
                    <div class="hidden lg:block mb-8">
                        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Settings</h1>
                        <p class="text-gray-600 dark:text-gray-400">Manage your account and application preferences</p>
                    </div>

                    <!-- Settings Content -->
                    <div class="space-y-6">
                        <UserProfileSettings v-if="activeTab === 'profile'" />
                        <NotificationSettings v-if="activeTab === 'notifications'" />
                        <DisplaySettings v-if="activeTab === 'display'" />
                        <SystemSettings v-if="activeTab === 'system'" />
                    </div>
                </div>
            </main>
        </div>
    </div>
</template>
