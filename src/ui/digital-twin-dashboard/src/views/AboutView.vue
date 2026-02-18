<template>
  <SectionContainer title="About the Digital Twin Platform" eyebrow="Platform overview" maxWidth="lg" bordered>
    <div class="about-content">
      <p>
        The Digital Twin Platform brings together real-time telemetry, predictive analytics, and immersive 3D visualizations
        to give operations teams a crystal-clear view of every asset on the factory floor. This dashboard showcases the
        capabilities currently available in the unified platform.
      </p>

      <div class="capabilities-grid">
        <BaseCard variant="soft" hoverable>
          <template #header>Real-time Monitoring</template>
          <p>
            Track live sensor data across pressure, humidity, vibration, and power channels. Chart widgets use the same
            design tokens for consistent theming and responsive layouts.
          </p>
        </BaseCard>
        <BaseCard variant="soft" hoverable>
          <template #header>Predictive Analytics</template>
          <p>
            Machine-learning pipelines forecast maintenance needs and highlight potential anomalies before they impact
            production schedules.
          </p>
        </BaseCard>
        <BaseCard variant="soft" hoverable>
          <template #header>3D & Spatial Awareness</template>
          <p>
            Interactive floor plans and 3D viewers give engineers spatial context, helping them pinpoint issues faster and
            coordinate on-site interventions.
          </p>
        </BaseCard>
      </div>

      <BaseCard variant="bordered">
        <template #header>What’s Next</template>
        <p>
          The current modernization initiative focuses on unifying UI components, responsive behavior, and design tokens.
          Upcoming work includes accessibility enhancements, advanced drill-down interactions, and role-based dashboards.
        </p>
      </BaseCard>
    </div>
  </SectionContainer>
</template>

<script setup lang="ts">
import SectionContainer from '../components/base/SectionContainer.vue'
import BaseCard from '../components/base/BaseCard.vue'
</script>

<style scoped>
.about-content {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
  font-size: var(--font-size-base);
  color: var(--color-text-secondary);
}

.about-content p {
  margin: 0;
  line-height: var(--font-lineheight-normal);
}

.capabilities-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: var(--space-16);
}

@media (max-width: 768px) {
  .about-content {
    gap: var(--space-20);
  }
}
</style>
