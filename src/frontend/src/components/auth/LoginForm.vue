<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

// Form state
const email = ref('')
const password = ref('')
const rememberMe = ref(false)
const showPassword = ref(false)
const errors = ref<Record<string, string>>({})

// Validation
const emailError = computed(() => {
    if (!email.value) return ''
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(email.value) ? '' : 'Please enter a valid email address'
})

const passwordError = computed(() => {
    if (!password.value) return ''
    return password.value.length >= 6 ? '' : 'Password must be at least 6 characters'
})

const isFormValid = computed(() => {
    return email.value && password.value && !emailError.value && !passwordError.value
})

// Form submission
async function handleSubmit() {
    errors.value = {}
    
    if (!isFormValid.value) {
        return
    }

    const success = await authStore.login({
        email: email.value,
        password: password.value,
        rememberMe: rememberMe.value
    })

    if (success) {
        const redirect = route.query.redirect as string
        router.push(redirect || '/dashboard')
    } else {
        errors.value.general = authStore.error || 'Login failed'
    }
}

// Toggle password visibility
function togglePasswordVisibility() {
    showPassword.value = !showPassword.value
}
</script>

<template>
    <form @submit.prevent="handleSubmit" class="space-y-6" aria-label="Login form">
        <!-- General Error -->
        <div v-if="errors.general" 
             class="p-4 bg-danger-50 border border-danger-200 rounded-lg text-danger-600 text-sm"
             role="alert">
            {{ errors.general }}
        </div>

        <!-- Email Field -->
        <div>
            <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
                Email Address
            </label>
            <div class="relative">
                <input
                    id="email"
                    v-model="email"
                    type="email"
                    autocomplete="email"
                    :class="[
                        'input pl-10 pr-4 py-3',
                        emailError && email ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : ''
                    ]"
                    :aria-invalid="!!emailError && !!email"
                    :aria-describedby="emailError && email ? 'email-error' : undefined"
                    placeholder="you@example.com"
                    required
                />
                <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" 
                     fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                          d="M16 12a4 4 0 10-8 0 4 4 0 008 0zm0 0v1.5a2.5 2.5 0 005 0V12a9 9 0 10-9 9m4.5-1.206a8.959 8.959 0 01-4.5 1.207" />
                </svg>
            </div>
            <p v-if="emailError && email" id="email-error" class="mt-1 text-sm text-danger-600">
                {{ emailError }}
            </p>
        </div>

        <!-- Password Field -->
        <div>
            <label for="password" class="block text-sm font-medium text-gray-700 mb-1">
                Password
            </label>
            <div class="relative">
                <input
                    id="password"
                    v-model="password"
                    :type="showPassword ? 'text' : 'password'"
                    autocomplete="current-password"
                    :class="[
                        'input pl-10 pr-10 py-3',
                        passwordError && password ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : ''
                    ]"
                    :aria-invalid="!!passwordError && !!password"
                    :aria-describedby="passwordError && password ? 'password-error' : undefined"
                    placeholder="Enter your password"
                    required
                />
                <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" 
                     fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                          d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                </svg>
                <button
                    type="button"
                    @click="togglePasswordVisibility"
                    class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 focus:outline-none"
                    :aria-label="showPassword ? 'Hide password' : 'Show password'"
                >
                    <svg v-if="showPassword" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                              d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
                    </svg>
                    <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                              d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                              d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                </button>
            </div>
            <p v-if="passwordError && password" id="password-error" class="mt-1 text-sm text-danger-600">
                {{ passwordError }}
            </p>
        </div>

        <!-- Remember Me & Forgot Password -->
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <label class="flex items-center">
                <input
                    v-model="rememberMe"
                    type="checkbox"
                    class="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded cursor-pointer"
                />
                <span class="ml-2 text-sm text-gray-600">Remember me</span>
            </label>
            <router-link 
                to="/forgot-password"
                class="text-sm font-medium text-primary-600 hover:text-primary-500 transition-colors"
            >
                Forgot password?
            </router-link>
        </div>

        <!-- Submit Button -->
        <button
            type="submit"
            :disabled="!isFormValid || authStore.isLoading"
            class="w-full btn-primary py-3 text-base disabled:opacity-50 disabled:cursor-not-allowed"
        >
            <svg v-if="authStore.isLoading" class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" 
                 fill="none" viewBox="0 0 24 24" aria-hidden="true">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" 
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            {{ authStore.isLoading ? 'Signing in...' : 'Sign In' }}
        </button>

        <!-- Register Link -->
        <p class="text-center text-sm text-gray-600">
            Don't have an account?
            <router-link 
                to="/register"
                class="font-medium text-primary-600 hover:text-primary-500 transition-colors"
            >
                Create an account
            </router-link>
        </p>
    </form>
</template>
