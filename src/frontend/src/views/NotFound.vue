<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

function goHome() {
  router.push('/dashboard')
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4">
    <div class="max-w-md w-full text-center">
      <div class="text-6xl font-bold text-primary-600">404</div>
      <h1 class="mt-4 text-3xl font-bold text-gray-900">Page Not Found</h1>
      <p class="mt-2 text-gray-600">
        Sorry, we couldn't find the page you're looking for.
      </p>
      <div class="mt-6">
        <button @click="goHome" class="btn-primary">
          Go to Dashboard
        </button>
      </div>
    </div>
  </div>
</template>
