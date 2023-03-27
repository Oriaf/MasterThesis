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
	
	// Update cache
	float x;

	public SimpleTone(double f0, float g, double sr, float s, AudioSource aus) : base(f0, g, sr){
		scale = s;
		audio = aus;
		
		oscillator = new TriangleOscillator(f0, g, sr);
	}
	
	override public void performUpdateWork(){
		audio.panStereo = Mathf.Clamp(x, -1.0f, 1.0f);
		updateFlag = false;
	}
	
	override public void sampleInstrument(float[] data, int channels, Vector3 pos){
		// Map y to frequency logarithmically
		float y = scale * (pos.y + 1.0f); //Go between 0 and 2 when scale is 1.0f
		oscillator.setFrequency(frequency * Mathf.Pow(2.0f, y));
		
		oscillator.sampleTone(data, channels);
		
		// Set update, since panning isn't allowed on non-main thread
		x = pos.x;
		updateFlag = true;
	}
}
