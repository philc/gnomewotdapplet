/// <remarks>
/// Author: Michael Quinn
/// </remarks>
using System;
using Gtk;
using GtkSharp;

namespace org.gnome.Applets.GnomeWotdApplet{

	///<remarks>
	/// Class representing the display widget, consisting of a label which displays
	/// a new word according to a specific interval of time (set in preferences).  
	/// A tooltip attached to the label will display the definition.
	///</remarks>
	public class DisplayWindow : Gnome.App
	{
		Gnome.Program program=null;
		GConf.Client gconfClient=null;
		
		protected IWordGenerator wordGenerator;
		protected Preferences preferences;
		protected Label word = new Label();
		protected EventBox eventBox = new EventBox();
		protected Tooltips definition = new Tooltips();
		protected Clipboard clipboard; 
		
		private Menu menu = new Menu();
		
		public DisplayWindow (Gnome.Program program) : base ("WOTD", "WOTD")
		{		
			this.program = program;
			this.preferences=Preferences.FromGConfStore();
			
			// Load word generator
			this.wordGenerator = Strategy.GenerateWordGenerator(preferences); 

			// TODO: add some error checking here for above loads.
			
			
			clipboard = Clipboard.GetForDisplay(Gdk.Display.Default, Gdk.Selection.Clipboard);
			this.DefaultWidth = 150;
			this.DefaultHeight = 25;
			this.DeleteEvent += new DeleteEventHandler(delete_event);
			this.wordGenerator=wordGenerator;
			
			// Add EventBox to windows
			this.Contents = eventBox;
			
			// Add label to EventBox			
			eventBox.Add(word);

			// Add menu to eventbox
			eventBox.ButtonPressEvent += new ButtonPressEventHandler (HandleButtonPressEvent);
			eventBox.ButtonReleaseEvent += new ButtonReleaseEventHandler (HandleButtonReleaseEvent);
			
			// Put label in standby state. When the wordgenerator advances,
			// it will be updated to reflect the current word.
			word.Text="Retrieving...";
			
			// Listen to word change events
			wordGenerator.WordChangedEvent+=new WordChangedEventHandler(this.HandleWordChangedEvent);
			this.wordGenerator.MoveNext();			
			
			// Initialize Menu
			CreatePopupMenu();
			this.ShowAll();
		}
		
		///<summary />Displays a new word.
		///<param name="entry"/ >Entry object describing the new word
		private void DisplayWord(Entry entry) {
			DisplayWord(entry,null);
		}
		private void DisplayWord(Entry entry, WordChangedEventArgs args)  {
			// No entry means we haven't gotten it yet
			if (args==null && entry==null){ 
				word.Text = "Retrieving...";
			}else if (args!=null && args.IsError){
				word.Text= ((entry==null || entry.Word==null) ? "Error" : entry.Word);
				definition.SetTip(eventBox, args.ErrorMessage, null);
			}else{
				//valid word.
				word.Text=entry.Word;
				//definition.SetTip(eventBox, entry.ToString(), null);
				definition = new Tooltips();
				definition.SetTip(eventBox, entry.ToString(), null);
			}
			word.ShowAll();
		}
		
		///<summary />Creates the popup menu and attaches even handlers for each option
		private void CreatePopupMenu()  {
			// Create menu Items and append them to menu
			ImageMenuItem copyWordItem = new ImageMenuItem("Copy _Word");
			copyWordItem.Image = new Gtk.Image(Stock.Copy, IconSize.Menu);
			menu.Append(copyWordItem);
			
			ImageMenuItem copyDefItem = new ImageMenuItem("Copy _Definition");
			copyDefItem.Image = new Gtk.Image(Stock.Copy, IconSize.Menu);
			menu.Append(copyDefItem);
			
			ImageMenuItem nextWordItem = new ImageMenuItem("_Next Word");
			menu.Append(nextWordItem);
			
			SeparatorMenuItem sep = new SeparatorMenuItem();
			menu.Append(sep);
			
			ImageMenuItem prefItem = new ImageMenuItem("Preferences");
			prefItem.Image = new Gtk.Image(Stock.Preferences, IconSize.Menu);
			menu.Append(prefItem);
			
			ImageMenuItem aboutItem = new ImageMenuItem("About");
			aboutItem.Image = new Gtk.Image(Gnome.Stock.About, IconSize.Menu);
			menu.Append(aboutItem);
			
			ImageMenuItem quitItem = new ImageMenuItem("Quit");
			quitItem.Image = new Gtk.Image(Stock.Quit, IconSize.Menu);
			menu.Append(quitItem);
			
			// Activate event handlers
			copyWordItem.Activated += new EventHandler(handleCopyWordCommand);
			copyDefItem.Activated += new EventHandler(handleCopyDefCommand);
			nextWordItem.Activated += new EventHandler(HandleNextWordCommand);
			prefItem.Activated += new EventHandler(handlePrefCommand);
			aboutItem.Activated += new EventHandler(handleAboutCommand);
			quitItem.Activated += new EventHandler(handleQuitCommand);
			
			// Make menu items visible
			menu.ShowAll();
		}
		
		///<summary>Event handler for a button press on the event box.  Displays
		///the popup menu.
		///</summary>
		private void HandleButtonPressEvent (object o, ButtonPressEventArgs args)  {
			switch (args.Event.Button)	{
			case 1:
				break;
			case 2:
				break;
			case 3:
				menu.Popup (null, null, null, IntPtr.Zero,
				            args.Event.Button, args.Event.Time);			
				break;
			default:
				break;
			}

			args.RetVal = false;
		}

		///<summary>Event handler for a button release on the event box.  Hides
		///the popup menu.
		///</summary>
		private void HandleButtonReleaseEvent (object o, ButtonReleaseEventArgs args)	{
			switch (args.Event.Button)	{
			case 1:
			case 2:
			case 3:
				menu.Popdown ();			
				break;

			default:
				break;
			}
			
			args.RetVal = false;
		}
		
		///<summary>
		///Event handler for the Copy Word menu item.  Copies the
		///word to the clipboard.
		///</summary>
		private void HandleNextWordCommand(object o, EventArgs args){
			this.wordGenerator.MoveNext();	
		}

		///<summary>
		///Handler for a word changed event; fired when the appropriate
		///interval time has passed in the word generator.
		///</summary>
		private void HandleWordChangedEvent(object sender, WordChangedEventArgs e){ 
			DisplayWord(e.Word, e);
		}
		
		private void handleCopyWordCommand(object o, EventArgs args)  {
			if (wordGenerator.CurrentWord!=null)			
				clipboard.SetText(wordGenerator.CurrentWord.Word);
		}
		
		///<summary>
		///Event handler for the Copy Word menu item.  Copies the
		///definition to the clipboard.
		///</summary>
		private void handleCopyDefCommand(object o, EventArgs args)  {
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			System.Collections.IList defs = wordGenerator.CurrentWord.Definitions;
			
			bool first = true;
			foreach (System.Object def in defs)  {
				if (!first)
					builder.Append("\n");
				first = false;
				builder.Append(def.ToString())	;
			}
			clipboard.SetText(builder.ToString());
		}
		
		///<summary>Event handler for the Preferences menu item.  Opens up a
		/// new preferences dialog.
		///</summary>
		private void handlePrefCommand(object o, EventArgs args)  {
			PreferencesDialog pd = new PreferencesDialog(wordGenerator, preferences);
			pd.Run();
			pd.Hide();
			
			wordGenerator.WordChangedEvent-=HandleWordChangedEvent;
			wordGenerator = pd.WordGenerator;
			
			// Set visited words
			if (wordGenerator is FileWordGenerator)
				preferences.VisitedWords = ((FileWordGenerator)wordGenerator).VisitedWords;
			else
				preferences.VisitedWords = new string[0];
					
			// save preferences to GConf
			this.preferences.SaveToGConfStore();
			
			// Put label in standby state. When the wordgenerator advances,
			// it will be updated to reflect the current word.
			word.Text="Retrieving...";
			
			// Listen to word change events
			wordGenerator.WordChangedEvent+=new WordChangedEventHandler(this.HandleWordChangedEvent);
			this.wordGenerator.MoveNext();			
			
			// TODO: must update word generator etc. to reflect new preferences
			// that the user selected.
		}
		
		private void handleAboutCommand(object o, EventArgs args)  {
			About.ShowWindow();
		}
		
		/// <summary>
		/// Event handler for the Quit menu item.  Ends the
		/// program thread.
		/// </summary>
		private void handleQuitCommand(object o, EventArgs args)  
		{
			Quit();
		}
		
		///<summary>
		///Event handler for the delete event on the window.
		///Ends the program thread.
		///</summary>
		private void delete_event (object obj, DeleteEventArgs args)
		{
			Quit();			
		}
		///<summary>
		///Preform cleanup/serialization routines.
		///</summary>
		private void Quit()
		{
			// Write preferences to gconf store; in particular,
			// this is important for list of visited words, as
			// that changes on a timer independent of the user input
			// and must be saved.
			
			// This file word generator rule needs to be abstracted.
			FileWordGenerator gen = this.wordGenerator as FileWordGenerator;
			if (gen != null)
				this.preferences.VisitedWords=gen.VisitedWords;
			this.preferences.SaveToGConfStore();
			
			Application.Quit ();
		}
	}
}