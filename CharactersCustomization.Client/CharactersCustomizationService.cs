using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NFive.SDK.Client.Commands;
using NFive.SDK.Client.Communications;
using NFive.SDK.Client.Events;
using NFive.SDK.Client.Interface;
using NFive.SDK.Client.Services;
using NFive.SDK.Core.Diagnostics;
using NFive.SDK.Core.Models.Player;
using Addemod.CharactersCustomization.Shared;
using Addemod.Characters.Client.Models;
using Addemod.Characters.Shared;
using CitizenFX.Core;
using Addemod.CharactersCustomization.Client.Overlays;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using NFive.SDK.Client.Extensions;
using NFive.SDK.Client.Input;
using NFive.SDK.Core.Input;
using Addemod.Characters.Client;

namespace Addemod.CharactersCustomization.Client
{
	[PublicAPI]
	public class CharactersCustomizationService : Service
	{
		private CharactersCustomizationConfiguration config;

		private CharacterSession characterSession;
		private Character currentCharacter {
			get {
				return this.characterSession.Character;
			}

			set {
				this.characterSession.Character = currentCharacter;
			}
		}

		private int customizeCharacterCamera;

		public CharactersCustomizationService(ILogger logger, ITickManager ticks, ICommunicationManager comms, ICommandManager commands, IOverlayManager overlay, User user) : base(logger, ticks, comms, commands, overlay, user) { }

		public override async Task Started()
		{
			// Request server configuration
			this.config = await Comms.Event(CharacterCustomizationEvents.Configuration).ToServer().Request<CharactersCustomizationConfiguration>();

			//Comms.Event(CharacterEvents.Selected).FromServer().On<CharacterSession>(OnCharacterSelected);
		}

		private async Task InvalidateIdleCam() {
			await Delay(1000);
			API.InvalidateIdleCam();
		}

		private async void OnCharacterSelected(ICommunicationMessage e, CharacterSession _characterSession) {
			// Should we have the menu on the left or right hand side?
			//MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
			this.characterSession = _characterSession;

			// Wait until player switch is done
			while (API.IsPlayerSwitchInProgress()) await Delay(300);

			// Hide all other players
			foreach(var player in Players) {
				API.SetEntityLocallyInvisible(player.Handle);
			}

			// Hide the player's ped for everyone
			API.SetEntityInvincible(Game.PlayerPed.Handle, true);

			// Set player unable to move
			API.DisableAllControlActions(0);
			API.SetEntityLocallyVisible(Game.PlayerPed.Handle);

			// Create the camera
			this.customizeCharacterCamera = API.CreateCam("DEFAULT_SCRIPTED_CAMERA", true);

			// Activate and render
			API.SetCamActive(this.customizeCharacterCamera, true);
			API.RenderScriptCams(true, false, 0, true, true);

			// Get ped pos, head pos and the calculated camera pos
			var pedPos = API.GetEntityCoords(Game.PlayerPed.Handle, true);
			var headPos = API.GetPedBoneCoords(Game.PlayerPed.Handle, (int)Bone.SKEL_Head, 0, 0, 0);
			var camPos = new Vector3(headPos.X + 0.5f, headPos.Y - 1.1f, headPos.Z + 0.5f);

			// Set camera position and what to point at
			API.SetCamCoord(this.customizeCharacterCamera, camPos.X, camPos.Y, camPos.Z);
			API.PointCamAtCoord(this.customizeCharacterCamera, headPos.X, headPos.Y, headPos.Z);

			// Set player facing camera
			API.SetEntityHeading(Game.PlayerPed.Handle, API.GetHeadingFromVector_2d(camPos.X - pedPos.X, camPos.Y - pedPos.Y));

			var originalCamViewMode = API.GetFollowPedCamViewMode();
			API.SetFollowPedCamViewMode(0);

			// Hide HUD
			Screen.Hud.IsVisible = false;

			// Disable the loading screen from automatically being dismissed
			API.SetManualShutdownLoadingScreenNui(true);

			// Hide loading screen
			API.ShutdownLoadingScreen();

			// Show the overlay
			var overlay = new CharactersCustomizationOverlay(this.currentCharacter, this.customizeCharacterCamera, this.OverlayManager);

			overlay.OverlayOpenEvent += (sender, _e) => {
				this.Ticks.On(InvalidateIdleCam);
			};

			overlay.SaveCharacterEvent += (sender, _e) => {
				// Update the character
				//this.currentCharacter.Apparel = Character.GetApparelFromPed(Game.PlayerPed, this.currentCharacter.ApparelId);
				//this.currentCharacter.Appearance = Character.GetAppearanceFromPed(Game.PlayerPed, this.currentCharacter.AppearanceId);
				//this.currentCharacter.FaceShape = Character.GetFaceShapeFromPed(Game.PlayerPed, this.currentCharacter.FaceShapeId);
				//this.currentCharacter.Heritage = Character.GetHeritageFromPed(Game.PlayerPed, this.currentCharacter.HeritageId);
				this.currentCharacter = _e.Character;

				// Save the character
				this.Comms.Event(CharacterEvents.SaveCharacter).ToServer().Emit(this.currentCharacter);

				// Show all players again
				foreach (var player in Players) {
					API.SetEntityLocallyVisible(player.Handle);
				}

				// Show the player's ped for everyone again
				API.SetEntityInvincible(Game.PlayerPed.Handle, false);

				// Set player able to move again
				API.EnableAllControlActions(0);

				// Close overlay
				overlay.Dispose();

				// Show HUD
				Screen.Hud.IsVisible = true;

				// Unset camera
				API.SetCamActive(this.customizeCharacterCamera, false);
				API.RenderScriptCams(false, false, 0, true, true);
				API.SetFollowPedCamViewMode(originalCamViewMode);

				// Dunno
				API.SetManualShutdownLoadingScreenNui(false);

				this.Ticks.Off(InvalidateIdleCam);
			};
			overlay.Focus(true);

			// Shut down the NUI loading screen
			API.ShutdownLoadingScreenNui();

			//API.FreezeEntityPosition(Game.Player.Handle, false);

			// Not working?
			//MenuController.EnableMenuToggleKeyOnController = false;
			//MenuController.MenuToggleKey = (Control)(-1);

			// Create the menu
			/*Menu characterCustomizationMenu = new Menu("Testing", "Customize your character");
			MenuController.AddMenu(characterCustomizationMenu);

			#region Sub menus
			var hairSubMenu = new Menu("Hair", "Choose a fresh cut!");
			var faceSubMenu = new Menu("Face", "Customize your face appearance");
			var iconsSubMenu = new Menu("Icons", "Preview all icons here");
			#endregion

			#region Create buttons for submenu
			var hairButton = new MenuItem(hairSubMenu.MenuTitle, hairSubMenu.MenuSubtitle) {
				LeftIcon = MenuItem.Icon.BARBER
			};
			characterCustomizationMenu.AddMenuItem(hairButton);

			var faceButton = new MenuItem(faceSubMenu.MenuTitle, faceSubMenu.MenuSubtitle) {
				LeftIcon = MenuItem.Icon.MASK
			};
			characterCustomizationMenu.AddMenuItem(faceButton);

			var iconsButton = new MenuItem("Icons") {
				LeftIcon = MenuItem.Icon.INV_QUESTIONMARK
			};
			characterCustomizationMenu.AddMenuItem(iconsButton);


			MenuController.BindMenuItem(characterCustomizationMenu, hairSubMenu, hairButton);
			MenuController.BindMenuItem(characterCustomizationMenu, faceSubMenu, faceButton);
			MenuController.BindMenuItem(characterCustomizationMenu, iconsSubMenu, iconsButton);
			#endregion

			foreach (int value in Enum.GetValues(typeof(MenuItem.Icon))) {
				string name = Enum.GetName(typeof(MenuItem.Icon), value);
				iconsSubMenu.AddMenuItem(new MenuItem(name) {
					LeftIcon = (MenuItem.Icon)value
				});
			}

			characterCustomizationMenu.OnMenuOpen += (_menu) => {
				// Code in here gets triggered whenever the menu is opened.
				Logger.Debug($"OnMenuOpen: [{_menu}]");
			};

			characterCustomizationMenu.OpenMenu();*/
		}
	}
}
