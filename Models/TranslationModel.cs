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
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Translated name for this language.
    /// </summary>
    /// <example>Espanol</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Direction this language is read in.  Options are
    /// (default)LR, RL, TD
    /// </summary>
    /// <example>LR</example>
    public string LanguageTextDirection { get; set; } = string.Empty;

    /// <summary>
    /// If the language requires a special font it must be bundled then named here
    /// (default)LR, RL, TD
    /// </summary>
    /// <example>Arabic200</example>
    public string LanguageFont { get; set; } = string.Empty;

    public string NavButtonBegin { get; set; } = string.Empty;
    public string GotIt { get; set; } = string.Empty;
    public string NavButtonContinue { get; set; } = string.Empty;
    public string NavButtonFinish { get; set; } = string.Empty;
    public string CheckForUpdates { get; set; } = string.Empty;
	public string CreatedBy { get; set; } = string.Empty;
	public string RevLong { get; set; } = string.Empty;
    public string RevLongEmail { get; set; } = string.Empty;
	public string DevelopedBy { get; set; } = string.Empty;
	public string RevSmith { get; set; } = string.Empty;
    public string RevSmithEmail { get; set; } = string.Empty;
    public string AppsmithsLLC { get; set; } = string.Empty;
	public string SelectLanguage { get; set; } = string.Empty;
	public string Page { get; set; } = string.Empty;
	public string Of { get; set; } = string.Empty;
	public string SwipeTo { get; set; } = string.Empty;
	public string Navigate { get; set; } = string.Empty;
    public string Instructable { get; set; } = string.Empty;
    public string Loading { get; set; } = string.Empty;
    public string TapTo { get; set; } = string.Empty;
    public string Quit { get; set; } = string.Empty;
    public string AreYouSure { get; set; } = string.Empty;
    public string Yes { get; set; } = string.Empty;
    public string No { get; set; } = string.Empty;
    public string ParallelMode { get; set; } = string.Empty;
    public string OnlyAvailableOnTablets { get; set; } = string.Empty;
    public string StudentLanguage { get; set; } = string.Empty;
    public string TeacherLanguage { get; set; } = string.Empty;
    public string Settings { get; set; } = string.Empty;

    public string SpiritualGiftsTestTitle { get; set; } = string.Empty;
    public string BibleStudyTitle { get; set; } = string.Empty;
    public string Acts412 { get; set; } = string.Empty;
    public string Acts412v { get; set; } = string.Empty;

    public string HowManyGospels { get; set; } = string.Empty;
    public string Gal18 { get; set; } = string.Empty;
    public string Gal18v { get; set; } = string.Empty;
    public string Gal19 { get; set; } = string.Empty;
    public string Gal19v { get; set; } = string.Empty;

    public string OneGospel { get; set; } = string.Empty;
    public string ItWasPreachedTo { get; set; } = string.Empty;
    public string PreachedToList { get; set; } = string.Empty;

    public string WhatIsTheGospel { get; set; } = string.Empty;
    public string GoodNews { get; set; } = string.Empty;
		public string GospelAppliedTitle { get; set; } = string.Empty;
    public string GospelApplied1 { get; set; } = string.Empty;
    public string Death { get; set; } = string.Empty;
	public string DeathDetail { get; set; } = string.Empty;
	public string Burial { get; set; } = string.Empty;
	public string BurialDetail { get; set; } = string.Empty;
	public string Resurrection { get; set; } = string.Empty;
	public string ResurrectionDetail { get; set; } = string.Empty;

    public string WhyTwoBaptisms { get; set; } = string.Empty;
    public string TwoBaptismsTopic1 { get; set; } = string.Empty;
    public string Rom64 { get; set; } = string.Empty;
    public string Rom64v { get; set; } = string.Empty;
    public string TwoBaptismsDetail1 { get; set; } = string.Empty;
    public string TwoBaptismsTopic2 { get; set; } = string.Empty;
    public string Col127 { get; set; } = string.Empty;
    public string Col127v { get; set; } = string.Empty;

    public string BibleSays { get; set; } = string.Empty;
	public string KeyWords { get; set; } = string.Empty;
    public string BibleSaysKeyWords { get; set; } = string.Empty;
    public string Mark1615 { get; set; } = string.Empty;
    public string Mark1616 { get; set; } = string.Empty;
    public string Mark1617 { get; set; } = string.Empty;
    public string Mark1615v { get; set; } = string.Empty;
    public string Mark1616v { get; set; } = string.Empty;
    public string Mark1617v { get; set; } = string.Empty;

    public string KeyWordsRepentBaptism { get; set; } = string.Empty;
    public string Luke2446 { get; set; } = string.Empty;
    public string Luke2447 { get; set; } = string.Empty;
    public string Luke2448 { get; set; } = string.Empty;
    public string Luke2449 { get; set; } = string.Empty;
    public string Luke2446v { get; set; } = string.Empty;
    public string Luke2447v { get; set; } = string.Empty;
    public string Luke2448v { get; set; } = string.Empty;
    public string Luke2449v { get; set; } = string.Empty;

    public string Beginning { get; set; } = string.Empty;
    public string BeginningKeyWords { get; set; } = string.Empty;
    public string Acts236 { get; set; } = string.Empty;
    public string Acts237 { get; set; } = string.Empty;
    public string Acts238 { get; set; } = string.Empty;
    public string Acts239 { get; set; } = string.Empty;
    public string Acts236v { get; set; } = string.Empty;
    public string Acts237v { get; set; } = string.Empty;
    public string Acts238v { get; set; } = string.Empty;
    public string Acts239v { get; set; } = string.Empty;

    public string Gentiles { get; set; } = string.Empty;
    public string GentilesKeyWords { get; set; } = string.Empty;
    public string Acts1043 { get; set; } = string.Empty;
    public string Acts1044 { get; set; } = string.Empty;
    public string Acts1045 { get; set; } = string.Empty;
    public string Acts1046 { get; set; } = string.Empty;
    public string Acts1047 { get; set; } = string.Empty;
    public string Acts1048 { get; set; } = string.Empty;
    public string Acts1043v { get; set; } = string.Empty;
    public string Acts1044v { get; set; } = string.Empty;
    public string Acts1045v { get; set; } = string.Empty;
    public string Acts1046v { get; set; } = string.Empty;
    public string Acts1047v { get; set; } = string.Empty;
    public string Acts1048v { get; set; } = string.Empty;

    public string BelieveKeyWords { get; set; } = string.Empty;
    public string Acts191 { get; set; } = string.Empty;
    public string Acts192 { get; set; } = string.Empty;
    public string Acts193 { get; set; } = string.Empty;
    public string Acts194 { get; set; } = string.Empty;
    public string Acts195 { get; set; } = string.Empty;
    public string Acts196 { get; set; } = string.Empty;
    public string Acts191v { get; set; } = string.Empty;
    public string Acts192v { get; set; } = string.Empty;
    public string Acts193v { get; set; } = string.Empty;
    public string Acts194v { get; set; } = string.Empty;
    public string Acts195v { get; set; } = string.Empty;
    public string Acts196v { get; set; } = string.Empty;

    public string Samaritans { get; set; } = string.Empty;
    public string SamaritansKeyWords { get; set; } = string.Empty;
    public string Acts85 { get; set; } = string.Empty;
    public string Acts812 { get; set; } = string.Empty;
    public string Acts813 { get; set; } = string.Empty;
    public string Acts814 { get; set; } = string.Empty;
    public string Acts815 { get; set; } = string.Empty;
    public string Acts816 { get; set; } = string.Empty;
    public string Acts817 { get; set; } = string.Empty;
    public string Acts85v { get; set; } = string.Empty;
    public string Acts812v { get; set; } = string.Empty;
    public string Acts813v { get; set; } = string.Empty;
    public string Acts814v { get; set; } = string.Empty;
    public string Acts815v { get; set; } = string.Empty;
    public string Acts816v { get; set; } = string.Empty;
    public string Acts817v { get; set; } = string.Empty;

    public string Acts { get; set; } = string.Empty;
    public string History { get; set; } = string.Empty;
	public string Acts2 { get; set; } = string.Empty;
	public string Acts2Detail { get; set; } = string.Empty;
	public string Acts8 { get; set; } = string.Empty;
	public string Acts8Detail { get; set; } = string.Empty;
	public string Acts10 { get; set; } = string.Empty;
	public string Acts10Detail { get; set; } = string.Empty;
	public string Acts19 { get; set; } = string.Empty;
    public string Acts19Detail { get; set; } = string.Empty;

    public string Remission { get; set; } = string.Empty;
    public string RemissionTopic1 { get; set; } = string.Empty;
    public string RemissionTopic2 { get; set; } = string.Empty;
    public string Luke177 { get; set; } = string.Empty;
    public string Luke177v { get; set; } = string.Empty;
    public string Acts238vRemission { get; set; } = string.Empty;

    public string Baptized { get; set; } = string.Empty;
    public string Acts238vBaptized { get; set; } = string.Empty;
    public string Acts816vBaptized { get; set; } = string.Empty;
    public string Luke2447vBaptized { get; set; } = string.Empty;

    public string MoreScriptures { get; set; } = string.Empty;
    public string Acts1048vBaptized { get; set; } = string.Empty;
    public string Acts195vBaptized { get; set; } = string.Empty;
    public string Acts2216 { get; set; } = string.Empty;
    public string Romans63 { get; set; } = string.Empty;
    public string Romans64 { get; set; } = string.Empty;
    public string Acts2216v { get; set; } = string.Empty;
    public string Romans63v { get; set; } = string.Empty;
    public string Romans64v { get; set; } = string.Empty;

    public string WhatAboutMatthew2819 { get; set; } = string.Empty;
    public string Matt2819 { get; set; } = string.Empty;
    public string Matt2819v { get; set; } = string.Empty;
    public string Matt2819Explained { get; set; } = string.Empty;

	public string HowManyTopic1 { get; set; } = string.Empty;
    public string None { get; set; } = string.Empty;
	public string HowManyTopic2 { get; set; } = string.Empty;
    public string EveryOne { get; set; } = string.Empty;
    public string Kingdom { get; set; } = string.Empty;
	public string KingdomDetail1 { get; set; } = string.Empty;
	public string Abba { get; set; } = string.Empty;
	public string AbbaDetail { get; set; } = string.Empty;
	public string Father { get; set; } = string.Empty;
    public string FatherDetail { get; set; } = string.Empty;
    public string KingdomDetail2 { get; set; } = string.Empty;
    public string Acts812vKingdom { get; set; } = string.Empty;

    public string John33 { get; set; } = string.Empty;
    public string John35 { get; set; } = string.Empty;
    public string TwoPeter11011 { get; set; } = string.Empty;
    public string Gal46 { get; set; } = string.Empty;
    public string John33v { get; set; } = string.Empty;
    public string John35v { get; set; } = string.Empty;
    public string TwoPeter11011v { get; set; } = string.Empty;
    public string Gal46v { get; set; } = string.Empty;

    public string FamilyName { get; set; } = string.Empty;
    public string Gal327 { get; set; } = string.Empty;
    public string Romans65 { get; set; } = string.Empty;
    public string Eph314 { get; set; } = string.Empty;
    public string Eph315 { get; set; } = string.Empty;
    public string Acts412vName { get; set; } = string.Empty;
    public string Gal327v { get; set; } = string.Empty;
    public string Romans65v { get; set; } = string.Empty;
    public string Eph314v { get; set; } = string.Empty;
    public string Eph315v { get; set; } = string.Empty;

    public string GetFamilyName { get; set; } = string.Empty;

    public string ThreeQuestions { get; set; } = string.Empty;
    public string Question1 { get; set; } = string.Empty;
    public string Question2 { get; set; } = string.Empty;
    public string Question3 { get; set; } = string.Empty;
}
