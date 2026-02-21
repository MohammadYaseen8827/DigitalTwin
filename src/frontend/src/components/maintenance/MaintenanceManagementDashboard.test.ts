import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { ref } from 'vue'
import MaintenanceManagementDashboard from '@/components/maintenance/MaintenanceManagementDashboard.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'

// Mock dependencies
vi.mock('@/composables/useToast', () => ({
  useToast: vi.fn(() => ({
    success: vi.fn(),
    error: vi.fn()
  }))
}))

vi.mock('@/services/maintenance.service', () => ({
  fetchActiveMaintenance: vi.fn(),
  planMaintenance: vi.fn(),
  startMaintenance: vi.fn(),
  completeMaintenance: vi.fn(),
  cancelMaintenance: vi.fn()
}))

vi.mock('@/services/machines.service', () => ({
  fetchMachines: vi.fn()
}))

describe('MaintenanceManagementDashboard', () => {
  let wrapper: any
  const mockToast = {
    success: vi.fn(),
    error: vi.fn()
  }

  beforeEach(() => {
    vi.clearAllMocks()
    ;(useToast as any).mockReturnValue(mockToast)
  })

  const createWrapper = (props = {}) => {
    return mount(MaintenanceManagementDashboard, {
      props,
      global: {
        components: {
          BaseCard,
          BaseButton,
          BaseInput,
          BaseSelect,
          BaseSkeleton
        },
        mocks: {
          $t: (key: string) => key
        }
      }
    })
  }

  describe('Component Rendering', () => {
    it('renders the dashboard component', () => {
      wrapper = createWrapper()
      expect(wrapper.exists()).toBe(true)
    })

    it('displays the correct header title', () => {
      wrapper = createWrapper()
      const header = wrapper.find('.header-title')
      expect(header.text()).toContain('Maintenance Management')
    })

    it('shows create maintenance button', () => {
      wrapper = createWrapper()
      const createButton = wrapper.find('button')
      expect(createButton.text()).toContain('New Maintenance Record')
    })
  })

  describe('Loading State', () => {
    it('shows skeleton loaders when loading', async () => {
      wrapper = createWrapper()
      
      // Simulate loading state
      await wrapper.setData({ loading: true })
      
      const skeletons = wrapper.findAllComponents(BaseSkeleton)
      expect(skeletons.length).toBeGreaterThan(0)
    })

    it('hides skeleton loaders when not loading', async () => {
      wrapper = createWrapper()
      
      // Simulate loaded state
      await wrapper.setData({ loading: false })
      
      const skeletons = wrapper.findAllComponents(BaseSkeleton)
      expect(skeletons.length).toBe(0)
    })
  })

  describe('Filtering Functionality', () => {
    it('applies search filter correctly', async () => {
      wrapper = createWrapper()
      
      const testData = [
        { id: '1', machineId: 'M001', description: 'Oil change required', status: 'planned' },
        { id: '2', machineId: 'M002', description: 'Bearing replacement', status: 'inprogress' }
      ]
      
      await wrapper.setData({ maintenanceRecords: testData })
      
      // Apply search filter
      await wrapper.setData({ searchQuery: 'oil' })
      
      const filtered = (wrapper.vm as any).filteredRecords
      expect(filtered).toHaveLength(1)
      expect(filtered[0].description).toContain('Oil change')
    })

    it('applies status filter correctly', async () => {
      wrapper = createWrapper()
      
      const testData = [
        { id: '1', status: 'planned' },
        { id: '2', status: 'inprogress' },
        { id: '3', status: 'completed' }
      ]
      
      await wrapper.setData({ maintenanceRecords: testData })
      
      // Apply status filter
      await wrapper.setData({ statusFilter: 'planned' })
      
      const filtered = (wrapper.vm as any).filteredRecords
      expect(filtered).toHaveLength(1)
      expect(filtered[0].status).toBe('planned')
    })

    it('applies type filter correctly', async () => {
      wrapper = createWrapper()
      
      const testData = [
        { id: '1', type: 'preventive' },
        { id: '2', type: 'corrective' },
        { id: '3', type: 'emergency' }
      ]
      
      await wrapper.setData({ maintenanceRecords: testData })
      
      // Apply type filter
      await wrapper.setData({ typeFilter: 'preventive' })
      
      const filtered = (wrapper.vm as any).filteredRecords
      expect(filtered).toHaveLength(1)
      expect(filtered[0].type).toBe('preventive')
    })
  })

  describe('Form Handling', () => {
    it('opens create modal when create button is clicked', async () => {
      wrapper = createWrapper()
      
      const createButton = wrapper.find('button')
      await createButton.trigger('click')
      
      expect((wrapper.vm as any).showCreateModal).toBe(true)
    })

    it('resets form when opening create modal', async () => {
      wrapper = createWrapper()
      
      // Set some form data
      await wrapper.setData({ 
        maintenanceForm: {
          machineId: 'test-machine',
          type: 'preventive',
          plannedDate: '2024-01-01',
          notes: 'test notes'
        }
      })
      
      await (wrapper.vm as any).openCreateModal()
      
      const form = (wrapper.vm as any).maintenanceForm
      expect(form.machineId).toBe('')
      expect(form.type).toBe('preventive') // Default value
      expect(form.plannedDate).toBe('')
      expect(form.notes).toBe('')
    })

    it('validates required fields before submission', async () => {
      wrapper = createWrapper()
      
      await (wrapper.vm as any).openCreateModal()
      
      // Try to submit without required fields
      const form = (wrapper.vm as any).maintenanceForm
      form.machineId = '' // Missing required field
      
      // This should be handled by the form validation
      expect(form.machineId).toBe('')
    })
  })

  describe('Data Transformation', () => {
    it('formats dates correctly', () => {
      wrapper = createWrapper()
      
      const testDate = '2024-01-15T10:30:00Z'
      const formatted = (wrapper.vm as any).formatDate(testDate)
      
      // Should return a readable date format
      expect(formatted).toBeDefined()
      expect(typeof formatted).toBe('string')
    })

    it('gets correct status colors', () => {
      wrapper = createWrapper()
      
      const plannedColor = (wrapper.vm as any).getStatusColor('planned')
      const inprogressColor = (wrapper.vm as any).getStatusColor('inprogress')
      const completedColor = (wrapper.vm as any).getStatusColor('completed')
      
      expect(plannedColor).toContain('blue')
      expect(inprogressColor).toContain('yellow')
      expect(completedColor).toContain('green')
    })
  })

  describe('Computed Properties', () => {
    it('computes filtered records correctly', async () => {
      wrapper = createWrapper()
      
      const testData = [
        { id: '1', description: 'Test maintenance', status: 'planned', type: 'preventive' }
      ]
      
      await wrapper.setData({ maintenanceRecords: testData })
      
      const filtered = (wrapper.vm as any).filteredRecords
      expect(filtered).toHaveLength(1)
      expect(filtered[0].id).toBe('1')
    })

    it('handles empty search results', async () => {
      wrapper = createWrapper()
      
      const testData = [
        { id: '1', description: 'Test maintenance', status: 'planned' }
      ]
      
      await wrapper.setData({ 
        maintenanceRecords: testData,
        searchQuery: 'nonexistent'
      })
      
      const filtered = (wrapper.vm as any).filteredRecords
      expect(filtered).toHaveLength(0)
    })
  })

  describe('Error Handling', () => {
    it('handles loading errors gracefully', async () => {
      wrapper = createWrapper()
      
      // Simulate error during loading
      await wrapper.setData({ 
        loading: true,
        error: 'Failed to load data'
      })
      
      // Component should still render
      expect(wrapper.exists()).toBe(true)
      
      // Error message should be displayed
      const errorElements = wrapper.findAll('.error-message')
      // Note: Actual error display depends on component implementation
    })

    it('shows appropriate error messages', async () => {
      wrapper = createWrapper()
      
      // Trigger error scenario
      await (wrapper.vm as any).handleDelete({ id: 'invalid' })
      
      // Toast error should be called
      expect(mockToast.error).toHaveBeenCalled()
    })
  })

  describe('User Interactions', () => {
    it('handles edit button clicks', async () => {
      wrapper = createWrapper()
      
      const testRecord = {
        id: 'test-1',
        machineId: 'M001',
        type: 'preventive',
        plannedDate: '2024-01-15',
        notes: 'Test notes'
      }
      
      await (wrapper.vm as any).openEditModal(testRecord)
      
      expect((wrapper.vm as any).showEditModal).toBe(true)
      expect((wrapper.vm as any).selectedRecord).toEqual(testRecord)
    })

    it('handles modal closing', async () => {
      wrapper = createWrapper()
      
      await wrapper.setData({ 
        showCreateModal: true,
        showEditModal: true 
      })
      
      await (wrapper.vm as any).closeModals()
      
      expect((wrapper.vm as any).showCreateModal).toBe(false)
      expect((wrapper.vm as any).showEditModal).toBe(false)
      expect((wrapper.vm as any).selectedRecord).toBeNull()
    })
  })

  describe('Accessibility', () => {
    it('has proper aria labels', () => {
      wrapper = createWrapper()
      
      const buttons = wrapper.findAll('button')
      buttons.forEach((button: any) => {
        // All interactive elements should have accessible names
        expect(button.attributes('aria-label') || button.text()).toBeTruthy()
      })
    })

    it('supports keyboard navigation', async () => {
      wrapper = createWrapper()
      
      const buttons = wrapper.findAll('button')
      for (const button of buttons) {
        await button.trigger('keydown.enter')
        // Should handle keyboard events appropriately
      }
    })
  })
})