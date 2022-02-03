///<remarks>
///Author:Phil Crosby
///</remarks>

using System;

namespace org.gnome.Applets.GnomeWotdApplet{

	///<remarks>Event arguments used in conjunction with WordChanged
	///events; allows accessed to the new word.
	///</remarks>
	public class WordChangedEventArgs : EventArgs{		
		Entry word;
		string errorMessage=null;
		
		
		#region Constructors
		public WordChangedEventArgs(Entry word){
			this.word=word;
		}
		#endregion
		/// <summary>
		/// Construct a word changed event argument indicating
		/// that there was an error retrieving the word.
		/// </summary>
		public WordChangedEventArgs(Entry word, string errorMessage)
		{
			this.word=word;
			this.errorMessage=errorMessage;
		}
		///<summary />The new word	
		public Entry Word{
			get { return word; }
		}
		///<summary />Determines whether this message was an error.
		public bool IsError{
			get { return (errorMessage!=null); }
		}
		/// <summary>
		/// Retrieves the error message associated with this event, if
		/// there was an error retrieving the word.
		/// </summary>
		public string ErrorMessage{
			get { return errorMessage; }
		}
			
	}

}