using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateMeshRenderer : MonoBehaviour
{
    MeshRenderer meshRender;
    bool agentWithinRange;


    // Start is called before the first frame update
    void Start()
    {
        meshRender = this.GetComponent<MeshRenderer>();
        meshRender.enabled = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            meshRender.enabled = true;
        }
    }
}
