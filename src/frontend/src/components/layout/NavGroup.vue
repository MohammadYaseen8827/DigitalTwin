<script setup lang="ts">
import { useCssModule } from 'vue'
import type { Component } from 'vue'
import { RouterLink } from 'vue-router'

export interface NavItem {
  name: string
  path: string
  icon: Component
}

export interface NavGroupProps {
  items: NavItem[]
  collapsed: boolean
}

const props = defineProps<NavGroupProps>()
const styles = useCssModule()
</script>

<template>
  <div :class="styles['items-list']">
    <RouterLink
      v-for="item in items"
      :key="item.path"
      :to="item.path"
      :class="styles['nav-link']"
      :title="collapsed ? item.name : undefined"
      v-slot="{ isActive }"
    >
      <div :class="[
        styles['nav-item'],
        isActive && styles['nav-item--active'],
        collapsed && styles['nav-item--collapsed']
      ]">
        <div :class="styles['icon-container']">
          <component
            :is="item.icon"
            :width="16"
            :height="16"
          />
        </div>
        <Transition name="fade">
          <span v-if="!collapsed" :class="styles['label']">{{ item.name }}</span>
        </Transition>
        <div v-if="isActive" :class="styles['glow-indicator']" />
      </div>
    </RouterLink>
  </div>
</template>

<style module>
.items-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.nav-link {
  text-decoration: none;
  outline: none;
}

.nav-item {
  position: relative;
  display: flex;
  align-items: center;
  gap: var(--space-12);
  padding: var(--space-8) var(--space-12);
  border-radius: var(--radius-lg);
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
  font-weight: 500;
  transition: all var(--transition-normal);
  cursor: pointer;
  border: 1px solid transparent;
}

.nav-item:hover {
  background: var(--color-surface-alt);
  color: var(--color-text-primary);
}

.nav-item--active {
  background: var(--color-surface-elevated);
  color: var(--color-primary);
  border-color: var(--color-border);
  box-shadow: var(--shadow-xs);
}

.nav-item--active::before {
  content: '';
  position: absolute;
  inset: 0;
  background: radial-gradient(circle at 0% 50%, rgba(var(--color-primary-rgb), 0.08), transparent 50%);
  pointer-events: none;
}

.nav-item--collapsed {
  justify-content: center;
  padding: var(--space-10);
}

.icon-container {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: all var(--transition-normal);
  color: var(--color-text-dim);
}

.nav-item--active .icon-container {
  color: var(--color-primary);
  filter: drop-shadow(0 0 8px var(--color-primary-glow));
}

.nav-item:hover .icon-container {
  color: var(--color-text-primary);
  transform: translateX(2px);
}

.label {
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  letter-spacing: var(--font-tracking-tight);
}

.glow-indicator {
  position: absolute;
  left: 4px;
  width: 2px;
  height: 12px;
  background: var(--color-primary);
  border-radius: var(--radius-full);
  box-shadow: 0 0 12px var(--color-primary);
}
</style>
