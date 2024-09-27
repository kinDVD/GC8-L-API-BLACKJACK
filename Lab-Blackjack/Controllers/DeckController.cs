using Lab_Blackjack.Models;
using Lab_Blackjack.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;

namespace Lab_Blackjack.Controllers
{
    [Route("blackjack")]
    [ApiController]
    public class DeckController : ControllerBase
    {
        //static GameStatus status = new GameStatus();
        static List<GameStatus> allGames = new List<GameStatus>();
        static int nextId = 1;
        private readonly DeckService _service;
        public DeckController(DeckService service)
        {
            _service = service;
        }
        [HttpGet("allHands")]
        public ActionResult GetAll()
        {
            return Ok(allGames);
        }

        [HttpGet()]
        public async Task<IActionResult> GetGame(int gameId)
        {
            GameStatus status = allGames.FirstOrDefault(g => g.id == gameId);
            if (status.DeckId == null)
            {
                return NotFound("No game started.");
            }
            return Ok(status);
        }

        [HttpPost()]
        public async Task<IActionResult> NewGame(int? gameId = null)
        {
            GameStatus status;
            if (gameId != null)
            {
                status = allGames.FirstOrDefault(g => g.id == gameId);
                if (status.DeckId != null)
                {
                    return Conflict("Game in progress.");
                }
            }
            DeckModel newDeck = await _service.NewDeck();
            status = new GameStatus();
            allGames.Add(status);
            status.id = nextId++;
            status.DeckId = newDeck.deck_id;
            status.GameOver = false;
            status.Outcome = "";
            DeckModel resultCards = await _service.DrawCards(3, status.DeckId);
            status.DealerCards = new List<Card>() { resultCards.cards[0] };
            status.PlayerCards = new List<Card>() { resultCards.cards[1], resultCards.cards[2] }; 
            status.DealerScore = GetCardScore(status.DealerCards);
            status.PlayerScore = GetCardScore(status.PlayerCards);
            CheckStatus(status);
            return Ok(status);
        }

        [HttpPost("play")]
        public async Task<IActionResult> GameAction(string action, int? gameId = null)
        {
                GameStatus status = allGames.FirstOrDefault(g => g.id == gameId);
                action = action.ToLower().Trim();
                if (ValidatePlayerAction(action, status) != null)
                {
                    return ValidatePlayerAction(action, status);   
                }
                if (action == "hit")
                {
                    DeckModel cardsResult = await _service.DrawCards(1, status.DeckId);
                    status.PlayerCards.Add(cardsResult.cards[0]);
                    status.PlayerScore = GetCardScore(status.PlayerCards);
                    CheckStatus(status);
                }
                if (action == "stand")
                {
                    if (status.DealerScore < 17)
                    {
                        DeckModel dealerResult = await _service.DrawCards(1, status.DeckId);
                        status.DealerCards.Add(dealerResult.cards[0]);
                        status.DealerScore = GetCardScore(status.DealerCards);
                        CheckStatus(status);
                    }
                }
                return Ok(status);

        }
        private IActionResult ValidatePlayerAction(string action, GameStatus status)
        {
            if (status.DeckId == null)
            {
                return NotFound("No game started.");
            }
            else if (status.GameOver == true)
            {
                return Conflict("Game is over.");
            }
            else if (action != "hit" && action != "stand")
            {
                return BadRequest("Invalid option. Are you sure you should be betting right now?");
            }
            else
            {
                return null;
            }
        }
        private int GetCardScore(List<Card> hand)
        {
            int scoreCheck = 0;
            foreach (Card c in hand)
            {
                if (c.value == "KING" | c.value == "QUEEN" | c.value == "JACK")
                { 
                    scoreCheck += 10; 
                }
                else if (c.value == "JOKER")
                {
                    scoreCheck += 0;
                }
                else if(int.TryParse(c.value, out int numberCard))
                {
                    scoreCheck += numberCard;
                }
            }
            foreach (Card c in hand)
            {
                if(scoreCheck + 11 <= 21 && c.value == "ACE")
                {
                    scoreCheck += 11;
                }
                else if(scoreCheck + 11 > 21 && c.value == "ACE")
                {
                    scoreCheck += 1;
                }
            }
            return scoreCheck;
        }

        private void CheckStatus(GameStatus status)
        {
            if (status.DealerScore == 21)
            {
                status.GameOver = true;
                status.Outcome = "Blackjack for the dealer! You lose.";
            }
            else if (status.DealerScore > 21)
            {
                status.GameOver = true;
                status.Outcome = "Dealer busts! You won!!";
            }
            else if (status.DealerScore >= 17 && status.DealerScore > status.PlayerScore)
            {
                status.GameOver = true;
                status.Outcome = "The House always wins.";
            }
            else if (status.DealerScore >= 17 && status.DealerScore == status.PlayerScore)
            {
                status.GameOver = true;
                status.Outcome = "Push.";
            }
            else if (status.DealerScore >= 17 && status.DealerScore < status.PlayerScore)
            {
                status.GameOver = true;
                status.Outcome = "You beat the dealer!!!";
            }
            else if (status.PlayerScore > 21)
            {
                status.GameOver = true;
                status.Outcome = "Bust! You lose.";
            }
            else if (status.PlayerScore == 21)
            {
                status.GameOver = true;
                status.Outcome = "BLACKJACK!!!";
            }
            else
            {
                status.GameOver = false;
                status.Outcome = "Would you like to Stand or Hit?";
            }
        }

    }
}
