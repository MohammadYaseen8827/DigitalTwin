/**
 * Enterprise ECharts Configuration
 * Optimized for real-time streaming with 60fps performance
 */

import type { EChartsOption } from 'echarts'

/**
 * Base configuration for real-time streaming charts
 * Optimized for sub-100ms updates
 */
export const streamingChartBase: EChartsOption = {
  animation: false,
  grid: {
    left: 60,
    right: 20,
    top: 20,
    bottom: 40
  },
  tooltip: {
    trigger: 'axis',
    backgroundColor: '#1E293B',
    borderColor: '#334155',
    borderWidth: 1,
    textStyle: {
      color: '#E2E8F0',
      fontFamily: 'Inter, system-ui, sans-serif',
      fontSize: 12
    },
    axisPointer: {
      type: 'line',
      lineStyle: {
        color: '#3B82F6',
        width: 1,
        type: 'solid'
      }
    }
  },
  xAxis: {
    type: 'time',
    splitLine: {
      show: false
    },
    axisLine: {
      lineStyle: {
        color: '#334155'
      }
    },
    axisLabel: {
      color: '#64748B',
      fontFamily: 'Inter, system-ui, sans-serif',
      fontSize: 11
    }
  },
  yAxis: {
    type: 'value',
    scale: true,
    splitLine: {
      lineStyle: {
        color: '#1F2A3A',
        type: 'dashed'
      }
    },
    axisLine: {
      lineStyle: {
        color: '#334155'
      }
    },
    axisLabel: {
      color: '#64748B',
      fontFamily: 'JetBrains Mono, Consolas, monospace',
      fontSize: 11
    }
  }
}

/**
 * Telemetry streaming series configuration
 */
export const telemetrySeriesConfig = {
  type: 'line' as const,
  symbol: 'none',
  sampling: 'lttb' as const,
  smooth: false,
  lineStyle: {
    width: 2
  },
  areaStyle: {
    opacity: 0.1
  },
  progressive: 500,
  progressiveThreshold: 3000
}

/**
 * Confidence interval band configuration
 * Use stacked area for RUL prediction confidence
 */
export function createConfidenceBandSeries(lowerName: string, upperName: string, color: string): EChartsOption {
  return {
    series: [
      {
        name: upperName,
        type: 'line',
        stack: 'confidence',
        symbol: 'none',
        sampling: 'lttb',
        lineStyle: { opacity: 0 },
        areaStyle: {
          color: color,
          opacity: 0.15
        },
        emphasis: { disabled: true }
      },
      {
        name: lowerName,
        type: 'line',
        stack: 'confidence',
        symbol: 'none',
        sampling: 'lttb',
        lineStyle: { opacity: 0 },
        areaStyle: {
          color: '#0F172A',
          opacity: 1
        },
        emphasis: { disabled: true }
      }
    ]
  }
}

/**
 * Status color mapping for enterprise dashboard
 */
export const statusColors = {
  healthy: '#22C55E',
  warning: '#F59E0B',
  critical: '#EF4444',
  offline: '#64748B'
} as const

/**
 * Get status color
 */
export function getStatusColor(status: keyof typeof statusColors): string {
  return statusColors[status] || statusColors.offline
}

/**
 * Create a telemetry chart option with optimizations
 */
export function createTelemetryOption(params: {
  data: Array<[number, number]>
  color?: string
  yAxisLabel?: string
  min?: number
  max?: number
}): EChartsOption {
  return {
    ...streamingChartBase,
    xAxis: {
      ...streamingChartBase.xAxis,
      axisLabel: {
        ...(streamingChartBase.xAxis as any)?.axisLabel,
        formatter: '{HH}:{mm}:{ss}'
      }
    },
    yAxis: {
      ...streamingChartBase.yAxis,
      name: params.yAxisLabel,
      nameTextStyle: {
        color: '#94A3B8',
        fontSize: 11,
        padding: [0, 0, 0, 40]
      },
      min: params.min,
      max: params.max
    },
    series: [
      {
        ...telemetrySeriesConfig,
        name: 'Value',
        lineStyle: {
          color: params.color || '#38BDF8',
          width: 2
        },
        areaStyle: {
          color: params.color || '#38BDF8',
          opacity: 0.1
        },
        data: params.data
      }
    ]
  }
}

/**
 * Create RUL prediction chart with confidence bands
 */
export function createRULChartOption(params: {
  timestamps: number[]
  predictions: number[]
  lowerBound?: number[]
  upperBound?: number[]
}): EChartsOption {
  const series: any[] = []

  // Add confidence band if provided
  if (params.lowerBound && params.upperBound) {
    series.push(
      {
        name: 'Upper Bound',
        type: 'line',
        stack: 'confidence',
        symbol: 'none',
        lineStyle: { opacity: 0 },
        areaStyle: { color: '#6366F1', opacity: 0.15 },
        data: params.upperBound.map((v, i) => [params.timestamps[i], v])
      },
      {
        name: 'Lower Bound',
        type: 'line',
        stack: 'confidence',
        symbol: 'none',
        lineStyle: { opacity: 0 },
        areaStyle: { color: '#0F172A', opacity: 1 },
        data: params.lowerBound.map((v, i) => [params.timestamps[i], v])
      }
    )
  }

  // Add prediction line
  series.push({
    name: 'RUL Prediction',
    type: 'line',
    symbol: 'none',
    sampling: 'lttb',
    lineStyle: {
      color: '#3B82F6',
      width: 3
    },
    data: params.predictions.map((v, i) => [params.timestamps[i], v])
  })

  return {
    ...streamingChartBase,
    xAxis: {
      ...streamingChartBase.xAxis,
      name: 'Time'
    },
    yAxis: {
      ...streamingChartBase.yAxis,
      name: 'RUL (hours)',
      nameTextStyle: {
        color: '#94A3B8',
        fontSize: 11,
        padding: [0, 0, 0, 40]
      }
    },
    series,
    legend: {
      show: true,
      top: 0,
      textStyle: {
        color: '#94A3B8',
        fontSize: 11
      }
    }
  }
}

/**
 * SHAP contribution bar chart
 */
export function createSHAPChartOption(contributions: Array<{
  feature: string
  value: number
}>): EChartsOption {
  const sorted = [...contributions].sort((a, b) => b.value - a.value)
  const features = sorted.map(c => c.feature)
  const values = sorted.map(c => c.value)

  return {
    grid: {
      left: 120,
      right: 40,
      top: 10,
      bottom: 30
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'shadow' },
      backgroundColor: '#1E293B',
      borderColor: '#334155',
      textStyle: { color: '#E2E8F0' }
    },
    xAxis: {
      type: 'value',
      axisLine: { lineStyle: { color: '#334155' } },
      axisLabel: {
        color: '#64748B',
        fontFamily: 'JetBrains Mono, monospace',
        fontSize: 10
      },
      splitLine: { lineStyle: { color: '#1F2A3A', type: 'dashed' } }
    },
    yAxis: {
      type: 'category',
      data: features,
      axisLine: { lineStyle: { color: '#334155' } },
      axisLabel: {
        color: '#94A3B8',
        fontFamily: 'Inter, system-ui, sans-serif',
        fontSize: 11
      }
    },
    series: [
      {
        type: 'bar',
        data: values,
        itemStyle: {
          color: (params: any) => {
            return params.data >= 0 ? '#10B981' : '#F43F5E'
          },
          borderRadius: [0, 4, 4, 0]
        },
        barWidth: 16
      }
    ]
  }
}
