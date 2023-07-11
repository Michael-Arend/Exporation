
using Poker.Data;
using PokerLibrary.Business;

var gameHandler = new GameBusinessHandler(@"C:\PioSolver\PioSOLVER2-pro.exe");
GameBusinessHandler.NewMessage += GameBusinessHandlerNewMessage;

void GameBusinessHandlerNewMessage(object? sender, string e)
{
    Console.WriteLine(e);
}

var handAmount = Console.ReadLine();
if (Int32.TryParse(handAmount, out var hands))
{
 
    await gameHandler.Play(CarrotsRanges.GetRanges(), hands, @"s:",  @"C:\PioSolver\hh.txt");
}








