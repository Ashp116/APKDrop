using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace APKDrop;

public class DeviceInfo : INotifyPropertyChanged
{
    public string Serial { get; set; }
    public string Model { get; set; }

    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected != value) // Toggle selection
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(BorderColor));
            }
        }
    }

    // Use TryGetValue to fetch colors from resources dynamically
    public Color BackgroundColor
    {
        get
        {
            if (Application.Current.Resources.TryGetValue("SelectedBackgroundColor", out var colorValue) && colorValue is Color selectedColor)
            {
                return IsSelected ? selectedColor : GetDeselectedBackgroundColor();
            }
            return GetDeselectedBackgroundColor();
        }
    }

    public Color BorderColor
    {
        get
        {
            if (Application.Current.Resources.TryGetValue("SelectedBorderColor", out var colorValue) && colorValue is Color selectedColor)
            {
                return IsSelected ? selectedColor : GetDeselectedBorderColor();
            }
            return GetDeselectedBorderColor();
        }
    }

    // Helper methods to handle deselected colors if not found in resources
    private Color GetDeselectedBackgroundColor()
    {
        return Application.Current.Resources.TryGetValue("DeselectedBackgroundColor", out var colorValue) && colorValue is Color deselectedColor
            ? deselectedColor
            : Color.FromArgb("#2c2c2c"); // Default if not found
    }

    private Color GetDeselectedBorderColor()
    {
        return Application.Current.Resources.TryGetValue("DeselectedBorderColor", out var colorValue) && colorValue is Color deselectedColor
            ? deselectedColor
            : Color.FromArgb("#444444"); // Default if not found
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}