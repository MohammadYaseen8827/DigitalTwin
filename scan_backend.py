import os
import re

backend_path = r'd:\Work\Diploma\src\api'
controllers_path = os.path.join(backend_path, 'DigitalTwinPlatform.API', 'Controllers')

def scan_controllers():
    controllers = []
    for filename in os.listdir(controllers_path):
        if filename.endswith('.cs'):
            with open(os.path.join(controllers_path, filename), 'r', encoding='utf-8') as f:
                content = f.read()
                
                controller_name = filename.replace('.cs', '')
                route_match = re.search(r'\[Route\("(.+?)"\)\]', content)
                route = route_match.group(1) if route_match else "N/A"
                
                # Check for Authorize attribute
                auth = "Authorize" if "[Authorize" in content else "Anonymous"
                
                # Scan for methods
                methods = []
                # Simple regex for finding methods with Http attributes
                method_regex = r'\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)(?:\("(.+?)"\))?\].*?\s+public\s+async\s+Task<ActionResult<(.*?)>>\s+(\w+)\((.*?)\)'
                for match in re.finditer(method_regex, content, re.DOTALL):
                    http_method = match.group(1)
                    endpoint_route = match.group(2) or ""
                    response_dto = match.group(3)
                    method_name = match.group(4)
                    params = match.group(5)
                    
                    # Extract request DTO from params
                    request_dto = "None"
                    if '[FromBody]' in params:
                        dto_match = re.search(r'\[FromBody\]\s*([\w<>]+)\s+\w+', params)
                        if dto_match:
                            request_dto = dto_match.group(1)
                    elif '[FromQuery]' in params:
                         dto_match = re.search(r'\[FromQuery\]\s*([\w<>]+)\s+\w+', params)
                         if dto_match:
                             request_dto = f"Query: {dto_match.group(1)}"
                    
                    full_route = f"/{route.replace('[controller]', controller_name.replace('Controller', ''))}/{endpoint_route}".replace('//', '/').rstrip('/')
                    
                    methods.append({
                        'method_name': method_name,
                        'http_method': http_method,
                        'route': full_route,
                        'request_dto': request_dto,
                        'response_dto': response_dto,
                        'auth': auth
                    })
                
                controllers.append({
                    'name': controller_name,
                    'route': route,
                    'auth': auth,
                    'endpoints': methods
                })
    return controllers

if __name__ == "__main__":
    controllers = scan_controllers()
    print("## Backend Feature Map")
    for c in controllers:
        print(f"Controller: {c['name']}")
        for e in c['endpoints']:
            print(f"  Endpoint: {e['method_name']}")
            print(f"    Route: {e['route']}")
            print(f"    Method: {e['http_method']}")
            print(f"    Request DTO: {e['request_dto']}")
            print(f"    Response DTO: {e['response_dto']}")
            print(f"    Auth: {e['auth']}")
            print(f"    Used In Frontend: TBD")
