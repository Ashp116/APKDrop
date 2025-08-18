using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace APKDrop;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private bool isLoading;
    public bool IsLoading
    {
        get => isLoading;
        set
        {
            if (isLoading != value)
            {
                isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }

    private double loadingProgress;
    public double LoadingProgress
    {
        get => loadingProgress;
        set
        {
            if (loadingProgress != value)
            {
                loadingProgress = value;
                OnPropertyChanged(nameof(LoadingProgress));
            }
        }
    }

    private string selectedApkPath;
    public ObservableCollection<DeviceInfo> Devices { get; set; } = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        
        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            LoadConnectedDevices();
            return true;
        });
    }
    
    private async void OnSelectApkClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select an APK",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".apk" } }
            })
        });

        if (result != null)
        {
            selectedApkPath = result.FullPath;
            PushButton.IsEnabled = true;
            ApkPathLabel.Text = $"Selected APK: {selectedApkPath}";
        }
        else
        {
            ApkPathLabel.Text = !string.IsNullOrEmpty(selectedApkPath) ? $"Selected APK: {selectedApkPath}" : "No APK selected.";
        }
    }

    private async void OnPushClicked(object sender, EventArgs e)
    {
        var selectedDevices = Devices.Where(d => d.IsSelected).ToList();
        if (selectedDevices.Count == 0 || string.IsNullOrEmpty(selectedApkPath))
        {
            await DisplayAlert("Error", "Select at least one device and an APK.", "OK");
            return;
        }

        try
        {
            IsLoading = true;
            LoadingProgress = 0;

            int successPushes = 0;
            int total = selectedDevices.Count;

            for (int i = 0; i < total; i++)
            {
                var device = selectedDevices[i];
                bool success = await Task.Run(() => AdbHelper.PushAppInstall(device.Serial, selectedApkPath));
                if (success) successPushes++;

                LoadingProgress = (i + 1) / (double)total;
                await Task.Delay(100); // optional small delay to show progress
            }

            if (successPushes > 0)
                await DisplayAlert("Success", $"Pushed to {successPushes} devices!", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadConnectedDevices()
    {
        var adbDevices = AdbHelper.ListDevices();

        foreach (var device in adbDevices)
        {
            var existingDevice = Devices.FirstOrDefault(d => d.Serial == device.SerialNumber);
            if (existingDevice != null)
            {
                existingDevice.DeviceName = device.DeviceName;
                existingDevice.Model = device.Model;
            }
            else
            {
                Devices.Add(new DeviceInfo
                {
                    Serial = device.SerialNumber,
                    DeviceName = device.DeviceName,
                    Model = device.Model
                });
            }
        }

        for (int i = Devices.Count - 1; i >= 0; i--)
        {
            if (!adbDevices.Any(d => d.SerialNumber == Devices[i].Serial))
            {
                Devices.RemoveAt(i);
            }
        }
    }


    private void OnSelectAllClicked(object sender, EventArgs e)
    {
        foreach (var device in Devices)
            device.IsSelected = true;
    }

    private void OnDeselectAllClicked(object sender, EventArgs e)
    {
        foreach (var device in Devices)
            device.IsSelected = false;
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadConnectedDevices();
    }

    private void OnDeviceTapped(object sender, EventArgs e)
    {
        if (sender is Frame tappedFrame && tappedFrame.BindingContext is DeviceInfo device)
        {
            device.IsSelected = !device.IsSelected;
        }
    }

    // PropertyChanged support
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
