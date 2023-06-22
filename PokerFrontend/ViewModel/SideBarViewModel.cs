using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokerFrontend.Infrastructure.Enums;

namespace PokerFrontend.ViewModel;

public class SideBarViewModel : ObservableObject
{
    public SideBarViewModel()
    {
        NavigationCommand = new RelayCommand<NavigationEnum>(NavigateTo);
    }

    public ICommand NavigationCommand { get; set; }
    public event EventHandler<NavigationEnum>? NavigationEvent;

    private void NavigateTo(NavigationEnum target)
    {
        NavigationEvent?.Invoke(this, target);
    }
}