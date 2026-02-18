import axiosClient from '@/api/axiosClient'
import type { PredictionDto } from '@/api/types'

export async function requestPrediction(machineId: string): Promise<PredictionDto> {
  return axiosClient.post<PredictionDto, PredictionDto>('/Predictions', {
    machineId
  })
}

export async function fetchPredictionHistory(machineId: string, take = 100): Promise<PredictionDto[]> {
  return axiosClient.get<PredictionDto[], PredictionDto[]>(`/Predictions/${encodeURIComponent(machineId)}`, {
    params: { take }
  })
}

export async function requestEnsemblePrediction(machineId: string): Promise<PredictionDto> {
  return axiosClient.post<PredictionDto, PredictionDto>(`/AdvancedAnalytics/predictions/${encodeURIComponent(machineId)}/ensemble`)
}

export async function retrainModels(forceRetrain = false, modelType = 'all'): Promise<any> {
  return axiosClient.post('/Predictions/train', {
    forceRetrain,
    modelType
  })
}
