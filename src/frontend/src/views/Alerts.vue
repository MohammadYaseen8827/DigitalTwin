<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import SidebarNav from '@/components/dashboard/SidebarNav.vue'
import HeaderBar from '@/components/dashboard/HeaderBar.vue'
import { AlertListPanel, AlertDetailModal, AlertRulesConfig } from '@/components/alerts'
import { useAlertsStore } from '@/stores/alerts'
import type { Alert } from '@/types/alert'

const router = useRouter()
const alertsStore = useAlertsStore()

// UI State
const sidebarCollapsed = ref(false)
const activeTab = ref<'alerts' | 'rules'>('alerts')
const selectedAlert = ref<Alert | null>(null)
const showDetailModal = ref(false)

// Use alerts from store instead of mock data
const alerts = computed(() => alertsStore.alerts)

// Computed
const activeAlertsCount = computed(() => alertsStore.activeAlerts.length)
const criticalAlertsCount = computed(() => alertsStore.criticalAlerts.length)

// Methods
const handleAlertClick = (alert: Alert) => {
    selectedAlert.value = alert
    showDetailModal.value = true
}

const handleAcknowledge = async (id: string) => {
    await alertsStore.acknowledgeAlert(id)
}

const handleResolve = async (id: string) => {
    await alertsStore.resolveAlert(id)
}

const handleAcknowledgeAll = async () => {
    for (const alert of alertsStore.activeAlerts) {
        await alertsStore.acknowledgeAlert(alert.id)
    }
}

const handleClearResolved = () => {
    // Clear resolved alerts from the list
    alertsStore.clearAlerts()
}

// Alert rules: empty until backend AlertRules API is used; show empty state instead of mock data
const alertRules = ref<Array<{
    id: string
    name: string
    description: string
    enabled: boolean
    severity: 'info' | 'warning' | 'critical' | 'error'
    condition: { type: string; sensor?: string; threshold?: number; operator?: string }
    notificationChannels: string[]
    escalationEnabled: boolean
    escalationDelay?: number
    createdAt: Date
    updatedAt: Date
}>>([])

const handleSaveRule = (rule: any) => {
    if (rule.id) {
        const index = alertRules.value.findIndex(r => r.id === rule.id)
        if (index !== -1) {
            alertRules.value[index] = { ...rule, updatedAt: new Date() }
        }
    } else {
        alertRules.value.push({
            ...rule,
            id: `rule-${Date.now()}`,
            createdAt: new Date(),
            updatedAt: new Date()
        })
    }
}

const handleDeleteRule = (id: string) => {
    alertRules.value = alertRules.value.filter(r => r.id !== id)
}

const handleToggleRule = (id: string, enabled: boolean) => {
    const rule = alertRules.value.find(r => r.id === id)
    if (rule) {
        rule.enabled = enabled
        rule.updatedAt = new Date()
    }
}

const handleViewPrediction = (predictionId: string) => {
    router.push(`/predictions/${predictionId}`)
}

onMounted(() => {
    alertsStore.fetchAlerts()
})
</script>

<template>
    <div class="min-h-screen bg-gray-50">
        <SidebarNav v-model:collapsed="sidebarCollapsed" />
        
        <div :class="['transition-all duration-300', sidebarCollapsed ? 'ml-16' : 'ml-64']">
            <HeaderBar title="Alerts" subtitle="Monitor and manage system alerts" />
            
            <main class="p-6">
                <!-- Summary cards -->
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                    <div class="bg-white rounded-xl shadow-sm border p-4">
                        <div class="flex items-center justify-between">
                            <div>
                                <p class="text-sm text-gray-500">Active Alerts</p>
                                <p class="text-2xl font-bold text-gray-900">{{ activeAlertsCount }}</p>
                            </div>
                            <div class="w-12 h-12 rounded-full bg-yellow-100 flex items-center justify-center">
                                <svg class="w-6 h-6 text-yellow-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                                </svg>
                            </div>
                        </div>
                    </div>
                    
                    <div class="bg-white rounded-xl shadow-sm border p-4">
                        <div class="flex items-center justify-between">
                            <div>
                                <p class="text-sm text-gray-500">Critical</p>
                                <p class="text-2xl font-bold text-red-600">{{ criticalAlertsCount }}</p>
                            </div>
                            <div class="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center">
                                <svg class="w-6 h-6 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                                </svg>
                            </div>
                        </div>
                    </div>
                    
                    <div class="bg-white rounded-xl shadow-sm border p-4">
                        <div class="flex items-center justify-between">
                            <div>
                                <p class="text-sm text-gray-500">Alert Rules</p>
                                <p class="text-2xl font-bold text-gray-900">{{ alertRules.length }}</p>
                            </div>
                            <div class="w-12 h-12 rounded-full bg-blue-100 flex items-center justify-center">
                                <svg class="w-6 h-6 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                                </svg>
                            </div>
                        </div>
                    </div>
                </div>
                
                <!-- Tabs -->
                <div class="border-b border-gray-200 mb-6">
                    <nav class="flex gap-8">
                        <button
                            class="py-3 text-sm font-medium border-b-2 transition-colors"
                            :class="[activeTab === 'alerts' ? 'border-primary-500 text-primary-600' : 'border-transparent text-gray-500 hover:text-gray-700']"
                            @click="activeTab = 'alerts'"
                        >
                            Alerts
                        </button>
                        <button
                            class="py-3 text-sm font-medium border-b-2 transition-colors"
                            :class="[activeTab === 'rules' ? 'border-primary-500 text-primary-600' : 'border-transparent text-gray-500 hover:text-gray-700']"
                            @click="activeTab = 'rules'"
                        >
                            Alert Rules
                        </button>
                    </nav>
                </div>
                
                <!-- Alerts Tab -->
                <div v-if="activeTab === 'alerts'">
                    <AlertListPanel
                        :alerts="alerts"
                        :loading="alertsStore.isLoading"
                        @alert-click="handleAlertClick"
                        @acknowledge="handleAcknowledge"
                        @resolve="handleResolve"
                        @acknowledge-all="handleAcknowledgeAll"
                        @clear-resolved="handleClearResolved"
                    />
                </div>
                
                <!-- Rules Tab -->
                <div v-if="activeTab === 'rules'">
                    <AlertRulesConfig
                        :rules="alertRules"
                        :loading="false"
                        @save-rule="handleSaveRule"
                        @delete-rule="handleDeleteRule"
                        @toggle-rule="handleToggleRule"
                    />
                </div>
            </main>
        </div>
        
        <!-- Detail Modal -->
        <AlertDetailModal
            :show="showDetailModal"
            :alert="selectedAlert"
            @close="showDetailModal = false"
            @acknowledge="handleAcknowledge"
            @resolve="handleResolve"
            @view-prediction="handleViewPrediction"
        />
    </div>
</template>
