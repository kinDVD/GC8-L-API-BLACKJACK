namespace Lab_Blackjack.Models
{
    public class GameStatus
    {
        public string DeckId { get; set; }
        public List<Card> DealerCards { get; set; }
        public List<Card> PlayerCards { get; set; }
        public int DealerScore { get; set; }
        public int PlayerScore { get; set; }
        public bool GameOver { get; set; }
        public string? Outcome { get; set; }

    }
}
