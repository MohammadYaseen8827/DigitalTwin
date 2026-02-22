import { ref, computed, type Ref, type ComputedRef, type UnwrapRef } from 'vue'

type AsyncStateType = 'idle' | 'loading' | 'success' | 'error' | 'empty'

export interface UseAsyncStateOptions<T> {
  initialValue?: T
  onSuccess?: (data: T) => void
  onError?: (error: Error) => void
}

export interface UseAsyncStateReturn<T> {
  state: Ref<AsyncStateType>
  data: Ref<T | undefined>
  error: Ref<Error | null>
  isLoading: ComputedRef<boolean>
  isSuccess: ComputedRef<boolean>
  isError: ComputedRef<boolean>
  isEmpty: ComputedRef<boolean>
  isIdle: ComputedRef<boolean>
  execute: (promise: Promise<T>) => Promise<T>
  reset: () => void
}

/**
 * Composable for handling async state with loading, error, empty, and success states.
 * Provides a standardized pattern for handling async operations in components.
 */
export function useAsyncState<T>(options: UseAsyncStateOptions<T> = {}): UseAsyncStateReturn<T> {
  const state = ref<AsyncStateType>('idle')
  const data = ref<T | undefined>(options.initialValue)
  const error = ref<Error | null>(null)

  const isLoading = computed(() => state.value === 'loading')
  const isSuccess = computed(() => state.value === 'success')
  const isError = computed(() => state.value === 'error')
  const isEmpty = computed(() => state.value === 'empty')
  const isIdle = computed(() => state.value === 'idle')

  const execute = async (promise: Promise<T>): Promise<T> => {
    state.value = 'loading'
    error.value = null

    try {
      const result = await promise
      data.value = result

      // Check if result is empty (array or falsy)
      const isEmptyResult = Array.isArray(result) ? result.length === 0 : !result
      state.value = isEmptyResult ? 'empty' : 'success'

      options.onSuccess?.(result)
      return result
    } catch (e) {
      const err = e instanceof Error ? e : new Error(String(e))
      error.value = err
      state.value = 'error'
      options.onError?.(err)
      throw err
    }
  }

  const reset = () => {
    state.value = 'idle'
    data.value = options.initialValue
    error.value = null
  }

  return {
    state: state as Ref<AsyncStateType>,
    data: data as Ref<T | undefined>,
    error,
    isLoading,
    isSuccess,
    isError,
    isEmpty,
    isIdle,
    execute,
    reset
  }
}

/**
 * Helper to create a simple loading/error state without complex logic
 */
export function useState<T>(initialValue?: T) {
  return {
    data: ref<T | undefined>(initialValue),
    loading: ref(false),
    error: ref<Error | null>(null)
  }
}
