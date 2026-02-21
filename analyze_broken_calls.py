import json
import os
import re

# Load broken calls
with open('integrity_analysis.json', 'r') as f:
    data = json.load(f)
    broken_calls = data['broken_calls']

# Scan all backend controllers for endpoints
controllers_path = r'd:\Work\Diploma\src\api\DigitalTwinPlatform.API\Controllers'
backend_endpoints = {}

for file in os.listdir(controllers_path):
    if file.endswith('Controller.cs'):
        filepath = os.path.join(controllers_path, file)
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
            # Extract [Route] and [Http*] patterns
            routes = re.findall(r'\[Route\("([^"]+)"\)', content)
            methods = re.findall(r'\[Http(Get|Post|Put|Delete|Patch)\("([^"]+)"\)', content)
            
            if routes:
                base_route = routes[0]
            else:
                base_route = ''
            
            for method, path in methods:
                full_path = f"{base_route}/{path}".replace('//', '/').lower()
                if full_path not in backend_endpoints:
                    backend_endpoints[full_path] = {'methods': [], 'file': file}
                backend_endpoints[full_path]['methods'].append(method.upper())

print("BROKEN CALLS ANALYSIS\n" + "="*80)
print(f"Total broken calls: {len(broken_calls)}\n")

analysis = {
    'has_exact_match': [],
    'http_verb_mismatch': [],
    'no_match': []
}

for call in broken_calls:
    service = call['service']
    method = call['method']
    url = call['url'].replace('{param}', '{id}').lower()
    verb = call['verb'].upper()
    
    # Check for exact match
    found = False
    for endpoint_path, endpoint_data in backend_endpoints.items():
        if url in endpoint_path and verb in endpoint_data['methods']:
            analysis['has_exact_match'].append({
                'frontend_call': f"{service}.{method}",
                'frontend_url': call['url'],
                'backend_endpoint': endpoint_path,
                'backend_verb': endpoint_data['methods']
            })
            found = True
            break
    
    if not found:
        # Check for similar path match (different verb)
        for endpoint_path, endpoint_data in backend_endpoints.items():
            if url in endpoint_path:
                analysis['http_verb_mismatch'].append({
                    'frontend_call': f"{service}.{method}",
                    'frontend_url': call['url'],
                    'frontend_verb': verb,
                    'backend_endpoint': endpoint_path,
                    'backend_verbs': endpoint_data['methods']
                })
                found = True
                break
    
    if not found:
        analysis['no_match'].append({
            'frontend_call': f"{service}.{method}",
            'url': call['url'],
            'verb': verb
        })

print(f"\nEXACT MATCHES: {len(analysis['has_exact_match'])}")
for item in analysis['has_exact_match'][:5]:
    print(f"  ✓ {item['frontend_call']}")

print(f"\nHTTP VERB MISMATCHES: {len(analysis['http_verb_mismatch'])}")
for item in analysis['http_verb_mismatch'][:10]:
    print(f"  ✗ {item['frontend_call']}")
    print(f"    Frontend: {item['frontend_verb']} {item['frontend_url']}")
    print(f"    Backend:  {','.join(item['backend_verbs'])} {item['backend_endpoint']}")

print(f"\nNO MATCH: {len(analysis['no_match'])}")
for item in analysis['no_match'][:10]:
    print(f"  ✗ {item['frontend_call']} ({item['verb']} {item['url']})")

# Save analysis
with open('broken_calls_analysis.json', 'w') as f:
    json.dump(analysis, f, indent=2)

print("\n" + "="*80)
print(f"Summary:")
print(f"  Exact matches:        {len(analysis['has_exact_match'])}")
print(f"  HTTP verb mismatches: {len(analysis['http_verb_mismatch'])}")
print(f"  No match:             {len(analysis['no_match'])}")
print(f"\nAnalysis saved to broken_calls_analysis.json")
