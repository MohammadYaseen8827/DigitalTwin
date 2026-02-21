import type { EquipmentStatus } from '@/api/types'

type StatusToken = EquipmentStatus | string | number | undefined | null

const numericStatusMap: Record<number, keyof typeof statusLabelMap> = {
  0: 'operational',
  1: 'warning',
  2: 'critical',
  3: 'maintenance',
  4: 'offline'
}

const statusLabelMap = {
  operational: 'Operational',
  maintenance: 'Maintenance',
  warning: 'Warning',
  critical: 'Critical',
  offline: 'Offline',
  unknown: 'Unknown'
} as const

function toLowerToken(input: StatusToken): string | undefined {
  if (input === null || input === undefined) {
    return undefined
  }

  if (typeof input === 'number') {
    return numericStatusMap[input]
  }

  return String(input).trim().toLowerCase()
}

export function normalizeStatus(status: StatusToken): keyof typeof statusLabelMap {
  const token = toLowerToken(status)

  switch (token) {
    case 'operational':
      return 'operational'
    case 'maintenance':
      return 'maintenance'
    case 'warning':
      return 'warning'
    case 'critical':
      return 'critical'
    case 'offline':
      return 'offline'
    default:
      return 'unknown'
  }
}

export function formatStatus(status: StatusToken): string {
  const normalized = normalizeStatus(status)
  return statusLabelMap[normalized]
}

export function statusClassifier(status: StatusToken): string {
  const normalized = normalizeStatus(status)
  return `status-${normalized}`
}
