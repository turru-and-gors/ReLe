using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public float stepSize = 1;

    protected Vector2 initialPos;
    protected Vector2 currentPos;
    protected float steerFactor = 1f;

    
    // =====> PUBLIC METHODS
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

    public void ActOnInput(float value)
    {
        if (value < 0) SteerLeft();
        else if (value > 0) SteerRight();
        else GoStraight();

        transform.position = currentPos;
    }


    // =====> PRIVATE MONO BEHAVIOUR METHODS
    private void Start()
    {
        OnStart();
    }

    // =====> OTHER PRIVATE METHODS
    protected virtual void OnStart()
    {
        initialPos = transform.position;
    }

    /*! \brief GO STRAIGHT increments only the Y position
     * on fixed-size steps (constant velocity).
     */
    protected void GoStraight()
    {
        currentPos.y += stepSize;
    }

    /*! \brief STEER RIGHT increments both X and Y positions.
     * Y position increments on fixed-size steps, 
     * X position increments depend on the steer factor.
     */
    protected void SteerRight()
    {
        currentPos.y += stepSize;
        currentPos.x += stepSize + steerFactor;
    }

    /*! \brief STEER LEFT increments both X and Y positions.
     * Y position increments on fixed-size steps, 
     * X position increments depend on the steer factor.
     */
    protected void SteerLeft()
    {
        currentPos.y += stepSize;
        currentPos.x -= (stepSize - steerFactor);
    }
}
