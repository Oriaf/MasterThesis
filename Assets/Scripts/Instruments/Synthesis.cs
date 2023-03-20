using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Synthesis : MonoBehaviour
{

	/*
	 	Simple:
			Triangle Wave Oscillator or Sine Wave Oscillator
				Equal Temperament Midi notes
				
			Vary Pitch (130 Hz to 523 Hz; 251 Hz at th center)
			Panning from left to right
			
			Multiple tones
				Major Chord
				
		Psychoacoustic:
			Shepard Tone
				Change between rise and fall
				Change frequency of the rise/fall
				
				Vary gain
				Vary inharmonicity, roughness and noisiness
				
		Spatial Audio:
			Some repeating sound
			Steam Audio
			HRTF?
	 */
	 
	private TriangleOscillator osc;
	private SineOscillator oscSine;
	private ShepardTone shepard;
	private ExpChirpOscillator chirp;

    // Start is called before the first frame update
    void Start()
    {
        osc = new TriangleOscillator(220, 0.5f, 48000);
		oscSine = new SineOscillator(220, 0.5f, 48000);
		shepard = new ShepardTone(3.125, 0.5f, 48000, 12);
		chirp = new ExpChirpOscillator(440, 0.5f, 48000, 8);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void OnAudioFilterRead(float[] data, int channels){
		//osc.sampleTone(data, channels);
		//oscSine.sampleTone(data, channels);
		//chirp.sampleTone(data, channels);
		
		Vector3 pos = Vector3.zero;
		pos.x = 0.1f;
		
		shepard.sampleInstrument(data, channels, pos);
		
		//double maxAmp = shepard.getMaxAmp();
		for(int i = 0; i < data.Length; i++){
			data[i] = data[i] / (float) (12);
		}
	}
}
