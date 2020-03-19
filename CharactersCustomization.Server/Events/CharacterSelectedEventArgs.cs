using Addemod.Characters.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Addemod.CharactersCustomization.Shared.Events {
	public class CharacterSelectedEventArgs : EventArgs {
		public Character Character;
		public CharacterSession CharacterSession;

		public CharacterSelectedEventArgs(Character character, CharacterSession characterSession) {
			this.Character = character;
			this.CharacterSession = characterSession;
		}
	}
}
