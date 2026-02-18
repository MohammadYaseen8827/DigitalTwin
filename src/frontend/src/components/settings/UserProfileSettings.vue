<script setup lang="ts">
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

// Profile form state
const name = ref(authStore.user?.name || '')
const email = ref(authStore.user?.email || '')
const currentPassword = ref('')
const newPassword = ref('')
const confirmNewPassword = ref('')
const showCurrentPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmNewPassword = ref(false)
const activeTab = ref<'profile' | 'password' | 'security'>('profile')
const errors = ref<Record<string, string>>({})
const successMessage = ref('')

// Profile validation
const nameError = computed(() => {
    if (!name.value) return ''
    return name.value.trim().length >= 2 ? '' : 'Name must be at least 2 characters'
})

const emailError = computed(() => {
    if (!email.value) return ''
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(email.value) ? '' : 'Please enter a valid email address'
})

// Password validation
const passwordError = computed(() => {
    if (!newPassword.value) return ''
    if (newPassword.value.length < 8) return 'Password must be at least 8 characters'
    if (!/[A-Z]/.test(newPassword.value)) return 'Password must contain at least one uppercase letter'
    if (!/[a-z]/.test(newPassword.value)) return 'Password must contain at least one lowercase letter'
    if (!/[0-9]/.test(newPassword.value)) return 'Password must contain at least one number'
    return ''
})

const confirmPasswordError = computed(() => {
    if (!confirmNewPassword.value) return ''
    return confirmNewPassword.value === newPassword.value ? '' : 'Passwords do not match'
})

// Save profile
async function saveProfile() {
    errors.value = {}
    successMessage.value = ''

    if (nameError.value) {
        errors.value.name = nameError.value
        return
    }

    if (emailError.value) {
        errors.value.email = emailError.value
        return
    }

    const success = await authStore.updateProfile({
        name: name.value,
        email: email.value
    })

    if (success) {
        successMessage.value = 'Profile updated successfully'
        setTimeout(() => successMessage.value = '', 3000)
    } else {
        errors.value.general = authStore.error || 'Failed to update profile'
    }
}

// Change password
async function changePassword() {
    errors.value = {}
    successMessage.value = ''

    if (!currentPassword.value) {
        errors.value.currentPassword = 'Current password is required'
        return
    }

    if (passwordError.value) {
        errors.value.newPassword = passwordError.value
        return
    }

    if (confirmPasswordError.value) {
        errors.value.confirmNewPassword = confirmPasswordError.value
        return
    }

    const success = await authStore.changePassword(currentPassword.value, newPassword.value)

    if (success) {
        successMessage.value = 'Password changed successfully'
        currentPassword.value = ''
        newPassword.value = ''
        confirmNewPassword.value = ''
        setTimeout(() => successMessage.value = '', 3000)
    } else {
        errors.value.general = authStore.error || 'Failed to change password'
    }
}

// Toggle password visibility
function togglePassword(field: 'current' | 'new' | 'confirm') {
    if (field === 'current') showCurrentPassword.value = !showCurrentPassword.value
    else if (field === 'new') showNewPassword.value = !showNewPassword.value
    else showConfirmNewPassword.value = !showConfirmNewPassword.value
}
</script>

<template>
    <div class="space-y-6">
        <!-- Header -->
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
                <h2 class="text-xl font-semibold text-gray-900">Profile Settings</h2>
                <p class="text-sm text-gray-600">Manage your account information and security</p>
            </div>
        </div>

        <!-- Success Message -->
        <div v-if="successMessage" 
             class="p-4 bg-success-50 border border-success-200 rounded-lg text-success-600 text-sm"
             role="status">
            {{ successMessage }}
        </div>

        <!-- General Error -->
        <div v-if="errors.general" 
             class="p-4 bg-danger-50 border border-danger-200 rounded-lg text-danger-600 text-sm"
             role="alert">
            {{ errors.general }}
        </div>

        <!-- Tabs -->
        <div class="border-b border-gray-200">
            <nav class="-mb-px flex space-x-8" aria-label="Tabs" role="tablist">
                <button
                    @click="activeTab = 'profile'"
                    :class="[
                        'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
                        activeTab === 'profile'
                            ? 'border-primary-500 text-primary-600'
                            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    ]"
                    role="tab"
                    :aria-selected="activeTab === 'profile'"
                >
                    <svg class="inline w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                    Profile
                </button>
                <button
                    @click="activeTab = 'password'"
                    :class="[
                        'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
                        activeTab === 'password'
                            ? 'border-primary-500 text-primary-600'
                            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    ]"
                    role="tab"
                    :aria-selected="activeTab === 'password'"
                >
                    <svg class="inline w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                    </svg>
                    Password
                </button>
                <button
                    @click="activeTab = 'security'"
                    :class="[
                        'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
                        activeTab === 'security'
                            ? 'border-primary-500 text-primary-600'
                            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    ]"
                    role="tab"
                    :aria-selected="activeTab === 'security'"
                >
                    <svg class="inline w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                    </svg>
                    Security
                </button>
            </nav>
        </div>

        <!-- Profile Tab -->
        <div v-if="activeTab === 'profile'" class="space-y-6">
            <!-- Avatar Section -->
            <div class="flex flex-col sm:flex-row sm:items-center gap-6">
                <div class="flex-shrink-0">
                    <div class="h-24 w-24 rounded-full bg-primary-100 flex items-center justify-center text-primary-600 text-2xl font-semibold">
                        {{ authStore.userInitials }}
                    </div>
                </div>
                <div>
                    <h3 class="text-base font-medium text-gray-900">Profile Photo</h3>
                    <p class="text-sm text-gray-500">JPG, GIF or PNG. Max size 2MB</p>
                    <button type="button" class="mt-2 btn-secondary text-sm py-1.5">
                        Change Photo
                    </button>
                </div>
            </div>

            <!-- Name Field -->
            <div>
                <label for="name" class="block text-sm font-medium text-gray-700 mb-1">Full Name</label>
                <input
                    id="name"
                    v-model="name"
                    type="text"
                    :class="['input py-2.5', errors.name ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : '']"
                />
                <p v-if="errors.name" class="mt-1 text-sm text-danger-600">{{ errors.name }}</p>
            </div>

            <!-- Email Field -->
            <div>
                <label for="email" class="block text-sm font-medium text-gray-700 mb-1">Email Address</label>
                <input
                    id="email"
                    v-model="email"
                    type="email"
                    :class="['input py-2.5', errors.email ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : '']"
                />
                <p v-if="errors.email" class="mt-1 text-sm text-danger-600">{{ errors.email }}</p>
            </div>

            <!-- Save Button -->
            <div class="flex justify-end">
                <button @click="saveProfile" :disabled="authStore.isLoading" class="btn-primary">
                    {{ authStore.isLoading ? 'Saving...' : 'Save Changes' }}
                </button>
            </div>
        </div>

        <!-- Password Tab -->
        <div v-if="activeTab === 'password'" class="space-y-6">
            <!-- Current Password -->
            <div>
                <label for="currentPassword" class="block text-sm font-medium text-gray-700 mb-1">Current Password</label>
                <div class="relative">
                    <input
                        id="currentPassword"
                        v-model="currentPassword"
                        :type="showCurrentPassword ? 'text' : 'password'"
                        :class="['input py-2.5 pr-10', errors.currentPassword ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : '']"
                    />
                    <button
                        type="button"
                        @click="togglePassword('current')"
                        class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                    >
                        <svg v-if="showCurrentPassword" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
                        </svg>
                        <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                        </svg>
                    </button>
                </div>
                <p v-if="errors.currentPassword" class="mt-1 text-sm text-danger-600">{{ errors.currentPassword }}</p>
            </div>

            <!-- New Password -->
            <div>
                <label for="newPassword" class="block text-sm font-medium text-gray-700 mb-1">New Password</label>
                <div class="relative">
                    <input
                        id="newPassword"
                        v-model="newPassword"
                        :type="showNewPassword ? 'text' : 'password'"
                        :class="['input py-2.5 pr-10', errors.newPassword ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : '']"
                    />
                    <button
                        type="button"
                        @click="togglePassword('new')"
                        class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                    >
                        <svg v-if="showNewPassword" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
                        </svg>
                        <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                        </svg>
                    </button>
                </div>
                <p v-if="errors.newPassword" class="mt-1 text-sm text-danger-600">{{ errors.newPassword }}</p>
            </div>

            <!-- Confirm New Password -->
            <div>
                <label for="confirmNewPassword" class="block text-sm font-medium text-gray-700 mb-1">Confirm New Password</label>
                <div class="relative">
                    <input
                        id="confirmNewPassword"
                        v-model="confirmNewPassword"
                        :type="showConfirmNewPassword ? 'text' : 'password'"
                        :class="['input py-2.5 pr-10', errors.confirmNewPassword ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : '']"
                    />
                    <button
                        type="button"
                        @click="togglePassword('confirm')"
                        class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                    >
                        <svg v-if="showConfirmNewPassword" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
                        </svg>
                        <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                        </svg>
                    </button>
                </div>
                <p v-if="errors.confirmNewPassword" class="mt-1 text-sm text-danger-600">{{ errors.confirmNewPassword }}</p>
            </div>

            <!-- Change Password Button -->
            <div class="flex justify-end">
                <button @click="changePassword" :disabled="authStore.isLoading" class="btn-primary">
                    {{ authStore.isLoading ? 'Changing...' : 'Change Password' }}
                </button>
            </div>
        </div>

        <!-- Security Tab -->
        <div v-if="activeTab === 'security'" class="space-y-6">
            <!-- 2FA: Coming soon (backend endpoints not yet implemented) -->
            <div class="card">
                <div class="p-6">
                    <div class="flex items-center gap-4">
                        <div class="h-12 w-12 rounded-full flex items-center justify-center bg-gray-100">
                            <svg class="w-6 h-6 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                            </svg>
                        </div>
                        <div>
                            <h3 class="text-base font-medium text-gray-900">Two-Factor Authentication</h3>
                            <p class="text-sm text-gray-500">
                                Coming soon — Two-factor authentication will be available in a future update.
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
