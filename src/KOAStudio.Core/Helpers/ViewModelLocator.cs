using CommunityToolkit.Mvvm.DependencyInjection;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;

namespace KOAStudio.Core.Helpers
{
    // https://github.com/PrismLibrary/Prism/blob/17d20e9c7723b1fc6db3f2f25daf11b8febd5b5e/src/Wpf/Prism.Wpf/Mvvm/ViewModelLocator.cs#L14

    /// <summary>
    /// This class defines the attached property and related change handler that calls the ViewModelLocator in Prism.Mvvm.
    /// </summary>
    public static class ViewModelLocator
    {
        /// <summary>
        /// The AutoWireViewModel attached property.
        /// </summary>
        public static DependencyProperty AutoWireViewModelProperty = DependencyProperty.RegisterAttached("AutoWireViewModel", typeof(bool?), typeof(ViewModelLocator), new PropertyMetadata(defaultValue: null, propertyChangedCallback: AutoWireViewModelChanged));

        /// <summary>
        /// Gets the value for the <see cref="AutoWireViewModelProperty"/> attached property.
        /// </summary>
        /// <param name="obj">The target element.</param>
        /// <returns>The <see cref="AutoWireViewModelProperty"/> attached to the <paramref name="obj"/> element.</returns>
        public static bool? GetAutoWireViewModel(DependencyObject obj)
        {
            return (bool?)obj.GetValue(AutoWireViewModelProperty);
        }

        /// <summary>
        /// Sets the <see cref="AutoWireViewModelProperty"/> attached property.
        /// </summary>
        /// <param name="obj">The target element.</param>
        /// <param name="value">The value to attach.</param>
        public static void SetAutoWireViewModel(DependencyObject obj, bool? value)
        {
            obj.SetValue(AutoWireViewModelProperty, value);
        }

        private static void AutoWireViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(d))
            {
                var value = (bool?)e.NewValue;
                if (value.HasValue && value.Value)
                {
                    if (d is FrameworkElement view)
                    {
                        var viewModel = GetViewModelForView(view);
                        view.DataContext = viewModel;
                    }
                }
            }
        }

        private static object? GetViewModelForView(object view)
        {
            var viewType = view.GetType();
            var viewName = viewType.FullName;
            if (viewName is not null)
            {
                viewName = viewName.Replace(".Views.", ".ViewModels.");
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var suffix = viewName.EndsWith("View") ? "Model" : "ViewModel";
                var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}{1}, {2}", viewName, suffix, viewAssemblyName);
                var viemodel_type = Type.GetType(viewModelName);
                if (viemodel_type is not null)
                {
                    var constructInfo = viemodel_type.GetConstructors()[0];
                    var parameters = constructInfo.GetParameters().Select(x => Ioc.Default.GetService(x.ParameterType)).ToArray();
                    return Activator.CreateInstance(viemodel_type, parameters);
                }
            }
            return null;
        }

        public static string GetWireNamedViewModel(DependencyObject obj)
        {
            return (string)obj.GetValue(WireNamedViewModelProperty);
        }

        public static void SetWireNamedViewModel(DependencyObject obj, string value)
        {
            obj.SetValue(WireNamedViewModelProperty, value);
        }

        // Using a DependencyProperty as the backing store for WireNamedViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WireNamedViewModelProperty =
            DependencyProperty.RegisterAttached("WireNamedViewModel", typeof(string), typeof(ViewModelLocator), new PropertyMetadata(defaultValue: null, propertyChangedCallback: WireNamedViewModelChanged));

        private static void WireNamedViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(d))
            {
                var value = (string)e.NewValue;
                if (value != null && value.Length > 0)
                {
                    if (d is FrameworkElement view)
                    {
                        if (view.DataContext == null)
                        {
                            var viewModel = GetNamedViewModel(view, value);
                            view.DataContext = viewModel;
                        }
                    }
                }
            }
        }

        private static object? GetNamedViewModel(object view, string name)
        {
            var viewType = view.GetType();
            var viewModelFullName = name;
            if (name.IndexOf(',') == -1)
            {
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                viewModelFullName += ", " + viewAssemblyName;
            }
            var viemodel_type = Type.GetType(viewModelFullName);
            if (viemodel_type != null)
            {
                var diVM = Ioc.Default.GetService(viemodel_type);
                if (diVM != null)
                {
                    return diVM;
                }

                var constructInfo = viemodel_type.GetConstructors()[0];
                var parameters = constructInfo.GetParameters().Select(x => Ioc.Default.GetService(x.ParameterType)).ToArray();
                return Activator.CreateInstance(viemodel_type, parameters);
            }
            return null;
        }
    }
}
