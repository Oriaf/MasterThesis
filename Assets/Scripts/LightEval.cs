using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEval : MonoBehaviour
{

	public enum SonificationType {
		Simple, Psychoacoustic, Spatial
	}
	
	[Header("Sonification Settings")]
	public SonificationType sonification = SonificationType.Simple;
	
	[Header("Light EvaluationS Settings")]
	public int n = 20;
	public Vector3[] values; //2D, -1 to 1

	// Evaluation systems
	private int[] valIndex;
	private int currentTrial;
	private float time;
	private bool pause;
	private bool trainingDone;
	private bool answered;
	private Vector3 mousePos;

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
		
		valIndex = new int[n];
		int max = values.Length;
		int rep = n - max;
		bool[] seen = new bool[max];
		for(int i = 0; i < max; i++) seen[i] = false;
		
		// Generate a random order of the positions
		System.Random rand = new System.Random();
		string order = "";
		for(int i = 0; i < n; i++){
			int indx = rand.Next(max);
			if(seen[indx] && rep <= 0){
				i--;
				continue;
			}
			else if(seen[indx]) rep--;
			
			valIndex[i] = indx;
			seen[indx] = true;
			order += ", " + (indx + 1);
		}
		
		Debug.Log(order);
		Debug.Log("");
		
		mousePos = Vector3.zero;
		
		currentTrial = -1;
		time = float.NegativeInfinity;
		pause = true;
		trainingDone = false;
		answered = false;
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
	
	private int calculateField(Vector3 pos){
		// Convert to polar coordinates
		float r = Vector3.Distance(Vector3.zero, pos);
		float phi = Mathf.Atan2(pos.y, pos.x);
		if(phi < 0) phi += 2f * Mathf.PI;
		
		int quadrantBase = 0;
		if(phi < 0); //Debug.LogError("Answer " + pos + " gives an invalid phi value of " + phi);
		else if(phi < Mathf.PI / 2f){
			// First quadrant
			quadrantBase = 0;
		}
		else if(phi < Mathf.PI){
			// Second quadrant
			quadrantBase = 4;
		}
		else if(phi < Mathf.PI * 1.5f){
			// Third quadrant
			quadrantBase = 8;
		}
		else if(phi < 2f * Mathf.PI){
			// Fourth quadrant
			quadrantBase = 12;
		}
		
		int fieldIndex = 0;
		if (r < 0 || r > 1f); //Debug.LogError("Answer " + pos + " gives an invalid radius value of " + r);
		else if (r < 0.5f){
			fieldIndex = 1;
		}
		else if (r < 0.75f){
			float angle = Mathf.Repeat(phi, Mathf.PI / 2f);
			if (angle < Mathf.PI / 4f) fieldIndex = 2;
			else fieldIndex = 3;
		}
		else if (r < 1f) {
			fieldIndex = 4;
		}
		
		return quadrantBase + fieldIndex;
	}

    // Update is called once per frame
    void Update()
    {
		mousePos = Input.mousePosition;
		mousePos.x = (2f * mousePos.x) / Screen.width - 1f;
		mousePos.y = (2f * mousePos.y) / Screen.height - 1f;
	
		if(!trainingDone){
			mousePos.z = calculateField(mousePos);
			//Debug.Log(mousePos);
			
			// 3D sound
			Vector3 pos = mousePos;
			pos.z = pos.y;
			pos.y = 0;
			transform.position = pos;
			
			if(Input.GetKeyDown("space")) trainingDone = true;
		
			return;
		}
		
		if(Input.GetMouseButtonDown(0) && !answered){
			mousePos.z = calculateField(mousePos);
			Debug.Log(mousePos);
			answered = true;
			
			if(!pause){
				pause = true;
				time = Time.time;
			}
		}
	
		if(Time.time - time > 7.0f){
			if(pause){
				currentTrial++;
				Debug.Log(currentTrial + 1);
				
				//3D sound
				Vector3 pos = values[valIndex[currentTrial]];
				pos.z = pos.y;
				pos.y = 0;
				
				transform.position = pos;
				
				answered = false;
			}
		
			pause = !pause;
			time = Time.time;
		}
    }
	
	void OnAudioFilterRead(float[] data, int channels){
		if(!trainingDone){
			Instrument instrument = getInstrument();
			instrument.sampleInstrument(data, channels, mousePos);
		
			return;
		}
	
		if(!pause && currentTrial < n){
			Vector3 pos = values[valIndex[currentTrial]];
		
			Instrument instrument = getInstrument();
			instrument.sampleInstrument(data, channels, pos);
		}
	}
}
