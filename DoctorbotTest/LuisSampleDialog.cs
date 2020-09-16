namespace DoctorbotTest
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using MySql.Data.MySqlClient;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;


    [LuisModel("70f3755d-a8b0-4ceb-9b22-3dfc54846422", "d343878fe5d34725a2339cead0bfe662")]
    [Serializable]
    public class LuisSampleDialog : LuisDialog<object>
    {
        private String dogsName;
        private const string EntitySymptom1 = "symptom1";
        private const string EntitySymptom2 = "symptom2";
        private const string EntitySymptom = "symptom";
        private string index_num = "초기값";
        int selectIndex; // 180624 수민
        string aaaa;
        string why_s1;

        private const string EntityDiseas0 = "disease";
        private const string EntityDoggieName = "dogName";

        private IEnumerable<q1> qmenu = new List<q1>();

        private string ss1 = "";
        private string specific1_copy = "";

        private Boolean hasOnly = false;

        [LuisIntent("greeting")]
        public async Task greets(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("안녕하세요! 강아지 이름이 뭔가요~?");
        }


        [LuisIntent("introduce")]
        public async Task introduce(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            var dQueryForName = new DiseaseQuery();

            EntityRecommendation nameEntityRecommendation;
            
            //결과 
            if (result.TryFindEntity(EntityDoggieName, out nameEntityRecommendation))
            {
                nameEntityRecommendation.Type = "dogName";
            }
            
            var NameDialog = new FormDialog<DiseaseQuery>(dQueryForName, this.BuildNameForm, FormOptions.PromptInStart, result.Entities);

            context.Call(NameDialog, this.findName);
        }

        // 인사해주기(서비스 선택)
        private async Task findName(IDialogContext context, IAwaitable<DiseaseQuery> result)
        {
            var searchQuery = await result;

            dogsName = Regex.Replace(searchQuery.dogName + "", " ", "");

            // await context.PostAsync($"{Name}주인님 안녕하세요 왈왈 무슨 일이신가요?!"); // 기존에 했던 거 180513

            // 새로 하는거 180513
            // 버튼 형식으로 서비스를 선택하도록 하기
            try
            {
                //180514 수민 주석처리함
                 await context.PostAsync(dogsName+"주인님 안녕하세요 왈왈");
                var message = context.MakeMessage();
                var actions = new List<CardAction>();

                // 서비스 (1. 증상에 따른 질병 검색 2. 약품 검색 3. 강아지 종류에 따라 자주 일어나는 질병 검색 4. 근처 동물 병원을 알려주세요)
                actions.Add(new CardAction() { Title = 1 + ". " + "우리 강아지가 아파요", Value = 1 + "" });
                actions.Add(new CardAction() { Title = 2 + ". " + "강아지 약품을 검색하고 싶어요", Value = 2 + "" });
                actions.Add(new CardAction() { Title = 3 + ". " + "강아지 종류에 따라 자주 일어나는 질병은 뭔가요?", Value = 3 + "" });

                message.Attachments.Add(
                    new HeroCard
                    {
                        Title = "원하시는 서비스를 선택하세요!",
                        Subtitle = "서비스를 선택하고 싶다면 '서비스'라고 입력해주세요!",
                        Buttons = actions
                    }.ToAttachment());

                await context.PostAsync(message);
                context.Wait(ServiceSelect);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                // context.Done<object>(null);
            }
        }

        public async Task Select_menu(IDialogContext context)
        {
            try
            {
                var message = context.MakeMessage();
                var actions = new List<CardAction>();

                // 서비스 (1. 증상에 따른 질병 검색 2. 약품 검색 3. 강아지 종류에 따라 자주 일어나는 질병 검색 4. 근처 동물 병원을 알려주세요)
                actions.Add(new CardAction() { Title = 1 + ". " + "우리 강아지가 아파요", Value = 1 + "" });
                actions.Add(new CardAction() { Title = 2 + ". " + "강아지 약품을 검색하고 싶어요", Value = 2 + "" });
                actions.Add(new CardAction() { Title = 3 + ". " + "강아지 종류에 따라 자주 일어나는 질병은 뭔가요?", Value = 3 + "" });

                message.Attachments.Add(
                    new HeroCard
                    {
                        Title = "원하시는 서비스를 선택하세요!",
                        Subtitle = "서비스를 선택하고 싶다면 '서비스'라고 입력해주세요!",
                        Buttons = actions
                    }.ToAttachment());

                await context.PostAsync(message);
                context.Wait(ServiceSelect);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                // context.Done<object>(null);
            }
        }

        // 180513 서비스 선택 수민
        private async Task ServiceSelect(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            string selected = activity.Text;
            int selectedNum = Int32.Parse(selected);

            if (selected == "" + selectedNum)
            {
                if (selectedNum == 1) // 증상 검색
                {
                    await context.PostAsync("증상 검색 서비스를 선택하셨군요!");
                    await context.PostAsync("어떤 증상이 있나요?");

                    context.Wait(MessageReceived);
                }
                else if (selectedNum == 2) // 약품 검색
                {
                    await context.PostAsync("약품 검색 서비스를 선택하셨군요!");
                    await context.PostAsync("약품명을 말해주세요~");


                    context.Wait(inputMedicine);
                }
                else if (selectedNum == 3) // 종류에 따라 자주 일어나는 질병
                {
                    // await context.PostAsync("서비스 구현 중입니당(종류에 따라 자주 일어나는 질병)");
                    await context.PostAsync("강아지 종류가 뭔가요?");

                    context.Wait(dogType);
                }
            }
        }
        
        // 수민 Explain  - 180506 완성
        private string ExplainationSystem(int index_num)
        {
            if (hasOnly)
            {
                string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

                string symptom = "";
                string specific1 = "";
                
                using (MySqlConnection connection = new MySqlConnection(strConn))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT DISTINCT symptom, specific_symptom1 from doggie where index_num= " + index_num + " ;");

                    String sql = sb.ToString();

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                symptom = reader.GetString(0);
                                specific1 = reader.GetString(1);
                            };
                            connection.Close();
                        }
                    }

                }
                string transform1 = Morpheme.TransformSentence(symptom, "는 경우에, \n");
                string transform2 = Morpheme.TransformSentence(specific1, "면");

                string end = transform1 + " " + transform2 + " 이런 질병이 나올 수 있습니다. \n질병에 맞게 대처해주세요.";
                return end;
            }
            else
            {
                string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

                string symptom = "";
                string specific1 = "";
                string specific2 = "";
                
                using (MySqlConnection connection = new MySqlConnection(strConn))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT DISTINCT symptom,specific_symptom1,specific_symptom2 from doggie where index_num=" + index_num + " ;");

                    String sql = sb.ToString();

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                symptom = reader.GetString(0);
                                specific1 = reader.GetString(1);
                                specific2 = reader.GetString(2);

                            };
                            connection.Close();
                        }
                    }

                }

                string transform1 = Morpheme.TransformSentence(symptom, "는 경우에, \n");
                string transform2 = Morpheme.TransformSentence(specific1, "고, ");
                string transform3 = Morpheme.TransformSentence(specific2, "면");


                string end = transform1 + " " + transform2 + " " + transform3 + " 이런 질병이 나올 수 있습니다. \n질병에 맞게 대처해주세요.";

                return end;
            }
        }



        // 수민 - Explanation System 구현
        [LuisIntent("explain")]
        public async Task Explain(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"왜냐면요~ "+ dogsName +"주인님");
            await context.PostAsync(ExplainationSystem(Convert.ToInt32(index_num)));
            context.Wait(MessageReceived);

        }

        private IForm<DiseaseQuery> BuildNameForm()
        {
            OnCompletionAsyncDelegate<DiseaseQuery> processNameSearch = async (context, state) =>
            {
                //안에 증상이 포함되있었다면
                if (!string.IsNullOrEmpty(state.dogName))
                {
                    string s1 = Regex.Replace(state.dogName + "", " ", "");
                }
            };

            return new FormBuilder<DiseaseQuery>()
            .Field(nameof(DiseaseQuery.dogName), (state) => string.IsNullOrEmpty(state.dogName))
            .OnCompletion(processNameSearch)
            .Build();
        }

        // 종류에 따라 자주 일어나는 질병 180514 수민
        public async Task dogType(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var activity = await result as Activity;
                string got_type = activity.Text;

                var results = GetDogType(got_type);
                var message = new StringBuilder();

                // 카드 형식
                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                if (results.Count() == 0)
                {
                    // await context.PostAsync("해당 강아지에 대한 정보를 찾을 수 없습니다. ");



                    string urlname = HttpUtility.UrlEncode(got_type);
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = "해당 견종에 대한 정보를 찾을 수 없습니다.",
                        Subtitle = "Naver 지식백과에 대신 검색해드릴게요!",
                        Images = new List<CardImage>()
                            {
                                new CardImage(url: "https://mblogthumb-phinf.pstatic.net/20141203_173/dic_master_1417582673615KpXfe_JPEG/%C1%F6%BD%C4%B9%E9%B0%FA%B7%CE%B0%ED.jpg?type=w2")
                            },
                        Buttons = new List<CardAction>()
                            {
                                new CardAction()
                                {
                                    Type = ActionTypes.OpenUrl,
                                    Title = "Naver 지식백과에 검색된 내용 보기",
                                    Value = "https://terms.naver.com/search.nhn?query="+urlname+""
                                 }
                            }
                    };

                    resultMessage.Attachments = new List<Attachment> { heroCard.ToAttachment() };
                    await context.PostAsync(resultMessage);




                    context.Wait(MessageReceived);
                }

                else
                {

                    foreach (var i in results)
                    {
                        HeroCard heroCard = new HeroCard()
                        {
                            Images = new List<CardImage>()
                        {
                            new CardImage() { Url = i.d_img }
                        },
                            Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = i.d_type,
                                Value = i.d_type
                            }
                        }
                        };

                        resultMessage.Attachments.Add(heroCard.ToAttachment());
                    }

                    await context.PostAsync(resultMessage);
                    context.Wait(GoToDisease);

                }



            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {

            }
        }

        // 강아지 종류 보여주기(강아지 종류랑 강아지 사진만)
        private IEnumerable<dogTypes> GetDogType(String got_type)
        {
            // 데이터베이스에서 종류에 따라 자주 발생하는 질병 찾아오기
            var DataList = new List<dogTypes>();
            {
                string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";


                using (MySqlConnection connection = new MySqlConnection(strConn))
                {
                    connection.Open();

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT DISTINCT dog_type, dog_img from dogdisease;");

                    String sql = sb.ToString();

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string type = reader.GetString(0);
                                string img = reader.GetString(1);

                                if ((type.Contains(got_type)))   // 찾았으면,, 그 안에서 
                                {
                                    dogTypes typeDisease = new dogTypes()
                                    {
                                        d_type = reader.GetString(0), // 강아지 종류
                                        d_img = reader.GetString(1) // 강아지 사진
                                    };

                                    DataList.Add(typeDisease);
                                }
                            };
                            connection.Close();
                        }
                    }
                }
            }

            return DataList;
        }

        // 질병 알려주깅 180514
        private async Task GoToDisease(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var activity = await result as Activity;
                string dogType = activity.Text; // 사용자가 선택한 강아지 종류

                var diseases = GetTypeDisease(dogType); // 질병들 가져오기

                var message = context.MakeMessage();
                var actions = new List<CardAction>();

                //
                foreach (var disease in diseases)
                {

                    actions.Add(new CardAction()
                    {
                        Title = disease.disease, Type = ActionTypes.OpenUrl,  Value = disease.disease_info
                    });
                }

                message.Attachments.Add(
                    new HeroCard
                    {
                        Title = $"{dogType}에게 자주 발생하는 질병은 다음과 같습니다.",
                        Subtitle = "자세한 정보는 질병을 선택하세요.",
                        Buttons = actions
                    }.ToAttachment());

                await context.PostAsync(message);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Wait(MessageReceived);
            }
        }

        // 종류에 따라 자주 일어나는 질병,,,,
        private IEnumerable<dogTypeDisease> GetTypeDisease(String got_type)
        {
            // 데이터베이스에서 종류에 따라 자주 발생하는 질병 찾아오기
            var DataList = new List<dogTypeDisease>();
            {
                string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

                using (MySqlConnection connection = new MySqlConnection(strConn))
                {
                    connection.Open();

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT DISTINCT dog_type, disease, disease_info from dogdisease where dog_type='" + got_type + "';");
                    String sql = sb.ToString();

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string type = reader.GetString(0);
                                string disease = reader.GetString(1);
                                string disease_url = reader.GetString(2);

                                if ((type.Contains(got_type)))   // 찾았으면,, 그 안에서 
                                {
                                    dogTypeDisease typeDisease = new dogTypeDisease()
                                    {
                                        dog_type = reader.GetString(0), // 강아지 종류
                                        disease = reader.GetString(1), // 자주 발생하는 질병
                                        disease_info = reader.GetString(2) // 질병에 대한 자세한 설명 url
                                    };

                                    DataList.Add(typeDisease);
                                }
                            };
                            connection.Close();
                        }
                    }
                }
            }

            return DataList;
        }
        
        // 수민 180510 약품 찾기
        [LuisIntent("medicine")]
        public async Task findMediCate(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            // 약품명을 찾고싶어ㅣ요 -> 의도가 medicine -> 약품명을 말해주세요.
            await context.PostAsync("약품명을 말해주세요~");

            context.Wait(inputMedicine);
        }

        // 사용자가 약품명을 말하면 그에 대한 대답(해당 약품명의 카테고리를 알려주기)
        public async Task inputMedicine(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var activity = await result as Activity;
                string receivedName = activity.Text;

                var got_result = GetMedicines(receivedName); // 가져온 약품들(DataList 타입)
                var message = new StringBuilder();

                // 여기부터 카드형식으로 하는거 (수민 180513)
                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();


                if (got_result.Count() == 0)
                {
                    //   await context.PostAsync("해당 약품에 대한 정보를 찾을 수 없습니다.");


                    string urlname = HttpUtility.UrlEncode(receivedName);
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = "해당 약품에 대한 정보를 찾을 수 없습니다.",
                        Subtitle = "Naver 지식백과에 대신 검색해드릴게요!",
                        Images = new List<CardImage>()
                            {
                                new CardImage(url: "https://mblogthumb-phinf.pstatic.net/20141203_173/dic_master_1417582673615KpXfe_JPEG/%C1%F6%BD%C4%B9%E9%B0%FA%B7%CE%B0%ED.jpg?type=w2")
                            },
                        Buttons = new List<CardAction>()
                            {
                                new CardAction()
                                {
                                    Type = ActionTypes.OpenUrl,
                                    Title = "Naver 지식백과에 검색된 내용 보기",
                                    Value = "https://terms.naver.com/search.nhn?query="+urlname+""
                                 }
                            }
                    };

                    resultMessage.Attachments = new List<Attachment> {heroCard.ToAttachment()};
                    await context.PostAsync(resultMessage);

                }
                else
                {

                    foreach (var i in got_result)
                    {
                        HeroCard heroCard = new HeroCard()
                        {
                            Title = i.medicine_name,
                            Subtitle = $"{i.medicine_cate}로 분류됩니다.",
                            Images = new List<CardImage>()
                        {
                            new CardImage() { Url = i.medicine_img }
                        },
                            Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "더 자세한 정보는 여기에",
                                Type = ActionTypes.OpenUrl,
                                Value = i.medicine_info
                            }
                        }
                    };

                        resultMessage.Attachments.Add(heroCard.ToAttachment());
                    }

                    await context.PostAsync(resultMessage);


                }

            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Wait(MessageReceived);
            }
        }

        // 약을 찾아 헤맨다..
        private IEnumerable<med> GetMedicines(String got_name)
        {
            var DataList = new List<med>();
            {
                string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

                using (MySqlConnection connection = new MySqlConnection(strConn))
                {
                    connection.Open();

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT DISTINCT category,name, image_url, medicine_url from medd;");

                    String sql = sb.ToString();

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string name = reader.GetString(1);
                                string name_received = got_name.ToString();

                                if ((name.Contains(name_received)))   // 찾았으면,, 그 안에서 
                                {
                                    med mediciness = new med()
                                    {
                                        medicine_cate = reader.GetString(0), // 약품 카테고리
                                        medicine_name = reader.GetString(1), // 약품 이름
                                        medicine_img = reader.GetString(2), // 약품 사진
                                        medicine_info = reader.GetString(3) // 약품 정보를 알려주는 상세 페이지의 url
                                    };
                                    DataList.Add(mediciness);

                                }
                            };
                            connection.Close();
                        }
                    }
                }
            }

            return DataList;
        }
        
        // 수민 ) 약품 이름을 쳤을 경우, db에서 약품 정보 조회 180510 -> 얘가 안되는듯
        private string medicineCategory(string medicineName)
        {
            String category = "";
            {
                string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

                using (MySqlConnection connection = new MySqlConnection(strConn))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT DISTINCT category, name from medd;");
                

                    String sql = sb.ToString();
                    
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string s = reader.GetString(1);

                                if ((s.Contains(medicineName)))   // 찾았으면,, 그 안에서 
                                {
                                    category = reader.GetValue(0).ToString();
                                }
                            };
                            connection.Close();
                        }
                    }
                }
            }
            
            return category;
        }

        [LuisIntent("help")]
        public async Task help(IDialogContext context, LuisResult result)
        {
            try
            {
                //180514 수민 주석처리함
                var message = context.MakeMessage();
                var actions = new List<CardAction>();

                // 서비스 (1. 증상에 따른 질병 검색 2. 약품 검색 3. 강아지 종류에 따라 자주 일어나는 질병 검색 4. 근처 동물 병원을 알려주세요)
                actions.Add(new CardAction() { Title = 1 + ". " + "우리 강아지가 아파요", Value = 1 + "" });
                actions.Add(new CardAction() { Title = 2 + ". " + "강아지 약품을 검색하고 싶어요", Value = 2 + "" });
                actions.Add(new CardAction() { Title = 3 + ". " + "강아지 종류에 따라 자주 일어나는 질병은 뭔가요?", Value = 3 + "" });

                message.Attachments.Add(
                    new HeroCard
                    {
                        Title = "원하시는 서비스를 선택하세요!",
                        Subtitle = "서비스를 선택하고 싶다면 '서비스'라고 입력해주세요!",
                        Buttons = actions
                    }.ToAttachment());

                await context.PostAsync(message);
                context.Wait(ServiceSelect);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                // context.Done<object>(null);
            }
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"알아듣지 못했어요 다시 입력해주세요.");
            context.Wait(MessageReceived);
        }


        // 의도가 질병에 관한 것일 때
        [LuisIntent("symptom_simple")]
        public async Task symptom0(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            await context.PostAsync($"'{message.Text}'라고...");
            
            var dQuery = new DiseaseQuery();

            EntityRecommendation symptommEntityRecommendation;

            //결과 
            if (result.TryFindEntity(EntitySymptom, out symptommEntityRecommendation))
            {
                symptommEntityRecommendation.Type = "Symptom";
            }

            else if (result.TryFindEntity(EntitySymptom1, out symptommEntityRecommendation))
            {
                symptommEntityRecommendation.Type = "Symptom_noun";
            }
            else
            {
                await context.PostAsync($"정보가 없어용...");
            }

            var SymtomDialog = new FormDialog<DiseaseQuery>(dQuery, this.BuildDiseaseForm_1, FormOptions.PromptInStart, result.Entities);

            context.Call(SymtomDialog, this.finded_1);
        }

        [LuisIntent("symptom_body")] //[LuisIntent("DogSymptom")]
        public async Task symptom1(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            // await context.PostAsync($"'{message.Text}'라고...");


            var dQuery = new DiseaseQuery();

            EntityRecommendation symptomEntityRecommendation_1;
            EntityRecommendation symptomEntityRecommendation_2;


            //결과 
            if (result.TryFindEntity(EntitySymptom1, out symptomEntityRecommendation_1))
            {
                symptomEntityRecommendation_1.Type = "Symptom_noun";
            }

            if (result.TryFindEntity(EntitySymptom2, out symptomEntityRecommendation_2))
            {
                symptomEntityRecommendation_2.Type = "Symptom_verb";
            }

            var SymtomDialog = new FormDialog<DiseaseQuery>(dQuery, this.BuildDiseaseForm_2, FormOptions.PromptInStart, result.Entities);

            context.Call(SymtomDialog, this.finded_2);
        }

        
        private IForm<DiseaseQuery> BuildDiseaseForm_1()
        {
            OnCompletionAsyncDelegate<DiseaseQuery> processDiseaseSearch = async (context, state) =>
            {
                var message = "";

                //안에 증상이 포함되있었다면
                if (!string.IsNullOrEmpty(state.Symptom))
                {
                    string s1 = Regex.Replace(state.Symptom + "", " ", "");

                    message += $"{s1}를 하는 증상이 있다면...";
                }
                else
                {
                    message = "!!!증상을 찾을 수 없습니다.";
                }


                await context.PostAsync(message);
            };


            return new FormBuilder<DiseaseQuery>()
            .Field(nameof(DiseaseQuery.Symptom), (state) => string.IsNullOrEmpty(state.Symptom))
            .OnCompletion(processDiseaseSearch)
            .Build();
        }
        
        private IForm<DiseaseQuery> BuildDiseaseForm_2()
        {
            OnCompletionAsyncDelegate<DiseaseQuery> processDiseaseSearch = async (context, state) =>
            {
                var message = "흠..";
                string s1 = "";
                string s2 = "";

                //안에 증상이 포함되있었다면
                if (!string.IsNullOrEmpty(state.Symptom_noun) && !string.IsNullOrEmpty(state.Symptom_verb))
                {
                    s1 = Regex.Replace(state.Symptom_noun + "", " ", "");
                    s2 = Regex.Replace(state.Symptom_verb + "", " ", "");
                    message += $"[{s1}]가 [{s2}]하다면..";
                }

                await context.PostAsync(message);
            };


            return new FormBuilder<DiseaseQuery>()
            .Field(nameof(DiseaseQuery.Symptom_noun))
            .Field(nameof(DiseaseQuery.Symptom_verb))
            .OnCompletion(processDiseaseSearch)
            .Build();
        }


        //잠시만 주석쓰 1001
        /*
        // 이게 진짜 쓰는거 180528 수민
        private async Task Select2(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            string selected = activity.Text;
            int selectedNum = Int32.Parse(selected);


            if (selected == "" + selectedNum)
            {
                await context.PostAsync(selectedNum + "번을 선택하셨습니다.");
                var diseases = this.GetDiseases3(selectedNum);

                UpdateSelected_s2();

                var message = context.MakeMessage();
                var actions = new List<CardAction>();

                int i = 1;

                int sum = 0;
                int selPercent = -1;
                foreach (var d in diseases) // 각각에 대해 select_num을 가져온다.
                {
                    sum += GetSelectNum(d.iNum); // 전체 합산
                }

                int selNum = -1;
                // 180624 수민 버튼 형식으로 바꿈 ing
                foreach (var d in diseases)
                {
                    selNum = GetSelectNum(d.iNum);

                    if (sum == 0)
                    {
                        selPercent = 0;
                    }
                    else {
                        selPercent = selNum * 100 / sum;
                    }

                    actions.Add(new CardAction()
                    {
                            Title = d.disease + "(" + selPercent + "%)" + " : " + d.handle,
                            Type = ActionTypes.PostBack,
                            Value = d.disease_url
                     });
                    
                    i++;
                }

                message.Attachments.Add(
                    new HeroCard
                    {
                        Title = "이런 질병이 있을 수 있습니다\n",
                        Subtitle = "자세한 정보는 질병을 선택하세요. 퍼센트는 다른 사용자들이 해당 질병을 선택한 비율입니다.",
                        Buttons = actions
                    }.ToAttachment()
                );

                await context.PostAsync(message);
            }

            context.Wait(DiseaseSel);
            
        }
        */




        private IEnumerable<q3> explainWhy()
        {
            var data11 = new List<q2>();

            string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

            using (MySqlConnection connection = new MySqlConnection(strConn))
            {

                connection.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT DISTINCT case_num, symptom,specific_symptom2 from doggie WHERE specific_symptom1= '" + specific1_copy + "' order by selected_s2 desc;");
                String sql = sb.ToString();



                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string s = reader.GetString(1); // 변비를 합니다.

                            if (s.Equals(ss1))   // 찾았으면,, 그 안에서 
                            {

                                // 리스트에 추가하기 
                                q2 forAnswer = new q2()
                                {
                                    symNum = reader.GetValue(0).ToString(),
                                    specific_2 = reader.GetString(2)

                                };

                                data11.Add(forAnswer);
                            }
                        };
                    }
                }

                connection.Close();

            }


            //n개의 세부 증상을 찾는 거야,, 
            int n = data11.Count();

            var data22 = new List<q3>();

            int count = 0;

            for (int i = 0; i < n; i++)
            {
                // 저장

                why_s1 = data11[i].specific_2.ToString();

                String save1 = "";


                using (MySqlConnection connection1 = new MySqlConnection(strConn))
                {
                    connection1.Open();
                    StringBuilder sb1 = new StringBuilder();
                    sb1.Append("SELECT DISTINCT index_num,symptom, disease, specific_symptom2 from doggie WHERE specific_symptom2= '" + why_s1 + "' order by select_num desc;");

                    String sql1 = sb1.ToString();

                    using (MySqlCommand command = new MySqlCommand(sql1, connection1))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            count = 0;
                            while (reader.Read())
                            {
                                string s = reader.GetString(1); // 변비를 합니다.

                                if (s.Equals(ss1))   // 찾았으면,, 그 안에서 
                                {
                                    if (count == 0)
                                    {
                                        // 리스트에 추가하기 
                                        save1 += reader.GetString(2).ToString();
                                    }

                                    else{
                                        // 리스트에 추가하기 
                                        save1 += ", " + reader.GetString(2).ToString();
                                    }
                                    count++;
                                }
                            };
                            connection1.Close();
                        }
                    }
                }

                q3 saveQ3 = new q3()
                {
                    iNum = 0,
                    specific_2 = why_s1,
                    disease = save1
                };

                data22.Add(saveQ3);


            }




            //for문 끝남


            return data22;

        }




        // 이게 진짜 쓰는거 180528 수민
        private async Task Select2(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            string selected = activity.Text;
            int selectedNum; //= Int32.Parse(selected);


            if (Int32.TryParse(selected, out selectedNum))
            {

                if (selected == "" + selectedNum)
                {
                    await context.PostAsync(selectedNum + "번을 선택하셨습니다.");
                    var diseases = this.GetDiseases3(selectedNum);

                    UpdateSelected_s2();

                    var message = context.MakeMessage();
                    var actions = new List<CardAction>();

                    int i = 1;

                    int sum = 0;
                    int selPercent = -1;
                    foreach (var d in diseases) // 각각에 대해 select_num을 가져온다.
                    {
                        sum += GetSelectNum(d.iNum); // 전체 합산
                    }

                    int selNum = -1;
                    // 180624 수민 버튼 형식으로 바꿈 ing
                    foreach (var d in diseases)
                    {
                        selNum = GetSelectNum(d.iNum);

                        if (sum == 0)
                        {
                            selPercent = 0;
                        }
                        else
                        {
                            selPercent = selNum * 100 / sum;
                        }

                        actions.Add(new CardAction()
                        {
                            Title = d.disease + "(" + selPercent + "%)" + " : " + d.handle,
                            Type = ActionTypes.PostBack,
                            Value = d.disease_url
                        });

                        i++;
                    }

                    message.Attachments.Add(
                        new HeroCard
                        {
                            Title = "이런 질병이 있을 수 있습니다\n",
                            Subtitle = "자세한 정보는 질병을 선택하세요. 퍼센트는 다른 사용자들이 해당 질병을 선택한 비율입니다.",
                            Buttons = actions
                        }.ToAttachment()
                    );

                    await context.PostAsync(message);
                }
                
                context.Wait(DiseaseSel);
            }

            // why 처리
            else// 숫자로 선택 안했을 때 
            {
                selectedNum=0; 
                //왜 이런 리스트를 보여주는 지 설명을 해야 된다

                var reason = explainWhy();

                var message = new StringBuilder();

                message.AppendLine("# 이유에 대해 설명하겠습니다..");
                //    var message = context.MakeMessage();
                var actions = new List<CardAction>();
                foreach (var item in reason)
                {
                    //          actions.Add(new CardAction() { Title = "\'"+item.specific_2 + "\'의 증상이 있을 경우에는 \n" + item.disease+"의 질병이 나올 수 있습니다.", Value = i + "" });
                    message.Append($"* \'{item.specific_2}\'의 증상이 있을 경우에는\n {item.disease}의 질병이 의심될 수 있습니다.\n");

                }

                /*
                message.Attachments.Add(
                new HeroCard
                {
                    Title = "이유에 대해서 설명",
                    Buttons = actions
                }.ToAttachment()
            );*/

                await context.PostAsync(message.ToString());
            }

        }

        // selected_s2 숫자 1 증가
        private void UpdateSelected_s2()
        {
            string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

            using (MySqlConnection conn = new MySqlConnection(strConn))
            {
                conn.Open();
                StringBuilder sb = new StringBuilder();

                sb.Append("update doggie set selected_s2 = selected_s2 + 1 where specific_symptom2='" + aaaa + "';");

                String sql = sb.ToString();

                using (MySqlCommand command = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        conn.Close();
                    }
                }
            }
        }

        // select_num 가져오기
        private int GetSelectNum(int index_num)
        {
            int selNum = -1;
            string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";


            using (MySqlConnection conn = new MySqlConnection(strConn))
            {
                conn.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT select_num from doggie where index_num=" + index_num + ";");

                String sql = sb.ToString();

                using (MySqlCommand command = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            selNum = reader.GetInt32(0);
                        };
                        conn.Close();
                    }
                }
            }

            return selNum;
        }

        // 180624 수민
        private async Task DiseaseSel(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            string selected = activity.Text;
            

            int index_numPos = selected.IndexOf("index_num");
            
            int ddd = selected.Length - index_numPos;
            String index_num = selected.Substring(index_numPos + 10, ddd - 10);
            selectIndex = Int32.Parse(index_num);

            var message = context.MakeMessage();
            var actions = new List<CardAction>();

            string disease = GetDiseaseName(selectIndex); // 질병 이름 가져오기

            int http = selected.IndexOf("http");

            if (selected.IndexOf("http") == -1)
            {
                actions.Add(new CardAction()
                {
                    Title = "[" + disease + "]",
                    Type = ActionTypes.OpenUrl,
                    Value = selected
                });
            }
            else
            {
                actions.Add(new CardAction()
                {
                    Title = "[" + disease + "]에 대해 알고싶으시다면 클릭하세요!",
                    Type = ActionTypes.OpenUrl,
                    Value = selected
                });
            }

            actions.Add(new CardAction()
            {
                Title = "예",
                Value = "예"
            });

            actions.Add(new CardAction()
            {
                Title = "아니오",
                Value = "아니오"
            });

            actions.Add(new CardAction()
            {
                Title = "진단의 이유를 알고싶으신가요?",
                Value = "왜 이런 결과가 나왔나요?"
            });

            message.Attachments.Add(
                new HeroCard
                {
                    Title = "이 질병에 대한 정보가 도움이 되셨나요?",
                    Subtitle = "피드백을 해주세요 :)",
                    Buttons = actions
                }.ToAttachment()
            );

            await context.PostAsync(message);
            context.Wait(IsUsable);
            
        }

        // 질병 이름 가져오기
        private string GetDiseaseName(int index_num)
        {
            string disease = "";
            string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";


            using (MySqlConnection conn = new MySqlConnection(strConn))
            {
                conn.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT disease from doggie where index_num=" + index_num + ";");

                String sql = sb.ToString();

                using (MySqlCommand command = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            disease = reader.GetString(0);
                        };
                        conn.Close();
                    }
                }
            }

            return disease;
        }

        // 여기부터
        private async Task IsUsable(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            string isUsable = activity.Text;

            if (isUsable.Equals("예"))
            {
                UpdateSelectNum(selectIndex);
                await context.PostAsync("감사합니다:) 도움이 되었다면 기뻐요! 멍멍");
            }
            else if (isUsable.Equals("아니오"))
            {
                await context.PostAsync("아쉽네요 ㅜ-ㅜ 다음엔 좀 더 정확한 진단을 가져올게요! 멍멍");
            }
            else if (isUsable.Equals("왜 이런 결과가 나왔나요?"))
            {
                await context.PostAsync(ExplainationSystem(Convert.ToInt32(index_num)));
            }

            context.Wait(MessageReceived);
        }

        // 180624 수민 select_num update하기
        private void UpdateSelectNum(int selectIndex)
        {
            string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

            using (MySqlConnection conn = new MySqlConnection(strConn))
            {
                conn.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("update doggie set select_num = select_num + 1 where index_num=" + selectIndex + ";");

                String sql = sb.ToString();

                using (MySqlCommand command = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        conn.Close();
                    }
                }
            }
        }

        // selected_s1 증가시키는 함수
        private void UpdateSelected_s1()
        {
            string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

            using (MySqlConnection conn = new MySqlConnection(strConn))
            {
                conn.Open();
                StringBuilder sb = new StringBuilder();
                
                sb.Append("update doggie set selected_s1 = selected_s1 + 1 where specific_symptom1='" + specific1_copy + "';");

                String sql = sb.ToString();

                using (MySqlCommand command = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        conn.Close();
                    }
                }
            }
        }

        private async Task Select1(IDialogContext context, IAwaitable<object> result)
        {

            var activity = await result as Activity;
            string selected = activity.Text;
            // 받은 번호가 몇 번인지
            int selectedNum = Int32.Parse(selected);

            hasOnly = false;

            // 남은 선택지가 더 있을 때 
            if (selected == "" + selectedNum)
            {
                await context.PostAsync(selectedNum + "번을 선택하셨습니다"); // 여기까지는 됨
                
                var diseases = this.GetDiseases2(selectedNum);

                UpdateSelected_s1();

                if (diseases.Count() > 1)
                {
                    await context.PostAsync($"{diseases.Count()} 가지의 세부 증상을 찾았습니다.\n");

                }
                
                if (!hasOnly)
                {
                    var message = context.MakeMessage();
                    var actions = new List<CardAction>();
                    int i = 1;

                    foreach (var d in diseases) {
                        actions.Add(new CardAction() { Title = i + ". " + d.specific_2 + "", Value = i + "" });
                        i++;
                    }

                    message.Attachments.Add(
                    new HeroCard
                    {
                        Title = "혹시 이 중에 해당하는 증상이 있나요?",
                        Buttons = actions
                    }.ToAttachment());

                    await context.PostAsync(message);


                    context.Wait(Select2);
                }
                else
                {
                    try
                    {
                        var diseases12 = this.GetDiseases4(selectedNum);
                        var message3 = context.MakeMessage();
                        var actions = new List<CardAction>();

                        var message2 = new StringBuilder();

                        int sum = 0;
                        int selPercent = -1;
                        foreach (var d in diseases12) // 각각에 대해 select_num을 가져온다.
                        {
                            sum += GetSelectNum(d.iNum); // 전체 합산
                        }
                        int selNum = -1;
                        // 180624 수민 버튼 형식으로 바꿈 요깅
                        foreach (var d in diseases12)
                        {
                            selNum = GetSelectNum(d.iNum);
                            if (sum == 0)
                            {
                                selPercent = 0;
                            }
                            else
                            {
                                selPercent = selNum * 100 / sum;
                            }
                            actions.Add(new CardAction()
                            {
                                Title = d.disease + "(" + selPercent + "%)" + " : " + d.handle,
                                Type = ActionTypes.PostBack,
                                Value = d.disease_url
                            });
                        }

                        message3.Attachments.Add(
                            new HeroCard
                            {
                                Title = "이런 질병이 있을 수 있습니다.\n",
                                Subtitle = "자세한 정보는 질병을 선택하세요.",
                                Buttons = actions
                            }.ToAttachment()
                        );

                        await context.PostAsync(message3);
                        context.Wait(DiseaseSel);
                    }
                    catch (Exception ex)
                    {

                    }
                }


            }
            // 남은 선택지가 없을 때
        }
        

        // 찾은 내용 말해주기
        private async Task finded_1(IDialogContext context, IAwaitable<DiseaseQuery> result)
        {
            try
            {
                var searchQuery = await result;
                var diseases = this.GetDiseases(searchQuery);

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                if (diseases.Count() == 0)
                {
                    // await context.PostAsync("해당 증상에 대한 정보를 찾을 수 없습니다.\n");

                    

                    string urlname = HttpUtility.UrlEncode("강아지 "+searchQuery.Symptom);
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = "해당 증상에 대한 정보를 찾을 수 없습니다.",
                        Subtitle = "Naver 지식백과에 대신 검색해드릴게요!",
                        Images = new List<CardImage>()
                            {
                                new CardImage(url: "https://mblogthumb-phinf.pstatic.net/20141203_173/dic_master_1417582673615KpXfe_JPEG/%C1%F6%BD%C4%B9%E9%B0%FA%B7%CE%B0%ED.jpg?type=w2")
                            },
                        Buttons = new List<CardAction>()
                            {
                                new CardAction()
                                {
                                    Type = ActionTypes.OpenUrl,
                                    Title = "Naver 지식백과에 검색된 내용 보기",
                                    Value = "https://terms.naver.com/search.nhn?query="+urlname+""
                                 }
                            }
                    };

                    resultMessage.Attachments = new List<Attachment> { heroCard.ToAttachment() };
                    await context.PostAsync(resultMessage);




                }
                else
                {


                    if (diseases.Count()>1)
                        await context.PostAsync($"{diseases.Count()} 가지의 세부 증상을 찾았습니다");


                    var message = context.MakeMessage();
                    var actions = new List<CardAction>();
                    int i = 1;

                    foreach (var d in diseases)
                    {
                        actions.Add(new CardAction() { Title = d.iNum + ". " + d.specific_1 + "", Value = i + "" });
                        i++;
                    }

                    message.Attachments.Add(
                    new HeroCard
                    {
                        Title = "혹시 이 중에 해당하는 증상이 있나요?.",
                        Buttons = actions
                    }.ToAttachment()
                );

                    await context.PostAsync(message);

                    context.Wait(Select1);  // 선택하면 여기로 이동

                }




            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                // context.Done<object>(null);
            }
        }

        // 찾은 내용 말해주기
        private async Task finded_2(IDialogContext context, IAwaitable<DiseaseQuery> result)
        {
            try
            {
                var searchQuery = await result;

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                var diseases = this.GetDiseases_body(searchQuery);

                if (diseases.Count() == 0)
                {
                    // await context.PostAsync("해당 증상에 대한 정보를 찾을 수 없습니다.\n다른 서비스를 선택해주세요.");
                    string s1 = Regex.Replace(searchQuery.Symptom_noun + "", " ", ""); // 사용자가 입력한 증상 그대로 놔둬도 됨

                    string urlname = HttpUtility.UrlEncode("강아지 " + s1);
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = "해당 증상에 대한 정보를 찾을 수 없습니다.",
                        Subtitle = "Naver 지식백과에 대신 검색해드릴게요!",
                        Images = new List<CardImage>()
                            {
                                new CardImage(url: "https://mblogthumb-phinf.pstatic.net/20141203_173/dic_master_1417582673615KpXfe_JPEG/%C1%F6%BD%C4%B9%E9%B0%FA%B7%CE%B0%ED.jpg?type=w2")
                            },
                        Buttons = new List<CardAction>()
                            {
                                new CardAction()
                                {
                                    Type = ActionTypes.OpenUrl,
                                    Title = "Naver 지식백과에 검색된 내용 보기",
                                    Value = "https://terms.naver.com/search.nhn?query="+urlname+""
                                 }
                            }
                    };

                    resultMessage.Attachments = new List<Attachment> { heroCard.ToAttachment() };
                    await context.PostAsync(resultMessage);



                }
                else
                {
                    if (diseases.Count() > 1)
                    {
                        await context.PostAsync($"{diseases.Count()} 가지의 질병을 찾았습니다");
                    }

                    var message = context.MakeMessage();
                    var actions = new List<CardAction>();
                    int i = 1;

                    foreach (var d in diseases)
                    {
                        actions.Add(new CardAction() { Title = d.iNum + ". " + d.specific_1 + "", Value = i + "" });
                        i++;
                    }

                    message.Attachments.Add(
                    new HeroCard
                    {
                        Title = "혹시 이 중에 해당하는 증상이 있나요?.",
                        Buttons = actions
                    }.ToAttachment()
                );

                    await context.PostAsync(message);

                    context.Wait(Select1);  // 선택하면 여기로 이동

                }


            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                // context.Done<object>(null);
            }
        }



        private IEnumerable<q1> GetDiseases(DiseaseQuery searchQuery)
        {
            var DataList = new List<q1>();
            {
                var resultList = new List<q1>();
                int indexx = 1;

                string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";


                 using (MySqlConnection conn = new MySqlConnection(strConn))
                 {
                     conn.Open();
                     StringBuilder sb = new StringBuilder();
                     sb.Append("SELECT DISTINCT case_num, symptom, specific_symptom1 from doggie order by selected_s1 desc;");

                     String sql = sb.ToString();

                     using (MySqlCommand command = new MySqlCommand(sql, conn))
                     {
                         using (MySqlDataReader reader = command.ExecuteReader())
                         {
                             while (reader.Read())
                             {
                                 string s = reader.GetString(1); // 열의 2번째 DB에서 가져온 필드 내용
                                 string s1 = Regex.Replace(searchQuery.Symptom + "", " ", ""); // 사용자가 입력한 증상 그대로 놔둬도 됨

                                 if ((s.Contains(s1))) // DB에서 가져온 내용이 사용자가 입력한 증상을 포함하면
                                 {
                                     // 리스트에 추가하기 q1이라는 객체를 만듦
                                     q1 forAnswer = new q1()
                                     {
                                         iNum = indexx++,
                                         symNum = reader.GetValue(0).ToString(),
                                         specific_1 = reader.GetString(2)
                                     };

                                     DataList.Add(forAnswer);

                                     ss1 = s;
                                 }
                             };
                             conn.Close();
                         }
                     }
                 }
            }
            
            return DataList;
        }




        private IEnumerable<q1> GetDiseases_body(DiseaseQuery searchQuery)
        {


            var DataList = new List<q1>();
            {

                var resultList = new List<q1>();
                int indexx = 1;


                string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

                using (MySqlConnection connection = new MySqlConnection(strConn))
                {

                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT DISTINCT case_num, symptom, specific_symptom1 from doggie order by selected_s1 desc;");
                
                    String sql = sb.ToString();

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string s = reader.GetString(1);
                                string s1 = Regex.Replace(searchQuery.Symptom_noun + "", " ", "");
                                string s2 = Regex.Replace(searchQuery.Symptom_verb + "", " ", "");

                                if ((s.Contains(s1)) && (s.Contains(s2)))   // 찾았으면,, 그 안에서 
                                {
                                    // 리스트에 추가하기 
                                    q1 forAnswer = new q1()
                                    {
                                        iNum = indexx++,
                                        symNum = reader.GetValue(0).ToString(),
                                        specific_1 = reader.GetString(2)
                                    };

                                    DataList.Add(forAnswer);
                                    
                                    ss1 = s;
                                }
                            };
                            connection.Close();
                        }
                    }
                }
            }

            return DataList;
        }
        
        private IEnumerable<q2> GetDiseases2(int n)
        {   
            var preList = new List<q1>();
            string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";
           

            using (MySqlConnection connection = new MySqlConnection(strConn))
            {
                connection.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT DISTINCT case_num, symptom, specific_symptom1 from doggie order by selected_s1 desc; ");
            
                String sql = sb.ToString();

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string s = reader.GetString(1); // 변비를 합니다.
                            
                            if (s.Equals(ss1))   // 찾았으면,, 그 안에서 
                            {
                                // 리스트에 추가하기 
                                q1 forAnswer = new q1()
                                {
                                    symNum = reader.GetValue(0).ToString(),
                                    specific_1 = reader.GetString(2)
                                };
                                preList.Add(forAnswer);

                            }
                        };

                    }
                }
                connection.Close();
            }
            
            string specific1_string = preList[n - 1].specific_1.ToString();
            specific1_copy = specific1_string;


            var data_result = new List<q2>();

            

            using (MySqlConnection connection1 = new MySqlConnection(strConn))
            {
                connection1.Open();
                StringBuilder sb1 = new StringBuilder();
                sb1.Append("SELECT DISTINCT case_num,symptom, specific_symptom2 from doggie where specific_symptom1= '" + specific1_copy + "' order by selected_s2 desc;");
 

                String sql1 = sb1.ToString();

                using (MySqlCommand command = new MySqlCommand(sql1, connection1))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string s = reader.GetString(1); // 변비를 합니다.
                            string sp2 = reader.GetString(2);

                            if (sp2.Equals(""))
                            {
                                hasOnly = true;
                                q2 forAnswer = new q2()
                                {
                                    symNum = reader.GetValue(0).ToString(),
                                    specific_2 = ""
                                };

                                data_result.Add(forAnswer);
                            }

                            else if (s.Equals(ss1) && (!sp2.Equals("")))   // 찾았으면,, 그 안에서 
                            {
                                // 리스트에 추가하기 
                                q2 forAnswer = new q2()
                                {
                                    symNum = reader.GetValue(0).ToString(),
                                    specific_2 = reader.GetString(2).ToString()
                                };

                                data_result.Add(forAnswer);

                            }
                        };
                        connection1.Close();
                    }
                }
            }
            return data_result;
            
        }










        private IEnumerable<q3> GetDiseases3(int n)
        {
            var data11 = new List<q2>();

            string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

            using (MySqlConnection connection = new MySqlConnection(strConn))
            {

                connection.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT DISTINCT case_num, symptom,specific_symptom2 from doggie WHERE specific_symptom1= '" + specific1_copy + "' order by selected_s2 desc;");
                String sql = sb.ToString();



                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string s = reader.GetString(1); // 변비를 합니다.
                            
                            if (s.Equals(ss1))   // 찾았으면,, 그 안에서 
                            {

                                // 리스트에 추가하기 
                                q2 forAnswer = new q2()
                                {
                                    symNum = reader.GetValue(0).ToString(),
                                    specific_2 = reader.GetString(2)

                                };

                                data11.Add(forAnswer);
                            }
                        };
                    }
                }

                connection.Close();

            }





            // 저장

            aaaa = data11[n - 1].specific_2.ToString();

            var data22 = new List<q3>();
            using (MySqlConnection connection1 = new MySqlConnection(strConn))
            {
                connection1.Open();
                StringBuilder sb1 = new StringBuilder();
                sb1.Append("SELECT DISTINCT index_num,symptom, disease, handle, specific_symptom2, disease_url from doggie WHERE specific_symptom2= '" + aaaa + "' order by select_num desc;");
                
                String sql1 = sb1.ToString();
                
                using (MySqlCommand command = new MySqlCommand(sql1, connection1))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string s = reader.GetString(1); // 변비를 합니다.
                            
                            if (s.Equals(ss1))   // 찾았으면,, 그 안에서 
                            {
                                // 리스트에 추가하기 

                                var disease1 = reader.GetString(2);
                                q3 forAnswer = new q3()
                                {
                                    iNum = reader.GetInt32(0),
                                    specific_2 = reader.GetString(4).ToString(),
                                    disease = disease1,
                                    handle = reader.GetString(3).ToString(),
                                    disease_url = reader.GetString(5).ToString()
                                };
                                
                                data22.Add(forAnswer);
                            }
                            index_num = reader.GetValue(0).ToString();
                        };
                        connection1.Close();
                    }
                }
            }

            return data22;
            
        }


        private IEnumerable<q4> GetDiseases4(int n)
        {
            var preList = new List<q1>();
            string strConn = "Server=drmeongdb.mysql.database.azure.com;Database=drmeong;Uid=drmeong_admin@drmeongdb;Pwd=taco127!;";

            using (MySqlConnection connection = new MySqlConnection(strConn))
            {

                connection.Open();
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT DISTINCT case_num, symptom, specific_symptom1 from doggie order by selected_s1 desc;");

                String sql = sb.ToString();

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string s = reader.GetString(1); // 변비를 합니다.

                             if (s.Equals(ss1))   // 찾았으면,, 그 안에서 
                            {
                                // 리스트에 추가하기 
                                q1 forAnswer = new q1()
                                {
                                    symNum = reader.GetValue(0).ToString(),
                                    specific_1 = reader.GetString(2)
                                };
                                preList.Add(forAnswer);

                            }
                        };

                    }
                }
                connection.Close();
            }

            string specific1_string = preList[n - 1].specific_1.ToString();
            specific1_copy = specific1_string;
            

            var resultData = new List<q4>();

            using (MySqlConnection connection1 = new MySqlConnection(strConn))
            {

                connection1.Open();
                StringBuilder sb1 = new StringBuilder();
                sb1.Append("SELECT DISTINCT index_num,disease, handle, disease_url from doggie WHERE specific_symptom1= '" + specific1_copy + "' order by select_num desc;");
                String sql1 = sb1.ToString();

                using (MySqlCommand command = new MySqlCommand(sql1, connection1))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            {
                                // 리스트에 추가하기 
                                q4 forAnswer = new q4()
                                {
                                    iNum = reader.GetInt32(0),
                                    disease = reader.GetString(1),
                                    handle = reader.GetString(2),
                                    disease_url = reader.GetString(3)
                                };

                                resultData.Add(forAnswer);
                            }

                            index_num = reader.GetValue(0).ToString();
                        };

                        connection1.Close();
                    }
                }
            }

            return resultData;
        }
    }
}