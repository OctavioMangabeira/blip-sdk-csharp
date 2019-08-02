using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Client.Extensions.HelpDesk;

namespace Take.Blip.Builder.Actions.CreateTicket
{
    public class CreateTicketAction : ActionBase<CreateTicketSettings>
    {
        private readonly IHelpDeskExtension _helpDeskExtension;

        public CreateTicketAction(IHelpDeskExtension helpDeskExtension) 
            : base(nameof(CreateTicket))
        {
            _helpDeskExtension = helpDeskExtension;
        }

        public override async Task ExecuteAsync(IContext context, CreateTicketSettings settings, CancellationToken cancellationToken)
        {
            if (settings.CustomerIdentity == null)
            {
                settings.CustomerIdentity = context.UserIdentity;
            }

            var ticket = await _helpDeskExtension.CreateTicketAsync(settings, cancellationToken);
            context.SetTicket(ticket);
        }
    }
}