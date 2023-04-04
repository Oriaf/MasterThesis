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
	
	private void calculateField(Vector3 pos){
		float d = Vector3.Distance(Vector3.zero, pos);
		
		
	
		if(d < 0.5f){
		
		}
		else if (d < 0.75f){
		
		}
		else if (d < 1f) {
		
		}
		else {
		
		}
	}

    // Update is called once per frame
    void Update()
    {
		mousePos = Input.mousePosition;
		mousePos.x = (2f * mousePos.x) / Screen.width - 1f;
		mousePos.y = (2f * mousePos.y) / Screen.height - 1f;
	
		if(!trainingDone){
			//Debug.Log(mousePos);
			Vector3 pos = mousePos;
			pos.z = pos.y;
			pos.y = 0;
			transform.position = pos;
			
			if(Input.GetKeyDown("space")) trainingDone = true;
		
			return;
		}
		
		if(Input.GetMouseButtonDown(0) && !answered){
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
