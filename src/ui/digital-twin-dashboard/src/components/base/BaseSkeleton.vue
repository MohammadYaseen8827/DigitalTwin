<template>
  <div class="skeleton" :class="{ inline }" :style="style"></div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps({
  width: {
    type: [String, Number],
    default: '100%'
  },
  height: {
    type: [String, Number],
    default: '1rem'
  },
  radius: {
    type: String,
    default: 'var(--radius-md)'
  },
  inline: {
    type: Boolean,
    default: false
  }
})

const formatSize = (value: string | number) =>
  typeof value === 'number' ? `${value}px` : value

const style = computed(() => ({
  width: formatSize(props.width),
  height: formatSize(props.height),
  borderRadius: props.radius
}))
</script>

<style scoped>
.skeleton {
  position: relative;
  overflow: hidden;
  background: color-mix(in srgb, var(--color-surface-alt) 88%, transparent);
}

.inline {
  display: inline-block;
}

.skeleton::after {
  content: '';
  position: absolute;
  inset: 0;
  transform: translateX(-100%);
  background: linear-gradient(
    90deg,
    transparent,
    color-mix(in srgb, var(--color-surface) 65%, white 35%),
    transparent
  );
  animation: shimmer 1.4s ease-in-out infinite;
}

@keyframes shimmer {
  100% {
    transform: translateX(100%);
  }
}
</style>
