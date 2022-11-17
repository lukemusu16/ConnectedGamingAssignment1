using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalValues
{
    private static bool _launchCheck = true;
    private static string _playerID = "";

    public static bool LaunchCheck
    { get { return _launchCheck; } set { _launchCheck = value; } }

    public static string PlayerID
    { get { return _playerID; } set { _playerID = value; } }

}
