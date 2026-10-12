using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Tamagotchi.ViewModels;

namespace Tamagotchi.Vistas;

/// Dibuja y anima la rueda del ropero. Qué ropa está elegida lo decide
/// RoperoViewModel (IndiceSeleccionado); esta clase solo se encarga de
/// que la rueda GIRE hasta dejar esa ropa arriba, bajo la flechita.
///
/// Cómo funciona el giro:
///   - _posicion es "qué porción está arriba", pero con decimales
///     (2.5 = justo entre la porción 2 y la 3).
///   - Cada porción i está a un ángulo (i - _posicion) * 360 / cantidad.
///     Ángulo 0 = arriba de todo.
///   - Al elegir otra ropa, cambia _objetivo, y un timer acerca _posicion
///     a _objetivo un poquito en cada cuadro (rápido al principio, lento
///     al final): eso hace que la rueda gire y "frene" sola.
public partial class RoperoView : UserControl
{
    // Medidas de la rueda (el Canvas "Rueda" mide 560x560)
    private const double Centro = 280;
    private const double Radio = 185;
    private const double AnchoPorcion = 150;
    private const double AltoPorcion = 180;

    // Qué fracción de la distancia que falta se recorre en cada cuadro
    private const double Suavidad = 0.12;

    // Borde de la porción elegida (naranja) y de las demás
    private static readonly IBrush BordeElegida = new SolidColorBrush(Color.Parse("#FF7043"));
    private static readonly IBrush BordeNormal = new SolidColorBrush(Color.Parse("#55FFFFFF"));

    private readonly List<Border> _porciones = new();
    private readonly DispatcherTimer _timerGiro;

    private double _posicion;
    private double _objetivo;

    private RoperoViewModel? _vm;

    public RoperoView()
    {
        InitializeComponent();

        // ~60 cuadros por segundo
        _timerGiro = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timerGiro.Tick += (_, _) => AvanzarGiro();

        DataContextChanged += (_, _) =>
        {
            if (_vm != null)
                _vm.PropertyChanged -= Vm_PropertyChanged;

            _vm = DataContext as RoperoViewModel;

            if (_vm != null)
                _vm.PropertyChanged += Vm_PropertyChanged;
        };

        DetachedFromVisualTree += (_, _) =>
        {
            _timerGiro.Stop();
            if (_vm != null)
                _vm.PropertyChanged -= Vm_PropertyChanged;
        };
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_vm == null) return;

        if (e.PropertyName == nameof(RoperoViewModel.Opciones))
        {
            // Llegó la ropa de la API: se arma la rueda ya ubicada en la
            // ropa que tiene puesta, sin animación
            ArmarRueda();
            _posicion = _objetivo = _vm.IndiceSeleccionado;
            Dibujar();
        }
        else if (e.PropertyName == nameof(RoperoViewModel.IndiceSeleccionado))
        {
            GirarHasta(_vm.IndiceSeleccionado);
        }
    }

    /// Crea una porción (un cuadrito con el pingüino y el nombre) por cada
    /// ropa, y los rayos de la rueda
    private void ArmarRueda()
    {
        if (_vm == null) return;

        Porciones.Children.Clear();
        Rayos.Children.Clear();
        _porciones.Clear();

        int cantidad = _vm.Opciones.Count;

        for (int i = 0; i < cantidad; i++)
        {
            var opcion = _vm.Opciones[i];

            var contenido = new StackPanel { Spacing = 2, HorizontalAlignment = HorizontalAlignment.Center };
            contenido.Children.Add(new Image { Source = opcion.Vista, Width = 120, Height = 120 });
            contenido.Children.Add(new TextBlock
            {
                Text = opcion.Nombre,
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold,
                FontSize = 14,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            if (!opcion.Disponible)
            {
                contenido.Children.Add(new TextBlock
                {
                    Text = opcion.TextoBloqueo,
                    Foreground = new SolidColorBrush(Color.Parse("#FFE082")),
                    FontSize = 12,
                    TextAlignment = TextAlignment.Center
                });
            }

            var porcion = new Border
            {
                Width = AnchoPorcion,
                Height = AltoPorcion,
                CornerRadius = new CornerRadius(20),
                Background = new SolidColorBrush(Color.Parse("#EE2A2A45")),
                BorderThickness = new Thickness(4),
                Padding = new Thickness(6),
                Child = contenido,
                Cursor = new Cursor(StandardCursorType.Hand),
                // Las que tienen candado se ven medio apagadas
                Opacity = opcion.Disponible ? 1 : 0.55
            };

            // Clic en una porción = girar hasta ella
            int indice = i;
            porcion.PointerPressed += (_, _) => _vm?.Elegir(indice);

            _porciones.Add(porcion);
            Porciones.Children.Add(porcion);

            // Un rayo desde el centro hacia cada porción
            double angulo = i * 2 * Math.PI / cantidad;
            Rayos.Children.Add(new Line
            {
                StartPoint = new Point(Centro, Centro),
                EndPoint = new Point(Centro + 255 * Math.Sin(angulo), Centro - 255 * Math.Cos(angulo)),
                Stroke = new SolidColorBrush(Color.Parse("#99FFE082")),
                StrokeThickness = 4
            });
        }
    }

    /// Calcula hacia dónde tiene que girar la rueda y arranca la animación
    private void GirarHasta(int indice)
    {
        if (_vm == null || _porciones.Count == 0) return;

        int cantidad = _porciones.Count;

        // En qué porción está "parado" el objetivo ahora (sin contar vueltas)
        int desde = Modulo((int)Math.Round(_objetivo), cantidad);
        int diferencia = Modulo(indice - desde, cantidad);

        if (_vm.VueltasExtra > 0)
        {
            // Giro al azar: siempre para adelante y con vueltas de más, como una ruleta
            diferencia += _vm.VueltasExtra * cantidad;
            _vm.VueltasExtra = 0;
        }
        else if (diferencia > cantidad / 2)
        {
            // Giro normal: por el camino más corto (para atrás si queda más cerca)
            diferencia -= cantidad;
        }

        _objetivo += diferencia;
        _timerGiro.Start();
    }

    private void AvanzarGiro()
    {
        double falta = _objetivo - _posicion;

        if (Math.Abs(falta) < 0.002)
        {
            _posicion = _objetivo;
            _timerGiro.Stop();
        }
        else
        {
            _posicion += falta * Suavidad;
        }

        Dibujar();
    }

    /// Ubica cada porción alrededor del círculo según _posicion
    private void Dibujar()
    {
        int cantidad = _porciones.Count;
        if (cantidad == 0 || _vm == null) return;

        double paso = 2 * Math.PI / cantidad;

        for (int i = 0; i < cantidad; i++)
        {
            double angulo = (i - _posicion) * paso;

            // Seno y coseno: x a la derecha, y para arriba (por eso el "-")
            double x = Centro + Radio * Math.Sin(angulo);
            double y = Centro - Radio * Math.Cos(angulo);

            // 1 = arriba de todo, 0 = abajo de todo
            double cercania = (1 + Math.Cos(angulo)) / 2;

            var porcion = _porciones[i];
            Canvas.SetLeft(porcion, x - AnchoPorcion / 2);
            Canvas.SetTop(porcion, y - AltoPorcion / 2);

            // La de arriba se ve más grande y queda por delante de las otras
            double escala = 0.6 + 0.4 * cercania;
            porcion.RenderTransform = new ScaleTransform(escala, escala);
            porcion.ZIndex = (int)(cercania * 100);

            porcion.BorderBrush = i == _vm.IndiceSeleccionado ? BordeElegida : BordeNormal;
        }

        // Los rayos giran junto con las porciones
        if (Rayos.RenderTransform is RotateTransform giro)
            giro.Angle = -_posicion * 360 / cantidad;
    }

    // Ruedita del mouse: para arriba gira para atrás, para abajo para adelante
    private void Rueda_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_vm == null) return;

        if (e.Delta.Y > 0)
            _vm.AnteriorCommand.Execute(null);
        else if (e.Delta.Y < 0)
            _vm.SiguienteCommand.Execute(null);

        e.Handled = true;
    }

    // Como el % de C#, pero sin números negativos (-1 % 4 = -1, Modulo(-1, 4) = 3)
    private static int Modulo(int a, int b) => ((a % b) + b) % b;
}
