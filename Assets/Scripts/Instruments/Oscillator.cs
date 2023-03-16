using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Oscillator
{
	protected double frequency;
	protected float gain;
	protected double sampleRate;
	
	protected double pos;
	
	public Oscillator(){
		frequency = 440;
		gain = 0.1f;
		sampleRate = 48000;
		
		pos = 0;
	}
	
	public Oscillator(double f, float g, double sr){
		frequency = f;
		gain = g;
		sampleRate = sr;
		
		pos = 0;
	}
	
	public double getFrequency() { return frequency; }
	public double getGain() { return gain; }
	public double getSampleRate() { return sampleRate; }
	
	public void setFrequency(double f) { frequency = f; }
	public void setGain(float g) { gain = g; }
	public void setSampleRate(double sr) { sampleRate = sr; }
	
	/*
	 * Fills a data buffer with samples from the Oscillator.
	 * The data is filled in based on the current settings of the Oscillator
	 */
	public abstract void sampleTone(float[] data, int channels);
}
