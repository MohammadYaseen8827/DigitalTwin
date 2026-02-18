/**
 * Advanced Analytics Service
 * Handles advanced ML analytics including ensemble predictions, deep learning,
 * anomaly detection, time series forecasting, and prescriptive analytics
 */

import axiosClient from '@/api/axiosClient'
import type { PredictionDto } from '@/api/types'

export interface AnomalyDetectionRequest {
  startTime?: string
  endTime?: string
}

export interface AnomalyDetectionResult {
  machineId: string
  overallRiskScore: number
  anomalies: Array<{
    timestamp: string
    metric: string
    value: number
    expectedRange: { min: number; max: number }
    severity: 'low' | 'medium' | 'high' | 'critical'
  }>
  detectedAt: string
}

export interface ForecastRequest {
  forecastHorizon: number
}

export interface ForecastResult {
  machineId: string
  metric: string
  forecastedValues: number[]
  timestamps: string[]
  confidenceIntervals: Array<{
    lower: number
    upper: number
  }>
  methods: Array<{
    method: string
    accuracy: number
  }>
  generatedAt: string
}

export interface MaintenanceRecommendationRequest {
  costFactors: {
    preventiveMaintenanceCost: number
    reactiveFailureCost: number
    downtimeCostPerHour: number
  }
  businessImpact: {
    productionLossPerHour: number
    customerImpact: number
  }
  timeConstraints: {
    earliestDate?: string
    latestDate?: string
    preferredDays?: string[]
  }
}

export interface PrescriptiveRecommendation {
  machineId: string
  recommendedDate: string
  recommendedType: string
  expectedCost: number
  riskReduction: number
  priorityScore: number
  reasoning: string
  alternatives: Array<{
    date: string
    cost: number
    riskLevel: string
  }>
}

export interface SchedulingOptimizationRequest {
  machineIds: string[]
  planningHorizon: string
}

export interface SchedulingRecommendation {
  schedule: Array<{
    machineId: string
    recommendedSlot: string
    maintenanceType: string
    estimatedDuration: number
  }>
  optimizationScore: number
  constraints: string[]
}

export interface ResourceAllocationRequest {
  machineIds: string[]
  planningPeriod: string
}

export interface ResourceAllocationPlan {
  allocations: Array<{
    machineId: string
    team: string
    technician: string
    timeSlot: string
  }>
  resourceUtilization: number
  conflicts: string[]
}

export interface CostOptimizationRequest {
  machineIds: string[]
  budget: {
    totalBudget: number
    maxPerMachine?: number
  }
}

export interface CostOptimizationResult {
  optimizedPlan: Array<{
    machineId: string
    recommendedDate: string
    type: string
    cost: number
    priority: number
  }>
  totalCost: number
  budgetRemaining: number
  savings: number
}

export interface AdvancedAnalyticsDashboard {
  machineId: string
  prediction: PredictionDto | null
  anomalyDetection: AnomalyDetectionResult
  maintenanceRecommendation: PrescriptiveRecommendation
  generatedAt: string
  healthScore: number
}

/**
 * Perform ensemble prediction combining multiple ML models
 */
export async function ensemblePrediction(machineId: string): Promise<PredictionDto> {
  return axiosClient.post<PredictionDto, PredictionDto>(
    `/AdvancedAnalytics/predictions/${machineId}/ensemble`
  )
}

/**
 * Perform deep learning-based prediction using time series analysis
 */
export async function deepLearningPrediction(machineId: string): Promise<PredictionDto> {
  return axiosClient.post<PredictionDto, PredictionDto>(
    `/AdvancedAnalytics/predictions/${machineId}/deep-learning`
  )
}

/**
 * Detect anomalies in machine telemetry data
 */
export async function detectAnomalies(
  machineId: string,
  request: AnomalyDetectionRequest = {}
): Promise<AnomalyDetectionResult> {
  return axiosClient.post<AnomalyDetectionResult, AnomalyDetectionResult>(
    `/AdvancedAnalytics/anomaly-detection/${machineId}`,
    request
  )
}

/**
 * Perform time series forecasting for machine metrics
 */
export async function forecastTimeSeries(
  machineId: string,
  metric: string,
  request: ForecastRequest
): Promise<ForecastResult> {
  return axiosClient.post<ForecastResult, ForecastResult>(
    `/AdvancedAnalytics/forecasting/${machineId}/${metric}`,
    request
  )
}

/**
 * Generate prescriptive maintenance recommendations
 */
export async function generateMaintenanceRecommendation(
  machineId: string,
  request: MaintenanceRecommendationRequest
): Promise<PrescriptiveRecommendation> {
  return axiosClient.post<PrescriptiveRecommendation, PrescriptiveRecommendation>(
    `/AdvancedAnalytics/prescriptive/maintenance/${machineId}`,
    request
  )
}

/**
 * Optimize production scheduling based on equipment health
 */
export async function optimizeProductionSchedule(
  request: SchedulingOptimizationRequest
): Promise<SchedulingRecommendation> {
  return axiosClient.post<SchedulingRecommendation, SchedulingRecommendation>(
    '/AdvancedAnalytics/prescriptive/scheduling',
    request
  )
}

/**
 * Optimize resource allocation for maintenance activities
 */
export async function optimizeResourceAllocation(
  request: ResourceAllocationRequest
): Promise<ResourceAllocationPlan> {
  return axiosClient.post<ResourceAllocationPlan, ResourceAllocationPlan>(
    '/AdvancedAnalytics/prescriptive/resource-allocation',
    request
  )
}

/**
 * Optimize maintenance costs within budget constraints
 */
export async function optimizeMaintenanceCosts(
  request: CostOptimizationRequest
): Promise<CostOptimizationResult> {
  return axiosClient.post<CostOptimizationResult, CostOptimizationResult>(
    '/AdvancedAnalytics/prescriptive/cost-optimization',
    request
  )
}

/**
 * Get comprehensive advanced analytics dashboard data
 */
export async function getAdvancedAnalyticsDashboard(
  machineId: string
): Promise<AdvancedAnalyticsDashboard> {
  return axiosClient.get<AdvancedAnalyticsDashboard, AdvancedAnalyticsDashboard>(
    `/AdvancedAnalytics/dashboard/${machineId}`
  )
}
