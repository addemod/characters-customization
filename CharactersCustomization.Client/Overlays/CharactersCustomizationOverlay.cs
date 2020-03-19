using Addemod.Characters.Client.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using NFive.SDK.Client.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Addemod.CharactersCustomization.Client.Overlays {
	public class CharactersCustomizationOverlay : Overlay {

		private Character actualCharacter;
		private Character customizedCharacter;
		private int camera;
		private Vector3 originalCamPos; // The original camera is positioned towards the face
		private Vector3 originalCamPointCoord; // The original position that the camera is pointed towards
		private Vector3 currentCamPos;
		private Vector3 pedPos;

		public event EventHandler<OverlayEventArgs> OverlayOpenEvent;
		public event EventHandler<SaveCharacterEventArgs> SaveCharacterEvent;

		public CharactersCustomizationOverlay(Character character, int camera, IOverlayManager overlayManager) : base(overlayManager) {
			this.actualCharacter = character;
			this.customizedCharacter = character;
			this.camera = camera;

			On<int>("setMotherHeritage", SetMotherHeritage);
			On<int>("setFatherHeritage", SetFatherHeritage);
			On<float>("setHeritageResemblance", SetHeritageResemblance);
			On<float>("setHeritageSkinTone", SetHeritageSkinTone);
			On<int>("rotateCharacter", RotateCharacter);
			On<CameraPositionEnum>("setCameraPosition", SetCameraPosition);
			On<string>("moveCamera", MoveCamera);
			On("saveCharacter", SaveCharacter);

			this.originalCamPos = API.GetCamCoord(this.camera);
			this.originalCamPointCoord = API.GetPedBoneCoords(Game.PlayerPed.Handle, (int)Bone.SKEL_Head, 0, 0, 0);
			this.currentCamPos = this.originalCamPos;
			this.pedPos = API.GetEntityCoords(Game.PlayerPed.Handle, true);
		}

		private void SaveCharacter() {
			this.SaveCharacterEvent?.Invoke(this, new SaveCharacterEventArgs(this.customizedCharacter, this));
			//this.Blur();
		}

		private void MoveCamera(string key) {
			switch(key) {
				case "A":
					// Move X towards negative
					this.currentCamPos.X -= 0.1f;
					break;
				case "D":
					// Move X towards positive
					this.currentCamPos.X += 0.1f;
					break;
				case "S":
					// Move Y towards negative
					this.currentCamPos.Y -= 0.1f;
					break;
				case "W":
					// Move Y towards positive
					this.currentCamPos.Y += 0.1f;
					break;
				case "UP":
					// Move Z towards positive
					this.currentCamPos.Z += 0.1f;
					break;
				case "DOWN":
					// Move Z towards negative
					this.currentCamPos.Z -= 0.1f;
					break;
				case "SPACE":
					this.Emit("LOG", this.currentCamPos.ToString());
					break;
			}
			// Point towards the ped
			API.SetCamCoord(this.camera, this.currentCamPos.X, this.currentCamPos.Y, this.currentCamPos.Z);
		}

		/** Vector3
		 * X - horizontal axis
		 * Y - vertical axis
		 * Z - height axis
		 */
		private void SetCameraPosition(CameraPositionEnum position) {
			switch (position) {
				case CameraPositionEnum.FACE:
					this.currentCamPos = this.originalCamPos;
					//this.currentCamPos.X -= 1f;
					//this.currentCamPos.Y += 1.5f;
					//this.currentCamPos.Z += 0.5f;
					API.PointCamAtCoord(this.camera, this.originalCamPointCoord.X, this.originalCamPointCoord.Y, this.originalCamPointCoord.Z);
					break;

				case CameraPositionEnum.TORSO:
					break;

				case CameraPositionEnum.LEGS:
					break;

				case CameraPositionEnum.SHOES:
					break;

				case CameraPositionEnum.FULLBODY:
					this.currentCamPos = this.originalCamPos;
					this.currentCamPos.X += 1f;
					this.currentCamPos.Y -= 1f;
					this.currentCamPos.Z -= 1f;
					API.PointCamAtCoord(this.camera, this.pedPos.X, this.pedPos.Y, this.pedPos.Z+0.5f); // Point at dick
					break;

				default: break;
			}
			API.SetCamCoord(this.camera, this.currentCamPos.X, this.currentCamPos.Y, this.currentCamPos.Z);
		}

		protected override dynamic Ready() {
			this.OverlayOpenEvent?.Invoke(this, new OverlayEventArgs(this));
			return this.customizedCharacter;
		}

		private void RotateCharacter(int heading) {
			var handle = Game.PlayerPed.Handle;
			API.SetEntityHeading(handle, API.GetEntityHeading(handle) + heading);
		}

		private void SetMotherHeritage(int parent1) {
			customizedCharacter.Heritage.Parent1 = parent1;
			UpdateHeritage();
		}

		private void SetFatherHeritage(int parent2) {
			customizedCharacter.Heritage.Parent2 = parent2;
			UpdateHeritage();
			//API.SetPedHeadBlendData(Game.PlayerPed.Handle, customizedCharacter.Heritage.Parent1, parent2, 0, customizedCharacter.Heritage.Parent1, parent2, 0, customizedCharacter.Heritage.Resemblance, customizedCharacter.Heritage.SkinTone, 0, false);
		}

		private void SetHeritageResemblance(float resemblance) {
			customizedCharacter.Heritage.Resemblance = resemblance;
			UpdateHeritage();
			//API.SetPedHeadBlendData(Game.PlayerPed.Handle, customizedCharacter.Heritage.Parent1, parent2, 0, customizedCharacter.Heritage.Parent1, parent2, 0, customizedCharacter.Heritage.Resemblance, customizedCharacter.Heritage.SkinTone, 0, false);
		}

		private void SetHeritageSkinTone(float skinTone) {
			customizedCharacter.Heritage.SkinTone = skinTone;
			UpdateHeritage();
		}

		private void UpdateHeritage() {
			var heritage = customizedCharacter.Heritage;
			API.SetPedHeadBlendData(Game.PlayerPed.Handle, heritage.Parent1, heritage.Parent2, 0, heritage.Parent1, heritage.Parent2, 0, heritage.Resemblance, heritage.SkinTone, 0, false);
		}

		public class SaveCharacterEventArgs : OverlayEventArgs {

			public Character Character;

			public SaveCharacterEventArgs(Character character, Overlay overlay) : base(overlay) {
				this.Character = character;
			}
		}
	}
}
