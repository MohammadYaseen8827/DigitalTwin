<script setup lang="ts">
import PredictiveAnalytics from '../components/PredictiveAnalytics.vue'
</script>

<template>
  <main class="pa-page container">
    <PredictiveAnalytics />
  </main>
</template>

<style scoped>
.pa-page {
  padding: clamp(var(--space-16), 3vw, var(--space-24)) 0;
}
</style>