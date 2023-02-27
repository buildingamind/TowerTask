using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class HallwayManager : MonoBehaviour
{
    public GameObject TowerAgent;
    public TowerAgent TowerAgentScript;
    public NavMeshAgent TowerNavMeshAgent;
    public GameObject HallwayUnitPrefab;
    public GameObject HallwayEndPrefab;

    public NavMeshSurface surface;

    [Range(1, 20)] // If you need more than 20 segments, change these ranges appropriately.
    [Header("[?] Hallway Configuration:")]
    [Tooltip("How many units long should the hallway section be?")]
    public int HallwayLength;

    [Header("[?] Pillar Configuration:")]
    [Tooltip("Declare a random seed to reproduce configurations")]
    [Range(0, 999)]
    public int RandomSeed = 0;

    [Tooltip("Force this number of Left Pillars to be enabled")]
    [Range(0, 20 * 4)] // 20 segments | 4 pillars per side
    public int MinimumLeftPillars;

    [Tooltip("Force this number of Right Pillars to be enabled")]
    [Range(0, 20 * 4)]  // 20 segments | 4 pillars per side
    public int MinimumRightPillars;

    [Range(0, 20 * 8)] // 20 segments | 8 pillars per segment
    [Tooltip("What is the minimum number of pillars to present? (Forced to be >= Left + Right)")]
    public int MinimumTotalPillars = 0;

    [Tooltip("Should we avoid the case where the pillar count is equal?")]
    public bool AvoidEqualPillarCounts = false;

    [Header("ReadOnly:")]
    [ReadOnly]
    public string GoalCondition;
    [ReadOnly]
    public int LeftPillarsCount;
    [ReadOnly]
    public int RightPillarsCount;
    [ReadOnly]
    public GameObject HallwayEndObject;
    [ReadOnly]
    public List<GameObject> HallwaySegments;
    [ReadOnly]
    public List<GameObject> leftPillarPositions;
    [ReadOnly]
    public List<GameObject> rightPillarPositions;

    [HideInInspector]
    public GameObject HallwayendObject;

    private void Awake()
    {
        StaticManager.hallwayManagers.Add(this);
    }

    private void Start()
    {
        TowerAgentScript = TowerAgent.GetComponent<TowerAgent>();
        TowerAgentScript.hallwayManager = this;
        TowerNavMeshAgent = TowerAgent.GetComponent<NavMeshAgent>();

        BuildTower();
        if (surface is null){
            surface = GetComponentInParent<NavMeshSurface>();
        }
        if (surface is null)
        {
            surface = GetComponent<NavMeshSurface>();
        }
    }

    public void BuildTower()
    {
        HallwayEndObject = Instantiate(HallwayEndPrefab, transform.forward * (HallwayLength +1), this.transform.rotation, this.gameObject.transform);
        HallwayEndObject.name = "HallwayEnd";
    }

    private void Update()
    {
        ManageTowerSize();
        ManagePillarCount();
        ManageCurrentGoalState();
    }

    public void RandomizeConfiguration()
    {
        HallwayLength = Random.Range(4, 8);
        MinimumTotalPillars = Random.Range(1, 7);
    }

    private void ManageCurrentGoalState()
    {
        if (LeftPillarsCount == RightPillarsCount)
        {
            GoalCondition = "equal";
        }
        else
        {
            GoalCondition = LeftPillarsCount > RightPillarsCount ? "left" : "right";
        }
    }

    private void ManageTowerSize()
    {
        float unitScale = this.transform.localScale.x;

        bool updateRequired = false;

        while (HallwaySegments.Count > HallwayLength)
        {
            Destroy(HallwaySegments[HallwaySegments.Count - 1]);
            HallwaySegments.RemoveAt(HallwaySegments.Count - 1);
            updateRequired = true;
            break;
        }
        while (HallwaySegments.Count < HallwayLength)
        {
            HallwaySegments.Add(Instantiate(HallwayUnitPrefab, transform.forward * ((HallwaySegments.Count + 1) * unitScale), this.transform.rotation, this.gameObject.transform));
            updateRequired = true;
            break;
        }

        for (int i = 0; i < HallwaySegments.Count; i++)
        {
            if (HallwaySegments[i].transform.position != this.transform.position + transform.forward * ((i + 1) * unitScale))
            {
                HallwaySegments[i].transform.position = this.transform.position + transform.forward * ((i + 1) * unitScale);
                HallwaySegments[i].name = string.Format("Hallway_{0}", i + 1);
                HallwaySegments[i].GetComponent<PillarPositionReference>().hallwayManager = this;
            }
        }
        HallwayEndObject.transform.position = this.transform.position + transform.forward * ((HallwayLength + 1) * unitScale);

        if (updateRequired)
        {
            Invoke(nameof(RebuildNavMesh), 0.1f);
        }
    }

    public void ManagePillarCount()
    {
        int numAvailablePillars = leftPillarPositions.Count + rightPillarPositions.Count;
        MinimumLeftPillars = Mathf.Clamp(MinimumLeftPillars, 0, leftPillarPositions.Count);
        MinimumRightPillars = Mathf.Clamp(MinimumRightPillars, 0, rightPillarPositions.Count);
        MinimumTotalPillars = Mathf.Clamp(MinimumTotalPillars, MinimumLeftPillars + MinimumRightPillars, numAvailablePillars);

        if (GetPillarCount() != MinimumTotalPillars || LeftPillarsCount != MinimumLeftPillars || RightPillarsCount != MinimumRightPillars)
        {
            foreach (GameObject pillar in leftPillarPositions)
            {
                pillar.SetActive(false);
            }
            foreach (GameObject pillar in rightPillarPositions)
            {
                pillar.SetActive(false);
            }
            EnableRandomPillars();
        }
    }

    void RebuildNavMesh()
    {
        surface.navMeshData = null;
        surface.BuildNavMesh();
        surface.UpdateNavMesh(surface.navMeshData);
        //Debug.Log("[NavMesh] Baked");
    }

    public List<GameObject> EnableRandomPillars()
    {
        List<GameObject> totalPositions = new();
        totalPositions.AddRange(leftPillarPositions);
        totalPositions.AddRange(rightPillarPositions);

        if (MinimumTotalPillars >= totalPositions.Count)
        {
            foreach (GameObject obj in totalPositions)
            {
                obj.SetActive(true);
            }
            return new List<GameObject>();
        }
        else
        {
            List<GameObject> selectedObjects = new List<GameObject>();
            Random.InitState(RandomSeed);

            // Start with Manually Forced Left Pillars
            int leftCount = 0;
            while (leftCount < MinimumLeftPillars)
            {
                int randomIndex = Random.Range(0, leftPillarPositions.Count);
                GameObject randomObject = leftPillarPositions[randomIndex];
                if (!selectedObjects.Contains(randomObject))
                {
                    selectedObjects.Add(randomObject);
                    randomObject.SetActive(true);
                    leftCount++;
                }
            }

            // Then Add the Manually Forced Right Pillars
            int rightCount = 0;
            while (rightCount < MinimumRightPillars)
            {
                int randomIndex = Random.Range(0, rightPillarPositions.Count);
                GameObject randomObject = rightPillarPositions[randomIndex];
                if (!selectedObjects.Contains(randomObject))
                {
                    selectedObjects.Add(randomObject);
                    randomObject.SetActive(true);
                    rightCount++;
                }
            }

            // Add any remaining Pillars
            while (selectedObjects.Count < MinimumTotalPillars)
            {
                int randomIndex = Random.Range(0, totalPositions.Count);
                GameObject randomObject = totalPositions[randomIndex];
                if (!selectedObjects.Contains(randomObject))
                {
                    selectedObjects.Add(randomObject);
                    randomObject.SetActive(true);
                }
            }
            return selectedObjects;
        }
    }

    public int GetPillarCount()
    {
        int leftPillars = 0;
        int rightPillars = 0;
        int totalPillarSpaces = 0;

        foreach (GameObject pillar in leftPillarPositions)
        {
            if (pillar.activeSelf)
            {
                leftPillars++;
            }
            totalPillarSpaces++;
        }
        foreach (GameObject pillar in rightPillarPositions)
        {
            if (pillar.activeSelf)
            {
                rightPillars++;
            }
            totalPillarSpaces++;
        }
        LeftPillarsCount = leftPillars;
        RightPillarsCount = rightPillars;
            
        // If we want to avoid equal length pillars, then we need to account for how many we can place as well before adding to the minimum
        if (AvoidEqualPillarCounts && (LeftPillarsCount == RightPillarsCount) && (LeftPillarsCount + RightPillarsCount) < (leftPillarPositions.Count + rightPillarPositions.Count))
        {
            MinimumTotalPillars++;
        }
        return leftPillars + rightPillars;
    }
}
