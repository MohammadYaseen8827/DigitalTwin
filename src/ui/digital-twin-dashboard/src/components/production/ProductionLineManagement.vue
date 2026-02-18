<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  fetchProductionLines,
  createProductionLine,
  updateProductionLine,
  deleteProductionLine
} from '@/services/productionLines.service'
import { fetchMachines } from '@/services/machines.service'
import type { 
  ProductionLineDto, 
  ProductionLineCreateDto,
  ProductionLineUpdateDto
} from '@/api/types/index'
import { 
  Factory, 
  Plus, 
  Edit, 
  Trash2,
  Settings,
  Filter,
  Search,
  RefreshCw,
  Link,
  Unlink
} from 'lucide-vue-next'

const toast = useToast()

// State
const productionLines: any = ref([])
const machines: any = ref([])
const loading = ref(false)
const searchQuery = ref('')
const machineFilter = ref('all')

// Modal state
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showAssignModal = ref(false)
const selectedLine: any = ref(null)

// Form state
const lineForm: any = ref({
  name: '',
  configuration: {}
})

const assignForm: any = ref({
  machineIds: [] as string[]
})

// Computed
const filteredLines = computed(() => {
  let filtered = [...productionLines.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter((line: any) => 
      line.name?.toLowerCase().includes(query) ||
      line.id?.toLowerCase().includes(query)
    )
  }
  
  // Apply machine filter
  if (machineFilter.value !== 'all') {
    filtered = filtered.filter((line: any) => 
      line.machineIds?.includes(machineFilter.value)
    )
  }
  
  return filtered
})

const machineOptions = computed(() => {
  return [
    { label: 'All Machines', value: 'all' },
    ...machines.value.map((machine: any) => ({
      label: `${machine.name} (${machine.type})`,
      value: machine.id
    }))
  ]
})

const availableMachines = computed(() => {
  return machines.value.map((machine: any) => ({
    ...machine,
    assigned: selectedLine.value?.machineIds?.includes(machine.id) || false
  }))
})

// Methods
const loadProductionLines = async () => {
  try {
    loading.value = true
    productionLines.value = await fetchProductionLines()
  } catch (error) {
    console.error('Failed to load production lines:', error)
    toast.error('Unable to load production lines')
  } finally {
    loading.value = false
  }
}

const loadMachines = async () => {
  try {
    machines.value = await fetchMachines()
  } catch (error) {
    console.error('Failed to load machines:', error)
    // Continue without machines - not critical
  }
}

const refreshData = async () => {
  await Promise.all([
    loadProductionLines(),
    loadMachines()
  ])
  toast.success('Data refreshed')
}

const openCreateModal = () => {
  resetLineForm()
  showCreateModal.value = true
}

const openEditModal = (line: any) => {
  selectedLine.value = line
  lineForm.value = {
    name: line.name,
    configuration: line.configuration || {}
  }
  showEditModal.value = true
}

const openAssignModal = (line: any) => {
  selectedLine.value = line
  assignForm.value.machineIds = [...(line.machineIds || [])]
  showAssignModal.value = true
}

const closeModals = () => {
  showCreateModal.value = false
  showEditModal.value = false
  showAssignModal.value = false
  selectedLine.value = null
  resetLineForm()
}

const resetLineForm = () => {
  lineForm.value = {
    name: '',
    configuration: {}
  }
  assignForm.value = {
    machineIds: []
  }
}

const handleCreateLine = async () => {
  try {
    await createProductionLine(lineForm.value)
    toast.success('Production line created successfully')
    closeModals()
    await loadProductionLines()
  } catch (error) {
    console.error('Failed to create production line:', error)
    toast.error('Failed to create production line')
  }
}

const handleUpdateLine = async () => {
  if (!selectedLine.value) return
  
  try {
    const updateData: ProductionLineUpdateDto = {
      name: lineForm.value.name,
      configuration: lineForm.value.configuration
    }
    
    await updateProductionLine(selectedLine.value.id, updateData)
    toast.success('Production line updated successfully')
    closeModals()
    await loadProductionLines()
  } catch (error) {
    console.error('Failed to update production line:', error)
    toast.error('Failed to update production line')
  }
}

const handleDeleteLine = async (line: any) => {
  if (!confirm(`Are you sure you want to delete production line "${line.name}"?`)) {
    return
  }
  
  try {
    await deleteProductionLine(line.id)
    productionLines.value = productionLines.value.filter((l: any) => l.id !== line.id)
    toast.success('Production line deleted successfully')
  } catch (error) {
    console.error('Failed to delete production line:', error)
    toast.error('Failed to delete production line')
  }
}

const handleAssignMachines = async () => {
  if (!selectedLine.value) return
  
  try {
    const updateData: ProductionLineUpdateDto = {
      name: selectedLine.value.name,
      configuration: selectedLine.value.configuration || {},
      machineIds: assignForm.value.machineIds
    }
    
    await updateProductionLine(selectedLine.value.id, updateData)
    toast.success('Machine assignments updated successfully')
    closeModals()
    await loadProductionLines()
  } catch (error) {
    console.error('Failed to update machine assignments:', error)
    toast.error('Failed to update machine assignments')
  }
}

const toggleMachineAssignment = (machineId: string) => {
  const index = assignForm.value.machineIds.indexOf(machineId)
  if (index >= 0) {
    assignForm.value.machineIds.splice(index, 1)
  } else {
    assignForm.value.machineIds.push(machineId)
  }
}

const getMachineCount = (line: any): number => {
  return line.machineIds?.length || 0
}

const getStatusColor = (count: number): string => {
  if (count === 0) return 'bg-red-500'
  if (count < 3) return 'bg-yellow-500'
  return 'bg-green-500'
}

const formatTime = (dateString?: string): string => {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleString()
}

// Lifecycle
onMounted(async () => {
  await refreshData()
})
</script>

<template>
  <BaseCard>
    <div class="production-line-management space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Production Line Management</h1>
          <p class="text-gray-600 mt-2">Configure and manage production line configurations</p>
        </div>
        <div class="flex gap-2">
          <BaseButton variant="outline" @click="refreshData">
            <RefreshCw class="w-4 h-4 mr-2" />
            Refresh
          </BaseButton>
          <BaseButton variant="primary" @click="openCreateModal">
            <Plus class="w-4 h-4 mr-2" />
            New Line
          </BaseButton>
        </div>
      </div>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Total Lines</p>
                <p class="text-2xl font-bold">{{ productionLines.length }}</p>
              </div>
              <Factory class="w-8 h-8 text-blue-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Active Lines</p>
                <p class="text-2xl font-bold">
                  {{ productionLines.filter((l: any) => (l.machineIds?.length || 0) > 0).length }}
                </p>
              </div>
              <Settings class="w-8 h-8 text-green-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Total Machines</p>
                <p class="text-2xl font-bold">{{ machines.length }}</p>
              </div>
              <Link class="w-8 h-8 text-purple-500" />
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Filters -->
      <BaseCard>
        <div class="p-4 space-y-4">
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="flex-1">
              <div class="relative">
                <Search class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                <BaseInput
                  v-model="searchQuery"
                  placeholder="Search production lines..."
                  class="pl-10"
                />
              </div>
            </div>
            
            <BaseSelect
              v-model="machineFilter"
              :options="machineOptions"
            />
          </div>
        </div>
      </BaseCard>

      <!-- Production Lines List -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            Production Lines ({{ filteredLines.length }})
          </h2>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Loading production lines...</p>
          </div>
          
          <div v-else-if="filteredLines.length === 0" class="text-center py-8">
            <Factory class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No production lines found</p>
            <p class="text-sm text-gray-500 mt-1">Create your first production line to get started</p>
          </div>
          
          <div v-else class="space-y-4">
            <BaseCard
              v-for="line in filteredLines"
              :key="line.id"
              class="hover:shadow-md transition-shadow"
            >
              <div class="p-4">
                <div class="flex flex-col sm:flex-row justify-between gap-4">
                  <div class="flex-1">
                    <div class="flex items-start justify-between mb-3">
                      <div>
                        <h3 class="text-lg font-semibold text-gray-900">{{ line.name }}</h3>
                        <p class="text-gray-600 text-sm font-mono">ID: {{ line.id }}</p>
                      </div>
                      <span 
                        class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium text-white"
                        :class="getStatusColor(getMachineCount(line))"
                      >
                        {{ getMachineCount(line) }} machines
                      </span>
                    </div>
                    
                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm text-gray-600 mb-3">
                      <div>
                        <span class="font-medium">Machines Assigned:</span>
                        <p>{{ getMachineCount(line) }}</p>
                      </div>
                      <div>
                        <span class="font-medium">Configuration:</span>
                        <p>{{ Object.keys(line.configuration || {}).length }} settings</p>
                      </div>
                      <div>
                        <span class="font-medium">Machine IDs:</span>
                        <p class="text-xs font-mono truncate">
                          {{ line.machineIds?.join(', ') || 'None' }}
                        </p>
                      </div>
                    </div>
                  </div>
                  
                  <div class="flex flex-col gap-2">
                    <BaseButton
                      size="sm"
                      variant="primary"
                      @click="openAssignModal(line)"
                    >
                      <Link class="w-4 h-4 mr-1" />
                      Assign Machines
                    </BaseButton>
                    
                    <BaseButton
                      size="sm"
                      variant="outline"
                      @click="openEditModal(line)"
                    >
                      <Edit class="w-4 h-4 mr-1" />
                      Edit
                    </BaseButton>
                    
                    <BaseButton
                      size="sm"
                      variant="outline"
                      @click="handleDeleteLine(line)"
                      class="text-red-600 hover:text-red-700 hover:bg-red-50"
                    >
                      <Trash2 class="w-4 h-4 mr-1" />
                      Delete
                    </BaseButton>
                  </div>
                </div>
              </div>
            </BaseCard>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>

  <!-- Create/Edit Modal -->
  <div 
    v-if="showCreateModal || showEditModal" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click="closeModals"
  >
    <BaseCard class="w-full max-w-md" @click.stop>
      <div class="p-6">
        <h2 class="text-2xl font-bold mb-6">
          {{ showEditModal ? 'Edit Production Line' : 'Create Production Line' }}
        </h2>
        
        <form @submit.prevent="showEditModal ? handleUpdateLine() : handleCreateLine()" class="space-y-4">
          <BaseInput
            v-model="lineForm.name"
            label="Line Name"
            required
          />
          
          <div class="pt-4">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit" class="ml-2">
              {{ showEditModal ? 'Update' : 'Create' }} Line
            </BaseButton>
          </div>
        </form>
      </div>
    </BaseCard>
  </div>

  <!-- Assign Machines Modal -->
  <div 
    v-if="showAssignModal" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click="closeModals"
  >
    <BaseCard class="w-full max-w-2xl max-h-[80vh] overflow-hidden" @click.stop>
      <div class="p-6">
        <h2 class="text-2xl font-bold mb-6">
          Assign Machines to {{ selectedLine?.name }}
        </h2>
        
        <div class="mb-4 max-h-96 overflow-y-auto">
          <div 
            v-for="machine in availableMachines" 
            :key="machine.id"
            class="flex items-center justify-between p-3 border rounded-lg mb-2 hover:bg-gray-50"
            :class="{ 'bg-blue-50 border-blue-200': machine.assigned }"
          >
            <div>
              <h3 class="font-medium">{{ machine.name }}</h3>
              <p class="text-sm text-gray-600">{{ machine.type }} • {{ machine.location }}</p>
            </div>
            <BaseButton
              :variant="machine.assigned ? 'primary' : 'outline'"
              size="sm"
              @click="toggleMachineAssignment(machine.id)"
            >
              {{ machine.assigned ? 'Assigned' : 'Assign' }}
            </BaseButton>
          </div>
        </div>
        
        <div class="flex justify-between pt-4">
          <div class="text-sm text-gray-600">
            Selected: {{ assignForm.machineIds.length }} machines
          </div>
          <div class="flex gap-2">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" @click="handleAssignMachines">
              Save Assignments
            </BaseButton>
          </div>
        </div>
      </div>
    </BaseCard>
  </div>
</template>

<style scoped>
.production-line-management {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .production-line-management {
    padding: 0.5rem;
  }
}
</style>