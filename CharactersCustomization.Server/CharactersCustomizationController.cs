using JetBrains.Annotations;
using NFive.SDK.Core.Diagnostics;
using NFive.SDK.Server.Communications;
using NFive.SDK.Server.Controllers;
using Addemod.CharactersCustomization.Shared;
using Addemod.Characters.Server;
using Addemod.Characters.Shared;
using Addemod.Characters.Server.Events;
using System.Linq;
using NFive.SDK.Core.Models.Player;

namespace Addemod.CharactersCustomization.Server
{
	[PublicAPI]
	public class CharactersCustomizationController : ConfigurableController<CharactersCustomizationConfiguration>
	{
		private ICommunicationManager comms;
		private CharactersManager charactersManager;

		private readonly IClientList clientList;

		public CharactersCustomizationController(ILogger logger, CharactersCustomizationConfiguration configuration, ICommunicationManager comms, IClientList clientList, CharactersManager charactersManager) : base(logger, configuration)
		{
			this.comms = comms;
			this.clientList = clientList;
			this.charactersManager = charactersManager;

			// Send configuration when requested
			this.comms.Event(CharacterCustomizationEvents.Configuration).FromClients().OnRequest(e => e.Reply(this.Configuration));

			charactersManager.Selected += OnCharacterSelected;
		}

		private void OnCharacterSelected(object sender, CharacterSessionEventArgs e) {
			Logger.Debug("OnCharacterSelected: " + e.CharacterSession);
			var client = GetClientFromSession(e.CharacterSession.Session);
			Logger.Debug("Client: " + client.Name);
			this.comms.Event(CharacterEvents.Selected).ToClient(client).Emit(e.CharacterSession);
		}

		private IClient GetClientFromSession(Session session) {
			return this.clientList.Clients.FirstOrDefault(c => c.Handle == session.Handle);
		}
	}
}
