<script setup lang="ts">
import { ref, computed, onMounted, type Ref } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { useTenants, type Tenant, type TenantCreateDto, type TenantUpdateDto, UserRole } from '@/services/tenants.service'
import { Building, Users, Settings, Plus, Edit, Trash2, CheckCircle, XCircle, Eye, EyeOff } from 'lucide-vue-next'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  BarChart as EChartsBar,
  PieChart as EChartsPie,
  LineChart as EChartsLine
} from 'echarts/charts'
import {
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

// Register ECharts components
echarts.use([
  EChartsBar,
  EChartsPie,
  EChartsLine,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  CanvasRenderer
])

const toast = useToast()

// Use tenant composable
const {
  tenants,
  currentTenant,
  loading,
  error,
  fetchTenants,
  createTenant,
  updateTenant,
  deleteTenant,
  activateTenant,
  deactivateTenant
} = useTenants()

// State
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showDetailsModal = ref(false)
const editingTenant = ref<Tenant | null>(null)
const viewingTenant = ref<Tenant | null>(null)

// Form data
const formData = ref({
  name: '',
  slug: '',
  description: '',
  connectionString: ''
})

// Computed
const activeTenants = computed(() => tenants.filter((t: Tenant) => t.isActive))
const inactiveTenants = computed(() => tenants.filter((t: Tenant) => !t.isActive))
const tenantStats = computed(() => ({
  total: tenants.length,
  active: activeTenants.value.length,
  inactive: inactiveTenants.value.length,
  activationRate: tenants.length > 0 ? Math.round((activeTenants.value.length / tenants.length) * 100) : 0
}))

// Methods
const openCreateModal = () => {
  formData.value = {
    name: '',
    slug: '',
    description: '',
    connectionString: ''
  }
  showCreateModal.value = true
}

const openEditModal = (tenant: Tenant) => {
  editingTenant.value = tenant
  formData.value = {
    name: tenant.name,
    slug: tenant.slug,
    description: tenant.description || '',
    connectionString: tenant.connectionString || ''
  }
  showEditModal.value = true
}

const openDetailsModal = (tenant: Tenant) => {
  viewingTenant.value = tenant
  showDetailsModal.value = true
}

const closeModal = () => {
  showCreateModal.value = false
  showEditModal.value = false
  showDetailsModal.value = false
  editingTenant.value = null
  viewingTenant.value = null
}

const handleSubmit = async () => {
  try {
    if (editingTenant.value) {
      // Update existing tenant
      const updateData: TenantUpdateDto = {
        name: formData.value.name || undefined,
        slug: formData.value.slug || undefined,
        description: formData.value.description || undefined,
        connectionString: formData.value.connectionString || undefined
      }
      
      await updateTenant(editingTenant.value.id, updateData)
      toast.success('Tenant updated successfully')
    } else {
      // Create new tenant
      const createData: TenantCreateDto = {
        name: formData.value.name,
        slug: formData.value.slug,
        description: formData.value.description,
        connectionString: formData.value.connectionString
      }
      
      await createTenant(createData)
      toast.success('Tenant created successfully')
    }
    
    closeModal()
  } catch (error) {
    console.error('Failed to save tenant:', error)
    toast.error('Failed to save tenant')
  }
}

const handleDelete = async (tenant: Tenant) => {
  if (!confirm(`Are you sure you want to delete tenant "${tenant.name}"? This action cannot be undone.`)) {
    return
  }
  
  try {
    await deleteTenant(tenant.id)
    toast.success('Tenant deleted successfully')
  } catch (error) {
    console.error('Failed to delete tenant:', error)
    toast.error('Failed to delete tenant')
  }
}

const toggleTenantStatus = async (tenant: Tenant) => {
  try {
    if (tenant.isActive) {
      await deactivateTenant(tenant.id)
      toast.success('Tenant deactivated')
    } else {
      await activateTenant(tenant.id)
      toast.success('Tenant activated')
    }
  } catch (error) {
    console.error('Failed to toggle tenant status:', error)
    toast.error('Failed to update tenant status')
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

const getStatusBadgeClass = (isActive: boolean) => {
  return isActive 
    ? 'bg-green-100 text-green-800' 
    : 'bg-red-100 text-red-800'
}

const getStatusIcon = (isActive: boolean) => {
  return isActive ? CheckCircle : XCircle
}

// Lifecycle
onMounted(async () => {
  await fetchTenants()
})
</script>

<template>
  <BaseCard>
    <div class="tenant-management space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Tenant Management</h1>
          <p class="text-gray-600 mt-2">Manage multi-tenant environments and tenant configurations</p>
        </div>
        <BaseButton variant="primary" @click="openCreateModal">
          <Plus class="w-4 h-4 mr-2" />
          Add New Tenant
        </BaseButton>
      </div>

      <!-- Stats Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard class="p-4">
          <div class="flex items-center">
            <Building class="w-8 h-8 text-blue-500 mr-3" />
            <div>
              <p class="text-sm text-gray-600">Total Tenants</p>
              <p class="text-2xl font-bold">{{ tenantStats.total }}</p>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="p-4">
          <div class="flex items-center">
            <CheckCircle class="w-8 h-8 text-green-500 mr-3" />
            <div>
              <p class="text-sm text-gray-600">Active</p>
              <p class="text-2xl font-bold">{{ tenantStats.active }}</p>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="p-4">
          <div class="flex items-center">
            <XCircle class="w-8 h-8 text-red-500 mr-3" />
            <div>
              <p class="text-sm text-gray-600">Inactive</p>
              <p class="text-2xl font-bold">{{ tenantStats.inactive }}</p>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="p-4">
          <div class="flex items-center">
            <div class="w-8 h-8 bg-purple-100 rounded-full flex items-center justify-center mr-3">
              <span class="text-purple-600 font-bold text-sm">{{ tenantStats.activationRate }}%</span>
            </div>
            <div>
              <p class="text-sm text-gray-600">Activation Rate</p>
              <p class="text-2xl font-bold">{{ tenantStats.activationRate }}%</p>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Loading/Error States -->
      <div v-if="loading" class="text-center py-8">
        <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
        <p class="mt-2 text-gray-600">Loading tenants...</p>
      </div>

      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4">
        <p class="text-red-700">{{ error }}</p>
        <BaseButton variant="outline" @click="fetchTenants" class="mt-2">
          Retry
        </BaseButton>
      </div>

      <!-- Tenants Table -->
      <div v-else>
        <h2 class="text-xl font-semibold mb-4">Tenants</h2>
        
        <div v-if="tenants.length === 0" class="text-center py-12 bg-gray-50 rounded-lg">
          <Building class="w-12 h-12 mx-auto text-gray-400 mb-4" />
          <p class="text-gray-500 mb-4">No tenants found</p>
          <BaseButton variant="primary" @click="openCreateModal">
            <Plus class="w-4 h-4 mr-2" />
            Create Your First Tenant
          </BaseButton>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tenant</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Slug</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="tenant in tenants" :key="tenant.id" class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <Building class="w-5 h-5 text-gray-400 mr-3" />
                    <div>
                      <div class="text-sm font-medium text-gray-900">{{ tenant.name }}</div>
                      <div v-if="tenant.description" class="text-sm text-gray-500">{{ tenant.description }}</div>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  {{ tenant.slug }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full" :class="getStatusBadgeClass(tenant.isActive)">
                    <component :is="getStatusIcon(tenant.isActive)" class="w-3 h-3 mr-1 inline" />
                    {{ tenant.isActive ? 'Active' : 'Inactive' }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ formatDate(tenant.createdAt) }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <div class="flex space-x-2">
                    <BaseButton variant="outline" size="sm" @click="openDetailsModal(tenant)">
                      <Eye class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton variant="outline" size="sm" @click="openEditModal(tenant)">
                      <Edit class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton 
                      :variant="tenant.isActive ? 'outline' : 'primary'" 
                      size="sm" 
                      @click="toggleTenantStatus(tenant)"
                    >
                      <component :is="tenant.isActive ? EyeOff : Eye" class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton variant="danger" size="sm" @click="handleDelete(tenant)">
                      <Trash2 class="w-4 h-4" />
                    </BaseButton>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </BaseCard>

  <!-- Create/Edit Modal -->
  <div v-if="showCreateModal || showEditModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
    <BaseCard class="w-full max-w-2xl max-h-[90vh] overflow-y-auto">
      <div class="p-6">
        <h2 class="text-xl font-semibold mb-4">
          {{ editingTenant ? 'Edit Tenant' : 'Create New Tenant' }}
        </h2>
        
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <BaseInput
            v-model="formData.name"
            label="Tenant Name"
            placeholder="Enter tenant name"
            required
          />
          
          <BaseInput
            v-model="formData.slug"
            label="Tenant Slug"
            placeholder="Enter tenant slug (lowercase, no spaces)"
            required
          />
          
          <BaseInput
            v-model="formData.description"
            label="Description"
            placeholder="Enter tenant description"
            type="textarea"
          />
          
          <BaseInput
            v-model="formData.connectionString"
            label="Connection String"
            placeholder="Enter database connection string"
            type="textarea"
          />
          
          <div class="flex justify-end space-x-3 pt-4">
            <BaseButton variant="outline" @click="closeModal">Cancel</BaseButton>
            <BaseButton variant="primary" type="submit" :disabled="loading">
              {{ editingTenant ? 'Update Tenant' : 'Create Tenant' }}
            </BaseButton>
          </div>
        </form>
      </div>
    </BaseCard>
  </div>

  <!-- Details Modal -->
  <div v-if="showDetailsModal && viewingTenant" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
    <BaseCard class="w-full max-w-2xl">
      <div class="p-6">
        <h2 class="text-xl font-semibold mb-4">Tenant Details</h2>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700">Name</label>
            <p class="mt-1 text-sm text-gray-900">{{ viewingTenant.name }}</p>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700">Slug</label>
            <p class="mt-1 text-sm text-gray-900">{{ viewingTenant.slug }}</p>
          </div>
          
          <div v-if="viewingTenant.description">
            <label class="block text-sm font-medium text-gray-700">Description</label>
            <p class="mt-1 text-sm text-gray-900">{{ viewingTenant.description }}</p>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700">Status</label>
            <span class="mt-1 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium" :class="getStatusBadgeClass(viewingTenant.isActive)">
              <component :is="getStatusIcon(viewingTenant.isActive)" class="w-3 h-3 mr-1" />
              {{ viewingTenant.isActive ? 'Active' : 'Inactive' }}
            </span>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700">Created</label>
            <p class="mt-1 text-sm text-gray-900">{{ formatDate(viewingTenant.createdAt) }}</p>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700">Last Updated</label>
            <p class="mt-1 text-sm text-gray-900">{{ formatDate(viewingTenant.updatedAt) }}</p>
          </div>
          
          <div v-if="viewingTenant.connectionString">
            <label class="block text-sm font-medium text-gray-700">Connection String</label>
            <p class="mt-1 text-sm text-gray-900 font-mono bg-gray-50 p-2 rounded">{{ viewingTenant.connectionString }}</p>
          </div>
        </div>
        
        <div class="flex justify-end space-x-3 pt-4">
          <BaseButton variant="outline" @click="closeModal">Close</BaseButton>
        </div>
      </div>
    </BaseCard>
  </div>
</template>

<style scoped>
.tenant-management {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .tenant-management {
    padding: 0.5rem;
  }
}
</style>