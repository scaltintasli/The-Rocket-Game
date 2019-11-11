using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    Rigidbody rigidBody;
    AudioSource audioSource;

    bool musicOneTime;
    bool CollisionsAreEnabled = true;

    public enum State { Alive, Dying, Trascending }
    public State state = State.Alive;

    [SerializeField] float levelLoadDelay = 2f;
    [SerializeField] float rcsThrust = 250f;
    [SerializeField] float mainThrust = 2f;
    [SerializeField] AudioClip mainEngineSound;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip SuccessSound;

    [SerializeField] ParticleSystem mainEngineParticle;
    [SerializeField] ParticleSystem deathParticle;
    [SerializeField] ParticleSystem successParticle;



    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        musicOneTime = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            RespondToThrustInput();
            RespondToRotateInput();
        }

        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            LoadNextScene();
        }
        else if(Input.GetKeyDown(KeyCode.C))
        {
            CollisionsAreEnabled = !CollisionsAreEnabled;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(state != State.Alive || !CollisionsAreEnabled)
        {
            return;
        }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                //do nothing
                break;
            case "Finish":
                StartSuccessSeq();
                break;
            default:
                StartDyingSeq();
                break;
        }
    }

    private void StartSuccessSeq()
    {
        state = State.Trascending;
        audioSource.Stop();
        audioSource.PlayOneShot(SuccessSound);
        successParticle.Play();
        Invoke("LoadNextScene", levelLoadDelay); //parametries the time
    }

    private void StartDyingSeq()
    {
        state = State.Dying;
        audioSource.Stop();
        mainEngineParticle.Stop();
        audioSource.PlayOneShot(deathSound);
        deathParticle.Play();
        Invoke("LoadFirstLevel", levelLoadDelay);
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextScene()
    {
        int currentscene = SceneManager.GetActiveScene().buildIndex;
        if (currentscene != SceneManager.sceneCountInBuildSettings - 1)
        {
            SceneManager.LoadScene(currentscene + 1); // todo allow more than 2 levels.
        }
        else
        {
            LoadFirstLevel();
        }
    }

    private void RespondToThrustInput()
    {
        float thrustThisFrame = mainThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            audioSource.Stop();
            mainEngineParticle.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(mainEngineSound);
        mainEngineParticle.Play();
    }

    private void RespondToRotateInput()
    {
        rigidBody.freezeRotation = true;

        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        rigidBody.freezeRotation = false;
    }
}
