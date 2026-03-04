import axiosClient from '@/api/axiosClient'
import type {
  MachineDto,
  MachineCreateDto,
  MachineUpdateDto,
  PaginatedResponse
} from '@/api/types'

export async function fetchMachines(): Promise<MachineDto[]> {
  const response = await axiosClient.get<MachineDto[] | PaginatedResponse<MachineDto>, MachineDto[] | PaginatedResponse<MachineDto>>('Machines')
  
  if (Array.isArray(response)) {
    return response
  }
  
  if (response && typeof response === 'object' && 'items' in response) {
    return response.items as MachineDto[]
  }

  return []
}

export async function fetchMachinesPage(params: { page?: number; pageSize?: number } = {}): Promise<PaginatedResponse<MachineDto>> {
  const query = {
    page: params.page,
    pageSize: params.pageSize
  }

  const response = await axiosClient.get<PaginatedResponse<MachineDto> | MachineDto[], PaginatedResponse<MachineDto> | MachineDto[]>(
    'Machines',
    {
      params: query
    }
  )

  if (Array.isArray(response)) {
    const fallbackPageSize = params.pageSize ?? (response.length > 0 ? response.length : 1)
    return {
      items: response,
      totalCount: response.length,
      totalPages: Math.ceil(response.length / fallbackPageSize),
      page: params.page ?? 1,
      pageSize: fallbackPageSize
    }
  }

  return response
}

export async function fetchMachine(id: string): Promise<MachineDto> {
  return axiosClient.get<MachineDto, MachineDto>(`/Machines/${encodeURIComponent(id)}`)
}

export async function createMachine(payload: MachineCreateDto): Promise<MachineDto> {
  return axiosClient.post<MachineDto, MachineDto>('Machines', payload)
}

export async function updateMachine(id: string, payload: MachineUpdateDto): Promise<MachineDto> {
  return axiosClient.put<MachineDto, MachineDto>(`/Machines/${encodeURIComponent(id)}`, payload)
}

export async function deleteMachine(id: string): Promise<void> {
  await axiosClient.delete<void, void>(`/Machines/${encodeURIComponent(id)}`)
}

// Export service object for convenience
export const machinesService = {
  fetchMachines,
  fetchMachinesPage,
  fetchMachine,
  createMachine,
  updateMachine,
  deleteMachine
}

export default machinesService
