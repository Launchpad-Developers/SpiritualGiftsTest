using SQLite;

namespace SpiritualGiftsTest.Models;

	
[Table("Translation")]
public class TranslationModel
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    /// <summary>
    /// English name for this language
    /// </summary>
		/// <example>Spanish</example>        
    public string Code { get; set; }

    /// <summary>
    /// Translated name for this language.
    /// </summary>
    /// <example>Espanol</example>
    public string Name { get; set; }

    /// <summary>
    /// Direction this language is read in.  Options are
    /// (default)LR, RL, TD
    /// </summary>
    /// <example>LR</example>
    public string LanguageTextDirection { get; set; }

    /// <summary>
    /// If the language requires a special font it must be bundled then named here
    /// (default)LR, RL, TD
    /// </summary>
    /// <example>Arabic200</example>
    public string LanguageFont { get; set; }

    public string NavButtonBegin { get; set; }
    public string GotIt { get; set; }
    public string NavButtonContinue { get; set; }
    public string NavButtonFinish { get; set; }
    public string CheckForUpdates { get; set; }
		public string CreatedBy { get; set; }
		public string RevLong { get; set; }
    public string RevLongEmail { get; set; }
		public string DevelopedBy { get; set; }
		public string RevSmith { get; set; }
    public string RevSmithEmail { get; set; }
    public string AppsmithsLLC { get; set; }
		public string SelectLanguage { get; set; }
		public string Page { get; set; }
		public string Of { get; set; }
		public string SwipeTo { get; set; }
		public string Navigate { get; set; }
    public string Instructable { get; set; }
    public string Loading { get; set; }
    public string TapTo { get; set; }
    public string Quit { get; set; }
    public string AreYouSure { get; set; }
    public string Yes { get; set; }
    public string No { get; set; }
    public string ParallelMode { get; set; }
    public string OnlyAvailableOnTablets { get; set; }
    public string StudentLanguage { get; set; }
    public string TeacherLanguage { get; set; }
    public string Settings { get; set; }

    public string SpiritualGiftsTestTitle { get; set; }
    public string BibleStudyTitle { get; set; }
    public string Acts412 { get; set; }
    public string Acts412v { get; set; }

    public string HowManyGospels { get; set; }
    public string Gal18 { get; set; }
    public string Gal18v { get; set; }
    public string Gal19 { get; set; }
    public string Gal19v { get; set; }

    public string OneGospel { get; set; }
    public string ItWasPreachedTo { get; set; }
    public string PreachedToList { get; set; }

    public string WhatIsTheGospel { get; set; }
    public string GoodNews { get; set; }
		public string GospelAppliedTitle { get; set; }
    public string GospelApplied1 { get; set; }
    public string Death { get; set; }
		public string DeathDetail { get; set; }
		public string Burial { get; set; }
		public string BurialDetail { get; set; }
		public string Resurrection { get; set; }
		public string ResurrectionDetail { get; set; }

    public string WhyTwoBaptisms { get; set; }
    public string TwoBaptismsTopic1 { get; set; }
    public string Rom64 { get; set; }
    public string Rom64v { get; set; }
    public string TwoBaptismsDetail1 { get; set; }
    public string TwoBaptismsTopic2 { get; set; }
    public string Col127 { get; set; }
    public string Col127v { get; set; }

    public string BibleSays { get; set; }
		public string KeyWords { get; set; }
    public string BibleSaysKeyWords { get; set; }
    public string Mark1615 { get; set; }
    public string Mark1616 { get; set; }
    public string Mark1617 { get; set; }
    public string Mark1615v { get; set; }
    public string Mark1616v { get; set; }
    public string Mark1617v { get; set; }

    public string KeyWordsRepentBaptism { get; set; }
    public string Luke2446 { get; set; }
    public string Luke2447 { get; set; }
    public string Luke2448 { get; set; }
    public string Luke2449 { get; set; }
    public string Luke2446v { get; set; }
    public string Luke2447v { get; set; }
    public string Luke2448v { get; set; }
    public string Luke2449v { get; set; }

    public string Beginning { get; set; }
    public string BeginningKeyWords { get; set; }
    public string Acts236 { get; set; }
    public string Acts237 { get; set; }
    public string Acts238 { get; set; }
    public string Acts239 { get; set; }
    public string Acts236v { get; set; }
    public string Acts237v { get; set; }
    public string Acts238v { get; set; }
    public string Acts239v { get; set; }

    public string Gentiles { get; set; }
    public string GentilesKeyWords { get; set; }
    public string Acts1043 { get; set; }
    public string Acts1044 { get; set; }
    public string Acts1045 { get; set; }
    public string Acts1046 { get; set; }
    public string Acts1047 { get; set; }
    public string Acts1048 { get; set; }
    public string Acts1043v { get; set; }
    public string Acts1044v { get; set; }
    public string Acts1045v { get; set; }
    public string Acts1046v { get; set; }
    public string Acts1047v { get; set; }
    public string Acts1048v { get; set; }

    public string BelieveKeyWords { get; set; }
    public string Acts191 { get; set; }
    public string Acts192 { get; set; }
    public string Acts193 { get; set; }
    public string Acts194 { get; set; }
    public string Acts195 { get; set; }
    public string Acts196 { get; set; }
    public string Acts191v { get; set; }
    public string Acts192v { get; set; }
    public string Acts193v { get; set; }
    public string Acts194v { get; set; }
    public string Acts195v { get; set; }
    public string Acts196v { get; set; }

    public string Samaritans { get; set; }
    public string SamaritansKeyWords { get; set; }
    public string Acts85 { get; set; }
    public string Acts812 { get; set; }
    public string Acts813 { get; set; }
    public string Acts814 { get; set; }
    public string Acts815 { get; set; }
    public string Acts816 { get; set; }
    public string Acts817 { get; set; }
    public string Acts85v { get; set; }
    public string Acts812v { get; set; }
    public string Acts813v { get; set; }
    public string Acts814v { get; set; }
    public string Acts815v { get; set; }
    public string Acts816v { get; set; }
    public string Acts817v { get; set; }

    public string Acts { get; set; }
    public string History { get; set; }
		public string Acts2 { get; set; }
		public string Acts2Detail { get; set; }
		public string Acts8 { get; set; }
		public string Acts8Detail { get; set; }
		public string Acts10 { get; set; }
		public string Acts10Detail { get; set; }
		public string Acts19 { get; set; }
    public string Acts19Detail { get; set; }

    public string Remission { get; set; }
    public string RemissionTopic1 { get; set; }
    public string RemissionTopic2 { get; set; }
    public string Luke177 { get; set; }
    public string Luke177v { get; set; }
    public string Acts238vRemission { get; set; }

    public string Baptized { get; set; }
    public string Acts238vBaptized { get; set; }
    public string Acts816vBaptized { get; set; }
    public string Luke2447vBaptized { get; set; }

    public string MoreScriptures { get; set; }
    public string Acts1048vBaptized { get; set; }
    public string Acts195vBaptized { get; set; }
    public string Acts2216 { get; set; }
    public string Romans63 { get; set; }
    public string Romans64 { get; set; }
    public string Acts2216v { get; set; }
    public string Romans63v { get; set; }
    public string Romans64v { get; set; }

    public string WhatAboutMatthew2819 { get; set; }
    public string Matt2819 { get; set; }
    public string Matt2819v { get; set; }
    public string Matt2819Explained { get; set; }

		public string HowManyTopic1 { get; set; }
    public string None { get; set; }
		public string HowManyTopic2 { get; set; }
    public string EveryOne { get; set; }
    public string Kingdom { get; set; }
		public string KingdomDetail1 { get; set; }
		public string Abba { get; set; }
		public string AbbaDetail { get; set; }
		public string Father { get; set; }
    public string FatherDetail { get; set; }
    public string KingdomDetail2 { get; set; }
    public string Acts812vKingdom { get; set; }

    public string John33 { get; set; }
    public string John35 { get; set; }
    public string TwoPeter11011 { get; set; }
    public string Gal46 { get; set; }
    public string John33v { get; set; }
    public string John35v { get; set; }
    public string TwoPeter11011v { get; set; }
    public string Gal46v { get; set; }

    public string FamilyName { get; set; }
    public string Gal327 { get; set; }
    public string Romans65 { get; set; }
    public string Eph314 { get; set; }
    public string Eph315 { get; set; }
    public string Acts412vName { get; set; }
    public string Gal327v { get; set; }
    public string Romans65v { get; set; }
    public string Eph314v { get; set; }
    public string Eph315v { get; set; }

    public string GetFamilyName { get; set; }

    public string ThreeQuestions { get; set; }
    public string Question1 { get; set; }
    public string Question2 { get; set; }
    public string Question3 { get; set; }
}
