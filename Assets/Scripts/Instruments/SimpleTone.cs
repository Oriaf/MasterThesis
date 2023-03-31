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
	private SineOscillator refOscillator;

	public SimpleTone(double f0, float g, double sr, float s, AudioSource aus) : base(f0, g, sr){
		scale = s;
		audio = aus;
		
		oscillator = new TriangleOscillator(f0, g, sr);
		refOscillator = new SineOscillator(f0 * 2, g / 2.0f, sr);
	}
	
	override public void sampleInstrument(float[] data, int channels, Vector3 pos){
		if(channels < 2) Debug.LogError("This instrument requires stereo sound for panning!");
	
		float y = scale * (Mathf.Clamp(pos.y, -1f, 1f) + 1f); //Go between 0 and 2 when scale is 1.0f
		float x = (Mathf.Clamp(pos.x, -1f, 1f) + 1f) / 2f; //Go between 0 and 1
	
		// Map y to frequency logarithmically
		oscillator.setFrequency(frequency * Mathf.Pow(2.0f, y));
		
		float pan = Mathf.Clamp(pos.x, -1.0f, 1.0f);
		
		oscillator.sampleTone(data, channels);
		float[] buf = new float[data.Length];
		refOscillator.sampleTone(buf, channels);
		for(int i = 0; i < data.Length; i += channels){
			data[i] += buf[i];
			data[i + 1] += buf[i + 1];
		
			//Perform stereo panning
			/*data[i] = (1.0f - x) * data[i];
			data[i + 1] = x * data[i + 1]; // Linear panning*/
			/*data[i] = (Mathf.Pow(2.0f, (1.0f - x)) - 1.0f) * data[i];
			data[i + 1] = (Mathf.Pow(2.0f, x) - 1.0f) * data[i + 1]; // Linear panning with logarithmic mapping*/
			/*data[i] = (Mathf.Pow(2.0f, Mathf.Clamp(1.0f - pan, 0f, 1.0f)) - 1.0f) * data[i];
			data[i + 1] = (Mathf.Pow(2.0f, Mathf.Clamp(1.0f + pan, 0f, 1.0f)) - 1.0f) * data[i + 1]; // Semi-Linear panning with logarithmic mapping */
			/*data[i] = Mathf.Sqrt(1.0f - x) * data[i];
			data[i + 1] = Mathf.Sqrt(x) * data[i + 1]; // Square-law panning*/
			data[i] = Mathf.Sin((1.0f - x) * (Mathf.PI / 2.0f)) * data[i];
			data[i + 1] = Mathf.Sin(x * (Mathf.PI / 2.0f)) * data[i + 1]; // Sin-law panning
			
			
		
			// Handle the rest of the channels if more than stereo
			for(int c = 2; c < channels; c++){
				data[i + c] = 0;
			}
		}
	}
}
