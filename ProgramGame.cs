using System;

namespace RpgVR
{
	internal class ProgramGame
	{
		internal static void Start()
		{
			OpenVRRoom.Enable();

			LoadingRoom.Show();

			NesFile.Load("Final Fantasy (U).nes");

			FinalFantasyGame.Load();

			LoadingRoom.Hide();

			GamePlayer.Start();

			GameRoom.Run();

			OpenVRRoom.Disable();
		}
	}
}