#!/usr/bin/env python

def decode(g):
    lines = g.split('\n')[1:-1]
    m = len(lines[0]) - 1
    s = 0
    for y, l in enumerate(lines[1:]):
        for x, i in enumerate(l[1:]):
            s += 2**(y*m+x) * (0 if i == ' ' else 1)
    return (-s if len(lines) > len(lines[0]) else s)


print(decode("""
 ███████
█   ██ █
███ █ █
███  ██
█   ███
██  █
█ █
█
█
"""))

print(decode("""
 ████
██
███
█  █
█████
"""))

print(decode("""
 █
█
"""))

print(decode("""
 █
██
█
"""))

print(decode("""
 ██
█ █
██
"""))

print(decode("""
 ██
███
"""))

