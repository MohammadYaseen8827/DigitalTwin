<script setup lang="ts">
import { useCssModule, computed } from 'vue'

interface Props {
  direction?: 'row' | 'column' | 'row-reverse' | 'column-reverse'
  align?: 'start' | 'center' | 'end' | 'baseline' | 'stretch'
  justify?: 'start' | 'center' | 'end' | 'between' | 'around' | 'evenly'
  gap?: number | string | 'xs' | 'sm' | 'md' | 'lg' | 'xl' | 'none'
  wrap?: boolean
  inline?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  direction: 'row',
  align: 'stretch',
  justify: 'start',
  gap: 'md',
  wrap: false,
  inline: false
})

const styles = useCssModule()

const gapValue = computed(() => {
  if (typeof props.gap === 'number') return `${props.gap}px`
  const gapMap: Record<string, string> = {
    'none': '0',
    'xs': 'var(--space-4)',
    'sm': 'var(--space-8)',
    'md': 'var(--space-16)',
    'lg': 'var(--space-24)',
    'xl': 'var(--space-32)'
  }
  return gapMap[props.gap] || props.gap
})

const classes = computed(() => [
  styles['flex'],
  styles[`flex--${props.direction}`],
  styles[`align--${props.align}`],
  styles[`justify--${props.justify}`],
  props.wrap && styles['flex--wrap'],
  props.inline && styles['flex--inline']
])
</script>

<template>
  <div :class="classes" :style="{ gap: gapValue }">
    <slot />
  </div>
</template>

<style module>
.flex {
  display: flex;
}

.flex--inline {
  display: inline-flex;
}

.flex--row { flex-direction: row; }
.flex--column { flex-direction: column; }
.flex--row-reverse { flex-direction: row-reverse; }
.flex--column-reverse { flex-direction: column-reverse; }

.align--start { align-items: flex-start; }
.align--center { align-items: center; }
.align--end { align-items: flex-end; }
.align--baseline { align-items: baseline; }
.align--stretch { align-items: stretch; }

.justify--start { justify-content: flex-start; }
.justify--center { justify-content: center; }
.justify--end { justify-content: flex-end; }
.justify--between { justify-content: space-between; }
.justify--around { justify-content: space-around; }
.justify--evenly { justify-content: space-evenly; }

.flex--wrap {
  flex-wrap: wrap;
}
</style>
