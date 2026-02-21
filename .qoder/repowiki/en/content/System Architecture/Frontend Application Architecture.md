# Frontend Application Architecture

<cite>
**Referenced Files in This Document**
- [package.json](file://src/frontend/package.json)
- [vite.config.ts](file://src/frontend/vite.config.ts)
- [main.ts](file://src/frontend/src/main.ts)
- [router/index.ts](file://src/frontend/src/router/index.ts)
- [stores/auth.ts](file://src/frontend/src/stores/auth.ts)
- [services/api.ts](file://src/frontend/src/services/api.ts)
- [services/signalr.ts](file://src/frontend/src/services/signalr.ts)
- [composables/useSignalRCharts.ts](file://src/frontend/src/composables/useSignalRCharts.ts)
- [package.json](file://src/ui/digital-twin-dashboard/package.json)
- [vite.config.ts](file://src/ui/digital-twin-dashboard/vite.config.ts)
- [main.ts](file://src/ui/digital-twin-dashboard/src/main.ts)
- [router/index.ts](file://src/ui/digital-twin-dashboard/src/router/index.ts)
- [stores/auth.store.ts](file://src/ui/digital-twin-dashboard/src/stores/auth.store.ts)
- [services/auth.service.ts](file://src/ui/digital-twin-dashboard/src/services/auth.service.ts)
- [services/signalr.service.ts](file://src/ui/digital-twin-dashboard/src/services/signalr.service.ts)
- [composables/useApiCrud.ts](file://src/ui/digital-twin-dashboard/src/composables/useApiCrud.ts)
</cite>

## Table of Contents
1. [Introduction](#introduction)
2. [Project Structure](#project-structure)
3. [Core Components](#core-components)
4. [Architecture Overview](#architecture-overview)
5. [Detailed Component Analysis](#detailed-component-analysis)
6. [Dependency Analysis](#dependency-analysis)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Conclusion](#conclusion)

## Introduction
This document describes the frontend application architecture for the Digital Twin Platform, focusing on two Vue.js implementations. It covers component-based architecture, state management with Pinia, routing configuration, real-time visualization with SignalR, API service layer, reusable component libraries, build system configuration, TypeScript integration, and styling with TailwindCSS. It also addresses performance optimization, accessibility, and cross-browser compatibility strategies.

## Project Structure
The repository contains two distinct frontend implementations:
- Minimal Vue 3 application under src/frontend with a focused set of features and a simple component hierarchy.
- Rich Vue 3 application under src/ui/digital-twin-dashboard with a comprehensive feature set, extensive reusable components, and advanced integrations.

Both applications share common patterns:
- Vue 3 with Composition API
- Pinia for state management
- Vue Router for navigation
- Vite as the build tool
- TypeScript for type safety
- TailwindCSS for styling
- SignalR for real-time updates

```mermaid
graph TB
subgraph "Frontend Implementation (src/frontend)"
FE_Pkg["package.json"]
FE_Vite["vite.config.ts"]
FE_Main["main.ts"]
FE_Router["router/index.ts"]
FE_AuthStore["stores/auth.ts"]
FE_Api["services/api.ts"]
FE_SignalR["services/signalr.ts"]
FE_Charts["composables/useSignalRCharts.ts"]
end
subgraph "Dashboard Implementation (src/ui/digital-twin-dashboard)"
DB_Pkg["package.json"]
DB_Vite["vite.config.ts"]
DB_Main["main.ts"]
DB_Router["router/index.ts"]
DB_AuthStore["stores/auth.store.ts"]
DB_AuthSvc["services/auth.service.ts"]
DB_SignalR["services/signalr.service.ts"]
DB_Crud["composables/useApiCrud.ts"]
end
FE_Pkg --> FE_Vite --> FE_Main --> FE_Router
FE_Main --> FE_AuthStore
FE_Main --> FE_Api
FE_Main --> FE_SignalR
FE_SignalR --> FE_Charts
DB_Pkg --> DB_Vite --> DB_Main --> DB_Router
DB_Main --> DB_AuthStore
DB_Main --> DB_AuthSvc
DB_Main --> DB_SignalR
DB_SignalR --> DB_Crud
```

**Diagram sources**
- [package.json](file://src/frontend/package.json#L1-L44)
- [vite.config.ts](file://src/frontend/vite.config.ts#L1-L48)
- [main.ts](file://src/frontend/src/main.ts#L1-L35)
- [router/index.ts](file://src/frontend/src/router/index.ts#L1-L112)
- [stores/auth.ts](file://src/frontend/src/stores/auth.ts#L1-L508)
- [services/api.ts](file://src/frontend/src/services/api.ts#L1-L96)
- [services/signalr.ts](file://src/frontend/src/services/signalr.ts#L1-L271)
- [composables/useSignalRCharts.ts](file://src/frontend/src/composables/useSignalRCharts.ts#L1-L352)
- [package.json](file://src/ui/digital-twin-dashboard/package.json#L1-L87)
- [vite.config.ts](file://src/ui/digital-twin-dashboard/vite.config.ts#L1-L62)
- [main.ts](file://src/ui/digital-twin-dashboard/src/main.ts#L1-L17)
- [router/index.ts](file://src/ui/digital-twin-dashboard/src/router/index.ts#L1-L339)
- [stores/auth.store.ts](file://src/ui/digital-twin-dashboard/src/stores/auth.store.ts#L1-L136)
- [services/auth.service.ts](file://src/ui/digital-twin-dashboard/src/services/auth.service.ts#L1-L204)
- [services/signalr.service.ts](file://src/ui/digital-twin-dashboard/src/services/signalr.service.ts#L1-L62)
- [composables/useApiCrud.ts](file://src/ui/digital-twin-dashboard/src/composables/useApiCrud.ts#L1-L154)

**Section sources**
- [package.json](file://src/frontend/package.json#L1-L44)
- [vite.config.ts](file://src/frontend/vite.config.ts#L1-L48)
- [main.ts](file://src/frontend/src/main.ts#L1-L35)
- [router/index.ts](file://src/frontend/src/router/index.ts#L1-L112)
- [stores/auth.ts](file://src/frontend/src/stores/auth.ts#L1-L508)
- [services/api.ts](file://src/frontend/src/services/api.ts#L1-L96)
- [services/signalr.ts](file://src/frontend/src/services/signalr.ts#L1-L271)
- [composables/useSignalRCharts.ts](file://src/frontend/src/composables/useSignalRCharts.ts#L1-L352)
- [package.json](file://src/ui/digital-twin-dashboard/package.json#L1-L87)
- [vite.config.ts](file://src/ui/digital-twin-dashboard/vite.config.ts#L1-L62)
- [main.ts](file://src/ui/digital-twin-dashboard/src/main.ts#L1-L17)
- [router/index.ts](file://src/ui/digital-twin-dashboard/src/router/index.ts#L1-L339)
- [stores/auth.store.ts](file://src/ui/digital-twin-dashboard/src/stores/auth.store.ts#L1-L136)
- [services/auth.service.ts](file://src/ui/digital-twin-dashboard/src/services/auth.service.ts#L1-L204)
- [services/signalr.service.ts](file://src/ui/digital-twin-dashboard/src/services/signalr.service.ts#L1-L62)
- [composables/useApiCrud.ts](file://src/ui/digital-twin-dashboard/src/composables/useApiCrud.ts#L1-L154)

## Core Components
- Component-based architecture: Both applications use Vue 3 Single File Components organized by feature areas (views, components, services, stores, composables).
- State management with Pinia: Stores encapsulate domain-specific state and actions, with persistence and lifecycle management.
- Routing configuration: Both apps define route records with meta fields for authentication and titles, and global navigation guards.
- Real-time visualization: SignalR integration streams telemetry, alerts, and predictions to reactive stores and composables.
- API service layer: Centralized Axios clients with interceptors for authentication and token refresh.
- Reusable component library: Shared components and composables promote consistency and reduce duplication.
- Build system: Vite configuration with aliases, chunk splitting, and plugin ecosystems for Vue, TypeScript, and icons.
- Styling: TailwindCSS for utility-first styling with PostCSS and autoprefixing.

**Section sources**
- [main.ts](file://src/frontend/src/main.ts#L1-L35)
- [router/index.ts](file://src/frontend/src/router/index.ts#L1-L112)
- [stores/auth.ts](file://src/frontend/src/stores/auth.ts#L1-L508)
- [services/api.ts](file://src/frontend/src/services/api.ts#L1-L96)
- [services/signalr.ts](file://src/frontend/src/services/signalr.ts#L1-L271)
- [composables/useSignalRCharts.ts](file://src/frontend/src/composables/useSignalRCharts.ts#L1-L352)
- [main.ts](file://src/ui/digital-twin-dashboard/src/main.ts#L1-L17)
- [router/index.ts](file://src/ui/digital-twin-dashboard/src/router/index.ts#L1-L339)
- [stores/auth.store.ts](file://src/ui/digital-twin-dashboard/src/stores/auth.store.ts#L1-L136)
- [services/auth.service.ts](file://src/ui/digital-twin-dashboard/src/services/auth.service.ts#L1-L204)
- [services/signalr.service.ts](file://src/ui/digital-twin-dashboard/src/services/signalr.service.ts#L1-L62)
- [composables/useApiCrud.ts](file://src/ui/digital-twin-dashboard/src/composables/useApiCrud.ts#L1-L154)

## Architecture Overview
The frontend architecture follows a layered pattern:
- Presentation Layer: Vue components and views
- Composables Layer: Reusable logic for SignalR, API, and UI concerns
- Services Layer: Axios-based API clients and SignalR hubs
- State Management: Pinia stores for domain state
- Routing: Vue Router with navigation guards and meta-driven configuration

```mermaid
graph TB
subgraph "Presentation Layer"
Views["Views (Dashboard, Machines, Predictions, Alerts)"]
Components["Reusable Components"]
end
subgraph "Composables Layer"
UseSignalR["useSignalRCharts"]
UseApiCrud["useApiCrud"]
end
subgraph "Services Layer"
ApiClient["Axios Client (api.ts / axiosClient)"]
SignalR["SignalR Service"]
end
subgraph "State Management"
AuthStore["Auth Store (Pinia)"]
TelemetryStore["Telemetry Store (Pinia)"]
AlertsStore["Alerts Store (Pinia)"]
PredictionsStore["Predictions Store (Pinia)"]
end
subgraph "Routing"
Router["Vue Router"]
end
Views --> Components
Components --> UseSignalR
Components --> UseApiCrud
UseSignalR --> SignalR
UseSignalR --> TelemetryStore
UseSignalR --> AlertsStore
UseSignalR --> PredictionsStore
UseApiCrud --> ApiClient
Router --> AuthStore
Router --> Views
AuthStore --> ApiClient
```

**Diagram sources**
- [router/index.ts](file://src/frontend/src/router/index.ts#L1-L112)
- [stores/auth.ts](file://src/frontend/src/stores/auth.ts#L1-L508)
- [services/api.ts](file://src/frontend/src/services/api.ts#L1-L96)
- [services/signalr.ts](file://src/frontend/src/services/signalr.ts#L1-L271)
- [composables/useSignalRCharts.ts](file://src/frontend/src/composables/useSignalRCharts.ts#L1-L352)
- [router/index.ts](file://src/ui/digital-twin-dashboard/src/router/index.ts#L1-L339)
- [stores/auth.store.ts](file://src/ui/digital-twin-dashboard/src/stores/auth.store.ts#L1-L136)
- [services/auth.service.ts](file://src/ui/digital-twin-dashboard/src/services/auth.service.ts#L1-L204)
- [services/signalr.service.ts](file://src/ui/digital-twin-dashboard/src/services/signalr.service.ts#L1-L62)
- [composables/useApiCrud.ts](file://src/ui/digital-twin-dashboard/src/composables/useApiCrud.ts#L1-L154)

## Detailed Component Analysis

### Authentication and Session Management
Both implementations provide robust authentication flows:
- Frontend app: Centralized auth store with token persistence, session monitoring, and automatic refresh.
- Dashboard app: Secure token manager using sessionStorage and role-based access checks.

```mermaid
sequenceDiagram
participant User as "User"
participant View as "Login View"
participant AuthService as "Auth Service"
participant Api as "Axios Client"
participant Store as "Auth Store"
User->>View : Submit credentials
View->>AuthService : login(request)
AuthService->>Api : POST /Auth/login
Api-->>AuthService : {accessToken, refreshToken, user}
AuthService-->>Store : setToken(...)
Store-->>View : isAuthenticated = true
View-->>User : Navigate to Dashboard
```

**Diagram sources**
- [stores/auth.ts](file://src/frontend/src/stores/auth.ts#L105-L153)
- [services/api.ts](file://src/frontend/src/services/api.ts#L74-L93)
- [stores/auth.store.ts](file://src/ui/digital-twin-dashboard/src/stores/auth.store.ts#L74-L97)
- [services/auth.service.ts](file://src/ui/digital-twin-dashboard/src/services/auth.service.ts#L98-L101)

**Section sources**
- [stores/auth.ts](file://src/frontend/src/stores/auth.ts#L1-L508)
- [services/api.ts](file://src/frontend/src/services/api.ts#L1-L96)
- [stores/auth.store.ts](file://src/ui/digital-twin-dashboard/src/stores/auth.store.ts#L1-L136)
- [services/auth.service.ts](file://src/ui/digital-twin-dashboard/src/services/auth.service.ts#L1-L204)

### Real-Time Visualization with SignalR
The SignalR integration provides live telemetry, alerts, and predictions:
- Connection lifecycle with automatic reconnection and error handling
- Event handlers mapping backend payloads to frontend models
- Composable for buffering and batching updates to maintain performance

```mermaid
sequenceDiagram
participant FE as "Frontend App"
participant SR as "SignalR Service"
participant Hub as "Telemetry Hub"
participant TS as "Telemetry Store"
participant PS as "Predictions Store"
participant AS as "Alerts Store"
FE->>SR : connect()
SR->>Hub : start()
Hub-->>SR : connected
Hub-->>SR : TelemetryUpdate(data)
SR->>TS : setRealtimeData(machineId, mapped)
Hub-->>SR : NewAlert(data)
SR->>AS : addAlert(mapped)
Hub-->>SR : PredictionUpdate(data)
SR->>PS : addPrediction(mapped)
```

**Diagram sources**
- [services/signalr.ts](file://src/frontend/src/services/signalr.ts#L55-L110)
- [services/signalr.ts](file://src/frontend/src/services/signalr.ts#L148-L204)
- [composables/useSignalRCharts.ts](file://src/frontend/src/composables/useSignalRCharts.ts#L136-L164)

**Section sources**
- [services/signalr.ts](file://src/frontend/src/services/signalr.ts#L1-L271)
- [composables/useSignalRCharts.ts](file://src/frontend/src/composables/useSignalRCharts.ts#L1-L352)

### API Service Layer
The API layer abstracts HTTP communication:
- Axios client with base URL and timeout configuration
- Request interceptor adding Authorization headers
- Response interceptor handling 401 and token refresh
- Typed service functions for each domain endpoint

```mermaid
flowchart TD
Start(["HTTP Request"]) --> ReqInt["Request Interceptor<br/>Add Authorization"]
ReqInt --> Send["Send to Backend"]
Send --> Resp{"Response"}
Resp --> |2xx| Pass["Pass Through"]
Resp --> |401 & No Retry| Refresh["POST /Auth/refresh"]
Refresh --> Ok{"Success?"}
Ok --> |Yes| Update["Update Tokens in Storage"]
Update --> Retry["Retry Original Request"]
Ok --> |No| Logout["Redirect to Login"]
Pass --> End(["Return Data"])
Retry --> End
Logout --> End
```

**Diagram sources**
- [services/api.ts](file://src/frontend/src/services/api.ts#L15-L71)

**Section sources**
- [services/api.ts](file://src/frontend/src/services/api.ts#L1-L96)

### Routing Configuration
Both applications implement route-based navigation with authentication guards:
- Meta fields define requiresAuth and titles
- beforeEach guard checks authentication state and redirects accordingly
- Dynamic imports prevent circular dependencies

```mermaid
flowchart TD
Enter(["Route Change"]) --> CheckAuth{"requiresAuth?"}
CheckAuth --> |No| Allow["Allow Navigation"]
CheckAuth --> |Yes| IsAuth{"isAuthenticated?"}
IsAuth --> |Yes| Allow
IsAuth --> |No| CheckStore["checkAuth()"]
CheckStore --> AuthOk{"Authenticated?"}
AuthOk --> |Yes| Allow
AuthOk --> |No| Redirect["Redirect to Login"]
Allow --> SetTitle["Set Document Title"]
Redirect --> End(["End"])
SetTitle --> End
```

**Diagram sources**
- [router/index.ts](file://src/frontend/src/router/index.ts#L85-L109)
- [router/index.ts](file://src/ui/digital-twin-dashboard/src/router/index.ts#L297-L337)

**Section sources**
- [router/index.ts](file://src/frontend/src/router/index.ts#L1-L112)
- [router/index.ts](file://src/ui/digital-twin-dashboard/src/router/index.ts#L1-L339)

### Build System and Tooling
Build configurations differ slightly between implementations:
- Aliases for cleaner imports (@, @components, @services, @stores, @types, @utils, @views)
- Proxy configuration for API and SignalR WebSocket traffic
- Chunk splitting for vendor, charts, and utilities
- Dashboard app includes icon and component auto-registration plugins

```mermaid
graph LR
Vite["Vite Config"] --> Alias["@ Aliases"]
Vite --> Proxy["Proxy /api -> /hub"]
Vite --> Split["manualChunks"]
Vite --> Plugins["Plugins (Vue, Icons, Components)"]
```

**Diagram sources**
- [vite.config.ts](file://src/frontend/vite.config.ts#L5-L47)
- [vite.config.ts](file://src/ui/digital-twin-dashboard/vite.config.ts#L8-L24)

**Section sources**
- [vite.config.ts](file://src/frontend/vite.config.ts#L1-L48)
- [vite.config.ts](file://src/ui/digital-twin-dashboard/vite.config.ts#L1-L62)

### TypeScript Integration and Styling
- TypeScript is configured across both projects with appropriate tsconfig files and Vue-specific setups.
- TailwindCSS is integrated with PostCSS and autoprefixer for modern CSS processing.
- The dashboard app leverages additional tooling for testing and linting.

**Section sources**
- [package.json](file://src/frontend/package.json#L29-L40)
- [package.json](file://src/ui/digital-twin-dashboard/package.json#L41-L84)

## Dependency Analysis
The applications demonstrate clear separation of concerns with minimal coupling:
- Frontend app: lean dependencies (Vue, Router, Pinia, ApexCharts, SignalR, TailwindCSS)
- Dashboard app: richer ecosystem (Heroicons, Lucide, ECharts, Three.js, Vue Sonner)

```mermaid
graph TB
subgraph "Frontend Dependencies"
FV["vue@^3.4.21"]
FR["vue-router@^4.3.0"]
FP["pinia@^2.1.7"]
FA["axios@^1.6.8"]
FS["@microsoft/signalr-client@^1.0.0"]
FAC["vue3-apexcharts@^1.5.2"]
FT["tailwindcss@^3.4.1"]
end
subgraph "Dashboard Dependencies"
DV["@vueuse/core@^10.9.0"]
DE["@microsoft/signalr@10.0.0"]
DCH["echarts@^5.5.0"]
DTH["three@^0.159.0"]
DSV["vue-echarts@^7.0.3"]
DSN["vue-sonner@^2.0.9"]
DHI["@heroicons/vue@^2.1.5"]
DLU["lucide-vue-next@^0.338.0"]
end
```

**Diagram sources**
- [package.json](file://src/frontend/package.json#L14-L28)
- [package.json](file://src/ui/digital-twin-dashboard/package.json#L24-L40)

**Section sources**
- [package.json](file://src/frontend/package.json#L1-L44)
- [package.json](file://src/ui/digital-twin-dashboard/package.json#L1-L87)

## Performance Considerations
- SignalR buffering: The composable buffers updates and processes in batches to limit DOM updates and maintain smooth chart rendering.
- Chunk splitting: Vite manualChunks separate vendor libraries, charting libraries, and utilities to optimize caching and initial load.
- Lazy loading: Route components are dynamically imported to reduce initial bundle size.
- Reactive stores: Efficient updates through Pinia stores minimize unnecessary re-renders.
- Icon and component auto-registration: Reduces manual imports and improves tree-shaking.

**Section sources**
- [composables/useSignalRCharts.ts](file://src/frontend/src/composables/useSignalRCharts.ts#L12-L17)
- [vite.config.ts](file://src/frontend/vite.config.ts#L34-L46)
- [router/index.ts](file://src/frontend/src/router/index.ts#L10-L72)

## Troubleshooting Guide
Common issues and resolutions:
- Authentication failures: Verify token storage and refresh logic; ensure interceptors are applied.
- SignalR connection errors: Check proxy configuration for WebSocket traffic and CORS settings on the backend.
- Navigation guard loops: Confirm auth store initialization and router guard conditions.
- Build errors: Validate Vite aliases and ensure TypeScript types match runtime behavior.

**Section sources**
- [services/api.ts](file://src/frontend/src/services/api.ts#L35-L71)
- [services/signalr.ts](file://src/frontend/src/services/signalr.ts#L62-L110)
- [router/index.ts](file://src/frontend/src/router/index.ts#L85-L109)

## Conclusion
The frontend architecture combines a pragmatic minimal implementation with a feature-rich dashboard, unified by strong patterns in component composition, state management, routing, and real-time data streaming. The SignalR integration, centralized API layer, and reusable composables provide a solid foundation for scalable enhancements while maintaining performance and developer productivity.