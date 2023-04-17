using System.Security.Cryptography;
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
	public float MAX_DISTANCE = 1f;
	public float MIN_DISTANCE = 1f;
    public double[] spatialChord = { 440.0, 554.37, 659.25 }; //A4, C#6, E5 (A Major Chord)

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

    // Start is called before the first frame update
    void Start()
    {
		shepard = new ShepardTone(30, 0.5f, 48000, 12);
		simple = new SimpleTone(130, 0.25f, 48000, 1.0f, GetComponent<AudioSource>());
		spatial = new SpatialTone(spatialChord, 0.5f, 48000);
		
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
		//Angle axis to undo roll?

        Debug.DrawRay(targetPoint, targetNormal, Color.red);

		Debug.DrawRay(catheter.position, catheter.forward, Color.green);
        Debug.DrawRay(catheter.position, catheter.right, Color.red);
        Debug.DrawRay(catheter.position, catheter.up, Color.blue);

        Debug.DrawRay(catheter.position, targetNormal, Color.yellow);
		
		/*
			1. Convert the two target vectors to spherical coordinates with the xz reference plane
			2. Take the difference between their angles to get the relative yaw and pitch
			3. Update the position of the sound for spatial reasons
		*/
		const float DEG_CON = 180f / Mathf.PI;

		//Up is z-axis, forward is y-axis
		//Debug.Log(catheter.up + ", " + catheter.right + ", " + catheter.forward);

        float catPitch = Mathf.Acos(catheter.up.y / catheter.up.magnitude) * DEG_CON; //0 - 180 degrees
        float catYaw = Mathf.Atan2(catheter.up.z, catheter.up.x) * DEG_CON; //-180 - 180 degress
        //Debug.Log(catYaw + ", " + catPitch);

        float targetPitch = Mathf.Acos(targetNormal.y / targetNormal.magnitude) * DEG_CON; //0 - 180 degrees
        float targetYaw = Mathf.Atan2(targetNormal.z, targetNormal.x) * DEG_CON; //-180 - 180 degress 
        //Debug.Log("\t" + targetYaw + ", " + targetPitch);

        float pitch = (targetPitch - catPitch) / 180f; //-1 to 1
        float yaw = Mathf.Repeat(360f + (targetYaw - catYaw), 360f); //0 to 360 degrees
		yaw = (yaw > 180f) ? yaw - 360f : yaw; //-180 to 180 degrees
		yaw = yaw / 180f; //-1 to 1
        //Debug.Log(yaw + ", " + pitch);


        Debug.DrawRay(catheter.position, targetNormal, Color.yellow);

        angles.x = Mathf.Abs(Mathf.Abs(yaw) - Mathf.Abs(angles.x)) > 0.01f ? yaw : angles.x;
        angles.y = Mathf.Abs(Mathf.Abs(pitch) - Mathf.Abs(angles.y)) > 0.01f ? pitch : angles.y;

        //Debug.Log("\t" + angles.x + ", " + angles.y);


        // Update the psotion of the sound source (for spatial)
        const float RAD_CON = Mathf.PI / 180f;

		// Calculate the target sounds position
		float d = Mathf.Sqrt(Mathf.Pow(angles.x, 2f) + Mathf.Pow(angles.y, 2f)) / Mathf.Sqrt(2); // Calculate the angle distance and normalize it
		float r = MAX_DISTANCE * d + MIN_DISTANCE; // Calculate the radius so that the sound is near at 1 and far away at angle distance 0*
        Vector3 pos = (new Vector3(Mathf.Sin(targetPitch * RAD_CON) * Mathf.Cos(targetYaw * RAD_CON), Mathf.Cos(targetPitch * RAD_CON), Mathf.Sin(targetPitch * RAD_CON) * Mathf.Sin(targetYaw * RAD_CON))) * r;
		pos += camera.position; // Center on the camera
        transform.position = pos;

        // Calculate the catheter sounds position
        pos = (new Vector3(Mathf.Sin(catPitch * RAD_CON) * Mathf.Cos(catYaw * RAD_CON), Mathf.Cos(catPitch * RAD_CON), Mathf.Sin(catPitch * RAD_CON) * Mathf.Sin(catYaw * RAD_CON))) * r;
		pos += camera.position;
        catheterSoundTransform.position = pos;



        Debug.Log(angles);
    }
	
	void OnAudioFilterRead(float[] data, int channels){
		Vector3 pos = angles;
		
		Instrument instrument = getInstrument();
		instrument.sampleInstrument(data, channels, pos);
	}
}
