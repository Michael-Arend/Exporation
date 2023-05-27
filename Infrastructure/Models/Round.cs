﻿using Poker.Infrastructure.Helper.Extensions;

namespace Poker.Infrastructure.Models;

public class Round
{
    public Round(decimal smallBlind, decimal bigBlind, decimal limit)
    {
        Deck = CreateDeck();
        Players = CreatePlayers();
        BigBlind = bigBlind;
        SmallBlind = smallBlind;
        Pot = 0;
        DealCards();
        Limit = limit;
        BettingPattern = "";
    }

    
    public List<Card> Deck { get; set; }
    public List<Player?> Players { get; set; }
    public List<Player?> PlayersInHand => Players.Where(x => x.PlayerInHand).ToList();

    public Player? PlayerToAct { get; private set; }
    public decimal SmallBlind { get; set; }
    public decimal BigBlind { get; set; }
    public decimal Limit { get; set; }

    public decimal Pot { get; set; }
    public string BettingPattern { get; private set; }
    public string TreePath { get;  set; }



    public void NextPlayer()
    {
        var posInt = (int)PlayerToAct.Position == 6 ? 0 : (int)PlayerToAct.Position + 1;
        var notFound = true;
        while (notFound)
        {
            
            var foundPlayer = PlayersInHand.FirstOrDefault(x => (int)x.Position == posInt);
            posInt = posInt == 6 ? 0 : posInt + 1;
            if (foundPlayer == null) continue;
            PlayerToAct = foundPlayer;
            notFound = false;

        }
    }

    public List<Card> CreateDeck()
    {
        var newDeck = new List<Card>();
        for (var i = 2; i < 15; i++)
        for (var j = 1; j < 5; j++)
            newDeck.Add(new Card((CardValue)i, (CardColor)j));
        return newDeck;
    }

    public List<Player?> CreatePlayers()
    {
        var newPlayers = new List<Player?>();
        var names = new List<string> { "HeroGto", "GTO2", "GTO3", "GTO4", "GTO5", "GTO6" };
        names.Shuffle();
        var pos = 1;
        foreach (var name in names)
        {
            newPlayers.Add(new Player { Chips = 100, Name = name, Position = (Position)pos });
            pos++;
        }

        PlayerToAct = newPlayers.FirstOrDefault(x => x.Position == Position.SB);
        return newPlayers;
    }

    public void DealCards()
    {
        Deck.Shuffle();
        var rdm = new Random();
        foreach (var player in Players)
        {
            var num = rdm.Next(Deck.Count - 1);
            var card1 = Deck[num];
            Deck.Remove(card1);
            var num2 = rdm.Next(Deck.Count - 1);
            var card2 = Deck[num2];
            player.Hand = new Hand(card1, card2);
        }
    }

    public void UpdateBettingPattern(Decision decision)
    {
        switch (decision.Kind)
        {
            case DecisionKind.Bet:
                BettingPattern += "b";
                break;
            case DecisionKind.Fold:
                BettingPattern += "f";
                break;
            case DecisionKind.Call:
                BettingPattern += "c";
                break;
            case DecisionKind.Check:
                BettingPattern += "c";
                break;
        }
    }

    public void HandleBettingAndCalling(Player player, decimal betSize)
    {
        player.Chips -= betSize;
        player.ChipsInvestedInRound += betSize;
        Pot += betSize;
    }


    public Position FindLastToAct()
    {
        var posInt = (int)PlayerToAct.Position == 0 ? 6 : (int)PlayerToAct.Position - 1;
        while (true)
        {
            var foundPlayer = PlayersInHand.FirstOrDefault(x => (int)x.Position == posInt);
            posInt = posInt == 0 ? 6 : posInt - 1;
            if (foundPlayer == null) continue;
            return foundPlayer.Position;
        }
    }
}