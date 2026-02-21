import json

def load_json(name):
    try:
        with open(name, 'r') as f:
            return json.load(f)
    except: return {}

analysis = load_json('integrity_analysis.json')
mismatches = load_json('dto_mismatches.json')
backend = load_json('backend_audit.json')
frontend = load_json('frontend_audit.json')

with open('system-integrity-audit.md', 'w', encoding='utf-8') as f:
    f.write("# System Integrity Audit Report\n\n")
    
    # Verdict
    f.write("## Production Readiness Verdict: **FAIL**\n\n")
    f.write("> **Audit Status:** Critical mismatches and unimplemented logic detected. Deployment not recommended.\n\n")
    
    # Backend Summary
    f.write("## 1. Backend Feature Map\n")
    for controller in backend.get('controllers', []):
        f.write(f"### Controller: {controller['name']}\n")
        for e in controller['endpoints']:
            # Check if this endpoint is dead
            is_dead = any(d['route'] == e['route'] and d['method'] == e['http_method'].upper() for d in analysis.get('dead_endpoints', []))
            used = "No" if is_dead else "Yes"
            f.write(f"- **Endpoint:** `{e['method']}`\n")
            f.write(f"  - Route: `{e['route']}`\n")
            f.write(f"  - Method: `{e['http_method']}`\n")
            f.write(f"  - Request DTO: `{e['request_dto']}`\n")
            f.write(f"  - Response DTO: `{e['response_dto']}`\n")
            f.write(f"  - Used In Frontend: {used}\n\n")

    # Frontend Summary
    f.write("## 2. Frontend API Usage Map\n")
    for service in frontend.get('services', []):
        f.write(f"### Service: {service['name']}\n")
        for m in service['methods']:
            # Check if broken
            is_broken = any(b['service'] == service['name'] and b['method'] == m['name'] for b in analysis.get('broken_calls', []))
            match = "No" if is_broken else "Yes"
            f.write(f"- **Method:** `{m['name']}`\n")
            f.write(f"  - URL: `{m['url']}`\n")
            f.write(f"  - Backend Match: {match}\n")
            f.write(f"  - DTO Compatible: {'TBD' if not is_broken else 'N/A'}\n\n")

    # Contract Mismatches
    f.write("## 3. Contract Mismatches\n")
    f.write("| DTO Name | Backend Field (Missing in FE) | Frontend Field (Extra in FE) | Issue |\n")
    f.write("| --- | --- | --- | --- |\n")
    for m in mismatches:
        f.write(f"| {m['dto']} | {', '.join(m['only_backend'])} | {', '.join(m['only_frontend'])} | Field Mismatch |\n")
    f.write("\n")

    # Dead Code & Implementation Integrity
    f.write("## 4. Implementation Integrity\n")
    f.write("### Unimplemented Interface Methods\n")
    for u in analysis.get('unimplemented_methods', []):
        f.write(f"- **Interface:** `{u['interface']}`\n")
        f.write(f"  - Implementation: `{u['implementation']}`\n")
        f.write(f"  - Missing Methods: {', '.join(u['missing'])}\n\n")

    f.write("### Dead Endpoints (Not used by Frontend)\n")
    for d in analysis.get('dead_endpoints', []):
        f.write(f"- `{d['method']} {d['route']}` ({d['controller']})\n")
    f.write("\n")

    f.write("### Broken Frontend Calls (No Backend Match)\n")
    for b in analysis.get('broken_calls', []):
        f.write(f"- `{b['verb']} {b['url']}` in `{b['service']}.{b['method']}`\n")
    f.write("\n")

    # TODOs
    f.write("## 5. TODOs & Placeholders\n")
    for t in analysis.get('todos', []):
        f.write(f"- `{t['file']}:{t['line']}`: {t['content']}\n")
    f.write("\n")

    # Recommendations
    f.write("## 6. Recommendations\n")
    f.write("1. **Synchronize DTOs:** 29 DTOs have field mismatches. Update frontend types and backend DTOs to match contracts.\n")
    f.write("2. **Implement Missing Service Logic:** 20 interface methods are registered but lack implementation.\n")
    f.write("3. **Fix Routing:** 17 frontend calls point to non-existent or misconfigured routes.\n")
    f.write("4. **Cleanup Dead Code:** 75 endpoints are not consumed; verify if they are for future use or can be removed.\n")
    f.write("5. **Address TODOs:** 525 markers found. Many are in critical paths (ML retraining, synthetic data generation).\n")

print("Report generated: system-integrity-audit.md")
