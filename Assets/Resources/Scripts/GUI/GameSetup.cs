using System.Collections.Generic;

/// The party chosen on the title screen. PlayerManager reads it when the dungeon starts;
/// it stays null when MainScene is opened directly, and the scene's own settings apply.
public static class GameSetup
{
	public static List<playerClass> classes;
}
