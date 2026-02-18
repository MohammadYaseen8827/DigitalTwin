<template>
  <span class="tooltip-icon" role="button" tabindex="0" :aria-label="tooltip" @focus="show = true" @blur="show = false" @mouseenter="show = true" @mouseleave="show = false">
    <slot />
    <span v-if="show" class="tooltip-bubble" role="tooltip">{{ tooltip }}</span>
  </span>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps({
  tooltip: {
    type: String,
    required: true
  }
})

const show = ref(false)
</script>

<style scoped>
.tooltip-icon {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  cursor: help;
}

.tooltip-bubble {
  position: absolute;
  bottom: 120%;
  left: 50%;
  transform: translateX(-50%);
  padding: 6px 10px;
  background: rgba(15, 23, 42, 0.9);
  color: #f8fafc;
  border-radius: 6px;
  font-size: 12px;
  line-height: 1.3;
  white-space: nowrap;
  box-shadow: 0 4px 12px rgba(15, 23, 42, 0.4);
  z-index: 10;
}
</style>
