using System;
using UnityEngine;

public class Minigame_1Obstacle : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        MinigameManager_1.i.PlayerHit();
    }
}
