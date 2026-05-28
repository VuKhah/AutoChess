[System.Serializable]
public class AILibrary {
    public Chromosome hardBot;      // Rank 1 — best overall từ population cuối
    public Chromosome babylonBot;   // Tribe specialist: sBabylon dominant
    public Chromosome nileBot;      // Tribe specialist: sNiles dominant
    public Chromosome aggressorBot; // Playstyle: ATK rush, reroll heavy, ignore growth
    public Chromosome resilientBot; // Playstyle: HP/Taunt/Reborn, phản đòn, bền bỉ
}
