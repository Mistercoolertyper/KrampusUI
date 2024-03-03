class Globals
{
	public static Settings Settings = new Settings(true, true, false);
}

class Settings
{
	public Settings(bool topMost, bool autoAttach, bool autoLaunch)
	{
		TopMost = topMost;
		AutoAttach = autoAttach;
		AutoLaunch = autoLaunch;
	}

	public bool TopMost { get; set; }
	public bool AutoAttach { get; set; }
	public bool AutoLaunch { get; set; }
}
