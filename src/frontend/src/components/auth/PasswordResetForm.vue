<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const props = defineProps<{
    mode?: 'request' | 'reset'
}>()

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// Determine mode from props or route
const isResetMode = computed(() => props.mode === 'reset' || route.name === 'PasswordReset')

// Form state (email can be pre-filled from reset link query, e.g. ?token=...&email=...)
const email = ref((route.query.email as string) || '')
const password = ref('')
const confirmPassword = ref('')
const showPassword = ref(false)
const showConfirmPassword = ref(false)
const resetToken = ref(route.query.token as string || '')
const errors = ref<Record<string, string>>({})
const successMessage = ref('')

// Validation
const emailError = computed(() => {
    if (!email.value) return ''
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(email.value) ? '' : 'Please enter a valid email address'
})

const passwordError = computed(() => {
    if (!password.value) return ''
    if (password.value.length < 8) return 'Password must be at least 8 characters'
    if (!/[A-Z]/.test(password.value)) return 'Password must contain at least one uppercase letter'
    if (!/[a-z]/.test(password.value)) return 'Password must contain at least one lowercase letter'
    if (!/[0-9]/.test(password.value)) return 'Password must contain at least one number'
    return ''
})

const confirmPasswordError = computed(() => {
    if (!confirmPassword.value) return ''
    return confirmPassword.value === password.value ? '' : 'Passwords do not match'
})

const isFormValid = computed(() => {
    if (!isResetMode.value) {
        return email.value && !emailError.value
    }
    return email.value && !emailError.value && password.value && confirmPassword.value &&
           !passwordError.value && !confirmPasswordError.value
})

// Form submission
async function handleSubmit() {
    errors.value = {}
    successMessage.value = ''

    if (!isResetMode.value) {
        // Request password reset
        const success = await authStore.requestPasswordReset(email.value)
        if (success) {
            successMessage.value = 'Password reset link sent to your email. Please check your inbox.'
        } else {
            errors.value.general = authStore.error || 'Failed to send reset email.'
        }
    } else {
        // Reset password with token
        if (!resetToken.value) {
            errors.value.general = 'Reset token is missing. Please use the link from your email.'
            return
        }

        const success = await authStore.resetPassword({
            email: email.value,
            token: resetToken.value,
            password: password.value,
            confirmPassword: confirmPassword.value
        })

        if (success) {
            successMessage.value = 'Password reset successful. Redirecting to login...'
            setTimeout(() => {
                router.push('/login')
            }, 2000)
        } else {
            errors.value.general = authStore.error || 'Failed to reset password.'
        }
    }
}

// Go back to login
function goToLogin() {
    router.push('/login')
}

// Toggle password visibility
function togglePasswordVisibility(field: 'password' | 'confirm') {
    if (field === 'password') {
        showPassword.value = !showPassword.value
    } else {
        showConfirmPassword.value = !showConfirmPassword.value
    }
}
</script>

<template>
    <form @submit.prevent="handleSubmit" class="space-y-6" aria-label="Password reset form">
        <!-- General Error -->
        <div v-if="errors.general" 
             class="p-4 bg-danger-50 border border-danger-200 rounded-lg text-danger-600 text-sm"
             role="alert">
            {{ errors.general }}
        </div>

        <!-- Success Message -->
        <div v-if="successMessage" 
             class="p-4 bg-success-50 border border-success-200 rounded-lg text-success-600 text-sm"
             role="status">
            {{ successMessage }}
        </div>

        <!-- Request Reset Mode -->
        <template v-if="!isResetMode">
            <div class="text-center mb-6">
                <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                          d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
                </svg>
                <h2 class="mt-4 text-xl font-semibold text-gray-900">Forgot your password?</h2>
                <p class="mt-2 text-sm text-gray-600">
                    Enter your email address and we'll send you a link to reset your password.
                </p>
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
                        placeholder="you@example.com"
                        required
                    />
                    <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" 
                         fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                              d="M16 12a4 4 0 10-8 0 4 4 0 008 0zm0 0v1.5a2.5 2.5 0 005 0V12a9 9 0 10-9 9m4.5-1.206a8.959 8.959 0 01-4.5 1.207" />
                    </svg>
                </div>
                <p v-if="emailError && email" class="mt-1 text-sm text-danger-600">{{ emailError }}</p>
            </div>
        </template>

        <!-- Reset Mode -->
        <template v-else>
            <div class="text-center mb-6">
                <svg class="mx-auto h-12 w-12 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                          d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                </svg>
                <h2 class="mt-4 text-xl font-semibold text-gray-900">Set new password</h2>
                <p class="mt-2 text-sm text-gray-600">
                    Enter the email you requested the reset for, then set your new password.
                </p>
            </div>

            <!-- Email (required by backend to identify the account) -->
            <div>
                <label for="reset-email" class="block text-sm font-medium text-gray-700 mb-1">
                    Email address
                </label>
                <input
                    id="reset-email"
                    v-model="email"
                    type="email"
                    autocomplete="email"
                    :class="['input w-full py-3', emailError && email ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : '']"
                    placeholder="you@example.com"
                    required
                />
                <p v-if="emailError && email" class="mt-1 text-sm text-danger-600">{{ emailError }}</p>
            </div>

            <!-- Password Field -->
            <div>
                <label for="password" class="block text-sm font-medium text-gray-700 mb-1">
                    New Password
                </label>
                <div class="relative">
                    <input
                        id="password"
                        v-model="password"
                        :type="showPassword ? 'text' : 'password'"
                        autocomplete="new-password"
                        :class="[
                            'input pl-10 pr-10 py-3',
                            passwordError && password ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : ''
                        ]"
                        placeholder="Enter new password"
                        required
                    />
                    <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" 
                         fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                              d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                    </svg>
                    <button
                        type="button"
                        @click="togglePasswordVisibility('password')"
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
                <p v-if="passwordError && password" class="mt-1 text-sm text-danger-600">{{ passwordError }}</p>
            </div>

            <!-- Confirm Password Field -->
            <div>
                <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-1">
                    Confirm New Password
                </label>
                <div class="relative">
                    <input
                        id="confirmPassword"
                        v-model="confirmPassword"
                        :type="showConfirmPassword ? 'text' : 'password'"
                        autocomplete="new-password"
                        :class="[
                            'input pl-10 pr-10 py-3',
                            confirmPasswordError && confirmPassword ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : ''
                        ]"
                        placeholder="Confirm new password"
                        required
                    />
                    <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" 
                         fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                              d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <button
                        type="button"
                        @click="togglePasswordVisibility('confirm')"
                        class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 focus:outline-none"
                        :aria-label="showConfirmPassword ? 'Hide password' : 'Show password'"
                    >
                        <svg v-if="showConfirmPassword" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
                <p v-if="confirmPasswordError && confirmPassword" class="mt-1 text-sm text-danger-600">
                    {{ confirmPasswordError }}
                </p>
            </div>
        </template>

        <!-- Submit Button -->
        <button
            type="submit"
            :disabled="!isFormValid || authStore.isLoading || !!successMessage"
            class="w-full btn-primary py-3 text-base disabled:opacity-50 disabled:cursor-not-allowed"
        >
            <svg v-if="authStore.isLoading" class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" 
                 fill="none" viewBox="0 0 24 24" aria-hidden="true">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" 
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            {{ authStore.isLoading ? 'Processing...' : (isResetMode ? 'Reset Password' : 'Send Reset Link') }}
        </button>

        <!-- Back to Login -->
        <p class="text-center text-sm text-gray-600">
            <button
                type="button"
                @click="goToLogin"
                class="font-medium text-primary-600 hover:text-primary-500 transition-colors"
            >
                Back to Sign In
            </button>
        </p>
    </form>
</template>
