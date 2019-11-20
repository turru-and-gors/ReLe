
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*! \brief Describe an agent's index and reward for
 * further sorting purposes.
 */
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

/*! \brief Comparing class, useful when sorting Scores
 * in descending order.
 */
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
    public Text resultLabel;

    private GameObject[] car;               /*!< Set of agents */

    private bool done = false;              /*!< The epoch has finished */
    private bool stopIterating = false;     /*!< The training process has finished */
    private readonly float timeToRefresh = 0.2f;     /*!< Time between animated frames */
    private float timeCounter = 0;          /*!< Timer */
    
    private int epochCounter = 1;           /*!< Current number of epochs */

    // ========================================================================
    // ==========                   PUBLIC METHODS                   ==========
    // ========================================================================

    /*! \brief Cancel training, compute and show current results.
     */
    public void StopTraining()
    {
        // Use half the agents to get the average steer factor.
        int agentsToKeep = numAgents / 2;
        int[] best = GetBestIndividuals(agentsToKeep);
        float averageWeight = 0;
        for(int i=0; i<agentsToKeep; i++)
            averageWeight += car[best[i]].GetComponent<TrainingCar>().GetSteeringFactor();
        averageWeight /= agentsToKeep;

        stopIterating = true;

        resultLabel.text = averageWeight.ToString();
        Globals.steeringValue = averageWeight;
    }


    // ========================================================================
    // ==========                MONO BEHAVIOR METHODS               ==========
    // ========================================================================

    /*! \brief Initialization function.
     */
    private void Start()
    {
        // Create and initialize agents
        car = new GameObject[numAgents];
        CreateCars();
        InitializeCars();

        // Display number of epochs
        epochLabel.text = "Epoch " + epochCounter.ToString() + "/" + epochs.ToString();
    }


    /*! \brief Update scene.
     */
    private void Update()
    {
        // If training is done, do not update scene.
        if (stopIterating) return;

        // This epoch has finished, then start a new one or
        // finish the program
        if (done)
        {
            // Make a new road to keep training
            road.PopulateRoad();
            road.DrawRoad();

            // Create a new generation of agents
            NewGeneration();
            InitializeCars();

            // Camera goes back to its initial position
            mCamera.ResetCamera();

            // This epoch is done, count another one
            epochCounter++;
            // If all epochs are done, stop training and show results
            if (epochCounter > epochs)
            {
                StopTraining();
                return;
            }

            // Update epochs label
            epochLabel.text = "Epoch " + epochCounter.ToString() + "/" + epochs.ToString();

            // Start a new epoch, a new timer.
            done = false;
            timeCounter = 0;
        }

        // Count time
        timeCounter += Time.deltaTime;
        // When refresh is needed
        if (timeCounter >= timeToRefresh)
        {
            // Update cars and camera position
            MoveCars();
            UpdateLabelInCars();
            mCamera.MoveCameraUp(1);

            timeCounter = 0;
        }
    }


    // ========================================================================
    // ==========                   TRAINING PROCESS                 ==========
    // ========================================================================

    /*! \brief Update all agents positions and rewards.
     */
    private void MoveCars()
    {
        done = true;
        for (int i = 0; i < numAgents; i++)
        {
            // If the current car is not enabled, ignore
            TrainingCar tCar = car[i].GetComponent<TrainingCar>();
            if (!tCar.GetEnabled()) continue;

            // Map car's current position (continuous) to its position
            // inside the road matrix (discrete). If the car is outside
            // the map, disable it (not a good agent).
            Vector2 carPos = tCar.GetCurrentPosition();
            carPos = PositionToIndex(carPos);
            if (carPos.x < 0 || carPos.x >= road.width ||
                carPos.y < 0 || carPos.y >= road.length)
            {
                tCar.SetEnabled(false);
                continue;
            }

            // Make the car move using the road matrix value as 
            // steering force.
            tCar.ActOnInput(road.At((ulong)carPos.y, (ulong)carPos.x));
            // Update the car's reward.
            tCar.SetReward(ComputeReward(tCar.GetInitialPosition().x,
                                          tCar.GetCurrentPosition().x)
                          );

            // If at least one car is enabled, the epoch is not finished.
            done = false;
        }
    }

    /*! \brief The reward for this problem is the current horizontal position of 
     * the car respect to its initial position. Maximize this reward to let
     * the agent reproduce.
     */
    private float ComputeReward(float initialPosX, float currPosX)
    {
        return (road.width - Mathf.Abs(currPosX - initialPosX));
    }

    /*! \brief Create a new generation of agents, using the best individuals as
     * parents.
     */
    private void NewGeneration()
    {
        // We will use just half the agents to create the new generation.
        int agentsToKeep = numAgents / 2;
        // Get the indices of the best individuals (those with highest rewards).
        int[] best = GetBestIndividuals(agentsToKeep);

        // Keep only the weights (steerFactor) of the new agents.
        List<float> newAgents = new List<float>();
        for (int i = 0; i < agentsToKeep; i++)
        {
            // Reproduction occurs between i-th agent and a random one
            int index = Random.Range(0, agentsToKeep);
            if (index == i) index++;
            if (index >= agentsToKeep) index = 0;

            TrainingCar agent1 = car[best[i]].GetComponent<TrainingCar>();
            TrainingCar agent2 = car[best[index]].GetComponent<TrainingCar>();

            newAgents.Add(agent1.GenerateNewAgent(ref agent2));
            newAgents.Add(agent2.GenerateNewAgent(ref agent1));
        }

        // Create new agents using the new steering factors.
        for (int i = 0; i < newAgents.Count; i++)
            car[i].GetComponent<TrainingCar>().SetSteerFactor(newAgents[i]);
    }


    /*! \brief Find the individuals that achieved highest rewards.
     * \return Indices of the individuals containing the highest rewards.
     */
    private int[] GetBestIndividuals(int agentsToKeep)
    {
        // Make a list of individuals <index, reward>
        List<Score> agent = new List<Score>();
        for (int i = 0; i < numAgents; i++)
            agent.Add(new Score(i, car[i].GetComponent<TrainingCar>().GetReward()));

        // Sort list to get the best individuals
        CompareScore comp = new CompareScore();
        agent.Sort(comp);

        // Create an array with the best individuals
        int[] best = new int[agentsToKeep];
        for (int i = 0; i < agentsToKeep; i++)
        {
            best[i] = agent[i].index;
        }
        return best;
    }


    /*! \brief Map the car's current position (continuous) to a position in the 
     * road matrix (discrete).
     */
    private Vector2 PositionToIndex(Vector2 position)
    {
        Vector2 index;

        float X0 = transform.position.x - road.width / 2;
        float Y0 = transform.position.y;

        index.x = position.x - X0;
        index.y = position.y - Y0;

        return index;
    }


    /*! \brief Every car displays its current steering factor.
     */
    private void UpdateLabelInCars()
    {
        for(int i=0; i<numAgents; i++)
        {
            float w = car[i].GetComponent<TrainingCar>().GetSteeringFactor();
            car[i].GetComponent<TextOnSpot>().SetText(w.ToString());
        }
    }


    // ========================================================================
    // ==========                   INITIALIZATION                   ==========
    // ========================================================================
    /*! \brief Initialization function. Creates a set of GameObjects containing
     * cars (agents).
     */
    private void CreateCars()
    {
        float y = transform.position.y;
        for (int i = 0; i < numAgents; i++)
        {
            car[i] = Instantiate(agent);
            car[i].name = "car_" + i.ToString();
        }
    }

    /*! \brief Initialize the cars positions and rewards to start a new epoch.
     */
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
