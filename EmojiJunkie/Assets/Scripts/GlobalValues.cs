using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalValues
{
    private static string _playerID = "";

    public static string PlayerID
    { get { return _playerID; } set { _playerID = value; } }

}
