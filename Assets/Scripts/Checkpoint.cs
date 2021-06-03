using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{

    private CheckpointManager checkpointManager;
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            checkpointManager.PlayersThroughCheckpoint(this,other.transform);
        }
    }

    public void SetCheckpointManager(CheckpointManager checkpointManager)
    {
        this.checkpointManager = checkpointManager;
    }
}
