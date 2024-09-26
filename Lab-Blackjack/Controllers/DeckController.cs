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
        static GameStatus status = new GameStatus();
        private readonly DeckService _service;
        public DeckController(DeckService service)
        {
            _service = service;
        }

        [HttpGet()]
        public async Task<IActionResult> GetGame()
        {
            if(status.DeckId == null)
            {
                return NotFound("No game started.");
            }
            return Ok(status);
        }

        [HttpPost()]
        public async Task<IActionResult> NewGame()
        {
            if (status.DeckId != null)
            {
                return Conflict("Game in progress.");
            }
            DeckModel newDeck = await _service.NewDeck();
            status = new GameStatus();
            status.DeckId = newDeck.deck_id;
            DeckModel resultCards = await _service.DrawCards(3, status.DeckId);
            status.DealerCards = new List<Card>() { resultCards.cards[0] };
            status.PlayerCards = new List<Card>() { resultCards.cards[1], resultCards.cards[2] }; ;
            status.DealerScore = GetCardScore(resultCards.cards[0]);
            status.PlayerScore = GetCardScore(resultCards.cards[1]) + GetCardScore(resultCards.cards[2]);
            status.GameOver = false;
            status.Outcome = "";

            if(status.PlayerScore > 21)
            {
                status.GameOver = true;
                status.Outcome = "Bust";
            }
            if (status.PlayerScore == 21)
            {
                status.GameOver = true;
                status.Outcome = "BLACKJACK!!!";
            }


            return Created("",status);
        }

        [HttpPost("play")]
        public async Task<IActionResult> GameAction(string action)
        {
            //action = action.ToLower().Trim();
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
                if (action == "hit")
                {
                    DeckModel cardsResult = await _service.DrawCards(1, status.DeckId);
                    status.PlayerCards.Add(cardsResult.cards[0]);
                    status.PlayerScore += GetCardScore(cardsResult.cards[0]);
                    if (status.PlayerScore > 21 && cardsResult.cards[0].value == "Ace")
                    {
                        status.PlayerScore -= 10;
                    }
                    if (status.PlayerScore > 21)
                    {
                        status.GameOver = true;
                        status.Outcome = "Bust! You lose.";
                    }
                    if (status.PlayerScore == 21)
                    {
                        status.GameOver = true;
                        status.Outcome = "BLACKJACK!!!";
                    }
                }
                if (action == "stand")
                {
                    if (status.DealerScore < 17)
                    {
                        DeckModel dealerResult = await _service.DrawCards(1, status.DeckId);
                        status.DealerScore += GetCardScore(dealerResult.cards[0]);
                        status.DealerCards.Add(dealerResult.cards[0]);
                        if (status.DealerScore > 21 && dealerResult.cards[0].value == "Ace")
                        {
                            status.DealerScore -= 10;
                        }
                        if (status.DealerScore == 21)
                        {
                            status.GameOver = true;
                            status.Outcome = "Blackjack for the dealer! You lose.";
                        }
                        if (status.DealerScore > 21)
                        {
                            status.GameOver = true;
                            status.Outcome = "Dealer busts! You won!!";
                        }

                        else if (status.DealerScore == status.PlayerScore)
                        {
                            status.GameOver = true;
                            status.Outcome = "Push.";
                        }
                    }
                }
                return Ok(status);
            }

        }

        private int GetCardScore(Card c)
        {
            if (c.value == "ACE")
            { return 11; }
            else if(c.value == "KING" | c.value == "QUEEN" | c.value == "JACK")
            { return 10; }
            else if(c.value == "JOKER")
            {
              return 0;
            }
            else
            {
                return int.Parse(c.value);
            }
        }


    }
}
