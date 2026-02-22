import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import path from 'path';
import Icons from 'unplugin-icons/vite';
import Components from 'unplugin-vue-components/vite';
import { FileSystemIconLoader } from 'unplugin-icons/loaders';

export default defineConfig({
  plugins: [
    vue(),
    Icons({
      compiler: 'vue3',
      autoInstall: true,
      customCollections: {
        'app': FileSystemIconLoader('./src/assets/icons')
      }
    }),
    Components({
      dts: true,
      dirs: ['src/components'],
      deep: true,
      include: [/\.vue$/, /\.vue\?vue/, /\.tsx?$/]
    })
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src')
    }
  },
  server: {
    port: 5174,
    host: true,
    strictPort: true,
    watch: {
      usePolling: true
    }
  },
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
    sourcemap: true,
    chunkSizeWarningLimit: 1000
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './tests/setup.ts',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        '**/node_modules/',
        '**/dist/',
        '**/tests/',
        '**/*.d.ts',
        '**/types.ts',
        '**/main.ts',
        '**/vite-env.d.ts'
      ]
    }
  }
});