using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Poker.Data;
using PokerFrontend.Business;
using PokerFrontend.Infrastructure.Models;
using PokerLibrary.Business;

namespace PokerFrontend.ViewModel.HandHistories;

public class CreateViewModel
    : BaseViewModel
{
    private string _amount;
    private readonly GameBusinessHandler _gameBusinessHandler;

    private string _information;
    private PreFlopRange _range;
    private List<PreFlopRange> _ranges;

    private readonly ReadSaveSettingsBusinessHandler _readSaveSettingsBusinessHandler;
    private bool _started;


    public CreateViewModel(ReadSaveSettingsBusinessHandler readSaveSettingsBusinessHandler)
    {
        _readSaveSettingsBusinessHandler = readSaveSettingsBusinessHandler;
        StopCommand = new RelayCommand(Stop);
        _information = "";
        _gameBusinessHandler = new GameBusinessHandler(@"C:\PioSolver\PioSOLVER2-pro.exe");
        StartCommand = new AsyncRelayCommand(Start);
        Initialize();
    }


    public ICommand StartCommand { get; set; }
    public ICommand StopCommand { get; set; }

    public string Information
    {
        get => _information;
        set => SetProperty(ref _information, value);
    }

    public List<PreFlopRange> Ranges
    {
        get => _ranges;
        set => SetProperty(ref _ranges, value);
    }

    public PreFlopRange Range
    {
        get => _range;
        set => SetProperty(ref _range, value);
    }

    public bool Started
    {
        get => _started;
        set => SetProperty(ref _started, value);
    }

    public string Amount
    {
        get => _amount;
        set
        {
            SetProperty(ref _amount, value);
            ValidateAmount();
        }
    }


    private void Load()
    {
        Ranges = _readSaveSettingsBusinessHandler.GetPreFlopRanges().ToList();
    }

    public void Initialize()
    {
        Load();
        Started = false;
    }

    private async Task Start()
    {
        GameBusinessHandler.NewMessage += HandleNewMessage;
        Started = true;
        //Todo convert range in format
        var task = Task.Run(() =>
            _gameBusinessHandler.Play(CarrotsRanges.GetRanges(), int.Parse(Amount), @"F:\", @"C:\PioSolver\hh.txt"));

        while (!task.IsCompleted) await Task.Delay(100);
    }

    private void Stop()
    {
        _gameBusinessHandler.Stop();
        _started = false;
        GameBusinessHandler.NewMessage -= HandleNewMessage;
    }

    private void HandleNewMessage(object? sender, string e)
    {
        Information += "\n";
        Information += e;
    }


    private void ValidateAmount()
    {
        ClearValidationErrors("Amount");

        if (!int.TryParse(Amount, out var amount) || amount <= 0)
            AddValidationError("Amount", "has to be a decimal number greater 0");
    }
}