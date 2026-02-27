// File: src/services/degradationModeling.service.ts
import axiosClient from '@/api/axiosClient'
import { errorReporter } from './errorReporter.service'
import type { 
  DegradationModelDto,
  CreateDegradationModelDto,
  UpdateDegradationModelDto,
  DegradationModelType,
  DegradationPredictionDto,
  AIModelTrainingRequestDto,
  AIModelTrainingResultDto,
  DegradationAnalysisDto
} from '@/api/types'
import { useToast } from '@/lib/magic-mcp-ui'

const toast = useToast()

// === ODE Solver Endpoints ===

export interface ExponentialDegradationRequest {
  machineType: string
  initialValue: number
  degradationRate: number
  timeHorizon: number
  timePoints?: number
  solverMethod?: string
}

export interface PowerLawDegradationRequest {
  machineType: string
  initialValue: number
  degradationRate: number
  power: number
  timeHorizon: number
  timePoints?: number
  solverMethod?: string
}

export interface MultiVariableDegradationRequest {
  machineType: string
  initialValues: number[]
  parameters: Record<string, number>
  timeHorizon: number
  timePoints?: number
  solverMethod?: string
}

export interface StochasticDegradationRequest {
  machineType: string
  modelType: string
  initialValue: number
  degradationRate: number
  diffusionCoefficient: number
  power?: number
  timeHorizon?: number
  timePoints?: number
  numberOfSimulations?: number
  confidenceLevel?: number
}

export interface ODESolution {
  timePoints: number[]
  values: number[][]
  solverMethod: string
  metadata: Record<string, any>
}

/**
 * Solve exponential degradation ODE
 */
export async function solveExponentialDegradation(request: ExponentialDegradationRequest): Promise<ODESolution> {
  try {
    const response = await axiosClient.post('/DegradationModeling/solve/exponential', request)
    return response.data
  } catch (error) {
    errorReporter.error('Failed to solve exponential degradation:', error)
    throw error
  }
}

/**
 * Solve power-law degradation ODE
 */
export async function solvePowerLawDegradation(request: PowerLawDegradationRequest): Promise<ODESolution> {
  try {
    const response = await axiosClient.post('/DegradationModeling/solve/powerlaw', request)
    return response.data
  } catch (error) {
    errorReporter.error('Failed to solve power-law degradation:', error)
    throw error
  }
}

/**
 * Solve multi-variable degradation ODE
 */
export async function solveMultiVariableDegradation(request: MultiVariableDegradationRequest): Promise<ODESolution> {
  try {
    const response = await axiosClient.post('/DegradationModeling/solve/multivariable', request)
    return response.data
  } catch (error) {
    errorReporter.error('Failed to solve multi-variable degradation:', error)
    throw error
  }
}

/**
 * Solve stochastic degradation model
 */
export async function solveStochasticDegradation(request: StochasticDegradationRequest): Promise<any> {
  try {
    const response = await axiosClient.post('/DegradationModeling/solve/stochastic', request)
    return response.data
  } catch (error) {
    errorReporter.error('Failed to solve stochastic degradation:', error)
    throw error
  }
}

// === Parameter Estimation Endpoints ===

export interface ParameterEstimationRequest {
  historicalData: Array<{ time: number; value: number }>
  initialGuess?: Record<string, number>
}

export interface ParameterEstimationResult {
  parameters: Record<string, number>
  logLikelihood: number
  standardErrors: Record<string, number>
  estimationMetadata: Record<string, any>
}

/**
 * Estimate exponential degradation parameters
 */
export async function estimateExponentialParameters(request: ParameterEstimationRequest): Promise<ParameterEstimationResult> {
  try {
    const response = await axiosClient.post('/DegradationModeling/estimate/exponential', request)
    return response.data
  } catch (error) {
    errorReporter.error('Failed to estimate exponential parameters:', error)
    throw error
  }
}

/**
 * Estimate power-law degradation parameters
 */
export async function estimatePowerLawParameters(request: ParameterEstimationRequest): Promise<ParameterEstimationResult> {
  try {
    const response = await axiosClient.post('/DegradationModeling/estimate/powerlaw', request)
    return response.data
  } catch (error) {
    errorReporter.error('Failed to estimate power-law parameters:', error)
    throw error
  }
}

/**
 * Estimate multi-variable degradation parameters
 */
export async function estimateMultiVariableParameters(request: {
  historicalData: Array<{ time: number; value: number }>
  variableNames: string[]
  initialGuess?: Record<string, number>
}): Promise<ParameterEstimationResult> {
  try {
    const response = await axiosClient.post('/DegradationModeling/estimate/multivariable', request)
    return response.data
  } catch (error) {
    errorReporter.error('Failed to estimate multi-variable parameters:', error)
    throw error
  }
}

/**
 * Compare multiple degradation models
 */
export async function compareModels(request: {
  historicalData: Array<{ time: number; value: number }>
  modelTypes: string[]
}): Promise<any> {
  try {
    const response = await axiosClient.post('/DegradationModeling/compare', request)
    return response.data
  } catch (error) {
    errorReporter.error('Failed to compare models:', error)
    throw error
  }
}

/**
 * Validate degradation model parameters
 */
export async function validateParameters(request: {
  estimatedParameters: ParameterEstimationResult
  historicalData: Array<{ time: number; value: number }>
  folds?: number
}): Promise<any> {
  try {
    const response = await axiosClient.post('/DegradationModeling/validate', request)
    return response.data
  } catch (error) {
    errorReporter.error('Failed to validate parameters:', error)
    throw error
  }
}

/**
 * Validate ODE solution for numerical stability
 */
export async function validateSolution(request: {
  solution: ODESolution
  validationParameters?: Record<string, number>
}): Promise<any> {
  try {
    const response = await axiosClient.post('/DegradationModeling/validate/solution', request)
    return response.data
  } catch (error) {
    errorReporter.error('Failed to validate solution:', error)
    throw error
  }
}

// Utility functions
export function getDegradationModelTypeDisplayName(type: DegradationModelType): string {
  const typeNames: Record<DegradationModelType, string> = {
    'MarkovChain': 'Markov Chain',
    'Weibull': 'Weibull Distribution',
    'Exponential': 'Exponential Decay',
    'Linear': 'Linear Degradation',
    'Polynomial': 'Polynomial Regression',
    'NeuralNetwork': 'Neural Network',
    'RandomForest': 'Random Forest',
    'LSTM': 'Long Short-Term Memory'
  }
  return typeNames[type] || type
}

export function getDegradationSeverityColor(severity: number): string {
  if (severity >= 0.8) return 'var(--color-error)'
  if (severity >= 0.6) return 'var(--color-warning)'
  if (severity >= 0.4) return 'var(--color-text-warning)'
  return 'var(--color-success)'
}

export function formatDegradationRate(rate: number): string {
  return `${(rate * 100).toFixed(2)}% per day`
}

export function calculateRemainingUsefulLife(currentHealth: number, degradationRate: number): number {
  if (degradationRate <= 0) return Infinity
  return Math.max(0, (currentHealth - 0.1) / degradationRate) // Assume failure at 10% health
}
