using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class MinigameManager_1 : MonoBehaviour, ISavable
{
    [SerializeField]
    private enum GState
    {
        Idle,
        Playing,
        Dead,
        Paused
    }

    [Serializable]
    private struct LayerS
    {
        public List<GameObject> prefab, starterPrefab, spawnedPrefab;
        public float speedMultiplier;
    }

    private const float AVERAGE = 763f;
    public static MinigameManager_1 i;
    [SerializeField] private float jumpForce, groundRayDist, initSpeed, speedIncrementRate, maxSpeed, initTimeDelay, timeDelayDecrement, minTimeDelay, delayPercentRange = 50, scores, highScore, animSpeed;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Transform groundCheck, spawnPoint, despawnPoint, saqibPoint;
    [SerializeField] private List<GameObject> obstacles, spawnedObstacles, roads, starterRoads, spawnedRoads, backGrounds, starterBackGrounds, spawnedBackgrounds, skies, starterSkies, spawnedSkies;
    [SerializeField] private List<LayerS> layers;
    [SerializeField] private Sprite playerDeadSprite;
    [SerializeField] private TextMeshProUGUI scoresText, highScoresText, grade, average;
    [SerializeField] private AudioSource  endSound, bgMusic;
    [SerializeField] private GameObject gameOverUI, pauseUI, mainUI;
    [SerializeField] private new AnimBase animation;
    private float currentSpeed, currentTimeDelay, time, randTime, overTimeDelay = 1, currOTD;
    private bool jumped;
    private GState state, lastState;
    private GState State
    {
        get => state;
        set
        {
            if (state != value)
            {
                lastState = state;
                state = value;
            }
        }
    }
    Vector3 dirVect;

    private void Awake()
    {
        if (i == null) i = this;
        else Destroy(this);
    }

    private void Start()
    {
        SavingSystem.i.Load("minigame1save");
        animation.OnEnd += () => {
            State = GState.Playing;
            bgMusic.Play();

        };
        State = GState.Idle;
    }

    void Update()
    {
        if (State == GState.Playing)
        {
            ObstacleSpawn();
            ObstacleMove();
            LayerSpawn();
            LayerMove();
        }

        Scores();
        gameOverUI.SetActive(State == GState.Dead);
        mainUI.SetActive(State != GState.Paused);
        pauseUI.SetActive(State == GState.Paused);
        Time.timeScale = State == GState.Paused ? 0 : 1;
        animation.isPlaying = State == GState.Playing;
        currOTD += Time.deltaTime;
    }

    int indO, ind;
    private void LayerSpawn()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (i == 1)
                indO = ind;
            ind = i == 1 ? BackIndexSet() : UnityEngine.Random.Range(0, layers[i].prefab.Count);

            foreach (Transform child in layers[i].spawnedPrefab[layers[i].spawnedPrefab.Count - 1].transform)
            {
                if (child.name == "EndPoint" && Vector2.Distance(child.position, spawnPoint.position) < 10f)
                {
                    layers[i].spawnedPrefab.Add(Instantiate(layers[i].prefab[ind], spawnPoint));
                    layers[i].spawnedPrefab[layers[i].spawnedPrefab.Count - 1].transform.position = new Vector3(child.position.x, layers[i].spawnedPrefab[layers[i].spawnedPrefab.Count - 1].transform.position.y, 0);
                    break;
                }

                if (child.name == "EndPoint" && child.transform.position.x < spawnPoint.position.x)
                {
                    layers[i].spawnedPrefab.Add(Instantiate(layers[i].prefab[ind], spawnPoint));
                    layers[i].spawnedPrefab[layers[i].spawnedPrefab.Count - 1].transform.position = new Vector3(child.position.x, layers[i].spawnedPrefab[layers[i].spawnedPrefab.Count - 1].transform.position.y, 0);
                    break;
                }
            }
        }
    }

    private void LayerMove()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = layers[i].spawnedPrefab.Count - 1; j >= 0; j--)
            {
                foreach (Transform child in layers[i].spawnedPrefab[j].transform)
                {
                    if (child.name == "EndPoint")
                    {
                        dirVect = despawnPoint.position - new Vector3(child.position.x, layers[i].spawnedPrefab[j].transform.position.y, despawnPoint.position.z);
                        layers[i].spawnedPrefab[j].transform.position += dirVect.normalized * currentSpeed * Time.deltaTime * layers[i].speedMultiplier;

                        if (Vector2.Distance(child.position, despawnPoint.position) < 0.4f)
                        {
                            Destroy(layers[i].spawnedPrefab[j]);
                            layers[i].spawnedPrefab.RemoveAt(j);
                        }
                    }
                }
            }
        }
    }

    private int BackIndexSet()
    {
        int ind;
        ind = UnityEngine.Random.Range(0, layers[1].prefab.Count);
        if (ind == indO || (UnityEngine.Random.Range(0, 25) != 5 && ind >= 3))
            ind = BackIndexSet();

        return ind;
    }

    private void Scores()
    {
        if (State == GState.Playing)
            scores += Time.deltaTime * 12;

        if (highScore < scores)
            highScore = scores;

        scoresText.text = $"{(int)scores}";
        highScoresText.text = $"{(int)highScore}";
    }

    private void ObstacleMove()
    {
        currentSpeed = Mathf.Clamp(currentSpeed + speedIncrementRate * Time.deltaTime, 0, maxSpeed);

        for (int i = spawnedObstacles.Count - 1; i >= 0; i--)
        {
            dirVect = despawnPoint.position - spawnedObstacles[i].transform.position;
            spawnedObstacles[i].transform.position += dirVect.normalized * currentSpeed * Time.deltaTime;

            if (spawnedObstacles[i].transform.position.x < saqibPoint.position.x)
            {
                Destroy(spawnedObstacles[i]);
                spawnedObstacles.RemoveAt(i);
            }
        }
    }

    private bool randSet;
    private void ObstacleSpawn()
    {
        currentTimeDelay = Mathf.Clamp(currentTimeDelay - timeDelayDecrement * Time.deltaTime, minTimeDelay, initTimeDelay);
        time += Time.deltaTime;
        int ind = ObstacleIndexSet();
        if (!randSet)
        {
            randSet = true;
            randTime = UnityEngine.Random.Range(currentTimeDelay - (delayPercentRange * currentTimeDelay / 100f), currentTimeDelay + (delayPercentRange * currentTimeDelay / 100f));
        }

        if (time >= randTime)
        {
            randSet = false;
            spawnedObstacles.Add(Instantiate(obstacles[ind], spawnPoint));
            time = 0;
        }
    }

    private int ObstacleIndexSet()
    {
        int ind;
        ind = UnityEngine.Random.Range(0, obstacles.Count);
        if (ind == 0 && UnityEngine.Random.Range(0, 50) != 5)
            ind = ObstacleIndexSet();
        return ind;
    }

    public void Jump()
    {
        if (jumped || currOTD < overTimeDelay  || animation.PlayAnim)
            return;

        if (State == GState.Dead || State == GState.Idle)
        {
            StartGame();
            return;
        }
        if (!Physics2D.Raycast(groundCheck.position, Vector2.down, groundRayDist, layerMask) && State == GState.Playing)
            return;

        Debug.Log("f");
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        //AudioManager.inst.PlaySound(jump);
        jumped = true;
        StartCoroutine(UnJump());
    }

    private IEnumerator UnJump()
    {
        yield return new WaitForSeconds(.3f);
        jumped = false;
    }

    public void StartGame()
    {
        endSound.Stop();

        scores = 0;
        State = GState.Idle;
        currentSpeed = initSpeed;
        rb.simulated = true;
        currentTimeDelay = initTimeDelay;
        rb.transform.position = new Vector3(
            rb.transform.position.x,
            -.92f - 2.02f,
            rb.transform.position.z
            );
        animation.PlayAnim = true;
        for (int i = spawnedObstacles.Count - 1; i >= 0; i--)
        {
            Destroy(spawnedObstacles[i]);
            spawnedObstacles.RemoveAt(i);
        }

        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = layers[i].spawnedPrefab.Count - 1; j >= 0; j--)
            {
                Destroy(layers[i].spawnedPrefab[j]);
                layers[i].spawnedPrefab.RemoveAt(j);
            }
        }


        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j <= 1; j++)
            {
                layers[i].spawnedPrefab.Add(Instantiate(layers[i].starterPrefab[j], spawnPoint));
            }
        }
    }

    public void PauseGame()
    {
        if (State == GState.Paused)
        {
            State = lastState;
            if (State == GState.Playing) bgMusic.Play();

            return;
        }
        
       State = GState.Paused;
       bgMusic.Pause();
    }
    public void PlayerHit()
    {
        currOTD = 0;

        State = GState.Dead;
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;
        //AudioManager.inst.PlaySound(hit); 
        SavingSystem.i.Save("minigame1save");
        bgMusic.Stop();
        endSound.Play();

        average.text = $"You are {(scores>AVERAGE? "above " : scores<AVERAGE?"below ":"")}average";

        switch(CalculateGrade())
        {
            case 0:
                grade.text = "Grade: F";
                break;
            case 1:
                grade.text = "Grade: D";
                break;
            case 2:
                grade.text = "Grade: C";
                break;
            case 3:
                grade.text = "Grade: B";
                break;
            case 4:
                grade.text = "Grade: A";
                break;
        }
    }

    // 0 - F, 1 - D, 2 - C, 3 - B, 4 - A
    private float CalculateGrade()
    {
        float _grade = 100 * (scores / AVERAGE);
        
        if (_grade < 40)
            return 0;

        if (_grade < 80)
            return 1;

        if (_grade < 150)
            return 2;

        if (_grade < 200)
            return 3;

        if (_grade >= 200)
            return 4;

        return 0;
    }

    public object CaptureState()
    {
        return highScore;
    }

    public void RestoreState(object state)
    {
        highScore = (float)state;
    }

}
