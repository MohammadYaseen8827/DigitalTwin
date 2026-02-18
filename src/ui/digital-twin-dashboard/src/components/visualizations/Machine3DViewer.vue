<template>
  <div class="machine-3d-viewer">
    <h2>3D Machine Visualization</h2>

    <BaseCard :loading="loading" variant="glass" class="viewer-card">
      <template #loading>
        <div class="viewer-skeleton">
          <BaseSkeleton height="320px" radius="var(--radius-xl)" />
          <div class="controls-skeleton">
            <BaseSkeleton width="60%" height="38px" radius="var(--radius-md)" />
            <BaseSkeleton width="220px" height="38px" radius="var(--radius-full)" />
          </div>
        </div>
      </template>

      <template v-if="!loading">
        <div class="viewer-container">
          <div ref="canvasContainer" class="canvas-container"></div>

          <div class="controls">
            <div class="machine-selector">
              <label>Select Machine:</label>
              <select v-model="selectedMachineId" @change="updateMachineModel">
                <option value="">Select a machine</option>
                <option v-for="machine in machines" :key="machine.id" :value="machine.id">
                  {{ machine.name }} ({{ machine.type }})
                </option>
              </select>
            </div>

            <div class="view-controls">
              <button @click="rotateLeft" class="control-btn">↺</button>
              <button @click="rotateRight" class="control-btn">↻</button>
              <button @click="zoomIn" class="control-btn">+</button>
              <button @click="zoomOut" class="control-btn">-</button>
              <button @click="resetView" class="control-btn">↺</button>
            </div>
          </div>
        </div>
      </template>
    </BaseCard>

    <div v-if="!loading && selectedMachine" class="machine-details">
      <h3>{{ selectedMachine.name }} Details</h3>
      <div class="details-grid">
        <div class="detail-item">
          <span class="label">Status:</span>
          <span class="value" :class="statusClass">{{ selectedMachine.status }}</span>
        </div>
        <div class="detail-item">
          <span class="label">Type:</span>
          <span class="value">{{ selectedMachine.type }}</span>
        </div>
        <div class="detail-item">
          <span class="label">Model:</span>
          <span class="value">{{ selectedMachine.properties?.model || 'N/A' }}</span>
        </div>
        <div class="detail-item">
          <span class="label">Manufacturer:</span>
          <span class="value">{{ selectedMachine.properties?.manufacturer || 'N/A' }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, computed, watch } from 'vue'
import { useToast } from '@/composables/useToast'
import * as THREE from 'three'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js'
import { fetchMachines } from '@/services/machines.service'
import type { MachineDto } from '@/api/types'
import BaseCard from '../base/BaseCard.vue'
import BaseSkeleton from '../base/BaseSkeleton.vue'

const props = defineProps<{
  machines?: MachineDto[]
  selectedMachineId?: string | null
}>()

const emit = defineEmits<{
  (e: 'machine-selected', machineId: string): void
  (e: 'machine-focused', machine: MachineDto): void
}>()

// Reactive references
const localMachines = ref<MachineDto[]>([])
const computedMachines = computed(() => (props.machines?.length ? props.machines : localMachines.value))
const selectedMachineId = ref(props.selectedMachineId ?? '')
const selectedMachine = computed<MachineDto | null>(() => {
  return computedMachines.value.find(m => m.id === selectedMachineId.value) ?? null
})

const loading = ref(false)

const toast = useToast()

// Three.js references
const canvasContainer = ref<HTMLDivElement | null>(null)
let scene: THREE.Scene | null = null
let camera: THREE.PerspectiveCamera | null = null
let renderer: THREE.WebGLRenderer | null = null
let controls: OrbitControls | null = null
let machineModel: THREE.Object3D | null = null
let animationFrameId: number | null = null
let activeMachineType = ''

const disposeObject3D = (object: THREE.Object3D) => {
  object.traverse(child => {
    const mesh = child as THREE.Mesh
    if (mesh.isMesh) {
      mesh.geometry?.dispose?.()
      if (Array.isArray(mesh.material)) {
        mesh.material.forEach(material => material.dispose?.())
      } else {
        mesh.material?.dispose?.()
      }
    }
  })
}

// Status class computation
const statusClass = computed(() => {
  if (!selectedMachine.value) return ''
  return `status-${String(selectedMachine.value.status).toLowerCase()}`
})

const formatStatus = (status: MachineDto['status']) =>
  typeof status === 'number' ? status.toString() : status

const getMachineProperty = (machine: MachineDto, key: string) => {
  const value = machine.properties?.[key]
  return typeof value === 'string' ? value : 'N/A'
}

// Fetch machines
const loadMachines = async () => {
  try {
    loading.value = true
    localMachines.value = await fetchMachines()
    if (!selectedMachineId.value && localMachines.value.length) {
      selectedMachineId.value = localMachines.value[0]!.id
    }
  } catch (error) {
    console.error('Error fetching machines:', error)
    toast.error('Unable to load machines for the 3D viewer right now.')
  } finally {
    loading.value = false
  }
}

// Initialize Three.js scene
const initScene = () => {
  if (!canvasContainer.value) return

  // Create scene
  scene = new THREE.Scene()
  scene.background = new THREE.Color(0x0a0a2a)
  scene.fog = new THREE.Fog(0x0a0a2a, 10, 20)

  // Create camera
  camera = new THREE.PerspectiveCamera(
    75,
    canvasContainer.value.clientWidth / canvasContainer.value.clientHeight,
    0.1,
    1000
  )
  camera.position.z = 5

  // Create renderer
  renderer = new THREE.WebGLRenderer({ antialias: true })
  renderer.setSize(
    canvasContainer.value.clientWidth,
    canvasContainer.value.clientHeight
  )
  renderer.setPixelRatio(window.devicePixelRatio)
  canvasContainer.value.appendChild(renderer.domElement)

  // Add lighting
  const ambientLight = new THREE.AmbientLight(0x404040, 2)
  scene.add(ambientLight)

  const directionalLight = new THREE.DirectionalLight(0xffffff, 1)
  directionalLight.position.set(1, 1, 1)
  scene.add(directionalLight)

  const pointLight = new THREE.PointLight(0x4dabf7, 1, 100)
  pointLight.position.set(5, 5, 5)
  scene.add(pointLight)

  // Add orbit controls
  controls = new OrbitControls(camera, renderer.domElement)
  controls.enableDamping = true
  controls.dampingFactor = 0.05

  // Create a simple machine model (cylinder for CNC machine)
  createMachineModel('CNC')

  // Start animation loop
  animate()

  // Handle window resize
  window.addEventListener('resize', onWindowResize)
}

// Create machine model based on type
const createMachineModel = (type: string) => {
  if (!scene || (type === activeMachineType && machineModel)) {
    return
  }

  // Remove existing model
  if (machineModel) {
    scene.remove(machineModel)
    disposeObject3D(machineModel)
  }

  // Create new model based on machine type
  switch (type) {
    case 'CNC':
      machineModel = createCNCMachine()
      break
    case 'Robot':
      machineModel = createRobotMachine()
      break
    case 'Welder':
      machineModel = createWelderMachine()
      break
    case 'Inspection':
      machineModel = createInspectionMachine()
      break
    case 'Press':
      machineModel = createPressMachine()
      break
    case 'SMT':
      machineModel = createSMTMachine()
      break
    case 'Oven':
      machineModel = createOvenMachine()
      break
    case 'Soldering':
      machineModel = createSolderingMachine()
      break
    case 'Extruder':
      machineModel = createExtruderMachine()
      break
    case 'Granulator':
      machineModel = createGranulatorMachine()
      break
    case 'Dryer':
      machineModel = createDryerMachine()
      break
    case 'Injection Molder':
      machineModel = createInjectionMolderMachine()
      break
    case '3D Printer':
      machineModel = createPrinterMachine()
      break
    case 'Laser Cutter':
      machineModel = createLaserCutterMachine()
      break
    case 'Assembly Station':
      machineModel = createAssemblyStationMachine()
      break
    default:
      machineModel = createGenericMachine()
  }

  scene.add(machineModel)
  activeMachineType = type
}

// Create CNC machine model
const createCNCMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(3, 0.5, 2)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Column
  const columnGeometry = new THREE.BoxGeometry(0.3, 3, 0.3)
  const columnMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const column = new THREE.Mesh(columnGeometry, columnMaterial)
  column.position.y = 0.5
  group.add(column)

  // Spindle
  const spindleGeometry = new THREE.CylinderGeometry(0.2, 0.2, 1, 32)
  const spindleMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const spindle = new THREE.Mesh(spindleGeometry, spindleMaterial)
  spindle.rotation.x = Math.PI / 2
  spindle.position.y = 1.5
  spindle.position.z = 0.5
  group.add(spindle)

  // Worktable
  const tableGeometry = new THREE.BoxGeometry(1.5, 0.1, 1.5)
  const tableMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x69db7c,
    shininess: 100
  })
  const table = new THREE.Mesh(tableGeometry, tableMaterial)
  table.position.y = -0.2
  table.position.z = -0.2
  group.add(table)

  return group
}

// Create Robot machine model
const createRobotMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.CylinderGeometry(0.8, 1, 0.3, 32)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Arm 1
  const arm1Geometry = new THREE.BoxGeometry(0.2, 1.5, 0.2)
  const arm1Material = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const arm1 = new THREE.Mesh(arm1Geometry, arm1Material)
  arm1.position.y = -0.2
  group.add(arm1)

  // Arm 2
  const arm2Geometry = new THREE.BoxGeometry(0.2, 1, 0.2)
  const arm2Material = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const arm2 = new THREE.Mesh(arm2Geometry, arm2Material)
  arm2.position.y = 0.8
  arm2.position.x = 0.5
  arm2.rotation.z = Math.PI / 4
  group.add(arm2)

  // Gripper
  const gripperGeometry = new THREE.BoxGeometry(0.3, 0.1, 0.1)
  const gripperMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x69db7c,
    shininess: 100
  })
  const gripper = new THREE.Mesh(gripperGeometry, gripperMaterial)
  gripper.position.y = 1.3
  gripper.position.x = 0.8
  group.add(gripper)

  return group
}

// Create Welder machine model
const createWelderMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(2, 0.3, 1)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Welding arm
  const armGeometry = new THREE.BoxGeometry(0.2, 2, 0.2)
  const armMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const arm = new THREE.Mesh(armGeometry, armMaterial)
  arm.position.y = 0
  group.add(arm)

  // Welding torch
  const torchGeometry = new THREE.CylinderGeometry(0.05, 0.1, 0.5, 32)
  const torchMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xff6b6b,
    shininess: 100
  })
  const torch = new THREE.Mesh(torchGeometry, torchMaterial)
  torch.rotation.x = Math.PI / 2
  torch.position.y = 1
  torch.position.z = 0.3
  group.add(torch)

  return group
}

// Create Inspection machine model
const createInspectionMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(1.5, 0.2, 1.5)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Camera stand
  const standGeometry = new THREE.BoxGeometry(0.1, 1, 0.1)
  const standMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const stand = new THREE.Mesh(standGeometry, standMaterial)
  stand.position.y = -0.5
  group.add(stand)

  // Camera
  const cameraGeometry = new THREE.SphereGeometry(0.2, 32, 32)
  const cameraMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const cameraHead = new THREE.Mesh(cameraGeometry, cameraMaterial)
  cameraHead.position.y = 0
  group.add(cameraHead)

  return group
}

// Create Press machine model
const createPressMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(3, 0.5, 2)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Hydraulic cylinder
  const cylinderGeometry = new THREE.CylinderGeometry(0.3, 0.3, 2, 32)
  const cylinderMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const cylinder = new THREE.Mesh(cylinderGeometry, cylinderMaterial)
  cylinder.position.y = 0.5
  group.add(cylinder)

  // Press head
  const headGeometry = new THREE.BoxGeometry(1.5, 0.2, 1.5)
  const headMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const head = new THREE.Mesh(headGeometry, headMaterial)
  head.position.y = 1.5
  group.add(head)

  return group
}

// Create SMT machine model
const createSMTMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(2.5, 0.3, 1.5)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Conveyor
  const conveyorGeometry = new THREE.BoxGeometry(2, 0.1, 0.8)
  const conveyorMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const conveyor = new THREE.Mesh(conveyorGeometry, conveyorMaterial)
  conveyor.position.y = -0.8
  group.add(conveyor)

  // Pick and place head
  const headGeometry = new THREE.BoxGeometry(0.3, 0.5, 0.3)
  const headMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const head = new THREE.Mesh(headGeometry, headMaterial)
  head.position.y = -0.5
  group.add(head)

  return group
}

// Create Oven machine model
const createOvenMachine = () => {
  const group = new THREE.Group()

  // Main body
  const bodyGeometry = new THREE.BoxGeometry(3, 2, 2)
  const bodyMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const body = new THREE.Mesh(bodyGeometry, bodyMaterial)
  body.position.y = -0.5
  group.add(body)

  // Door
  const doorGeometry = new THREE.BoxGeometry(0.1, 1.8, 1.8)
  const doorMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const door = new THREE.Mesh(doorGeometry, doorMaterial)
  door.position.x = 1.55
  door.position.y = -0.5
  group.add(door)

  // Heating elements (glow)
  const elementsGeometry = new THREE.BoxGeometry(0.1, 0.2, 1.6)
  const elementsMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xff6b6b,
    emissive: 0xff6b6b,
    emissiveIntensity: 0.5,
    shininess: 100
  })
  const elements = new THREE.Mesh(elementsGeometry, elementsMaterial)
  elements.position.x = 1.4
  elements.position.y = -0.5
  group.add(elements)

  return group
}

// Create Soldering machine model
const createSolderingMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(2, 0.3, 1)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Soldering station
  const stationGeometry = new THREE.BoxGeometry(0.5, 0.8, 0.5)
  const stationMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const station = new THREE.Mesh(stationGeometry, stationMaterial)
  station.position.y = -0.6
  group.add(station)

  // Soldering iron
  const ironGeometry = new THREE.CylinderGeometry(0.02, 0.05, 0.8, 32)
  const ironMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const iron = new THREE.Mesh(ironGeometry, ironMaterial)
  iron.rotation.z = Math.PI / 2
  iron.position.x = 0.3
  iron.position.y = -0.3
  group.add(iron)

  return group
}

// Create Extruder machine model
const createExtruderMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(3, 0.5, 1.5)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Hopper
  const hopperGeometry = new THREE.CylinderGeometry(0.5, 0.8, 1, 32)
  const hopperMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const hopper = new THREE.Mesh(hopperGeometry, hopperMaterial)
  hopper.position.y = -0.2
  hopper.position.x = -1
  group.add(hopper)

  // Barrel
  const barrelGeometry = new THREE.CylinderGeometry(0.3, 0.3, 1.5, 32)
  const barrelMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const barrel = new THREE.Mesh(barrelGeometry, barrelMaterial)
  barrel.rotation.z = Math.PI / 2
  barrel.position.x = 0.5
  group.add(barrel)

  return group
}

// Create Granulator machine model
const createGranulatorMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(2.5, 0.3, 1.5)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Cutting chamber
  const chamberGeometry = new THREE.CylinderGeometry(0.6, 0.6, 1, 32)
  const chamberMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const chamber = new THREE.Mesh(chamberGeometry, chamberMaterial)
  chamber.position.y = -0.5
  group.add(chamber)

  // Motor
  const motorGeometry = new THREE.CylinderGeometry(0.2, 0.2, 0.5, 32)
  const motorMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const motor = new THREE.Mesh(motorGeometry, motorMaterial)
  motor.position.x = -0.8
  motor.position.y = -0.7
  group.add(motor)

  return group
}

// Create Dryer machine model
const createDryerMachine = () => {
  const group = new THREE.Group()

  // Main body
  const bodyGeometry = new THREE.CylinderGeometry(0.8, 0.8, 2, 32)
  const bodyMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const body = new THREE.Mesh(bodyGeometry, bodyMaterial)
  body.position.y = -0.5
  group.add(body)

  // Air intake
  const intakeGeometry = new THREE.CylinderGeometry(0.1, 0.1, 0.3, 32)
  const intakeMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const intake = new THREE.Mesh(intakeGeometry, intakeMaterial)
  intake.position.x = -0.8
  intake.position.y = 0
  group.add(intake)

  // Air outlet
  const outletGeometry = new THREE.CylinderGeometry(0.1, 0.1, 0.3, 32)
  const outletMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const outlet = new THREE.Mesh(outletGeometry, outletMaterial)
  outlet.position.x = 0.8
  outlet.position.y = 0
  group.add(outlet)

  return group
}

// Create Injection Molder machine model
const createInjectionMolderMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(3, 0.5, 2)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Injection unit
  const unitGeometry = new THREE.BoxGeometry(1, 1.5, 1)
  const unitMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const unit = new THREE.Mesh(unitGeometry, unitMaterial)
  unit.position.x = -1
  unit.position.y = -0.25
  group.add(unit)

  // Clamping unit
  const clampGeometry = new THREE.BoxGeometry(1, 1.5, 1)
  const clampMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const clamp = new THREE.Mesh(clampGeometry, clampMaterial)
  clamp.position.x = 1
  clamp.position.y = -0.25
  group.add(clamp)

  return group
}

// Create 3D Printer machine model
const createPrinterMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(2.5, 0.3, 2)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Print bed
  const bedGeometry = new THREE.BoxGeometry(1.8, 0.1, 1.8)
  const bedMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const bed = new THREE.Mesh(bedGeometry, bedMaterial)
  bed.position.y = -0.8
  group.add(bed)

  // Print head
  const headGeometry = new THREE.BoxGeometry(0.2, 0.3, 0.2)
  const headMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const head = new THREE.Mesh(headGeometry, headMaterial)
  head.position.y = -0.6
  group.add(head)

  return group
}

// Create Laser Cutter machine model
const createLaserCutterMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(3, 0.3, 2)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Cutting bed
  const bedGeometry = new THREE.BoxGeometry(2.5, 0.1, 1.5)
  const bedMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const bed = new THREE.Mesh(bedGeometry, bedMaterial)
  bed.position.y = -0.8
  group.add(bed)

  // Laser head
  const headGeometry = new THREE.BoxGeometry(0.3, 0.5, 0.3)
  const headMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xff6b6b,
    emissive: 0xff6b6b,
    emissiveIntensity: 0.5,
    shininess: 100
  })
  const head = new THREE.Mesh(headGeometry, headMaterial)
  head.position.y = -0.5
  group.add(head)

  return group
}

// Create Assembly Station machine model
const createAssemblyStationMachine = () => {
  const group = new THREE.Group()

  // Base
  const baseGeometry = new THREE.BoxGeometry(2, 0.3, 1.5)
  const baseMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const base = new THREE.Mesh(baseGeometry, baseMaterial)
  base.position.y = -1
  group.add(base)

  // Conveyor
  const conveyorGeometry = new THREE.BoxGeometry(1.8, 0.1, 1)
  const conveyorMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const conveyor = new THREE.Mesh(conveyorGeometry, conveyorMaterial)
  conveyor.position.y = -0.8
  group.add(conveyor)

  // Assembly arm
  const armGeometry = new THREE.BoxGeometry(0.1, 0.8, 0.1)
  const armMaterial = new THREE.MeshPhongMaterial({ 
    color: 0xe599f7,
    shininess: 100
  })
  const arm = new THREE.Mesh(armGeometry, armMaterial)
  arm.position.y = -0.4
  group.add(arm)

  return group
}

// Create generic machine model
const createGenericMachine = () => {
  const group = new THREE.Group()

  // Main body
  const bodyGeometry = new THREE.BoxGeometry(2, 2, 2)
  const bodyMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x4dabf7,
    shininess: 100
  })
  const body = new THREE.Mesh(bodyGeometry, bodyMaterial)
  group.add(body)

  // Top panel
  const panelGeometry = new THREE.BoxGeometry(1.8, 0.1, 1.8)
  const panelMaterial = new THREE.MeshPhongMaterial({ 
    color: 0x3bc9db,
    shininess: 100
  })
  const panel = new THREE.Mesh(panelGeometry, panelMaterial)
  panel.position.y = 1.05
  group.add(panel)

  return group
}

// Update machine model when selection changes
const updateMachineModel = () => {
  if (!selectedMachine.value) return

  const machineType = selectedMachine.value.type
  createMachineModel(machineType)
  emit('machine-focused', selectedMachine.value)
}

// Animation loop
const animate = () => {
  animationFrameId = requestAnimationFrame(animate)

  if (controls) {
    controls.update()
  }

  if (renderer && scene && camera) {
    renderer.render(scene, camera)
  }
}

const stopAnimation = () => {
  if (animationFrameId !== null) {
    cancelAnimationFrame(animationFrameId)
    animationFrameId = null
  }
}

// Handle window resize
const onWindowResize = () => {
  if (!camera || !renderer || !canvasContainer.value) return

  camera.aspect = canvasContainer.value.clientWidth / canvasContainer.value.clientHeight
  camera.updateProjectionMatrix()
  renderer.setSize(
    canvasContainer.value.clientWidth,
    canvasContainer.value.clientHeight
  )
}

// Control functions
const rotateLeft = () => {
  if (machineModel) {
    machineModel.rotation.y += 0.2
  }
}

const rotateRight = () => {
  if (machineModel) {
    machineModel.rotation.y -= 0.2
  }
}

const zoomIn = () => {
  if (camera) {
    camera.position.z = Math.max(2, camera.position.z - 0.5)
  }
}

const zoomOut = () => {
  if (camera) {
    camera.position.z = Math.min(10, camera.position.z + 0.5)
  }
}

const resetView = () => {
  if (camera) {
    camera.position.set(0, 0, 5)
    camera.lookAt(0, 0, 0)
  }
  if (machineModel) {
    machineModel.rotation.set(0, 0, 0)
  }
}

watch(
  () => props.machines,
  (newMachines) => {
    if (newMachines?.length && !selectedMachineId.value) {
      selectedMachineId.value = newMachines[0]!.id
    }
  },
  { deep: true }
)

watch(
  () => props.selectedMachineId,
  (newId) => {
    if (newId && newId !== selectedMachineId.value) {
      selectedMachineId.value = newId
      updateMachineModel()
    }
  }
)

watch(selectedMachineId, (machineId) => {
  if (machineId) {
    emit('machine-selected', machineId)
  }
})

watch(
  () => computedMachines.value,
  (list) => {
    if (!selectedMachineId.value && list.length) {
      selectedMachineId.value = list[0]!.id
    }
  },
  { immediate: true }
)

watch(selectedMachine, (machine) => {
  if (machine) {
    updateMachineModel()
  }
})

// Initialize
onMounted(async () => {
  if (!props.machines?.length) {
    await loadMachines()
  }
  initScene()
})

// Cleanup
onBeforeUnmount(() => {
  window.removeEventListener('resize', onWindowResize)

  stopAnimation()

  if (renderer) {
    renderer.dispose()
  }

  if (controls) {
    controls.dispose()
  }

  if (machineModel && scene) {
    scene.remove(machineModel)
    disposeObject3D(machineModel)
  }
})
</script>

<style scoped>
.machine-3d-viewer {
  padding: 20px;
  background: linear-gradient(135deg, #1a2a6c, #2c3e50);
  border-radius: 10px;
  color: white;
  margin: 20px 0;
}

.machine-3d-viewer h2 {
  text-align: center;
  margin-bottom: 30px;
  font-size: 2rem;
  text-shadow: 0 2px 4px rgba(0,0,0,0.3);
}

.viewer-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.canvas-container {
  width: 100%;
  height: 400px;
  border-radius: 10px;
  overflow: hidden;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
}

.controls {
  display: flex;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 20px;
}

.machine-selector {
  display: flex;
  align-items: center;
  gap: 10px;
}

.machine-selector label {
  font-weight: bold;
}

.machine-selector select {
  padding: 8px 12px;
  border-radius: 5px;
  border: none;
  background: rgba(255, 255, 255, 0.9);
  color: #333;
}

.view-controls {
  display: flex;
  gap: 10px;
}

.control-btn {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  border: none;
  background: rgba(255, 255, 255, 0.2);
  color: white;
  font-size: 18px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.control-btn:hover {
  background: rgba(255, 255, 255, 0.3);
  transform: scale(1.1);
}

.machine-details {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  padding: 20px;
  margin-top: 20px;
}

.machine-details h3 {
  margin-top: 0;
  margin-bottom: 20px;
  text-align: center;
}

.details-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 15px;
}

.detail-item {
  display: flex;
  flex-direction: column;
}

.label {
  font-weight: bold;
  margin-bottom: 5px;
  color: #a0a0ff;
}

.value {
  font-size: 1.1rem;
}

.status-idle {
  color: #bbbbbb;
}

.status-running {
  color: #69db7c;
}

.status-error {
  color: #ff6b6b;
}

.status-maintenance {
  color: #ffd43b;
}

@media (max-width: 768px) {
  .controls {
    flex-direction: column;
  }
  
  .canvas-container {
    height: 300px;
  }
}
</style>
