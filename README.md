# Convex Holes - Project Euler
 Problem: 252\
 Difficulty Rating: 80%\
 https://projecteuler.net/problem=252

## Problem Statement
Given a set of points on a plane, we define a convex hole to be a convex polygon having as vertices any of the given points and not containing any of the given points in its interior (in addition to the vertices, other given points may lie on the perimeter of the polygon). What is the maximum area for a convex hole on the set containing the first points in the pseudo-random sequence? Specify your answer including one digit after the decimal point.
> S_0 = 290797\
> S_n+1 = (S_n)^2 mod 50515093\
> T_n = (S_n mod 2000) - 1000

## About Project Euler
Project Euler is website that hosts a series of challenging mathematical/computer programming problems that require mathematical insights and efficient code to solve.