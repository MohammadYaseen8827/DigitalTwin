import json
import os
import re

# Load broken calls
with open('broken_calls_analysis.json', 'r') as f:
    data = json.load(f)
    broken_calls = data['no_match']

# Group by service and method
broken_by_service = {}
for call in broken_calls:
    parts = call['frontend_call'].split('.')
    service = parts[0]
    method = parts[1]
    
    if service not in broken_by_service:
        broken_by_service[service] = []
    broken_by_service[service].append(method)

print("BROKEN CALLS TO REMOVE")
print("=" * 80)

# Map service names to files
services_path = r'd:\Work\Diploma\src\ui\digital-twin-dashboard\src\services'
service_files_map = {
    'alertRules': 'alertRules.service.ts',
    'alerts': 'alerts.service.ts',
    'antiForgery': 'antiForgery.service.ts',
    'azureDigitalTwin': 'azureDigitalTwin.service.ts',
    'benchmarkValidation': 'benchmarkValidation.service.ts',
    'csrf': 'csrf.service.ts',
    'dataArchival': 'dataArchival.service.ts',
    'external-systems': 'external-systems.service.ts',
    'predictions': 'predictions.service.ts',
    'reporting': 'reporting.service.ts'
}

# Methods to remove from service exports
export_removals = {}

for service_name, methods in broken_by_service.items():
    filepath = os.path.join(services_path, service_files_map[service_name])
    
    if not os.path.exists(filepath):
        print(f"✗ {service_name}: FILE NOT FOUND")
        continue
    
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    print(f"\n✓ {service_name}.service.ts")
    print(f"  Methods to remove from service export: {', '.join(methods)}")
    
    # Check if methods exist in the service export
    export_match = re.search(r'export\s+const\s+\w+Service\s*=\s*\{([^}]+)\}', content, re.DOTALL)
    if export_match:
        export_content = export_match.group(1)
        found_methods = []
        for method in methods:
            if method in export_content:
                found_methods.append(method)
        if found_methods:
            print(f"  ✓ Found in export: {', '.join(found_methods)}")
            export_removals[service_name] = found_methods

print("\n" + "=" * 80)
print("\nSummary:")
print(f"  Services to modify: {len(broken_by_service)}")
print(f"  Total methods to remove: {sum(len(methods) for methods in broken_by_service.values())}")
print("\nNote: Run this script manually for each service to remove the broken calls")
