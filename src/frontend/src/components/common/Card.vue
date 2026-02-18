<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  variant?: 'default' | 'bordered' | 'elevated'
  padding?: 'none' | 'sm' | 'md' | 'lg'
  hoverable?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'default',
  padding: 'md',
  hoverable: false
})

const cardClasses = computed(() => {
  const base = 'bg-white rounded-xl shadow-sm border overflow-hidden'
  
  const variants: Record<string, string> = {
    default: 'border-gray-200',
    bordered: 'border-gray-300',
    elevated: 'border-transparent shadow-md'
  }
  
  const paddings: Record<string, string> = {
    none: '',
    sm: 'p-3',
    md: 'p-6',
    lg: 'p-8'
  }
  
  const hover = props.hoverable ? 'transition-shadow duration-200 hover:shadow-md cursor-pointer' : ''
  
  return `${base} ${variants[props.variant]} ${paddings[props.padding]} ${hover}`
})
</script>

<template>
  <div :class="cardClasses" class="card-component">
    <div v-if="$slots.header || $slots.title" class="card-header">
      <slot name="header">
        <slot name="title">
          <h3 v-if="$slots.title" class="text-lg font-semibold text-gray-900">
            <slot name="title" />
          </h3>
        </slot>
      </slot>
    </div>
    
    <div :class="{ 'pt-4': $slots.header || $slots.title }">
      <slot />
    </div>
    
    <div v-if="$slots.footer" class="card-footer mt-4">
      <slot name="footer" />
    </div>
  </div>
</template>

<style scoped>
.card-component {
  @apply transition-all duration-200;
}
</style>
