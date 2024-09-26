using Lab_Blackjack.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lab_Blackjack.Services
{
    public class DeckService
    {
        private HttpClient _httpClient;
        public DeckService(HttpClient httpClient)
        { 
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://www.deckofcardsapi.com/api/deck/");
        }

        public async Task<DeckModel> NewDeck()
        {
            DeckModel result = await _httpClient.GetFromJsonAsync<DeckModel>("new/shuffle");
            return result;
        }
        public async Task<DeckModel> DrawCards(int count, string DeckId)
        {
            DeckModel result = await _httpClient.GetFromJsonAsync<DeckModel>($"{DeckId}/draw/?count={count}");
            return result;
        }

    }
}
