<script setup lang="ts">
import { ref, computed } from 'vue'

interface Column {
  key: string
  label: string
  sortable?: boolean
  width?: string
  align?: 'left' | 'center' | 'right'
}

interface Props {
  columns: Column[]
  data: Record<string, unknown>[]
  loading?: boolean
  emptyMessage?: string
  rowKey?: string
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  emptyMessage: 'No data available',
  rowKey: 'id'
})

const emit = defineEmits<{
  sort: [key: string, direction: 'asc' | 'desc']
  rowClick: [row: Record<string, unknown>]
}>()

const sortKey = ref<string>('')
const sortDirection = ref<'asc' | 'desc'>('asc')

const sortedData = computed(() => {
  if (!sortKey.value) return props.data
  return [...props.data].sort((a, b) => {
    const aVal = a[sortKey.value]
    const bVal = b[sortKey.value]
    if (aVal === bVal) return 0
    const comparison = aVal! > bVal! ? 1 : -1
    return sortDirection.value === 'asc' ? comparison : -comparison
  })
})

const handleSort = (key: string) => {
  if (sortKey.value === key) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortKey.value = key
    sortDirection.value = 'asc'
  }
  emit('sort', key, sortDirection.value)
}

const alignClasses: Record<string, string> = {
  left: 'text-left',
  center: 'text-center',
  right: 'text-right'
}

const getSortIcon = (key: string) => {
  if (sortKey.value !== key) return '↕'
  return sortDirection.value === 'asc' ? '↑' : '↓'
}
</script>

<template>
  <div class="overflow-hidden rounded-lg border border-gray-200">
    <table class="min-w-full divide-y divide-gray-200">
      <thead class="bg-gray-50">
        <tr>
          <th v-for="col in columns" :key="col.key" :style="{ width: col.width }" :class="[
            'px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider',
            col.sortable ? 'cursor-pointer hover:bg-gray-100 select-none' : '',
            alignClasses[col.align || 'left']
          ]" @click="col.sortable && handleSort(col.key)">
            <div class="flex items-center gap-1">
              {{ col.label }}
              <span v-if="col.sortable" class="text-gray-400">{{ getSortIcon(col.key) }}</span>
            </div>
          </th>
        </tr>
      </thead>
      <tbody class="bg-white divide-y divide-gray-200">
        <tr v-if="loading">
          <td :colspan="columns.length" class="px-6 py-12 text-center">
            <Spinner size="lg" />
          </td>
        </tr>
        <tr v-else-if="data.length === 0">
          <td :colspan="columns.length" class="px-6 py-12 text-center text-gray-500">{{ emptyMessage }}</td>
        </tr>
        <template v-else>
          <tr v-for="row in sortedData" :key="row[rowKey] as string" class="hover:bg-gray-50 transition-colors cursor-pointer" @click="emit('rowClick', row)">
            <td v-for="col in columns" :key="col.key" :class="['px-6 py-4 whitespace-nowrap text-sm text-gray-900', alignClasses[col.align || 'left']]">
              <slot :name="`cell-${col.key}`" :row="row" :value="row[col.key]">
                {{ row[col.key] }}
              </slot>
            </td>
          </tr>
        </template>
      </tbody>
    </table>
  </div>
</template>
