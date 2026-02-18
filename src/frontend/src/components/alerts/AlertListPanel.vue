<script setup lang="ts">
import { ref, computed } from 'vue'
import type { Alert, AlertFilter } from '@/types/alert'
import { SEVERITY_COLORS, SEVERITY_LABELS } from '@/types/alert'
import Card from '@/components/common/Card.vue'

interface Props {
    alerts: Alert[]
    loading?: boolean
    showBulkActions?: boolean
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
    showBulkActions: true
})

const emit = defineEmits<{
    'alert-click': [alert: Alert]
    'acknowledge': [id: string]
    'resolve': [id: string]
    'acknowledge-all': []
    'clear-resolved': []
    'filter-change': [filter: AlertFilter]
}>()

// Filter state
const severityFilter = ref<Alert['severity'][]>([])
const statusFilter = ref<Alert['status'][]>(['active'])
const searchQuery = ref('')
const sortBy = ref<'time' | 'severity'>('time')
const sortOrder = ref<'asc' | 'desc'>('desc')

// Filtered and sorted alerts
const filteredAlerts = computed(() => {
    let result = [...props.alerts]
    
    // Apply severity filter
    if (severityFilter.value.length > 0) {
        result = result.filter(a => severityFilter.value.includes(a.severity))
    }
    
    // Apply status filter
    if (statusFilter.value.length > 0) {
        result = result.filter(a => statusFilter.value.includes(a.status))
    }
    
    // Apply search query
    if (searchQuery.value) {
        const query = searchQuery.value.toLowerCase()
        result = result.filter(a => 
            a.title.toLowerCase().includes(query) ||
            a.description.toLowerCase().includes(query) ||
            a.machineName?.toLowerCase().includes(query) ||
            a.machineId.toLowerCase().includes(query)
        )
    }
    
    // Sort
    result.sort((a, b) => {
        if (sortBy.value === 'severity') {
            const severityOrder = { critical: 0, error: 1, warning: 2, info: 3 }
            const aOrder = severityOrder[a.severity]
            const bOrder = severityOrder[b.severity]
            return sortOrder.value === 'desc' ? bOrder - aOrder : aOrder - bOrder
        }
        // Sort by time
        const aTime = new Date(a.timestamp).getTime()
        const bTime = new Date(b.timestamp).getTime()
        return sortOrder.value === 'desc' ? bTime - aTime : aTime - bTime
    })
    
    return result
})

// Stats
const stats = computed(() => ({
    total: props.alerts.length,
    active: props.alerts.filter(a => a.status === 'active').length,
    critical: props.alerts.filter(a => a.severity === 'critical' && a.status === 'active').length,
    acknowledged: props.alerts.filter(a => a.status === 'acknowledged').length,
    resolved: props.alerts.filter(a => a.status === 'resolved').length
}))

// Methods
const toggleSeverityFilter = (severity: Alert['severity']) => {
    const index = severityFilter.value.indexOf(severity)
    if (index === -1) {
        severityFilter.value.push(severity)
    } else {
        severityFilter.value.splice(index, 1)
    }
    emit('filter-change', getCurrentFilter())
}

const toggleStatusFilter = (status: Alert['status']) => {
    const index = statusFilter.value.indexOf(status)
    if (index === -1) {
        statusFilter.value.push(status)
    } else {
        statusFilter.value.splice(index, 1)
    }
    emit('filter-change', getCurrentFilter())
}

const getCurrentFilter = (): AlertFilter => ({
    severity: severityFilter.value,
    status: statusFilter.value,
    searchQuery: searchQuery.value || undefined
})

const formatTime = (timestamp: Date | string): string => {
    const date = new Date(timestamp)
    const now = new Date()
    const diff = now.getTime() - date.getTime()
    
    if (diff < 60000) return 'Just now'
    if (diff < 3600000) return `${Math.floor(diff / 60000)}m ago`
    if (diff < 86400000) return `${Math.floor(diff / 3600000)}h ago`
    return date.toLocaleDateString()
}
</script>

<template>
    <Card class="h-full">
        <template #header>
            <div class="flex items-center justify-between">
                <div class="flex items-center gap-3">
                    <h2 class="text-lg font-semibold text-gray-900">Alerts</h2>
                    <span 
                        v-if="stats.active > 0"
                        class="badge badge-danger"
                    >
                        {{ stats.active }} active
                    </span>
                </div>
                
                <div class="flex items-center gap-2">
                    <button
                        v-if="showBulkActions && stats.active > 0"
                        class="btn-secondary text-xs"
                        @click="$emit('acknowledge-all')"
                    >
                        Acknowledge All
                    </button>
                    <button
                        v-if="showBulkActions && stats.acknowledged > 0"
                        class="btn-secondary text-xs"
                        @click="$emit('clear-resolved')"
                    >
                        Clear Resolved
                    </button>
                </div>
            </div>
        </template>
        
        <template #default>
            <div class="space-y-4">
                <!-- Stats -->
                <div class="grid grid-cols-4 gap-2 text-center">
                    <div class="p-2 bg-gray-50 rounded-lg">
                        <div class="text-lg font-bold text-gray-900">{{ stats.total }}</div>
                        <div class="text-xs text-gray-500">Total</div>
                    </div>
                    <div class="p-2 bg-red-50 rounded-lg">
                        <div class="text-lg font-bold text-red-600">{{ stats.critical }}</div>
                        <div class="text-xs text-gray-500">Critical</div>
                    </div>
                    <div class="p-2 bg-yellow-50 rounded-lg">
                        <div class="text-lg font-bold text-yellow-600">{{ stats.acknowledged }}</div>
                        <div class="text-xs text-gray-500">Ack'd</div>
                    </div>
                    <div class="p-2 bg-green-50 rounded-lg">
                        <div class="text-lg font-bold text-green-600">{{ stats.resolved }}</div>
                        <div class="text-xs text-gray-500">Resolved</div>
                    </div>
                </div>
                
                <!-- Search -->
                <div class="relative">
                    <input
                        v-model="searchQuery"
                        type="text"
                        placeholder="Search alerts..."
                        class="input pl-10"
                    />
                    <svg 
                        class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400"
                        fill="none" 
                        viewBox="0 0 24 24" 
                        stroke="currentColor"
                    >
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                    </svg>
                </div>
                
                <!-- Filters -->
                <div class="flex flex-wrap gap-2">
                    <!-- Severity filter -->
                    <div class="flex gap-1">
                        <button
                            v-for="severity in ['critical', 'warning', 'info']"
                            :key="severity"
                            type="button"
                            class="px-2 py-1 rounded-full text-xs font-medium transition-colors"
                            :class="[
                                severityFilter.includes(severity as Alert['severity'])
                                    ? 'ring-2 ring-offset-1'
                                    : 'bg-gray-100 text-gray-600'
                            ]"
                            :style="{
                                backgroundColor: severityFilter.includes(severity as Alert['severity']) 
                                    ? SEVERITY_COLORS[severity as Alert['severity']] 
                                    : undefined,
                                color: severityFilter.includes(severity as Alert['severity']) ? 'white' : undefined
                            }"
                            @click="toggleSeverityFilter(severity as Alert['severity'])"
                        >
                            {{ SEVERITY_LABELS[severity as Alert['severity']] }}
                        </button>
                    </div>
                    
                    <!-- Status filter -->
                    <div class="flex gap-1 ml-auto">
                        <button
                            v-for="status in ['active', 'acknowledged', 'resolved']"
                            :key="status"
                            type="button"
                            class="px-2 py-1 rounded-full text-xs font-medium transition-colors"
                            :class="[
                                statusFilter.includes(status as Alert['status'])
                                    ? 'bg-primary-100 text-primary-700'
                                    : 'bg-gray-100 text-gray-600'
                            ]"
                            @click="toggleStatusFilter(status as Alert['status'])"
                        >
                            {{ status.charAt(0).toUpperCase() + status.slice(1) }}
                        </button>
                    </div>
                </div>
                
                <!-- Sort -->
                <div class="flex items-center gap-2 text-sm">
                    <span class="text-gray-500">Sort by:</span>
                    <button
                        class="font-medium"
                        :class="sortBy === 'time' ? 'text-primary-600' : 'text-gray-700'"
                        @click="sortBy = 'time'"
                    >
                        Time
                    </button>
                    <span class="text-gray-300">|</span>
                    <button
                        class="font-medium"
                        :class="sortBy === 'severity' ? 'text-primary-600' : 'text-gray-700'"
                        @click="sortBy = 'severity'"
                    >
                        Severity
                    </button>
                    <button
                        class="ml-auto text-gray-400 hover:text-gray-600"
                        @click="sortOrder = sortOrder === 'asc' ? 'desc' : 'asc'"
                    >
                        <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path 
                                stroke-linecap="round" 
                                stroke-linejoin="round" 
                                stroke-width="2" 
                                :d="sortOrder === 'asc' ? 'M5 15l7-7 7 7' : 'M19 9l-7 7-7-7'"
                            />
                        </svg>
                    </button>
                </div>
                
                <!-- Alert list -->
                <div 
                    v-if="loading"
                    class="flex items-center justify-center py-8"
                >
                    <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
                </div>
                
                <div 
                    v-else-if="filteredAlerts.length === 0"
                    class="text-center py-8 text-gray-500"
                >
                    <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707.293l-2.414-2.414A1 1 0 006.586 13H4" />
                    </svg>
                    <p class="mt-2">No alerts found</p>
                </div>
                
                <div 
                    v-else
                    class="space-y-2 max-h-96 overflow-y-auto"
                >
                    <div
                        v-for="alert in filteredAlerts"
                        :key="alert.id"
                        class="p-3 rounded-lg border border-gray-100 hover:border-gray-200 hover:bg-gray-50 cursor-pointer transition-colors"
                        @click="$emit('alert-click', alert)"
                    >
                        <div class="flex items-start gap-3">
                            <!-- Severity indicator -->
                            <div 
                                class="flex-shrink-0 w-2 h-2 rounded-full mt-2"
                                :style="{ backgroundColor: SEVERITY_COLORS[alert.severity] }"
                            />
                            
                            <div class="flex-1 min-w-0">
                                <div class="flex items-center justify-between gap-2">
                                    <p class="text-sm font-medium text-gray-900 truncate">
                                        {{ alert.title }}
                                    </p>
                                    <span class="text-xs text-gray-500 flex-shrink-0">
                                        {{ formatTime(alert.timestamp) }}
                                    </span>
                                </div>
                                
                                <p class="text-xs text-gray-600 mt-0.5 truncate">
                                    {{ alert.machineName || alert.machineId }} • {{ alert.description }}
                                </p>
                                
                                <div class="flex items-center gap-2 mt-2">
                                    <span 
                                        class="text-xs px-2 py-0.5 rounded"
                                        :class="{
                                            'bg-red-100 text-red-700': alert.severity === 'critical',
                                            'bg-yellow-100 text-yellow-700': alert.severity === 'warning',
                                            'bg-blue-100 text-blue-700': alert.severity === 'info',
                                            'bg-purple-100 text-purple-700': alert.severity === 'error'
                                        }"
                                    >
                                        {{ SEVERITY_LABELS[alert.severity] }}
                                    </span>
                                    <span 
                                        class="text-xs px-2 py-0.5 rounded"
                                        :class="{
                                            'bg-green-100 text-green-700': alert.status === 'resolved',
                                            'bg-yellow-100 text-yellow-700': alert.status === 'acknowledged',
                                            'bg-gray-100 text-gray-700': alert.status === 'active'
                                        }"
                                    >
                                        {{ alert.status }}
                                    </span>
                                </div>
                            </div>
                            
                            <!-- Actions -->
                            <div 
                                v-if="alert.status === 'active'"
                                class="flex items-center gap-1"
                                @click.stop
                            >
                                <button
                                    class="p-1 text-gray-400 hover:text-primary-600 rounded"
                                    title="Acknowledge"
                                    @click="$emit('acknowledge', alert.id)"
                                >
                                    <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                                    </svg>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </Card>
</template>
