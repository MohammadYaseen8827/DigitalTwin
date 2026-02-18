<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watchEffect } from 'vue'
// @ts-ignore
import { debounce, throttle } from 'lodash-es'
// @ts-ignore
import { useVirtualList } from '@vueuse/core'

// Props
interface Props {
  data: any[]
  itemHeight?: number
  bufferSize?: number
}

const props = withDefaults(defineProps<Props>(), {
  itemHeight: 54,
  bufferSize: 5
})

// Emits
const emit = defineEmits(['retry', 'itemClick', 'scroll'])

// Reactive state
const searchQuery = ref('')
const sortField = ref('')
const sortOrder = ref<'asc' | 'desc'>('asc')
const isLoading = ref(false)
const error = ref<string | null>(null)

// Performance-optimized computed properties
const filteredData = computed(() => {
  if (!props.data?.length) return []
  
  let result = [...props.data]
  
  // Apply search filter
  if (searchQuery.value) {
    const term = searchQuery.value.toLowerCase()
    result = result.filter(item => 
      Object.values(item).some(val => 
        String(val).toLowerCase().includes(term)
      )
    )
  }
  
  // Apply sorting
  if (sortField.value) {
    result.sort((a, b) => {
      const aVal = a[sortField.value]
      const bVal = b[sortField.value]
      
      if (aVal < bVal) return sortOrder.value === 'asc' ? -1 : 1
      if (aVal > bVal) return sortOrder.value === 'asc' ? 1 : -1
      return 0
    })
  }
  
  return result
})

// Virtual scrolling for large datasets
const { list, containerProps, wrapperProps } = useVirtualList(
  filteredData,
  {
    itemHeight: props.itemHeight,
    overscan: props.bufferSize
  }
)

// Performance-optimized methods
const debouncedSearch = debounce((query: string) => {
  searchQuery.value = query
}, 300)

const throttledScroll = throttle((position: number) => {
  emit('scroll', position)
}, 100)

// Lazy loading for images
const loadImage = (img: HTMLImageElement) => {
  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const img = entry.target as HTMLImageElement
        img.src = img.dataset.src || ''
        observer.unobserve(img)
      }
    })
  })
  
  observer.observe(img)
}

// Memory management
let resizeObserver: ResizeObserver | null = null

onMounted(() => {
  // Setup resize observer for responsive adjustments
  resizeObserver = new ResizeObserver(throttle(() => {
    // Handle container resize
  }, 100))
  
  const container = document.querySelector('.virtual-container')
  if (container) {
    resizeObserver.observe(container)
  }
})

onUnmounted(() => {
  if (resizeObserver) {
    resizeObserver.disconnect()
  }
})

// Watch for data changes with performance considerations
watchEffect(() => {
  if (props.data?.length > 1000) {
    console.warn('Large dataset detected, consider pagination')
  }
})

// Expose methods for parent components
defineExpose({
  scrollToTop: () => {
    if (containerProps.ref) {
      (containerProps.ref as HTMLElement).scrollTop = 0
    }
  },
  refresh: () => {
    // Trigger refresh without re-rendering everything
    console.log('Refreshing virtual list')
  }
})
</script>

<template>
  <div class="performance-data-grid">
    <!-- Search and Filter Controls -->
    <div class="controls">
      <input
        v-model="searchQuery"
        @input="debouncedSearch(($event.target as HTMLInputElement).value)"
        placeholder="Search..."
        class="search-input"
        autocomplete="off"
      />
      
      <select v-model="sortField" class="sort-select">
        <option value="">Sort by...</option>
        <option value="name">Name</option>
        <option value="date">Date</option>
        <option value="status">Status</option>
      </select>
      
      <button 
        v-if="sortField"
        @click="sortOrder = sortOrder === 'asc' ? 'desc' : 'asc'"
        class="sort-toggle"
      >
        {{ sortOrder === 'asc' ? '↑' : '↓' }}
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="loading-state">
      <div class="spinner"></div>
      <span>Loading data...</span>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="error-state">
      <span>{{ error }}</span>
      <button @click="() => emit('retry')">Retry</button>
    </div>

    <!-- Virtual Scrolled List -->
    <div 
      v-else
      v-bind="containerProps"
      class="virtual-container"
      @scroll="throttledScroll(($event.target as HTMLElement).scrollTop)"
    >
      <div v-bind="wrapperProps" class="virtual-wrapper">
        <div
          v-for="item in list"
          :key="item.index"
          class="list-item"
          :style="{ height: `${itemHeight}px` }"
          @click="$emit('itemClick', item.data)"
        >
          <!-- Lazy-loaded image -->
          <img
            v-if="item.data.avatar"
            :data-src="item.data.avatar"
            alt=""
            class="avatar lazy-image"
            @load="loadImage($event.target as HTMLImageElement)"
          />
          
          <!-- Item content -->
          <div class="item-content">
            <div class="item-title">{{ item.data.name }}</div>
            <div class="item-subtitle">{{ item.data.description }}</div>
          </div>
          
          <!-- Status indicator -->
          <div 
            class="status-indicator"
            :class="`status-${item.data.status}`"
          >
            {{ item.data.status }}
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="!isLoading && filteredData.length === 0" class="empty-state">
      <div class="empty-icon">📭</div>
      <div class="empty-text">No data found</div>
    </div>
  </div>
</template>

<style scoped>
.performance-data-grid {
  display: flex;
  flex-direction: column;
  height: 100%;
  contain: layout style paint;
}

.controls {
  display: flex;
  gap: 1rem;
  padding: 1rem;
  background: var(--surface-color);
  border-bottom: 1px solid var(--border-color);
  contain: layout style;
}

.search-input,
.sort-select {
  padding: 0.5rem;
  border: 1px solid var(--border-color);
  border-radius: 4px;
  font-size: 0.875rem;
  transition: border-color 0.2s ease;
}

.search-input:focus,
.sort-select:focus {
  outline: none;
  border-color: var(--primary-color);
}

.virtual-container {
  flex: 1;
  overflow-y: auto;
  contain: strict;
  will-change: scroll-position;
}

.virtual-wrapper {
  position: relative;
  width: 100%;
  contain: layout style;
}

.list-item {
  display: flex;
  align-items: center;
  padding: 0 1rem;
  border-bottom: 1px solid var(--border-color);
  cursor: pointer;
  transition: background-color 0.15s ease;
  contain: layout style paint;
  transform: translateZ(0); /* Hardware acceleration */
}

.list-item:hover {
  background-color: var(--hover-color);
}

.list-item:active {
  background-color: var(--active-color);
}

.lazy-image {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  object-fit: cover;
  background: var(--skeleton-color);
  contain: layout style;
}

.item-content {
  flex: 1;
  margin-left: 1rem;
  min-width: 0;
  contain: layout style;
}

.item-title {
  font-weight: 500;
  margin-bottom: 0.25rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.item-subtitle {
  font-size: 0.875rem;
  color: var(--text-secondary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.status-indicator {
  padding: 0.25rem 0.5rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: uppercase;
  contain: layout style;
}

.status-active {
  background: var(--success-bg);
  color: var(--success-color);
}

.status-inactive {
  background: var(--warning-bg);
  color: var(--warning-color);
}

.loading-state,
.error-state,
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  text-align: center;
}

.spinner {
  width: 2rem;
  height: 2rem;
  border: 2px solid var(--border-color);
  border-top: 2px solid var(--primary-color);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* Performance optimizations */
@media (prefers-reduced-motion: reduce) {
  .list-item,
  .search-input,
  .sort-select {
    transition: none;
  }
  
  .spinner {
    animation: none;
  }
}

/* Containment for better rendering performance */
.virtual-container,
.virtual-wrapper,
.list-item {
  backface-visibility: hidden;
  perspective: 1000px;
}
</style>