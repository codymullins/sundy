using Mediator;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Handler for disconnecting a Microsoft Outlook account.
/// </summary>
public class DisconnectOutlookCommandHandler(
    IMicrosoftAccountManager accountManager,
    ICalendarStore calendarStore)
    : ICommandHandler<DisconnectOutlookCommand, DisconnectOutlookResult>
{
    public async ValueTask<DisconnectOutlookResult> Handle(DisconnectOutlookCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Remove the account from the account manager (handles MSAL sign out)
            await accountManager.RemoveAccountAsync(command.AccountId, cancellationToken);

            // Remove all calendars associated with this account
            var allCalendars = await calendarStore.GetAllAsync(cancellationToken);
            var accountCalendars = allCalendars.Where(c =>
                c.Type == CalendarType.Microsoft &&
                c.ExternalAccountId == command.AccountId);

            foreach (var calendar in accountCalendars)
            {
                await calendarStore.DeleteCalendarAsync(calendar.Id, cancellationToken);
            }

            return new DisconnectOutlookResult(true);
        }
        catch (Exception ex)
        {
            return new DisconnectOutlookResult(false, ex.Message);
        }
    }
}
