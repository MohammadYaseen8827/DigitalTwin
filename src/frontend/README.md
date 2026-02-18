# Digital Twin Platform - Frontend

Vue.js 3 frontend for the Digital Twin Platform for Predictive Maintenance.

## Technology Stack

- **Vue.js 3** - Progressive JavaScript framework with Composition API
- **TypeScript** - Type-safe JavaScript
- **Pinia** - State management
- **Vue Router** - Client-side routing with lazy loading
- **ApexCharts** - Interactive charts for RUL, temperature, vibration data
- **SignalR** - Real-time communication for live telemetry
- **Tailwind CSS** - Utility-first CSS framework
- **Axios** - HTTP client with interceptors

## Project Structure

```
frontend/
├── src/
│   ├── api/              # API client services
│   ├── assets/           # Static assets
│   │   └── css/         # Global styles with Tailwind
│   ├── components/       # Reusable Vue components
│   │   ├── common/      # Buttons, inputs, cards
│   │   ├── charts/      # Chart components
│   │   ├── dashboard/   # Dashboard components
│   │   └── alerts/      # Alert components
│   ├── composables/      # Vue composables
│   ├── layouts/         # Page layouts
│   ├── router/          # Vue Router configuration
│   ├── stores/          # Pinia stores
│   ├── types/           # TypeScript types
│   ├── views/           # Page views
│   ├── services/        # External services
│   ├── utils/           # Utility functions
│   ├── App.vue
│   └── main.ts
├── index.html
├── package.json
├── tsconfig.json
├── vite.config.ts
└── tailwind.config.js
```

## Features

- Real-time monitoring via SignalR
- Predictive analytics visualization
- Alert management system
- Responsive design with Tailwind CSS
- JWT authentication with token refresh

## Getting Started

```bash
cd frontend
npm install
npm run dev
```

## License

MIT
