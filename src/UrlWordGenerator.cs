///<remarks>
///Author: Phil Crosby
///</remarks>
using System.Collections;
using System;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Threading;

namespace org.gnome.Applets.GnomeWotdApplet{

	/// <remarks>
	/// A WordGenerator that retrieves words from a URL. If we fail to resolve the
	/// url in any way, CurrentWord will be null.
	/// </remarks>
	// Warning: this class obtains words from remote sources by parsing their HTML
	// pages (unless they have RSS XML feeds) which is a fragile process and is
	// prone to break as the websites themselves change.
	public class UrlWordGenerator : IWordGenerator{
		private static readonly string dictionaryComUrl=@"http://dictionary.reference.com/wordoftheday/";
		private static readonly string merriamWebsterUrl=@"http://www.m-w.com/cgi-bin/mwwod.pl";
		private static readonly string defaultDictionaryUrl=dictionaryComUrl;
		
		// Amount of seconds we wait for the word retrieval.
		private static readonly int timeout = 30;
		
		private static int secondsInADay=86400;
		private static int secondsInAnHour=secondsInADay/24;
				
		// Instance members
		Entry currentWord=null;
		/// <remarks>
		/// This field indicates the time between our checks to the WOTD
		/// server defined by URL.
		/// </rmarks>
		long cycleTime = 3600;
		String url;
		Timer timer;
		WebRequest request;
		public event WordChangedEventHandler WordChangedEvent;
		
		///<summary>
		///Construct a WordGenerator from dictionary.com's
		///word of the day url.
		///</summary>
		public UrlWordGenerator() : this(defaultDictionaryUrl)
		{			
		}
		///<summary />Construct a WordGenerator from a URL source
		///<param name="url" />The url to parse the WOTD from. 
		public UrlWordGenerator(String url)
		{
			// We have to parse the URL, so we only support URLs we understand
			if (url!=DictionaryComUrl && url!=MerriamWebsterUrl)
				throw new ArgumentException(String.Format("{0} is not a supported URL.",url));
			this.url=url;
			request = WebRequest.Create(url);
			request.Timeout=1;
		}
		///<summary>
		///Extract a definition from the dictionary.com wotd page.
		///</summary>
		///<returns />A dictionary entry constructed from the webpage
		private static Entry ParseDictionaryComPage(String htmlPage)
		{
			/* The target html looks like:
			   <h2>Word of the Day for Monday June 21, 2004</h2>
			   <p>
			   <span style="font-size: large; font-weight: bold;">deipnosophist</span> \dyp-NOS-uh-fist\, <i>noun</i>:<br>
			   <!-- wotd="deipnosophist" -->
			   Someone who is skilled in table talk.
			   </p>
			*/
			
			int pos, pos2, pos3;

			// Parse article type		
			pos=htmlPage.IndexOf("Word of the Day for");
			pos2 = htmlPage.IndexOf("<i>",pos)+3;
			pos3 = htmlPage.IndexOf("</i>");
			string article = htmlPage.Substring(pos2,pos3-pos2);
			
			// Parse word
			pos = htmlPage.IndexOf("WOTD=\"",pos3) + 6;
			pos2 = htmlPage.IndexOf("\"",pos);
			string word = htmlPage.Substring(pos,pos2-pos);
			
			// Parse definition
			pos = htmlPage.IndexOf("-->\n",pos2) + 4;
			pos2 = htmlPage.IndexOf("</p>",pos);
			string definition = htmlPage.Substring(pos,pos2-pos);
			definition=definition.Replace("<br>","\n").Replace("<b>","").Replace("</b>","").Trim();
			
			return new Entry(word, new Definition(article, definition));
		}
		/// <summary>
		/// Extract a definition from the merriam-webster wotd page.
		/// </summary>
		/// <returns />A dictionary entry constructed from the webpage.
		private static Entry ParseMerriamWebsterPage(String htmlPage)
		{
			/* Target HTMl looks like:
			   The Word of the Day for July 8 is<b>:</b>
			   </font></b><p><b>
			   lackadaisical</b> &#149; \lak-uh-DAY-zih-kul\&nbsp;<a href="javascript:popWin('/cgi-bin/audio.pl?lackad01.wav=lackadaisical')"><img src="/images/audio.gif" border=0 height=11 width=16 alt="Audio icon"></a> &#149; <i>adjective
			   </i>
			   <br>
     		   <b>:</b> lacking life, spirit, or zest <b>:</b> languid
			   </p><p><b>Example sentence<b>:</b><br></b>
			*/
			// Parse the word
			
			//Console.WriteLine(htmlPage);	//TODO remove
			int pos1, pos2, pos3;
			pos1=htmlPage.IndexOf("The Word of the Day for");
			pos2=htmlPage.IndexOf("<p><b>\n",pos1) + 6;
			pos3=htmlPage.IndexOf("</b>",pos2);
			string word = htmlPage.Substring(pos2,pos3-pos2).Trim();
			
			// Parse the article			
			pos1=htmlPage.IndexOf("<i>",pos3) + 3;
			pos2=htmlPage.IndexOf("\n</i>",pos1);
			string article = htmlPage.Substring(pos1,pos2-pos1);
			
			// Parse the definition
			pos1=htmlPage.IndexOf("<br>",pos2)+4;
			pos2=htmlPage.IndexOf("</p>",pos1);
			string definition = htmlPage.Substring(pos1,pos2-pos1);
			// Do not need to replace <br> with \n in this particular page.
			definition=definition.Replace("<b>","").Replace("</b>","");
			definition=definition.Replace("<br>","").Replace("*","").Trim();					
			
			return new Entry(word, new Definition(article, definition));						
		}
		///<see cref="IWordGenerator.MoveNext">
		public void MoveNext()
		{
			Debug.WriteLine("Beginning UrlWordGenerator web request.");
			// Begin a web request; all of the event firing logic resides in the
			// callback for that response.
			try{			
				// The callback will handle firing the WordChangedEvent
				request.BeginGetResponse(new AsyncCallback(this.WebResponseCallback),request);								
			}catch(WebException e){
				System.Console.WriteLine(e);	//TODO: remove
				FireErrorEvent(String.Format("Could not connect to \"{0}\"",this.url));
			}
			// Set the timer / check the word on the website every hour.
			if (timer==null)
				timer = new Timer(new TimerCallback(this.TimerCallback),null,secondsInAnHour*1000,secondsInAnHour*1000);
		}
		/// <summary>
		/// Fire an updated word event indicating we had an error.
		/// </summary>
		/// <param name="errorMessage" />Error message to send with the event.
		private void FireErrorEvent(string errorMessage)
		{
			// Fire a word event, but indicate we couldn't get the word
			WordChangedEventArgs args = new WordChangedEventArgs(this.CurrentWord,
				"Could not obtain word of the day from the online source!");
			OnWordChangedEvent(args);
		}
		/// <summary>
		/// WordChangedEvent issuing method.
		/// </summary>
		protected void OnWordChangedEvent(WordChangedEventArgs e){
			if (WordChangedEvent!=null){
				WordChangedEvent(this,e);
			}
		}
		/// <summary>
		/// Callback for the webresponse when it has successfully reached
		/// the website. Parses the site's page and fires appropriate event.
		/// </summary>
		public void WebResponseCallback(IAsyncResult result)
		{
			Debug.WriteLine("Received result from the web response.");
			Entry originalWord = this.CurrentWord;
			try{				
				WebResponse response = ((WebRequest)result.AsyncState).EndGetResponse(result);
				string htmlPage = (new StreamReader(response.GetResponseStream())).ReadToEnd();

				// We can only parse URLs we know about...
				if (this.url==DictionaryComUrl)
					this.currentWord=ParseDictionaryComPage(htmlPage);
				else if (this.url==MerriamWebsterUrl)
					this.currentWord=ParseMerriamWebsterPage(htmlPage);
				else{
					FireErrorEvent(String.Format("Don't know how to parse \"{0}\"",this.url));
				}
			}catch(WebException e){
				System.Console.WriteLine("here" + e); //TODO remove
				FireErrorEvent(String.Format("Could not connect to \"{0}\"",this.url));
			}catch(Exception e){
				//TODO catch parsing exceptions, not "Exception"
				Console.WriteLine(e);
			}finally{
				// Only fire an updated word event if the word really got updated.
				// In the case of an error, the word will be the same as it was
				// previously, so this will not fire if above errors fired.	
				if (currentWord!=null && currentWord.CompareTo(originalWord)!=0)
					OnWordChangedEvent(new WordChangedEventArgs(this.CurrentWord));	
			}

		}
		/// <summary>
		/// Timer callback used to respond to timer notifications.
		/// </summary>
		public void TimerCallback(Object state){
			MoveNext();
		}
		
		#region Properties
		///<see cref="IWordGenerator.CurrentWord">
		public Entry CurrentWord
		{
			get {
				return currentWord;
			}
		}
		///<see cref="IWordGenerator.CycleTime">
		public long CycleTime
		{
			get{
				return cycleTime;
				}
			set{
				cycleTime=value;
			}
		}
		/// <see cref="IWordGenerator.Dictionary">
		public Dictionary Dictionary
		{
			get {
				IList list = new ArrayList(1);
				list.Add(this.CurrentWord.Clone());
				return org.gnome.Applets.GnomeWotdApplet.Dictionary.FromEntries(list);
			}
			set { /*do nothing*/ }
		}
		/// <summary>
		/// The URL of Dictionary.com's WOTD service.
		/// </summary>
		public static string DictionaryComUrl{
			get{
				return dictionaryComUrl;
			}
		}
		/// <summary>
		/// The URL of Merriam-Webster's WOTD service.
		/// </summary>
		public static string MerriamWebsterUrl{
			get{
				return merriamWebsterUrl;
			}
		}
		/// <summary>
		/// The dictionary website's URL accessed by default.
		/// </summary>
		public static string DefaultDictionaryUrl{
			get{
				return defaultDictionaryUrl;
			}
		}
		#endregion
	}

}