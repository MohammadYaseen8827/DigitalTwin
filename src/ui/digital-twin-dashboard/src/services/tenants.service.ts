import axios from 'axios'
import type { Ref } from 'vue'
import { ref, reactive } from 'vue'

// Define TypeScript interfaces
export interface Tenant {
  id: string
  name: string
  slug: string
  description?: string
  connectionString?: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface TenantUser {
  id: string
  tenantId: string
  userId: string
  userName: string
  userEmail: string
  role: UserRole
  createdAt: string
  updatedAt: string
}

export interface TenantSetting {
  id: string
  tenantId: string
  key: string
  value: string
  createdAt: string
  updatedAt: string
}

export enum UserRole {
  User = 'User',
  Admin = 'Admin'
}

export interface TenantCreateDto {
  name: string
  slug: string
  description?: string
  connectionString?: string
}

export interface TenantUpdateDto {
  name?: string
  slug?: string
  description?: string
  connectionString?: string
}

export interface TenantUserCreateDto {
  userId: string
  role: UserRole
}

export interface TenantSettingCreateDto {
  key: string
  value: string
}

export interface TenantSettingUpdateDto {
  key?: string
  value?: string
}

// Tenant Service Class
class TenantService {
  private baseUrl = '/api/tenants'

  async getAllTenants(): Promise<Tenant[]> {
    try {
      const response = await axios.get<Tenant[]>(this.baseUrl)
      return response.data
    } catch (error) {
      console.error('Failed to fetch tenants:', error)
      throw error
    }
  }

  async getActiveTenants(): Promise<Tenant[]> {
    try {
      const response = await axios.get<Tenant[]>(`${this.baseUrl}/active`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch active tenants:', error)
      throw error
    }
  }

  async getTenantById(id: string): Promise<Tenant> {
    try {
      const response = await axios.get<Tenant>(`${this.baseUrl}/${id}`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch tenant ${id}:`, error)
      throw error
    }
  }

  async getTenantBySlug(slug: string): Promise<Tenant> {
    try {
      const response = await axios.get<Tenant>(`${this.baseUrl}/slug/${slug}`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch tenant by slug ${slug}:`, error)
      throw error
    }
  }

  async createTenant(tenant: TenantCreateDto): Promise<Tenant> {
    try {
      const response = await axios.post<Tenant>(this.baseUrl, tenant)
      return response.data
    } catch (error) {
      console.error('Failed to create tenant:', error)
      throw error
    }
  }

  async updateTenant(id: string, tenant: TenantUpdateDto): Promise<Tenant> {
    try {
      const response = await axios.put<Tenant>(`${this.baseUrl}/${id}`, tenant)
      return response.data
    } catch (error) {
      console.error(`Failed to update tenant ${id}:`, error)
      throw error
    }
  }

  async deleteTenant(id: string): Promise<void> {
    try {
      await axios.delete(`${this.baseUrl}/${id}`)
    } catch (error) {
      console.error(`Failed to delete tenant ${id}:`, error)
      throw error
    }
  }

  async activateTenant(id: string): Promise<void> {
    try {
      await axios.post(`${this.baseUrl}/${id}/activate`)
    } catch (error) {
      console.error(`Failed to activate tenant ${id}:`, error)
      throw error
    }
  }

  async deactivateTenant(id: string): Promise<void> {
    try {
      await axios.post(`${this.baseUrl}/${id}/deactivate`)
    } catch (error) {
      console.error(`Failed to deactivate tenant ${id}:`, error)
      throw error
    }
  }

  // Tenant User Management
  async getTenantUsers(tenantId: string): Promise<TenantUser[]> {
    try {
      const response = await axios.get<TenantUser[]>(`${this.baseUrl}/${tenantId}/users`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch tenant users for ${tenantId}:`, error)
      throw error
    }
  }

  async addUserToTenant(tenantId: string, user: TenantUserCreateDto): Promise<TenantUser> {
    try {
      const response = await axios.post<TenantUser>(`${this.baseUrl}/${tenantId}/users`, user)
      return response.data
    } catch (error) {
      console.error(`Failed to add user to tenant ${tenantId}:`, error)
      throw error
    }
  }

  async removeUserFromTenant(tenantId: string, userId: string): Promise<void> {
    try {
      await axios.delete(`${this.baseUrl}/${tenantId}/users/${userId}`)
    } catch (error) {
      console.error(`Failed to remove user ${userId} from tenant ${tenantId}:`, error)
      throw error
    }
  }

  async updateTenantUserRole(tenantId: string, userId: string, role: UserRole): Promise<void> {
    try {
      await axios.put(`${this.baseUrl}/${tenantId}/users/${userId}/role`, role)
    } catch (error) {
      console.error(`Failed to update role for user ${userId} in tenant ${tenantId}:`, error)
      throw error
    }
  }

  // Tenant Setting Management
  async getTenantSettings(tenantId: string): Promise<TenantSetting[]> {
    try {
      const response = await axios.get<TenantSetting[]>(`${this.baseUrl}/${tenantId}/settings`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch tenant settings for ${tenantId}:`, error)
      throw error
    }
  }

  async createTenantSetting(tenantId: string, setting: TenantSettingCreateDto): Promise<TenantSetting> {
    try {
      const response = await axios.post<TenantSetting>(`${this.baseUrl}/${tenantId}/settings`, setting)
      return response.data
    } catch (error) {
      console.error(`Failed to create tenant setting for ${tenantId}:`, error)
      throw error
    }
  }

  async updateTenantSetting(tenantId: string, settingId: string, setting: TenantSettingUpdateDto): Promise<TenantSetting> {
    try {
      const response = await axios.put<TenantSetting>(`${this.baseUrl}/${tenantId}/settings/${settingId}`, setting)
      return response.data
    } catch (error) {
      console.error(`Failed to update tenant setting ${settingId} for ${tenantId}:`, error)
      throw error
    }
  }

  async deleteTenantSetting(tenantId: string, settingId: string): Promise<void> {
    try {
      await axios.delete(`${this.baseUrl}/${tenantId}/settings/${settingId}`)
    } catch (error) {
      console.error(`Failed to delete tenant setting ${settingId} for ${tenantId}:`, error)
      throw error
    }
  }

  async getTenantSettingByKey(tenantId: string, key: string): Promise<TenantSetting> {
    try {
      const response = await axios.get<TenantSetting>(`${this.baseUrl}/${tenantId}/settings/key/${key}`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch tenant setting by key ${key} for ${tenantId}:`, error)
      throw error
    }
  }
}

// Create singleton instance
export const tenantService = new TenantService()

// Composable for tenant management
export function useTenants() {
  const tenants: Ref<Tenant[]> = ref([])
  const currentTenant: Ref<Tenant | null> = ref(null)
  const loading: Ref<boolean> = ref(false)
  const error: Ref<string | null> = ref(null)

  const tenantState = reactive({
    tenants,
    currentTenant,
    loading,
    error
  })

  const fetchTenants = async () => {
    loading.value = true
    error.value = null
    try {
      tenants.value = await tenantService.getAllTenants()
    } catch (err) {
      error.value = 'Failed to fetch tenants'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const fetchActiveTenants = async () => {
    loading.value = true
    error.value = null
    try {
      tenants.value = await tenantService.getActiveTenants()
    } catch (err) {
      error.value = 'Failed to fetch active tenants'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const selectTenant = async (tenantId: string) => {
    loading.value = true
    error.value = null
    try {
      currentTenant.value = await tenantService.getTenantById(tenantId)
    } catch (err) {
      error.value = 'Failed to select tenant'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const createTenant = async (tenantData: TenantCreateDto) => {
    loading.value = true
    error.value = null
    try {
      const newTenant = await tenantService.createTenant(tenantData)
      tenants.value.push(newTenant)
      return newTenant
    } catch (err) {
      error.value = 'Failed to create tenant'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const updateTenant = async (id: string, tenantData: TenantUpdateDto) => {
    loading.value = true
    error.value = null
    try {
      const updatedTenant = await tenantService.updateTenant(id, tenantData)
      const index = tenants.value.findIndex(t => t.id === id)
      if (index !== -1) {
        tenants.value[index] = updatedTenant
      }
      if (currentTenant.value?.id === id) {
        currentTenant.value = updatedTenant
      }
      return updatedTenant
    } catch (err) {
      error.value = 'Failed to update tenant'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const deleteTenant = async (id: string) => {
    loading.value = true
    error.value = null
    try {
      await tenantService.deleteTenant(id)
      tenants.value = tenants.value.filter(t => t.id !== id)
      if (currentTenant.value?.id === id) {
        currentTenant.value = null
      }
    } catch (err) {
      error.value = 'Failed to delete tenant'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const activateTenant = async (id: string) => {
    loading.value = true
    error.value = null
    try {
      await tenantService.activateTenant(id)
      const tenant = tenants.value.find(t => t.id === id)
      if (tenant) {
        tenant.isActive = true
        tenant.updatedAt = new Date().toISOString()
      }
    } catch (err) {
      error.value = 'Failed to activate tenant'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const deactivateTenant = async (id: string) => {
    loading.value = true
    error.value = null
    try {
      await tenantService.deactivateTenant(id)
      const tenant = tenants.value.find(t => t.id === id)
      if (tenant) {
        tenant.isActive = false
        tenant.updatedAt = new Date().toISOString()
      }
    } catch (err) {
      error.value = 'Failed to deactivate tenant'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    ...tenantState,
    fetchTenants,
    fetchActiveTenants,
    selectTenant,
    createTenant,
    updateTenant,
    deleteTenant,
    activateTenant,
    deactivateTenant
  }
}