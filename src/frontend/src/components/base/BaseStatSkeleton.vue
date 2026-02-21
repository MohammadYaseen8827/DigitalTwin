<template>
  <BaseCard class="stat-skeleton" variant="solid">
    <div class="stat-skeleton__content">
      <BaseSkeleton width="48px" height="48px" radius="var(--radius-lg)" />
      <div class="stat-skeleton__meta">
        <BaseSkeleton width="60%" height="24px" />
        <BaseSkeleton width="40%" height="16px" />
      </div>
    </div>
  </BaseCard>
</template>

<script setup lang="ts">
import BaseCard from './BaseCard.vue'
import BaseSkeleton from './BaseSkeleton.vue'
</script>

<style scoped>
.stat-skeleton {
  display: flex;
  align-items: center;
  padding: var(--space-16);
}

.stat-skeleton__content {
  display: flex;
  gap: var(--space-12);
  align-items: center;
  width: 100%;
}

.stat-skeleton__meta {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
}
</style>
