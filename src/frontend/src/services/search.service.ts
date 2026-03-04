import type { SearchRequestDto, SearchResultDto, SearchSuggestionDto, SavedSearchDto, SaveSearchRequestDto } from '@/api/types'
import axiosClient from '@/api/axiosClient'

export class SearchService {
  /**
   * Performs a unified search across all entity types
   * POST /api/Search
   */
  static async search(request: SearchRequestDto): Promise<SearchResultDto> {
    const response = await axiosClient.post<SearchResultDto>('Search', request)
    return response.data
  }

  /**
   * Gets search suggestions for autocomplete
   * GET /api/Search/suggestions
   */
  static async getSuggestions(query: string, entityType?: string, limit = 10): Promise<SearchSuggestionDto[]> {
    const params = new URLSearchParams({
      query,
      limit: limit.toString()
    })
    
    if (entityType) {
      params.append('entityType', entityType)
    }

    const response = await axiosClient.get<SearchSuggestionDto[]>(`/Search/suggestions?${params}`)
    return response.data || []
  }

  /**
   * Gets saved searches for the current user
   * GET /api/Search/saved
   */
  static async getSavedSearches(): Promise<SavedSearchDto[]> {
    const response = await axiosClient.get<SavedSearchDto[]>('/Search/saved')
    return response.data || []
  }

  /**
   * Saves a search query for future use
   * POST /api/Search/saved
   */
  static async saveSearch(request: SaveSearchRequestDto): Promise<SavedSearchDto> {
    const response = await axiosClient.post<SavedSearchDto>('/Search/saved', request)
    return response.data
  }

  /**
   * Deletes a saved search
   * DELETE /api/Search/saved/{id}
   */
  static async deleteSavedSearch(id: string): Promise<void> {
    await axiosClient.delete(`/Search/saved/${encodeURIComponent(id)}`)
  }
}

export const searchService = SearchService
