using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarPositionReference : MonoBehaviour
{
    // This is to be placed on each HallwayUnit Prefab so that it has coordinate references to possible pillar placement
    public List<GameObject> leftPillars;
    public List<GameObject> rightPillars;

    public HallwayManager hallwayManager;

    private void Start()
    {
        hallwayManager.leftPillarPositions.AddRange(leftPillars);
        hallwayManager.rightPillarPositions.AddRange(rightPillars);
    }

    private void OnDestroy()
    {
        foreach (GameObject pillar in leftPillars)
        {
            hallwayManager.leftPillarPositions.Remove(pillar);
        }
        foreach (GameObject pillar in rightPillars)
        {
            hallwayManager.rightPillarPositions.Remove(pillar);
        }
    }
}
