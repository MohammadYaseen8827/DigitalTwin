<script setup lang="ts">
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import LoginForm from '@/components/auth/LoginForm.vue'
import RegisterForm from '@/components/auth/RegisterForm.vue'
import PasswordResetForm from '@/components/auth/PasswordResetForm.vue'

const route = useRoute()
const currentView = ref<'login' | 'register' | 'forgot-password' | 'reset-password'>(
    route.name === 'Register' ? 'register' :
    route.name === 'ForgotPassword' ? 'forgot-password' :
    route.name === 'PasswordReset' ? 'reset-password' : 'login'
)

function setView(view: 'login' | 'register' | 'forgot-password') {
    currentView.value = view
}
</script>

<template>
    <div class="min-h-screen flex flex-col justify-center py-12 sm:px-6 lg:px-8 bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800">
        <!-- Mobile Header -->
        <div class="sm:hidden absolute top-0 left-0 right-0 p-4 bg-white dark:bg-gray-800 shadow-sm">
            <div class="flex items-center justify-between">
                <div class="flex items-center gap-2">
                    <div class="h-8 w-8 bg-primary-600 rounded-lg flex items-center justify-center">
                        <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z" />
                        </svg>
                    </div>
                    <span class="text-lg font-bold text-gray-900 dark:text-white">Digital Twin</span>
                </div>
            </div>
        </div>

        <!-- Desktop Logo -->
        <div class="hidden sm:mx-auto sm:w-full sm:max-w-md mb-6">
            <div class="flex justify-center">
                <div class="flex items-center gap-3">
                    <div class="h-12 w-12 bg-primary-600 rounded-xl flex items-center justify-center shadow-lg shadow-primary-600/30">
                        <svg class="w-7 h-7 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z" />
                        </svg>
                    </div>
                    <div class="text-left">
                        <h1 class="text-xl font-bold text-gray-900 dark:text-white">Digital Twin Platform</h1>
                        <p class="text-xs text-gray-500 dark:text-gray-400">Predictive Maintenance System</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Form Card -->
        <div class="sm:mx-auto sm:w-full sm:max-w-md">
            <div class="bg-white dark:bg-gray-800 py-8 px-4 shadow-xl sm:rounded-xl sm:px-10">
                <!-- Login Form -->
                <Transition
                    enter-active-class="transition-all duration-300"
                    enter-from-class="opacity-0 translate-x-4"
                    enter-to-class="opacity-100 translate-x-0"
                    leave-active-class="transition-all duration-200"
                    leave-from-class="opacity-100 translate-x-0"
                    leave-to-class="opacity-0 -translate-x-4"
                >
                    <div v-if="currentView === 'login'">
                        <div class="text-center mb-6">
                            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Welcome back</h2>
                            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">Sign in to your account to continue</p>
                        </div>
                        <LoginForm />
                        
                        <!-- Register Link -->
                        <div class="mt-6 text-center">
                            <p class="text-sm text-gray-600 dark:text-gray-400">
                                Don't have an account?
                                <button @click="setView('register')" class="font-medium text-primary-600 hover:text-primary-500">
                                    Create one
                                </button>
                            </p>
                        </div>
                    </div>
                </Transition>

                <!-- Register Form -->
                <Transition
                    enter-active-class="transition-all duration-300"
                    enter-from-class="opacity-0 translate-x-4"
                    enter-to-class="opacity-100 translate-x-0"
                    leave-active-class="transition-all duration-200"
                    leave-from-class="opacity-100 translate-x-0"
                    leave-to-class="opacity-0 -translate-x-4"
                >
                    <div v-if="currentView === 'register'">
                        <div class="text-center mb-6">
                            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Create an account</h2>
                            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">Get started with Digital Twin Platform</p>
                        </div>
                        <RegisterForm />
                        
                        <!-- Login Link -->
                        <div class="mt-6 text-center">
                            <p class="text-sm text-gray-600 dark:text-gray-400">
                                Already have an account?
                                <button @click="setView('login')" class="font-medium text-primary-600 hover:text-primary-500">
                                    Sign in
                                </button>
                            </p>
                        </div>
                    </div>
                </Transition>

                <!-- Forgot Password Form -->
                <Transition
                    enter-active-class="transition-all duration-300"
                    enter-from-class="opacity-0 translate-x-4"
                    enter-to-class="opacity-100 translate-x-0"
                    leave-active-class="transition-all duration-200"
                    leave-from-class="opacity-100 translate-x-0"
                    leave-to-class="opacity-0 -translate-x-4"
                >
                    <div v-if="currentView === 'forgot-password'">
                        <PasswordResetForm mode="request" />
                        
                        <!-- Back to Login -->
                        <div class="mt-6 text-center">
                            <button @click="setView('login')" class="font-medium text-primary-600 hover:text-primary-500">
                                ← Back to Sign In
                            </button>
                        </div>
                    </div>
                </Transition>
            </div>
        </div>

        <!-- Footer -->
        <div class="hidden sm:mx-auto sm:max-w-md mt-8">
            <p class="text-center text-xs text-gray-500 dark:text-gray-400">
                © {{ new Date().getFullYear() }} Digital Twin Platform. All rights reserved.
            </p>
        </div>
    </div>
</template>
