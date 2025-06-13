using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramForwardly.WebApi.Models.Dtos;
using TelegramForwardly.WebApi.Services.Interfaces;

namespace TelegramForwardly.WebApi.Services.Bot.Managers
{
    public static class MenuManager
    {
        public static async Task ShowMainMenuAsync(
            BotUser user,
            long chatId,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await BotHelper.SendTextMessageAsync(
                chatId, BotHelper.GetMenuText(user),
                botClient, logger,
                cancellationToken,
                BotHelper.GetMenuKeyboard(user.ForwardlyEnabled!.Value));
        }

        public static async Task EnterSetupAsync(
            BotUser user,
            long chatId,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if ((bool)user.IsAuthenticated!)
            {
                await BotHelper.SendTextMessageAsync(chatId,
                    "✔️ You're already authenticated! Use /menu\n\n" +
                    "If you want to *remove your data from this bot* " +
                    "including sensitive account information and API credentials:\n" +
                    " - use _*/delete*_\n",
                    botClient, logger,
                    cancellationToken);
                return;
            }

            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingPhoneNumber);

            await BotHelper.SendTextMessageAsync(
                chatId,
                BotHelper.GetSetupMessage(),
                botClient, logger,
                cancellationToken,
                BotHelper.GetPhoneKeyboard());
        }


        public static async Task EnterSettingsAsync(
            bool editSourceMessage,
            Message message,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var keyboard = new InlineKeyboardMarkup(
            [
                [InlineKeyboardButton.WithCallbackData("👥 Set group for forwarding", "set_forum")],
                [InlineKeyboardButton.WithCallbackData("🗂️ Set grouping mode", "set_grouping")],
                [InlineKeyboardButton.WithCallbackData("🏠 Back to Menu", "back_to_menu")]
            ]);
            if (editSourceMessage)
            {
                await botClient.EditMessageText(
                    message.Chat.Id, message.MessageId, BotHelper.DefaultEscapeMarkdownV2("Choose an option:"),
                    replyMarkup: keyboard, cancellationToken: cancellationToken);
            }
            else
            {
                await BotHelper.SendTextMessageAsync(
                    message.Chat.Id, "Choose an option:", botClient, logger, 
                    replyMarkup: keyboard, cancellationToken: cancellationToken);
            }
        }

        public static async Task AskToDeleteData(
            BotUser user,
            long chatId,
            IUserService userService,
            ITelegramBotClient botClient,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await userService.SetUserStateAsync(user.TelegramUserId, UserState.AwaitingDeleteConfirmation);

            await BotHelper.SendTextMessageAsync(
                chatId,
                "⚠️ Are you sure you want to remove all your data from this bot?\n" +
                "This deletion includes not only secret API credentials but also record of keywords and other settings and preferences.\n" +
                "If you proceed, you will need to set up your account again to be able to use this bot again.\n\n" +
                "Please confirm by sending _'yes, I want this bot not to persist my data'_ or *_'no'_ to cancel*.",
                botClient, logger,
                cancellationToken);
        }
    }
}
