<script setup lang="ts">
import { ref } from 'vue'
import type { AlertRule } from '@/types/alert'

interface Props {
    rules: AlertRule[]
    loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
    loading: false
})

const emit = defineEmits<{
    'save-rule': [rule: Partial<AlertRule> & { id?: string }]
    'delete-rule': [id: string]
    'toggle-rule': [id: string, enabled: boolean]
}>()

const showForm = ref(false)
const editingRule = ref<Partial<AlertRule> | null>(null)

const formData = ref({
    name: '',
    description: '',
    severity: 'warning' as const,
    condition: {
        type: 'threshold' as const,
        sensor: 'vibration',
        threshold: 2.0,
        operator: 'gt' as const
    },
    notificationChannels: ['push'] as ('email' | 'sms' | 'push' | 'webhook')[],
    escalationEnabled: false,
    escalationDelay: 5
})

const resetForm = () => {
    formData.value = {
        name: '',
        description: '',
        severity: 'warning',
        condition: {
            type: 'threshold',
            sensor: 'vibration',
            threshold: 2.0,
            operator: 'gt'
        },
        notificationChannels: ['push'],
        escalationEnabled: false,
        escalationDelay: 5
    }
    editingRule.value = null
    showForm.value = false
}

const handleEdit = (rule: AlertRule) => {
    editingRule.value = rule
    formData.value = {
        name: rule.name,
        description: rule.description,
        severity: rule.severity,
        condition: { ...rule.condition },
        notificationChannels: [...rule.notificationChannels],
        escalationEnabled: rule.escalationEnabled,
        escalationDelay: rule.escalationDelay || 5
    }
    showForm.value = true
}

const handleSave = () => {
    if (!formData.value.name) return
    
    emit('save-rule', {
        ...formData.value,
        id: editingRule.value?.id
    })
    resetForm()
}

const handleDelete = (id: string) => {
    if (confirm('Are you sure you want to delete this rule?')) {
        emit('delete-rule', id)
    }
}

const toggleChannel = (channel: 'email' | 'sms' | 'push' | 'webhook') => {
    const idx = formData.value.notificationChannels.indexOf(channel)
    if (idx > -1) {
        formData.value.notificationChannels.splice(idx, 1)
    } else {
        formData.value.notificationChannels.push(channel)
    }
}

const getSeverityClass = (severity: string) => {
    switch (severity) {
        case 'critical': return 'bg-red-100 text-red-700 border-red-200'
        case 'warning': return 'bg-yellow-100 text-yellow-700 border-yellow-200'
        case 'error': return 'bg-purple-100 text-purple-700 border-purple-200'
        default: return 'bg-blue-100 text-blue-700 border-blue-200'
    }
}
</script>

<template>
    <div class="space-y-6">
        <!-- Header -->
        <div class="flex items-center justify-between">
            <div>
                <h3 class="text-lg font-semibold text-gray-900">Alert Rules</h3>
                <p class="text-sm text-gray-500">Configure automatic alert triggers</p>
            </div>
            <button 
                @click="showForm = true; resetForm()"
                class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors text-sm font-medium"
            >
                Add Rule
            </button>
        </div>

        <!-- Rules List -->
        <div v-if="rules.length === 0" class="text-center py-8 text-gray-500">
            No alert rules configured
        </div>

        <div v-else class="space-y-4">
            <div 
                v-for="rule in rules" 
                :key="rule.id"
                class="bg-white rounded-lg border border-gray-200 p-4"
            >
                <div class="flex items-start justify-between">
                    <div class="flex items-start gap-3">
                        <div class="flex items-center gap-2">
                            <button
                                @click="emit('toggle-rule', rule.id, !rule.enabled)"
                                :class="[
                                    'relative inline-flex h-6 w-11 items-center rounded-full transition-colors',
                                    rule.enabled ? 'bg-primary-600' : 'bg-gray-200'
                                ]"
                            >
                                <span
                                    :class="[
                                        'inline-block h-4 w-4 transform rounded-full bg-white transition-transform',
                                        rule.enabled ? 'translate-x-6' : 'translate-x-1'
                                    ]"
                                />
                            </button>
                        </div>
                        <div>
                            <div class="flex items-center gap-2">
                                <h4 class="font-medium text-gray-900">{{ rule.name }}</h4>
                                <span :class="['px-2 py-0.5 text-xs rounded-full border', getSeverityClass(rule.severity)]">
                                    {{ rule.severity }}
                                </span>
                            </div>
                            <p class="text-sm text-gray-500 mt-1">{{ rule.description }}</p>
                            <div class="flex items-center gap-4 mt-2 text-xs text-gray-400">
                                <span>Sensor: {{ rule.condition.sensor }}</span>
                                <span>Threshold: {{ rule.condition.operator }} {{ rule.condition.threshold }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="flex items-center gap-2">
                        <button 
                            @click="handleEdit(rule)"
                            class="p-2 text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-100"
                        >
                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                            </svg>
                        </button>
                        <button 
                            @click="handleDelete(rule.id)"
                            class="p-2 text-gray-400 hover:text-red-600 rounded-lg hover:bg-gray-100"
                        >
                            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                            </svg>
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Add/Edit Form Modal -->
        <div v-if="showForm" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div class="bg-white rounded-xl shadow-xl w-full max-w-lg mx-4 max-h-[90vh] overflow-y-auto">
                <div class="p-6 border-b border-gray-200">
                    <h3 class="text-lg font-semibold text-gray-900">
                        {{ editingRule ? 'Edit Rule' : 'New Alert Rule' }}
                    </h3>
                </div>
                
                <div class="p-6 space-y-4">
                    <!-- Name -->
                    <div>
                        <label class="block text-sm font-medium text-gray-700 mb-1">Rule Name</label>
                        <input 
                            v-model="formData.name"
                            type="text"
                            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                            placeholder="e.g., High Vibration Alert"
                        />
                    </div>

                    <!-- Description -->
                    <div>
                        <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
                        <textarea 
                            v-model="formData.description"
                            rows="2"
                            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                            placeholder="Describe when this alert triggers..."
                        ></textarea>
                    </div>

                    <!-- Severity -->
                    <div>
                        <label class="block text-sm font-medium text-gray-700 mb-1">Severity</label>
                        <select 
                            v-model="formData.severity"
                            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                        >
                            <option value="info">Info</option>
                            <option value="warning">Warning</option>
                            <option value="critical">Critical</option>
                            <option value="error">Error</option>
                        </select>
                    </div>

                    <!-- Condition -->
                    <div class="bg-gray-50 p-4 rounded-lg">
                        <h4 class="text-sm font-medium text-gray-700 mb-3">Condition</h4>
                        <div class="grid grid-cols-3 gap-3">
                            <div>
                                <label class="block text-xs text-gray-500 mb-1">Sensor</label>
                                <select 
                                    v-model="formData.condition.sensor"
                                    class="w-full px-2 py-2 text-sm border border-gray-300 rounded-lg"
                                >
                                    <option value="vibration">Vibration</option>
                                    <option value="temperature">Temperature</option>
                                    <option value="pressure">Pressure</option>
                                    <option value="rpm">RPM</option>
                                    <option value="power">Power</option>
                                </select>
                            </div>
                            <div>
                                <label class="block text-xs text-gray-500 mb-1">Operator</label>
                                <select 
                                    v-model="formData.condition.operator"
                                    class="w-full px-2 py-2 text-sm border border-gray-300 rounded-lg"
                                >
                                    <option value="gt">></option>
                                    <option value="gte">&ge;</option>
                                    <option value="lt"><</option>
                                    <option value="lte">&le;</option>
                                    <option value="eq">=</option>
                                </select>
                            </div>
                            <div>
                                <label class="block text-xs text-gray-500 mb-1">Threshold</label>
                                <input 
                                    v-model.number="formData.condition.threshold"
                                    type="number"
                                    step="0.1"
                                    class="w-full px-2 py-2 text-sm border border-gray-300 rounded-lg"
                                />
                            </div>
                        </div>
                    </div>

                    <!-- Notification Channels -->
                    <div>
                        <label class="block text-sm font-medium text-gray-700 mb-2">Notification Channels</label>
                        <div class="flex flex-wrap gap-2">
                            <button
                                v-for="channel in ['email', 'sms', 'push', 'webhook'] as const"
                                :key="channel"
                                @click="toggleChannel(channel)"
                                :class="[
                                    'px-3 py-1.5 text-sm rounded-lg border transition-colors capitalize',
                                    formData.notificationChannels.includes(channel)
                                        ? 'bg-primary-100 border-primary-300 text-primary-700'
                                        : 'bg-white border-gray-300 text-gray-600'
                                ]"
                            >
                                {{ channel }}
                            </button>
                        </div>
                    </div>

                    <!-- Escalation -->
                    <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                        <div>
                            <p class="font-medium text-gray-700">Enable Escalation</p>
                            <p class="text-xs text-gray-500">Automatically escalate if not acknowledged</p>
                        </div>
                        <button
                            @click="formData.escalationEnabled = !formData.escalationEnabled"
                            :class="[
                                'relative inline-flex h-6 w-11 items-center rounded-full transition-colors',
                                formData.escalationEnabled ? 'bg-primary-600' : 'bg-gray-200'
                            ]"
                        >
                            <span
                                :class="[
                                    'inline-block h-4 w-4 transform rounded-full bg-white transition-transform',
                                    formData.escalationEnabled ? 'translate-x-6' : 'translate-x-1'
                                ]"
                            />
                        </button>
                    </div>
                </div>

                <div class="p-6 border-t border-gray-200 flex justify-end gap-3">
                    <button 
                        @click="resetForm()"
                        class="px-4 py-2 text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
                    >
                        Cancel
                    </button>
                    <button 
                        @click="handleSave"
                        class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors"
                    >
                        {{ editingRule ? 'Update' : 'Create' }} Rule
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
