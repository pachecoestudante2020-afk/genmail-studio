namespace GenMail.Wpf.ViewModels;

public sealed class RuleOptionViewModel(string id) : ObservableObject
{
    private bool _isSelected;

    public string Id { get; } = id;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
