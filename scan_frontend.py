import os
import re
import json

frontend_root = r'd:\Work\Diploma\src\ui\digital-twin-dashboard\src'
services_path = os.path.join(frontend_root, 'services')
components_path = frontend_root # Scan all src for component usage

def get_files(path, extension):
    files = []
    for root, _, filenames in os.walk(path):
        for filename in filenames:
            if filename.endswith(extension):
                files.append(os.path.join(root, filename))
    return files

def normalize_url(url, base_url=None):
    """Normalize URL by resolving template expressions."""
    if not url:
        return url
    
    # Replace ${this.baseUrl} with actual baseUrl
    if base_url and '${this.baseUrl}' in url:
        url = url.replace('${this.baseUrl}', base_url)
    
    # Replace ${encodeURIComponent(varname)} with {varname}
    url = re.sub(r'\$\{encodeURIComponent\((\w+)\)\}', r'{\1}', url)
    
    # Replace ${varname} with {varname} for simple template vars
    url = re.sub(r'\$\{(\w+)\}', r'{\1}', url)
    
    return url

def extract_base_url(content):
    """Extract baseUrl from class property if present."""
    match = re.search(r"private\s+baseUrl\s*=\s*['\"]([^'\"]+)['\"]", content)
    if match:
        return match.group(1)
    return None

def scan_frontend_services():
    services = []
    service_files = get_files(services_path, '.ts')
    
    for file_path in service_files:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            service_name = os.path.basename(file_path).replace('.ts', '')
            
            methods = []
            seen_methods = set()
            
            # Extract baseUrl for class-based services
            base_url = extract_base_url(content)
            
            # Pattern to find axiosClient/api calls
            # Handles: axiosClient.get<T>('/path'), axiosClient.get<T>(`/path/${id}`), and axiosClient.get<T>(this.baseUrl)
            # Note: Use a more permissive pattern for generic types to handle nested generics like <Record<string, T>>
            url_pattern = r'''(?:axiosClient|api)\.(get|post|put|delete|patch)\s*(?:<[^(]+>)?\s*\(\s*(?:['"]([^'"]+)['"]|`([^`]+)`|this\.baseUrl)'''
            
            # Alternative pattern for `this.baseUrl` alone
            baseurl_only_pattern = r'''(?:axiosClient|api)\.(get|post|put|delete|patch)\s*(?:<[^>]*>)?\s*\(\s*this\.baseUrl\s*[,)]'''
            
            # Find all export async function declarations (including generics like exportData<T>)
            func_pattern = r'export\s+async\s+function\s+(\w+)(?:<[^>]+>)?\s*\(([^)]*)\)'
            
            func_matches = list(re.finditer(func_pattern, content))
            
            for i, func_match in enumerate(func_matches):
                func_name = func_match.group(1)
                func_params = func_match.group(2).strip()
                func_start = func_match.end()
                
                # Find the end of this function (next export function or end of file)
                if i + 1 < len(func_matches):
                    func_end = func_matches[i + 1].start()
                else:
                    func_end = len(content)
                
                func_body = content[func_start:func_end]
                
                # Find first API call in this function body
                api_match = re.search(url_pattern, func_body)
                if api_match and func_name not in seen_methods:
                    http_verb = api_match.group(1)
                    url = api_match.group(2) or api_match.group(3)
                    if url:
                        normalized_url = normalize_url(url, base_url)
                        methods.append({
                            'name': func_name,
                            'verb': http_verb,
                            'url': normalized_url,
                            'params': func_params
                        })
                        seen_methods.add(func_name)
                    elif base_url:
                        # Handle case where URL is just this.baseUrl
                        methods.append({
                            'name': func_name,
                            'verb': http_verb,
                            'url': base_url,
                            'params': func_params
                        })
                        seen_methods.add(func_name)
            
            # Also check for class methods (async methodName())
            class_method_pattern = r'async\s+(\w+)\s*\(([^)]*)\)\s*(?::\s*[^{]+)?\s*\{'
            for match in re.finditer(class_method_pattern, content):
                method_name = match.group(1)
                if method_name in seen_methods:
                    continue
                
                method_start = match.end()
                # Find the closing brace by counting braces
                brace_count = 1
                pos = method_start
                while pos < len(content) and brace_count > 0:
                    if content[pos] == '{':
                        brace_count += 1
                    elif content[pos] == '}':
                        brace_count -= 1
                    pos += 1
                method_end = pos
                
                method_body = content[method_start:method_end]
                api_match = re.search(url_pattern, method_body)
                if api_match:
                    http_verb = api_match.group(1)
                    url = api_match.group(2) or api_match.group(3)
                    if url:
                        normalized_url = normalize_url(url, base_url)
                        methods.append({
                            'name': method_name,
                            'verb': http_verb,
                            'url': normalized_url,
                            'params': match.group(2).strip()
                        })
                        seen_methods.add(method_name)
                    elif base_url:
                        # Handle case where URL is just this.baseUrl
                        methods.append({
                            'name': method_name,
                            'verb': http_verb,
                            'url': base_url,
                            'params': match.group(2).strip()
                        })
                        seen_methods.add(method_name)
            
            services.append({
                'name': service_name,
                'methods': methods
            })
    return services

def scan_component_usage():
    usage = {}
    src_files = get_files(frontend_root, ('.vue', '.ts'))
    for file_path in src_files:
        rel_path = os.path.relpath(file_path, frontend_root)
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            # Look for imports from @/services
            service_imports = re.findall(r'import\s+.*?\s+from\s+[\'"]@/services/(.*?)[\'"]', content)
            for si in service_imports:
                si_name = si.split('.')[0]
                if si_name not in usage: usage[si_name] = []
                usage[si_name].append(rel_path)
    return usage

def scan_frontend_types():
    types_path = os.path.join(frontend_root, 'types', 'dtos.ts')
    types = {}
    if os.path.exists(types_path):
        with open(types_path, 'r', encoding='utf-8') as f:
            content = f.read()
            # Find interfaces
            interface_matches = re.finditer(r'export\s+interface\s+(\w+).*?\{(.*?)\}', content, re.DOTALL)
            for im in interface_matches:
                name = im.group(1)
                body = im.group(2)
                props = re.findall(r'(\w+)\??:\s*(.*?)(?:\n|;)', body)
                types[name] = {p[0]: p[1].strip() for p in props}
    return types

if __name__ == "__main__":
    data = {
        'services': scan_frontend_services(),
        'usage': scan_component_usage(),
        'types': scan_frontend_types()
    }
    with open('frontend_audit.json', 'w') as f:
        json.dump(data, f, indent=2)
    print("Frontend audit data saved to frontend_audit.json")
