using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardConfiguration : MonoBehaviour
{

    public float rewardPerSecond = -0.05f;
    public float actionPenalty = 0;
    public float correctReward = 1;
    public float incorrectReward = -1;

    // Start is called before the first frame update
    void Start()
    {
        StaticManager.rewardConfiguration = this;
    }
}
