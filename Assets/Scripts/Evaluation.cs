using UnityEngine;

public class Evaluation : MonoBehaviour
{

	public enum SonificationType {
		Simple, Psychoacoustic, Spatial
	}
	 
    [Header("Evaluations Settings")]
    public int n = 10;
    public float[] skullYaw;
    public float[] skullPitch;

    // Result variables
    private Vector3[] resPerDim; // Error per angle
    private float[] resAngle; // Error shortest angle
    private float[] resTime; // The time it took to reach the angle

    [Header("Sonification Settings")]
	public SonificationType sonification = SonificationType.Simple;
    public bool sequential = false;
    public float errorMargin = 5.0f; // Error margin in degrees
    public float advanceMargin = 1.0f; // Error margin allowed to move to the next axis, in degrees
    public float lockinDelay = 0.5f; // Time in seconds that the user needs to keep the catheter within the error margin before the axis changes in a sequential sonification
    public float outOfMarginDelay = 0.1f; // Time in seconds that the user needs to be outside of the error margin before the axis changes back to the faulty axis in a sequential sonification
    public float MAX_DISTANCE = 10f;
	public float MIN_DISTANCE = 1f;
    public double[] spatialChord = { 440.0, 554.37, 659.25 }; //A4, C#6, E5 (A Major Chord)
    public float THRESHOLD = 0.5f; // The threshold in degrees that a movement needs to surpass to be sonified

    [Header("Sonification Feedback Settings")]
    public double[] sequentialAdvanceChord = { 554.37, 698.46, 830.61 }; //C#5, F5, G#5 (C# Major Chord)
    public double[] sequentialReverseChord = { 554.37, 659.25, 830.61 }; //C#5, E5, G5 (C# Minor Chord)
    public float chordDuration = 1.0f; //How long the chord will play for, in seconds

    // Sequential/Parallel
    private int currentAxis = 0;
    private bool playChord = false;
    private float chordTime = 0;
    private float lockinTime = 0;
    private bool lockingIn = false;

    [Header("Sonification Data")]
	public Transform catheter;
    public Collider catheterCollider;
	public Transform camera;
	public Transform catheterSoundTransform;

    [Header("Sonification Target")]
    public Transform targetHole;
    public float targetDepth = 6.5f; // Target depth in cms
    private Vector3 targetPoint;
    public Transform skull;

    // Data Variables
    private Vector3 targetNormal;
	private Vector3 angles;

    // Sonification Instruments
    private ShepardTone shepard;
	private SimpleTone simple;
	private SpatialTone spatial;

    // Status noises
    private ChordTone sequentialFeedbackAdvance;
    private ChordTone sequentialFeedbackReverse;

    // Evaluation systems
    private int currentTrial;
    private float time;
    private bool end;
    private bool trainingDone;
    private bool answered;

    // Start is called before the first frame update
    void Start()
    {
		shepard = new ShepardTone(30, 0.5f, 48000, 12);
		simple = new SimpleTone(130, 0.25f, 48000, 1.0f, GetComponent<AudioSource>());
		spatial = new SpatialTone(spatialChord, 0.5f, 48000);

        sequentialFeedbackAdvance = new ChordTone(sequentialAdvanceChord, 0.5f, 48000, false);
        sequentialFeedbackReverse = new ChordTone(sequentialReverseChord, 0.5f, 48000, false);

        angles = Vector3.zero;

        resPerDim = new Vector3[n];
        resAngle = new float[n];
        resTime = new float[n];

        // Calculate the target point and normal
        calculateTarget();

        currentTrial = 0;
        time = float.NegativeInfinity;
        end = false;
        trainingDone = false;
        answered = false;
        targetNormal = Vector3.up;
    }
	
	private Instrument getInstrument(){
		Instrument instrument = simple;
	
		switch(sonification){
			case SonificationType.Simple:
				instrument = simple;
				break;
			case SonificationType.Psychoacoustic:
				instrument = shepard;
				break;
			case SonificationType.Spatial:
				instrument = spatial;
				break;
			default:
				Debug.LogError("The choosen sonification type has not been registered as an instrument!");
				break;
		}
		
		return instrument;
	}

    private void calculateTarget() {
        // Calculate the target alignment
        targetNormal = targetHole.up;
        Debug.Log("Taget Normal: " + targetNormal);

        // Calculate the target point
        targetPoint = targetHole.position - targetHole.up * (targetDepth / 100f);
        Debug.Log("Target Point: " + targetPoint);
    }

    private void prepareTrial()
    {
        // Rotate the skull appropriately
        Vector3 eul = new Vector3(skullPitch[currentTrial] + 90f, skullYaw[currentTrial], 0);
        skull.eulerAngles = eul;

        // Recalculate where the target is
        calculateTarget();

        // Set flags
        answered = false;

        // Start the timer
        time = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        /*
            1. Convert the two target vectors to spherical coordinates with the xz reference plane
            2. Take the difference between their angles to get the relative yaw and pitch
            3. Update the position of the sound for spatial reasons
        */
        const float DEG_CON = 180f / Mathf.PI;

        float catPitch = Mathf.Acos(catheter.up.y / catheter.up.magnitude) * DEG_CON; //0 - 180 degrees
        float catYaw = Mathf.Atan2(catheter.up.z, catheter.up.x) * DEG_CON; //-180 - 180 degress

        float targetPitch = Mathf.Acos(targetNormal.y / targetNormal.magnitude) * DEG_CON; //0 - 180 degrees
        float targetYaw = Mathf.Atan2(targetNormal.z, targetNormal.x) * DEG_CON; //-180 - 180 degress 

        float pitch = (targetPitch - catPitch) / 180f; //-1 to 1
        float yaw = Mathf.Repeat(360f + (targetYaw - catYaw), 360f); //0 to 360 degrees
        yaw = (yaw > 180f) ? yaw - 360f : yaw; //-180 to 180 degrees
        yaw = yaw / 180f; //-1 to 1

        angles.x = Mathf.Abs(Mathf.Abs(yaw) - Mathf.Abs(angles.x)) > THRESHOLD / 180f ? yaw : angles.x;
        angles.y = Mathf.Abs(Mathf.Abs(pitch) - Mathf.Abs(angles.y)) > THRESHOLD / 180f ? pitch : angles.y;

        if (sequential)
        {
            if (currentAxis == 0)
            {
                if (Mathf.Abs(angles.y) < advanceMargin / 180f) //We are within an acceptable difference
                {
                    if (!lockingIn)
                    {
                        lockingIn = true;
                        lockinTime = Time.time;
                    }
                    else if (Time.time - lockinTime > lockinDelay)
                    {
                        currentAxis = 1;
                        playChord = true;
                        chordTime = Time.time;

                        lockingIn = false;
                    }

                }
                else
                {
                    lockingIn = false; // We are no longer in the error margin, so we are not locking in anymore (if we previously were)
                }

                // Only the pitch matters currently
                angles.x = 0;
                catYaw = 0;
            }

            if (currentAxis == 1)
            {
                if (Mathf.Abs(angles.y) > errorMargin / 180f) //We have moved out of the acceptable margin for the first axis, go back
                {
                    if (!lockingIn)
                    {
                        lockingIn = true;
                        lockinTime = Time.time;
                    }
                    else if (Time.time - lockinTime > outOfMarginDelay)
                    {
                        currentAxis = 0;
                        playChord = true;
                        chordTime = Time.time;
                    }
                }
                else
                {
                    lockingIn = false;
                }

                angles.y = 0; // Only the yaw matters currently
                catPitch = 0;
            }


        }

        if (playChord && Time.time - chordTime > chordDuration)
        {
            playChord = false;
        }


        // Update the psotion of the sound source (for spatial)
        const float RAD_CON = Mathf.PI / 180f;

        // Calculate the target sounds position
        //float d = Mathf.Sqrt(Mathf.Pow(angles.x, 2f) + Mathf.Pow(angles.y, 2f)) / Mathf.Sqrt(2); // Calculate the angle distance and normalize it
        float d = Vector3.Angle(catheter.up, targetNormal) / 180f;
        float r = MAX_DISTANCE * d + MIN_DISTANCE; // Calculate the radius so that the sound is near at 1 and far away at angle distance 0*
        Vector3 pos = (new Vector3(Mathf.Sin(targetPitch * RAD_CON) * Mathf.Cos(targetYaw * RAD_CON), Mathf.Cos(targetPitch * RAD_CON), Mathf.Sin(targetPitch * RAD_CON) * Mathf.Sin(targetYaw * RAD_CON))) * r;
        pos += camera.position; // Center on the camera
        transform.position = pos;

        // Calculate the catheter sounds position
        pos = (new Vector3(Mathf.Sin(catPitch * RAD_CON) * Mathf.Cos(catYaw * RAD_CON), Mathf.Cos(catPitch * RAD_CON), Mathf.Sin(catPitch * RAD_CON) * Mathf.Sin(catYaw * RAD_CON))) * r;
        pos += camera.position;
        catheterSoundTransform.position = pos;

        if (!trainingDone)
        {
            if (Input.GetKeyDown("space")){
                trainingDone = true;

                Debug.Log("Starting the trial!");
                prepareTrial();
            }

            return;
        }

        if (catheterCollider.bounds.Contains(targetPoint) && !answered && !end)
        {
            // Provide feedback that the target was reached
            //TODO!

            // Calculate and print the results
            resPerDim[currentTrial] = angles * 180f;
            resAngle[currentTrial] = Vector3.Angle(catheter.up, targetNormal);
            resTime[currentTrial] = (Time.time - time);

            Debug.Log(currentTrial + ": " + resPerDim[currentTrial] + " degrees, " + resAngle[currentTrial] + " degrees, " + resTime[currentTrial] + "s");

            // Prepare for the nextion session
            answered = true;
            currentTrial++;
        }

        if (answered && Input.GetKeyDown("space") && !end)
        {
            prepareTrial();
        }

        if(currentTrial >= n)
        {
            if (!end)
            {
                Vector3 errorPerAngle = Vector3.zero;
                float errorAngle = 0f;
                float timeTaken = 0f;

                for(int i = 0; i < n; i++)
                {
                    Vector3 temp = resPerDim[i];
                    temp.x = Mathf.Abs(temp.x);
                    temp.y = Mathf.Abs(temp.y);
                    temp.z = Mathf.Abs(temp.z);

                    errorPerAngle += temp;
                    errorAngle += Mathf.Abs(resAngle[i]);
                    timeTaken += Mathf.Abs(resTime[i]);
                }

                errorPerAngle = errorPerAngle / n;
                errorAngle = errorAngle / n;
                timeTaken = timeTaken / n;

                Debug.Log("Mean: " + errorPerAngle + " degrees, " + errorAngle + " degrees, " + timeTaken + "s");
            }

            end = true;
        }

    }
	
	void OnAudioFilterRead(float[] data, int channels){
		Vector3 pos = angles;

        if (!answered && currentTrial < n)
        {
            if (playChord)
            {
                if (currentAxis == 0) sequentialFeedbackReverse.sampleInstrument(data, channels, pos);
                else sequentialFeedbackAdvance.sampleInstrument(data, channels, pos);
            }
            else
            {
                Instrument instrument = getInstrument();
                instrument.sampleInstrument(data, channels, pos);
            }
        }
	}
}
