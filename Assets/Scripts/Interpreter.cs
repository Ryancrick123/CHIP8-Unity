using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class Interpreter
{
    
    // 4kb RAM, first 512 bytes originally stored the interpreter itself
    byte[] RAM = new byte[0x1000];

    // Program counter starts at 512th byte
    ushort PC = 0x200;

    // array of registers V0-VF
    byte[] V = new byte[0x10];

    // Address register original implementation was 12 bits, hopefully 16 won't break anything
    ushort I;

    // Used to store return addresses when calling subroutines
    Stack<ushort> stack = new Stack<ushort>();

    // Sound and delay timers, changed and set by 
    int delay = 0, sound = 0;

    public bool playSound = false;

    public byte? pressedKey;

    bool paused = false;
    int X_paused;

    System.Random rand = new System.Random();

    public bool[,] screenBuffer = new bool[64,32];

    public void OpenFile(string path)
    {
        byte[] romData = File.ReadAllBytes(path);
        romData.CopyTo(RAM, 0x200);
    }

    public void LoadFont()
    {
        byte[] fontData = 
        {0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80};  // F

        fontData.CopyTo(RAM, 0x050);
    }

    public void Tick()
    {
        if(!paused)
        {
            if (delay > 0) { delay--; }
            if (sound > 0) { sound--; playSound = true; }
            
            ExecuteOpCode(NextOpcode());
        }
        else    // Deal with blocking behaviour in FX0A
        {
            if(pressedKey != null)
            {
                V[X_paused] = (byte)pressedKey;
                paused = false;
            }
        }
    }

    public void SetKey(byte key)
    {
        pressedKey = key;
    }

    ushort NextOpcode()
    {
        ushort opcode = (ushort)((RAM[PC++] << 8) | (RAM[PC++]));
        //Debug.Log(opcode.ToString("X4"));
        return opcode;
    }

    ushort RepeatOpcode()
    {
        ushort opcode = (ushort)((RAM[PC++] << 8) | (RAM[PC--]));
        return opcode;
    }

    void ExecuteOpCode(ushort opcode)
    {
        // START WITH 00E0, 1NNN, 6XNN, 7XNN, ANNN, DXYN

        switch(opcode & 0xF000)
        {
            case 0x0000:
                switch(opcode)
                {
                    case 0x00E0: Opcode00E0(); break;
                    case 0x00EE: Opcode00EE(); break;
                    default: break;
                }
                break;
            case 0x1000: Opcode1NNN(opcode); break;
            case 0x2000: Opcode2NNN(opcode); break;
            case 0x3000: Opcode3XNN(opcode); break;
            case 0x4000: Opcode4XNN(opcode); break;
            case 0x5000: Opcode5XY0(opcode); break;
            case 0x6000: Opcode6XNN(opcode); break;
            case 0x7000: Opcode7XNN(opcode); break;
            case 0x8000:
                switch(opcode & 0x000F)
                {
                    case 0x0000: Opcode8XY0(opcode); break;
                    case 0x0001: Opcode8XY1(opcode); break;
                    case 0x0002: Opcode8XY2(opcode); break;
                    case 0x0003: Opcode8XY3(opcode); break;
                    case 0x0004: Opcode8XY4(opcode); break;
                    case 0x0005: Opcode8XY5(opcode); break;
                    case 0x0006: Opcode8XY6(opcode); break;
                    case 0x0007: Opcode8XY7(opcode); break;
                    case 0x000E: Opcode8XYE(opcode); break;
                }
                break;
            case 0x9000: Opcode9XY0(opcode); break;
            case 0xA000: OpcodeANNN(opcode); break;
            case 0xB000: OpcodeBNNN(opcode); break;
            case 0xC000: OpcodeCXNN(opcode); break;
            case 0xD000: OpcodeDXYN(opcode); break;
            case 0xE000:
                switch(opcode & 0x000F)
                {
                    case 0x000E: OpcodeEX9E(opcode); break;
                    case 0x0001: OpcodeEXA1(opcode); break;
                }
                break;
            case 0xF000:
                switch(opcode & 0x00FF)
                {
                    case 0x0007: OpcodeFX07(opcode); break;
                    case 0x000A: OpcodeFX0A(opcode); break;
                    case 0x0015: OpcodeFX15(opcode); break;
                    case 0x0018: OpcodeFX18(opcode); break;
                    case 0x001E: OpcodeFX1E(opcode); break;
                    case 0x0029: OpcodeFX29(opcode); break;
                    case 0x0033: OpcodeFX33(opcode); break;
                    case 0x0055: OpcodeFX55(opcode); break;
                    case 0x0065: OpcodeFX65(opcode); break;
                }
                break;
        }
    }

    // Clear Screen
    void Opcode00E0()
    {
        for(int i = 0; i < 64; i++)
        {
            for(int j = 0; j < 32; j++)
            {

                screenBuffer[i,j] = false;
            }
        }
    }

    // Return from subroutine
    void Opcode00EE()
    {
        // Sets program counter to address stored in the stack
        PC = stack.Pop();
    }

    // Set PC = NNN
    void Opcode1NNN(ushort opcode)
    {
        ushort NNN = (ushort)(opcode & 0x0FFF);
        PC = NNN;
    }

    // Call subroutine
    void Opcode2NNN(ushort opcode)
    {
        ushort NNN = (ushort)(opcode & 0x0FFF);
        stack.Push(PC);
        PC = NNN;
    }

    // Skip opcode if V[X] == NN
    void Opcode3XNN(ushort opcode)
    {
        byte NN = (byte)(opcode & 0x00FF);
        int X = (int)((opcode & 0x0F00) >> 8);
        if(V[X] == NN)
        {
            NextOpcode();
        }
    }

    // Skip opcode if V[X] != NN
    void Opcode4XNN(ushort opcode)
    {
        byte NN = (byte)(opcode & 0x00FF);
        int X = (int)((opcode & 0x0F00) >> 8);
        if(V[X] != NN)
        {
            NextOpcode();
        }
    }

    // Skip opcode if V[X] == V[Y]
    void Opcode5XY0(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);
        int Y = (int)((opcode & 0x00F0) >> 4);
        if(V[X] == V[Y])
        {
            NextOpcode();
        }
    }

    // Set V[X] = NN
    void Opcode6XNN(ushort opcode)
    {
        byte NN = (byte)(opcode & 0x00FF);
        int X = (int)((opcode & 0x0F00) >> 8);
        V[X] = NN;
    }

    // Add NN to V[X]
    void Opcode7XNN(ushort opcode)
    {
        byte NN = (byte)(opcode & 0x00FF);
        int X = (int)((opcode & 0x0F00) >> 8);
        V[X] += NN;
    }

    // Set V[X] = V[Y]
    void Opcode8XY0(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);
        int Y = (int)((opcode & 0x00F0) >> 4);
        V[X] = V[Y];
    }

    // Set V[X] to bitwise V[X] or V[Y]
    void Opcode8XY1(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);
        int Y = (int)((opcode & 0x00F0) >> 4);
        V[X] |= V[Y];
    }

    // Set V[X] to bitwise V[X] and V[Y]
    void Opcode8XY2(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);
        int Y = (int)((opcode & 0x00F0) >> 4);
        V[X] &= V[Y];
    }

    // Set V[X] to bitwise V[X] xor V[Y]
    void Opcode8XY3(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);
        int Y = (int)((opcode & 0x00F0) >> 4);
        V[X] ^= V[Y];
    }

    // Set V[X] to bitwise V[X] + V[Y]
    void Opcode8XY4(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);
        int Y = (int)((opcode & 0x00F0) >> 4);

        V[15] = 0;

        // Sum is tested as an int separately as it won't overflow
        if((int)V[X]+V[Y] > 255) {V[15] = 1;}

        V[X] += V[Y];
    }

    // Set V[X] to bitwise V[X] - V[Y]
    void Opcode8XY5(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);
        int Y = (int)((opcode & 0x00F0) >> 4);

        V[15] = 1;

        if(V[X] < V[Y]) {V[15] = 0;}

        V[X] -= V[Y];
    }

    // Store the least significant bit of VX in VF and then shift VX to the right by 1
    void Opcode8XY6(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        V[15] = (byte)(V[X] & 1);

        V[X] >>= 1;
    }

    // Set V[X] = V[Y] - V[X]
    void Opcode8XY7(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);
        int Y = (int)((opcode & 0x00F0) >> 4);

        V[15] = 1;

        if(V[Y] < V[X]) {V[15] = 0;}


        V[X] = (byte)(V[Y] - V[X]);
    }

    // Store the most significant bit of VX in VF and then shift VX to the left by 1
    void Opcode8XYE(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        V[15] = (byte)(V[X] >> 7);

        V[X] <<= 1;
    }

    // Skip opcode if V[X] != V[Y]
    void Opcode9XY0(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);
        int Y = (int)((opcode & 0x00F0) >> 4);
        if(V[X] != V[Y])
        {
            NextOpcode();
        }
    }

    // Set I = NNN
    void OpcodeANNN(ushort opcode)
    {
        ushort NNN = (ushort)(opcode & 0x0FFF);
        I = NNN;
    }

    // Jump to V[0] + NNN
    void OpcodeBNNN(ushort opcode)
    {
        ushort NNN = (ushort)(opcode & 0x0FFF);
        PC = (ushort)(V[0] + NNN);
    }

    // Set V[X] = random number & NN
    void OpcodeCXNN(ushort opcode)
    {
        byte NN = (byte)(opcode & 0x00FF);
        int X = (int)((opcode & 0x0F00) >> 8);

        V[X] = (byte)(rand.Next(0,255) & NN);
    }

    // Draw
    void OpcodeDXYN(ushort opcode)
    {
        int N = (byte)(opcode & 0x000F);
        int X = (int)((opcode & 0x0F00) >> 8);
        int Y = (int)((opcode & 0x00F0) >> 4);

        byte xCoord = V[X];
        byte yCoord = V[Y];

        V[0x0F] = 0; // Reset carry flag

        for(int i = 0; i < N; i++)
        {
            //Debug.Log(I.ToString("X4") + " + " + i.ToString("X4") + " = " + (I + i).ToString("X4"));
            byte pixelData = RAM[I + i];

            for(int j = 0; j < 8; j++)
            {
                int bit = pixelData & (1 << 7 - j);

                if (xCoord + j > 63 || yCoord + i > 31)
                {
                    break;
                }
                else if(bit != 0)
                {
                    if(V[15] == 0 && screenBuffer[xCoord + j, yCoord + i])
                    {
                        V[15] = 1;
                    }
                    screenBuffer[xCoord + j, yCoord + i] = !screenBuffer[xCoord + j, yCoord + i];
                }
            }
        }
    }

    void OpcodeEX9E(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        if(pressedKey == V[X])
        {
            NextOpcode();
        }
    }

    void OpcodeEXA1(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        if(pressedKey != V[X])
        {
        

            NextOpcode();
        }
    }

    void OpcodeFX07(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        V[X] = (byte)delay;
    }

        void OpcodeFX0A(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        paused = true;
        X_paused = X;
        // rest of the functionality is handled in Tick()
    }

        void OpcodeFX15(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        delay = V[X];
    }

    void OpcodeFX18(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        sound = V[X];
    }

    void OpcodeFX1E(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        I += V[X];
    }

    void OpcodeFX29(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        I = (ushort)(0x050 + (V[X] * 5));
    }

    void OpcodeFX33(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        int bcd = V[X];
        int unit = bcd % 10;
        int ten = ((bcd - unit) % 100) / 10;
        int hundred = ((bcd - ten - unit) % 1000) / 100;

        RAM[I] = (byte)hundred;
        RAM[I+1] = (byte)ten;
        RAM[I+2] = (byte)unit;
    }

    void OpcodeFX55(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        for(int i = 0; i <= X; i++)
            {
                RAM[I + i] = V[i];
            }
    }

    
    void OpcodeFX65(ushort opcode)
    {
        int X = (int)((opcode & 0x0F00) >> 8);

        for(int i = 0; i <= X; i++)
            {
                V[i] = RAM[I + i];
            }
    }

    /*public void DebugPrint()
    {
        Debug.Log("PC: " + PC
        + "\n Delay: " + delay
        + "\n Sound: " + sound
        + "\n\n V[0]: " + V[0]
        + "\n V[1]: " + V[1]
        + "\n V[2]: " + V[2]
        + "\n V[3]: " + V[3]
        + "\n V[4]: " + V[4]
        + "\n V[5]: " + V[5]
        + "\n V[6]: " + V[6]
        + "\n V[7]: " + V[7]
        + "\n V[8]: " + V[8]
        + "\n V[9]: " + V[9]
        + "\n V[10]: " + V[10]
        + "\n V[11]: " + V[11]
        + "\n V[12]: " + V[12]
        + "\n V[13]: " + V[13]
        + "\n V[14]: " + V[14]
        + "\n V[15]: " + V[15]);
    }*/
}
