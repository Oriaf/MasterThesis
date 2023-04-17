using UnityEngine;

public class MediumEval : MonoBehaviour
{

	public enum SonificationType {
		Simple, Psychoacoustic, Spatial
	}
	 
    [Header("Medium Evaluations Settings")]
    public int n = 20;
    private Vector3[] values; // Unit vectors

    // Result variables
    private Vector3[] resPerDim; // Error per angle
    private float[] resAngle; // Error shortest angle
    private float[] resTime; // The time it took to reach the angle

    [Header("Sonification Settings")]
	public SonificationType sonification = SonificationType.Simple;
    public bool sequential = false;
    public float errorMargin = 5.0f; // Error margin in degrees
    public float advanceMargin = 1.0f; // Error margin allowed to move to the next axis, in degrees
    public float MAX_DISTANCE = 1f;
	public float MIN_DISTANCE = 1f;
    public double[] spatialChord = { 440.0, 554.37, 659.25 }; //A4, C#6, E5 (A Major Chord)

    [Header("Sonification Data")]
	public Transform catheter;
	public Transform camera;
	public Transform catheterSoundTransform;
	
	// Data Variables
	private Vector3 targetNormal;
	private Vector3 angles;

    // Sequential/Parallel
    private int currentAxis = 0;

    // Sonification Instruments
    private ShepardTone shepard;
	private SimpleTone simple;
	private SpatialTone spatial;

    // Evaluation systems
    private int currentTrial;
    private float time;
    private bool pause;
    private bool trainingDone;
    private bool answered;

    // Start is called before the first frame update
    void Start()
    {
		shepard = new ShepardTone(30, 0.5f, 48000, 12);
		simple = new SimpleTone(130, 0.25f, 48000, 1.0f, GetComponent<AudioSource>());
		spatial = new SpatialTone(spatialChord, 0.5f, 48000);
		
		angles = Vector3.zero;

		values = new Vector3[n];
        resPerDim = new Vector3[n];
        resAngle = new float[n];
        resTime = new float[n];

        // Calculate the target alignment
        for (int i = 0; i < n; i++)
		{
			values[i] = Random.onUnitSphere;
            Debug.Log(values[i]);
		}

        currentTrial = 0;
        time = float.NegativeInfinity;
        pause = false;
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

        angles.x = Mathf.Abs(Mathf.Abs(yaw) - Mathf.Abs(angles.x)) > 0.01f ? yaw : angles.x;
        angles.y = Mathf.Abs(Mathf.Abs(pitch) - Mathf.Abs(angles.y)) > 0.01f ? pitch : angles.y;

        if (sequential)
        {
            if (currentAxis == 0)
            {
                if (Mathf.Abs(angles.y) < advanceMargin / 180f) //We are within an acceptable difference, move on to the next step
                {
                    currentAxis = 1;
                }
                else
                {
                    // Only the pitch matters currently
                    angles.x = 0;
                    catYaw = 0;
                }
            }

            if (currentAxis == 1)
            {
                if (Mathf.Abs(angles.y) > errorMargin / 180f) //We have moved out of the acceptable margin for the first axis, go back
                {
                    currentAxis = 0;

                    // Only the pitch matters then
                    angles.x = 0;
                    catYaw = 0;
                }
                else
                {
                    angles.y = 0; // Only the yaw matters currently
                    catPitch = 0;
                }
            }
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

                targetNormal = values[0];
                time = Time.time;
            }

            return;
        }

        if (Input.GetKeyDown("space") && !answered)
        {
            resPerDim[currentTrial] = angles * 180f;
            resAngle[currentTrial] = Vector3.Angle(catheter.up, targetNormal);
            resTime[currentTrial] = (Time.time - time);

            Debug.Log(currentTrial + ": " + resPerDim[currentTrial] + " degrees, " + resAngle[currentTrial] + " degrees, " + resTime[currentTrial] + "s");

            answered = true;

            currentTrial++;
            pause = true;
            time = Time.time;
        }

        if (pause && Time.time - time > 3.0f)
        {
            answered = false;
            targetNormal = values[currentTrial];

            pause = !pause;
            time = Time.time;
        }

        if(currentTrial >= n)
        {
            if (pause)
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

            answered = true;
            pause = false;
        }

    }
	
	void OnAudioFilterRead(float[] data, int channels){
		Vector3 pos = angles;

        if (!pause && currentTrial < n)
        {
            Instrument instrument = getInstrument();
            instrument.sampleInstrument(data, channels, pos);
        }
	}
}
