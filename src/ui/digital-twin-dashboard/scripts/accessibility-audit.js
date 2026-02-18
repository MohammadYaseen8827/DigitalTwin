#!/usr/bin/env node

/**
 * Accessibility Audit Script
 * Performs comprehensive WCAG 2.1 AA compliance testing
 */

const pa11y = require('pa11y');
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
    'http://localhost:5173/login',
    'http://localhost:5173/machines',
    'http://localhost:5173/maintenance',
    'http://localhost:5173/telemetry',
    'http://localhost:5173/analytics',
    'http://localhost:5173/alerts',
    'http://localhost:5173/reports'
  ],
  pa11yConfig: {
    standard: 'WCAG2AA',
    runners: ['axe'],
    timeout: 30000,
    wait: 1000,
    screenCapture: './accessibility-screenshots/',
    ignore: [
      'notice',
      'warning'
    ],
    hideElements: '.ad-banner, .cookie-banner',
    rootElement: 'body',
    rules: [
      'color-contrast',
      'image-alt',
      'label',
      'link-name',
      'button-name',
      'html-has-lang',
      'document-title',
      'bypass',
      'heading-order',
      'nested-interactive',
      'aria-allowed-attr',
      'aria-required-attr',
      'aria-valid-attr-value',
      'aria-roles'
    ]
  }
};

class AccessibilityAuditor {
  constructor() {
    this.results = [];
    this.summary = {
      totalIssues: 0,
      criticalIssues: 0,
      seriousIssues: 0,
      moderateIssues: 0,
      minorIssues: 0,
      passedUrls: 0,
      failedUrls: 0
    };
  }

  async runAudit() {
    console.log('🔍 Starting Accessibility Audit...\n');
    
    // Ensure screenshots directory exists
    const screenshotsDir = path.join(__dirname, '..', 'accessibility-screenshots');
    if (!fs.existsSync(screenshotsDir)) {
      fs.mkdirSync(screenshotsDir, { recursive: true });
    }

    for (const url of CONFIG.urls) {
      console.log(`🧪 Testing: ${url}`);
      await this.testUrl(url);
    }

    this.generateReport();
    this.printSummary();
  }

  async testUrl(url) {
    try {
      const result = await pa11y(url, {
        ...CONFIG.pa11yConfig,
        screenCapture: safePathJoin(__dirname, '..', 'accessibility-screenshots', `${this.slugify(url)}.png`)
      });

      const issues = this.categorizeIssues(result.issues);
      
      this.results.push({
        url,
        issues,
        totalIssues: result.issues.length,
        passed: result.issues.length === 0
      });

      this.updateSummary(issues, result.issues.length === 0);

      this.printUrlResults(url, issues, result.issues.length);
      
    } catch (error) {
      console.error(`❌ Error testing ${url}:`, error.message);
      this.results.push({
        url,
        error: error.message,
        issues: { critical: [], serious: [], moderate: [], minor: [] },
        totalIssues: 0,
        passed: false
      });
      this.summary.failedUrls++;
    }
  }

  categorizeIssues(issues) {
    const categorized = {
      critical: [],
      serious: [],
      moderate: [],
      minor: []
    };

    issues.forEach(issue => {
      switch (issue.type) {
        case 'error':
          categorized.critical.push(issue);
          break;
        case 'serious':
          categorized.serious.push(issue);
          break;
        case 'moderate':
        case 'warning':
          categorized.moderate.push(issue);
          break;
        case 'minor':
        case 'notice':
          categorized.minor.push(issue);
          break;
      }
    });

    return categorized;
  }

  updateSummary(issues, passed) {
    this.summary.totalIssues += 
      issues.critical.length + 
      issues.serious.length + 
      issues.moderate.length + 
      issues.minor.length;
    
    this.summary.criticalIssues += issues.critical.length;
    this.summary.seriousIssues += issues.serious.length;
    this.summary.moderateIssues += issues.moderate.length;
    this.summary.minorIssues += issues.minor.length;

    if (passed) {
      this.summary.passedUrls++;
    } else {
      this.summary.failedUrls++;
    }
  }

  printUrlResults(url, issues, totalIssues) {
    const status = totalIssues === 0 ? '✅ PASSED' : '❌ FAILED';
    console.log(`   ${status} - ${totalIssues} issues found`);
    
    if (totalIssues > 0) {
      console.log(`   🔴 Critical: ${issues.critical.length}`);
      console.log(`   🟠 Serious: ${issues.serious.length}`);
      console.log(`   🟡 Moderate: ${issues.moderate.length}`);
      console.log(`   🟢 Minor: ${issues.minor.length}`);
    }
    console.log('');
  }

  printSummary() {
    console.log('\n📊 ACCESSIBILITY AUDIT SUMMARY');
    console.log('================================');
    console.log(`Total URLs Tested: ${CONFIG.urls.length}`);
    console.log(`Passed URLs: ${this.summary.passedUrls}`);
    console.log(`Failed URLs: ${this.summary.failedUrls}`);
    console.log(``);
    console.log(`Total Issues Found: ${this.summary.totalIssues}`);
    console.log(`🔴 Critical Issues: ${this.summary.criticalIssues}`);
    console.log(`🟠 Serious Issues: ${this.summary.seriousIssues}`);
    console.log(`🟡 Moderate Issues: ${this.summary.moderateIssues}`);
    console.log(`🟢 Minor Issues: ${this.summary.minorIssues}`);
    console.log(``);
    
    const complianceRate = ((this.summary.passedUrls / CONFIG.urls.length) * 100).toFixed(1);
    console.log(`🎯 WCAG 2.1 AA Compliance Rate: ${complianceRate}%`);
    
    if (this.summary.criticalIssues === 0 && this.summary.seriousIssues === 0) {
      console.log(`🏆 Excellent! No critical or serious accessibility issues found.`);
    } else {
      console.log(`⚠️  Action required: Critical or serious issues need immediate attention.`);
    }
  }

  generateReport() {
    const report = {
      timestamp: new Date().toISOString(),
      configuration: CONFIG,
      summary: this.summary,
      detailedResults: this.results,
      recommendations: this.generateRecommendations()
    };

    const reportPath = safePathJoin(__dirname, '..', 'accessibility-report.json');
    fs.writeFileSync(reportPath, JSON.stringify(report, null, 2));

    // Also generate HTML report
    this.generateHtmlReport(report);
  }

  generateRecommendations() {
    const recommendations = [];

    if (this.summary.criticalIssues > 0) {
      recommendations.push({
        priority: 'HIGH',
        description: 'Address critical accessibility issues immediately',
        actionItems: [
          'Fix missing alternative text for images',
          'Ensure proper form labeling',
          'Implement keyboard navigation',
          'Add skip navigation links'
        ]
      });
    }

    if (this.summary.seriousIssues > 0) {
      recommendations.push({
        priority: 'MEDIUM',
        description: 'Address serious accessibility issues in next sprint',
        actionItems: [
          'Improve color contrast ratios',
          'Fix heading structure',
          'Ensure ARIA attributes are valid',
          'Implement proper focus management'
        ]
      });
    }

    if (this.summary.moderateIssues > 0) {
      recommendations.push({
        priority: 'LOW',
        description: 'Address moderate issues for improved UX',
        actionItems: [
          'Enhance link and button descriptions',
          'Improve landmark regions',
          'Add language attributes',
          'Optimize tab order'
        ]
      });
    }

    return recommendations;
  }

  generateHtmlReport(report) {
    const html = `
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Accessibility Audit Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background: #f5f5f5; padding: 20px; border-radius: 5px; }
        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin: 20px 0; }
        .metric { background: white; padding: 15px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        .metric h3 { margin: 0 0 10px 0; }
        .metric .value { font-size: 2em; font-weight: bold; }
        .results { margin: 20px 0; }
        .url-result { background: white; margin: 10px 0; padding: 15px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        .url-result.passed { border-left: 5px solid #4CAF50; }
        .url-result.failed { border-left: 5px solid #f44336; }
        .issues { margin-top: 10px; }
        .issue-category { margin: 5px 0; }
        .critical { color: #f44336; }
        .serious { color: #FF9800; }
        .moderate { color: #FFEB3B; }
        .minor { color: #4CAF50; }
    </style>
</head>
<body>
    <div class="header">
        <h1>Accessibility Audit Report</h1>
        <p>Generated: ${report.timestamp}</p>
        <p>Standards: WCAG 2.1 AA</p>
    </div>

    <div class="summary">
        <div class="metric">
            <h3>Total URLs</h3>
            <div class="value">${report.summary.passedUrls + report.summary.failedUrls}</div>
        </div>
        <div class="metric">
            <h3>Passed</h3>
            <div class="value" style="color: #4CAF50;">${report.summary.passedUrls}</div>
        </div>
        <div class="metric">
            <h3>Failed</h3>
            <div class="value" style="color: #f44336;">${report.summary.failedUrls}</div>
        </div>
        <div class="metric">
            <h3>Compliance Rate</h3>
            <div class="value">${((report.summary.passedUrls / (report.summary.passedUrls + report.summary.failedUrls)) * 100).toFixed(1)}%</div>
        </div>
    </div>

    <div class="results">
        <h2>Detailed Results</h2>
        ${report.detailedResults.map(result => `
            <div class="url-result ${result.passed ? 'passed' : 'failed'}">
                <h3>${result.url}</h3>
                ${result.passed ? 
                    '<p>✅ No accessibility issues found</p>' : 
                    `<div class="issues">
                        <div class="issue-category critical">🔴 Critical: ${result.issues.critical.length}</div>
                        <div class="issue-category serious">🟠 Serious: ${result.issues.serious.length}</div>
                        <div class="issue-category moderate">🟡 Moderate: ${result.issues.moderate.length}</div>
                        <div class="issue-category minor">🟢 Minor: ${result.issues.minor.length}</div>
                    </div>`
                }
            </div>
        `).join('')}
    </div>
</body>
</html>`;

    const htmlReportPath = path.join(__dirname, '..', 'accessibility-report.html');
    fs.writeFileSync(htmlReportPath, html);
  }

  slugify(text) {
    return text
      .toLowerCase()
      .replace(/[^\w\s-]/g, '')
      .replace(/[\s_-]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }
}

// Run the accessibility audit
async function run() {
  const auditor = new AccessibilityAuditor();
  try {
    await auditor.runAudit();
  } catch (error) {
    console.error('❌ Accessibility audit failed:', error);
    process.exit(1);
  }
}

// Export for use in other scripts
module.exports = AccessibilityAuditor;

// Run if called directly
if (require.main === module) {
  run();
}