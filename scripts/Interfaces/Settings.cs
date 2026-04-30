using Godot;
using System;

public partial class Settings : CanvasLayer
{

	[Export] private CheckBox _fullScreenCheckBox;
	[Export] private HSlider _sonidoSlider;
	[Export] private HSlider _musicaSlider;

	private bool isFullScreen;
	private float _sonidoValue;
	private float _musicaValue;
	private const string SETTINGS_FILE_PATH = "res://configFile/settings.cfg";
	private ConfigFile _configFile = new ConfigFile();
	class SettingsData
	{
		public bool IsFullScreen { get; set; }
		public float SonidoVolume { get; set; }
		public float MusicaVolume { get; set; }
	}

	private SettingsData _settingsData = new SettingsData();
	public override void _Ready()
    {
		// Guardar los valores actuales después de cargar
		isFullScreen = _fullScreenCheckBox.ButtonPressed;
		_settingsData.IsFullScreen = isFullScreen;

		_sonidoValue = (float)_sonidoSlider.Value;
		_settingsData.SonidoVolume = _sonidoValue;

		_musicaValue = (float)_musicaSlider.Value;
		_settingsData.MusicaVolume = _musicaValue;
        loadSettings();
    }
	public void onVolverPressed()
	{
		_fullScreenCheckBox.ButtonPressed = isFullScreen;
		_sonidoSlider.Value = _sonidoValue;
		_musicaSlider.Value = _musicaValue;
		Visible = false;
	}

	public void onSonidoChanged(float value)
	{

		float db = Mathf.LinearToDb(value);
		_settingsData.SonidoVolume = value;
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Sonido"), db);
		_sonidoSlider.Value = value;
		
	}

	public void onMusicaChanged(float value)
	{
		float db = Mathf.LinearToDb(value);
		_settingsData.MusicaVolume = value;
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Musica"), db);
		 _musicaSlider.Value = value;
	}
	public void saveSettings()
    {
		// Guardar pantalla completa
		if(_settingsData.IsFullScreen != isFullScreen)
        {
			isFullScreen = _settingsData.IsFullScreen;
			_configFile.SetValue("Display", "FullScreen", isFullScreen);
			DisplayServer.WindowSetMode(isFullScreen ? DisplayServer.WindowMode.ExclusiveFullscreen : DisplayServer.WindowMode.Windowed);
        }
		
		// Guardar volumen de sonido
		if(_settingsData.SonidoVolume != _sonidoValue)
        {
			_sonidoValue = _settingsData.SonidoVolume;
			_configFile.SetValue("Sonido", "Volume", _sonidoValue);
        }

		// Guardar volumen de música
		if(_settingsData.MusicaVolume != _musicaValue)
        {
			_musicaValue = _settingsData.MusicaVolume;
			_configFile.SetValue("Musica", "Volume", _musicaValue);
        }

		// Guardar archivo de configuración
		Error err = _configFile.Save(SETTINGS_FILE_PATH);
		if (err != Error.Ok)
		{			
			GD.PrintErr("Error al guardar la configuración: " + err);
		}
    }

// Configurar el estado del checkbox de pantalla completa
	public void fullScreenActive(bool active)
	{
		_settingsData.IsFullScreen = active;
	}

	public void loadSettings()
	{
		Error err = _configFile.Load(SETTINGS_FILE_PATH);
		if (err != Error.Ok)
		{
			GD.Print("Archivo de configuración no encontrado o no válido. Se creará uno nuevo con valores por defecto.");
			InitializeDefaultSettings();
		}

		// Cargar pantalla completa
		isFullScreen = (bool)_configFile.GetValue("Display", "FullScreen", false);
		_settingsData.IsFullScreen = isFullScreen;
		_fullScreenCheckBox.ButtonPressed = isFullScreen;
		DisplayServer.WindowSetMode(isFullScreen ? DisplayServer.WindowMode.ExclusiveFullscreen : DisplayServer.WindowMode.Windowed);

		// Cargar volumen de sonido
		_sonidoValue = (float)_configFile.GetValue("Sonido", "Volume", 0.5f);
		_settingsData.SonidoVolume = _sonidoValue;
		_sonidoSlider.Value = _sonidoValue;
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Sonido"), Mathf.LinearToDb(_sonidoValue));

		// Cargar volumen de música
		_musicaValue = (float)_configFile.GetValue("Musica", "Volume", 0.5f);
		_settingsData.MusicaVolume = _musicaValue;
		_musicaSlider.Value = _musicaValue;
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Musica"), Mathf.LinearToDb(_musicaValue));
	}

	private void InitializeDefaultSettings()
	{
		_configFile.SetValue("Display", "FullScreen", false);
		_configFile.SetValue("Sonido", "Volume", 0.5f);
		_configFile.SetValue("Musica", "Volume", 0.5f);

		Error saveErr = _configFile.Save(SETTINGS_FILE_PATH);
		if (saveErr != Error.Ok)
		{
			GD.PrintErr("Error al crear el archivo de configuración por defecto: " + saveErr);
		}
	}

}
