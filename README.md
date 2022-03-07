# CHIP-8 Interpreter
#### Written in C#

The interpreter class is intended to be a completely library-agnostic class, able to be used with any (or without) external libraries.
All CHIP-8 op-codes are handled and have been tested against several different test programs.

All sprite drawing and input handling is done outside of the Interpreter class, and in this case, Unity features are used to achieve this.

Input keys are:
1, 2, 3, 4
Q, W, E, R
A, S, D, F
Z, X, C, V
(UK QWERTY Keyboard)

Equivalent to the hex keypad:
1, 2, 3, C
4, 5, 6, D
7, 8, 9, E
A, 0, B, F

Currently only used inside of the Unity Editor, so .ch8 program loading is only handled via test input in the GameController.cs file.
