// Generic CRUD helper built atop the shared axios client and pagination utilities.
import { ref, readonly } from 'vue'
import axiosClient from '@/api/axiosClient'
import type { PaginationParams, PaginationResult } from '@/composables/usePaginatedList'

type HttpMethod = 'get' | 'post' | 'put' | 'patch' | 'delete'

type RequestConfig = {
  method?: HttpMethod
  url: string
  data?: unknown
  params?: Record<string, unknown>
}

async function request<T>(config: RequestConfig): Promise<T> {
  const response = await axiosClient.request<T>({
    method: config.method ?? 'get',
    url: config.url,
    data: config.data,
    params: config.params
  })

  return response as T
}

export interface UseApiCrudOptions<TModel, TCreate, TUpdate> {
  /** Base URL (e.g. /Machines). */
  baseUrl: string
  /** Optional mapper if API wraps payloads. */
  mapModel?: (payload: unknown) => TModel
  mapList?: (payload: unknown) => TModel[] | PaginationResult<TModel>
  /** Hook for customizing requests (e.g. tenant headers). */
  onBeforeRequest?: (config: RequestConfig) => RequestConfig
  /** Optional hook to run after successful mutation. */
  onAfterMutation?: (model: TModel) => void
}

export interface UseApiCrud<TModel, TCreate, TUpdate> {
  loading: Readonly<{ value: boolean }>
  error: Readonly<{ value: unknown }>
  list: (params?: PaginationParams | Record<string, unknown>) => Promise<PaginationResult<TModel> | TModel[]>
  get: (id: string) => Promise<TModel>
  create: (payload: TCreate) => Promise<TModel>
  update: (id: string, payload: TUpdate) => Promise<TModel>
  remove: (id: string) => Promise<void>
}

export function useApiCrud<TModel, TCreate = Partial<TModel>, TUpdate = Partial<TModel>>(
  options: UseApiCrudOptions<TModel, TCreate, TUpdate>
): UseApiCrud<TModel, TCreate, TUpdate> {
  const loading = ref(false)
  const error = ref<unknown>(null)

  const applyBeforeRequest = (config: RequestConfig): RequestConfig => {
    return options.onBeforeRequest ? options.onBeforeRequest(config) : config
  }

  const handleMutationSideEffects = (model: TModel) => {
    options.onAfterMutation?.(model)
  }

  const mapModel = (payload: unknown): TModel => {
    return options.mapModel ? options.mapModel(payload) : (payload as TModel)
  }

  const mapList = (payload: unknown): TModel[] | PaginationResult<TModel> => {
    if (options.mapList) {
      return options.mapList(payload)
    }

    return payload as TModel[] | PaginationResult<TModel>
  }

  const withLoading = async <T>(operation: () => Promise<T>): Promise<T> => {
    loading.value = true
    error.value = null
    try {
      return await operation()
    } catch (err) {
      error.value = err
      throw err
    } finally {
      loading.value = false
    }
  }

  const list = async (
    params: PaginationParams | Record<string, unknown> = {}
  ): Promise<PaginationResult<TModel> | TModel[]> => {
    const normalizedParams: Record<string, unknown> = { ...params }
    const config = applyBeforeRequest({
      url: options.baseUrl,
      params: normalizedParams
    })

    const payload = await withLoading(() => request<unknown>(config))
    return mapList(payload)
  }

  const get = async (id: string): Promise<TModel> => {
    const config = applyBeforeRequest({
      url: `${options.baseUrl}/${encodeURIComponent(id)}`
    })

    const payload = await withLoading(() => request<unknown>(config))
    return mapModel(payload)
  }

  const create = async (payload: TCreate): Promise<TModel> => {
    const config = applyBeforeRequest({
      method: 'post',
      url: options.baseUrl,
      data: payload
    })

    const response = await withLoading(() => request<unknown>(config))
    const model = mapModel(response)
    handleMutationSideEffects(model)
    return model
  }

  const update = async (id: string, payload: TUpdate): Promise<TModel> => {
    const config = applyBeforeRequest({
      method: 'put',
      url: `${options.baseUrl}/${encodeURIComponent(id)}`,
      data: payload
    })

    const response = await withLoading(() => request<unknown>(config))
    const model = mapModel(response)
    handleMutationSideEffects(model)
    return model
  }

  const remove = async (id: string): Promise<void> => {
    const config = applyBeforeRequest({
      method: 'delete',
      url: `${options.baseUrl}/${encodeURIComponent(id)}`
    })

    await withLoading(() => request<unknown>(config))
  }

  return {
    loading: readonly(loading),
    error: readonly(error),
    list,
    get,
    create,
    update,
    remove
  }
}
