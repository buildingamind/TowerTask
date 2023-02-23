using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticManager : MonoBehaviour
{
    // To make variables easier to share, we set up this StaticManager, which contains static references to every script
    // You may always reference the StaticManager to reference indivdual instances of these scripts, which allows
    // for us to set public variables on those scripts, yet access them statically here.
    public static List<HallwayManager> hallwayManagers = new();

    public static RewardConfiguration rewardConfiguration;
    public static LogUtility logUtility;

    public static TextManager textManager;
}
