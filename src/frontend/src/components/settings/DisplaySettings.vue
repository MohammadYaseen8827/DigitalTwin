<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useToast } from '@/composables/useToast'

const toast = useToast()

// Theme state
type Theme = 'light' | 'dark' | 'system'
const currentTheme = ref<Theme>('system')
const systemPrefersDark = ref(false)

// Language options
const languages = [
    { code: 'en', name: 'English' },
    { code: 'es', name: 'Español' },
    { code: 'fr', name: 'Français' },
    { code: 'de', name: 'Deutsch' },
    { code: 'zh', name: '中文' },
    { code: 'ja', name: '日本語' },
]
const selectedLanguage = ref('en')

// Date/time format options
const dateFormats = [
    { value: 'MM/DD/YYYY', label: 'MM/DD/YYYY' },
    { value: 'DD/MM/YYYY', label: 'DD/MM/YYYY' },
    { value: 'YYYY-MM-DD', label: 'YYYY-MM-DD' },
    { value: 'DD.MM.YYYY', label: 'DD.MM.YYYY' },
]
const selectedDateFormat = ref('MM/DD/YYYY')

const timeFormats = [
    { value: '12h', label: '12-hour (AM/PM)' },
    { value: '24h', label: '24-hour' },
]
const selectedTimeFormat = ref('24h')

// Number format
const numberFormats = [
    { value: 'dot-comma', label: '1,234.56' },
    { value: 'comma-dot', label: '1.234,56' },
    { value: 'space-comma', label: '1 234,56' },
]
const selectedNumberFormat = ref('dot-comma')

// Dashboard refresh rate
const refreshRates = [
    { value: '5000', label: '5 seconds' },
    { value: '10000', label: '10 seconds' },
    { value: '30000', label: '30 seconds' },
    { value: '60000', label: '1 minute' },
    { value: '0', label: 'Manual' },
]
const selectedRefreshRate = ref('10000')

// Compact mode
const compactMode = ref(false)

// High contrast mode
const highContrastMode = ref(false)

// Animation level
const animationLevels = [
    { value: 'full', label: 'Full' },
    { value: 'reduced', label: 'Reduced' },
    { value: 'none', label: 'None' },
]
const selectedAnimationLevel = ref('full')

// Check system preference
function checkSystemPreference() {
    if (window.matchMedia) {
        systemPrefersDark.value = window.matchMedia('(prefers-color-scheme: dark)').matches
    }
}

// Apply theme
function applyTheme(theme: Theme) {
    const isDark = theme === 'dark' || (theme === 'system' && systemPrefersDark.value)
    
    if (isDark) {
        document.documentElement.classList.add('dark')
    } else {
        document.documentElement.classList.remove('dark')
    }
    
    // Save preference
    localStorage.setItem('theme', theme)
}

// Watch for theme changes
watch(currentTheme, (newTheme) => {
    applyTheme(newTheme)
})

// Save all settings
function saveSettings() {
    const settings = {
        theme: currentTheme.value,
        language: selectedLanguage.value,
        dateFormat: selectedDateFormat.value,
        timeFormat: selectedTimeFormat.value,
        numberFormat: selectedNumberFormat.value,
        refreshRate: selectedRefreshRate.value,
        compactMode: compactMode.value,
        highContrastMode: highContrastMode.value,
        animationLevel: selectedAnimationLevel.value,
    }
    
    localStorage.setItem('displaySettings', JSON.stringify(settings))
    toast.success('Display settings saved!')
}

// Reset to defaults
function resetToDefaults() {
    currentTheme.value = 'system'
    selectedLanguage.value = 'en'
    selectedDateFormat.value = 'MM/DD/YYYY'
    selectedTimeFormat.value = '24h'
    selectedNumberFormat.value = 'dot-comma'
    selectedRefreshRate.value = '10000'
    compactMode.value = false
    highContrastMode.value = false
    selectedAnimationLevel.value = 'full'
    applyTheme('system')
}

// Load saved settings
onMounted(() => {
    checkSystemPreference()
    
    // Listen for system theme changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
        checkSystemPreference()
        if (currentTheme.value === 'system') {
            applyTheme('system')
        }
    })
    
    // Load saved settings
    const saved = localStorage.getItem('displaySettings')
    if (saved) {
        const settings = JSON.parse(saved)
        currentTheme.value = settings.theme || 'system'
        selectedLanguage.value = settings.language || 'en'
        selectedDateFormat.value = settings.dateFormat || 'MM/DD/YYYY'
        selectedTimeFormat.value = settings.timeFormat || '24h'
        selectedNumberFormat.value = settings.numberFormat || 'dot-comma'
        selectedRefreshRate.value = settings.refreshRate || '10000'
        compactMode.value = settings.compactMode || false
        highContrastMode.value = settings.highContrastMode || false
        selectedAnimationLevel.value = settings.animationLevel || 'full'
    }
    
    // Load saved theme
    const savedTheme = localStorage.getItem('theme') as Theme | null
    if (savedTheme) {
        currentTheme.value = savedTheme
    }
    
    applyTheme(currentTheme.value)
})
</script>

<template>
    <div class="space-y-6">
        <!-- Header -->
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
                <h2 class="text-xl font-semibold text-gray-900">Display Settings</h2>
                <p class="text-sm text-gray-600">Customize how the interface looks and behaves</p>
            </div>
            <div class="flex gap-3">
                <button @click="resetToDefaults" class="btn-secondary text-sm">
                    Reset to Defaults
                </button>
                <button @click="saveSettings" class="btn-primary text-sm">
                    Save Changes
                </button>
            </div>
        </div>

        <!-- Theme -->
        <div class="card">
            <div class="p-6">
                <h3 class="text-lg font-medium text-gray-900 mb-4">Theme</h3>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                    <!-- Light Theme -->
                    <label 
                        :class="[
                            'p-4 rounded-lg border-2 cursor-pointer transition-all',
                            currentTheme === 'light' 
                                ? 'border-primary-500 bg-primary-50 ring-2 ring-primary-200' 
                                : 'border-gray-200 hover:border-gray-300'
                        ]"
                    >
                        <input v-model="currentTheme" type="radio" value="light" class="sr-only" />
                        <div class="flex items-center justify-center h-12 mb-3 bg-white rounded-lg border border-gray-200">
                            <div class="w-8 h-8 bg-gray-100 rounded"></div>
                        </div>
                        <div class="text-center">
                            <span class="text-sm font-medium text-gray-900">Light</span>
                            <p class="text-xs text-gray-500 mt-1">Clean and bright</p>
                        </div>
                    </label>

                    <!-- Dark Theme -->
                    <label 
                        :class="[
                            'p-4 rounded-lg border-2 cursor-pointer transition-all',
                            currentTheme === 'dark' 
                                ? 'border-primary-500 bg-primary-50 ring-2 ring-primary-200' 
                                : 'border-gray-200 hover:border-gray-300'
                        ]"
                    >
                        <input v-model="currentTheme" type="radio" value="dark" class="sr-only" />
                        <div class="flex items-center justify-center h-12 mb-3 bg-gray-800 rounded-lg">
                            <div class="w-8 h-8 bg-gray-700 rounded"></div>
                        </div>
                        <div class="text-center">
                            <span class="text-sm font-medium text-gray-900">Dark</span>
                            <p class="text-xs text-gray-500 mt-1">Easy on the eyes</p>
                        </div>
                    </label>

                    <!-- System Theme -->
                    <label 
                        :class="[
                            'p-4 rounded-lg border-2 cursor-pointer transition-all',
                            currentTheme === 'system' 
                                ? 'border-primary-500 bg-primary-50 ring-2 ring-primary-200' 
                                : 'border-gray-200 hover:border-gray-300'
                        ]"
                    >
                        <input v-model="currentTheme" type="radio" value="system" class="sr-only" />
                        <div class="flex items-center justify-center h-12 mb-3 bg-gradient-to-r from-white to-gray-800 rounded-lg">
                            <div class="w-8 h-8 bg-gray-200 rounded-l"></div>
                            <div class="w-8 h-8 bg-gray-700 rounded-r"></div>
                        </div>
                        <div class="text-center">
                            <span class="text-sm font-medium text-gray-900">System</span>
                            <p class="text-xs text-gray-500 mt-1">Match your device</p>
                        </div>
                    </label>
                </div>
            </div>
        </div>

        <!-- Language & Region -->
        <div class="card">
            <div class="p-6">
                <h3 class="text-lg font-medium text-gray-900 mb-4">Language & Region</h3>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
                    <!-- Language -->
                    <div>
                        <label for="language" class="block text-sm font-medium text-gray-700 mb-1">Language</label>
                        <select id="language" v-model="selectedLanguage" class="input py-2.5">
                            <option v-for="lang in languages" :key="lang.code" :value="lang.code">
                                {{ lang.name }}
                            </option>
                        </select>
                    </div>

                    <!-- Date Format -->
                    <div>
                        <label for="dateFormat" class="block text-sm font-medium text-gray-700 mb-1">Date Format</label>
                        <select id="dateFormat" v-model="selectedDateFormat" class="input py-2.5">
                            <option v-for="format in dateFormats" :key="format.value" :value="format.value">
                                {{ format.label }}
                            </option>
                        </select>
                    </div>

                    <!-- Time Format -->
                    <div>
                        <label for="timeFormat" class="block text-sm font-medium text-gray-700 mb-1">Time Format</label>
                        <select id="timeFormat" v-model="selectedTimeFormat" class="input py-2.5">
                            <option v-for="format in timeFormats" :key="format.value" :value="format.value">
                                {{ format.label }}
                            </option>
                        </select>
                    </div>

                    <!-- Number Format -->
                    <div>
                        <label for="numberFormat" class="block text-sm font-medium text-gray-700 mb-1">Number Format</label>
                        <select id="numberFormat" v-model="selectedNumberFormat" class="input py-2.5">
                            <option v-for="format in numberFormats" :key="format.value" :value="format.value">
                                {{ format.label }}
                            </option>
                        </select>
                    </div>
                </div>
            </div>
        </div>

        <!-- Dashboard Preferences -->
        <div class="card">
            <div class="p-6">
                <h3 class="text-lg font-medium text-gray-900 mb-4">Dashboard Preferences</h3>
                <div class="space-y-4">
                    <!-- Auto Refresh -->
                    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 py-3 border-b border-gray-100 last:border-0">
                        <div>
                            <h4 class="text-sm font-medium text-gray-900">Dashboard Refresh Rate</h4>
                            <p class="text-sm text-gray-500">How often to refresh dashboard data</p>
                        </div>
                        <select v-model="selectedRefreshRate" class="input py-2 w-full sm:w-40">
                            <option v-for="rate in refreshRates" :key="rate.value" :value="rate.value">
                                {{ rate.label }}
                            </option>
                        </select>
                    </div>

                    <!-- Compact Mode -->
                    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 py-3 border-b border-gray-100 last:border-0">
                        <div>
                            <h4 class="text-sm font-medium text-gray-900">Compact Mode</h4>
                            <p class="text-sm text-gray-500">Reduce spacing for more information density</p>
                        </div>
                        <label class="relative inline-flex items-center cursor-pointer">
                            <input v-model="compactMode" type="checkbox" class="sr-only peer" />
                            <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                        </label>
                    </div>

                    <!-- High Contrast -->
                    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 py-3 border-b border-gray-100 last:border-0">
                        <div>
                            <h4 class="text-sm font-medium text-gray-900">High Contrast Mode</h4>
                            <p class="text-sm text-gray-500">Increase visibility for accessibility</p>
                        </div>
                        <label class="relative inline-flex items-center cursor-pointer">
                            <input v-model="highContrastMode" type="checkbox" class="sr-only peer" />
                            <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                        </label>
                    </div>

                    <!-- Animations -->
                    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 py-3 border-b border-gray-100 last:border-0">
                        <div>
                            <h4 class="text-sm font-medium text-gray-900">Animations</h4>
                            <p class="text-sm text-gray-500">Control interface animations</p>
                        </div>
                        <select v-model="selectedAnimationLevel" class="input py-2 w-full sm:w-40">
                            <option v-for="level in animationLevels" :key="level.value" :value="level.value">
                                {{ level.label }}
                            </option>
                        </select>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
