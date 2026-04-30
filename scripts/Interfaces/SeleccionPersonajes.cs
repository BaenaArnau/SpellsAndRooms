using Godot;
using System;
using SpellsAndRooms.scripts.map;

public partial class SeleccionPersonajes : CanvasLayer
{

	[Export] private TextureButton _caballeroButton;
	[Export] private TextureButton _magoButton;
	[Export] private PopupPanel _popupPanel;
	private Map _map;
	private string _selectedCharacter = "";
	public static bool _magoIsUnlocked = false;

	private const string SETTINGS_FILE_PATH = "res://configFile/unclok.cfg";
	private ConfigFile _configFile = new ConfigFile();

	
	public override void _Ready()
	{
		// Cargar el estado de desbloqueo del Mago desde el archivo de configuración
		Error err = _configFile.Load(SETTINGS_FILE_PATH);
		if (err != Error.Ok)
		{
			GD.PrintErr("Error al cargar la configuración: " + err);
		}

		_magoIsUnlocked = (bool)_configFile.GetValue("Unlocks", "Mago", false);
		_magoButton.Disabled = !_magoIsUnlocked;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void onCaballeroSelect()
	{
		GD.Print("Caballero seleccionado");
		// Desactivar el botón de Mago
		_magoButton.ButtonPressed = false;
		_selectedCharacter = "res://scenes/Characters/Player/Oathbreakers.tscn";
	}

	public void onMagoSelect()
	{
		GD.Print("Mago seleccionado");
		// Desactivar el botón de Caballero
		_caballeroButton.ButtonPressed = false;
		_selectedCharacter = "res://scenes/Characters/Player/Wizard.tscn";
	}

	public void onVolverPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/Interfaces/menu_principal.tscn");
	}

	public void onSelecionarPressed()
	{
		Map.SetSelectedPlayerScenePath(_selectedCharacter);
		GetTree().ChangeSceneToFile("res://scenes/Map/map.tscn");
	}
	
	public static void UnlockMago(bool unlock)
	{
		_magoIsUnlocked = unlock;
	}

	public void onMagoFocusEntered()
    {
		// Solo mostrar el popup si el Mago está bloqueado
		if (!_magoIsUnlocked)
		{
			_popupPanel.Position = (Vector2I)(GetViewport().GetMousePosition() + new Vector2(10, 10));
			_popupPanel.Popup();
		}
    }

	public void onMagoFocusExited()
	{
		// Ocultar el popup cuando sale el foco
		_popupPanel.Hide();
	}
}
