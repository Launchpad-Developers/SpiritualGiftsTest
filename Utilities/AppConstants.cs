using System;

namespace SpiritualGiftsSurvey.Utilities
{
    public static class AppConstants
    {
        // Put constants here that are not of a sensitive nature
        public const int HeaderHeight = 90;
        public const int HeaderOffset_iOS = 110;

        public const int TitleFontSize_Large = 30;
        public const int TitleAttFontSize_Large = 36;

        public const int TitleFontSize_Medium = 28;
        public const int TitleAttFontSize_Medium = 33;

        public const int TitleFontSize_Small = 20;
        public const int TitleAttFontSize_Small = 24;

        public const int TitleFontSize_Micro = 19;
        public const int TitleAttFontSize_Micro = 22;


        public const string DefaultLangName = "English";
        public const string DefaultLangCode = "EN";

        //Preference Keys
        public const string ReportingEmailsKey = nameof(ReportingEmailsKey);
        public const string DebugTotalQuestionsKey = nameof(DebugTotalQuestionsKey);
        public const string DebugTotalTopicsKey = nameof(DebugTotalTopicsKey);
        public const string DebugQuestionsPerTopicKey = nameof(DebugQuestionsPerTopicKey);
        public const string DebugAllowUnansweredQuestionsKey = nameof(DebugAllowUnansweredQuestionsKey);
        public const string DebugTotalUnansweredQuestionsKey = nameof(DebugTotalUnansweredQuestionsKey);
    }
}
