using Godot;
using System;

public partial class MenuPrincipal : Control
{

	[Export] private CanvasLayer _settings;

	private Settings _settingsScript;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		// Obtener el script de Settings de la CanvasLayer exportada
		if (_settings != null)
		{
			_settingsScript = _settings as Settings;
			if (_settingsScript != null)
			{
				_settingsScript.loadSettings();
			}
			else
			{
				GD.PrintErr("No se pudo obtener el script Settings de la CanvasLayer");
			}
			_settings.Visible = false;
		}
		else
		{
			GD.PrintErr("_settings no está asignado en el inspector");
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void onStartPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/Interfaces/seleccion_personajes.tscn");
	}
	private void onSettingPressed()
    {
		_settings.Visible = true;
    }

	private void onExitPressed()
	{
		GetTree().Quit();
	}
}
