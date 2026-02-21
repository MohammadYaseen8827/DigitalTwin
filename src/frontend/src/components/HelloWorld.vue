<script setup lang="ts">
defineProps<{
  msg: string
}>()
</script>

<template>
  <div class="greetings">
    <h1 class="platform-title">{{ msg }}</h1>
  </div>
</template>

<style scoped>
h1 {
  font-weight: 500;
  font-size: 2.6rem;
  position: relative;
  top: -10px;
}

.platform-title {
  font-size: 2.5rem;
  color: white;
  text-align: center;
  margin: 0;
  text-shadow: 0 2px 4px rgba(0,0,0,0.3);
}

@media (min-width: 1024px) {
  .platform-title {
    text-align: left;
    font-size: 2.8rem;
  }
}
</style>