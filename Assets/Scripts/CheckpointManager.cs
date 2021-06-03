﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private List<Transform> playerTransformList;

    private List<Checkpoint> checkpointList;
    private List<int> nextCheckpointIndexList;

    public delegate void OnPlayerCorrectCheckpoint();
    public event OnPlayerCorrectCheckpoint OnPlayerCorrectCheckpointEvent;

    public delegate void OnPlayerWrongCheckpoint();
    public event OnPlayerWrongCheckpoint OnPlayerWrongCheckpointEvent;

    private void Awake()
    {
        Transform checkpointsTransform = transform.Find("Checkpoints");
        checkpointList = new List<Checkpoint>();
        foreach (Transform checkpointTransform in checkpointsTransform)
        {
            Checkpoint checkpoint = checkpointTransform.GetComponent<Checkpoint>();
            checkpoint.SetCheckpointManager(this);
            checkpointList.Add(checkpoint);
        }
        nextCheckpointIndexList = new List<int>();
        foreach (Transform playerTransform in playerTransformList)
        {
            nextCheckpointIndexList.Add(0);
        }
    }

    public void PlayersThroughCheckpoint(Checkpoint c,Transform playerTransform)
    {
        int nextCheckpointIndex = nextCheckpointIndexList[playerTransformList.IndexOf(playerTransform)];

        if (checkpointList.IndexOf(c) == nextCheckpointIndex)
        {
            //es el siguiente checkpoint
            nextCheckpointIndex = (nextCheckpointIndex + 1) % checkpointList.Count;
            OnPlayerCorrectCheckpointEvent?.Invoke();
        }
        else
        {
            //no es el siguiente checkpoint
            OnPlayerWrongCheckpointEvent?.Invoke();
        }
    }
}

