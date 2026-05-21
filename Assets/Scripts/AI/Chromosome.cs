using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chromosome
{
    public float[] genes = new float[8];
    public float fitness = 0;
    private static System.Random _rng = new System.Random();
    public Chromosome()
    {
        for (int i = 0; i < 8; i++) genes[i] = (float)_rng.NextDouble();
    }

    // Sao chép gen để đột biến
    public Chromosome Clone()
    {
        Chromosome copy = new Chromosome();
        for (int i = 0; i < 8; i++) copy.genes[i] = this.genes[i];
        copy.fitness = this.fitness;
        return copy;
    }
}

// Aggression, Survival, Economy, Defense, Growth, Persistence, BoardPresence, Risk.