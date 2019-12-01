using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ChatBotApplication.Dialogs
{
    [Serializable]
    public class MembershipDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.PostAsync($"신규 회원 가입 프로세스를 진행합니다.\n 성함을 입력해주세요.");
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as IMessageActivity;
            await context.PostAsync($"전화번호를 입력해주세요.");
            context.Wait(MessageGenderAsync);
        }

        private async Task MessageGenderAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as IMessageActivity;
            await context.PostAsync($"성별을 입력해주세요. [남] 또는 [여]");
            context.Wait(EntryNewMember);
        }

        private async Task EntryNewMember(IDialogContext context, IAwaitable<object> result)
        {
            //신규 회원 가입로직 처리 
            await context.PostAsync($"신규 회원 가입이 완료되었습니다.");

            context.Done(true);
        }

    }
}