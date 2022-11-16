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
        setPlayerID();
        setPrivacyPolicy();

        PlayerPrefs.Save();

        if (PlayerPrefs.HasKey("playerID"))
        {
            print(PlayerPrefs.GetString("playerID"));
        }

    }

    string InterpretRegex(string regex)
    {
        const string upperCase = "ABCDEFGHIJKLMNOPQERSTUVWXYZ";
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string nums = "0123456789";

        List<string> needs = new List<string>();
        List<string> reps = new List<string>();

        foreach (char c in regex)
        {
            Console.WriteLine(regex);
            if (c == '[')
            {

                string need = regex.Substring(regex.IndexOf(c), regex.IndexOf(']') + 1);
                Console.WriteLine(need);
                string newRegex = regex.Remove(regex.IndexOf(c), regex.IndexOf(']') + 1);
                regex = newRegex;

                needs.Add(need);
            }
            else if (c == '{')
            {

                string rep = regex.Substring(regex.IndexOf(c), regex.IndexOf('}') + 1);
                Console.WriteLine(rep);
                string newRegex = regex.Remove(regex.IndexOf(c), regex.IndexOf('}') + 1);
                regex = newRegex;

                reps.Add(rep);
            }
        }

        System.Random rnd = new System.Random();
        string interpretedRegex = "";

        for (int x = 0; x < needs.Count; x++)
        {
            if (needs[x].Contains("A-Z"))
            {
                string num = reps[x].Substring(1, reps[x].Length - 2);
                int repeater = Convert.ToInt32(num);

                for (int z = 0; z < repeater; z++)
                {
                    interpretedRegex += upperCase[rnd.Next(0, upperCase.Length - 1)];
                }
            }
            else if (needs[x].Contains("a-z"))
            {
                string num = reps[x].Substring(1, reps[x].Length - 2);
                int repeater = Convert.ToInt32(num);

                for (int z = 0; z < repeater; z++)
                {
                    interpretedRegex += lowerCase[rnd.Next(0, lowerCase.Length - 1)];
                }
            }
            else if (needs[x].Contains("0-9"))
            {
                string num = reps[x].Substring(1, reps[x].Length - 2);
                int repeater = Convert.ToInt32(num);

                for (int z = 0; z < repeater; z++)
                {
                    interpretedRegex += nums[rnd.Next(0, nums.Length - 1)];
                }
            }
        }

        return interpretedRegex;
    }

    void setPlayerID()
    {
        if (PlayerPrefs.HasKey("playerID"))
        {
            PlayerPrefs.SetString("playerID", InterpretRegex("[A-Z]{4}[0-9]{4}[a-z]{4}"));
        }
        else
        {
            PlayerPrefs.SetString("playerID", InterpretRegex("[A-Z]{4}[0-9]{4}[a-z]{4}"));
        }
    }

    void setPrivacyPolicy()
    {
        if (PlayerPrefs.HasKey("PrivacyPolicy"))
        {
            PlayerPrefs.SetInt("PrivacyPolicy", 0);
        }
        else
        {
            PlayerPrefs.SetInt("PrivacyPolicy", 0);
        }
    }
}
