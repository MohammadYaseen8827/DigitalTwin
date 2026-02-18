// Reactive helper for server-driven pagination, sorting, and filtering across API resources.
import { computed, reactive, ref, watch, type Ref } from 'vue'

export interface PaginationParams {
  page: number
  pageSize: number
  sort?: string | null
  filters?: Record<string, unknown>
  search?: string | null
}

export interface PaginationResult<T> {
  items: T[]
  total: number
  page?: number
  pageSize?: number
}

export interface UsePaginatedListOptions<T> {
  pageSize?: number
  page?: number
  sort?: string | null
  filters?: Record<string, unknown>
  search?: string | null
  immediate?: boolean
  debounceMs?: number
  transform?: (result: PaginationResult<T>, params: PaginationParams) => PaginationResult<T>
}

export interface UsePaginatedListReturn<T> {
  items: Ref<T[]>
  total: Ref<number>
  page: Ref<number>
  pageSize: Ref<number>
  totalPages: Ref<number>
  sort: Ref<string | null>
  filters: Ref<Record<string, unknown>>
  search: Ref<string | null>
  loading: Ref<boolean>
  error: Ref<unknown>
  load: () => Promise<void>
  refresh: () => Promise<void>
  setPage: (value: number) => void
  setPageSize: (value: number) => void
  setSort: (value: string | null) => void
  setFilters: (value: Record<string, unknown>) => void
  setSearch: (value: string | null) => void
}

export type FetchPageFn<T> = (params: PaginationParams) => Promise<PaginationResult<T>>

export function usePaginatedList<T>(
  fetchPage: FetchPageFn<T>,
  options: UsePaginatedListOptions<T> = {}
): UsePaginatedListReturn<T> {
  const items = ref<T[]>([])
  const total = ref(0)
  const page = ref(options.page ?? 1)
  const pageSize = ref(options.pageSize ?? 20)
  const sort = ref<string | null>(options.sort ?? null)
  const filters = ref<Record<string, unknown>>({ ...(options.filters ?? {}) })
  const search = ref<string | null>(options.search ?? null)
  const loading = ref(false)
  const error = ref<unknown>(null)

  const state = reactive({
    debounceTimer: undefined as number | undefined
  })

  const totalPages = computed(() => {
    if (pageSize.value === 0) return 0
    return Math.max(1, Math.ceil(total.value / pageSize.value))
  })

  const buildParams = (): PaginationParams => ({
    page: page.value,
    pageSize: pageSize.value,
    sort: sort.value,
    filters: filters.value,
    search: search.value
  })

  const runFetch = async (): Promise<void> => {
    loading.value = true
    error.value = null
    const params = buildParams()

    try {
      const raw = await fetchPage(params)
      const result = options.transform ? options.transform(raw, params) : raw

      items.value = result.items as T[]
      total.value = result.total
      if (typeof result.page === 'number') {
        page.value = result.page
      }
      if (typeof result.pageSize === 'number') {
        pageSize.value = result.pageSize
      }
    } catch (err) {
      error.value = err
    } finally {
      loading.value = false
    }
  }

  const scheduleFetch = () => {
    if (options.debounceMs && options.debounceMs > 0) {
      window.clearTimeout(state.debounceTimer)
      state.debounceTimer = window.setTimeout(() => {
        void runFetch()
      }, options.debounceMs)
    } else {
      void runFetch()
    }
  }

  watch([page, pageSize, sort], () => {
    scheduleFetch()
  })

  watch(
    () => filters.value,
    () => {
      page.value = 1
      scheduleFetch()
    },
    { deep: true }
  )

  watch(
    () => search.value,
    () => {
      page.value = 1
      scheduleFetch()
    }
  )

  const load = async (): Promise<void> => {
    await runFetch()
  }

  const refresh = async (): Promise<void> => {
    await runFetch()
  }

  const setPage = (value: number) => {
    page.value = Math.max(1, value)
  }

  const setPageSize = (value: number) => {
    pageSize.value = Math.max(1, value)
    page.value = 1
  }

  const setSort = (value: string | null) => {
    sort.value = value
  }

  const setFilters = (value: Record<string, unknown>) => {
    filters.value = { ...value }
  }

  const setSearch = (value: string | null) => {
    search.value = value
  }

  if (options.immediate !== false) {
    void load()
  }

  return {
    items: items as Ref<T[]>,
    total,
    page,
    pageSize,
    totalPages,
    sort,
    filters,
    search,
    loading,
    error,
    load,
    refresh,
    setPage,
    setPageSize,
    setSort,
    setFilters,
    setSearch
  }
}
