import json
import os
import re

# Load broken calls analysis
with open('broken_calls_analysis.json', 'r') as f:
    data = json.load(f)
    broken_calls = data['no_match']

# Group by service
services_to_clean = {}
for call in broken_calls:
    service_name = call['frontend_call'].split('.')[0]
    method_name = call['frontend_call'].split('.')[1]
    
    if service_name not in services_to_clean:
        services_to_clean[service_name] = []
    services_to_clean[service_name].append(method_name)

# Map service names to file paths
services_path = r'd:\Work\Diploma\src\ui\digital-twin-dashboard\src\services'
service_files = {}

for filename in os.listdir(services_path):
    if filename.endswith('.service.ts'):
        service_name = filename.replace('.service.ts', '')
        service_files[service_name] = os.path.join(services_path, filename)

# Also check for dashed names
service_files_dashed = {}
for service_name in services_to_clean:
    dashed_name = service_name.replace('_', '-')
    if dashed_name in service_files:
        service_files_dashed[service_name] = service_files[dashed_name]
    elif service_name in service_files:
        service_files_dashed[service_name] = service_files[service_name]

print("CLEANUP PLAN")
print("=" * 80)
print(f"\nServices with broken calls to remove: {len(services_to_clean)}\n")

for service_name, methods in services_to_clean.items():
    filepath = service_files_dashed.get(service_name)
    if filepath and os.path.exists(filepath):
        print(f"✓ {service_name}.service.ts")
        for method in methods:
            print(f"  - {method}")
    else:
        print(f"✗ {service_name}.service.ts NOT FOUND")

print("\n" + "=" * 80)
print("\nTo remove these methods, you need to:")
print("1. Delete the export async function declaration")
print("2. Delete the method from the service object export")
print("3. Remove from the service object (if it's a class)")
print("\nBroken calls analysis saved to broken_calls_analysis.json")
