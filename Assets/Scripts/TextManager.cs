using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextManager : MonoBehaviour
{
    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        StaticManager.textManager = this;
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = string.Format("Total Hallway Length: {0} unit{1}\nLeft-Side Pillars: {2} (Requring: {3})\nRight-Side Pillars: {4} (Requring: {5})\nTotal Pillars: {6}/{7}\nRandom Seed: {8}\nAvoid Equal Pillar Count: {9}",
            StaticManager.hallwayManagers[0].HallwayLength,
            StaticManager.hallwayManagers[0].HallwayLength > 1 ? "s" : "",
            StaticManager.hallwayManagers[0].LeftPillarsCount,
            StaticManager.hallwayManagers[0].MinimumLeftPillars,
            StaticManager.hallwayManagers[0].RightPillarsCount,
            StaticManager.hallwayManagers[0].MinimumRightPillars,
            StaticManager.hallwayManagers[0].MinimumTotalPillars,
            StaticManager.hallwayManagers[0].leftPillarPositions.Count + StaticManager.hallwayManagers[0].rightPillarPositions.Count,
            StaticManager.hallwayManagers[0].RandomSeed,
            StaticManager.hallwayManagers[0].AvoidEqualPillarCounts
            );
    }
}
