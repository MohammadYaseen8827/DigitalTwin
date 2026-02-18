import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'
import vueJsx from '@vitejs/plugin-vue-jsx'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue({
      template: {
        compilerOptions: {
          // treat all tags with a dash as custom elements
          isCustomElement: (tag) => tag.includes('-')
        }
      }
    }),
    vueJsx(),
  ],

  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      '~': resolve(__dirname, 'src'),
    },
    extensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json', '.vue']
  },

  css: {
    preprocessorOptions: {
      scss: {
        additionalData: `@import "@/styles/variables.scss";`
      }
    },
    postcss: {
      plugins: [
        require('autoprefixer'),
        require('cssnano')({
          preset: ['default', {
            discardComments: { removeAll: true },
          }]
        })
      ]
    }
  },

  build: {
    target: 'es2015',
    outDir: 'dist',
    assetsDir: 'assets',
    sourcemap: false,
    rollupOptions: {
      output: {
        manualChunks: {
          // Vendor chunks
          'vue-vendor': ['vue', 'vue-router', 'pinia'],
          'ui-components': ['@/components/base', '@/components/layout'],
          'charting': ['echarts', 'vue-echarts'],
          'utilities': ['lodash-es', 'moment', 'axios'],
          
          // Route-based chunks for better caching
          'dashboard-pages': ['@/views/Dashboard.vue', '@/views/Overview.vue'],
          'maintenance-pages': ['@/views/Maintenance.vue', '@/views/MaintenanceSchedule.vue'],
          'analytics-pages': ['@/views/Analytics.vue', '@/views/Reports.vue'],
          'configuration-pages': ['@/views/Configuration.vue', '@/views/Settings.vue'],
          
          // Heavy component chunks
          'data-tables': ['@/components/data', '@/components/grid'],
          'forms': ['@/components/forms', '@/components/input'],
          'charts': ['@/components/charts', '@/components/visualization'],
        },
        chunkFileNames: 'assets/js/[name]-[hash].js',
        entryFileNames: 'assets/js/[name]-[hash].js',
        assetFileNames: (assetInfo) => {
          if (assetInfo.name) {
            if (assetInfo.name.endsWith('.css')) {
              return 'assets/css/[name]-[hash].[ext]'
            }
            if (/\.(png|jpe?g|gif|svg|webp|avif)$/.test(assetInfo.name)) {
              return 'assets/images/[name]-[hash].[ext]'
            }
            if (/\.(woff2?|eot|ttf|otf)$/.test(assetInfo.name)) {
              return 'assets/fonts/[name]-[hash].[ext]'
            }
          }
          return 'assets/[name]-[hash].[ext]'
        }
      },
      external: []
    },
    
    // Performance optimizations
    chunkSizeWarningLimit: 1000,
    assetsInlineLimit: 4096, // Inline assets smaller than 4kb
    
    terserOptions: {
      compress: {
        drop_console: true,
        drop_debugger: true,
        pure_funcs: ['console.info', 'console.debug', 'console.warn']
      },
      mangle: {
        properties: {
          regex: /^__/,
        }
      },
      format: {
        comments: false
      }
    },
    
    // CSS optimization
    cssCodeSplit: true,
    cssMinify: 'esbuild',
  },

  server: {
    host: '0.0.0.0',
    port: 3000,
    strictPort: false,
    open: true,
    cors: true,
    
    // Proxy for API requests
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
        ws: true
      }
    }
  },

  preview: {
    host: '0.0.0.0',
    port: 5000,
    strictPort: true,
    open: true
  },

  // Environment variables
  define: {
    __VUE_OPTIONS_API__: true,
    __VUE_PROD_DEVTOOLS__: false,
    __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: false
  },

  // Experimental features
  experimental: {
    renderBuiltUrl(filename, { hostType }) {
      if (hostType === 'js') {
        return { runtime: `window.__publicUrl + ${JSON.stringify(filename)}` }
      }
      return { relative: true }
    }
  },

  // Worker configuration
  worker: {
    format: 'es',
    rollupOptions: {
      output: {
        entryFileNames: 'assets/workers/[name]-[hash].js'
      }
    }
  }
})