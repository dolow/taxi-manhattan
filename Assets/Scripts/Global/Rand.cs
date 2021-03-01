using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rand
{
    private static System.Random instance = null;

    public static void InitIfNeeded()
    {
        if (Rand.instance != null)
            return;

        int seed = (int)(DateTimeOffset.Now.ToUnixTimeSeconds() % 10000);
        Rand.instance = new System.Random(seed);
    }

    public static int Next(int until)
    {
        Rand.InitIfNeeded();
        return Rand.instance.Next(until);
    }
}
