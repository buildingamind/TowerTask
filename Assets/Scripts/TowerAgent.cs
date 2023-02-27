using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine.AI;

public class TowerAgent : Agent
{
    [ReadOnly]
    public bool requestDecision = false;

    [ReadOnly]
    public float lifetime;

    [HideInInspector]
    public Camera[] cams;

    // Eye Info
    public int MaxEyeRotation = 30; // This is degrees from 0. So 90 would mean -45 to 45.
    public float CurrentEyeInRotation; // This will keep track of our Y rotation so that it's not a problem with Euler integration.

    public GameObject LeftEyeGameobject;
    public Camera LeftEyeCamera;
    public CameraSensorComponent LeftEyeSensor;

    public GameObject RightEyeGameObject;
    public Camera RightEyeCamera;
    public CameraSensorComponent RightEyeSensor;

    public Camera SpectatorCamera; // Has a larger FoV to simulate both the eyes it has.

    [HideInInspector]
    public Collider agentCollider;
    public NavMeshAgent agent;

    private Vector3 spawnPoint;

    [ReadOnly]
    public BehaviorParameters behaviorParameters;

    [ReadOnly]
    public int step;
    [ReadOnly]
    public int episode;
    [ReadOnly]
    public float CumulativeReward;

    public float MoveSpeed;
    public float RotateSpeed;

    public HallwayManager hallwayManager;

    private void Awake()
    {
        LeftEyeSensor.Camera = LeftEyeCamera;
        RightEyeSensor.Camera = RightEyeCamera;
        agentCollider = GetComponent<Collider>();
        cams = GetComponentsInChildren<Camera>();
        behaviorParameters = GetComponent<BehaviorParameters>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isEditor)
        {
            behaviorParameters.Model = null; // Force this so that we can train a model instead
        }
        spawnPoint = transform.position;
        agent = GetComponent<NavMeshAgent>();
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        Respawn();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        LeftEyeSensor.Camera = LeftEyeCamera;
        RightEyeSensor.Camera = RightEyeCamera;

        lifetime += Time.deltaTime;
        AddReward(Time.deltaTime * StaticManager.rewardConfiguration.rewardPerSecond);

        if (!requestDecision)
        {
            transform.position += MoveSpeed * Time.deltaTime * transform.forward;
        }
    }

    public static float Sigmoid(float x)
    {
        return (1.0f / (1.0f + Mathf.Exp(-x))) - 0.5f;
    }

    // ML AGENTS CODE //////////////////////////////////////////////////

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!requestDecision)
        {
            return;
        }

        if (StaticManager.rewardConfiguration == null) { return; }

        // Actions ////////////////////
        if (actions.DiscreteActions.Length > 0) // Move
        {
            if (actions.DiscreteActions[0] == 0) return;

            if (actions.DiscreteActions[0] == -1 && hallwayManager.GoalCondition == "left")
            {
                AddReward(StaticManager.rewardConfiguration.correctReward);
                CumulativeReward += StaticManager.rewardConfiguration.correctReward;
            }
            else if (actions.DiscreteActions[0] == 1 && hallwayManager.GoalCondition == "right")
            {
                AddReward(StaticManager.rewardConfiguration.correctReward);
                CumulativeReward += StaticManager.rewardConfiguration.correctReward;
            }
            else
            {
                AddReward(StaticManager.rewardConfiguration.incorrectReward);
                CumulativeReward += StaticManager.rewardConfiguration.incorrectReward;
            }

            EndEpisode();
            Respawn();
            hallwayManager.RandomizeConfiguration();
            step++;  // Advance the agent's step count for us. 
            //transform.position += transform.forward * actions.DiscreteActions[0] * MoveSpeed * Time.deltaTime;
            //AddReward(StaticManager.rewardConfiguration.actionPenalty);
        }
        //
        if (actions.DiscreteActions.Length > 1) // Rotate
        {
            this.transform.Rotate(transform.up * actions.DiscreteActions[1] * RotateSpeed);
            AddReward(StaticManager.rewardConfiguration.actionPenalty);
        }
        //
        if (actions.DiscreteActions.Length > 2) // EyeRotation
        {
            float inputEyeRotation = (Sigmoid(actions.DiscreteActions[2]) + 0.5f) * MaxEyeRotation;
            LeftEyeGameobject.transform.localRotation = Quaternion.Euler(new Vector3(0, transform.forward.y - inputEyeRotation, 0));
            RightEyeGameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, transform.forward.y + inputEyeRotation, 0));
            CurrentEyeInRotation = inputEyeRotation;
        }
        //////////////////////////////

        if (StaticManager.logUtility != null)
        {
            StaticManager.logUtility.AddRecord(0, step);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (!requestDecision)
        {
            return;
        }

        ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;

        if (actionsOut.DiscreteActions.Length > 0)
        {
            if (Input.GetKey(KeyCode.D))
            {
                discreteActionsOut[0] = 1;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                discreteActionsOut[0] = -1;
            }
            else
            {
                discreteActionsOut[0] = 0;
            }


            // 
            /*// MOVE /////////////////////
            if (Input.GetKey(KeyCode.W))
            {
                discreteActionsOut[0] = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                discreteActionsOut[0] = -1;
            }
            else
            {
                discreteActionsOut[0] = 0;
            }*/
        }

        if (actionsOut.DiscreteActions.Length > 1)
        {
            // ROTATE ///////////////////
            if (Input.GetKey(KeyCode.D))
            {
                discreteActionsOut[1] = 1;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                discreteActionsOut[1] = -1;
            }
            else
            {
                discreteActionsOut[1] = 0;
            }
        }

    }

    public void ResetEyeRotation()
    {
        LeftEyeGameobject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        RightEyeGameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    void Respawn()
    {
        agent.Warp(spawnPoint);
        agent.enabled = true;
        requestDecision = false;
    }

    private void OnDrawGizmos()
    {
        GameObject currentTarget;
        RaycastHit hit;
        Vector3 agentPos = transform.position;

        for (int i = 1; i < 100; i++)
        {
            if (Physics.SphereCast(agentPos + (transform.forward * i), i, transform.forward, out hit, 1000, layerMask: LayerMask.GetMask("Targetable")))
            {
                currentTarget = hit.transform.gameObject;
                Debug.DrawLine(transform.position, currentTarget.transform.position, Color.green);
                break;
            }
        }

        Debug.DrawLine(LeftEyeGameobject.transform.position, LeftEyeGameobject.transform.position + LeftEyeGameobject.transform.forward, Color.blue);
        Debug.DrawLine(RightEyeGameObject.transform.position, RightEyeGameObject.transform.position + RightEyeGameObject.transform.forward, Color.blue);
    }


/*    public void OnCollisionEnter(Collision other)
    {
        string otherTag = other.gameObject.tag;
        if (otherTag.Equals("left") || otherTag.Equals("right"))
        {
            // Add the CorrectReward Value if Correct; Add the IncorrectReward Value if Incorrect
            AddReward(otherTag.Equals(hallwayManager.GoalCondition) ? StaticManager.rewardConfiguration.correctReward : StaticManager.rewardConfiguration.incorrectReward);
            // End the episode
            gameObject.transform.rotation = Quaternion.identity;
            episode++;
            step = 0;
            EndEpisode();
            hallwayManager.RandomizeConfiguration();
            Respawn();
        }
    }*/

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("end"))
        {
            requestDecision = true;
        }
    }
}
