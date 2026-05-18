using System.Configuration;
using System.Data;
using System.Windows;
using System.Reflection;

namespace smartFinder;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        EnsureStandardPopupAlignment();
        base.OnStartup(e);
    }

    private static void EnsureStandardPopupAlignment()
    {
        try
        {
            var field = typeof(SystemParameters).GetField("_menuDropAlignment", 
                BindingFlags.NonPublic | BindingFlags.Static);
            
            if (field != null)
            {
                field.SetValue(null, false);
            }
        }
        catch { }
    }
}

