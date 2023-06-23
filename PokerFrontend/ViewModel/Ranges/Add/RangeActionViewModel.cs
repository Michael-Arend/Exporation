using System;
using System.Globalization;
using System.Security.AccessControl;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokerFrontend.Infrastructure.Enums;
using PokerFrontend.Infrastructure.Models;
namespace PokerFrontend.ViewModel.Ranges.Add;

public class RangeActionViewModel : ObservableObject
{
    private PreFlopAction _action;
    private string _name;
    private string _folder;
    private DecisionKind _decision;
    private string _betSizing;
    private string _range;
    private string _pattern;
    private bool _isDefault;
    private bool _flopSolvesEnabled;
    private int _actualNumber;
    private int _overAllNumber;


    public RangeActionViewModel()
    {
        FolderSelectedCommand = new RelayCommand<string>(FolderSelected);
        NextCommand = new RelayCommand(()=> ActionFinished());
        CompleteCommand = new RelayCommand(() => CompleteEvent?.Invoke(this,null));
    }

    private void ActionFinished()
    {
        _action.Decision = Decision;
        _action.Name = Name;
        _action.Folder = Folder;
        _action.BetSizing = decimal.Parse(BetSizing);
        _action.Range = Range;
        _action.Pattern = Pattern;
        NextEvent?.Invoke(this, _action);
    }

    private void FolderSelected(string? obj)
    {
        if (obj != null)
        {
            Folder = obj;
        }
        
    }

    public event EventHandler<PreFlopAction> NextEvent;
    public event EventHandler CompleteEvent;
    public ICommand FolderSelectedCommand { get; set; }
    public ICommand NextCommand { get; set; }
    public ICommand CompleteCommand { get; set; }
    

    public PreFlopAction Action
    {
        get => _action;
        set => SetProperty(ref _action, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Folder
    {
        get => _folder;
        set => SetProperty(ref _folder, value);
    }
    public DecisionKind Decision
    {
        get => _decision;
        set
        {
            SetProperty(ref _decision, value);
            FlopSolvesEnabled = value == DecisionKind.Call;
        }
    }

    public string BetSizing
    {
        get => _betSizing;
        set
        {
            if (decimal.TryParse(value,out var dec))
            {
                SetProperty(ref _betSizing, value);
            }
        }
    }

    public string Range
    {
        get => _range;
        set => SetProperty(ref _range, value);
    }

    public string Pattern
    {
        get => _pattern;
        set => SetProperty(ref _pattern, value);
    }

    public int ActualNumber
    {
        get => _actualNumber;
        set => SetProperty(ref _actualNumber, value);
    }

    public int OverAllNumber
    {
        get => _overAllNumber;
        set => SetProperty(ref _overAllNumber, value);
    }
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    public bool FlopSolvesEnabled
    {
        get => _flopSolvesEnabled;
        set => SetProperty(ref _flopSolvesEnabled, value);
    }

    public void NextAction(PreFlopAction action, int actualNumber, int overAllNumber)
    {
        Action = action;
        BetSizing = action.BetSizing.ToString(CultureInfo.InvariantCulture);
        Pattern = action.Pattern;
        Range = action.Range;
        Name = action.Name;
        Decision = action.Decision;
        Folder = action.Folder;
        IsDefault = action.IsDefault;
        ActualNumber = actualNumber;
        OverAllNumber = overAllNumber;
    }
}

