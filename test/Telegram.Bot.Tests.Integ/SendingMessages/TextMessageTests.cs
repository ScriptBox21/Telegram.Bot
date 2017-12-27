using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Tests.Integ.Common;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace Telegram.Bot.Tests.Integ.SendingMessages
{
    [Collection(Constants.TestCollections.SendTextMessage)]
    [TestCaseOrderer(Constants.TestCaseOrderer, Constants.AssemblyName)]
    public class TextMessageTests : IClassFixture<TextMessageTestsFixture>
    {
        public ITelegramBotClient BotClient => _fixture.BotClient;

        private readonly TestsFixture _fixture;

        private readonly TextMessageTestsFixture _classFixture;

        public TextMessageTests(TextMessageTestsFixture classFixture)
        {
            _classFixture = classFixture;
            _fixture = classFixture.TestsFixture;
        }

        #region 1. Sending text message

        [Fact(DisplayName = FactTitles.ShouldSendTextMessage)]
        [Trait(Constants.MethodTraitName, Constants.TelegramBotApiMethods.SendMessage)]
        [ExecutionOrder(1.0)]
        public async Task Should_Send_Text_Message()
        {
            await _fixture.SendTestCaseNotificationAsync(FactTitles.ShouldSendTextMessage);

            const string text = "Hello world!";

            Message message = await BotClient.SendTextMessageAsync(
                chatId: _fixture.SuperGroupChatId,
                text: text
            );

            Assert.Equal(text, message.Text);
            Assert.Equal(MessageType.TextMessage, message.Type);
            Assert.Equal(_fixture.SuperGroupChatId.ToString(), message.Chat.Id.ToString());
            Assert.InRange(message.Date, DateTime.Now.AddSeconds(-5), DateTime.Now);
            Assert.True(JToken.DeepEquals(
                JToken.FromObject(_fixture.BotUser), JToken.FromObject(message.From)
            ));
        }

        [Fact(DisplayName = FactTitles.ShouldSendTextMessageToChannel)]
        [Trait(Constants.MethodTraitName, Constants.TelegramBotApiMethods.SendMessage)]
        [ExecutionOrder(1.1)]
        public async Task Should_Send_Text_Message_To_Channel()
        {
            await _fixture.SendTestCaseNotificationAsync(FactTitles.ShouldSendTextMessageToChannel);

            string text = $"Hello members of channel {_classFixture.ChannelChatId}";

            Message message = await BotClient.SendTextMessageAsync(
                chatId: _classFixture.ChannelChatId,
                text: text
            );

            Assert.Equal(text, message.Text);
            Assert.Equal(MessageType.TextMessage, message.Type);
            Assert.Equal(_classFixture.ChannelChat.Id, message.Chat.Id);
            Assert.Equal(_classFixture.ChannelChat.Username, message.Chat.Username);
        }

        #endregion

        #region 2. Parsing text message entities

        [Fact(DisplayName = FactTitles.ShouldParseMarkDownEntities)]
        [Trait(Constants.MethodTraitName, Constants.TelegramBotApiMethods.SendMessage)]
        [ExecutionOrder(2.1)]
        public async Task Should_Parse_MarkDown_Entities()
        {
            await _fixture.SendTestCaseNotificationAsync(FactTitles.ShouldParseMarkDownEntities);

            var entityValueMappings = new Dictionary<MessageEntityType, string>
            {
                { MessageEntityType.Bold, "*bold*" },
                { MessageEntityType.Italic, "_italic_" },
                { MessageEntityType.TextLink, "[inline url to Telegram.org](https://telegram.org/)" },
                {
                    MessageEntityType.TextMention,
                    $"[{_fixture.BotUser.Username}](tg://user?id={_fixture.BotUser.Id})"
                },
                { MessageEntityType.Code, @"inline ""`fixed-width code`""" },
                { MessageEntityType.Pre, "```pre-formatted fixed-width code block```" },
            };

            Message message = await BotClient.SendTextMessageAsync(
                chatId: _fixture.SuperGroupChatId,
                text: string.Join("\n", entityValueMappings.Values),
                parseMode: ParseMode.Markdown,
                disableWebPagePreview: true
            );

            Assert.Equal(entityValueMappings.Keys, message.Entities.Select(e => e.Type));
        }

        [Fact(DisplayName = FactTitles.ShouldParseHtmlEntities)]
        [Trait(Constants.MethodTraitName, Constants.TelegramBotApiMethods.SendMessage)]
        [ExecutionOrder(2.2)]
        public async Task Should_Parse_HTML_Entities()
        {
            await _fixture.SendTestCaseNotificationAsync(FactTitles.ShouldParseHtmlEntities);

            var entityValueMappings = new(MessageEntityType Type, string Value)[]
            {
                ( MessageEntityType.Bold, "<b>bold</b>" ),
                ( MessageEntityType.Bold, "<strong>&lt;strong&gt;</strong>" ),
                ( MessageEntityType.Italic, "<i>italic</i>" ),
                ( MessageEntityType.Italic, "<em>&lt;em&gt;</em>" ),
                ( MessageEntityType.TextLink, @"<a href=""https://telegram.org/"">inline url to Telegram.org</a>" ),
                (
                    MessageEntityType.TextMention,
                    $@"<a href=""tg://user?id={_fixture.BotUser.Id}"">{_fixture.BotUser.Username}</a>"
                ),
                ( MessageEntityType.Code, @"inline <code>""fixed-width code""</code>" ),
                ( MessageEntityType.Pre, "<pre>pre-formatted fixed-width code block</pre>" ),
            };

            Message message = await BotClient.SendTextMessageAsync(
                chatId: _fixture.SuperGroupChatId,
                text: string.Join("\n", entityValueMappings.Select(tuple => tuple.Value)),
                parseMode: ParseMode.Html,
                disableWebPagePreview: true
            );

            Assert.Equal(
                entityValueMappings.Select(tuple => tuple.Type),
                message.Entities.Select(e => e.Type)
            );
        }

        [Fact(DisplayName = FactTitles.ShouldPaseMessageEntitiesIntoValues)]
        [Trait(Constants.MethodTraitName, Constants.TelegramBotApiMethods.SendMessage)]
        [ExecutionOrder(2.3)]
        public async Task Should_Parse_Message_Entities_Into_Values()
        {
            await _fixture.SendTestCaseNotificationAsync(FactTitles.ShouldPaseMessageEntitiesIntoValues);

            var entityValueMappings = new(MessageEntityType Type, string Value)[]
            {
                ( MessageEntityType.Hashtag, "#TelegramBots" ),
                ( MessageEntityType.Mention, "@BotFather" ),
                ( MessageEntityType.Url, "http://github.com/TelegramBots" ),
                ( MessageEntityType.Email, "security@telegram.org" ),
                ( MessageEntityType.BotCommand, "/test" ),
                ( MessageEntityType.BotCommand, $"/test@{_fixture.BotUser.Username}" ),
            };

            Message message = await BotClient.SendTextMessageAsync(
                chatId: _fixture.SuperGroupChatId,
                text: string.Join("\n", entityValueMappings.Select(tuple => tuple.Value))
            );

            Assert.Equal(
                entityValueMappings.Select(t => t.Type),
                message.Entities.Select(e => e.Type)
            );
            Assert.Equal(entityValueMappings.Select(t => t.Value), message.EntityValues);
        }

        #endregion

        private static class FactTitles
        {
            public const string ShouldSendTextMessage = "Should send text message";

            public const string ShouldSendTextMessageToChannel = "Should send text message to channel";

            public const string ShouldParseMarkDownEntities = "Should send markdown formatted text message and parse its entities. Link preview should not appear.";

            public const string ShouldParseHtmlEntities = "Should send HTML formatted text message and parse its entities. Link preview should not appear.";

            public const string ShouldPaseMessageEntitiesIntoValues = "Should send text message and parse its entity values";
        }
    }
}
