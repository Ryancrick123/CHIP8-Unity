using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    Interpreter chip8 = new Interpreter();
    GameObject[,] pixels = new GameObject[64,32];
    public GameObject pixel;
    //where true is a black pixel and false is a white pixel
    bool[,] currentScreen = new bool[64,32];
    public bool debugMode;


    // Start is called before the first frame update
    void Start()
    {
        chip8.OpenFile("FILE PATH GOES HERE");
        chip8.LoadFont();
        ScreenSetup();

    }
    
    void Update()
    {
        if(!debugMode)
        {
            KeyPress();
            chip8.Tick();
            if(chip8.playSound)
            {
                Debug.Log("Beep");
            }
        }
        else
        {
            if(Input.GetKeyDown("j"))
            {
                //chip8.DebugPrint();
            }
            else if(Input.GetKeyDown("k"))
            {
                chip8.Tick();
            }
        }

        
        
    }

    void FixedUpdate()
    {
        Draw(chip8.screenBuffer);
    }

    
    void ScreenSetup()
    {
        for(int i = 0; i < 64; i++)
        {
            for(int j = 0; j < 32; j++)
            {
                pixels[i,j] = Instantiate(pixel,new Vector3((i-31.5f), (-j+15.5f), 0), Quaternion.identity);
                currentScreen[i,j] = false;
            }
        }
    }

    // Flips pixels to match the screen buffer
    void Draw(bool[,] buffer)
    {
        for(int i = 0; i < 64; i++)
        {
            for(int j = 0; j < 32; j++)
            {
                if(buffer[i,j] != currentScreen[i,j])
                {
                    PixelFlip(i, j);
                }
            }
        }
    }

    void PixelFlip(int column, int row)
    {
        SpriteRenderer pixelSprite = pixels[column,row].GetComponent<SpriteRenderer>();

        if(pixelSprite.color == Color.black)
        {
            pixelSprite.color = Color.white;
            currentScreen[column, row] = false;
        }
        else
        {
            pixelSprite.color = Color.black;
            currentScreen[column, row] = true;
        }
    }

    
    void KeyPress()
    {
        foreach(KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(key) == true)
            {
                if(key == KeyCode.Alpha1) { chip8.pressedKey = 0x01; return; }
                else if(key == KeyCode.Alpha2) { chip8.pressedKey = 0x02; return; }
                else if(key == KeyCode.Alpha3) { chip8.pressedKey = 0x03; return; }
                else if(key == KeyCode.Alpha4) { chip8.pressedKey = 0x0C; return; }
                else if(key == KeyCode.Q) { chip8.pressedKey = 0x04; return; }
                else if(key == KeyCode.W) { chip8.pressedKey = 0x05; return; }
                else if(key == KeyCode.E) { chip8.pressedKey = 0x06; return; }
                else if(key == KeyCode.R) { chip8.pressedKey = 0x0D; return; }
                else if(key == KeyCode.A) { chip8.pressedKey = 0x07; return; }
                else if(key == KeyCode.S) { chip8.pressedKey = 0x08; return; }
                else if(key == KeyCode.D) { chip8.pressedKey = 0x09; return; }
                else if(key == KeyCode.F) { chip8.pressedKey = 0x0E; return; }
                else if(key == KeyCode.Z) { chip8.pressedKey = 0x0A; return; }
                else if(key == KeyCode.X) { chip8.pressedKey = 0x00; return; }
                else if(key == KeyCode.C) { chip8.pressedKey = 0x0B; return; }
                else if(key == KeyCode.V) { chip8.pressedKey = 0x0F; return; }
            }
        }

        chip8.pressedKey = null;
    }


}
