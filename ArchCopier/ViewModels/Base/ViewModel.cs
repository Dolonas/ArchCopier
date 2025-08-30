using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProductComposition.ViewModels.Base;

public abstract class ViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	internal void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected virtual void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (Equals(field, value)) return;
		field = value;
		OnPropertyChanged(propertyName);
	}
}