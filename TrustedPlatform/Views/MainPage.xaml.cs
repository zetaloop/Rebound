using Rebound.TrustedPlatform.Models;

namespace Rebound.TrustedPlatform.Views;

public sealed partial class MainPage : Page
{
    public TpmViewModel ViewModelTpm
    {
        get;
    }
    public TpmManager TpmManager
    {
        get;
    }  // Old method for original properties

    public MainPage()
    {
        this.InitializeComponent();

        // Initialize TpmManager for the original properties
        TpmManager = new TpmManager();
        // Initialize ViewModelTpm for new properties
        ViewModelTpm = new TpmViewModel();

        // Set DataContext to TpmManager for original properties
        this.DataContext = this;

        // Load new TPM information asynchronously
        _ = ViewModelTpm.LoadTpmInfoAsync();

        // Assign values to the UI elements, ensuring to convert to strings if necessary
        ManufacturerName.Text = ViewModelTpm.ManufacturerName;
        ManufacturerVersion.Text = ViewModelTpm.ManufacturerVersion;
        SpecificationVersion.Text = ViewModelTpm.SpecificationVersion;
        TpmSubVersion.Text = ViewModelTpm.TpmSubVersion;
        PcClientSpecVersion.Text = ViewModelTpm.PcClientSpecVersion;
        PcrValues.Text = ViewModelTpm.PcrValues;
        Status.Text = ViewModelTpm.Status;

        // Status bar logic
        StatusBar.Severity = ViewModelTpm.Status == "已就绪" ? InfoBarSeverity.Success : InfoBarSeverity.Error;
        StatusBar.Title = $"状态：{ViewModelTpm.Status}";
    }

    ContentDialog dial;

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            PrimaryButtonText = "重置",
            SecondaryButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            Title = "重置 TPM",
            Content = "该操作不可挽回，确定要继续吗？",
        };
        dial = dialog;
        dialog.PrimaryButtonClick += Dialog_PrimaryButtonClick;
        dialog.SecondaryButtonClick += Dialog_SecondaryButtonClick;
        await dialog.ShowAsync(); // Show the dialog
    }

    private void Dialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        sender.Hide();
        dial = null;
        sender = null;
    }

    private async void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
        dial.Content = "开始重置 TPM... 请稍等。";
        dial.IsPrimaryButtonEnabled = false;
        dial.IsSecondaryButtonEnabled = false;

        // Call the TPM reset function
        await TpmReset.ResetTpmAsync(sender);

        // Assign values to the UI elements, ensuring to convert to strings if necessary
        ManufacturerName.Text = ViewModelTpm.ManufacturerName;
        ManufacturerVersion.Text = ViewModelTpm.ManufacturerVersion;
        SpecificationVersion.Text = ViewModelTpm.SpecificationVersion;
        TpmSubVersion.Text = ViewModelTpm.TpmSubVersion;
        PcClientSpecVersion.Text = ViewModelTpm.PcClientSpecVersion;
        Status.Text = ViewModelTpm.Status;

        // Status bar logic
        StatusBar.Severity = ViewModelTpm.Status == "已就绪" ? InfoBarSeverity.Success : InfoBarSeverity.Error;
        StatusBar.Title = $"状态：{ViewModelTpm.Status}";
    }

    private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {

        // Assign values to the UI elements, ensuring to convert to strings if necessary
        ManufacturerName.Text = ViewModelTpm.ManufacturerName;
        ManufacturerVersion.Text = ViewModelTpm.ManufacturerVersion;
        SpecificationVersion.Text = ViewModelTpm.SpecificationVersion;
        TpmSubVersion.Text = ViewModelTpm.TpmSubVersion;
        PcClientSpecVersion.Text = ViewModelTpm.PcClientSpecVersion;
        Status.Text = ViewModelTpm.Status;

        // Status bar logic
        StatusBar.Severity = ViewModelTpm.Status == "已就绪" ? InfoBarSeverity.Success : InfoBarSeverity.Error;
        StatusBar.Title = $"状态：{ViewModelTpm.Status}";

    }
}

