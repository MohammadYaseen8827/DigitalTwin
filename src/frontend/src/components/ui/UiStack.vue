<script setup lang="ts">
import { useCssModule, computed } from 'vue'

interface Props {
  gap?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | 'none'
  align?: 'start' | 'center' | 'end' | 'stretch'
  as?: string
}

const props = withDefaults(defineProps<Props>(), {
  gap: 'md',
  align: 'stretch',
  as: 'div'
})

const styles = useCssModule()

const gapMap: Record<string, string> = {
  'none': '0',
  'xs': 'var(--space-4)',
  'sm': 'var(--space-8)',
  'md': 'var(--space-16)',
  'lg': 'var(--space-24)',
  'xl': 'var(--space-32)'
}

const classes = computed(() => [
  styles['stack'],
  styles[`align--${props.align}`]
])
</script>

<template>
  <component 
    :is="as" 
    :class="classes" 
    :style="{ gap: gapMap[gap] }"
  >
    <slot />
  </component>
</template>

<style module>
.stack {
  display: flex;
  flex-direction: column;
}

.align--start { align-items: flex-start; }
.align--center { align-items: center; }
.align--end { align-items: flex-end; }
.align--stretch { align-items: stretch; }
</style>
