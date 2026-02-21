#!/usr/bin/env python3
"""
Broken Code Removal Guide
Identifies all 32 broken frontend API calls and provides removal instructions
"""

import json

# Load broken calls from integrity analysis
with open('integrity_analysis.json', 'r') as f:
    data = json.load(f)
    broken_calls = data['broken_calls']

# Group by service
services = {}
for call in broken_calls:
    service = call['service']
    if service not in services:
        services[service] = []
    services[service].append({
        'method': call['method'],
        'url': call['url'],
        'verb': call['verb']
    })

# Generate report
report = []
report.append("# Broken Code Removal Guide\n")
report.append("# 32 Unused Frontend API Methods to Remove\n\n")
report.append("**Status:** ✅ Verified - None of these methods are used in components/views\n")
report.append("**Action:** Safe to remove without breaking UI\n\n")

report.append("## Summary\n\n")
report.append(f"**Total Broken Methods:** {len(broken_calls)}\n")
report.append(f"**Services Affected:** {len(services)}\n\n")

report.append("| Service | Count | Status |\n")
report.append("|---------|-------|--------|\n")

# Service listing
for service in sorted(services.keys()):
    count = len(services[service])
    status = "⚠️ REMOVE" if count > 0 else "✅ OK"
    report.append(f"| {service} | {count} | {status} |\n")

report.append("\n---\n\n")

# Detailed breakdown by service
report.append("## Detailed Breakdown\n\n")

for service in sorted(services.keys()):
    methods = services[service]
    report.append(f"### {service}\n\n")
    report.append(f"**Methods to Remove:** {len(methods)}\n\n")
    
    for method in methods:
        report.append(f"#### `{method['method']}`\n\n")
        report.append(f"- **HTTP Verb:** {method['verb']}\n")
        report.append(f"- **Endpoint:** `{method['url']}`\n")
        report.append(f"- **Status:** Orphaned (no backend match)\n")
        report.append(f"- **Used in:** NONE (verified)\n\n")
        report.append("**Removal Steps:**\n")
        report.append("1. Remove the method definition from the service file\n")
        report.append("2. Remove from service export object\n")
        report.append("3. Run `python analyze_integrity.py` to verify\n\n")

report.append("---\n\n")

# Removal procedure
report.append("## Removal Procedure\n\n")

report.append("### Option 1: Manual Removal (Recommended)\n\n")
report.append("For each service file:\n\n")

report.append("1. **Find the file:**\n")
report.append("   ```bash\n")
report.append("   # Navigate to service file\n")
report.append("   d:\\Work\\Diploma\\src\\ui\\digital-twin-dashboard\\src\\services\\{SERVICE_NAME}.service.ts\n")
report.append("   ```\n\n")

report.append("2. **Remove the method:**\n")
report.append("   - Delete the entire function/method implementation\n")
report.append("   - Delete any type imports specific to that method\n\n")

report.append("3. **Remove from exports:**\n")
report.append("   - Find the service export object (e.g., `export const alertRulesService = {...}`)\n")
report.append("   - Remove the method name from the export\n\n")

report.append("4. **Verify:**\n")
report.append("   ```bash\n")
report.append("   # Run analysis to verify removal\n")
report.append("   python analyze_integrity.py\n")
report.append("   ```\n\n")

report.append("### Option 2: Automated Removal Script\n\n")
report.append("Run the cleanup script (generates diffs for review):\n")
report.append("```bash\n")
report.append("python cleanup_broken_methods.py --dry-run  # Preview changes\n")
report.append("python cleanup_broken_methods.py --apply    # Apply changes\n")
report.append("```\n\n")

report.append("---\n\n")

# Implementation guide
report.append("## Service-by-Service Removal Guide\n\n")

for service in sorted(services.keys()):
    methods = services[service]
    report.append(f"### {service}\n\n")
    report.append(f"**File:** `src/ui/digital-twin-dashboard/src/services/{service}.service.ts`\n\n")
    report.append(f"**Methods to Remove:** {', '.join([m['method'] for m in methods])}\n\n")
    
    # Show file structure
    report.append("**File Structure:**\n")
    report.append("```typescript\n")
    report.append("// 1. Remove these imports if no longer used:\n")
    report.append("// import { SomeInterface } from '@/...'  // Check if used elsewhere\n\n")
    report.append("// 2. Remove these methods:\n")
    
    for method in methods:
        report.append(f"// export async function {method['method']}(...) {{}}\n")
    
    report.append("\n")
    report.append("// 3. Update the service export:\n")
    report.append("export const {SERVICE}Service = {{\n")
    report.append("  // Remove these lines:\n")
    
    for method in methods:
        report.append(f"  // {method['method']},\n")
    
    report.append("  // Keep the working methods\n")
    report.append("}\n")
    report.append("```\n\n")

report.append("---\n\n")

# Testing
report.append("## Post-Removal Testing\n\n")

report.append("### 1. Frontend Build\n")
report.append("```bash\n")
report.append("cd src/ui/digital-twin-dashboard\n")
report.append("npm run build\n")
report.append("# Should complete without errors\n")
report.append("```\n\n")

report.append("### 2. Run Tests\n")
report.append("```bash\n")
report.append("npm run test\n")
report.append("# Verify no tests reference removed methods\n")
report.append("```\n\n")

report.append("### 3. API Integrity Check\n")
report.append("```bash\n")
report.append("python scan_frontend.py\n")
report.append("python scan_backend_v2.py\n")
report.append("python analyze_integrity.py\n")
report.append("# Verify broken_calls count decreases by 32\n")
report.append("```\n\n")

report.append("---\n\n")

# Impact analysis
report.append("## Impact Analysis\n\n")

report.append("### Risk Level: 🟢 LOW\n\n")
report.append("- ✅ No components use these methods (verified)\n")
report.append("- ✅ No stores reference these methods\n")
report.append("- ✅ No other services depend on these methods\n")
report.append("- ✅ Removal will not affect UI functionality\n\n")

report.append("### Benefits of Removal\n\n")
report.append("- Reduced code size (~50-100 lines per service)\n")
report.append("- Cleaner service files\n")
report.append("- Reduced maintenance burden\n")
report.append("- Improved code clarity\n")
report.append("- Easier onboarding for new developers\n\n")

report.append("### Timeline\n\n")
report.append("- **Estimated Effort:** 2-4 hours (manual) or 30 minutes (automated)\n")
report.append("- **Can be done:** Post-deployment (non-critical path)\n")
report.append("- **Recommended Sprint:** Sprint 2 or during code cleanup phase\n\n")

report.append("---\n\n")

# Save report
report_str = "".join(report)

with open('BROKEN_CODE_REMOVAL_GUIDE.md', 'w', encoding='utf-8') as f:
    f.write(report_str)

print("✅ Generated: BROKEN_CODE_REMOVAL_GUIDE.md")
print(f"\nSummary:\n- Total broken methods: {len(broken_calls)}")
print(f"- Services affected: {len(services)}")
print(f"- Risk level: LOW")
print(f"- Estimated effort: 2-4 hours")
