using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class SavingSystem : MonoBehaviour
{

    string CAPS = "ABCDEFGHIJKLMNOPQSTUVWXYZ";
    string numbers = "0123456789";
    string smallCaps = "abcdefghijklmnpqrstuvwxyz";


    // Start is called before the first frame update
    void Start()
    {
        generateUserID();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void generateUserID()
    {
        string ID = "";

        for (int i = 0; i <= 3; i++)
        {
            int ranInt = UnityEngine.Random.Range(0, CAPS.Length);

            foreach (char c in CAPS)
            {
                if (CAPS.IndexOf(c) == ranInt)
                {
                    ID += c;
                }
            }
        }

        for (int x = 0; x <= 3; x++)
        {
            int ranInt = UnityEngine.Random.Range(0, numbers.Length);
            foreach (char c in numbers)
            {
                if (CAPS.IndexOf(c) == ranInt)
                {
                    ID += c;
                }
            }
        }

        for (int y = 0; y <= 3; y++)
        {
            int ranInt = UnityEngine.Random.Range(0, smallCaps.Length);
            foreach (char c in smallCaps)
            {
                if (CAPS.IndexOf(c) == ranInt)
                {
                    ID += c;
                }
            }
        }

        print(ID);
    }
}
