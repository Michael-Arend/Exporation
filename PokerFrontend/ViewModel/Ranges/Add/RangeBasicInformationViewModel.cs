using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokerFrontend.Infrastructure.Models;

namespace PokerFrontend.ViewModel.Ranges.Add;

public class RangeBasicInformationViewModel : BaseViewModel
{
    private bool _buttonEnabled;
    private string _limitString;
    private string _name;
    private string _rakeCap;
    private string _rakeInPercent;
    private string _startingStacks;


    public RangeBasicInformationViewModel()
    {
        _name = "";
        ButtonCommand = new RelayCommand(Next);
        RakeInPercent = "5";
        StartingStacks = "100";
        RakeCap = "0";
    }


    public ICommand ButtonCommand { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            SetProperty(ref _name, value);
            ValidateName();
        }
    }

    public string LimitString
    {
        get => _limitString;
        set => SetProperty(ref _limitString, value);
    }

    public string StartingStacks
    {
        get => _startingStacks;
        set
        {
            SetProperty(ref _startingStacks, value);
            ValidateStartingStacks();
        }
    }

    public string RakeInPercent
    {
        get => _rakeInPercent;
        set
        {
            SetProperty(ref _rakeInPercent, value);
            ValidateRakeInPercent();
        }
    }

    public string RakeCap
    {
        get => _rakeCap;
        set
        {
            SetProperty(ref _rakeCap, value);
            ValidateRakeCap();
        }
    }

    public bool ButtonEnabled
    {
        get => _buttonEnabled;
        set => SetProperty(ref _buttonEnabled, value);
    }

    public event EventHandler<PreFlopRange>? RangeBasicCompletedEvent;

    private void Next()
    {
        RangeBasicCompletedEvent?.Invoke(this, new PreFlopRange(Name, int.Parse(StartingStacks), LimitFromLimitString(), double.Parse(RakeInPercent), double.Parse(RakeCap),new List<PreFlopAction>()));
    }

    private int LimitFromLimitString()
    {
        return int.Parse(LimitString[2..]);
    }

    private void ValidateStartingStacks()
    {
        ClearValidationErrors("StartingStacks");

        if (!int.TryParse(StartingStacks, out var stacks) || stacks is > 500 or < 1)
            AddValidationError("StartingStacks", "has to be a number between 1 and 500");
        ButtonEnabled = CheckButtonEnabled();
    }

    private void ValidateRakeCap()
    {
        ClearValidationErrors("RakeCap");

        if (!decimal.TryParse(RakeCap, out var cap) || cap < 0)
            AddValidationError("RakeCap", "Has to be a positive decimal number");
        ButtonEnabled = CheckButtonEnabled();
    }

    private void ValidateName()
    {
        ClearValidationErrors("Name");

        if (Name.Length is > 200 or 0)
            AddValidationError("Name", "Mandatory and max. 200 character");
        ButtonEnabled = CheckButtonEnabled();
    }

    private void ValidateRakeInPercent()
    {
        ClearValidationErrors("RakeInPercent");

        if (!decimal.TryParse(RakeInPercent, out var rake) || rake < 0 || rake > 50)
            AddValidationError("RakeInPercent", "has to be a decimal number between 0 and 50");
        ButtonEnabled = CheckButtonEnabled();
    }

    private bool CheckButtonEnabled()
    {
        return !HasErrors && Name.Length > 0 && LimitString?.Length > 0;
    }
}