import json

backend = json.load(open('backend_audit.json'))
frontend = json.load(open('frontend_audit.json'))

diffs = []

common_dtos = set(backend['dtos'].keys()) & set(frontend['types'].keys())

for dto in common_dtos:
    b_fields = set(backend['dtos'][dto].keys())
    f_fields = set(frontend['types'][dto].keys())
    
    only_b = b_fields - f_fields
    only_f = f_fields - b_fields
    
    if only_b or only_f:
        diffs.append({
            'dto': dto,
            'only_backend': list(only_b),
            'only_frontend': list(only_f)
        })

with open('dto_mismatches.json', 'w') as f:
    json.dump(diffs, f, indent=2)

print(f"Found {len(diffs)} DTO mismatches.")
