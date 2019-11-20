
using UnityEngine;

/*! \brief Extension of Car class, for training purposes.
 */
public class TrainingCar : Car
{
    public float minSteerFactor = -1;   /*!< The steer factor minimum possible value */
    public float maxSteerFactor = 1;    /*!< The steer factor maximum possible value */

    private float m;        /*!< Slope for conversion between float and uint */
    private readonly int numBytes = 2;  /*!< Number of bytes to represent the steer factor */
    private uint maxUint;               /*!< Maximum value of representation for steer factor */
    private float probMutation = 0.1f;  /*!< Probability of mutation for the new agent */
    private bool running = true;        /*!< Is the car still enabled */
    private float reward;               /*!< Reward value (cummulative) */

    // =====> PUBLIC FUNCTIONS
    public void SetEnabled(bool enabled)
    {
        this.running = enabled;
    }

    public bool GetEnabled()
    {
        return this.running;
    }

    public void SetReward(float reward)
    {
        this.reward += reward;
    }

    public float GetReward()
    {
        return this.reward;
    }

    public void ResetReward()
    {
        this.reward = 0;
    }    

    /*! \brief Create a new agent by combining this car and another one.
     * \param other Other parent of the new agent.
     */
    public float GenerateNewAgent(ref TrainingCar other)
    {
        return Crossover(other.steerFactor);
    }

    // =====> OVERRIDE FUNCTIONS
    /*! \brief Inherited from the parent to add important
     * initialization parameters.
     */
    protected override void OnStart()
    {
        base.OnStart();

        // Representation using 2 bytes
        maxUint = (uint)Mathf.Pow(2, numBytes*8) - 1;
        //Debug.Log(maxUint);
        m = (maxSteerFactor - minSteerFactor) / maxUint;

        // The initial steer factor will be set randomly
        steerFactor = Random.Range(minSteerFactor, maxSteerFactor);
        //Debug.Log(steerFactor);
    }


    // =====> PRIVATE FUNCTIONS
    /*! \brief Convert from integer to its corresponding
     * float value.
     */
    private float ToFloat(uint value)
    {
        return (m * value + minSteerFactor);
    }

    /*! \brief Convert from float to its corresponding
     * uint representation.
     */
    private uint ToUint(float value)
    {
        float f = (value - minSteerFactor) / m;
        return (uint)(f);
    }

    /*! \brief Crossover between this and other agent.
     */
    private float Crossover(float otherSteer)
    {
        // The crossover position will be defined randomly, with
        // at least one chromosome from each agent.
        int crossoverPoint = Random.Range(1, numBytes * 8 - 2);

        // Get the UINT representation for both parents
        uint myVal = ToUint(steerFactor);
        uint otherVal = ToUint(otherSteer);

        // Get a mask for both the lower and higher part of the 
        // parents' chromosome
        uint low = (uint)Mathf.Pow(2, crossoverPoint) - 1;
        uint high = maxUint ^ low;

        // Perform the crossover, and convert to float 
        uint newVal = (myVal & high) | (otherVal & low);
        newVal = Mutate(newVal);
        return ToFloat(newVal);
    }

    /*! \brief Mutation occurs to maintain diversity within
     * the population and prevent premature convergence.
     */
    private uint Mutate(uint value)
    {
        // Check if mutation will be applied
        float prob = Random.Range(0, 1);        
        if (prob > probMutation) return value;

        // --> TODO: test mutating more than one gene <--
        // Mutate ONE random gene
        int gene = (int)Random.Range(0, numBytes * 8);
        // Generate the mutation mask
        // x XOR 0 = x
        // x XOR 1 = x'
        uint mask = (uint)(1 << gene);

        // Apply mutation
        return (value ^ mask);
    }
}
