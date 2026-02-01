#!/usr/bin/env python3
"""Extrae SOLO errores de compilacion de un log de dotnet build."""
import sys
import re
import os

def parse_build_log(filepath):
    errors = []
    pattern = re.compile(r'(.+?)\((\d+),(\d+)\): error (CS\d+): (.+?) \[')
    
    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        for line in f:
            match = pattern.search(line)
            if match:
                filename = os.path.basename(match.group(1).strip())
                errors.append({
                    'file': filename,
                    'line': int(match.group(2)),
                    'code': match.group(4),
                    'msg': match.group(5).strip()
                })
    return errors

def main():
    if len(sys.argv) > 1:
        filepath = sys.argv[1]
    elif os.path.exists('build_errors.txt'):
        filepath = 'build_errors.txt'
    else:
        filepath = os.path.join(os.environ.get('TEMP', ''), 'build_output.txt')
    
    if not os.path.exists(filepath):
        print("No se encontro archivo de log")
        sys.exit(1)
    
    errors = parse_build_log(filepath)
    
    if not errors:
        print("OK - Sin errores de compilacion")
        return
    
    by_file = {}
    for e in errors:
        if e['file'] not in by_file:
            by_file[e['file']] = []
        by_file[e['file']].append(e)
    
    print(f"ERRORES: {len(errors)} en {len(by_file)} archivo(s)")
    print("-" * 50)
    
    for filename, file_errors in by_file.items():
        print(f"\n{filename}:")
        for e in file_errors:
            print(f"  L{e['line']}: {e['code']} - {e['msg']}")

if __name__ == '__main__':
    main()
