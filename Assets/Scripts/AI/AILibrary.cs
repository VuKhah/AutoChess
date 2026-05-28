[System.Serializable]
public class AILibrary {
    public Chromosome easyBot;    // Rank ~67% của population cuối
    public Chromosome mediumBot;  // Rank ~33% của population cuối
    public Chromosome hardBot;    // Rank 1 (tốt nhất) của population cuối
    public Chromosome babylonBot; // Best chromosome có sBabylon là tribe gene cao nhất
    public Chromosome nileBot;    // Best chromosome có sNiles là tribe gene cao nhất
}
