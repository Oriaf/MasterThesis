using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTone : Instrument
{
	// Instrument settings
	private float scale;
	private AudioSource audio;
	
	// Partial constants
	private TriangleOscillator oscillator;

	public SimpleTone(double f0, float g, double sr, float s, AudioSource aus) : base(f0, g, sr){
		scale = s;
		audio = aus;
		
		oscillator = new TriangleOscillator(f0, g, sr);
	}
	
	override public void sampleInstrument(float[] data, int channels, Vector3 pos){
		if(channels < 2) Debug.LogError("This instrument requires stereo sound for panning!");
	
		// Map y to frequency logarithmically
		float y = scale * (pos.y + 1.0f); //Go between 0 and 2 when scale is 1.0f
		oscillator.setFrequency(frequency * Mathf.Pow(2.0f, y));
		
		float pan = Mathf.Clamp(pos.x, -1.0f, 1.0f);
		
		oscillator.sampleTone(data, channels);
		for(int i = 0; i < data.Length; i += channels){
			//Perform stereo panning
			data[i] = Mathf.Clamp(1.0f - pan, 0f, 1.0f) * data[i];
			data[i + 1] = Mathf.Clamp(1.0f + pan, 0f, 1.0f) * data[i + 1];
		
			// Handle the rest of the channels if more than stereo
			for(int c = 2; c < channels; c++){
				data[i + c] = 0;
			}
		}
	}
}
