using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;


namespace APKDrop;

public partial class MainPage : ContentPage
{
    private string selectedApkPath;
    public ObservableCollection<DeviceInfo> Devices { get; set; } = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadConnectedDevices();
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
            ApkPathLabel.Text = selectedApkPath != "" ? $"Selected APK: {selectedApkPath}" : "No APK selected.";
        }
    }
    
    private async void OnPushClicked(object sender, EventArgs e)
    {
        var selectedDevices = Devices.Where(d => d.IsSelected).ToList();
        
        int successPushes = 0;
        foreach (var device in selectedDevices)
        {
            var success = AdbHelper.PushAppInstall(device.Serial, selectedApkPath);
            if (success)
            {
                successPushes++;
            }
            else
            {
                await DisplayAlert("Failed", $"Could not push to {device.Serial}", "OK");
            }
        }
        
        if (successPushes > 0)
            await DisplayAlert("Success", $"Pushed to {successPushes} devices!", "OK");
    }

    private void LoadConnectedDevices()
    {
        var adbDevices = AdbHelper.ListDevices(); // Replace with your actual ADB helper method
        Devices.Clear();

        foreach (var device in adbDevices)
        {
            Devices.Add(new DeviceInfo
            {
                Serial = device.SerialNumber,
                Model = device.Model
            });
        }
    }

    private void OnSelectAllClicked(object sender, EventArgs e)
    {
        foreach (var device in Devices)
        {
            device.IsSelected = true;
        }
    }

    private void OnDeselectAllClicked(object sender, EventArgs e)
    {
        foreach (var device in Devices)
        {
            device.IsSelected = false;
        }
    }
    
    private void OnRefreshClicked(object sender, EventArgs e)
    {
        LoadConnectedDevices();
    }

    private void OnDeviceTapped(object sender, EventArgs e)
    {
        if (sender is Frame tappedFrame)
        {
            // Find the device associated with the tapped frame (you can access the device from the BindingContext of the frame)
            var device = tappedFrame.BindingContext as DeviceInfo;
        
            if (device != null)
            {
                // Toggle the IsSelected property to allow multi-selection
                device.IsSelected = !device.IsSelected;
            }
        }
    }

}
