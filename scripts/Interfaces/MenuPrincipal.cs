using Godot;
using System;

public partial class MenuPrincipal : Control
{

	[Export] private CanvasLayer _settings;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        if (_settings != null)
            _settings = GetNode<CanvasLayer>("Settings");
        _settings.Visible = false;
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
