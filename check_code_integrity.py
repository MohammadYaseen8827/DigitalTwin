import os
import re
import json

empty_catch_pattern = re.compile(r'catch\s*(\([^\)]*\))?\s*\{\s*\}', re.MULTILINE)
# Task method without await
task_method_pattern = re.compile(r'async\s+Task(<[^\>]+>)?\s+\w+\([^\)]*\)\s*\{([^}]*)\}', re.MULTILINE)

src_root = r'd:\Work\Diploma\src'
backend_root = os.path.join(src_root, 'api')

issues = {
    'empty_catch': []
}

for root, _, files in os.walk(backend_root):
    if any(x in root for x in ['bin', 'obj']): continue
    for file in files:
        if file.endswith('.cs'):
            path = os.path.join(root, file)
            try:
                with open(path, 'r', encoding='utf-8') as f:
                    content = f.read()
                    if empty_catch_pattern.search(content):
                        issues['empty_catch'].append(os.path.relpath(path, src_root))
            except: pass

with open('code_integrity.json', 'w') as f:
    json.dump(issues, f, indent=2)

print(f"Analysis completed. Empty catches: {len(issues['empty_catch'])}")
