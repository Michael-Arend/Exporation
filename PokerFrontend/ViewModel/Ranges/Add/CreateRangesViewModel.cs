using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerFrontend.Business;
using PokerFrontend.Infrastructure.Enums;
using PokerFrontend.Infrastructure.Models;

namespace PokerFrontend.ViewModel.Ranges.Add;

public class CreateRangesViewModel : ObservableObject
{
        private object _selectedViewModel;

        private readonly RangeActionViewModel _rangeActionViewModel;
        private int _siteCounter;
        private int _minRangesCounter;
        private PreFlopRange _preflopRange;
 
    private ReadSaveSettingsBusinessHandler _businessHandler;

    public CreateRangesViewModel(ReadSaveSettingsBusinessHandler businessHandler)
    {
        var rangeBasicInformationViewModel = new RangeBasicInformationViewModel();
            rangeBasicInformationViewModel.RangeBasicCompletedEvent += BasicCompleted;
            _rangeActionViewModel = new RangeActionViewModel();
            SelectedViewModel = rangeBasicInformationViewModel;
            _businessHandler = businessHandler;
    }



    private void BasicCompleted(object? sender, PreFlopRange e)
    {
        _preflopRange = e;
        var businessHandler = new RangesBusinessHandler();
        _preflopRange.PreFlopActions = businessHandler.BuildDefaultRanges().PreFlopActions.Take(5);
        _siteCounter = 0;
        _rangeActionViewModel.NextEvent += (i,o)=>NextAction(); ;
        _rangeActionViewModel.CompleteEvent += RangeActionViewModelCompleteEvent;
        SelectedViewModel = _rangeActionViewModel;
        
        _minRangesCounter = _preflopRange.PreFlopActions.Count();
        NextAction();
    }

    private void RangeActionViewModelCompleteEvent(object? sender, EventArgs e)
    {
        _businessHandler.SavePreFlopRange(_preflopRange);
    }




    private void NextAction()
    {
        _siteCounter++;
        if (_siteCounter <= _preflopRange.PreFlopActions.Count())
        {
            _rangeActionViewModel.NextAction(_preflopRange.PreFlopActions.ToList()[_siteCounter - 1], _siteCounter, _minRangesCounter);
        }
        else
        {
            var newCustomRange = new PreFlopAction("", "", DecisionKind.Bet, 3, "", "", false);
            _preflopRange.PreFlopActions.Append(newCustomRange);
            _rangeActionViewModel.NextAction(newCustomRange, _siteCounter - _minRangesCounter, _siteCounter);
        }
   

    }

    public PreFlopRange Range
    {
        get => _preflopRange;
        set => SetProperty(ref _preflopRange, value);
    }


    public object SelectedViewModel
        {
            get => _selectedViewModel;
            set => SetProperty(ref _selectedViewModel, value);
        }
   

    }