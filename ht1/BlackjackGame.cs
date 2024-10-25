using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace BlackjackGame;

// Luokan ja ohjelman tekijätiedot ja versio
/// @author Tomas Vallas
/// @version 07.10.2024
/// <summary>
/// Blackjack peli
/// </summary>

public class BlackjackGame : PhysicsGame // Perii Jypelin PhysicsGame-luokan, joka mahdollistaa pelifysiikat ja tapahtumankäsittelyn
{
    private Deck deck;                    // Korttipakka
    private List<Card> playerHand;        // Pelaajan kortit
    private List<Card> dealerHand;        // Jakajan kortit
    private int playerScore;              // Pelaajan pisteet
    private int dealerScore;              // Jakajan pisteet
    private int playerMoney = 1000;       // Pelaajan aloitusrahat
    private int bet = 100;                // Veto per kierros

    private Label playerLabel;            // Pelaajan pistetiedon näyttävä label
    private Label dealerLabel;            // Jakajan pistetiedon näyttävä label
    private Label moneyLabel;             // Pelaajan rahasumman näyttävä label
    private Label messageLabel;           // Pelin viesteille tarkoitettu label

    public override void Begin()          // Pelin aloitusmetodi
    {
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");   // Mahdollistaa pelin sulkemisen puhelimen back-painikkeella
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli"); // Escape-näppäin lopettaa pelin
        ClearAll();                                          // Tyhjentää ruudun sisällön
        SetWindowSize(800, 600);                             // Määrittää peliruudun koon
        InitializeGame();                                    // Kutsuu pelin alustukseen tarkoitettua metodia
    }

    private void InitializeGame()         // Pelin alustustoiminnot
    {
        deck = new Deck();                // Luodaan uusi korttipakka
        playerHand = new List<Card>();    // Luodaan lista pelaajan kortteja varten
        dealerHand = new List<Card>();    // Luodaan lista jakajan kortteja varten
        playerScore = 0;                  // Asetetaan pelaajan pisteet nollaan
        dealerScore = 0;                  // Asetetaan jakajan pisteet nollaan

        // Alustetaan tekstilaatikot näytölle ja sijoitetaan ne pelialueelle
        playerLabel = new Label("Player: 0");
        playerLabel.Position = new Vector(-200, 200);
        Add(playerLabel);

        dealerLabel = new Label("Dealer: 0");
        dealerLabel.Position = new Vector(-200, 150);
        Add(dealerLabel);

        moneyLabel = new Label("Money: " + playerMoney);
        moneyLabel.Position = new Vector(-200, 100);
        Add(moneyLabel);

        messageLabel = new Label("Place your bet and press Enter.");
        messageLabel.Position = new Vector(0, 0);
        Add(messageLabel);

        // Määritetään näppäinkomennot
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, NewRound, "Start a new round"); // Enter aloittaa uuden kierroksen
        Keyboard.Listen(Key.Space, ButtonState.Pressed, PlayerHit, "Take a card");      // Space antaa pelaajalle uuden kortin
        Keyboard.Listen(Key.S, ButtonState.Pressed, PlayerStand, "Stand");              // S-painike saa pelaajan pysymään
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "Exit the game");        // Escape lopettaa pelin

        NewRound(); // Aloittaa ensimmäisen kierroksen
    }

    private void NewRound()               // Aloittaa uuden kierroksen
    {
        deck.Shuffle();                   // Sekoittaa pakan
        playerHand.Clear();               // Tyhjentää pelaajan käden
        dealerHand.Clear();               // Tyhjentää jakajan käden

        // Jakaa kaksi korttia pelaajalle ja jakajalle
        playerHand.Add(deck.DrawCard());
        playerHand.Add(deck.DrawCard());

        dealerHand.Add(deck.DrawCard());
        dealerHand.Add(deck.DrawCard());

        UpdateScore();                    // Päivittää pisteet
        UpdateUI();                       // Päivittää näytön
        messageLabel.Text = "Press Space to Hit, or S to Stand."; // Näyttää ohjeviestin
    }

    private void PlayerHit()              // Pelaajan toiminta "Hit"-valinnassa
    {
        playerHand.Add(deck.DrawCard());  // Jakaa pelaajalle uuden kortin
        UpdateScore();                    // Päivittää pelaajan pisteet

        if (playerScore > 21)             // Tarkistaa, meneekö pelaajan pisteet yli 21
        {
            messageLabel.Text = "Player busts! You lose.";
            playerMoney -= bet;           // Vähentää pelaajan rahasta panoksen verran
            UpdateUI();
            Timer.SingleShot(2.0, NewRound); // Aloittaa uuden kierroksen 2 sekunnin kuluttua
        }
        else
        {
            UpdateUI();                   // Päivittää näytön, jos pelaaja ei bustaa
        }
    }

    private void PlayerStand()            // Pelaajan toiminta "Stand"-valinnassa
    {
        while (dealerScore < 17)          // Jakaja ottaa kortteja, kunnes pisteet ovat vähintään 17
        {
            dealerHand.Add(deck.DrawCard());
            UpdateScore();
        }

        // Tarkistaa voittotilanteen: pelaaja voittaa, jos jakaja menee yli 21:n tai pelaajan pisteet ovat korkeammat
        if (dealerScore > 21 || playerScore > dealerScore)
        {
            messageLabel.Text = "You win!";
            playerMoney += bet;           // Lisää panoksen verran pelaajan rahaan
        }
        else if (dealerScore > playerScore)
        {
            messageLabel.Text = "Dealer wins!";
            playerMoney -= bet;           // Vähentää panoksen verran pelaajan rahasta
        }
        else
        {
            messageLabel.Text = "It's a draw."; // Jos tasapeli, ei rahamuutosta
        }

        UpdateUI();
        Timer.SingleShot(2.0, NewRound);  // Aloittaa uuden kierroksen 2 sekunnin kuluttua
    }

    private void UpdateScore()            // Päivittää pelaajan ja jakajan pisteet
    {
        playerScore = CalculateScore(playerHand);
        dealerScore = CalculateScore(dealerHand);
    }

    private int CalculateScore(List<Card> hand) // Laskee annetun korttilistan pisteet
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in hand)       // Käy kortit läpi ja lisää niiden arvot pisteisiin
        {
            if (card.Value >= 10)         // Kortit arvoltaan 10 tai yli lasketaan kympin arvoisiksi
                total += 10;
            else if (card.Value == 1)     // Ässä voi olla 1 tai 11
            {
                aceCount++;
                total += 11;
            }
            else
                total += card.Value;
        }

        while (total > 21 && aceCount > 0) // Jos pisteet ylittävät 21 ja kädessä on ässä, vähennetään 10 pistettä
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }

    private void UpdateUI()               // Päivittää näytölle pistetiedot ja pelaajan rahat
    {
        playerLabel.Text = "Player: " + playerScore;
        dealerLabel.Text = "Dealer: " + dealerScore;
        moneyLabel.Text = "Money: " + playerMoney;
    }
}

public class Deck                         // Korttipakkaluokka
{
    private List<Card> cards;

    public Deck()                         // Pakan konstruktori
    {
        cards = new List<Card>();
        for (int i = 1; i <= 13; i++)     // Jokainen korttinumero lisätään neljä kertaa (yksi kutakin maata varten)
        {
            for (int j = 0; j < 4; j++)
            {
                cards.Add(new Card(i));
            }
        }
    }

    public void Shuffle()                 // Korttien sekoitus
    {
        Random rng = new Random();
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
    }

    public Card DrawCard()                // Kortin jakaminen pakasta
    {
        if (cards.Count == 0) return null;
        Card card = cards[0];  // Luodaan 'card'-muuttuja, johon tallennetaan pakan ensimmäinen kortti.
        cards.RemoveAt(0);     // Poistetaan pakasta ensimmäinen kortti (index 0), koska se on juuri nostettu.
        return card;   
    }
}
    public class Card   // Määrittelee kortti-olion, jolla on yksi ominaisuus: kortin arvo.
{
    public int Value { get; private set; }  // Kortin arvo, jota voidaan vain lukea ulkoapäin.
    
    public Card(int value)  // Konstruktori, joka asettaa kortin arvon sen luontihetkellä.
    {
        Value = value;  // Asettaa kortin arvoksi 'value'-parametrin arvon.
    }
}