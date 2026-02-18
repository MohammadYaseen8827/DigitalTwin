<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

// Form state
const name = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const acceptTerms = ref(false)
const showPassword = ref(false)
const showConfirmPassword = ref(false)
const errors = ref<Record<string, string>>({})

// Validation
const nameError = computed(() => {
    if (!name.value) return ''
    return name.value.trim().length >= 2 ? '' : 'Name must be at least 2 characters'
})

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

const termsError = computed(() => {
    if (!acceptTerms.value && confirmPassword.value) return 'You must accept the terms and conditions'
    return ''
})

const isFormValid = computed(() => {
    return name.value && email.value && password.value && confirmPassword.value &&
           acceptTerms.value && !nameError.value && !emailError.value &&
           !passwordError.value && !confirmPasswordError.value
})

// Form submission
async function handleSubmit() {
    errors.value = {}
    
    if (!isFormValid.value) {
        return
    }

    const success = await authStore.register({
        name: name.value,
        email: email.value,
        password: password.value,
        confirmPassword: confirmPassword.value,
        acceptTerms: acceptTerms.value
    })

    if (success) {
        router.push('/dashboard')
    } else {
        errors.value.general = authStore.error || 'Registration failed'
    }
}

// Password strength indicator
const passwordStrength = computed(() => {
    const p = password.value
    let strength = 0
    if (p.length >= 8) strength++
    if (p.length >= 12) strength++
    if (/[A-Z]/.test(p)) strength++
    if (/[a-z]/.test(p)) strength++
    if (/[0-9]/.test(p)) strength++
    if (/[^A-Za-z0-9]/.test(p)) strength++
    
    if (strength <= 2) return { level: 'weak', color: 'bg-danger-500', text: 'Weak' }
    if (strength <= 3) return { level: 'fair', color: 'bg-warning-500', text: 'Fair' }
    if (strength <= 4) return { level: 'good', color: 'bg-primary-500', text: 'Good' }
    return { level: 'strong', color: 'bg-success-500', text: 'Strong' }
})

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
    <form @submit.prevent="handleSubmit" class="space-y-6" aria-label="Registration form">
        <!-- General Error -->
        <div v-if="errors.general" 
             class="p-4 bg-danger-50 border border-danger-200 rounded-lg text-danger-600 text-sm"
             role="alert">
            {{ errors.general }}
        </div>

        <!-- Name Field -->
        <div>
            <label for="name" class="block text-sm font-medium text-gray-700 mb-1">
                Full Name
            </label>
            <div class="relative">
                <input
                    id="name"
                    v-model="name"
                    type="text"
                    autocomplete="name"
                    :class="[
                        'input pl-10 pr-4 py-3',
                        nameError && name ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : ''
                    ]"
                    :aria-invalid="!!nameError && !!name"
                    placeholder="John Doe"
                    required
                />
                <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" 
                     fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                          d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
            </div>
            <p v-if="nameError && name" class="mt-1 text-sm text-danger-600">{{ nameError }}</p>
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
                    autocomplete="new-password"
                    :class="[
                        'input pl-10 pr-10 py-3',
                        passwordError && password ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : ''
                    ]"
                    placeholder="Create a strong password"
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
            
            <!-- Password Strength Indicator -->
            <div v-if="password" class="mt-2">
                <div class="flex items-center gap-2">
                    <div class="flex-1 h-1.5 bg-gray-200 rounded-full overflow-hidden">
                        <div 
                            class="h-full transition-all duration-300"
                            :class="passwordStrength.color"
                            :style="{ width: `${(passwordStrength.level === 'weak' ? 25 : passwordStrength.level === 'fair' ? 50 : passwordStrength.level === 'good' ? 75 : 100)}%` }"
                        ></div>
                    </div>
                    <span class="text-xs" :class="passwordStrength.level === 'weak' ? 'text-danger-600' : passwordStrength.level === 'fair' ? 'text-warning-600' : passwordStrength.level === 'good' ? 'text-primary-600' : 'text-success-600'">
                        {{ passwordStrength.text }}
                    </span>
                </div>
            </div>
            
            <p v-if="passwordError && password" class="mt-1 text-sm text-danger-600">{{ passwordError }}</p>
        </div>

        <!-- Confirm Password Field -->
        <div>
            <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-1">
                Confirm Password
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
                    placeholder="Confirm your password"
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

        <!-- Terms Acceptance -->
        <div>
            <label class="flex items-start">
                <input
                    v-model="acceptTerms"
                    type="checkbox"
                    class="mt-1 h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded cursor-pointer"
                />
                <span class="ml-2 text-sm text-gray-600">
                    I agree to the 
                    <a href="#" class="text-primary-600 hover:text-primary-500">Terms of Service</a>
                    and
                    <a href="#" class="text-primary-600 hover:text-primary-500">Privacy Policy</a>
                </span>
            </label>
            <p v-if="termsError" class="mt-1 text-sm text-danger-600">{{ termsError }}</p>
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
            {{ authStore.isLoading ? 'Creating account...' : 'Create Account' }}
        </button>

        <!-- Login Link -->
        <p class="text-center text-sm text-gray-600">
            Already have an account?
            <router-link 
                to="/login"
                class="font-medium text-primary-600 hover:text-primary-500 transition-colors"
            >
                Sign in
            </router-link>
        </p>
    </form>
</template>
