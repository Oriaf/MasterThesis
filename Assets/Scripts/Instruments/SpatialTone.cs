using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialTone : Instrument
{

	//private double[] notes = {440.0, 554.37, 659.25}; //A4, C#6, E5 (A Major Chord)
	
	private double[] timbreMul = {1, 2, 4, 8};
	private float[] timbreAmp = {1.0f, 0.5f, 0.25f, 0.125f};

	private SineOscillator[] notes;
	
	public SpatialTone(float g, double sr) : base(440.0, g, sr) {
		notes = new SineOscillator[3];
		notes[0] = new SineOscillator(440.0, g, sr);
		notes[1] = new SineOscillator(554.37, g, sr);
		notes[2] = new SineOscillator(659.25, g, sr);
	}
	
	override public void sampleInstrument(float[] data, int channels, Vector3 pos){
		float[] buffer = new float[data.Length];

		notes[0].sampleTone(data, channels);
		
		for(int i = 1; i < 3; i++){
			notes[i].sampleTone(buffer, channels);

			for(int j = 0; j < data.Length; j++){
				data[j] += buffer[j];
			}
		}
		
		// Normalize the sound level
		for(int i = 0; i < data.Length; i++){
			data[i] = data[i] / (3.0f);
		}
	}
}
