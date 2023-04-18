using UnityEngine;

public class Sonification : MonoBehaviour
{

	public enum SonificationType {
		Simple, Psychoacoustic, Spatial
	}
	 
	public float x = 0;
	public float y = 0;
	
	[Header("Sonification Settings")]
	public SonificationType sonification = SonificationType.Simple;
	public bool sequential = false;
	public float errorMargin = 5.0f; // Error margin in degrees
	public float advanceMargin = 1.0f; // Error margin allowed to move to the next axis, in degrees
	public float lockinDelay = 0.5f; // Time in seconds that the user needs to keep the catheter within the error margin before the axis changes in a sequential sonification
    public float outOfMarginDelay = 0.1f; // Time in seconds that the user needs to be outside of the error margin before the axis changes back to the faulty axis in a sequential sonification
    public float MAX_DISTANCE = 1f;
	public float MIN_DISTANCE = 1f;
    public double[] spatialChord = { 440.0, 554.37, 659.25 }; //A4, C#5, E5 (A Major Chord)
	public float THRESHOLD = 0.5f; // The threshold in degrees that a movement needs to surpass to be sonified

	[Header("Sonification Feedback Settings")]
    public double[] sequentialAdvanceChord = { 1108.73, 1396.91, 1760.00 }; //C#6, F6, A6 (C# Augmented Chord)
    public double[] sequentialReverseChord = { 69.30, 82.41, 98.00 }; //C#2, E2, G2 (C# Diminished Chord)
	public float chordDuration = 1.0f; //How long the chord will play for, in seconds

    // Sequential/Parallel
    private int currentAxis = 0;
    private bool playChord = false;
	private float chordTime = 0;
	private float lockinTime = 0;
	private bool lockingIn = false;


    [Header("Sonification Data")]
	public Transform catheter;
	public Collider patientSkull;
	public Vector3 entryPoint;
	public Transform camera;
	public Transform catheterSoundTransform;
	
	// Data Variables
	private Vector3 targetNormal;
	private Vector3 targetPoint;
	private Vector3 angles;

	// Sonification Instruments
	private ShepardTone shepard;
	private SimpleTone simple;
	private SpatialTone spatial;

	// Status noises
	private ChordTone sequentialFeedbackAdvance;
    private ChordTone sequentialFeedbackReverse;

    // Start is called before the first frame update
    void Start()
    {
		shepard = new ShepardTone(30, 0.5f, 48000, 12);
		simple = new SimpleTone(130, 0.25f, 48000, 1.0f, GetComponent<AudioSource>());
		spatial = new SpatialTone(spatialChord, 0.5f, 48000);

		sequentialFeedbackAdvance = new ChordTone(sequentialAdvanceChord, 0.5f, 48000, false);
        sequentialFeedbackReverse = new ChordTone(sequentialReverseChord, 0.5f, 48000, false);

        angles = Vector3.zero;
		
		// Calculate the target alignment
		RaycastHit hit;
		if(patientSkull.Raycast(new Ray(entryPoint - new Vector3(1,1,1), new Vector3(1,1,1)), out hit, 10.0f)){
			if(!hit.point.Equals(entryPoint)) Debug.LogWarning("Something went wrong with the raycast!");
			
			targetNormal = hit.normal;
			targetPoint = hit.point;
		}
		else{
			Debug.LogError("Could not find the surface normal of the patient's skull!");
			
			targetNormal = Vector3.zero;
		}
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
        /*Debug.DrawRay(targetPoint, targetNormal, Color.red);

		Debug.DrawRay(catheter.position, catheter.forward, Color.green);
        Debug.DrawRay(catheter.position, catheter.right, Color.red);
        Debug.DrawRay(catheter.position, catheter.up, Color.blue);

        Debug.DrawRay(catheter.position, targetNormal, Color.yellow);*/
		
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


        Debug.DrawRay(catheter.position, targetNormal, Color.yellow);

        angles.x = Mathf.Abs(Mathf.Abs(yaw) - Mathf.Abs(angles.x)) > THRESHOLD / 180f ? yaw : angles.x;
        angles.y = Mathf.Abs(Mathf.Abs(pitch) - Mathf.Abs(angles.y)) > THRESHOLD / 180f ? pitch : angles.y;

		if (sequential)
		{
			if(currentAxis == 0)
			{
				if(Mathf.Abs(angles.y) < advanceMargin / 180f) //We are within an acceptable difference
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
        float d = Mathf.Sqrt(Mathf.Pow(angles.x, 2f) + Mathf.Pow(angles.y, 2f)) / Mathf.Sqrt(2); // Calculate the angle distance and normalize it
        //float d = Vector3.Angle(catheter.up, targetNormal) / 180f;
        float r = MAX_DISTANCE * d + MIN_DISTANCE; // Calculate the radius so that the sound is near at 1 and far away at angle distance 0
        Vector3 pos = (new Vector3(Mathf.Sin(targetPitch * RAD_CON) * Mathf.Cos(targetYaw * RAD_CON), Mathf.Cos(targetPitch * RAD_CON), Mathf.Sin(targetPitch * RAD_CON) * Mathf.Sin(targetYaw * RAD_CON))) * r;
		pos += camera.position; // Center on the camera
        transform.position = pos;

        // Calculate the catheter sounds position
        pos = (new Vector3(Mathf.Sin(catPitch * RAD_CON) * Mathf.Cos(catYaw * RAD_CON), Mathf.Cos(catPitch * RAD_CON), Mathf.Sin(catPitch * RAD_CON) * Mathf.Sin(catYaw * RAD_CON))) * r;
		pos += camera.position;
        catheterSoundTransform.position = pos;

        Debug.Log(angles);
		//Debug.Log("\t Play Chord: " + playChord + ", Axis:" + currentAxis + ", Time Left: " + (chordDuration - (Time.time - chordTime)));
    }
	
	void OnAudioFilterRead(float[] data, int channels){
		Vector3 pos = angles;
		
		Instrument instrument = getInstrument();
		instrument.sampleInstrument(data, channels, pos);

		if (playChord)
		{
			float[] buff = new float[data.Length];
			if(currentAxis == 0) sequentialFeedbackReverse.sampleInstrument(buff, channels, pos);
			else sequentialFeedbackAdvance.sampleInstrument(buff, channels, pos);

			for (int i = 0; i < data.Length; i++)
			{
				data[i] = buff[i];
			}
		}
	}
}
