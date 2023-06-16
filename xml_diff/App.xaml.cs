using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using xml_diff.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace xml_diff
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            // 설정 Load
            ((SharedDataViewModel)((App.Current.Resources["ViewModelProvider"] as ViewModelProvider)!.SharedDataViewModel!)).WithoutColor =
                (SolidColorBrush)(new BrushConverter().ConvertFrom(ConfigurationManager.AppSettings["WithoutColor"]!))!;
            ((SharedDataViewModel)((App.Current.Resources["ViewModelProvider"] as ViewModelProvider)!.SharedDataViewModel!)).DupColor =
                (SolidColorBrush)(new BrushConverter().ConvertFrom(ConfigurationManager.AppSettings["DupColor"]!))!;
            ((SharedDataViewModel)((App.Current.Resources["ViewModelProvider"] as ViewModelProvider)!.SharedDataViewModel!)).DiffColor =
                (SolidColorBrush)(new BrushConverter().ConvertFrom(ConfigurationManager.AppSettings["DiffColor"]!))!;
            ((SharedDataViewModel)((App.Current.Resources["ViewModelProvider"] as ViewModelProvider)!.SharedDataViewModel!)).UncheckedColor =
                (SolidColorBrush)(new BrushConverter().ConvertFrom(ConfigurationManager.AppSettings["UncheckedColor"]!))!;
        }
    }
}
