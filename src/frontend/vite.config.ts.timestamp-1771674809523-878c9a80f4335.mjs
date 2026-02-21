// vite.config.ts
import { defineConfig } from "file:///D:/Work/Diploma/src/frontend/node_modules/vite/dist/node/index.js";
import vue from "file:///D:/Work/Diploma/src/frontend/node_modules/@vitejs/plugin-vue/dist/index.mjs";
import path from "path";
import Icons from "file:///D:/Work/Diploma/src/frontend/node_modules/unplugin-icons/dist/vite.mjs";
import Components from "file:///D:/Work/Diploma/src/frontend/node_modules/unplugin-vue-components/dist/vite.js";
import { FileSystemIconLoader } from "file:///D:/Work/Diploma/src/frontend/node_modules/unplugin-icons/dist/loaders.mjs";
var __vite_injected_original_dirname = "D:\\Work\\Diploma\\src\\frontend";
var vite_config_default = defineConfig({
  plugins: [
    vue(),
    Icons({
      compiler: "vue3",
      autoInstall: true,
      customCollections: {
        "app": FileSystemIconLoader("./src/assets/icons")
      }
    }),
    Components({
      dts: true,
      dirs: ["src/components"],
      deep: true,
      include: [/\.vue$/, /\.vue\?vue/, /\.tsx?$/]
    })
  ],
  resolve: {
    alias: {
      "@": path.resolve(__vite_injected_original_dirname, "./src")
    }
  },
  server: {
    port: 3e3,
    host: true,
    strictPort: true,
    watch: {
      usePolling: true
    }
  },
  build: {
    outDir: "dist",
    assetsDir: "assets",
    sourcemap: true,
    chunkSizeWarningLimit: 1e3
  },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: "./tests/setup.ts",
    coverage: {
      provider: "v8",
      reporter: ["text", "json", "html"],
      exclude: [
        "**/node_modules/",
        "**/dist/",
        "**/tests/",
        "**/*.d.ts",
        "**/types.ts",
        "**/main.ts",
        "**/vite-env.d.ts"
      ]
    }
  }
});
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcudHMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJEOlxcXFxXb3JrXFxcXERpcGxvbWFcXFxcc3JjXFxcXGZyb250ZW5kXCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ZpbGVuYW1lID0gXCJEOlxcXFxXb3JrXFxcXERpcGxvbWFcXFxcc3JjXFxcXGZyb250ZW5kXFxcXHZpdGUuY29uZmlnLnRzXCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ltcG9ydF9tZXRhX3VybCA9IFwiZmlsZTovLy9EOi9Xb3JrL0RpcGxvbWEvc3JjL2Zyb250ZW5kL3ZpdGUuY29uZmlnLnRzXCI7aW1wb3J0IHsgZGVmaW5lQ29uZmlnIH0gZnJvbSAndml0ZSc7XHJcbmltcG9ydCB2dWUgZnJvbSAnQHZpdGVqcy9wbHVnaW4tdnVlJztcclxuaW1wb3J0IHBhdGggZnJvbSAncGF0aCc7XHJcbmltcG9ydCBJY29ucyBmcm9tICd1bnBsdWdpbi1pY29ucy92aXRlJztcclxuaW1wb3J0IENvbXBvbmVudHMgZnJvbSAndW5wbHVnaW4tdnVlLWNvbXBvbmVudHMvdml0ZSc7XHJcbmltcG9ydCB7IEZpbGVTeXN0ZW1JY29uTG9hZGVyIH0gZnJvbSAndW5wbHVnaW4taWNvbnMvbG9hZGVycyc7XHJcblxyXG5leHBvcnQgZGVmYXVsdCBkZWZpbmVDb25maWcoe1xyXG4gIHBsdWdpbnM6IFtcclxuICAgIHZ1ZSgpLFxyXG4gICAgSWNvbnMoe1xyXG4gICAgICBjb21waWxlcjogJ3Z1ZTMnLFxyXG4gICAgICBhdXRvSW5zdGFsbDogdHJ1ZSxcclxuICAgICAgY3VzdG9tQ29sbGVjdGlvbnM6IHtcclxuICAgICAgICAnYXBwJzogRmlsZVN5c3RlbUljb25Mb2FkZXIoJy4vc3JjL2Fzc2V0cy9pY29ucycpXHJcbiAgICAgIH1cclxuICAgIH0pLFxyXG4gICAgQ29tcG9uZW50cyh7XHJcbiAgICAgIGR0czogdHJ1ZSxcclxuICAgICAgZGlyczogWydzcmMvY29tcG9uZW50cyddLFxyXG4gICAgICBkZWVwOiB0cnVlLFxyXG4gICAgICBpbmNsdWRlOiBbL1xcLnZ1ZSQvLCAvXFwudnVlXFw/dnVlLywgL1xcLnRzeD8kL11cclxuICAgIH0pXHJcbiAgXSxcclxuICByZXNvbHZlOiB7XHJcbiAgICBhbGlhczoge1xyXG4gICAgICAnQCc6IHBhdGgucmVzb2x2ZShfX2Rpcm5hbWUsICcuL3NyYycpXHJcbiAgICB9XHJcbiAgfSxcclxuICBzZXJ2ZXI6IHtcclxuICAgIHBvcnQ6IDMwMDAsXHJcbiAgICBob3N0OiB0cnVlLFxyXG4gICAgc3RyaWN0UG9ydDogdHJ1ZSxcclxuICAgIHdhdGNoOiB7XHJcbiAgICAgIHVzZVBvbGxpbmc6IHRydWVcclxuICAgIH1cclxuICB9LFxyXG4gIGJ1aWxkOiB7XHJcbiAgICBvdXREaXI6ICdkaXN0JyxcclxuICAgIGFzc2V0c0RpcjogJ2Fzc2V0cycsXHJcbiAgICBzb3VyY2VtYXA6IHRydWUsXHJcbiAgICBjaHVua1NpemVXYXJuaW5nTGltaXQ6IDEwMDBcclxuICB9LFxyXG4gIHRlc3Q6IHtcclxuICAgIGdsb2JhbHM6IHRydWUsXHJcbiAgICBlbnZpcm9ubWVudDogJ2pzZG9tJyxcclxuICAgIHNldHVwRmlsZXM6ICcuL3Rlc3RzL3NldHVwLnRzJyxcclxuICAgIGNvdmVyYWdlOiB7XHJcbiAgICAgIHByb3ZpZGVyOiAndjgnLFxyXG4gICAgICByZXBvcnRlcjogWyd0ZXh0JywgJ2pzb24nLCAnaHRtbCddLFxyXG4gICAgICBleGNsdWRlOiBbXHJcbiAgICAgICAgJyoqL25vZGVfbW9kdWxlcy8nLFxyXG4gICAgICAgICcqKi9kaXN0LycsXHJcbiAgICAgICAgJyoqL3Rlc3RzLycsXHJcbiAgICAgICAgJyoqLyouZC50cycsXHJcbiAgICAgICAgJyoqL3R5cGVzLnRzJyxcclxuICAgICAgICAnKiovbWFpbi50cycsXHJcbiAgICAgICAgJyoqL3ZpdGUtZW52LmQudHMnXHJcbiAgICAgIF1cclxuICAgIH1cclxuICB9XHJcbn0pOyJdLAogICJtYXBwaW5ncyI6ICI7QUFBZ1IsU0FBUyxvQkFBb0I7QUFDN1MsT0FBTyxTQUFTO0FBQ2hCLE9BQU8sVUFBVTtBQUNqQixPQUFPLFdBQVc7QUFDbEIsT0FBTyxnQkFBZ0I7QUFDdkIsU0FBUyw0QkFBNEI7QUFMckMsSUFBTSxtQ0FBbUM7QUFPekMsSUFBTyxzQkFBUSxhQUFhO0FBQUEsRUFDMUIsU0FBUztBQUFBLElBQ1AsSUFBSTtBQUFBLElBQ0osTUFBTTtBQUFBLE1BQ0osVUFBVTtBQUFBLE1BQ1YsYUFBYTtBQUFBLE1BQ2IsbUJBQW1CO0FBQUEsUUFDakIsT0FBTyxxQkFBcUIsb0JBQW9CO0FBQUEsTUFDbEQ7QUFBQSxJQUNGLENBQUM7QUFBQSxJQUNELFdBQVc7QUFBQSxNQUNULEtBQUs7QUFBQSxNQUNMLE1BQU0sQ0FBQyxnQkFBZ0I7QUFBQSxNQUN2QixNQUFNO0FBQUEsTUFDTixTQUFTLENBQUMsVUFBVSxjQUFjLFNBQVM7QUFBQSxJQUM3QyxDQUFDO0FBQUEsRUFDSDtBQUFBLEVBQ0EsU0FBUztBQUFBLElBQ1AsT0FBTztBQUFBLE1BQ0wsS0FBSyxLQUFLLFFBQVEsa0NBQVcsT0FBTztBQUFBLElBQ3RDO0FBQUEsRUFDRjtBQUFBLEVBQ0EsUUFBUTtBQUFBLElBQ04sTUFBTTtBQUFBLElBQ04sTUFBTTtBQUFBLElBQ04sWUFBWTtBQUFBLElBQ1osT0FBTztBQUFBLE1BQ0wsWUFBWTtBQUFBLElBQ2Q7QUFBQSxFQUNGO0FBQUEsRUFDQSxPQUFPO0FBQUEsSUFDTCxRQUFRO0FBQUEsSUFDUixXQUFXO0FBQUEsSUFDWCxXQUFXO0FBQUEsSUFDWCx1QkFBdUI7QUFBQSxFQUN6QjtBQUFBLEVBQ0EsTUFBTTtBQUFBLElBQ0osU0FBUztBQUFBLElBQ1QsYUFBYTtBQUFBLElBQ2IsWUFBWTtBQUFBLElBQ1osVUFBVTtBQUFBLE1BQ1IsVUFBVTtBQUFBLE1BQ1YsVUFBVSxDQUFDLFFBQVEsUUFBUSxNQUFNO0FBQUEsTUFDakMsU0FBUztBQUFBLFFBQ1A7QUFBQSxRQUNBO0FBQUEsUUFDQTtBQUFBLFFBQ0E7QUFBQSxRQUNBO0FBQUEsUUFDQTtBQUFBLFFBQ0E7QUFBQSxNQUNGO0FBQUEsSUFDRjtBQUFBLEVBQ0Y7QUFDRixDQUFDOyIsCiAgIm5hbWVzIjogW10KfQo=
