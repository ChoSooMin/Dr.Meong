namespace DoctorbotTest
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public sealed class DiseaseQuery 
    {
        [Prompt("{&}이름만 입력해주세요.")]
        [Optional]
        public string Symptom { get; set; }

        [Prompt("{&}을/를 입력해주세요")]
        [Optional]
        public string Symptom_noun { get; set; }

        [Prompt("{&}을/를 입력해주세요")]
        [Optional]
        public string Symptom_verb { get; set; }

        [Prompt("Please enter your {&} Disease")]
        [Optional]
        public string Disease { get; set; }

        [Prompt("Please enter your {&} DoggieName")]
        [Optional]
        public string dogName { get; set; }

        public const string DefaultSymptom = "dSymptom";

    }
}