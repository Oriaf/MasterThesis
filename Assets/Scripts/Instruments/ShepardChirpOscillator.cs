using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShepardChirpOscillator : Oscillator
{
	private int N; // Number of octaves to cover
	double phi;
	private double x;
	
	private const double MU_0 = 1; // Don't put this above 2!!!!!!
	private const double SIGMA_0 = 30; // Don't put this below 1!!!!!
	private const double V1 = 1.0;
	private const double V2 = 1.0;
	private const double OMEGA_MOD = 50;

	public ShepardChirpOscillator(double f_start, float g, double sr, int oct, double p) : base(f_start, g, sr){
		phi = p;
		N = oct;
	}
	
	public double getX() { return x; }
	public void setX(double X) { x = X; }

	private double PHI(double t){
		//Debug.Log(t + ", " + x + ", " + (x * t + phi) % 1.0);
	
		//return (x * t + phi) % 1.0;
		double a = (x * t + phi);
		double b = 1.0;
		return System.Math.IEEERemainder(a, b); // Mathematical Modulo
	}
	
	private double mu(double z){
		return (z < 0) ? MU_0 : MU_0 - z;
	}
	
	private double sigma(double y){
		return (y < 0) ? SIGMA_0 : SIGMA_0 - V2 * y;
	}
	
	private double A(double t){
		double power = System.Math.Pow(PHI(t) - mu(0), 2) / (2 * System.Math.Pow(sigma(0), 2));
		double sqrt = System.Math.Sqrt(2 * System.Math.PI * sigma(0));
		
		return System.Math.Pow(System.Math.E, power) / sqrt;
	}
	
	/*
	 * Fills a data buffer with samples from the Oscillator.
	 * The data is filled in based on the current settings of the Oscillator
	 */
	override public void sampleTone(float[] data, int channels){
		double increment = 1.0 / sampleRate;
		
		double constantPart = 2.0 * System.Math.PI * frequency;
	
		for(int i = 0; i < data.Length; i += channels){
			pos += increment;
			//if(pos > 1.0 / x) pos = System.Math.IEEERemainder(pos, 1.0 / x);
			
			// Sample the tone of the instrument and write it to each channel
			double arg = constantPart * System.Math.Pow(2, N * PHI(pos)) * pos; //FM Synthesis
			
			float tone = Mathf.Cos((float) arg);
			
			for(int j = 0; j < channels; j++){
				data[i + j] = gain * (float) A(pos) * tone;
			}
		}
	}
}
