using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace ClavierOr;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		var targetDbPath = ClavierOrContext.GetDatabasePath();
		var legacyRuntimeDbPath = Path.Combine(AppContext.BaseDirectory, "clavieror.db");
		var legacyLocalAppDataDbPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"ClavierOr",
			"clavieror.db");

		MigrateIfNewer(legacyLocalAppDataDbPath, targetDbPath);
		MigrateIfNewer(legacyRuntimeDbPath, targetDbPath);

		using var db = new ClavierOrContext();
		db.Database.Migrate();
	}

	private static void MigrateIfNewer(string sourcePath, string targetPath)
	{
		if (string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		if (!File.Exists(sourcePath))
		{
			return;
		}

		if (!File.Exists(targetPath))
		{
			File.Copy(sourcePath, targetPath);
			return;
		}

		var sourceInfo = new FileInfo(sourcePath);
		var targetInfo = new FileInfo(targetPath);

		if (sourceInfo.LastWriteTimeUtc <= targetInfo.LastWriteTimeUtc)
		{
			return;
		}

		var backupPath = $"{targetPath}.backup-{DateTime.Now:yyyyMMddHHmmss}";
		File.Copy(targetPath, backupPath, overwrite: true);
		File.Copy(sourcePath, targetPath, overwrite: true);
	}
}

