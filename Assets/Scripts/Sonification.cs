using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
	
	[Header("Sonification Data")]
	public Transform catheter;
	public Collider patientSkull;
	public Vector3 entryPoint;
	
	// Data Variables
	private Vector3 targetNormal;
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
		spatial = new SpatialTone(0.5f, 48000);
		
		angles = Vector3.zero;
		
		// Calculate the target alignment
		RaycastHit hit;
		if(patientSkull.Raycast(new Ray(entryPoint - new Vector3(1,1,1), new Vector3(1,1,1)), out hit, 10.0f)){
			if(!hit.point.Equals(entryPoint)) Debug.LogWarning("Something went wrong with the raycast!");
			
			targetNormal = hit.normal;
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
		//Debug.Log(catheter.up + ", " + targetNormal);
		
		/*
			1. Project both vectors onto the x-z plane
			2. Calculating the required yaw angle
			3. Rotate catheter vector by the required yaw angle
			4. Calculate the required pitch angle
			
			Pitch and yaw will always be the shortest angle to the target angle
			Pitch increases as the catheter's tip moves towards the world's positive y-axis
			Yaw increases as the catheter's tip moves in the x-z plane towards the negative z-axis clockwise
		*/
		
		// 1. Project both vectors onto the x-z plane
		/*Vector3 catProj = Vector3.ProjectOnPlane(catheter.up, Vector3.up);
		Vector3 targetProj = Vector3.ProjectOnPlane(targetNormal, Vector3.up);*/
        //Vector3 catProj = Vector3.ProjectOnPlane((new Vector3(1, 1, 1)).normalized, Vector3.up);
        //Vector3 targetProj = Vector3.ProjectOnPlane((new Vector3(-1, -1, 1)).normalized, Vector3.up);

		const float DEG_CON = 180f / Mathf.PI;

		Debug.Log(catheter.up);
		float catPitch = Mathf.Atan2(catheter.up.y, catheter.up.x) * DEG_CON;
        float catYaw = Mathf.Atan2(catheter.up.z, catheter.up.x) * DEG_CON;
		Debug.Log(catYaw + ", " + catPitch);

        float targetPitch = Mathf.Atan2(targetNormal.y, targetNormal.x) * DEG_CON;
        float targetYaw = Mathf.Atan2(targetNormal.z, targetNormal.x) * DEG_CON;
        Debug.Log("\t" + targetYaw + ", " + targetPitch);

		float yaw = targetYaw - catYaw;
		float pitch = targetPitch - catPitch;
        angles.x = Mathf.Abs(Mathf.Abs(yaw) - Mathf.Abs(angles.x)) > 1.8f ? yaw : angles.x;
        angles.y = Mathf.Abs(Mathf.Abs(pitch) - Mathf.Abs(angles.y)) > 1.8f ? pitch : angles.y;


        Debug.Log("\t" + angles.x + ", " + angles.y);

        // 2. Calculating the required yaw angle
        //float yaw = Vector3.SignedAngle(catProj, targetProj, Vector3.up);
        //Debug.Log(catProj + ", " + targetProj + ", " + yaw);
        //angles.x = Mathf.Abs(Mathf.Abs(yaw) - Mathf.Abs(angles.x)) > 1.8f ? yaw : angles.x;

		
		// 3. Rotate catheter vector by the required yaw angle
		/*Quaternion rot = new Quaternion();
		rot.eulerAngles = new Vector3(0, yaw, 0);
		Vector3 rotCat = rot * catheter.up;
		// 4. Calculate the required pitch angle
		float pitch = Vector3.SignedAngle(rotCat, targetNormal, Vector3.up);*/
        //catProj = Vector3.ProjectOnPlane((new Vector3(1, 1, 1)).normalized, Vector3.forward);
        //targetProj = Vector3.ProjectOnPlane((new Vector3(-1, -1, 1)).normalized, Vector3.forward);
        // 4. Calculate the required pitch angle
        //float pitch = Vector3.SignedAngle(catProj, targetProj, Vector3.forward);
        //angles.y = Mathf.Abs(Mathf.Abs(pitch) - Mathf.Abs(angles.y)) > 1.8f ? pitch : angles.y;
        //Debug.Log("\t" + catProj + ", " + targetProj + ", " + pitch);


        // Update the psotion of the sound source (for spatial)
        const float RAD_CON = Mathf.PI / 180f;
		//Vector3 pos = new Vector3(Mathf.Sin(angles.y * RAD_CON) * Mathf.Cos(angles.x * RAD_CON), Mathf.Cos(angles.y * RAD_CON), Mathf.Sin(angles.y * RAD_CON) * Mathf.Sin(angles.x * RAD_CON));

		/*float normalYaw = Vector3.SignedAngle(Vector3.right, targetProj, Vector3.up);
		rot.eulerAngles = new Vector3(0, -normalYaw, 0);
		Vector3 rotNorm = rot * targetNormal;
		float normalPitch = Vector3.SignedAngle(Vector3.right, rotNorm, Vector3.up);*/

        Vector3 pos = new Vector3(Mathf.Sin(angles.y * RAD_CON) * Mathf.Cos(angles.x * RAD_CON), Mathf.Cos(angles.y * RAD_CON), Mathf.Sin(angles.y * RAD_CON) * Mathf.Sin(angles.x * RAD_CON));
        transform.localPosition = pos;

		
		//Debug.Log(angles / 180f);
    }
	
	void OnAudioFilterRead(float[] data, int channels){
		Vector3 pos = angles / 180f;
		
		Instrument instrument = getInstrument();
		instrument.sampleInstrument(data, channels, pos);
	}
}
