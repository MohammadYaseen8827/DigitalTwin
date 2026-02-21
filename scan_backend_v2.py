import os
import re
import json

backend_root = r'd:\Work\Diploma\src\api'
controllers_path = os.path.join(backend_root, 'DigitalTwinPlatform.API', 'Controllers')
application_path = os.path.join(backend_root, 'DigitalTwinPlatform.Application')

def get_files(path, extension):
    files = []
    for root, _, filenames in os.walk(path):
        for filename in filenames:
            if filename.endswith(extension):
                files.append(os.path.join(root, filename))
    return files

def scan_backend():
    controllers = []
    controller_files = get_files(controllers_path, '.cs')
    
    for file_path in controller_files:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            
            controller_name = os.path.basename(file_path).replace('.cs', '')
            route_match = re.search(r'\[Route\("(.+?)"\)\]', content)
            base_route = route_match.group(1) if route_match else ""
            base_route = base_route.replace('[controller]', controller_name.replace('Controller', ''))
            
            # Find injected services
            services_used = re.findall(r'(\w+)\s+\w+[,)]', content)
            # Filter for common service naming patterns like I...Service
            services_used = [s for s in services_used if s.startswith('I') and ('Service' in s or 'Repository' in s or 'Mediator' in s)]
            
            endpoints = []
            # Regex for Http methods - fixed to not match across newlines
            method_regex = r'\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)(?:\("([^"]*?)"\))?\]\s*(?:\[[^\]]*\]\s*)*public\s+(?:async\s+)?(?:Task(?:<.*?>)?\s+)?(?:ActionResult(?:<.*?>)?\s+)?(\w+)\((.*?)\)'
            for match in re.finditer(method_regex, content, re.DOTALL):
                http_method = match.group(1)
                sub_route = match.group(2) or ""
                method_name = match.group(3)
                params = match.group(4)
                
                # Extract response DTO from return type if present
                response_dto = "None"
                return_type_match = re.search(r'public\s+(?:async\s+)?Task<ActionResult<([^>]+)>>', match.group(0))
                if return_type_match:
                    response_dto = return_type_match.group(1)
                elif 'IActionResult' in match.group(0):
                    response_dto = "IActionResult"
                
                full_route = f"/{base_route}/{sub_route}".replace('//', '/').rstrip('/')
                if not full_route.startswith('/'): full_route = '/' + full_route
                
                request_dto = "None"
                if '[FromBody]' in params:
                    dto_match = re.search(r'\[FromBody\]\s*([\w<>]+)\s+\w+', params)
                    if dto_match: request_dto = dto_match.group(1)
                elif '[FromQuery]' in params:
                    dto_match = re.search(r'\[FromQuery\]\s*([\w<>]+)\s+\w+', params)
                    if dto_match: request_dto = f"Query:{dto_match.group(1)}"
                
                endpoints.append({
                    'method': method_name,
                    'http_method': http_method,
                    'route': full_route,
                    'request_dto': request_dto,
                    'response_dto': response_dto
                })
            
            controllers.append({
                'name': controller_name,
                'base_route': base_route,
                'services': list(set(services_used)),
                'endpoints': endpoints
            })
            
    return controllers

def scan_dtos():
    dto_map = {}
    app_files = get_files(application_path, '.cs')
    for file_path in app_files:
        if 'Dto' in file_path or 'Request' in file_path or 'Response' in file_path or 'Model' in file_path:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
                # Find class/record definitions
                class_matches = re.finditer(r'public\s+(class|record)\s+(\w+).*?\{(.*?)\}', content, re.DOTALL)
                for cm in class_matches:
                    name = cm.group(2)
                    body = cm.group(3)
                    props = re.findall(r'public\s+([\w<>?\[\]]+)\s+(\w+)\s*\{\s*get;', body)
                    dto_map[name] = {p[1]: p[0] for p in props}
    return dto_map

def scan_interfaces():
    interfaces = {}
    app_files = get_files(application_path, '.cs')
    for file_path in app_files:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            if 'interface' in content:
                interface_matches = re.finditer(r'public\s+interface\s+(\w+).*?\{(.*?)\}', content, re.DOTALL)
                for im in interface_matches:
                    name = im.group(1)
                    body = im.group(2)
                    methods = re.findall(r'Task(?:<.*?>)?\s+(\w+)\(', body)
                    interfaces[name] = methods
    return interfaces

def scan_implementations():
    implementations = {}
    app_files = get_files(application_path, '.cs')
    for file_path in app_files:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            if 'class' in content and ':' in content:
                # Find classes that implement interfaces
                impl_matches = re.finditer(r'public\s+class\s+(\w+)\s*:\s*([\w, \s]+)\{', content)
                for im in impl_matches:
                    class_name = im.group(1)
                    impl_interfaces = [i.strip() for i in im.group(2).split(',')]
                    
                    # Find method implementations
                    method_impls = re.findall(r'public\s+async\s+Task(?:<.*?>)?\s+(\w+)\(', content)
                    implementations[class_name] = {
                        'interfaces': impl_interfaces,
                        'methods': method_impls
                    }
    return implementations

if __name__ == "__main__":
    data = {
        'controllers': scan_backend(),
        'dtos': scan_dtos(),
        'interfaces': scan_interfaces(),
        'implementations': scan_implementations()
    }
    with open('backend_audit.json', 'w') as f:
        json.dump(data, f, indent=2)
    print("Backend audit data saved to backend_audit.json")
