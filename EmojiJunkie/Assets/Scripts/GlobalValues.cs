using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalValues
{
    private static bool _launchCheck = true;

    public static bool LaunchCheck
    { get { return _launchCheck; } set { _launchCheck = value; } }

}
