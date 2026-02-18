export type ValueStatus = 'value' | 'zero' | 'not_available' | 'not_applicable' | 'missing'

export interface FormatMetricOptions {
  /** Force the value to be treated as not applicable (overrides other checks). */
  notApplicable?: boolean
  /** Optional precision for numeric values. */
  precision?: number
  /** Optional suffix appended directly to numeric output (e.g. "%" or "°C"). */
  suffix?: string
  /** Optional override for the tooltip copy. */
  tooltip?: string
}

export interface FormattedMetricValue {
  display: string
  status: ValueStatus
  tooltip?: string
  suffix?: string
}

export const valueStatusMeta: Record<ValueStatus, { tooltip: string; icon?: string }> = {
  value: { tooltip: '' },
  zero: {
    tooltip: 'Measured value is zero.',
    icon: 'MinusCircle'
  },
  not_available: {
    tooltip: 'Data point not provided.',
    icon: 'HelpCircle'
  },
  not_applicable: {
    tooltip: 'Metric not applicable for this context.',
    icon: 'Ban'
  },
  missing: {
    tooltip: 'Reading unavailable due to a data gap.',
    icon: 'AlertTriangle'
  }
}

const numberFormatterCache: Record<number, Intl.NumberFormat> = {}

function formatNumber(value: number, precision = 0): string {
  if (!Object.prototype.hasOwnProperty.call(numberFormatterCache, precision)) {
    numberFormatterCache[precision] = new Intl.NumberFormat('en-US', {
      minimumFractionDigits: precision,
      maximumFractionDigits: precision
    })
  }

  return numberFormatterCache[precision].format(value)
}

function normaliseString(input: string): ValueStatus | null {
  const trimmed = input.trim()
  if (!trimmed.length) return 'missing'

  const lowered = trimmed.toLowerCase()
  const notAvailableTokens = ['n/a', 'na', 'not available']
  if (notAvailableTokens.indexOf(lowered) !== -1) {
    return 'not_available'
  }

  const notApplicableTokens = ['not applicable', 'napp', 'n/a*']
  if (notApplicableTokens.indexOf(lowered) !== -1) {
    return 'not_applicable'
  }

  return null
}

export function formatMetricValue(value: unknown, options: FormatMetricOptions = {}): FormattedMetricValue {
  const { notApplicable, precision = 0, suffix, tooltip } = options

  if (notApplicable) {
    return {
      display: 'N/A',
      status: 'not_applicable',
      tooltip: tooltip ?? valueStatusMeta.not_applicable.tooltip,
      suffix
    }
  }

  if (value === null || value === undefined) {
    return {
      display: '—',
      status: 'missing',
      tooltip: tooltip ?? valueStatusMeta.missing.tooltip,
      suffix
    }
  }

  if (typeof value === 'number') {
    if (isNaN(value)) {
      return {
        display: '—',
        status: 'missing',
        tooltip: tooltip ?? valueStatusMeta.missing.tooltip,
        suffix
      }
    }

    if (value === 0) {
      return {
        display: `${formatNumber(0, precision)}${suffix ?? ''}`,
        status: 'zero',
        tooltip: tooltip ?? valueStatusMeta.zero.tooltip,
        suffix
      }
    }

    return {
      display: `${formatNumber(value, precision)}${suffix ?? ''}`,
      status: 'value',
      tooltip: tooltip ?? valueStatusMeta.value.tooltip,
      suffix
    }
  }

  if (typeof value === 'string') {
    const derivedStatus = normaliseString(value)

    if (derivedStatus === 'not_available') {
      return {
        display: 'N/A',
        status: 'not_available',
        tooltip: tooltip ?? valueStatusMeta.not_available.tooltip,
        suffix
      }
    }

    if (derivedStatus === 'not_applicable') {
      return {
        display: 'N/A',
        status: 'not_applicable',
        tooltip: tooltip ?? valueStatusMeta.not_applicable.tooltip,
        suffix
      }
    }

    if (derivedStatus === 'missing') {
      return {
        display: '—',
        status: 'missing',
        tooltip: tooltip ?? valueStatusMeta.missing.tooltip,
        suffix
      }
    }

    return {
      display: value,
      status: 'value',
      tooltip: tooltip ?? valueStatusMeta.value.tooltip,
      suffix
    }
  }

  if (typeof value === 'boolean') {
    return {
      display: value ? 'Yes' : 'No',
      status: value ? 'value' : 'zero',
      tooltip: tooltip ?? valueStatusMeta.value.tooltip,
      suffix
    }
  }

  if (value instanceof Date) {
    return {
      display: value.toLocaleString(),
      status: 'value',
      tooltip: tooltip ?? valueStatusMeta.value.tooltip,
      suffix
    }
  }

  // Fallback for unsupported types
  return {
    display: String(value),
    status: 'value',
    tooltip: tooltip ?? valueStatusMeta.value.tooltip,
    suffix
  }
}

export function getNumericProperty(
  properties: Record<string, unknown> | null | undefined,
  key: string
): number | null {
  if (!properties) return null
  const raw = properties[key]

  if (typeof raw === 'number' && Number.isFinite(raw)) {
    return raw
  }

  if (typeof raw === 'string') {
    const parsed = Number(raw)
    if (Number.isFinite(parsed)) {
      return parsed
    }
  }

  return null
}

export function deterministicMetric(id: string, min: number, max: number): number {
  const range = Math.max(max - min, 1)
  let hash = 0
  for (let i = 0; i < id.length; i += 1) {
    hash = (hash << 5) - hash + id.charCodeAt(i)
    hash |= 0
  }

  const normalized = Math.abs(hash) % range
  return Math.round(min + normalized)
}
