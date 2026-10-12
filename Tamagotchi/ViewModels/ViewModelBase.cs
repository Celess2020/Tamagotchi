using CommunityToolkit.Mvvm.ComponentModel;

namespace Tamagotchi.ViewModels;

/// Base de todos los ViewModels. ObservableObject (de CommunityToolkit.Mvvm)
/// ya trae lo necesario para avisarle a la vista cuando cambia una propiedad
/// (INotifyPropertyChanged), así los {Binding} se actualizan solos.
public abstract class ViewModelBase : ObservableObject
{
}
