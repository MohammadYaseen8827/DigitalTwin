<script setup lang="ts">
import { ref, computed, useCssModule } from 'vue'
import { ChevronUp, ChevronDown, ChevronsUpDown } from 'lucide-vue-next'
import { useUIStore } from '@/stores/ui.store'

interface Column {
  key: string
  label: string
  width?: string
  align?: 'left' | 'center' | 'right'
  sortable?: boolean
}

interface Props {
  columns: Column[]
  items: any[]
  loading?: boolean
  emptyText?: string
  stickyHeader?: boolean
  hover?: boolean
  density?: 'compact' | 'comfortable'
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  emptyText: 'No records matching current protocol filter.',
  stickyHeader: true,
  hover: true
})

const styles = useCssModule()
const uiStore = useUIStore()

const currentDensity = computed(() => props.density || uiStore.density)

// Sorting
const sortKey = ref('')
const sortOrder = ref<'asc' | 'desc'>('asc')

function toggleSort(key: string) {
  if (sortKey.value === key) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortKey.value = key
    sortOrder.value = 'asc'
  }
}

const sortedItems = computed(() => {
  if (!sortKey.value) return props.items
  return [...props.items].sort((a, b) => {
    const valA = a[sortKey.value]
    const valB = b[sortKey.value]
    if (valA < valB) return sortOrder.value === 'asc' ? -1 : 1
    if (valA > valB) return sortOrder.value === 'asc' ? 1 : -1
    return 0
  })
})
</script>

<template>
  <div :class="styles['table-container']">
    <table :class="[
      styles['table'], 
      stickyHeader && styles['table--sticky'],
      styles[`table--${currentDensity}`]
    ]">
      <thead>
        <tr>
          <th
            v-for="col in columns"
            :key="col.key"
            :style="{ width: col.width, textAlign: col.align || 'left' }"
            :class="[col.sortable && styles['th--sortable']]"
            @click="col.sortable && toggleSort(col.key)"
          >
            <div :class="styles['th-content']">
              <span>{{ col.label }}</span>
              <template v-if="col.sortable">
                <div :class="styles['sort-icon']">
                  <ChevronsUpDown v-if="sortKey !== col.key" :width="10" :height="10" />
                  <ChevronUp v-else-if="sortOrder === 'asc'" :width="10" :height="10" />
                  <ChevronDown v-else :width="10" :height="10" />
                </div>
              </template>
            </div>
          </th>
        </tr>
      </thead>
      <tbody>
        <template v-if="loading">
          <tr v-for="i in 5" :key="i">
            <td v-for="col in columns" :key="col.key">
              <div :class="styles['skeleton-wrap']">
                <div :class="styles['skeleton']" :style="{ width: (40 + Math.random() * 50) + '%' }" />
              </div>
            </td>
          </tr>
        </template>
        <template v-else-if="items.length === 0">
          <tr>
            <td :colspan="columns.length" :class="styles['empty-cell']">
              <div :class="styles['empty-state']">
                <p>{{ emptyText }}</p>
              </div>
            </td>
          </tr>
        </template>
        <template v-else>
          <tr
            v-for="(item, idx) in sortedItems"
            :key="item.id || idx"
            :class="[hover && styles['tr--hover']]"
          >
            <td
              v-for="col in columns"
              :key="col.key"
              :style="{ textAlign: col.align || 'left' }"
            >
              <slot :name="`cell-${col.key}`" :item="item" :index="idx">
                {{ item[col.key] }}
              </slot>
            </td>
          </tr>
        </template>
      </tbody>
    </table>
  </div>
</template>

<style module>
.table-container {
  width: 100%;
  overflow-x: auto;
  border-radius: var(--radius-lg);
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  scrollbar-width: thin;
}

.table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  font-family: var(--font-family);
}

.table--sticky thead th {
  position: sticky;
  top: 0;
  z-index: 20;
}

thead th {
  background: var(--color-surface-alt);
  padding: var(--space-12) var(--space-16);
  font-size: 10px;
  font-weight: 800;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.12em;
  border-bottom: 1px solid var(--color-border);
  white-space: nowrap;
  transition: all var(--transition-fast);
}

.th--sortable {
  cursor: pointer;
  user-select: none;
}

.th--sortable:hover {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

tbody td {
  padding: var(--space-14) var(--space-16);
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  border-bottom: 1px solid var(--color-border-subtle);
  vertical-align: middle;
  transition: background var(--transition-fast);
}

.table--compact tbody td {
  padding: var(--space-8) var(--space-12);
  font-size: var(--font-size-xs);
}

.tr--hover:hover td {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

.tr--hover {
  transition: all var(--transition-fast);
}

.tr--hover:hover {
  box-shadow: inset 2px 0 0 var(--color-primary);
}

.th-content {
  display: flex;
  align-items: center;
  gap: var(--space-8);
}

.sort-icon {
  display: flex;
  align-items: center;
  color: var(--color-text-muted);
}

.skeleton-wrap {
  padding: var(--space-4) 0;
}

.skeleton {
  height: 12px;
  background: var(--color-border);
  border-radius: 4px;
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0% { opacity: 0.3; }
  50% { opacity: 0.6; }
  100% { opacity: 0.3; }
}

.empty-cell {
  padding: var(--space-48) 0;
}

.empty-state {
  text-align: center;
  color: var(--color-text-dim);
  font-size: var(--font-size-sm);
  font-style: italic;
}
</style>
