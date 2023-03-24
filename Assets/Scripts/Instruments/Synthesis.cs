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
	 
	public float x = 0;
	 
	private TriangleOscillator osc;
	private SineOscillator oscSine;
	private ShepardTone shepard;
	private ExpChirpOscillator chirp;
	private ShepardChirpOscillator chirp2;

    // Start is called before the first frame update
    void Start()
    {
        osc = new TriangleOscillator(220, 0.5f, 48000);
		oscSine = new SineOscillator(220, 0.5f, 48000);
		shepard = new ShepardTone(30, 0.5f, 48000, 12);
		chirp = new ExpChirpOscillator(440, 0.5f, 48000, 8);
		chirp2 = new ShepardChirpOscillator(3.125, 0.5f, 48000, 12, 5.0 / 11.0);
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
		pos.x = x;
		
		/*chirp2.setX(pos.x);
		chirp2.sampleTone(data, channels);*/
		
		shepard.sampleInstrument(data, channels, pos);
	}
}
