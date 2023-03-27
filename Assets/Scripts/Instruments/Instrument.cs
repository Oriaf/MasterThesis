using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Instrument
{
	protected double frequency;
	protected float gain;
	protected double sampleRate;
	
	protected bool updateFlag; // Indicates if the main thread needs to call the update function

	public Instrument(double f, float g, double sr){
		frequency = f;
		gain = g;
		sampleRate = sr;
		
		updateFlag = false;
	}
	
	public double getFrequency() { return frequency; }
	public float getGain() { return gain; }
	public double getSampleRate() { return sampleRate; }
	
	public void setFrequency(double f) { frequency = f; }
	public void setGain(float g) { gain = g; }
	public void setSampleRate(double sr) { sampleRate = sr; }
	
	public bool hasUpdate() { return updateFlag; }
	
	public virtual void performUpdateWork(){
		//Intentionally empty; Should be overriden if used by instrument
	}

    public abstract void sampleInstrument(float[] data, int channels, Vector3 pos);
}
