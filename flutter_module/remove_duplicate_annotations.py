#!/usr/bin/env python3
import re

with open('lib/flutter_sharp_structs.dart', 'r') as f:
    lines = f.readlines()

fixed_lines = []
skip_next = False

for i, line in enumerate(lines):
    if skip_next:
        skip_next = False
        continue
    
    # Check if this line is an annotation and the next line is also an annotation
    if i < len(lines) - 1:
        curr_annotation = line.strip() in ['@Int8()', '@Int32()', '@Double()']
        next_annotation = lines[i+1].strip() in ['@Int8()', '@Int32()', '@Double()']
        
        if curr_annotation and next_annotation:
            # Keep only the first annotation, skip the second
            fixed_lines.append(line)
            skip_next = True
            continue
    
    fixed_lines.append(line)

with open('lib/flutter_sharp_structs.dart', 'w') as f:
    f.writelines(fixed_lines)

print("Removed duplicate annotations")
