import axiosClient from '@/api/axiosClient'

export interface UncertaintyAnalysisResult {
  analysisType: string
  machineId: string
  meanPrediction: number
  standardDeviation: number
  confidenceInterval: ConfidenceInterval
  featureUncertainties?: Record<string, number>
  predictionVariance: number
  sampleCount: number
  timestamp: string
}

export interface ConfidenceInterval {
  lowerBound: number
  upperBound: number
  confidenceLevel: number
  coverageProbability?: number
  timestamp?: string
}

export interface ModelUncertaintyResult {
  machineId: string
  aleatoricUncertainty: number
  epistemicUncertainty: number
  totalUncertainty: number
  featureImportanceWithUncertainty: Record<string, number>
  modelConfidence: number
  uncertaintyComponents: UncertaintyComponents
  timestamp: string
}

export interface UncertaintyComponents {
  dataNoise: number
  modelVariance: number
  total: number
}

export interface ComprehensiveUncertaintyReport {
  machineId: string
  monteCarloResults?: UncertaintyAnalysisResult
  bayesianResults?: UncertaintyAnalysisResult
  bootstrapIntervals?: ConfidenceInterval
  modelUncertainty?: ModelUncertaintyResult
  overallUncertaintyScore: number
  riskAssessment: string
  recommendations: string[]
  timestamp: string
}

export interface MonteCarloRequest {
  baseFeatures: Record<string, number>
  iterations: number
  noiseLevel: number
}

export interface BayesianRequest {
  observedData: number[]
  priorDistributions: Record<string, PriorDistribution>
  samples: number
}

export interface PriorDistribution {
  distributionType: string
  parameters: Record<string, number>
}

export interface ModelUncertaintyRequest {
  features: Record<string, number>
}

/**
 * Perform Monte Carlo uncertainty analysis
 */
export async function monteCarloAnalysis(
  machineId: string,
  request: MonteCarloRequest
): Promise<UncertaintyAnalysisResult> {
  const response = await axiosClient.post<UncertaintyAnalysisResult>(
    `/Uncertainty/${machineId}/monte-carlo`,
    request
  )
  return response.data
}

/**
 * Perform Bayesian uncertainty analysis
 */
export async function bayesianAnalysis(
  machineId: string,
  request: BayesianRequest
): Promise<UncertaintyAnalysisResult> {
  const response = await axiosClient.post<UncertaintyAnalysisResult>(
    `/Uncertainty/${machineId}/bayesian`,
    request
  )
  return response.data
}

/**
 * Calculate bootstrap confidence intervals
 */
export async function bootstrapIntervals(
  machineId: string,
  samples: number = 1000,
  confidenceLevel: number = 0.95
): Promise<ConfidenceInterval> {
  const response = await axiosClient.get<ConfidenceInterval>(
    `/Uncertainty/${machineId}/bootstrap-intervals?samples=${samples}&confidenceLevel=${confidenceLevel}`
  )
  return response.data
}

/**
 * Quantify model uncertainty
 */
export async function modelUncertainty(
  machineId: string,
  request: ModelUncertaintyRequest
): Promise<ModelUncertaintyResult> {
  const response = await axiosClient.post<ModelUncertaintyResult>(
    `/Uncertainty/${machineId}/model-uncertainty`,
    request
  )
  return response.data
}

/**
 * Get comprehensive uncertainty report
 */
export async function comprehensiveReport(machineId: string): Promise<ComprehensiveUncertaintyReport> {
  const response = await axiosClient.get<ComprehensiveUncertaintyReport>(
    `/Uncertainty/${machineId}/comprehensive-report`
  )
  return response.data
}

/**
 * Quick Monte Carlo analysis with default parameters
 */
export async function quickMonteCarlo(machineId: string): Promise<UncertaintyAnalysisResult> {
  const request: MonteCarloRequest = {
    baseFeatures: {
      temperature: 75,
      vibration: 0.5,
      pressure: 100,
      rpm: 1200
    },
    iterations: 1000,
    noiseLevel: 0.1
  }
  return monteCarloAnalysis(machineId, request)
}

/**
 * Quick model uncertainty analysis
 */
export async function quickModelUncertainty(machineId: string): Promise<ModelUncertaintyResult> {
  const request: ModelUncertaintyRequest = {
    features: {
      temperature: 75,
      vibration: 0.5,
      pressure: 100,
      rpm: 1200
    }
  }
  return modelUncertainty(machineId, request)
}