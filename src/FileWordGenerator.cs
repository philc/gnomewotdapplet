/// <remarks>
/// Author: Phil Crosby
/// </remarks>

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace org.gnome.Applets.GnomeWotdApplet{

	/// <summary>
	/// A WordGenerator that generates words from a dictionary file.
	/// </summary>
	public class FileWordGenerator : IWordGenerator{
		private static int secondsInADay=86400;
		// Instance members
		private string path=null;
		private long cycleTime;
		private Timer timer;
		public event WordChangedEventHandler WordChangedEvent;
		
		private Dictionary dictionary;
		IEnumerator currentWord;
		
		//The list of words that have already been accessed.
		private SortedList visitedWords = new SortedList();	
		
		//Constructors
		/// <summary />Construct a new FileWordGenerator from a file.
		/// <param name="path" />The path of the file.
		public FileWordGenerator(String path) : this(path, secondsInADay){
		}
		/// <summary />Construct a new FileWordGenerator from a file.
		/// <param name="path" />The path of the file.
		/// <param name="cycleTime" />The seconds between word changes.
		public FileWordGenerator(String path, int cycleTime){
			if (cycleTime<1)
				throw new ArgumentException("Cycle time cannot be null.");
			this.path=path;
			dictionary=Dictionary.FromFile(path);
			if (dictionary.Entries.Count<=0)
				throw new ArgumentException(String.Format("Dictionary {0} is empty.",dictionary));
			dictionary.Shuffle();
			//currentWord=dictionary.Entries.GetEnumerator();
			
			//Give timer cycletime*1000 because it receives ticks, not seconds.
			//A second is 1000 ticks.
			this.cycleTime = cycleTime;
			timer = new Timer(new TimerCallback(TimerCallback),null,cycleTime*1000,cycleTime*1000);
			
			//AdvanceWord();
		}
		
		/// <see cref="IWordGenerator.Save">
		public void Save(string path)
		{
			this.dictionary.Save(path);
		}
			
		public void Dispose(){
			timer.Dispose();
		}
		/// <see cref="IWordGenerator.MoveNext">
		public void MoveNext(){
			//Reset timer, advance word
			timer.Change(cycleTime*1000,cycleTime*1000);				
			AdvanceWord();
			
			//Fire event, notifying listeners that there's a new word
			OnWordChangedEvent(new WordChangedEventArgs(this.CurrentWord));
		}
		/// <summary>
		/// Advances to the next word in the dictionary. If we've already
		/// visited all of the words in this dictionary, clear the visited 
		/// words list and start over.
		/// </summary>
		private void AdvanceWord(){
			Entry oldWord=null;
			if (dictionary.Entries.Count<=0)
				return;
			else if (currentWord==null)
				currentWord=dictionary.Entries.GetEnumerator();
			else
				// Before we possibly invalidate the enumerator, save its value
				oldWord = currentWord.Current as Entry;
			
			bool result=currentWord.MoveNext();
			if (result){
				//Update our "visited" list
				String word = ((Entry)currentWord.Current).Word.ToLower();
				Debug.Assert(word!=null);
				//If we've already visited this word, move on to the next word.
				if (visitedWords.ContainsKey(word))
					AdvanceWord();
				else
					visitedWords.Add(word,null);
			}else{
				//We have exhausted our list of words; start over.
				visitedWords.Clear();
				dictionary.Shuffle();
				// Make sure our next word isn't the same as our current word.
				if (dictionary.Entries.Count>1 && oldWord.Equals(dictionary.Entries[0])){
					// Swap first and last word of the shuffled dictionary
					Entry tmp = (Entry)dictionary.Entries[0];
					dictionary.Entries[0]=dictionary.Entries[dictionary.Entries.Count-1];
					dictionary.Entries[dictionary.Entries.Count-1]=tmp;
				}					
				currentWord=null;
				AdvanceWord();
			}
		}
		/// <summary>
		/// Issues a WordChangedEvent.
		/// </summary>
		protected void OnWordChangedEvent(WordChangedEventArgs e){
			if (WordChangedEvent!=null){
				WordChangedEvent(this,e);
			}
		}
		/// <summary>
		/// Timer callback used to respond to timer notifications.
		/// </summary>
		public void TimerCallback(Object state){
			MoveNext();
		}
		
		#region Properties	
		///<see cref="IWordGenerator.CycleTime">
		public long CycleTime{
			get{ return cycleTime; }
			set{ 
				cycleTime=value; 
				//Move on to the next word immediately, so the user can't continually
				//reset the time by changing it and never see the next word.
				MoveNext(); 
			}
		}
		///<see cref="IWordGenerator.CurrentWord">
		public Entry CurrentWord
		{
			get { 
				return (currentWord==null) ? null : currentWord.Current as Entry;
			}
		}
		///<see cref="IWordGenerator.Dictionary">
		public Dictionary Dictionary{
			get { return dictionary.Clone() as Dictionary; }
			set {
				if (dictionary==null || dictionary.Entries.Count>0)
					throw new ArgumentException("A non-empty dictionary is required.");
				dictionary = value;
				dictionary.Shuffle();
				// Make sure our next word isn't the same as our current word.
				if (dictionary.Entries.Count>1 && currentWord.Equals(dictionary.Entries[1])){
					// Swap first and last word of the shuffled dictionary
					Entry tmp = (Entry)dictionary.Entries[0];
					dictionary.Entries[0]=dictionary.Entries[dictionary.Entries.Count-1];
					dictionary.Entries[dictionary.Entries.Count-1]=tmp;
				}
				currentWord=null;
				AdvanceWord();
			}
		}
		/// <summary>
		/// Set the dictionary's path that this generator should use.
		/// Loads the dictionary and clears the visited words if
		/// different from the current path.
		/// </summary>
		public string DictionaryPath
		{
			get{
				return this.path;
			}
			set{
				if (value==this.path)
					return;
				this.Dictionary = Dictionary.FromFile(value);
				this.path=value;
				this.VisitedWords=null;				
			}
				
		}
		/// <summary>
		/// Obtain the words visited by this word generator.
		/// </summary>
		/// <returns />String array of visited words.
		public string[] VisitedWords
		{
			get { 
				string [] result = new string[visitedWords.Keys.Count];
				this.visitedWords.Keys.CopyTo(result,0);
				return result;
			}
			set {
				this.visitedWords.Clear();
				if (value!=null){
					foreach (string str in value)
						if (str.Trim()!="")
							this.visitedWords.Add(str,null);
				}
				// Add the current entry, so we don't override/lose track of it
				if (this.CurrentWord!=null){
					if (visitedWords.Contains(this.CurrentWord.Word))
						MoveNext();
					else
						this.visitedWords.Add(this.CurrentWord.Word,null);
				}
			}
		}
		#endregion
	}

}//end namespace