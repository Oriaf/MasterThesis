using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineOscillator : Oscillator
{
	public SineOscillator(double f, float g, double sr) : base(f, g, sr){ }

	/*
	 * Fills a data buffer with samples from the Oscillator.
	 * The data is filled in based on the current settings of the Oscillator
	 */
	override public void sampleTone(float[] data, int channels){	
		double increment = frequency * 2.0 * Mathf.PI / sampleRate; // Period * seconds per sample
	
		for(int i = 0; i < data.Length; i += channels){
			pos += increment;
			if(pos > Mathf.PI * 2) pos -= Mathf.PI * 2;
			
			// Sample the tone of the instrument and write it to each channel
			float tone = Mathf.Sin((float) pos);;
			
			for(int j = 0; j < channels; j++){
				data[i + j] = gain * tone;
			}
		}
	}
}
