import axiosClient from '@/api/axiosClient'
import type {
  ProductionLineDto,
  ProductionLineCreateDto,
  ProductionLineUpdateDto
} from '@/api/types'

export async function fetchProductionLines(): Promise<ProductionLineDto[]> {
  return axiosClient.get<ProductionLineDto[], ProductionLineDto[]>('/ProductionLines')
}

export async function fetchProductionLine(id: string): Promise<ProductionLineDto> {
  return axiosClient.get<ProductionLineDto, ProductionLineDto>(`/ProductionLines/${encodeURIComponent(id)}`)
}

export async function createProductionLine(payload: ProductionLineCreateDto): Promise<ProductionLineDto> {
  return axiosClient.post<ProductionLineDto, ProductionLineDto>('/ProductionLines', payload)
}

export async function updateProductionLine(
  id: string,
  payload: ProductionLineUpdateDto
): Promise<ProductionLineDto> {
  return axiosClient.put<ProductionLineDto, ProductionLineDto>(`/ProductionLines/${encodeURIComponent(id)}`, payload)
}

export async function deleteProductionLine(id: string): Promise<void> {
  await axiosClient.delete(`/ProductionLines/${encodeURIComponent(id)}`)
}
