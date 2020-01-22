using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
	public Text scoreText;

	private int score { get {return points.Count; } }

	private List<GameObject> points = new List<GameObject>();

	void FixedUpdate()
	{
		scoreText.text = "Points: " + score;
	}

	public void OnPickupCollision(GameObject gameObject)
	{
		gameObject.SetActive(false);
		points.Add(gameObject);
	}

	public void OnPlayerDeath()
	{
		foreach (GameObject o in points)
		{
			o.SetActive(true);
		}
		points.Clear();
	}
}
