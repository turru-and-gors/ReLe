using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

struct Score
{
    public int index;
    public float reward;

    public Score(int index, float reward)
    {
        this.index = index;
        this.reward = reward;
    }
};

class CompareScore : IComparer<Score>
{
    public int Compare(Score elem1, Score elem2)
    {
        float diff = elem1.reward - elem2.reward;
        if (diff > 0.01f) return -1;
        if (diff < -0.01f) return 1;
        return 0;
    }
}

public class Trainer : MonoBehaviour
{
    public MovingCamera mCamera;
    public TrainingRoad road;
    public GameObject agent;    
    public int numAgents = 10;
    public int epochs = 10;

    public Text epochLabel;

    private GameObject[] car;

    private bool done = false;
    private float timeToRefresh = 0.2f;
    private float timeCounter = 0;
    
    private int numIterations = 0;

    // ========================================================================
    // ==========                MONO BEHAVIOR METHODS               ==========
    // ========================================================================
    private void Start()
    {
        // Create and initialize agents
        car = new GameObject[numAgents];
        CreateCars();
        InitializeCars();

        epochLabel.text = "Epoch " + numIterations.ToString();
    }


    private void Update()
    {
        // This epoch has finished, then start a new one or
        // finish the program
        if (done)
        {
            road.PopulateRoad();
            road.DrawRoad();

            NewGeneration();
            InitializeCars();
            mCamera.ResetCamera();
            numIterations++;

            if (numIterations >= epochs)
            {
                Debug.Log("Final steering factors:");
                for (int i = 0; i < numAgents; i++)
                    Debug.Log(car[i].GetComponent<TrainingCar>().GetSteeringFactor());
                UnityEditor.EditorApplication.isPlaying = false;
            }

            epochLabel.text = "Epoch " + numIterations.ToString();
            done = false;
            timeCounter = 0;
        }

        timeCounter += Time.deltaTime;
        if (timeCounter >= timeToRefresh)
        {
            MoveCars();
            UpdateLabelInCars();
            mCamera.MoveCameraUp(1);

            timeCounter = 0;
        }
    }


    // ========================================================================
    // ==========                   TRAINING PROCESS                 ==========
    // ========================================================================
    private void MoveCars()
    {
        done = true;
        for (int i = 0; i < numAgents; i++)
        {
            TrainingCar tCar = car[i].GetComponent<TrainingCar>();
            if (!tCar.GetEnabled()) continue;

            Vector2 carPos = tCar.GetCurrentPosition();
            carPos = PositionToIndex(carPos);
            if (carPos.x < 0 || carPos.x >= road.width ||
                carPos.y < 0 || carPos.y >= road.length)
            {
                tCar.SetEnabled(false);
                continue;
            }
            tCar.ActOnInput(road.At((ulong)carPos.y, (ulong)carPos.x));
            tCar.SetReward(ComputeReward(tCar.GetInitialPosition().x,
                                          tCar.GetCurrentPosition().x)
                          );
            done = false;
        }
    }

    private float ComputeReward(float initialPosX, float currPosX)
    {
        return (road.width - Mathf.Abs(currPosX - initialPosX));
    }

    private void NewGeneration()
    {
        int agentsToKeep = numAgents / 2;

        int[] best = GetBestIndividuals(agentsToKeep);

        List<float> newAgents = new List<float>();
        for (int i = 0; i < agentsToKeep; i++)
        {
            int index = Random.Range(0, agentsToKeep);
            if (index == i) index++;
            if (index >= agentsToKeep) index = 0;

            TrainingCar agent1 = car[best[i]].GetComponent<TrainingCar>();
            TrainingCar agent2 = car[best[index]].GetComponent<TrainingCar>();

            newAgents.Add(agent1.GenerateNewAgent(ref agent2));
            newAgents.Add(agent2.GenerateNewAgent(ref agent1));
        }

        for (int i = 0; i < newAgents.Count; i++)
            car[i].GetComponent<TrainingCar>().SetSteerFactor(newAgents[i]);
    }



    private int[] GetBestIndividuals(int agentsToKeep)
    {

        List<Score> agent = new List<Score>();
        for (int i = 0; i < numAgents; i++)
            agent.Add(new Score(i, car[i].GetComponent<TrainingCar>().GetReward()));
        CompareScore comp = new CompareScore();
        agent.Sort(comp);

        int[] best = new int[agentsToKeep];
        for (int i = 0; i < agentsToKeep; i++)
        {
            best[i] = agent[i].index;
        }
        return best;
    }


    private Vector2 PositionToIndex(Vector2 position)
    {
        Vector2 index;

        float X0 = transform.position.x - road.width / 2;
        float Y0 = transform.position.y;

        index.x = position.x - X0;
        index.y = position.y - Y0;

        return index;
    }


    private void UpdateLabelInCars()
    {
        for(int i=0; i<numAgents; i++)
        {
            float d = car[i].GetComponent<TrainingCar>().GetInitialPosition().x -
                      car[i].GetComponent<TrainingCar>().GetCurrentPosition().x;
            car[i].GetComponent<TextOnSpot>().SetText(d.ToString());
        }
    }


    // ========================================================================
    // ==========                   INITIALIZATION                   ==========
    // ========================================================================
    private void CreateCars()
    {
        float y = transform.position.y;
        for (int i = 0; i < numAgents; i++)
        {
            car[i] = Instantiate(agent);
            car[i].name = "car_" + i.ToString();
        }
    }

    private void InitializeCars()
    {
        float y = transform.position.y;
        for (int i = 0; i < numAgents; i++)
        {
            float x = Random.Range(0, road.width) - road.width / 2;

            TrainingCar cAgent = car[i].GetComponent<TrainingCar>();
            cAgent.SetInitialPosition(new Vector2(x, y));
            cAgent.ResetReward();
            cAgent.SetEnabled(true);
        }
    }
}
