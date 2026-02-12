using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace ClavierOr;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		using var db = new ClavierOrContext();
		db.Database.Migrate();
	}
}

