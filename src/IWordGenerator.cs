/// <remarks>
/// Author: Phil Crosby
/// </remarks>

using System;
using System.Collections;

namespace org.gnome.Applets.GnomeWotdApplet{

	/// <summary />Delegate used for change of word events.
	public delegate void WordChangedEventHandler(Object sender, WordChangedEventArgs e);

	/// <remarks>
	/// A generator of words, which notifies any observers of a word 
	/// change, whether they be from a URL or a from a dictionary on file.
	/// After instantiating, call MoveNext() to circulate to the first word.
	/// </remarks>
	public interface IWordGenerator{		
		/// <summary />Event observers should listen to for word changes.
		event WordChangedEventHandler WordChangedEvent;
		
		/// <summary>
		/// Moves to the next word in the dictionary. Fires the WordChangedEvent
		/// when the move is complete.
		/// </summary>
		void MoveNext();		

		/// <summary />Retrieves the current word.
		Entry CurrentWord{
			get;
		}
		/// <summary>
		/// Cycle time, in seconds.
		/// </summary>
		/// <remarks>
		/// Changing the cycle time will immediately advance to the next
		/// word, and the new cycle time will come into effect for that word.
		/// </remarks>
		long CycleTime{
			get;
			set;
		}
		/// <summary />Collection of entries.
		Dictionary Dictionary{
			get;
			set;
		}
	}

}