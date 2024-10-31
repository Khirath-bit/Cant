using System.Windows;
using System.Windows.Controls;
using Cant.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts.Wpf;

namespace Cant.ViewModel;
internal partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private UserControl _control = new VariableControl();

    [ObservableProperty]
    private Visibility _addPlotVisibility = Visibility.Collapsed;

    [RelayCommand]
    private void NavigateToPlots()
    {
        Control = new PlotControl();
        AddPlotVisibility = Visibility.Visible;
    }

    [RelayCommand]
    private void AddPlot()
    {
        if (_control is not PlotControl { DataContext: PlotViewModel vm })
            return;

        vm.OnPlotSettingsBtnClicked();
    }
}
