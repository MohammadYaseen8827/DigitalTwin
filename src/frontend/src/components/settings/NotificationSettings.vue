<script setup lang="ts">
import { ref } from 'vue'

// Notification preferences
const emailNotifications = ref(true)
const pushNotifications = ref(true)
const criticalAlertsOnly = ref(false)
const alertSeverityThreshold = ref('warning')
const quietHoursEnabled = ref(false)
const quietHoursStart = ref('22:00')
const quietHoursEnd = ref('07:00')
const dailyDigest = ref(true)
const dailyDigestTime = ref('09:00')
const weeklyReport = ref(false)
const maintenanceReminders = ref(true)
const predictionAlerts = ref(true)
const anomalyDetections = ref(true)
const systemUpdates = ref(false)

// Notification types
const notificationTypes = [
    { id: 'critical', label: 'Critical Alerts', description: 'Immediate attention required' },
    { id: 'warning', label: 'Warnings', description: 'Potential issues detected' },
    { id: 'info', label: 'Information', description: 'General notifications and updates' },
    { id: 'maintenance', label: 'Maintenance', description: 'Scheduled maintenance reminders' },
]

const selectedNotificationTypes = ref(['critical', 'warning', 'info'])

// Toggle notification type
function toggleNotificationType(typeId: string) {
    const index = selectedNotificationTypes.value.indexOf(typeId)
    if (index > -1) {
        selectedNotificationTypes.value.splice(index, 1)
    } else {
        selectedNotificationTypes.value.push(typeId)
    }
}

// Save preferences
function savePreferences() {
    // In production, call API to persist preferences
    if (import.meta.env.DEV) {
        console.debug('Saving notification preferences:', {
            emailNotifications: emailNotifications.value,
            pushNotifications: pushNotifications.value,
            alertSeverityThreshold: alertSeverityThreshold.value,
            quietHoursEnabled: quietHoursEnabled.value,
            quietHoursStart: quietHoursStart.value,
            quietHoursEnd: quietHoursEnd.value,
            dailyDigest: dailyDigest.value,
            dailyDigestTime: dailyDigestTime.value,
            weeklyReport: weeklyReport.value,
        selectedNotificationTypes: selectedNotificationTypes.value
        })
    }
    // Show success message
    alert('Notification preferences saved!')
}

// Reset to defaults
function resetToDefaults() {
    emailNotifications.value = true
    pushNotifications.value = true
    criticalAlertsOnly.value = false
    alertSeverityThreshold.value = 'warning'
    quietHoursEnabled.value = false
    quietHoursStart.value = '22:00'
    quietHoursEnd.value = '07:00'
    dailyDigest.value = true
    dailyDigestTime.value = '09:00'
    weeklyReport.value = false
    selectedNotificationTypes.value = ['critical', 'warning', 'info']
}
</script>

<template>
    <div class="space-y-6">
        <!-- Header -->
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
                <h2 class="text-xl font-semibold text-gray-900">Notification Settings</h2>
                <p class="text-sm text-gray-600">Manage how and when you receive notifications</p>
            </div>
            <div class="flex gap-3">
                <button @click="resetToDefaults" class="btn-secondary text-sm">
                    Reset to Defaults
                </button>
                <button @click="savePreferences" class="btn-primary text-sm">
                    Save Changes
                </button>
            </div>
        </div>

        <!-- Notification Channels -->
        <div class="card">
            <div class="p-6">
                <h3 class="text-lg font-medium text-gray-900 mb-4">Notification Channels</h3>
                <div class="space-y-4">
                    <!-- Email Notifications -->
                    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 py-3 border-b border-gray-100 last:border-0">
                        <div class="flex items-center gap-4">
                            <div class="h-10 w-10 rounded-full bg-primary-100 flex items-center justify-center">
                                <svg class="w-5 h-5 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                                </svg>
                            </div>
                            <div>
                                <h4 class="text-sm font-medium text-gray-900">Email Notifications</h4>
                                <p class="text-sm text-gray-500">Receive notifications via email</p>
                            </div>
                        </div>
                        <label class="relative inline-flex items-center cursor-pointer">
                            <input v-model="emailNotifications" type="checkbox" class="sr-only peer" />
                            <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                        </label>
                    </div>

                    <!-- Push Notifications -->
                    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 py-3 border-b border-gray-100 last:border-0">
                        <div class="flex items-center gap-4">
                            <div class="h-10 w-10 rounded-full bg-success-100 flex items-center justify-center">
                                <svg class="w-5 h-5 text-success-600" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                                </svg>
                            </div>
                            <div>
                                <h4 class="text-sm font-medium text-gray-900">Push Notifications</h4>
                                <p class="text-sm text-gray-500">Receive browser push notifications</p>
                            </div>
                        </div>
                        <label class="relative inline-flex items-center cursor-pointer">
                            <input v-model="pushNotifications" type="checkbox" class="sr-only peer" />
                            <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                        </label>
                    </div>
                </div>
            </div>
        </div>

        <!-- Alert Severity -->
        <div class="card">
            <div class="p-6">
                <h3 class="text-lg font-medium text-gray-900 mb-4">Alert Severity Threshold</h3>
                <p class="text-sm text-gray-600 mb-4">Select the minimum severity level for notifications</p>
                <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
                    <label 
                        v-for="severity in ['info', 'warning', 'critical', 'error']" 
                        :key="severity"
                        :class="[
                            'flex items-center gap-3 p-4 rounded-lg border-2 cursor-pointer transition-colors',
                            alertSeverityThreshold === severity 
                                ? 'border-primary-500 bg-primary-50' 
                                : 'border-gray-200 hover:border-gray-300'
                        ]"
                    >
                        <input 
                            v-model="alertSeverityThreshold" 
                            type="radio" 
                            :value="severity"
                            class="sr-only" 
                        />
                        <span :class="[
                            'h-3 w-3 rounded-full',
                            severity === 'info' ? 'bg-blue-500' : 
                            severity === 'warning' ? 'bg-yellow-500' : 
                            severity === 'critical' ? 'bg-orange-500' : 'bg-red-500'
                        ]"></span>
                        <span class="text-sm font-medium capitalize">{{ severity }}</span>
                    </label>
                </div>
            </div>
        </div>

        <!-- Quiet Hours -->
        <div class="card">
            <div class="p-6">
                <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-4">
                    <div>
                        <h3 class="text-lg font-medium text-gray-900">Quiet Hours</h3>
                        <p class="text-sm text-gray-600">Silence non-critical notifications during specific hours</p>
                    </div>
                    <label class="relative inline-flex items-center cursor-pointer">
                        <input v-model="quietHoursEnabled" type="checkbox" class="sr-only peer" />
                        <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                    </label>
                </div>
                
                <Transition
                    enter-active-class="transition-all duration-300"
                    enter-from-class="opacity-0 -translate-y-2"
                    enter-to-class="opacity-100 translate-y-0"
                    leave-active-class="transition-all duration-200"
                    leave-from-class="opacity-100 translate-y-0"
                    leave-to-class="opacity-0 -translate-y-2"
                >
                    <div v-if="quietHoursEnabled" class="flex flex-col sm:flex-row gap-4 pt-4 border-t border-gray-100">
                        <div class="flex-1">
                            <label for="quietStart" class="block text-sm font-medium text-gray-700 mb-1">Start Time</label>
                            <input
                                id="quietStart"
                                v-model="quietHoursStart"
                                type="time"
                                class="input py-2"
                            />
                        </div>
                        <div class="flex-1">
                            <label for="quietEnd" class="block text-sm font-medium text-gray-700 mb-1">End Time</label>
                            <input
                                id="quietEnd"
                                v-model="quietHoursEnd"
                                type="time"
                                class="input py-2"
                            />
                        </div>
                    </div>
                </Transition>
            </div>
        </div>

        <!-- Notification Types -->
        <div class="card">
            <div class="p-6">
                <h3 class="text-lg font-medium text-gray-900 mb-4">Notification Types</h3>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div 
                        v-for="type in notificationTypes" 
                        :key="type.id"
                        @click="toggleNotificationType(type.id)"
                        :class="[
                            'p-4 rounded-lg border-2 cursor-pointer transition-colors',
                            selectedNotificationTypes.includes(type.id)
                                ? 'border-primary-500 bg-primary-50'
                                : 'border-gray-200 hover:border-gray-300'
                        ]"
                    >
                        <div class="flex items-center gap-3">
                            <input 
                                type="checkbox" 
                                :checked="selectedNotificationTypes.includes(type.id)"
                                class="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                                readonly
                            />
                            <div>
                                <h4 class="text-sm font-medium text-gray-900">{{ type.label }}</h4>
                                <p class="text-xs text-gray-500">{{ type.description }}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Reports -->
        <div class="card">
            <div class="p-6">
                <h3 class="text-lg font-medium text-gray-900 mb-4">Reports & Digests</h3>
                <div class="space-y-4">
                    <!-- Daily Digest -->
                    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 py-3 border-b border-gray-100 last:border-0">
                        <div>
                            <h4 class="text-sm font-medium text-gray-900">Daily Digest</h4>
                            <p class="text-sm text-gray-500">Receive a daily summary of system status</p>
                        </div>
                        <div class="flex items-center gap-3">
                            <label v-if="dailyDigest" class="relative inline-flex items-center">
                                <input v-model="dailyDigestTime" type="time" class="input py-1.5 pr-8" />
                            </label>
                            <label class="relative inline-flex items-center cursor-pointer">
                                <input v-model="dailyDigest" type="checkbox" class="sr-only peer" />
                                <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                            </label>
                        </div>
                    </div>

                    <!-- Weekly Report -->
                    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 py-3 border-b border-gray-100 last:border-0">
                        <div>
                            <h4 class="text-sm font-medium text-gray-900">Weekly Report</h4>
                            <p class="text-sm text-gray-500">Receive a weekly analysis report</p>
                        </div>
                        <label class="relative inline-flex items-center cursor-pointer">
                            <input v-model="weeklyReport" type="checkbox" class="sr-only peer" />
                            <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                        </label>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
