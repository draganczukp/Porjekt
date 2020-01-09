using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
   
    private int score { get; set; }

   public void OnCoinPickup()
    {
        this.score++;
    }

    internal void OnPickupCollision(GameObject gameObject)
    {
        OnCoinPickup();
        gameObject.SetActive(false);
    }
}
