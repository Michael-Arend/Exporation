using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerFrontend.Business;
using PokerFrontend.Infrastructure.Enums;
using PokerFrontend.ViewModel;
using PokerFrontend.ViewModel.HandHistories;
using PokerFrontend.ViewModel.Ranges.Add;

namespace PokerFrontend
{
    public class MainViewModel : ObservableObject
    {
        private object _selectedViewModel;

        private SideBarViewModel _sideBarViewModel;
        private readonly CreateRangesViewModel _createRangesViewModel;
        private readonly CreateViewModel _createViewModel;

        public MainViewModel(CreateRangesViewModel createRangesViewModel, CreateViewModel createViewModel)
        {
            _sideBarViewModel = new SideBarViewModel();
            _createRangesViewModel = createRangesViewModel;
            _createViewModel = createViewModel;
            _sideBarViewModel.NavigationEvent += SetNavigation;

        }

        private void SetNavigation(object? sender, NavigationEnum e)
        {
            switch (e)
            {
                case NavigationEnum.CreateRange:
                    SelectedViewModel = _createRangesViewModel;
                    break;
                case NavigationEnum.CreateHandHistories:
                    _createViewModel.Initialize();
                    SelectedViewModel = _createViewModel;
                    break;

            }
        }

        public object SelectedViewModel
        {
            get => _selectedViewModel;
            set => SetProperty(ref _selectedViewModel, value);
        }
        public SideBarViewModel SideBarViewModel
        {
            get => _sideBarViewModel;
            set => SetProperty(ref _sideBarViewModel, value);
        }


    }
}