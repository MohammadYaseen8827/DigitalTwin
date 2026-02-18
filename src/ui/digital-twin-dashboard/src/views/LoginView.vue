<template>
  <div class="login-container">
    <div class="login-box">
      <h1>Digital Twin Platform</h1>
      <p>Please login to continue</p>
      
      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label for="email">Email</label>
          <input 
            type="email" 
            id="email" 
            v-model="email" 
            required 
            placeholder="admin@example.com"
          />
        </div>
        
        <div class="form-group">
          <label for="password">Password</label>
          <input 
            type="password" 
            id="password" 
            v-model="password" 
            required 
          />
        </div>
        
        <button type="submit" :disabled="loading">
          {{ loading ? 'Logging in...' : 'Login' }}
        </button>
        
        <!-- Debug button for testing -->
        <button 
          type="button" 
          @click="debugLogin" 
          class="debug-btn"
          style="margin-top: 1rem; background: #28a745; color: white;"
        >
          Debug Login (Skip Auth)
        </button>
        
        <p v-if="error" class="error-message">{{ error }}</p>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import axiosClient from '@/api/axiosClient'
import { useAuthStore, SecureTokenManager } from '@/stores/auth.store'

const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')
const router = useRouter()
const authStore = useAuthStore()

async function handleLogin() {
  loading.value = true
  error.value = ''
  
  try {
    // For testing purposes, accept any credentials
    if (email.value && password.value) {
      // Mock user data for testing
      const mockUser = {
        id: '1',
        email: email.value,
        fullName: 'Test User',
        userName: email.value.split('@')[0],
        roles: ['Admin', 'User']
      }
      
      // Mock token for testing
      const mockToken = 'mock-jwt-token-for-testing'
      const mockRefreshToken = 'mock-refresh-token-for-testing'
      
      // Store authentication data
      authStore.setToken(mockToken, mockUser, mockRefreshToken)
      
      console.log('Login successful:', { 
        user: mockUser, 
        token: mockToken,
        isAuthenticated: authStore.isAuthenticated,
        storedToken: SecureTokenManager.getToken(),
        storedUser: SecureTokenManager.getUser()
      })
      
      // Small delay to ensure state is set
      setTimeout(() => {
        console.log('Before redirect - auth state:', {
          isAuthenticated: authStore.isAuthenticated,
          user: authStore.user,
          token: authStore.token
        })
        router.push('/')
      }, 100)
    } else {
      error.value = 'Please enter email and password'
    }
  } catch (err: any) {
    error.value = err.response?.data?.error || err.response?.data?.message || err.response?.data || 'Login failed'
    console.error('Login error:', err)
  } finally {
    loading.value = false
  }
}

// Debug login function
function debugLogin() {
  console.log('Debug login triggered')
  
  const mockUser = {
    id: '1',
    email: 'debug@test.com',
    fullName: 'Debug User',
    userName: 'debug',
    roles: ['Admin', 'User']
  }
  
  const mockToken = 'debug-mock-token'
  const mockRefreshToken = 'debug-refresh-token'
  
  // Force set authentication
  authStore.setToken(mockToken, mockUser, mockRefreshToken)
  
  console.log('Debug login complete:', {
    user: mockUser,
    token: mockToken,
    isAuthenticated: authStore.isAuthenticated,
    storedToken: SecureTokenManager.getToken(),
    storedUser: SecureTokenManager.getUser()
  })
  
  // Force redirect to home
  router.push('/')
}
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background-color: var(--bg-secondary, #f0f2f5);
}

.login-box {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 400px;
}

h1 {
  margin-top: 0;
  color: var(--primary-color, #1890ff);
}

.form-group {
  margin-bottom: 1rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
}

input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d9d9d9;
  border-radius: 4px;
}

button {
  width: 100%;
  padding: 0.75rem;
  background-color: var(--primary-color, #1890ff);
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: bold;
}

button:disabled {
  background-color: #d9d9d9;
}

.error-message {
  color: red;
  margin-top: 1rem;
}
</style>
