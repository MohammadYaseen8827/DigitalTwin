<template>
  <div class="login-container">
    <div class="login-box glass-panel">
      <div class="login-header">
        <div class="logo-icon">🏭</div>
        <h1>Digital Twin Platform</h1>
        <p>Sign in to continue</p>
      </div>
      
      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label for="email">Email</label>
          <div class="input-wrapper">
            <input 
              type="email" 
              id="email" 
              v-model="email" 
              required 
              placeholder="admin@example.com"
              :class="{ 'has-error': emailError }"
              @blur="validateEmail"
            />
            <span v-if="emailError" class="error-text">{{ emailError }}</span>
          </div>
        </div>
        
        <div class="form-group">
          <label for="password">Password</label>
          <div class="input-wrapper">
            <input 
              type="password" 
              id="password" 
              v-model="password" 
              required 
              placeholder="Enter your password"
              :class="{ 'has-error': passwordError }"
              @blur="validatePassword"
            />
            <span v-if="passwordError" class="error-text">{{ passwordError }}</span>
          </div>
        </div>
        
        <button type="submit" :disabled="loading" class="login-btn">
          <span v-if="loading" class="loading-spinner"></span>
          <span :class="{ 'loading-content': loading }">
            {{ loading ? 'Signing in...' : 'Sign In' }}
          </span>
        </button>
        
        <transition name="fade">
          <p v-if="error" class="error-message">
            <span class="error-icon">⚠</span>
            {{ error }}
          </p>
        </transition>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login as apiLogin } from '@/services/auth.service'
import { useAuthStore } from '@/stores/auth.store'

const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')
const emailError = ref('')
const passwordError = ref('')
const router = useRouter()
const authStore = useAuthStore()

function validateEmail() {
  if (!email.value) {
    emailError.value = 'Email is required'
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value)) {
    emailError.value = 'Please enter a valid email'
  } else {
    emailError.value = ''
  }
}

function validatePassword() {
  if (!password.value) {
    passwordError.value = 'Password is required'
  } else if (password.value.length < 6) {
    passwordError.value = 'Password must be at least 6 characters'
  } else {
    passwordError.value = ''
  }
}

async function handleLogin() {
  // Validate before submitting
  validateEmail()
  validatePassword()
  
  if (emailError.value || passwordError.value) {
    return
  }
  
  loading.value = true
  error.value = ''
  
  try {
    const response = await apiLogin({
      email: email.value,
      password: password.value
    })
    
    const user = response.user
    authStore.setToken(response.accessToken, user as any, response.refreshToken)
    router.push('/')
  } catch (err: any) {
    error.value = err.response?.data?.error || err.response?.data?.message || err.response?.data || 'Login failed. Please check your credentials.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  padding: var(--space-16);
  background: var(--gradient-hero);
}

.login-box {
  width: 100%;
  max-width: 420px;
  padding: var(--space-32);
  background: var(--color-glass);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-medium);
  backdrop-filter: blur(16px);
}

.login-header {
  text-align: center;
  margin-bottom: var(--space-32);
}

.logo-icon {
  font-size: 2.5rem;
  width: 64px;
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto var(--space-16);
  background: var(--color-primary);
  border-radius: var(--radius-lg);
  box-shadow: var(--glow-primary);
}

.login-header h1 {
  font-size: var(--font-size-xl);
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0 0 var(--space-8);
}

.login-header p {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  margin: 0;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-20);
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
}

.form-group label {
  font-size: var(--font-size-sm);
  font-weight: 500;
  color: var(--color-text-primary);
}

.input-wrapper {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

input {
  width: 100%;
  padding: var(--space-12) var(--space-16);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-size: var(--font-size-base);
  color: var(--color-text-primary);
  transition: all 0.2s ease;
}

input::placeholder {
  color: var(--color-text-secondary);
  opacity: 0.6;
}

input:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-primary) 25%, transparent);
}

input.has-error {
  border-color: var(--color-error);
}

input.has-error:focus {
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-error) 25%, transparent);
}

.error-text {
  font-size: var(--font-size-xs);
  color: var(--color-error);
}

.login-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-8);
  width: 100%;
  padding: var(--space-14) var(--space-20);
  background: linear-gradient(135deg, var(--color-primary), color-mix(in srgb, var(--color-primary-dark) 80%, var(--color-primary) 20%));
  color: white;
  border: none;
  border-radius: var(--radius-md);
  font-size: var(--font-size-base);
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  min-height: 48px;
  box-shadow: var(--shadow-subtle);
}

.login-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: var(--shadow-medium);
}

.login-btn:active:not(:disabled) {
  transform: translateY(0);
}

.login-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.loading-spinner {
  width: 18px;
  height: 18px;
  border: 2px solid currentColor;
  border-top-color: transparent;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

.loading-content {
  opacity: 0.7;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.error-message {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  padding: var(--space-12) var(--space-16);
  background: color-mix(in srgb, var(--color-error) 15%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-error) 40%, transparent);
  border-radius: var(--radius-md);
  color: var(--color-error);
  font-size: var(--font-size-sm);
  margin: 0;
}

.error-icon {
  font-size: var(--font-size-lg);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
