import json
import re
import os

def load_json(name):
    try:
        with open(name, 'r') as f:
            return json.load(f)
    except Exception as e:
        print(f"Error loading {name}: {e}")
        return None

backend = load_json('backend_audit.json')
frontend = load_json('frontend_audit.json')

if not backend or not frontend:
    exit(1)

def clean_route(r):
    if not r: return ""
    # Remove junk from bad regex matches (e.g. ")]\n[Produces...")
    r = r.split('"')[0].split(')')[0]
    r = r.lower().strip('/')
    # Standardize parameter placeholders to just {param}
    r = re.sub(r'\{(\w+)(:\w+)?\}', r'{param}', r)
    # Remove query string parameters
    r = r.split('?')[0]
    return r

def normalize_http_method(method):
    """Convert HttpGet/HttpPost/etc to standard GET/POST/etc"""
    method = method.upper()
    # Remove 'HTTP' prefix if present (e.g., HTTPGET -> GET)
    if method.startswith('HTTP'):
        method = method[4:]
    return method

# 1. Map Backend Endpoints
backend_map = {}
for controller in backend['controllers']:
    for endpoint in controller['endpoints']:
        route = clean_route(endpoint['route'])
        if not route.startswith('api'):
            route = 'api/' + route
            
        key = f"{normalize_http_method(endpoint['http_method'])}:{route}"
        backend_map[key] = {
            'controller': controller['name'],
            'method': endpoint['method'],
            'request_dto': endpoint['request_dto'],
            'response_dto': endpoint['response_dto'],
            'used': False,
            'original_route': endpoint['route']
        }

# 2. Map Frontend Calls
frontend_calls = []
for service in frontend['services']:
    for method in service['methods']:
        url = method['url']
        if url == "Unknown": continue
        
        # Normalize URL
        norm_url = clean_route(url)
        # Replace ${var} with {var}
        norm_url = re.sub(r'\$\{(\w+)\}', r'{\1}', norm_url)
        
        if not norm_url.startswith('api'):
            norm_url = 'api/' + norm_url
        
        call_key = f"{normalize_http_method(method['verb'])}:{norm_url}"
        
        frontend_calls.append({
            'service': service['name'],
            'method': method['name'],
            'verb': normalize_http_method(method['verb']),
            'url': norm_url,
            'key': call_key,
            'match': None
        })

# DEBUG: Print first 5 keys
print("Sample Backend Keys:")
print(list(backend_map.keys())[:5])
print("Sample Frontend Keys:")
print([c['key'] for c in frontend_calls[:5]])

# 3. Match Frontend to Backend
matched_count = 0
for call in frontend_calls:
    if call['key'] in backend_map:
        call['match'] = backend_map[call['key']]
        backend_map[call['key']]['used'] = True
        matched_count += 1
    else:
        # Try a more aggressive match (ignoring api/ prefix if it's inconsistent)
        alt_key = call['key'].replace('api/', '')
        if alt_key in backend_map:
             call['match'] = backend_map[alt_key]
             backend_map[alt_key]['used'] = True
             matched_count += 1

# 4. Check Implementation Integrity
unimplemented = []
for iface_name, methods in backend['interfaces'].items():
    if not methods: continue
    found_impl = False
    for impl_name, impl_data in backend['implementations'].items():
        if iface_name in impl_data['interfaces']:
            found_impl = True
            # Normalize method names (case insensitive)
            impl_methods = [m.lower() for m in impl_data['methods']]
            missing = [m for m in methods if m.lower() not in impl_methods]
            if missing:
                unimplemented.append({
                    'interface': iface_name,
                    'implementation': impl_name,
                    'missing': missing
                })
    if not found_impl and methods:
         unimplemented.append({
            'interface': iface_name,
            'implementation': 'NONE',
            'missing': methods
        })

# 5. Dead Code
dead_endpoints = [
    {'route': val['original_route'], 'method': key.split(':')[0], 'controller': val['controller']}
    for key, val in backend_map.items() if not val['used']
]

# 6. Non-existent Frontend Calls
broken_calls = [
    {'service': call['service'], 'method': call['method'], 'url': call['url'], 'verb': call['verb']}
    for call in frontend_calls if not call['match']
]

# 7. TODO Search (Simplified)
todos = []
pattern = re.compile(r'TODO|FIXME|PLACEHOLDER|BUG|MOCK', re.IGNORECASE)
src_root = r'd:\Work\Diploma\src'
for root, _, files in os.walk(src_root):
    if any(x in root for x in ['node_modules', '.git', 'bin', 'obj']): continue
    for file in files:
        if file.endswith(('.cs', '.ts', '.vue')):
            path = os.path.join(root, file)
            try:
                with open(path, 'r', encoding='utf-8') as f:
                    for i, line in enumerate(f):
                        if pattern.search(line):
                            todos.append({
                                'file': os.path.relpath(path, src_root),
                                'line': i + 1,
                                'content': line.strip()
                            })
            except: pass

# Output summary
report = {
    'summary': {
        'total_backend_endpoints': len(backend_map),
        'total_frontend_calls': len(frontend_calls),
        'matched_calls': matched_count,
        'dead_endpoints_count': len(dead_endpoints),
        'broken_calls_count': len(broken_calls),
        'unimplemented_methods_count': len(unimplemented),
        'todos_count': len(todos)
    },
    'broken_calls': broken_calls,
    'dead_endpoints': dead_endpoints,
    'unimplemented_methods': unimplemented,
    'todos': todos[:200]
}

with open('integrity_analysis.json', 'w') as f:
    json.dump(report, f, indent=2)

print(f"Summary: {report['summary']}")
