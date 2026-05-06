using System.Collections.ObjectModel;
using System.Windows.Input;
using GenMail.Core.Generation;
using GenMail.Core.Models;
using GenMail.Core.Pipeline;
using GenMail.Core.Safety;
using GenMail.Wpf.Commands;
using GenMail.Wpf.Services;
using System.IO;

namespace GenMail.Wpf.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly FileDialogService _fileDialogService = new();
    private readonly FolderOpenService _folderOpenService = new();
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private string _inputPath = string.Empty;
    private string _domain = "example.com";
    private string _numberRangeText = string.Empty;
    private NumberMode _numberMode = NumberMode.BaseOnly;
    private NumberPlacementMode _numberPlacementMode = NumberPlacementMode.SuffixOnly;
    private DedupeMode _dedupeMode = DedupeMode.InMemory;
    private AliasFilterMode _aliasFilterMode = AliasFilterMode.None;
    private long _maxOutputEmails = 1_000_000;
    private long _maxNumbersPerBase = 1_000;
    private string _statusText = "Ready";
    private string _outputFolder = string.Empty;
    private long _inputLinesRead;
    private long _candidatesGenerated;
    private long _emailsWritten;
    private long _duplicatesSkipped;
    private long _qualityRejected;

    public MainViewModel()
    {
        RuleOptions = new ObservableCollection<RuleOptionViewModel>(
            BuiltInUsernameRules.Create().Select(rule => new RuleOptionViewModel(rule.Id)));

        BrowseInputFileCommand = new RelayCommand(BrowseInputFile, () => !IsRunning);
        EstimateCommand = new RelayCommand(Estimate, () => !IsRunning);
        StartCommand = new AsyncRelayCommand(StartAsync, () => !IsRunning);
        CancelCommand = new RelayCommand(Cancel, () => IsRunning);
        OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder, () => !string.IsNullOrWhiteSpace(OutputFolder));
    }

    public ObservableCollection<RuleOptionViewModel> RuleOptions { get; }
    public IEnumerable<NumberMode> NumberModes => Enum.GetValues<NumberMode>();
    public IEnumerable<NumberPlacementMode> NumberPlacementModes => Enum.GetValues<NumberPlacementMode>();
    public IEnumerable<DedupeMode> DedupeModes => Enum.GetValues<DedupeMode>();
    public IEnumerable<AliasFilterMode> AliasFilterModes => Enum.GetValues<AliasFilterMode>();

    public string InputPath { get => _inputPath; set => SetProperty(ref _inputPath, value); }
    public string Domain { get => _domain; set => SetProperty(ref _domain, value); }
    public string NumberRangeText { get => _numberRangeText; set => SetProperty(ref _numberRangeText, value); }
    public NumberMode NumberMode { get => _numberMode; set => SetProperty(ref _numberMode, value); }
    public NumberPlacementMode NumberPlacementMode { get => _numberPlacementMode; set => SetProperty(ref _numberPlacementMode, value); }
    public DedupeMode DedupeMode { get => _dedupeMode; set => SetProperty(ref _dedupeMode, value); }
    public AliasFilterMode AliasFilterMode { get => _aliasFilterMode; set => SetProperty(ref _aliasFilterMode, value); }
    public long MaxOutputEmails { get => _maxOutputEmails; set => SetProperty(ref _maxOutputEmails, value); }
    public long MaxNumbersPerBase { get => _maxNumbersPerBase; set => SetProperty(ref _maxNumbersPerBase, value); }
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
    public string OutputFolder { get => _outputFolder; set { if (SetProperty(ref _outputFolder, value)) RaiseCommandStates(); } }
    public long InputLinesRead { get => _inputLinesRead; set => SetProperty(ref _inputLinesRead, value); }
    public long CandidatesGenerated { get => _candidatesGenerated; set => SetProperty(ref _candidatesGenerated, value); }
    public long EmailsWritten { get => _emailsWritten; set => SetProperty(ref _emailsWritten, value); }
    public long DuplicatesSkipped { get => _duplicatesSkipped; set => SetProperty(ref _duplicatesSkipped, value); }
    public long QualityRejected { get => _qualityRejected; set => SetProperty(ref _qualityRejected, value); }

    public IReadOnlyList<string> SelectedRules => RuleOptions.Where(rule => rule.IsSelected).Select(rule => rule.Id).ToList();

    public ICommand BrowseInputFileCommand { get; }
    public ICommand EstimateCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand OpenOutputFolderCommand { get; }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetProperty(ref _isRunning, value))
            {
                RaiseCommandStates();
            }
        }
    }

    private void BrowseInputFile()
    {
        string? picked = _fileDialogService.PickInputTextFile();
        if (!string.IsNullOrWhiteSpace(picked))
        {
            InputPath = picked;
        }
    }

    private void Estimate()
    {
        try
        {
            long inputCount = File.Exists(InputPath) ? File.ReadLines(InputPath).LongCount() : 0;
            GenerationOptions options = BuildOptions();
            OutputEstimator estimator = new(new GenMail.Core.Numbering.NumberRangeParser());
            SafetyEstimate estimate = estimator.Estimate(inputCount, options);
            StatusText = $"Estimated outputs: {estimate.EstimatedOutputs:n0} (lines: {inputCount:n0}, per-base numbers: {estimate.EstimatedNumbersPerBase:n0})";
        }
        catch (Exception ex)
        {
            StatusText = $"Estimate failed: {ex.Message}";
        }
    }

    private async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        IsRunning = true;
        StatusText = "Running...";
        ResetCounters();

        try
        {
            GenMailPipeline pipeline = new();
            Progress<ProgressSnapshot> progress = new(snapshot =>
            {
                InputLinesRead = snapshot.LinesProcessed;
                CandidatesGenerated = snapshot.UsernamesAccepted;
                EmailsWritten = snapshot.EmailsWritten;
            });

            ProcessingResult result = await pipeline.RunAsync(InputPath, BuildOptions(), progress, _cts.Token).ConfigureAwait(true);
            OutputFolder = result.OutputDirectory;
            InputLinesRead = result.Counters.TotalLines;
            CandidatesGenerated = result.Counters.UsernamesGenerated;
            EmailsWritten = result.Counters.EmailsWritten;
            DuplicatesSkipped = result.Counters.DuplicateSkipped;
            QualityRejected = result.Counters.QualityRejected;
            StatusText = "Completed successfully.";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Canceled.";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void Cancel() => _cts?.Cancel();

    private void OpenOutputFolder() => _folderOpenService.OpenFolder(OutputFolder);

    private GenerationOptions BuildOptions() => new(
        Domain,
        SelectedRules,
        NumberRangeText,
        NumberMode,
        NumberPlacementMode,
        DedupeMode,
        AliasFilterMode,
        SkipEmptyLines: true,
        AllowAllDigitUsernames: false,
        MinUsernameLength: 3,
        MaxUsernameLength: 32,
        OutputRootPath: "output");

    private void ResetCounters()
    {
        InputLinesRead = 0;
        CandidatesGenerated = 0;
        EmailsWritten = 0;
        DuplicatesSkipped = 0;
        QualityRejected = 0;
    }

    private void RaiseCommandStates()
    {
        if (BrowseInputFileCommand is RelayCommand browse) browse.RaiseCanExecuteChanged();
        if (EstimateCommand is RelayCommand estimate) estimate.RaiseCanExecuteChanged();
        if (StartCommand is AsyncRelayCommand start) start.RaiseCanExecuteChanged();
        if (CancelCommand is RelayCommand cancel) cancel.RaiseCanExecuteChanged();
        if (OpenOutputFolderCommand is RelayCommand open) open.RaiseCanExecuteChanged();
    }
}
