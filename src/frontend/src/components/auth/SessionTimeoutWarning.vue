<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

// Format remaining time as MM:SS
const formattedTime = computed(() => {
    const minutes = Math.floor(authStore.sessionWarning.remainingSeconds / 60)
    const seconds = authStore.sessionWarning.remainingSeconds % 60
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
})

// Handle extend session
function extendSession() {
    authStore.extendSession()
}

// Handle logout
function logout() {
    authStore.logout()
}
</script>

<template>
    <Teleport to="body">
        <Transition
            enter-active-class="transition ease-out duration-300"
            enter-from-class="opacity-0"
            enter-to-class="opacity-100"
            leave-active-class="transition ease-in duration-200"
            leave-from-class="opacity-100"
            leave-to-class="opacity-0"
        >
            <div 
                v-if="authStore.sessionWarning.show"
                class="fixed inset-0 z-50 overflow-y-auto"
                role="dialog"
                aria-modal="true"
                aria-labelledby="session-warning-title"
                aria-describedby="session-warning-description"
            >
                <!-- Backdrop -->
                <div 
                    class="fixed inset-0 bg-gray-900/50 backdrop-blur-sm transition-opacity"
                    @click="logout"
                ></div>

                <!-- Modal -->
                <div class="flex min-h-full items-center justify-center p-4">
                    <Transition
                        enter-active-class="transition ease-out duration-300"
                        enter-from-class="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
                        enter-to-class="opacity-100 translate-y-0 sm:scale-100"
                        leave-active-class="transition ease-in duration-200"
                        leave-from-class="opacity-100 translate-y-0 sm:scale-100"
                        leave-to-class="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
                    >
                        <div class="relative w-full max-w-md transform overflow-hidden rounded-xl bg-white shadow-xl transition-all">
                            <!-- Warning Icon -->
                            <div class="bg-warning-50 px-6 py-4 flex items-center gap-4">
                                <div class="flex-shrink-0">
                                    <svg class="h-10 w-10 text-warning-500 animate-pulse" 
                                         fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                                              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                                    </svg>
                                </div>
                                <div>
                                    <h3 id="session-warning-title" class="text-lg font-semibold text-warning-800">
                                        Session Expiring Soon
                                    </h3>
                                    <p id="session-warning-description" class="text-sm text-warning-700">
                                        Your session will expire in {{ formattedTime }}
                                    </p>
                                </div>
                            </div>

                            <!-- Countdown Display -->
                            <div class="px-6 py-8 text-center">
                                <div class="text-5xl font-mono font-bold text-gray-900 mb-2">
                                    {{ formattedTime }}
                                </div>
                                <p class="text-sm text-gray-600">
                                    Your session will automatically expire and you'll be logged out for security.
                                </p>
                            </div>

                            <!-- Actions -->
                            <div class="bg-gray-50 px-6 py-4 flex flex-col sm:flex-row gap-3">
                                <button
                                    @click="extendSession"
                                    class="flex-1 btn-primary py-2.5 text-sm font-medium"
                                    autofocus
                                >
                                    <svg class="-ml-1 mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                                              d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                    Extend Session
                                </button>
                                <button
                                    @click="logout"
                                    class="flex-1 btn-secondary py-2.5 text-sm font-medium"
                                >
                                    <svg class="-ml-1 mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                                              d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                                    </svg>
                                    Logout Now
                                </button>
                            </div>
                        </div>
                    </Transition>
                </div>
            </div>
        </Transition>
    </Teleport>
</template>
