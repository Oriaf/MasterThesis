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
		
		currentTrial = -1;
		time = float.NegativeInfinity;
		pause = true;
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
		if(Time.time - time > 7.0f){
			if(pause){
				currentTrial++;
				Debug.Log(currentTrial + 1);
			}
		
			pause = !pause;
			time = Time.time;
		}
    }
	
	void OnAudioFilterRead(float[] data, int channels){
		if(!pause && currentTrial < n){
			Vector3 pos = values[valIndex[currentTrial]];
		
			Instrument instrument = getInstrument();
			instrument.sampleInstrument(data, channels, pos);
		}
	}
}
