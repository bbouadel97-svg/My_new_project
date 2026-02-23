using System.Diagnostics;

var solutionRoot = TrouverDossierSolution();
var clavierOrProjectPath = Path.Combine(solutionRoot, "MyNewProject", "ClavierOr", "ClavierOr.csproj");
var clavierOrExePath = Path.Combine(solutionRoot, "MyNewProject", "ClavierOr", "bin", "Debug", "net10.0-windows", "ClavierOr.exe");

if (!File.Exists(clavierOrProjectPath))
{
	Console.WriteLine("Impossible de trouver le projet ClavierOr.");
	Console.WriteLine($"Chemin recherché: {clavierOrProjectPath}");
	return;
}

if (File.Exists(clavierOrExePath))
{
	var process = Process.Start(new ProcessStartInfo
	{
		FileName = clavierOrExePath,
		UseShellExecute = true
	});

	Console.WriteLine("Interface ClavierOr lancée (exe).\n");
	process?.WaitForExit();
	return;
}

var dotnetProcess = Process.Start(new ProcessStartInfo
{
	FileName = "dotnet",
	Arguments = $"run --project \"{clavierOrProjectPath}\"",
	UseShellExecute = false
});

Console.WriteLine("Interface ClavierOr lancée via dotnet run.\n");
dotnetProcess?.WaitForExit();

return;

static string TrouverDossierSolution()
{
	var current = new DirectoryInfo(AppContext.BaseDirectory);

	while (current is not null)
	{
		var solutionFile = Path.Combine(current.FullName, "MyNewProject.sln");
		if (File.Exists(solutionFile))
		{
			return current.FullName;
		}

		current = current.Parent;
	}

	return Directory.GetCurrentDirectory();
}

