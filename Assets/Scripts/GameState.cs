using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class GameState : MonoBehaviour
{
    public Text scoreText;
    public Transform levelParent;

    public GameObject currentLevel;
    public List<GameObject> levels;

    private int levelNumber;

    private int oldPoints = 0;

    private List<GameObject> points = new List<GameObject>();

    void Update()
    {
        scoreText.text = $"Points: {points.Count + oldPoints}";

		Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		    Application.Quit();
#endif
        }

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

    public void OnNextLevel()
    {
        oldPoints += points.Count;
        points.Clear();
        Destroy(currentLevel);
        currentLevel = Instantiate(levels[levelNumber], levelParent);
        levelNumber = (levelNumber + 1) % levels.Count();
        currentLevel.SetActive(true);
    }

    public Transform GetStartPoint()
    {
        var transforms = from transform in currentLevel.GetComponentsInChildren<Transform>()
                         where transform.gameObject.CompareTag("Start")
                         select transform;
        return transforms.First();
    }
}
