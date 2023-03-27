using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpChirpOscillator : Oscillator
{
	private double duration; // Duration in seconds until the frequency is one octave higher

	public ExpChirpOscillator(double f_start, float g, double sr, double dur) : base(f_start, g, sr){ duration = dur; }
	
	public double getDuration() { return duration; }
	public void setDuration(double d) { duration = d; }

	/*
	 * Fills a data buffer with samples from the Oscillator.
	 * The data is filled in based on the current settings of the Oscillator
	 */
	override public void sampleTone(float[] data, int channels){
		double increment = 1.0 / sampleRate;
	
		double constantPart = (2.0 * System.Math.PI * frequency * duration) / System.Math.Log(2);
	
		for(int i = 0; i < data.Length; i += channels){
			pos += increment;
			if(pos > duration) pos -= duration;
			
			// Sample the tone of the instrument and write it to each channel
			double arg = constantPart * System.Math.Pow(2, pos / duration);
			if(duration == System.Double.PositiveInfinity) arg = frequency * 2.0 * Mathf.PI * pos;
			
			float tone = Mathf.Sin((float) arg);
			
			for(int j = 0; j < channels; j++){
				data[i + j] = gain * tone;
			}
		}
	}
}
