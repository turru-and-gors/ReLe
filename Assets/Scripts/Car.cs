
using UnityEngine;

public class Car : MonoBehaviour
{
    public float stepSize = 1;

    // Don't want the user to change these values from the GUI
    // during testing, so they are hidden to the user.
    // The values will be accessible for the children classes.
    protected Vector2 initialPos;
    protected Vector2 currentPos;
    protected float steerFactor = 1f;

    
    // =====> PUBLIC METHODS
    /*! \brief Resets the car's initial and current position.
     */
    public void SetInitialPosition(Vector2 pos)
    {
        initialPos = pos;
        transform.position = initialPos;
        currentPos = initialPos;
    }

    public Vector2 GetInitialPosition()
    {
        return initialPos;
    }

    public Vector2 GetCurrentPosition()
    {
        return currentPos;
    }

    public void SetSteerFactor(float value)
    {
        steerFactor = value;
    }

    public float GetSteeringFactor()
    {
        return steerFactor;
    }

    /*! \brief The car will move forward, and depending on the
     * force recived, will probably steer left or right.
     * \param value Force applied to the car, causing a steering effect.
     */
    public void ActOnInput(float value)
    {
        currentPos.y += stepSize;
        currentPos.x += stepSize * (value + SteerFunction(value));

        transform.position = currentPos;
    }

    /*! \brief Apply the nullify-force effect
     * \param value Force applied to the car, causing a steering effect.
     */
    private float SteerFunction(float value)
    {
        return value * steerFactor;
    }


    // =====> PRIVATE MONO BEHAVIOUR METHODS
    /*! \brief The Car class is designed to be further extended using
     * inheritance. The inherited class will not have access to this class,
     * so we will use OnStart insted.
     */
    private void Start()
    {
        OnStart();
    }

    // =====> OTHER PRIVATE METHODS
    /*! \brief Initialization function. To be overrided by the children.
     */
    protected virtual void OnStart()
    {
        initialPos = transform.position;
    }
}
