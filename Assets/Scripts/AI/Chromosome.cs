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

    // BUG-AI-07 FIX: Constructor nội bộ bỏ qua random init — dùng khi genes sẽ bị ghi đè ngay.
    private Chromosome(bool _) { }

    // Sao chép gen để đột biến
    public Chromosome Clone()
    {
        var copy = new Chromosome(false);
        copy.genes = new float[8];
        System.Array.Copy(this.genes, copy.genes, 8);
        copy.fitness = this.fitness;
        return copy;
    }
}

// Aggression, Survival, Economy, Defense, Growth, Persistence, BoardPresence, Risk.