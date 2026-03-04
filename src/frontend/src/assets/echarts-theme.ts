export const echartsTheme = {
  color: [
    '#3B82F6', // primary blue
    '#10B981', // success green
    '#F59E0B', // warning amber
    '#EF4444', // danger red
    '#06B6D4', // telemetry cyan
    '#8B5CF6', // violet
    '#F472B6'  // pink
  ],
  backgroundColor: 'transparent',
  textStyle: {
    fontFamily: 'Inter, system-ui, sans-serif',
    color: '#94A3B8'
  },
  title: {
    textStyle: {
      color: '#F8FAFC',
      fontWeight: 700,
      fontSize: 14
    }
  },
  line: {
    itemStyle: {
      borderWidth: 2
    },
    lineStyle: {
      width: 3
    },
    symbolSize: 0,
    symbol: 'circle',
    smooth: true
  },
  bar: {
    itemStyle: {
      barBorderRadius: [4, 4, 0, 0]
    }
  },
  categoryAxis: {
    axisLine: {
      show: true,
      lineStyle: { color: 'rgba(255, 255, 255, 0.1)' }
    },
    axisTick: {
      show: false
    },
    axisLabel: {
      color: '#64748B',
      fontSize: 10
    },
    splitLine: {
      show: false
    }
  },
  valueAxis: {
    axisLine: {
      show: false
    },
    axisTick: {
      show: false
    },
    axisLabel: {
      color: '#64748B',
      fontSize: 10
    },
    splitLine: {
      show: true,
      lineStyle: { color: 'rgba(255, 255, 255, 0.05)', type: 'dashed' }
    }
  },
  tooltip: {
    backgroundColor: 'rgba(5, 7, 10, 0.9)',
    borderColor: 'rgba(255, 255, 255, 0.1)',
    borderWidth: 1,
    padding: [8, 12],
    textStyle: {
      color: '#F8FAFC',
      fontSize: 12
    },
    axisPointer: {
      lineStyle: {
        color: 'rgba(59, 130, 246, 0.5)',
        width: 1
      },
      crossStyle: {
        color: 'rgba(59, 130, 246, 0.5)',
        width: 1
      }
    }
  },
  legend: {
    textStyle: {
      color: '#94A3B8',
      fontSize: 11
    },
    itemGap: 20,
    itemWidth: 10,
    itemHeight: 10
  }
}
