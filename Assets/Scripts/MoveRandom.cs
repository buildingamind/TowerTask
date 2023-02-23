using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveRandom : MonoBehaviour
{
    // This is a rougher version of Random Walk. this will just randomly 


    public NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InvokeRepeating(nameof(ChooseNewLocation), 1f, 1f);
    }

    private void Update()
    {
        //print(agent.isOnNavMesh);
    }

    // Update is called once per frame
    void ChooseNewLocation()
    {
        agent.Move(new Vector3(Random.Range(-1, 2), 0, Random.Range(-1, 2)));
    }
}
