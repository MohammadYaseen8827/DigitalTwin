#!/usr/bin/env node

/**
 * Security Vulnerability Assessment Script
 * Performs comprehensive security scanning and vulnerability assessment
 */

const { spawn } = require('child_process');
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
  targets: [
    'http://localhost:5173',  // Frontend
    'http://localhost:7300'   // Backend API
  ],
  scanTypes: {
    nmap: true,
    nikto: true,
    zap: true,
    ssl: true
  },
  outputDir: './security-reports',
  timeout: 300000 // 5 minutes
};

class SecurityScanner {
  constructor() {
    this.results = {
      timestamp: new Date().toISOString(),
      targets: CONFIG.targets,
      scans: {}
    };

    // Create output directory
    if (!fs.existsSync(CONFIG.outputDir)) {
      fs.mkdirSync(CONFIG.outputDir, { recursive: true });
    }
  }

  async runFullScan() {
    console.log('🔒 Starting Security Vulnerability Assessment...\n');

    for (const target of CONFIG.targets) {
      console.log(`🔍 Scanning target: ${target}`);
      this.results.scans[target] = {};

      if (CONFIG.scanTypes.nmap) {
        await this.runNmapScan(target);
      }

      if (CONFIG.scanTypes.nikto && target.includes('http')) {
        await this.runNiktoScan(target);
      }

      if (CONFIG.scanTypes.ssl && target.includes('https')) {
        await this.runSSLScan(target);
      }
    }

    // Run ZAP scan for comprehensive web application security
    if (CONFIG.scanTypes.zap) {
      await this.runZAPScan();
    }

    this.generateReport();
    this.printSummary();
  }

  async runNmapScan(target) {
    console.log('   📡 Running Nmap port scan...');

    return new Promise((resolve) => {
      const host = target.replace('http://', '').replace('https://', '').split(':')[0];
      const nmap = spawn('nmap', ['-sV', '-O', '--script', 'vuln', host]);

      let output = '';
      let errors = '';

      nmap.stdout.on('data', (data) => {
        output += data.toString();
      });

      nmap.stderr.on('data', (data) => {
        errors += data.toString();
      });

      nmap.on('close', (code) => {
        this.results.scans[target].nmap = {
          output,
          errors,
          exitCode: code
        };

        // Save detailed output
        const outputPath = safePathJoin(CONFIG.outputDir, `nmap-${this.sanitizeFilename(target)}.txt`);
        fs.writeFileSync(outputPath, output);

        console.log(`   📡 Nmap scan completed for ${target}`);
        resolve();
      });

      // Timeout after configured time
      setTimeout(() => {
        nmap.kill();
        console.log(`   ⏰ Nmap scan timed out for ${target}`);
        resolve();
      }, CONFIG.timeout);
    });
  }

  async runNiktoScan(target) {
    console.log('   🕵️ Running Nikto web scanner...');

    return new Promise((resolve) => {
      const nikto = spawn('nikto', ['-h', target, '-C', 'all']);

      let output = '';
      let errors = '';

      nikto.stdout.on('data', (data) => {
        output += data.toString();
      });

      nikto.stderr.on('data', (data) => {
        errors += data.toString();
      });

      nikto.on('close', (code) => {
        this.results.scans[target].nikto = {
          output,
          errors,
          exitCode: code
        };

        // Save detailed output
        const outputPath = safePathJoin(CONFIG.outputDir, `nikto-${this.sanitizeFilename(target)}.txt`);
        fs.writeFileSync(outputPath, output);

        console.log(`   🕵️ Nikto scan completed for ${target}`);
        resolve();
      });

      // Timeout after configured time
      setTimeout(() => {
        nikto.kill();
        console.log(`   ⏰ Nikto scan timed out for ${target}`);
        resolve();
      }, CONFIG.timeout);
    });
  }

  async runSSLScan(target) {
    console.log('   🔐 Running SSL/TLS security scan...');

    return new Promise((resolve) => {
      const host = target.replace('https://', '').split(':')[0];
      const port = target.includes(':') ? target.split(':')[2] : '443';

      const sslscan = spawn('sslscan', [`${host}:${port}`]);

      let output = '';
      let errors = '';

      sslscan.stdout.on('data', (data) => {
        output += data.toString();
      });

      sslscan.stderr.on('data', (data) => {
        errors += data.toString();
      });

      sslscan.on('close', (code) => {
        this.results.scans[target].ssl = {
          output,
          errors,
          exitCode: code
        };

        // Save detailed output
        const outputPath = safePathJoin(CONFIG.outputDir, `sslscan-${this.sanitizeFilename(target)}.txt`);
        fs.writeFileSync(outputPath, output);

        console.log(`   🔐 SSL scan completed for ${target}`);
        resolve();
      });

      // Timeout after configured time
      setTimeout(() => {
        sslscan.kill();
        console.log(`   ⏰ SSL scan timed out for ${target}`);
        resolve();
      }, CONFIG.timeout);
    });
  }

  async runZAPScan() {
    console.log('   🕷️ Running OWASP ZAP web application scan...');

    return new Promise((resolve) => {
      // Check if ZAP is installed and running
      const zap = spawn('zap-cli', ['quick-scan', 'http://localhost:5173']);

      let output = '';
      let errors = '';

      zap.stdout.on('data', (data) => {
        output += data.toString();
      });

      zap.stderr.on('data', (data) => {
        errors += data.toString();
      });

      zap.on('close', (code) => {
        this.results.scans.webapp = {
          zap: {
            output,
            errors,
            exitCode: code
          }
        };

        // Save detailed output
        const outputPath = path.join(CONFIG.outputDir, 'zap-scan.txt');
        fs.writeFileSync(outputPath, output);

        console.log('   🕷️ ZAP scan completed');
        resolve();
      });

      // Timeout after configured time
      setTimeout(() => {
        zap.kill();
        console.log('   ⏰ ZAP scan timed out');
        resolve();
      }, CONFIG.timeout * 2); // Longer timeout for ZAP
    });
  }

  sanitizeFilename(filename) {
    return filename
      .replace(/[:\/]/g, '-')
      .replace(/[^\w\-\.]/g, '_');
  }

  generateReport() {
    // Save JSON report
    const jsonReportPath = safePathJoin(CONFIG.outputDir, 'security-assessment.json');
    fs.writeFileSync(jsonReportPath, JSON.stringify(this.results, null, 2));

    // Generate HTML report
    this.generateHtmlReport();
  }

  generateHtmlReport() {
    const html = `
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Security Vulnerability Assessment Report</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 8px; margin-bottom: 20px; }
        .header h1 { margin: 0; font-size: 2em; }
        .header p { margin: 10px 0 0 0; opacity: 0.9; }
        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin: 20px 0; }
        .card { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); border-left: 4px solid #667eea; }
        .card h3 { margin: 0 0 15px 0; color: #333; }
        .vulnerability { background: #fff5f5; border-left: 4px solid #e53e3e; margin: 10px 0; padding: 15px; border-radius: 4px; }
        .info { background: #f0fff4; border-left: 4px solid #38a169; margin: 10px 0; padding: 15px; border-radius: 4px; }
        .warning { background: #fffbeb; border-left: 4px solid #d69e2e; margin: 10px 0; padding: 15px; border-radius: 4px; }
        .recommendations { background: #ebf8ff; border-left: 4px solid #3182ce; margin: 20px 0; padding: 20px; border-radius: 8px; }
        .recommendations h3 { margin-top: 0; color: #2c5282; }
        .recommendations ul { padding-left: 20px; }
        .recommendations li { margin: 8px 0; }
        pre { background: #f7fafc; padding: 15px; border-radius: 4px; overflow-x: auto; font-size: 0.9em; }
        .severity-high { color: #e53e3e; font-weight: bold; }
        .severity-medium { color: #d69e2e; font-weight: bold; }
        .severity-low { color: #38a169; font-weight: bold; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>🔒 Security Vulnerability Assessment Report</h1>
            <p>Generated: ${this.results.timestamp}</p>
            <p>Targets Scanned: ${CONFIG.targets.length}</p>
        </div>

        <div class="summary">
            <div class="card">
                <h3>📊 Scan Summary</h3>
                <p><strong>Targets:</strong> ${CONFIG.targets.length}</p>
                <p><strong>Scan Types:</strong> ${Object.keys(CONFIG.scanTypes).filter(k => CONFIG.scanTypes[k]).length}</p>
                <p><strong>Reports Generated:</strong> ${Object.keys(this.results.scans).length}</p>
            </div>

            <div class="card">
                <h3>🛡️ Security Posture</h3>
                <p>This assessment identifies potential security vulnerabilities and provides recommendations for remediation.</p>
            </div>
        </div>

        <div class="recommendations">
            <h3>📋 Security Recommendations</h3>
            <ul>
                <li>Implement proper input validation and sanitization</li>
                <li>Enforce strong authentication and authorization controls</li>
                <li>Regularly update dependencies and patch known vulnerabilities</li>
                <li>Implement Content Security Policy (CSP) headers</li>
                <li>Enable HTTP Strict Transport Security (HSTS)</li>
                <li>Configure proper CORS policies</li>
                <li>Implement rate limiting for API endpoints</li>
                <li>Use secure session management</li>
                <li>Encrypt sensitive data in transit and at rest</li>
                <li>Conduct regular security audits and penetration testing</li>
            </ul>
        </div>

        <div class="info">
            <h3>ℹ️ Next Steps</h3>
            <p>Review the detailed scan results in the individual report files:</p>
            <ul>
                <li>Nmap port scan results: nmap-*.txt</li>
                <li>Nikto web scanner results: nikto-*.txt</li>
                <li>SSL/TLS scan results: sslscan-*.txt</li>
                <li>ZAP web application scan: zap-scan.txt</li>
                <li>Complete JSON report: security-assessment.json</li>
            </ul>
        </div>
    </div>
</body>
</html>`;

    const htmlReportPath = safePathJoin(CONFIG.outputDir, 'security-report.html');
    fs.writeFileSync(htmlReportPath, html);
  }

  printSummary() {
    console.log('\n SECURITY ASSESSMENT SUMMARY');
    console.log('================================');
    console.log(`Targets Scanned: ${CONFIG.targets.length}`);
    console.log(`Scan Types Performed: ${Object.keys(CONFIG.scanTypes).filter(k => CONFIG.scanTypes[k]).length}`);
    console.log(`Reports Generated: ${Object.keys(this.results.scans).length}`);
    console.log(`Output Directory: ${CONFIG.outputDir}`);
    console.log('');
    console.log(' Key Security Areas Checked:');
    console.log('   • Port scanning and service detection');
    console.log('   • Web application vulnerability scanning');
    console.log('   • SSL/TLS configuration assessment');
    console.log('   • Common security misconfigurations');
    console.log('   • Known vulnerability databases');
    console.log('');
    console.log('  Important: Review detailed reports for specific findings and remediation steps.');
  }
}

// Run the security scanner
async function run() {
  const scanner = new SecurityScanner();
  try {
    await scanner.runFullScan();
  } catch (error) {
    console.error('❌ Security scanning failed:', error);
    process.exit(1);
  }
}

// Export for use in other scripts
module.exports = SecurityScanner;

// Run if called directly
if (require.main === module) {
  run();
}
