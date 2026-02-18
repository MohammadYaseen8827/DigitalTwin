import { vi } from 'vitest'

// Mock localStorage
export const mockLocalStorage = () => {
  const store: Record<string, string> = {}
  
  return {
    getItem: vi.fn((key: string) => store[key] || null),
    setItem: vi.fn((key: string, value: string) => {
      store[key] = value
    }),
    removeItem: vi.fn((key: string) => {
      delete store[key]
    }),
    clear: vi.fn(() => {
      Object.keys(store).forEach(key => delete store[key])
    }),
    get length() {
      return Object.keys(store).length
    },
    key: vi.fn((index: number) => {
      const keys = Object.keys(store)
      return keys[index] || null
    })
  }
}

// Mock console methods
export const mockConsole = () => {
  return {
    log: vi.spyOn(console, 'log').mockImplementation(() => {}),
    warn: vi.spyOn(console, 'warn').mockImplementation(() => {}),
    error: vi.spyOn(console, 'error').mockImplementation(() => {})
  }
}

// Mock router
export const mockRouter = () => {
  return {
    push: vi.fn(),
    replace: vi.fn(),
    go: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
    beforeEach: vi.fn(),
    afterEach: vi.fn()
  }
}

// Mock route
export const mockRoute = (params = {}, query = {}) => {
  return {
    params,
    query,
    hash: '',
    fullPath: '/',
    matched: []
  }
}

// Mock API responses
export const mockApiResponse = <T>(data: T, status = 200) => {
  return {
    data,
    status,
    statusText: 'OK',
    headers: {},
    config: {}
  }
}

// Mock error response
export const mockApiError = (message = 'API Error', status = 500) => {
  const error: any = new Error(message)
  error.response = {
    data: { message },
    status,
    statusText: 'Error'
  }
  return error
}

// Mock intersection observer
export const mockIntersectionObserver = () => {
  const observe = vi.fn()
  const unobserve = vi.fn()
  const disconnect = vi.fn()
  
  const mockObserver = vi.fn(() => ({
    observe,
    unobserve,
    disconnect,
    root: null,
    rootMargin: '',
    thresholds: [0]
  }))
  
  window.IntersectionObserver = mockObserver as any
  
  return { observe, unobserve, disconnect }
}

// Mock resize observer
export const mockResizeObserver = () => {
  const observe = vi.fn()
  const unobserve = vi.fn()
  const disconnect = vi.fn()
  
  const mockObserver = vi.fn(() => ({
    observe,
    unobserve,
    disconnect,
    disconnect: vi.fn()
  }))
  
  window.ResizeObserver = mockObserver as any
  
  return { observe, unobserve, disconnect }
}

// Mock match media
export const mockMatchMedia = () => {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: vi.fn().mockImplementation(query => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })),
  })
}

// Mock scrollIntoView
export const mockScrollIntoView = () => {
  Element.prototype.scrollIntoView = vi.fn()
}

// Mock getBoundingClientRect
export const mockGetBoundingClientRect = (rect: Partial<DOMRect> = { x: 0, y: 0, width: 100, height: 100 }) => {
  const fullRect: DOMRect = {
    x: rect.x || 0,
    y: rect.y || 0,
    width: rect.width || 100,
    height: rect.height || 100,
    top: rect.y || 0,
    left: rect.x || 0,
    right: (rect.x || 0) + (rect.width || 100),
    bottom: (rect.y || 0) + (rect.height || 100),
    toJSON: vi.fn()
  }
  Element.prototype.getBoundingClientRect = vi.fn(() => fullRect)
}

// Create mock DOM element
export const createMockElement = (tagName = 'div', attributes: Record<string, string> = {}) => {
  const element = document.createElement(tagName)
  Object.entries(attributes).forEach(([key, value]) => {
    element.setAttribute(key, value)
  })
  return element
}

// Mock fetch API
export const mockFetch = () => {
  const mockFetchImpl = vi.fn()
  global.fetch = mockFetchImpl
  return mockFetchImpl
}

// Mock WebSocket
export const mockWebSocket = () => {
  const mockWebSocketClass = vi.fn()
  mockWebSocketClass.prototype.send = vi.fn()
  mockWebSocketClass.prototype.close = vi.fn()
  mockWebSocketClass.prototype.addEventListener = vi.fn()
  mockWebSocketClass.prototype.removeEventListener = vi.fn()
  
  Object.defineProperty(window, 'WebSocket', {
    writable: true,
    value: mockWebSocketClass
  })
  
  return mockWebSocketClass
}

// Mock FileReader
export const mockFileReader = () => {
  const mockFileReaderClass = vi.fn()
  mockFileReaderClass.prototype.readAsText = vi.fn()
  mockFileReaderClass.prototype.readAsDataURL = vi.fn()
  
  Object.defineProperty(window, 'FileReader', {
    writable: true,
    value: mockFileReaderClass
  })
  
  return mockFileReaderClass
}

// Mock canvas context
export const mockCanvasContext = () => {
  const mockContext: any = {
    fillRect: vi.fn(),
    strokeRect: vi.fn(),
    clearRect: vi.fn(),
    getImageData: vi.fn(),
    putImageData: vi.fn(),
    createImageData: vi.fn(),
    setTransform: vi.fn(),
    drawImage: vi.fn(),
    save: vi.fn(),
    restore: vi.fn(),
    fillText: vi.fn(),
    measureText: vi.fn(() => ({ width: 100, height: 20 })),
    beginPath: vi.fn(),
    closePath: vi.fn(),
    moveTo: vi.fn(),
    lineTo: vi.fn(),
    arc: vi.fn(),
    fill: vi.fn(),
    stroke: vi.fn(),
    translate: vi.fn(),
    scale: vi.fn(),
    rotate: vi.fn(),
    rect: vi.fn(),
    clip: vi.fn(),
  })
  
  HTMLCanvasElement.prototype.getContext = vi.fn(() => mockContext)
  
  return mockContext
}
