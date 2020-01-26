using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class GameState : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("UI Text object for displaying points")]
    public Text scoreText;
    [Tooltip("UI Text object for displaying time elapsed in current level")]
    public Text timeText;

    [Header("Levels")]
    [Tooltip("GameObject to which the levels are attached to")]
    public Transform levelParent;
    [Tooltip("Reference to current level")]
    public GameObject currentLevel;
    [Tooltip("Array of Prefabs with levels")]
    public List<GameObject> levels;

    // Current level
    private int levelNumber;

    // Points collected in previous levels
    private int oldPoints = 0;

    // List of collected points. Used for resetting points on death
    private List<GameObject> points = new List<GameObject>();

    private float levelStartTime;

    void Start()
    {
        levelStartTime = Time.time;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    void Update()
    {
        int totalPoints = points.Count + oldPoints;
        scoreText.text = $"Points: {totalPoints}";
        
        float elapsedTime = Time.time - levelStartTime;
        timeText.text = $"Time: {elapsedTime.ToString("00.0000")}";

        // Pressing Escpe exits the game
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

    // Executed when player collides with a point pickup
    public void OnPickupCollision(GameObject gameObject)
    {
        gameObject.SetActive(false);
        points.Add(gameObject);
    }

    // Executed when player falls out of bounds and dies
    public void OnPlayerDeath()
    {
        foreach (GameObject o in points)
        {
            o.SetActive(true);
        }
        points.Clear();
    }

    // Runs when player touches the level end marker
    public void OnNextLevel()
    {
        oldPoints += points.Count;

        points.Clear();

        Destroy(currentLevel);

        currentLevel = Instantiate(levels[levelNumber], levelParent);

        levelNumber = (levelNumber + 1) % levels.Count();

        currentLevel.SetActive(true);

        levelStartTime = Time.time;
    }

    // Returns the position of player starting point
    public Transform GetStartPoint()
    {
        var transforms = from transform in currentLevel.GetComponentsInChildren<Transform>()
                         where transform.gameObject.CompareTag("Start")
                         select transform;
        return transforms.First();
    }
}
