using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerFrontend.Business;
using PokerFrontend.Infrastructure.Models;

namespace PokerFrontend.ViewModel.Ranges.Add;

public class CreateRangesViewModel : ObservableObject
{
        private object _selectedViewModel;

        private readonly RangeBasicInformationViewModel rangeBasicInformationViewModel;
        private readonly RangeActionViewModel rangeActionViewModel;
        private int siteCounter;
        private int minRangesCounter;
        private PreFlopRange preflopRange;

    public CreateRangesViewModel()
    {
            rangeBasicInformationViewModel = new RangeBasicInformationViewModel();
            rangeBasicInformationViewModel.RangeBasicCompletedEvent += BasicCompleted;
            rangeActionViewModel = new RangeActionViewModel();
            SelectedViewModel = rangeBasicInformationViewModel;
        }

    private void BasicCompleted(object? sender, PreFlopRange e)
    {
        preflopRange = e;
        var businessHandler = new RangesBusinessHandler();
        preflopRange.PreFlopActions = businessHandler.BuildDefaultRanges().PreFlopActions;
        siteCounter = 0;
        rangeActionViewModel.NextEvent += RangeActionViewModel_NextEvent;
        SelectedViewModel = rangeActionViewModel;
        
        minRangesCounter = preflopRange.PreFlopActions.Count();
        NextAction();
    }

    private void RangeActionViewModel_NextEvent(object? sender, PreFlopAction action)
    {
        preflopRange.PreFlopActions.ToList()[siteCounter - 1] = action;
        NextAction();
    }



    private void NextAction()
    {
        siteCounter++;
        rangeActionViewModel.NextAction(preflopRange.PreFlopActions.ToList()[siteCounter - 1],siteCounter,minRangesCounter);
    }

    public PreFlopRange Range
    {
        get => preflopRange;
        set => SetProperty(ref preflopRange, value);
    }

    public object SelectedViewModel
        {
            get => _selectedViewModel;
            set => SetProperty(ref _selectedViewModel, value);
        }
   

    }