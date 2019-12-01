using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using ChatBotApplication.Extensions;
using ChatBotApplication.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;

using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;

namespace ChatBotApplication.Dialogs
{
    [Serializable]
    public class BeautySalonRootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }



        /// <summary>
        /// Step1. 회원 여부 묻기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"차홍아르더 도산점 회원이시면 [예]를 그렇지 않으면 [아니오] 를  입력해주세요.");

            //2.다음단계 응답처리 함수 설정 및 수신대기
            context.Wait(HelpReplyReceivedAsync);
        }



        /// <summary>
        /// Step2. 회원여부 사용자 답변 분석하기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task HelpReplyReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            //1.채널로부터 전달된 Activity 파라메터 수신
            var activity = await result as Activity;

            //2.대화 로직처리하기
            if (activity.Text.ToLower().Equals("yes") == true || activity.Text.ToLower().Equals("y") || activity.Text.Equals("예"))
            {
                //2.1 서비스 유형 선택 요청
                await this.ConfirmServiceTypeMessageAsync(context);
            }
            else if (activity.Text.ToLower().Equals("no") == true || activity.Text.Equals("아니오") == true )
            {
                //2.2 신규회원가입 다이얼로그 전환
                context.Call(new MembershipDialog(), ReturnRootDialogAsync);
            }
            else
            {
                //다시 이전 질문하기
                //await this.MessageReceivedAsync(context, null);

                //자연어 처리 LUIS APP호출 사용자 의도 파악하기
                await this.GetLUISIntentAsync(context, result); 
            }
        }









        /// <summary>
        /// 3.서비스 유형 선택
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ConfirmServiceTypeMessageAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();

            var options = new[]
            {
                "예약하기",
                "개인정보변경하기",
                "차홍아르더 매장 둘러보기"
            };

            reply.AddHeroCard("서비스선택", "아래 원하시는 서비스유형을 선택해주세요.", 
                options, new[] { "http://www.chahongardor.com/wp-content/uploads/2019/02/d12.jpg" });
            await context.PostAsync(reply);

            context.Wait(this.OnServiceTypeSelected);
        }


        /// <summary>
        /// MembershipDialog에서 루트 다이얼로그로 돌아옴
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task ReturnRootDialogAsync(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"신규 회원 가입 완료 후 서비스 메뉴로 이동합니다.");

            //서비스 메뉴 이동
            await this.ConfirmServiceTypeMessageAsync(context);
        }


        //예약정보 멤버변수 선언
        private ReservationModel memberReservation = null;

        /// <summary>
        /// 3.1 서비스 유형 선택 결과 처리
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task OnServiceTypeSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text == "예약하기" || message.Text == "1")
            {
                memberReservation = new ReservationModel();
                await this.DesignerListMessageAsync(context, result);
            }
            else if (message.Text == "개인정보변경하기" || message.Text == "2")
            {
                await context.PostAsync($"사용자 인증을 진행합니다.");
            }
            else if (message.Text == "차홍아르더 매장 둘러보기" || message.Text == "3")
            {
                await this.WelcomeVideoMessageAsync(context, result);
            }
            else
            {
                await this.StartOverAsync(context, "죄송합니다. 요청사항을 이해하지 못했습니다.^^ ");
            }
        }









        /// <summary>
        /// 헤어디자이너 캐로셀 목록 메시지 발송
        /// </summary>
        /// <param name="context"></param>
        /// <param name="beforeActivity"></param>
        /// <returns></returns>
        private async Task DesignerListMessageAsync(IDialogContext context, IAwaitable<object> beforeActivity)
        {
            var activity = await beforeActivity as Activity;

            var carouselCards = new List<HeroCard>();
            carouselCards.Add(new HeroCard
            {
                Title = "1.차홍 대표원장",
                Images = new List<CardImage> { new CardImage("http://www.chahongardor.com/wp-content/uploads/2019/02/180710-profile_ED-1278.jpg", "차홍") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "선택하기", value: "1.차홍") }
            });

            carouselCards.Add(new HeroCard
            {
                Title = "2.고은 원장",
                Images = new List<CardImage> { new CardImage("http://www.chahongardor.com/wp-content/uploads/2019/02/180718-profile_ED-1335.jpg", "고은") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "선택하기", value: "2.고은") }
            });

            carouselCards.Add(new HeroCard
            {
                Title = "3.한정은 부원장",
                Images = new List<CardImage> { new CardImage("http://www.chahongardor.com/wp-content/uploads/2019/02/180718-profile_ED-1277.jpg", "한정은") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "선택하기", value: "3.한정은") }
            });

            carouselCards.Add(new HeroCard
            {
                Title = "4.주란 수석실장",
                Images = new List<CardImage> { new CardImage("http://www.chahongardor.com/wp-content/uploads/2019/02/190424-profile_CH-1474_%EC%A3%BC%EB%9E%80.jpg", "주란") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "선택하기", value: "4.주란") }
            });
            carouselCards.Add(new HeroCard
            {
                Title = "5.백민경 실장",
                Images = new List<CardImage> { new CardImage("http://www.chahongardor.com/wp-content/uploads/2019/02/180718-profile_ED-1044.jpg", "백민경") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "선택하기", value: "5. 백민경") }
            });

            var carousel = new PagedCarouselCards
            {
                Cards = carouselCards,
                TotalCount = 5
            };
        
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = new List<Attachment>();

            foreach (HeroCard productCard in carousel.Cards)
            {
                reply.Attachments.Add(productCard.ToAttachment());
            }

            await context.PostAsync(reply);

            //사용자의 디자이너 선택정보 처리
            context.Wait(this.OnDesignerItemSelected);
        }



        /// <summary>
        /// 헤어디자이너 선택 처리
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task OnDesignerItemSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            //사용자 선택 디자이너 예약정보 저장
            memberReservation.DesignerName = message.Text;

            if (message.Text == "1.차홍" || message.Text == "차홍")
            {
                await context.PostAsync($"차홍 대표원장을 선택하셨습니다.");
                await this.ReservationMenuListMessageAsync(context, result);
            }
            else if (message.Text == "2.고은" || message.Text == "고은")
            {
                await context.PostAsync($"고은 원장을 선택하셨습니다.");
                await this.ReservationMenuListMessageAsync(context, result);
            }
            else if (message.Text == "3.한정은" || message.Text == "한정은")
            {
                await context.PostAsync($"한정은 부원장을 선택하셨습니다.");
                await this.ReservationMenuListMessageAsync(context, result);
            }
            else if (message.Text == "4.주란" || message.Text == "주란")
            {
                await context.PostAsync($"주란 수석실장을 선택하셨습니다.");
                await this.ReservationMenuListMessageAsync(context, result);
            }
            else if (message.Text == "5.백민경" || message.Text == "백민경")
            {
                await context.PostAsync($"백민경 실장을 선택하셨습니다.");
                await this.ReservationMenuListMessageAsync(context, result);
            }
            else
            {
                await this.StartOverAsync(context, "죄송합니다. 요청사항을 이해하지 못했습니다. 디자이너의 이름을 정확히 입력해 주세요.");
            }
        }


        /// <summary>
        /// 예외 메시지 처리 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task StartOverAsync(IDialogContext context, string text)
        {
            var message = context.MakeMessage();
            message.Text = text;
            await this.StartOverAsync(context, message);
        }

        //예외 메시지 처리
        private async Task StartOverAsync(IDialogContext context, IMessageActivity message)
        {
            await context.PostAsync(message);
            await this.ConfirmServiceTypeMessageAsync(context);
        }



        /// <summary>
        /// 메뉴 선택하기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="beforeActivity"></param>
        /// <returns></returns>
        private async Task ReservationMenuListMessageAsync(IDialogContext context, IAwaitable<object> beforeActivity)
        {
            var activity = await beforeActivity as Activity;
            var reply = context.MakeMessage();

            var options = new[]
            {
                "커트",
                "부분펌",
                "숏펌",
                "단발펌",
                "미디움펌",
                "롱펌",
                "염색",
                "드라이",
                "크리닉",
                "기타"
            };

            reply.AddHeroCard(activity.Text, "아래 원하시는 스타일을 선택해주세요.", options
                , new[] { "https://www.chahongardor.com/wp-content/uploads/2019/02/d05.jpg" });
            await context.PostAsync(reply);

            context.Wait(this.ReservationDateListMessageAsync);
        }




        /// <summary>
        /// 예약날자 선택하기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="beforeActivity"></param>
        /// <returns></returns>
        private async Task ReservationDateListMessageAsync(IDialogContext context, IAwaitable<object> beforeActivity)
        {
            var activity = await beforeActivity as Activity;
            
            //사용자 예약 헤어 스타일 정보 저장
            memberReservation.Menu = activity.Text;

            var reply = context.MakeMessage();

            var options = new[]
            {
                "11월 04일 월요일",
                "11월 05일 화요일",
                "11월 06일 수요일",
                "11월 07일 목요일",
                "11월 08일 금요일",
                "11월 09일 토요일",
            };

            reply.AddHeroCard(activity.Text, "아래 원하시는 에약날짜을 선택해주세요.", options
                , new[] { "https://t1.daumcdn.net/cfile/tistory/9903C8335BDB06081A" });
            await context.PostAsync(reply);

            context.Wait(this.ReservationListMessageAsync);
        }




        /// <summary>
        /// 선택 디자이너 예약 가능 시간 알림
        /// </summary>
        /// <param name="context"></param>
        /// <param name="beforeActivity"></param>
        /// <returns></returns>
        private async Task ReservationListMessageAsync(IDialogContext context, IAwaitable<object> beforeActivity)
        {
            var activity = await beforeActivity as Activity;

            //사용자 예약일시 정보 저장
            memberReservation.Date = activity.Text;


            var reply = context.MakeMessage();

            var options = new[]
            {
                "오전 11시~12시",
                "오후 1시~2시",
                "오후 4시~5시",
            };

            reply.AddHeroCard(activity.Text, "아래 원하시는 에약시간을 선택해주세요.", options
                , new[] { "https://www.chahongardor.com/wp-content/uploads/2019/02/d01.jpg" });
            await context.PostAsync(reply);

            context.Wait(this.ReservationCheckMessageAsync);
        }


        /*/// <summary>
        /// 예약시간 선택완료
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task OnReservationTimeSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            memberReservation.Time = message.Text;

            if (message.Text == "오전 11시~12시" || message.Text == "1")
            {
                await context.PostAsync($" { memberReservation.DesignerName}디자이너에게 [{ memberReservation.Menu}]스타일로 \n\n " +
                    $"[{ memberReservation.Date}]에 [{ memberReservation.Time}]에 예약하셨습니다.");
                context.Wait(this.ReservationCheckMessageAsync);
            }
            else if (message.Text == "오후 1시~2시" || message.Text == "2")
            {
                await context.PostAsync($" { memberReservation.DesignerName}디자이너에게 [{ memberReservation.Menu}]스타일로 \n\n " +
                    $"[{ memberReservation.Date}]에 [{ memberReservation.Time}]에 예약하셨습니다.");
                context.Wait(this.ReservationCheckMessageAsync);
            }
            else if (message.Text == "오후 4시~5시" || message.Text == "3")
            {
                await context.PostAsync($" { memberReservation.DesignerName}디자이너에게 [{ memberReservation.Menu}]스타일로 \n\n " +
                    $"[{ memberReservation.Date}]에 [{ memberReservation.Time}]에 예약하셨습니다.");
                context.Wait(this.ReservationCheckMessageAsync);
            }
            else
            {
                await this.StartOverAsync(context, "죄송합니다. 요청사항을 이해하지 못했습니다.^^ ");
            }
        }*/



        /// <summary>
        /// 최종예약처리 여부 묻기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="beforeActivity"></param>
        /// <returns></returns>
        private async Task ReservationCheckMessageAsync(IDialogContext context, IAwaitable<object> beforeActivity)
        {
            var activity = await beforeActivity as Activity;

            //사용자 예약시간 정보 저장
            memberReservation.Time = activity.Text;

            var reply = context.MakeMessage();

            var options = new[]
            {
                "예약하기",
                "처음으로 돌아가기",
            };

            reply.AddHeroCard("최종 예약확인", 
                $"{ memberReservation.DesignerName}디자이너에게 [{ memberReservation.Menu}]스타일로\n" +
                    $"[{ memberReservation.Date}]에 [{ memberReservation.Time}]에 예약하셨습니다.\n그대로 진행 하시겠습니까?", options);
            await context.PostAsync(reply);

            context.Wait(this.OnReservationCheckSelected);
        }



        /// <summary>
        /// 예약 선택완료
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task OnReservationCheckSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            memberReservation.Time = message.Text;

            if (message.Text == "예약하기")
            {
                await context.PostAsync($"예약을 진행합니다. \n\n 성함을 입력해주세요.\n\n ");
                context.Wait(this.GetUserNameAsync);
            }
            else if (message.Text == "처음으로 돌아가기")
            {
                await this.StartOverAsync(context, "예약을 취소하고 처음으로 돌아갑니다.");
            }
            else
            {
                await this.StartOverAsync(context, "죄송합니다. 요청사항을 이해하지 못했습니다.^^ ");
            }
        }


        /// <summary>
        /// 이름받기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task GetUserNameAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            memberReservation.MemberName = activity.Text;

            await context.PostAsync($"전화번호를 입력해주세요.");
            context.Wait(this.GetUserTelephoneAsync);
        }




        /// <summary>
        /// 전화번호 받기
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task GetUserTelephoneAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result as Activity;
            memberReservation.Telephone = activity.Text;

            await context.PostAsync($"감사합니다. 예약이 완료되었습니다.\n\n ");
            await this.ConfirmServiceTypeMessageAsync(context);
        }



        /// <summary>
        /// 미용실 소개 동영상 메시지 처리
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task WelcomeVideoMessageAsync(IDialogContext context, IAwaitable<object> beforeActivity)
        {
            var reply = context.MakeMessage();

            var videoCard = new VideoCard
            {
                Title = "CHAHONG ARDOR DOSAN",
                Subtitle = "한국인 최초 로레알 프로페셔널 파리의 세계적인 아티스트로 선정된 " +
                "차홍원장이 이끄는 청담동미용실 차홍아르더 도산점, 본점, 청담점은 " +
                "헤어, 메이크업 ,네일 전문가들이 수많은 셀렙들의 스타일은 물론 " +
                "가장 트랜디한 스타일과 프라이빗한 고급서비스를 전달하는 공간입니다. ",
                Text = "",
                Image = new ThumbnailUrl
                {
                    Url = "http://www.chahongardor.com/wp-content/uploads/2019/08/MG_3947-1200x800.jpg"
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "https://www.youtube.com/watch?v=nK3ueByYr9w#action=share"
                    }
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "자세히 보기",
                        Type = ActionTypes.OpenUrl,
                        Value = "http://www.chahongardor.com/"
                    },
                    new CardAction()
                    {
                        Title = "이전으로",
                        Type = ActionTypes.ImBack,
                        Value = "이전으로"
                    }
                }
            };

            reply.Attachments.Add(videoCard.ToAttachment());
            await context.PostAsync(reply);
            context.Wait(this.OnWelcomMsgSelected);
        }

        private async Task OnWelcomMsgSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            await this.StartOverAsync(context, "감사합니다. 서비스 목록으로 이동합니다.");
        }


        /// <summary>
        /// 자연어처리(LUIS) 를 이용한 사용자 의도 파악
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task GetLUISIntentAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            string intent = "테스트";

            var client = new HttpClient();
            var uri = "https://westus.api.cognitive.microsoft.com/luis/v2.0"
                +"/apps/c5d5e819-10ae-477e-87f2-7f357f2dda82?subscription-key=eee7c9a1669449f1b1adef6c13ba5b49&verbose=true&timezoneOffset=540"
                +"&q=" + activity.Text;
            var response = await client.GetAsync(uri);

            var strResponseContent = await response.Content.ReadAsStringAsync();

            LuisResult objResult  = JsonConvert.DeserializeObject<LuisResult>(strResponseContent);
            intent = objResult.TopScoringIntent.Intent;
            await context.PostAsync($"사용자 최적화된 의도는 {objResult.TopScoringIntent.Intent} 이고 적합도는 {objResult.TopScoringIntent.Score} 입니다.");

            switch(intent)
            {
                case "예약":
                    await context.PostAsync($"바로 예약하시겠습니까?");
                    //context.Wait(TobeContinuedReservationProcess);
                    break;
                case "위치":
                    await context.PostAsync($"위치와 약도를 안내해드릴까요?");
                    //context.Wait(TobeContinuedContactUsProcess);
                    break;
            }
        }



    }
}