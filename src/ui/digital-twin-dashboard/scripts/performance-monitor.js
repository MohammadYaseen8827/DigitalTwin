#!/usr/bin/env node

/**
 * Performance Monitoring Script
 * Monitors key performance metrics for the Digital Twin Dashboard
 */

const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

// Safe path resolution utility
function safePathJoin(baseDir, ...segments) {
  // Normalize and resolve the path
  const resolvedPath = path.resolve(baseDir, ...segments);
  
  // Ensure the resolved path is within the base directory
  if (!resolvedPath.startsWith(path.resolve(baseDir))) {
    throw new Error('Path traversal detected');
  }
  
  return resolvedPath;
}

// Configuration
const CONFIG = {
  urls: [
    'http://localhost:5173/',
    'http://localhost:5173/machines',
    'http://localhost:5173/maintenance',
    'http://localhost:5173/telemetry',
    'http://localhost:5173/analytics'
  ],
  thresholds: {
    pageLoadTime: 2000, // ms
    firstContentfulPaint: 1000, // ms
    largestContentfulPaint: 2500, // ms
    cumulativeLayoutShift: 0.1,
    totalBlockingTime: 300 // ms
  },
  iterations: 3,
  userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
};

class PerformanceMonitor {
  constructor() {
    this.results = [];
    this.browser = null;
  }

  async initialize() {
    this.browser = await puppeteer.launch({
      headless: true,
      args: [
        '--no-sandbox',
        '--disable-setuid-sandbox',
        '--disable-dev-shm-usage',
        '--disable-accelerated-2d-canvas',
        '--no-first-run',
        '--no-zygote',
        '--disable-gpu'
      ]
    });
  }

  async measurePerformance(url) {
    const page = await this.browser.newPage();
    
    try {
      // Set user agent
      await page.setUserAgent(CONFIG.userAgent);
      
      // Enable performance metrics
      await page.evaluateOnNewDocument(() => {
        // Override console.time to capture timing
        const originalTime = console.time;
        const originalTimeEnd = console.timeEnd;
        window.performanceMetrics = {};
        
        console.time = (label) => {
          window.performanceMetrics[label] = performance.now();
          originalTime.call(console, label);
        };
        
        console.timeEnd = (label) => {
          if (window.performanceMetrics[label]) {
            window.performanceMetrics[label] = performance.now() - window.performanceMetrics[label];
          }
          originalTimeEnd.call(console, label);
        };
      });

      // Navigate to page
      const startTime = Date.now();
      await page.goto(url, { waitUntil: 'networkidle0' });
      const loadTime = Date.now() - startTime;

      // Get performance metrics
      const metrics = await page.evaluate(() => {
        const perfEntries = performance.getEntriesByType('navigation')[0];
        const paintEntries = performance.getEntriesByType('paint');
        
        return {
          pageLoadTime: perfEntries.loadEventEnd - perfEntries.fetchStart,
          domContentLoaded: perfEntries.domContentLoadedEventEnd - perfEntries.fetchStart,
          firstContentfulPaint: paintEntries.find(entry => entry.name === 'first-contentful-paint')?.startTime || 0,
          largestContentfulPaint: paintEntries.find(entry => entry.name === 'largest-contentful-paint')?.startTime || 0,
          cumulativeLayoutShift: 0, // Would need more complex measurement
          totalBlockingTime: 0, // Would need more complex calculation
          resourceCount: performance.getEntriesByType('resource').length,
          memoryUsage: performance.memory ? {
            usedJSHeapSize: performance.memory.usedJSHeapSize,
            totalJSHeapSize: performance.memory.totalJSHeapSize,
            jsHeapSizeLimit: performance.memory.jsHeapSizeLimit
          } : null
        };
      });

      // Take screenshot
      const screenshotPath = safePathJoin(__dirname, 'screenshots', `${Date.now()}-${encodeURIComponent(url)}.png`);
      await page.screenshot({ path: screenshotPath, fullPage: true });

      return {
        url,
        timestamp: new Date().toISOString(),
        loadTime,
        metrics,
        screenshot: screenshotPath
      };

    } finally {
      await page.close();
    }
  }

  async runAnalysis() {
    await this.initialize();
    
    console.log('🚀 Starting Performance Analysis...');
    
    for (const url of CONFIG.urls) {
      console.log(`\n🔍 Analyzing: ${url}`);
      
      const urlResults = [];
      for (let i = 0; i < CONFIG.iterations; i++) {
        console.log(`   Iteration ${i + 1}/${CONFIG.iterations}`);
        const result = await this.measurePerformance(url);
        urlResults.push(result);
      }
      
      // Calculate averages
      const averages = this.calculateAverages(urlResults);
      this.results.push({
        url,
        results: urlResults,
        averages,
        passed: this.checkThresholds(averages)
      });
      
      this.printResults(url, averages);
    }
    
    await this.browser.close();
    this.generateReport();
  }

  calculateAverages(results) {
    const metricsSum = {
      pageLoadTime: 0,
      domContentLoaded: 0,
      firstContentfulPaint: 0,
      largestContentfulPaint: 0,
      resourceCount: 0
    };

    results.forEach(result => {
      metricsSum.pageLoadTime += result.loadTime;
      metricsSum.domContentLoaded += result.metrics.domContentLoaded;
      metricsSum.firstContentfulPaint += result.metrics.firstContentfulPaint;
      metricsSum.largestContentfulPaint += result.metrics.largestContentfulPaint;
      metricsSum.resourceCount += result.metrics.resourceCount;
    });

    const count = results.length;
    return {
      pageLoadTime: metricsSum.pageLoadTime / count,
      domContentLoaded: metricsSum.domContentLoaded / count,
      firstContentfulPaint: metricsSum.firstContentfulPaint / count,
      largestContentfulPaint: metricsSum.largestContentfulPaint / count,
      resourceCount: Math.round(metricsSum.resourceCount / count)
    };
  }

  checkThresholds(averages) {
    return {
      pageLoadTime: averages.pageLoadTime <= CONFIG.thresholds.pageLoadTime,
      firstContentfulPaint: averages.firstContentfulPaint <= CONFIG.thresholds.firstContentfulPaint,
      largestContentfulPaint: averages.largestContentfulPaint <= CONFIG.thresholds.largestContentfulPaint
    };
  }

  printResults(url, averages) {
    const passed = this.checkThresholds(averages);
    
    console.log(`\n📊 Performance Results for ${url}:`);
    console.log(`   Page Load Time: ${averages.pageLoadTime.toFixed(2)}ms ${passed.pageLoadTime ? '✅' : '❌'}`);
    console.log(`   First Contentful Paint: ${averages.firstContentfulPaint.toFixed(2)}ms ${passed.firstContentfulPaint ? '✅' : '❌'}`);
    console.log(`   Largest Contentful Paint: ${averages.largestContentfulPaint.toFixed(2)}ms ${passed.largestContentfulPaint ? '✅' : '❌'}`);
    console.log(`   Resources Loaded: ${averages.resourceCount}`);
  }

  generateReport() {
    const report = {
      timestamp: new Date().toISOString(),
      configuration: CONFIG,
      results: this.results,
      summary: {
        totalUrls: CONFIG.urls.length,
        passedUrls: this.results.filter(r => Object.values(r.passed).every(Boolean)).length,
        failedUrls: this.results.filter(r => !Object.values(r.passed).every(Boolean)).length
      }
    };

    const reportPath = safePathJoin(__dirname, 'performance-report.json');
    fs.writeFileSync(reportPath, JSON.stringify(report, null, 2));
    
    console.log(`\n📋 Performance Report Generated: ${reportPath}`);
    console.log(`✅ Passed: ${report.summary.passedUrls}/${report.summary.totalUrls} URLs`);
    console.log(`❌ Failed: ${report.summary.failedUrls}/${report.summary.totalUrls} URLs`);
  }
}

// Run the performance monitor
async function run() {
  const monitor = new PerformanceMonitor();
  try {
    await monitor.runAnalysis();
  } catch (error) {
    console.error('❌ Performance monitoring failed:', error);
    process.exit(1);
  }
}

// Export for use in other scripts
module.exports = PerformanceMonitor;

// Run if called directly
if (require.main === module) {
  run();
}