import type { MachineDto } from '@/api/types'

/**
 * Fallback dataset used to generate deterministic pseudo metrics when the API
 * does not provide a specific property. This keeps visualisations stable while
 * still looking plausible during demos.
 */
const metricSeeds: Record<string, { uptime: number; risk: 'Low' | 'Medium' | 'High' }> = {
  alpha: { uptime: 92, risk: 'Low' },
  beta: { uptime: 88, risk: 'Medium' },
  gamma: { uptime: 76, risk: 'High' },
  delta: { uptime: 84, risk: 'Low' },
  epsilon: { uptime: 70, risk: 'High' }
}

function deterministicFromId(id: string): number {
  let hash = 0
  for (let i = 0; i < id.length; i += 1) {
    hash = (hash << 5) - hash + id.charCodeAt(i)
    hash |= 0 // Convert to 32bit integer
  }
  return Math.abs(hash)
}

export function getMachineUptime(id: string, machines?: MachineDto[]): number {
  const machine = machines?.find(m => m.id === id)
  if (machine?.properties && typeof machine.properties['uptime'] === 'number') {
    return Math.max(0, Math.min(100, Number(machine.properties['uptime'])))
  }

  const seed = metricSeeds[id.toLowerCase() as keyof typeof metricSeeds]
  if (seed) {
    return seed.uptime
  }

  const hash = deterministicFromId(id)
  return 65 + (hash % 31) // 65-95 range keeps variety while staying realistic
}

export function getRiskLevel(id: string, machines?: MachineDto[]): 'Low' | 'Medium' | 'High' | 'Critical' | 'Unknown' {
  const machine = machines?.find(m => m.id === id)
  const status = typeof machine?.status === 'string' ? machine.status : undefined

  if (status?.toLowerCase() === 'critical') {
    return 'Critical'
  }

  const riskProperty = machine?.properties?.riskLevel
  if (typeof riskProperty === 'string') {
    const normalized = riskProperty.toLowerCase()
    if (normalized === 'high') return 'High'
    if (normalized === 'medium') return 'Medium'
    if (normalized === 'low') return 'Low'
  }

  const seed = metricSeeds[id.toLowerCase() as keyof typeof metricSeeds]
  if (seed) {
    return seed.risk
  }

  const hash = deterministicFromId(id)
  const bucket = hash % 100
  if (bucket >= 85) return 'High'
  if (bucket >= 55) return 'Medium'
  if (bucket >= 15) return 'Low'
  return 'Unknown'
}
