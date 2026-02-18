import type { SearchRequestDto, SearchResultDto, SearchSuggestionDto, SavedSearchDto, SaveSearchRequestDto } from '@/api/types'
import axiosClient from '@/api/axiosClient'

export class SearchService {
  /**
   * Performs a unified search across all entity types
   */
  static async search(request: SearchRequestDto): Promise<SearchResultDto> {
    return axiosClient.post<SearchResultDto, SearchResultDto>('/api/search', request)
  }

  /**
   * Gets search suggestions for autocomplete
   */
  static async getSuggestions(query: string, entityType?: string, limit = 10): Promise<SearchSuggestionDto[]> {
    const params = new URLSearchParams({
      query,
      limit: limit.toString()
    })
    
    if (entityType) {
      params.append('entityType', entityType)
    }

    return axiosClient.get<SearchSuggestionDto[], SearchSuggestionDto[]>(`/api/search/suggestions?${params}`)
  }

  /**
   * Gets saved searches for the current user
   */
  static async getSavedSearches(): Promise<SavedSearchDto[]> {
    return axiosClient.get<SavedSearchDto[], SavedSearchDto[]>('/api/search/saved')
  }

  /**
   * Saves a search query for future use
   */
  static async saveSearch(request: SaveSearchRequestDto): Promise<SavedSearchDto> {
    return axiosClient.post<SavedSearchDto, SavedSearchDto>('/api/search/saved', request)
  }

  /**
   * Deletes a saved search
   */
  static async deleteSavedSearch(id: string): Promise<void> {
    await axiosClient.delete<void, void>(`/api/search/saved/${encodeURIComponent(id)}`)
  }
}

export const searchService = SearchService
