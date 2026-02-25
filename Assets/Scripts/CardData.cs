using System.Collections.Generic;

[System.Serializable]
public class CardData
{
    public int Moves;
    public int Matches;
    public List<int> MatchedIndexes = new List<int>();
    public List<int> CardOrder = new List<int>();
}