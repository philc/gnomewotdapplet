///<remarks>
///Author: Phil Crosby
///</remarks>
using System;

namespace org.gnome.Applets.GnomeWotdApplet{
	///<summary>
	///Interval periods over which word cycles can occur.
	///</summary>
	public enum Period{
		Day = 86400,
		Hour = 3600,
		Minute = 60,
		Second = 1
	}
	
	///<summary>
	///Preferences for the application.
	///</summary>
	public class Preferences{
		private static string GConfPath="/apps/gnome-wotdapplet/";
		
		#region Default preference values
		private static Period defaultPeriod=Period.Day;
		private static int defaultFrequency=1;
		private static bool defaultRetrieveFromUrl=true;
		private static string defaultDictionaryUrl=UrlWordGenerator.DefaultDictionaryUrl;
		private static string defaultDictionaryPath="";	//TODO: change this to where our dist dict will be
		private static string [] defaultVisitedWords = new string[0];
		private static int defaultDictionaryIndex=0;	
		#endregion
	
		Period period;
		int frequency;
		// Indicates whether we should be retrieving from the url
		bool retrieveFromUrl;
		string dictionaryUrl;
		string dictionaryPath;
		// Words we've already visited
		string [] visitedWords;
		// number of stored dictionary to use, as displayed in the preference dialog Option Menu
		int dictionaryIndex;
	
		#region Constructors
		private Preferences(){
		}
		#endregion
				
		///<summary>
		///Get an instance of the preferences from the GConf store.
		///The keys will be created if they don't exist.
		///</summary>
		public static Preferences FromGConfStore()
		{
			Preferences prefs = new Preferences();
			prefs.PopulateFromStore();							
			
			return prefs;
		}
		///<summary>
		///Saves this set of preferences into the GConf store.
		///</summary>
		public void SaveToGConfStore(){
			GConf.Client client = new GConf.Client();
			client.Set(GConfPath + "period",this.period.ToString());
			client.Set(GConfPath + "frequency",this.frequency);
			client.Set(GConfPath + "retrieveFromUrl", this.retrieveFromUrl);
			client.Set(GConfPath + "dictionaryUrl",this.dictionaryUrl);
			client.Set(GConfPath + "dictionaryPath",this.dictionaryPath);
			
			// Set the visitedWords key
			System.Text.StringBuilder words = 
				new System.Text.StringBuilder(visitedWords.Length*8);
			// Build a key with all of the visited words delimited by ";", except
			// the last word.
			for (int i=0, j=this.visitedWords.Length; i<j; i++){
				words.Append(visitedWords[i]);
				if (i<j-1)
					words.Append(";");
			}
			client.Set(GConfPath + "visitedWords",words.ToString());	
			
			client.Set(GConfPath + "dictionaryIndex",this.dictionaryIndex);		
		}
		
		///<summary>
		///Populates this preference object from a store.
		///</summary>
		private void PopulateFromStore()
		{
			GConf.Client client = new GConf.Client();

			// Each task below performs input validation		
			this.period=GetGConfInterval(client);
			this.frequency=GetGConfFrequency(client);
			this.retrieveFromUrl=GetGConfRetrieveFromUrl(client);
			this.dictionaryUrl = GetGConfDictionaryUrl(client);
			this.dictionaryPath = GetGConfDictionaryPath(client);
			this.visitedWords = GetGConfVisitedWords(client);
			this.dictionaryIndex = GetGConfDictionaryIndex(client);
		}
		private static Period GetGConfInterval(GConf.Client client)
		{
			Period gconfPeriod=defaultPeriod;
			Exception ex=null;
			try{
				// Try and load user's period
				string gconfValue = (string) client.Get(GConfPath + "period");
				gconfPeriod = (Period)System.Enum.Parse(typeof(Period),
					gconfValue, /*case sensitive?*/ false);
			}catch(GConf.NoSuchKeyException e){
				ex=e;				
			}catch(ArgumentException e){
				ex=e;				
			}catch(InvalidCastException e){
				ex=e;
			}
			finally{
				// if user's gconf key wasn't a valid period/didn't
				// exist, set it to a default.
				if (ex!=null)
					client.Set(GConfPath + "period",defaultPeriod.ToString());
			}
			return gconfPeriod;
		}
		private static int GetGConfFrequency(GConf.Client client)
		{
			int gconfFrequency=defaultFrequency;
			Exception ex=null;
			try{
				gconfFrequency = (int) client.Get(GConfPath + "frequency");
			}catch (GConf.NoSuchKeyException e){
				ex=e;
			}catch(ArgumentException e){
				ex=e;
			}catch(InvalidCastException e){
				ex=e;
			}			
			finally{
				// Can't have a frequency <1
				if (ex!=null || gconfFrequency<1)
					client.Set(GConfPath + "frequency",defaultFrequency);
			}
			return gconfFrequency;
		}
		private static bool GetGConfRetrieveFromUrl(GConf.Client client)
		{
			bool gconfRetrieveFromUrl=defaultRetrieveFromUrl;
			Exception ex = null;
			try{
				gconfRetrieveFromUrl = (bool)client.Get(GConfPath+"retrieveFromUrl");
			}
			catch(GConf.NoSuchKeyException e){
				ex=e;
			}catch(ArgumentException e){
				ex=e;
			}catch(InvalidCastException e){
				ex=e;
			}
			finally{
				if (ex!=null)
					client.Set(GConfPath + "retrieveFromUrl",defaultRetrieveFromUrl);
			}
			return gconfRetrieveFromUrl;
		}
		private static string GetGConfDictionaryUrl(GConf.Client client)
		{
			string gconfDictionaryUrl = defaultDictionaryUrl;
			Exception ex = null;
			try{
				gconfDictionaryUrl =(string) client.Get(GConfPath+"dictionaryUrl");
			}
			catch(GConf.NoSuchKeyException e){
				ex=e;
			}catch(InvalidCastException e){
				ex=e;
			}
			finally{
				if (ex!=null)
					client.Set(GConfPath + "dictionaryUrl",defaultDictionaryUrl);
			}
			return gconfDictionaryUrl;
		}
		private static string GetGConfDictionaryPath(GConf.Client client)
		{
			string gconfDictionaryPath = defaultDictionaryPath;
			Exception ex=null;
			try {
				gconfDictionaryPath = (string) client.Get(GConfPath + "dictionaryPath");
			}
			catch(GConf.NoSuchKeyException e){
				ex=e;
			}catch(InvalidCastException e){
				ex=e;
			}
			finally{
				if (ex!=null)
					client.Set(GConfPath + "dictionaryPath",defaultDictionaryPath);
			}
			return gconfDictionaryPath;
		}				
		private static string [] GetGConfVisitedWords(GConf.Client client)
		{
			string [] gconfVisitedWords = defaultVisitedWords;
			Exception ex=null;
			try{
				string gconfValue = (string) client.Get(GConfPath + "visitedWords");
				gconfVisitedWords = gconfValue.Split(';');
			}catch(GConf.NoSuchKeyException e){
				ex=e;
			}catch(InvalidCastException e){
				ex=e;
			}
			finally{
				if (ex!=null)
					client.Set(GConfPath+ "visitedWords","");
			}
			return gconfVisitedWords;
		}
		private static int GetGConfDictionaryIndex(GConf.Client client)
		{
			int gconfDictionaryIndex = defaultDictionaryIndex;
			Exception ex=null;
			try{
				gconfDictionaryIndex = (int) client.Get(GConfPath + "dictionaryIndex");
			}
			catch(GConf.NoSuchKeyException e){
				ex=e;
			}
			catch(InvalidCastException e){
				ex=e;
			}
			finally{
				if (ex!=null)
					client.Set(GConfPath + "dictionaryIndex",defaultDictionaryIndex);
			}
			return gconfDictionaryIndex;
		}
		
		///<summary>
		///Indicates the period over which the frequency occurs.
		///</summary>
		public Period Period{
			get{
				return this.period;
			}
			set{
				this.period=value;
			}
		}
		///<summary>
		///The number of times per period the word is advanced.
		///</summary>
		public int Frequency{
			get{
				return this.frequency;
			}
			set{
				if (frequency<1)
					throw new ArgumentException("Frequency cannot be less than 1.");
				this.frequency=value;
			}		
		}
		///<summary>
		///Indicates whether the word should be retrieved from a URL.
		///</summary>
		public bool RetrieveFromUrl{
			get{
				return this.retrieveFromUrl;
			}
			set{
				this.retrieveFromUrl=value;
			}				
		}
		public int CycleTime{
			get{
				return frequency * (int)period;
			}
		}
		///<summary>
		///The URL of the online dictionary.
		///</summary>
		public string DictionaryUrl{
			get{
				return this.dictionaryUrl;
			}
			set{
				this.dictionaryUrl=value;
			}
		}
		///<summary>
		///The path of the on-disk dictionary.
		///</summary>
		public string DictionaryPath{
			get {
				return this.dictionaryPath;
			}
			set{
				this.dictionaryPath=value;
			}
		}
		///<summary>
		///The words in the dictionary that have already been visited.
		///</summary>
		public string[] VisitedWords{
			get {
				return this.visitedWords;
			}
			set {
				this.visitedWords=value;
			}
		}
		///<summary>
		///Number of stored dictionary to use, as displayed in the preference dialog's OptionMenu
		///</summary>
		public int DictionaryIndex{
			get {
				return this.dictionaryIndex;
			}
			set {
				this.dictionaryIndex=value;
			}
		}
	}
}