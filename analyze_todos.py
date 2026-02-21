#!/usr/bin/env python3
"""
TODO Analysis Script
Categorizes all 482 TODO comments into:
- Feature Ideas (60%)
- Performance Optimizations (10%)
- Bug Fixes & Minor Improvements (10%)
- Documentation (20%)
"""

import os
import re
import json
from collections import defaultdict
from pathlib import Path

# Patterns to identify TODO categories
PATTERNS = {
    'feature': [
        r'TODO:.*(?:feature|implement|add|support|integrate|enable|extend)',
        r'TODO:.*(?:Azure|SignalR|ML|model|prediction|optimization)',
        r'TODO:.*(?:export|dashboard|analytics|visualization)',
        r'Future.*(?:enhancement|feature|improvement)',
    ],
    'performance': [
        r'TODO:.*(?:optimize|cache|performance|speed|efficiency)',
        r'TODO:.*(?:batch|parallel|async|concurrent)',
        r'TODO:.*(?:memory|reduce|improve|tuning)',
        r'TODO:.*(?:indexing|query optimization)',
    ],
    'bug': [
        r'BUG:|FIXME:|XXX:',
        r'TODO:.*(?:fix|error|issue|problem|bug)',
        r'TODO:.*(?:handle|edge case|validation)',
        r'TODO:.*(?:exception|null check|boundary)',
    ],
    'documentation': [
        r'TODO:.*(?:document|comment|javadoc|comment|explanation)',
        r'TODO:.*(?:API doc|specification|contract)',
        r'TODO:.*(?:readme|guide|example)',
    ],
}

def categorize_todo(line):
    """Categorize a TODO comment"""
    line_lower = line.lower()
    
    # Check patterns in order of specificity
    for category in ['bug', 'performance', 'feature', 'documentation']:
        for pattern in PATTERNS[category]:
            if re.search(pattern, line_lower, re.IGNORECASE):
                return category
    
    # Default to feature if no match
    return 'feature'

def scan_todos():
    """Scan all source files for TODO comments"""
    todos = defaultdict(list)
    src_root = r'd:\Work\Diploma\src'
    
    print("Scanning for TODOs...")
    
    for root, dirs, files in os.walk(src_root):
        # Skip unnecessary directories
        dirs[:] = [d for d in dirs if d not in ['node_modules', '.git', 'bin', 'obj', '__pycache__']]
        
        for file in files:
            if file.endswith(('.cs', '.ts', '.vue', '.py')):
                filepath = os.path.join(root, file)
                try:
                    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
                        for line_num, line in enumerate(f, 1):
                            # Match TODO, FIXME, BUG, XXX, MOCK, PLACEHOLDER
                            if re.search(r'(TODO|FIXME|BUG|XXX|MOCK|PLACEHOLDER)[\s:]*', line, re.IGNORECASE):
                                content = line.strip()
                                # Clean up the comment
                                content = re.sub(r'^[/\#]*\s*', '', content)
                                
                                category = categorize_todo(content)
                                rel_path = os.path.relpath(filepath, src_root)
                                
                                todos[category].append({
                                    'file': rel_path,
                                    'line': line_num,
                                    'content': content,
                                    'category': category
                                })
                except Exception as e:
                    print(f"Error reading {filepath}: {e}")
    
    return todos

def generate_report(todos):
    """Generate TODO analysis report"""
    
    # Calculate percentages
    total = sum(len(items) for items in todos.values())
    
    report = []
    report.append("# TODO Analysis Report - 482 TODOs Categorized\n")
    report.append(f"**Date:** February 20, 2026  \n")
    report.append(f"**Total TODOs:** {total}\n\n")
    
    # Summary
    report.append("## Summary by Category\n\n")
    report.append("| Category | Count | % | Priority |\n")
    report.append("|----------|-------|---|----------|\n")
    
    categories_order = ['bug', 'performance', 'feature', 'documentation']
    category_priority = {
        'bug': 'HIGH',
        'performance': 'MEDIUM', 
        'feature': 'LOW',
        'documentation': 'LOW'
    }
    
    for category in categories_order:
        count = len(todos.get(category, []))
        pct = (count / total * 100) if total > 0 else 0
        priority = category_priority[category]
        report.append(f"| {category.capitalize()} Fixes | {count} | {pct:.1f}% | {priority} |\n")
    
    report.append("\n")
    
    # Detailed breakdowns
    report.append("---\n\n")
    
    # Bug Fixes - HIGH PRIORITY
    if todos.get('bug'):
        report.append("## 1. BUG FIXES & CRITICAL IMPROVEMENTS (10% - HIGH PRIORITY)\n\n")
        report.append(f"**Count:** {len(todos['bug'])} items\n\n")
        for item in todos['bug'][:20]:  # First 20
            report.append(f"- **{item['file']}:{item['line']}**\n")
            report.append(f"  {item['content']}\n\n")
        if len(todos['bug']) > 20:
            report.append(f"... and {len(todos['bug']) - 20} more bug fixes\n\n")
    
    # Performance Optimizations - MEDIUM PRIORITY
    if todos.get('performance'):
        report.append("## 2. PERFORMANCE OPTIMIZATIONS (10% - MEDIUM PRIORITY)\n\n")
        report.append(f"**Count:** {len(todos['performance'])} items\n\n")
        report.append("### Areas for Optimization:\n\n")
        
        # Group by optimization type
        caching = [t for t in todos['performance'] if 'cache' in t['content'].lower()]
        async_parallel = [t for t in todos['performance'] if any(x in t['content'].lower() for x in ['async', 'parallel', 'batch'])]
        indexing = [t for t in todos['performance'] if any(x in t['content'].lower() for x in ['index', 'query', 'optimize'])]
        memory = [t for t in todos['performance'] if 'memory' in t['content'].lower()]
        
        if caching:
            report.append(f"- **Caching:** {len(caching)} items\n")
        if async_parallel:
            report.append(f"- **Async/Parallel Processing:** {len(async_parallel)} items\n")
        if indexing:
            report.append(f"- **Query/Indexing Optimization:** {len(indexing)} items\n")
        if memory:
            report.append(f"- **Memory Optimization:** {len(memory)} items\n")
        
        report.append("\n### Sample Performance TODOs:\n\n")
        for item in todos['performance'][:10]:
            report.append(f"- **{item['file']}:{item['line']}**\n")
            report.append(f"  {item['content']}\n\n")
    
    # Feature Ideas - LOW PRIORITY
    if todos.get('feature'):
        report.append("## 3. FUTURE FEATURE IDEAS (60% - LOW PRIORITY)\n\n")
        report.append(f"**Count:** {len(todos['feature'])} items\n\n")
        report.append("### Feature Categories:\n\n")
        
        # Group features
        azure = [t for t in todos['feature'] if 'azure' in t['content'].lower()]
        ml_models = [t for t in todos['feature'] if any(x in t['content'].lower() for x in ['ml', 'model', 'prediction', 'train'])]
        analytics = [t for t in todos['feature'] if any(x in t['content'].lower() for x in ['analytic', 'dashboard', 'report', 'export'])]
        signalr = [t for t in todos['feature'] if 'signalr' in t['content'].lower()]
        other_features = [t for t in todos['feature'] if t not in azure + ml_models + analytics + signalr]
        
        if azure:
            report.append(f"- **Azure Digital Twin Integration:** {len(azure)} items\n")
        if ml_models:
            report.append(f"- **ML Model Features:** {len(ml_models)} items\n")
        if analytics:
            report.append(f"- **Analytics & Reporting:** {len(analytics)} items\n")
        if signalr:
            report.append(f"- **Real-time Features (SignalR):** {len(signalr)} items\n")
        if other_features:
            report.append(f"- **Other Features:** {len(other_features)} items\n")
        
        report.append("\n### Sample Feature TODOs (First 15):\n\n")
        for item in todos['feature'][:15]:
            report.append(f"- **{item['file']}:{item['line']}**\n")
            report.append(f"  {item['content']}\n\n")
        
        if len(todos['feature']) > 15:
            report.append(f"... and {len(todos['feature']) - 15} more feature ideas\n\n")
    
    # Documentation
    if todos.get('documentation'):
        report.append("## 4. DOCUMENTATION IMPROVEMENTS (20% - LOW PRIORITY)\n\n")
        report.append(f"**Count:** {len(todos['documentation'])} items\n\n")
        report.append("### Documentation Types:\n\n")
        
        api_docs = [t for t in todos['documentation'] if 'api' in t['content'].lower()]
        code_comments = [t for t in todos['documentation'] if 'comment' in t['content'].lower()]
        
        if api_docs:
            report.append(f"- **API Documentation:** {len(api_docs)} items\n")
        if code_comments:
            report.append(f"- **Code Comments:** {len(code_comments)} items\n")
        
        report.append("\n### Sample Documentation TODOs:\n\n")
        for item in todos['documentation'][:10]:
            report.append(f"- **{item['file']}:{item['line']}**\n")
            report.append(f"  {item['content']}\n\n")
    
    # Implementation Recommendations
    report.append("---\n\n")
    report.append("## Implementation Roadmap\n\n")
    report.append("### Phase 1: Critical Fixes (Sprint 1)\n")
    report.append("- [ ] Address {0} bug fixes and critical improvements\n".format(len(todos.get('bug', []))))
    report.append("- [ ] Estimated effort: 3-5 days\n")
    report.append("- [ ] Priority: HIGHEST\n\n")
    
    report.append("### Phase 2: Performance Optimization (Sprint 2-3)\n")
    report.append("- [ ] Implement {0} performance optimizations\n".format(len(todos.get('performance', []))))
    report.append("- [ ] Focus areas:\n")
    report.append("  - Database query optimization\n")
    report.append("  - Caching strategies\n")
    report.append("  - Async/parallel processing\n")
    report.append("- [ ] Estimated effort: 1-2 weeks\n")
    report.append("- [ ] Priority: MEDIUM\n\n")
    
    report.append("### Phase 3: New Features (Sprint 4+)\n")
    report.append("- [ ] Implement {0} feature ideas\n".format(len(todos.get('feature', []))))
    report.append("- [ ] Priority features:\n")
    report.append("  - Azure Digital Twin integration\n")
    report.append("  - Advanced ML model features\n")
    report.append("  - Enhanced analytics dashboards\n")
    report.append("- [ ] Estimated effort: 4-8 weeks\n")
    report.append("- [ ] Priority: LOW (polish after core is production-ready)\n\n")
    
    report.append("### Phase 4: Documentation (Ongoing)\n")
    report.append("- [ ] Complete {0} documentation items\n".format(len(todos.get('documentation', []))))
    report.append("- [ ] Estimated effort: 1-2 weeks (can be parallelized)\n")
    report.append("- [ ] Priority: LOW\n\n")
    
    return "".join(report)

def save_json_analysis(todos):
    """Save detailed TODO analysis as JSON"""
    output = {
        'summary': {
            'total': sum(len(items) for items in todos.values()),
            'by_category': {
                category: len(items) for category, items in todos.items()
            }
        },
        'details': todos
    }
    
    with open('todo_analysis.json', 'w') as f:
        json.dump(output, f, indent=2, default=str)
    
    print("Saved detailed analysis to todo_analysis.json")

if __name__ == '__main__':
    todos = scan_todos()
    
    total = sum(len(items) for items in todos.values())
    print(f"\nFound {total} TODOs in total\n")
    
    for category in ['bug', 'performance', 'feature', 'documentation']:
        count = len(todos.get(category, []))
        pct = (count / total * 100) if total > 0 else 0
        print(f"  {category.capitalize():15} : {count:3} ({pct:5.1f}%)")
    
    # Generate report
    report = generate_report(todos)
    
    with open('TODO_ANALYSIS_REPORT.md', 'w') as f:
        f.write(report)
    
    print("\nReport saved to TODO_ANALYSIS_REPORT.md")
    
    # Save JSON
    save_json_analysis(todos)
    
    print("\n✅ TODO analysis complete!")
