import { ref, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { debounce } from 'lodash-es'

export function useSearchFilters() {
  const route = useRoute()
  const router = useRouter()

  // State
  const searchQuery = ref('')
  const activeStatus = ref<string | null>(null)
  const activeRiskLevels = ref<string[]>([])

  // Load state from URL on mount
  const loadFromUrl = () => {
    if (route.query.search) {
      searchQuery.value = route.query.search as string
    }
    if (route.query.status) {
      activeStatus.value = route.query.status as string
    }
    if (route.query.risk) {
      activeRiskLevels.value = Array.isArray(route.query.risk)
        ? route.query.risk
        : [route.query.risk as string]
    }
  }

  // Save state to URL
  const updateUrl = debounce(() => {
    const query: Record<string, any> = {}
    if (searchQuery.value) query.search = searchQuery.value
    if (activeStatus.value) query.status = activeStatus.value
    if (activeRiskLevels.value.length) query.risk = activeRiskLevels.value

    router.replace({ query })
  }, 300)

  // Watch for URL changes
  watch(
    () => route.query,
    () => {
      if (route.name !== 'PredictiveAnalytics') return
      loadFromUrl()
    },
    { immediate: true }
  )

  // Watch for filter changes
  watch([searchQuery, activeStatus, activeRiskLevels], updateUrl, { deep: true })

  // Computed properties
  const hasActiveFilters = computed(() => {
    return searchQuery.value !== '' || activeStatus.value !== null || activeRiskLevels.value.length > 0
  })

  // Methods
  const clearAllFilters = () => {
    searchQuery.value = ''
    activeStatus.value = null
    activeRiskLevels.value = []
  }

  return {
    searchQuery,
    activeStatus,
    activeRiskLevels,
    hasActiveFilters,
    clearAllFilters,
  }
}

export default useSearchFilters
