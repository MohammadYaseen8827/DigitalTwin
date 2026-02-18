<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

// API configuration
const apiUrl = ref('https://api.digitaltwin.example.com')
const apiVersion = ref('v1')
const connectionStatus = ref('connected')
const lastConnected = ref(new Date().toISOString())

// Data export options
const exportFormat = ref('csv')
const exportDateRange = ref('7d')
const isExporting = ref(false)

// Cache management
const cacheSize = ref('256 MB')
const isClearingCache = ref(false)

// System info
const systemVersion = ref('1.0.0')
const databaseStatus = ref('healthy')
const storageUsed = ref('1.2 GB')
const storageTotal = ref('10 GB')

// Export data
async function exportData() {
    isExporting.value = true
    
    // Simulate export
    await new Promise(resolve => setTimeout(resolve, 2000))
    
    // Create and download file
    const data = {
        exportDate: new Date().toISOString(),
        format: exportFormat.value,
        dateRange: exportDateRange.value
    }
    
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `digital-twin-export-${new Date().toISOString().split('T')[0]}.json`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
    
    isExporting.value = false
    alert('Data exported successfully!')
}

// Clear cache
async function clearCache() {
    if (!confirm('Are you sure you want to clear the cache? This may temporarily slow down the application.')) {
        return
    }
    
    isClearingCache.value = true
    
    // Simulate cache clearing
    await new Promise(resolve => setTimeout(resolve, 1500))
    
    cacheSize.value = '0 MB'
    isClearingCache.value = false
    alert('Cache cleared successfully!')
}

// Refresh connection status
function refreshConnection() {
    connectionStatus.value = 'connecting'
    
    // Simulate connection check
    setTimeout(() => {
        connectionStatus.value = 'connected'
        lastConnected.value = new Date().toISOString()
    }, 1000)
}

// Reset to defaults
function resetToDefaults() {
    if (!confirm('Are you sure you want to reset all system settings to defaults?')) {
        return
    }
    
    exportFormat.value = 'csv'
    exportDateRange.value = '7d'
    alert('System settings reset to defaults!')
}

// Check if user is admin
const isAdmin = ref(true) // In production, check authStore.isAdmin
</script>

<template>
    <div class="space-y-6">
        <!-- Header -->
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
                <h2 class="text-xl font-semibold text-gray-900">System Settings</h2>
                <p class="text-sm text-gray-600">Admin configuration and system management</p>
            </div>
            <button @click="resetToDefaults" class="btn-secondary text-sm">
                Reset to Defaults
            </button>
        </div>

        <!-- API Configuration -->
        <div class="card">
            <div class="p-6">
                <div class="flex items-center justify-between mb-4">
                    <h3 class="text-lg font-medium text-gray-900">API Configuration</h3>
                    <span :class="[
                        'px-2.5 py-0.5 rounded-full text-xs font-medium',
                        connectionStatus === 'connected' ? 'bg-success-100 text-success-800' :
                        connectionStatus === 'connecting' ? 'bg-warning-100 text-warning-800' :
                        'bg-danger-100 text-danger-800'
                    ]">
                        {{ connectionStatus === 'connected' ? 'Connected' : connectionStatus === 'connecting' ? 'Connecting...' : 'Disconnected' }}
                    </span>
                </div>
                
                <div class="space-y-4">
                    <!-- API URL -->
                    <div>
                        <label for="apiUrl" class="block text-sm font-medium text-gray-700 mb-1">API Endpoint URL</label>
                        <input
                            id="apiUrl"
                            v-model="apiUrl"
                            type="url"
                            class="input py-2.5"
                            readonly
                        />
                    </div>

                    <!-- API Version -->
                    <div>
                        <label for="apiVersion" class="block text-sm font-medium text-gray-700 mb-1">API Version</label>
                        <input
                            id="apiVersion"
                            v-model="apiVersion"
                            type="text"
                            class="input py-2.5"
                            readonly
                        />
                    </div>

                    <!-- Connection Info -->
                    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 pt-4 border-t border-gray-100">
                        <span class="text-sm text-gray-500">
                            Last connected: {{ new Date(lastConnected).toLocaleString() }}
                        </span>
                        <button @click="refreshConnection" class="btn-secondary text-sm py-1.5">
                            <svg class="w-4 h-4 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                            </svg>
                            Refresh Connection
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Data Export -->
        <div class="card">
            <div class="p-6">
                <h3 class="text-lg font-medium text-gray-900 mb-4">Data Export</h3>
                <p class="text-sm text-gray-600 mb-4">Export your system data for backup or analysis</p>
                
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-6">
                    <!-- Export Format -->
                    <div>
                        <label for="exportFormat" class="block text-sm font-medium text-gray-700 mb-1">Export Format</label>
                        <select id="exportFormat" v-model="exportFormat" class="input py-2.5">
                            <option value="csv">CSV</option>
                            <option value="json">JSON</option>
                            <option value="xlsx">Excel (XLSX)</option>
                            <option value="pdf">PDF</option>
                        </select>
                    </div>

                    <!-- Date Range -->
                    <div>
                        <label for="exportDateRange" class="block text-sm font-medium text-gray-700 mb-1">Date Range</label>
                        <select id="exportDateRange" v-model="exportDateRange" class="input py-2.5">
                            <option value="1d">Last 24 hours</option>
                            <option value="7d">Last 7 days</option>
                            <option value="30d">Last 30 days</option>
                            <option value="90d">Last 90 days</option>
                            <option value="1y">Last year</option>
                            <option value="all">All time</option>
                        </select>
                    </div>
                </div>

                <button 
                    @click="exportData" 
                    :disabled="isExporting"
                    class="btn-primary"
                >
                    <svg v-if="isExporting" class="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24" aria-hidden="true">
                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    <svg v-else class="-ml-1 mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                    </svg>
                    {{ isExporting ? 'Exporting...' : 'Export Data' }}
                </button>
            </div>
        </div>

        <!-- Cache Management -->
        <div class="card">
            <div class="p-6">
                <div class="flex items-center justify-between mb-4">
                    <div>
                        <h3 class="text-lg font-medium text-gray-900">Cache Management</h3>
                        <p class="text-sm text-gray-600">Clear cached data to free up space</p>
                    </div>
                    <div class="text-right">
                        <span class="text-2xl font-semibold text-gray-900">{{ cacheSize }}</span>
                        <p class="text-sm text-gray-500">Current cache size</p>
                    </div>
                </div>
                
                <div class="bg-gray-50 rounded-lg p-4 mb-4">
                    <div class="flex items-center justify-between text-sm mb-2">
                        <span class="text-gray-600">Storage Used</span>
                        <span class="font-medium">{{ storageUsed }} / {{ storageTotal }}</span>
                    </div>
                    <div class="w-full bg-gray-200 rounded-full h-2">
                        <div class="bg-primary-600 h-2 rounded-full" :style="{ width: `${(parseFloat(storageUsed) / parseFloat(storageTotal)) * 100}%` }"></div>
                    </div>
                </div>

                <button 
                    @click="clearCache" 
                    :disabled="isClearingCache"
                    class="btn-secondary"
                >
                    <svg v-if="isClearingCache" class="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24" aria-hidden="true">
                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    <svg v-else class="-ml-1 mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                    {{ isClearingCache ? 'Clearing...' : 'Clear Cache' }}
                </button>
            </div>
        </div>

        <!-- System Information -->
        <div class="card">
            <div class="p-6">
                <h3 class="text-lg font-medium text-gray-900 mb-4">System Information</h3>
                <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                    <!-- Version -->
                    <div class="bg-gray-50 rounded-lg p-4">
                        <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">Version</p>
                        <p class="mt-1 text-lg font-semibold text-gray-900">{{ systemVersion }}</p>
                    </div>

                    <!-- Database Status -->
                    <div class="bg-gray-50 rounded-lg p-4">
                        <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">Database</p>
                        <div class="flex items-center gap-2 mt-1">
                            <span :class="[
                                'h-2.5 w-2.5 rounded-full',
                                databaseStatus === 'healthy' ? 'bg-success-500' : 'bg-danger-500'
                            ]"></span>
                            <span class="text-lg font-semibold text-gray-900 capitalize">{{ databaseStatus }}</span>
                        </div>
                    </div>

                    <!-- Active Sessions -->
                    <div class="bg-gray-50 rounded-lg p-4">
                        <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">Active Sessions</p>
                        <p class="mt-1 text-lg font-semibold text-gray-900">1</p>
                    </div>

                    <!-- Uptime -->
                    <div class="bg-gray-50 rounded-lg p-4">
                        <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">Uptime</p>
                        <p class="mt-1 text-lg font-semibold text-gray-900">99.9%</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Warning for non-admin -->
        <div v-if="!isAdmin" class="card border-warning-200 bg-warning-50">
            <div class="p-6 flex items-start gap-4">
                <svg class="w-6 h-6 text-warning-600 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
                <div>
                    <h3 class="text-sm font-medium text-warning-800">Admin Access Required</h3>
                    <p class="mt-1 text-sm text-warning-700">
                        Some system settings are only accessible to administrators. Contact your system administrator if you need to modify these settings.
                    </p>
                </div>
            </div>
        </div>
    </div>
</template>
