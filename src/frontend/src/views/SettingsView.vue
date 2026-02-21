<template>
  <main class="settings-page container">
    <div class="page-header glass-panel">
      <div>
        <p class="eyebrow">System Configuration</p>
        <h1>Settings</h1>
        <p class="subtitle">
          Manage your account preferences, notification settings, and system configurations
        </p>
      </div>
    </div>

    <div class="settings-grid">
      <!-- Account Settings -->
      <div class="settings-card glass-panel">
        <div class="card-header">
          <div class="header-icon">
            <User class="icon" />
          </div>
          <h3>Account Settings</h3>
        </div>
        <div class="card-content">
          <div class="setting-item">
            <label>Username</label>
            <input 
              type="text" 
              :value="user?.email || ''" 
              disabled
              class="setting-input disabled"
            >
          </div>
          <div class="setting-item">
            <label>Email Notifications</label>
            <div class="toggle-group">
              <label class="toggle-switch">
                <input type="checkbox" v-model="notifications.enabled">
                <span class="slider"></span>
              </label>
              <span class="toggle-label">{{ notifications.enabled ? 'Enabled' : 'Disabled' }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- System Preferences -->
      <div class="settings-card glass-panel">
        <div class="card-header">
          <div class="header-icon">
            <Settings class="icon" />
          </div>
          <h3>System Preferences</h3>
        </div>
        <div class="card-content">
          <div class="setting-item">
            <label>Theme</label>
            <select v-model="preferences.theme" class="setting-select">
              <option value="light">Light</option>
              <option value="dark">Dark</option>
              <option value="auto">Auto</option>
            </select>
          </div>
          <div class="setting-item">
            <label>Data Refresh Rate</label>
            <select v-model="preferences.refreshRate" class="setting-select">
              <option value="30">30 seconds</option>
              <option value="60">1 minute</option>
              <option value="300">5 minutes</option>
              <option value="600">10 minutes</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Security Settings -->
      <div class="settings-card glass-panel">
        <div class="card-header">
          <div class="header-icon">
            <Shield class="icon" />
          </div>
          <h3>Security</h3>
        </div>
        <div class="card-content">
          <div class="setting-item">
            <label>Two-Factor Authentication</label>
            <div class="toggle-group">
              <label class="toggle-switch">
                <input type="checkbox" v-model="security.twoFactorEnabled">
                <span class="slider"></span>
              </label>
              <span class="toggle-label">{{ security.twoFactorEnabled ? 'Enabled' : 'Disabled' }}</span>
            </div>
          </div>
          <div class="setting-item">
            <label>Session Timeout</label>
            <select v-model="security.sessionTimeout" class="setting-select">
              <option value="15">15 minutes</option>
              <option value="30">30 minutes</option>
              <option value="60">1 hour</option>
              <option value="120">2 hours</option>
            </select>
          </div>
        </div>
      </div>

      <!-- API Settings -->
      <div class="settings-card glass-panel">
        <div class="card-header">
          <div class="header-icon">
            <Code class="icon" />
          </div>
          <h3>API Configuration</h3>
        </div>
        <div class="card-content">
          <div class="setting-item">
            <label>API Endpoint</label>
            <input 
              type="text" 
              v-model="api.endpoint" 
              class="setting-input"
              :placeholder="defaultApiEndpoint"
            >
          </div>
          <div class="setting-item">
            <label>API Key</label>
            <div class="api-key-group">
              <input 
                :type="showApiKey ? 'text' : 'password'" 
                v-model="api.key" 
                class="setting-input"
                placeholder="Enter your production API key"
              >
              <button @click="showApiKey = !showApiKey" class="eye-button">
                <Eye v-if="!showApiKey" class="eye-icon" />
                <EyeOff v-else class="eye-icon" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="actions-footer">
      <BaseButton size="md" variant="outline" @click="resetSettings">
        Reset to Defaults
      </BaseButton>
      <BaseButton size="md" variant="primary" @click="saveSettings">
        Save Changes
      </BaseButton>
    </div>
  </main>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/auth.store'
import { useToast } from '@/composables/useToast'
import { User, Settings, Shield, Code, Eye, EyeOff } from 'lucide-vue-next'

const authStore = useAuthStore()
const showApiKey = ref(false)
const toast = useToast()

const user = computed(() => authStore.user)

const defaultApiEndpoint = computed(() => {
  return window.location.hostname === 'localhost' 
    ? 'https://localhost:5001'
    : `https://${window.location.hostname}/api`
})

const notifications = ref({
  enabled: true
})

const preferences = ref({
  theme: 'dark',
  refreshRate: 60
})

const security = ref({
  twoFactorEnabled: false,
  sessionTimeout: 30
})

const api = ref({
  endpoint: 'https://localhost:5001',
  key: ''
})

const resetSettings = () => {
  notifications.value = { enabled: true }
  preferences.value = { theme: 'dark', refreshRate: 60 }
  security.value = { twoFactorEnabled: false, sessionTimeout: 30 }
  api.value = { endpoint: 'https://localhost:5001', key: '' }
  toast.info('Settings reset to defaults')
}

const saveSettings = () => {
  // In a real app, this would save to backend
  localStorage.setItem('userSettings', JSON.stringify({
    notifications: notifications.value,
    preferences: preferences.value,
    security: security.value,
    api: api.value
  }))
  toast.success('Settings saved successfully!')
}
</script>

<style scoped>
.settings-page {
  padding: clamp(var(--space-16), 3vw, var(--space-24)) 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.page-header {
  padding: var(--space-20);
}

.eyebrow {
  margin: 0 0 var(--space-4);
  font-size: var(--font-size-sm);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-secondary);
}

.page-header h1 {
  margin: 0 0 var(--space-8);
  font-size: clamp(1.75rem, 4vw, 2.5rem);
  font-weight: 700;
  background: linear-gradient(135deg, var(--primary-color), #096dd9);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.subtitle {
  margin: 0;
  font-size: var(--font-size-base);
  color: var(--color-text-secondary);
  max-width: 600px;
  line-height: 1.6;
}

.settings-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: var(--space-16);
}

.settings-card {
  padding: var(--space-16);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.card-header {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  margin-bottom: var(--space-12);
}

.header-icon {
  width: 3rem;
  height: 3rem;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--primary-color), #096dd9);
}

.icon {
  width: 1.5rem;
  height: 1.5rem;
  color: white;
}

.card-header h3 {
  margin: 0;
  color: var(--color-text-primary);
  font-size: 1.25rem;
}

.setting-item {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  margin-bottom: var(--space-8);
}

.setting-item label {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
  font-weight: 500;
}

.setting-input, .setting-select {
  padding: var(--space-8);
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 6px;
  color: var(--color-text-primary);
  font-size: 0.9rem;
}

.setting-input:focus, .setting-select:focus {
  outline: none;
  border-color: var(--primary-color);
}

.setting-input.disabled {
  background: rgba(255, 255, 255, 0.02);
  color: var(--color-text-secondary);
  cursor: not-allowed;
}

.toggle-group {
  display: flex;
  align-items: center;
  gap: var(--space-8);
}

.toggle-switch {
  position: relative;
  display: inline-block;
  width: 50px;
  height: 24px;
}

.toggle-switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #ccc;
  transition: .4s;
  border-radius: 24px;
}

.slider:before {
  position: absolute;
  content: "";
  height: 16px;
  width: 16px;
  left: 4px;
  bottom: 4px;
  background-color: white;
  transition: .4s;
  border-radius: 50%;
}

input:checked + .slider {
  background-color: var(--primary-color);
}

input:checked + .slider:before {
  transform: translateX(26px);
}

.toggle-label {
  font-size: 0.875rem;
  color: var(--color-text-primary);
}

.api-key-group {
  display: flex;
  gap: var(--space-4);
}

.eye-button {
  padding: var(--space-8);
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.eye-icon {
  width: 1rem;
  height: 1rem;
  color: var(--color-text-secondary);
}

.actions-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-8);
  padding: var(--space-16);
  margin-top: var(--space-16);
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

@media (max-width: 768px) {
  .settings-grid {
    grid-template-columns: 1fr;
  }
  
  .actions-footer {
    flex-direction: column;
  }
  
  .api-key-group {
    flex-direction: column;
  }
}
</style>
