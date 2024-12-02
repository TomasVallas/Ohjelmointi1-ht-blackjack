using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace BlackjackGame
{
    /// <summary>
    /// Blackjack-peli, jossa pelaaja ja jakaja pelaavat toisiaan vastaan.
    /// Pelaaja voi ottaa kortteja (Hit), pysyä (Stand) ja asettaa panoksia.
    /// </summary>
    public class BlackjackGame : PhysicsGame
    {
        private Deck deck;  // Korttipakka
        private List<Card> playerHand;  // Pelaajan kortit
        private List<Card> dealerHand;  // Jakajan kortit
        private int playerScore;  // Pelaajan pisteet
        private int dealerScore;  // Jakajan pisteet
        private int playerMoney = 1000;  // Pelaajan aloitusrahat
        private int bet = 100;  // Kierroksen panos

        private Label playerLabel;  // Pelaajan pistetiedon näyttävä tekstikenttä
        private Label dealerLabel;  // Jakajan pistetiedon näyttävä tekstikenttä
        private Label moneyLabel;  // Pelaajan rahasumman näyttävä tekstikenttä
        private Label messageLabel;  // Pelin viesteille tarkoitettu tekstikenttä

        /// <summary>
        /// Aloittaa pelin ja määrittää näppäinkomennot sekä käynnistää pelin alustusmetodin.
        /// </summary>
        public override void Begin()
        {
            PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
            ClearAll();
            SetWindowSize(800, 600);
            InitializeGame();
        }

        /// <summary>
        /// Alustaa pelin, luo korttipakan ja asettaa tekstikentät paikalleen.
        /// </summary>
        private void InitializeGame()
        {
            deck = new Deck();
            playerHand = new List<Card>();
            dealerHand = new List<Card>();
            playerScore = 0;
            dealerScore = 0;

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

            Keyboard.Listen(Key.Enter, ButtonState.Pressed, NewRound, "Aloita uusi kierros");
            Keyboard.Listen(Key.Space, ButtonState.Pressed, PlayerHit, "Ota kortti");
            Keyboard.Listen(Key.S, ButtonState.Pressed, PlayerStand, "Pysy");
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "Poistu pelistä");

            NewRound();
        }

        /// <summary>
        /// Aloittaa uuden kierroksen ja jakaa kortit pelaajalle ja jakajalle.
        /// </summary>
        private void NewRound()
        {
            deck.Shuffle();
            playerHand.Clear();
            dealerHand.Clear();

            playerHand.Add(deck.DrawCard());
            playerHand.Add(deck.DrawCard());

            dealerHand.Add(deck.DrawCard());
            dealerHand.Add(deck.DrawCard());

            UpdateScore();
            UpdateUI();
            messageLabel.Text = "Press Space to Hit, or S to Stand.";
        }

        /// <summary>
        /// Pelaajan "Hit"-toiminto, jossa pelaaja ottaa uuden kortin.
        /// </summary>
        private void PlayerHit()
        {
            playerHand.Add(deck.DrawCard());
            UpdateScore();

            if (playerScore > 21)
            {
                messageLabel.Text = "Player busts! You lose.";
                playerMoney -= bet;
                UpdateUI();
                Timer.SingleShot(2.0, NewRound);
            }
            else
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// Pelaajan "Stand"-toiminto, jossa pelaaja ei ota lisää kortteja.
        /// Jakaja pelaa, kunnes sen pisteet ovat vähintään 17.
        /// </summary>
        private void PlayerStand()
        {
            while (dealerScore < 17)
            {
                dealerHand.Add(deck.DrawCard());
                UpdateScore();
            }

            if (dealerScore > 21 || playerScore > dealerScore)
            {
                messageLabel.Text = "You win!";
                playerMoney += bet;
            }
            else if (dealerScore > playerScore)
            {
                messageLabel.Text = "Dealer wins!";
                playerMoney -= bet;
            }
            else
            {
                messageLabel.Text = "It's a draw.";
            }

            UpdateUI();
            Timer.SingleShot(2.0, NewRound);
        }

        /// <summary>
        /// Päivittää pelaajan ja jakajan pistemäärät.
        /// </summary>
        private void UpdateScore()
        {
            playerScore = CalculateScore(playerHand);
            dealerScore = CalculateScore(dealerHand);
        }

        /// <summary>
        /// Laskee kädessä olevien korttien pisteet.
        /// </summary>
        /// <param name="hand">Lista korteista.</param>
        /// <returns>Käden kokonaispistemäärä.</returns>
        private int CalculateScore(List<Card> hand)
        {
            int total = 0;
            int aceCount = 0;

            foreach (Card card in hand)
            {
                if (card.Value >= 10)
                    total += 10;
                else if (card.Value == 1)
                {
                    aceCount++;
                    total += 11;
                }
                else
                    total += card.Value;
            }

            while (total > 21 && aceCount > 0)
            {
                total -= 10;
                aceCount--;
            }

            return total;
        }

        /// <summary>
        /// Päivittää pelin tekstikentät näyttämään pistetiedot ja rahasumman.
        /// </summary>
        private void UpdateUI()
        {
            playerLabel.Text = "Player: " + playerScore;
            dealerLabel.Text = "Dealer: " + dealerScore;
            moneyLabel.Text = "Money: " + playerMoney;
        }
    }

    /// <summary>
    /// Luokka, joka edustaa korttipakkaa.
    /// </summary>
    public class Deck
    {
        private List<Card> cards;

        /// <summary>
        /// Luo uuden korttipakan.
        /// </summary>
        public Deck()
        {
            cards = new List<Card>();
            for (int i = 1; i <= 13; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    cards.Add(new Card(i));
                }
            }
        }

        /// <summary>
        /// Sekoittaa korttipakan.
        /// </summary>
        public void Shuffle()
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

        /// <summary>
        /// Nostaa kortin pakasta.
        /// </summary>
        /// <returns>Ensimmäinen kortti pakasta.</returns>
        public Card DrawCard()
        {
            if (cards.Count == 0) return null;
            Card card = cards[0];
            cards.RemoveAt(0);
            return card;
        }
    }

    /// <summary>
    /// Luokka, joka edustaa yksittäistä korttia.
    /// </summary>
    public class Card
    {
        public int Value { get; private set; }

        /// <summary>
        /// Luo uuden kortin ja asettaa sen arvon.
        /// </summary>
        /// <param name="value">Kortin arvo.</param>
        public Card(int value)
        {
            Value = value;
        }
    }
}