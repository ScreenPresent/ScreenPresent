using Avalonia.Controls;
using System;

namespace ScreenPresent.Views;
/// <summary>
/// Interaction logic for SelectTimeSpanWindow.xaml
/// </summary>
public partial class SelectTimeSpanWindow : Window {
    public readonly ViewModels.SelectTimeSpanViewModel ViewModel;

    public SelectTimeSpanWindow() {
        // Avalonia build constructor
        ViewModel = null!;
    }

    public SelectTimeSpanWindow(Classes.SelectTimeSpanConstructor constructor) {
        InitializeComponent();
        DataContext = ViewModel = new ViewModels.SelectTimeSpanViewModel(constructor);
        ViewModel.OnAccept += ViewModel_OnAccept;
    }

    private void ViewModel_OnAccept(Classes.SelectTimeSpanConstructor obj) {
        Close();
    }
}
