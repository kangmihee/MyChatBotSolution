using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ChatBotApplication.Dialogs
{
    [Serializable]
    public class KaKaoPlusBotDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as IMessageActivity;
            await context.PostAsync($"카카오톡 회원님~ 안녕하세요?");
            context.Wait(SecondMessageReceivedAsync);
        }

        private async Task SecondMessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as IMessageActivity;
            await context.PostAsync($"두번쨰 입력메시지는 {activity.Text}입니다.");
            //context.Wait(ThirdMessageReceivedAsync);
        }
    }
}