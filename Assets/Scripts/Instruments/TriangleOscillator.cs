using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleOscillator : Oscillator
{
	public TriangleOscillator(double f, float g, double sr) : base(f, g, sr){ }

	/*
	 * Fills a data buffer with samples from the Oscillator.
	 * The data is filled in based on the current settings of the Oscillator
	 */
	override public void sampleTone(float[] data, int channels){
		double period = 1 / frequency;
	
		double increment = 1 / sampleRate; // 1 / (samples per period)
	
		for(int i = 0; i < data.Length; i += channels){
			pos += increment;
			if(pos > period) pos -= period;
			//pos = (double) Mathf.Repeat((float) pos, (float) period); // Modulo pos so it is always within the period
			//double t = pos / period;
			
			// Sample the tone of the instrument and write it to each channel
			//float tone = 2 * Mathf.Abs(2 * ((float) t - Mathf.Floor((float) t + 0.5f))) - 1;
			
			float tone = (float) ((2 / System.Math.PI) * System.Math.Asin(System.Math.Sin(2 * System.Math.PI / period * pos)));
			
			for(int j = 0; j < channels; j++){
				data[i + j] = gain * tone;
			}
			
			//Debug.Log(data[i]);
		}
	}
}
