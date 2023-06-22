
using Poker.Data;
using PokerLibrary.Business;

var gameHandler = new GameBusinessHandler();
GameBusinessHandler.NewMessage += GameBusinessHandlerNewMessage;

void GameBusinessHandlerNewMessage(object? sender, string e)
{
    Console.WriteLine(e);
}

var handAmount = Console.ReadLine();
if (Int16.TryParse(handAmount, out var hands))
{
    gameHandler.Play(CarrotsRanges.GetRanges(), hands, @"F:\", @"C:\PioSolver\PioSOLVER2-pro.exe", @"C:\PioSolver\hh.txt");
}









