import axiosClient from '@/api/axiosClient'

export interface OdeSolution {
  systemName: string
  timePoints: number[]
  solutions: number[][]
  steps: number
  finalTime: number
  finalState: number[]
}

export interface SystemDynamicsSolution {
  systemName: string
  timePoints: number[]
  stateVariables: number[][]
  derivedQuantities: Record<string, number[]>
  stability: StabilityAnalysis
  energyBalance: EnergyBalance
}

export interface StabilityAnalysis {
  isStable: boolean
  maxChange: number
  convergenceRate: number
}

export interface EnergyBalance {
  kineticEnergy: number[]
  potentialEnergy: number[]
  totalEnergy: number[]
  energyConserved: boolean
}

export interface OptimizationResult {
  optimalParameters: number[]
  optimalValue: number
  iterations: number
  converged: boolean
  method: string
}

export interface OdeSolveRequest {
  systemName: string
  equations: string[]
  initialConditions: number[]
  startTime: number
  endTime: number
  stepSize: number
}

export interface SystemDynamicsRequest {
  systemName: string
  equations: string[]
  initialConditions: number[]
  stepSize: number
  simulationTime: number
  derivedQuantities: DerivedQuantityDefinition[]
}

export interface DerivedQuantityDefinition {
  name: string
  expression: string
}

export interface GradientOptimizationRequest {
  objectiveExpression: string
  initialGuess: number[]
  learningRate: number
  maxIterations: number
  tolerance: number
  epsilon: number
  parameterBounds?: ParameterBound[]
}

export interface GeneticOptimizationRequest {
  fitnessExpression: string
  parameterCount: number
  populationSize: number
  maxGenerations: number
  mutationRate: number
  tournamentSize: number
  parameterBounds: ParameterBound[]
}

export interface MultiObjectiveOptimizationRequest {
  objectives: string[]
  constraints: string[]
  parameterBounds: ParameterBound[]
  populationSize?: number
  generations?: number
}

export interface MultiObjectiveResult {
  paretoFront: Array<{
    parameters: number[]
    objectiveValues: number[]
  }>
  iterations: number
  hypervolume: number
}

export interface ParameterBound {
  min: number
  max: number
}

/**
 * Solve ordinary differential equations
 */
export async function solveOde(request: OdeSolveRequest): Promise<OdeSolution> {
  const response = await axiosClient.post<OdeSolution>('MathematicalModeling/ode/solve', request)
  return response.data
}

/**
 * Solve system dynamics with derived quantities
 */
export async function solveSystemDynamics(request: SystemDynamicsRequest): Promise<SystemDynamicsSolution> {
  const response = await axiosClient.post<SystemDynamicsSolution>('MathematicalModeling/system-dynamics/solve', request)
  return response.data
}

/**
 * Perform gradient-based optimization
 */
export async function gradientOptimization(request: GradientOptimizationRequest): Promise<OptimizationResult> {
  const response = await axiosClient.post<OptimizationResult>('MathematicalModeling/optimization/gradient', request)
  return response.data
}

/**
 * Perform genetic algorithm optimization
 */
export async function geneticOptimization(request: GeneticOptimizationRequest): Promise<OptimizationResult> {
  const response = await axiosClient.post<OptimizationResult>('MathematicalModeling/optimization/genetic', request)
  return response.data
}

/**
 * Perform multi-objective optimization
 */
export async function multiObjectiveOptimization(request: MultiObjectiveOptimizationRequest): Promise<MultiObjectiveResult> {
  const response = await axiosClient.post<MultiObjectiveResult>('MathematicalModeling/optimization/multi-objective', request)
  return response.data
}

/**
 * Get available system models
 */
export async function getSystemModels(): Promise<string[]> {
  const response = await axiosClient.get<string[]>('MathematicalModeling/models')
  return response.data || []
}

/**
 * Quick solve ODE with default parameters
 */
export async function quickSolveOde(systemName: string, equations: string[]): Promise<OdeSolution> {
  const request: OdeSolveRequest = {
    systemName,
    equations,
    initialConditions: new Array(equations.length).fill(0),
    startTime: 0,
    endTime: 10,
    stepSize: 0.01
  }
  return solveOde(request)
}

/**
 * Quick optimization with default parameters
 */
export async function quickOptimize(objectiveExpression: string): Promise<OptimizationResult> {
  const request: GradientOptimizationRequest = {
    objectiveExpression,
    initialGuess: [1, 1],
    learningRate: 0.01,
    maxIterations: 1000,
    tolerance: 1e-6,
    epsilon: 1e-8
  }
  return gradientOptimization(request)
}