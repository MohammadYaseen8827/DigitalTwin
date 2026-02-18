import { onScopeDispose, readonly, ref } from 'vue'

export interface RetryOptions {
  retries?: number
  baseDelay?: number
  maxDelay?: number
  jitter?: boolean
  /**
   * Decide whether the error is retryable. Return false to stop retrying immediately.
   */
  shouldRetry?: (error: unknown, attempt: number) => boolean
  /**
   * Optional hook executed before each retry attempt (after the first failure).
   */
  onRetry?: (context: { attempt: number; delay: number; error: unknown }) => void
}

export interface RetryState {
  readonly isRunning: boolean
  readonly attempt: number
  readonly lastError: unknown
  readonly isCancelled: boolean
}

const DEFAULT_OPTIONS: Required<Omit<RetryOptions, 'shouldRetry' | 'onRetry'>> = {
  retries: 3,
  baseDelay: 500,
  maxDelay: 4000,
  jitter: true
}

function computeDelay(attempt: number, options: Required<Omit<RetryOptions, 'shouldRetry' | 'onRetry'>>): number {
  const exponential = options.baseDelay * Math.pow(2, Math.max(0, attempt - 1))
  const capped = Math.min(options.maxDelay, exponential)

  if (!options.jitter) {
    return capped
  }

  const jitterFactor = 0.7 + Math.random() * 0.3
  return Math.round(capped * jitterFactor)
}

function createAbortError() {
  return new DOMException('Operation cancelled', 'AbortError')
}

async function runWithBackoff<T>(
  operation: (signal: AbortSignal) => Promise<T>,
  options: RetryOptions,
  hooks?: {
    onAttempt?: (attempt: number) => void
    onRetry?: (context: { attempt: number; delay: number; error: unknown }) => void
    onError?: (error: unknown) => void
  }
): Promise<T> {
  const merged: RetryOptions = { ...DEFAULT_OPTIONS, ...options }
  const retries = merged.retries ?? DEFAULT_OPTIONS.retries
  const baseDelay = merged.baseDelay ?? DEFAULT_OPTIONS.baseDelay
  const maxDelay = merged.maxDelay ?? DEFAULT_OPTIONS.maxDelay
  const jitter = merged.jitter ?? DEFAULT_OPTIONS.jitter

  const retryableOptions = {
    retries,
    baseDelay,
    maxDelay,
    jitter
  }

  const shouldRetry = merged.shouldRetry ?? (() => true)
  const onRetry = merged.onRetry ?? hooks?.onRetry

  let attempt = 0
  let lastError: unknown

  let abortController = new AbortController()
  let timeoutHandle: ReturnType<typeof setTimeout> | null = null

  const cleanup = () => {
    if (timeoutHandle) {
      clearTimeout(timeoutHandle)
      timeoutHandle = null
    }
    abortController.abort()
  }

  try {
    while (attempt <= retries) {
      if (attempt > 0) {
        hooks?.onAttempt?.(attempt)
      }

      try {
        const result = await operation(abortController.signal)
        return result
      } catch (error) {
        lastError = error
        hooks?.onError?.(error)

        if (attempt >= retries || !shouldRetry(error, attempt + 1)) {
          throw error
        }

        const delay = computeDelay(attempt + 1, retryableOptions)
        onRetry?.({ attempt: attempt + 1, delay, error })

        await new Promise((resolve, reject) => {
          timeoutHandle = setTimeout(resolve, delay)
          abortController.signal.addEventListener('abort', () => {
            clearTimeout(timeoutHandle ?? undefined)
            timeoutHandle = null
            reject(createAbortError())
          })
        })

        // Fresh controller for the next attempt
        abortController = new AbortController()
        attempt += 1
      }
    }

    throw lastError ?? new Error('Retry operation failed')
  } finally {
    cleanup()
  }
}

export function useRetryBackoff(defaultOptions: RetryOptions = {}) {
  const isRunning = ref(false)
  const attempt = ref(0)
  const lastError = ref<unknown>(null)
  const isCancelled = ref(false)
  const abortController = ref<AbortController | null>(null)

  const cancel = () => {
    if (isCancelled.value) {
      return
    }

    isCancelled.value = true
    abortController.value?.abort()
  }

  onScopeDispose(() => {
    cancel()
  })

  const execute = async <T>(
    operation: (signal: AbortSignal) => Promise<T>,
    overrideOptions: RetryOptions = {}
  ): Promise<T> => {
    if (isRunning.value) {
      throw new Error('Retry operation already in progress')
    }

    const options: RetryOptions = {
      ...defaultOptions,
      ...overrideOptions
    }

    isRunning.value = true
    isCancelled.value = false
    attempt.value = 0
    lastError.value = null

    try {
      const result = await runWithBackoff<T>(operation, options, {
        onAttempt: currentAttempt => {
          attempt.value = currentAttempt
        },
        onError: error => {
          lastError.value = error
        },
        onRetry: context => {
          options.onRetry?.(context)
        }
      })

      return result
    } finally {
      isRunning.value = false
      abortController.value = null
    }
  }

  return {
    execute,
    cancel,
    state: readonly({
      isRunning,
      attempt,
      lastError,
      isCancelled
    })
  }
}

export async function retryWithBackoff<T>(
  operation: (signal: AbortSignal) => Promise<T>,
  options: RetryOptions = {}
): Promise<T> {
  return runWithBackoff(operation, options)
}
