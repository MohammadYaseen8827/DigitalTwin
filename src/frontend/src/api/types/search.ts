/**
 * Search type definitions matching backend DTOs.
 * Auto-generated from DigitalTwinPlatform.Application.Search.Models
 */

// Entity types for search
export type SearchEntityType = 'machines' | 'telemetry' | 'maintenance' | 'alerts' | 'production_lines'
export type SortOrder = 'relevance' | 'date_desc' | 'date_asc' | 'name_asc' | 'name_desc'

export interface SearchRequestDto {
  /** The search query string */
  query: string
  /** Entity types to search. If null/empty, searches all types */
  entityTypes?: string[] | null
  /** Date range filter (1h, 6h, 12h, 24h, 7d, 30d, 1y) */
  dateRange?: string | null
  /** Status filter for entities that support status */
  statusFilter?: string | null
  /** Advanced filters for specific fields */
  advancedFilters?: AdvancedFiltersDto | null
  /** Sort order */
  sortBy?: SortOrder | string
  /** Page number for pagination (0-based) */
  page?: number
  /** Number of results per page */
  pageSize?: number
}

export interface AdvancedFiltersDto {
  /** Minimum temperature filter for telemetry data */
  minTemperature?: number | null
  /** Maximum temperature filter for telemetry data */
  maxTemperature?: number | null
  /** Minimum pressure filter for telemetry data */
  minPressure?: number | null
  /** Maximum pressure filter for telemetry data */
  maxPressure?: number | null
  /** Location filter for machines */
  location?: string | null
  /** Manufacturer filter for machines */
  manufacturer?: string | null
  /** Model filter for machines */
  model?: string | null
}

export interface SearchResultDto {
  /** List of search result items */
  items: SearchResultItemDto[]
  /** Total number of results matching the query */
  totalCount: number
  /** Current page number */
  page: number
  /** Number of results per page */
  pageSize: number
  /** Total number of pages */
  totalPages: number
  /** Query execution time in milliseconds */
  executionTimeMs: number
}

export interface SearchResultItemDto {
  /** Unique identifier of the result item */
  id: string
  /** Entity type (machine, telemetry, maintenance, alert, production_line) */
  type: string
  /** Title of the result item */
  title: string
  /** Subtitle or description of the result item */
  subtitle: string
  /** Status of the entity (if applicable) */
  status?: string | null
  /** Creation timestamp of the entity */
  createdAt: string
  /** Relevance score (0.0 to 1.0) */
  matchScore: number
  /** Additional metadata for the result item */
  metadata?: Record<string, unknown> | null
}

export interface SearchSuggestionDto {
  /** Suggestion text */
  text: string
  /** Entity type the suggestion applies to */
  entityType: string
  /** Estimated relevance score */
  score: number
}

export interface SavedSearchDto {
  /** Unique identifier of the saved search */
  id: string
  /** Display name for the saved search */
  name: string
  /** Search query string */
  query: string
  /** Entity types filter */
  entityTypes?: string[] | null
  /** Creation timestamp */
  createdAt: string
  /** Last used timestamp */
  lastUsed?: string | null
}

export interface SaveSearchRequestDto {
  /** Display name for the saved search */
  name: string
  /** Search query string */
  query: string
  /** Entity types filter */
  entityTypes?: string[] | null
}
