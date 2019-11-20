
using UnityEngine;

public class TestCar : MonoBehaviour
{
    public GameObject car;
    public MovingCamera mCamera;
    public Joystick joystick;

    private Car agent;
    private float timeToRefresh = 0.2f;
    private float timeCounter = 0;
    private bool roadEnded = false;
    private bool testEnded = false;

    public void EndTest()
    {
        testEnded = true;
    }


    private void Start()
    {
        agent = car.GetComponent<Car>();
        agent.SetSteerFactor(Globals.steeringValue);
    }

    private void Update()
    {
        if (testEnded) return;

        // The road is not infinite. When you reach the end, 
        // go back to the start and keep testing. The user
        // won't notice...
        if(roadEnded)
        {
            // Reset positions of car and camera
            Vector2 pos = agent.GetInitialPosition();
            pos.x = agent.GetCurrentPosition().x;
            agent.SetInitialPosition(pos);
            mCamera.ResetCamera();

            roadEnded = false;
        }

        // Timer
        timeCounter += Time.deltaTime;
        // If time to refresh the scene
        if(timeCounter >= timeToRefresh)
        {
            // Get the input force from the joystick
            int force = GetJoystickInput();
            // Move the car, applying the force value
            agent.ActOnInput(force);

            Vector2 pos = agent.GetCurrentPosition();
            if (pos.y >= 285) roadEnded = true;

            // Move camera
            mCamera.MoveCameraUp(1);

            timeCounter = 0;
        }
    }

    /*! \brief Use the joystick to get the steering force
     */
    private int GetJoystickInput()
    {
        // The 0.4f value is to adjust the sensibility of the joystick
        if (joystick.Horizontal >= 0.4f) return 1;
        if (joystick.Horizontal <= -0.4f) return -1;
        return 0;
    }
}
