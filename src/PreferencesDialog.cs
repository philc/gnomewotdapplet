///<remarks>Author: Michael Quinn
///</remarks>
using System;
using Gtk;
using Glade;
using GtkSharp;
using System.Collections;
using System.IO;
// using Gnome;
using System.Diagnostics;

namespace org.gnome.Applets.GnomeWotdApplet{
	public class PreferencesDialog : Dialog {
		// import all widgets needed from glade file
		[Glade.Widget] Gtk.SpinButton spnTimes;
		[Glade.Widget] Gtk.OptionMenu optInterval;
		[Glade.Widget] Gtk.Button btnEditDictionary;
		[Glade.Widget] Gtk.OptionMenu optWebsite;
		[Glade.Widget] Gtk.OptionMenu optDictionary;
		[Glade.Widget] Gtk.CheckButton chkFromInternet;

		[Glade.Widget] Gtk.Label lblGetWordsFrom;
		[Glade.Widget] Gtk.Label lblSpacer;
		[Glade.Widget] Gtk.Label lblUseDictionary;
		[Glade.Widget] Gtk.Label lblUpdate;
		
		Dictionary dictionary;
				
		IWordGenerator wordGenerator;
		Preferences preferences;
		
		private int importPosition;
		
		static private string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
	    static private string programDirectory = homeDirectory + "/.gnomewotd/";
	    private string[] files;
	    
		///<summary>
		/// Loads the dialog from its glade file and connects all event handlers.
		///</summary>
		private void Setup()  {		
			Glade.XML gxml = new Glade.XML(null, "PreferencesDialog.glade", "mainVBox", null);
			gxml.Autoconnect(this);
			
			// add the close button
			this.AddButton(Gtk.Stock.Close, ResponseType.Close);
			this.Response += new ResponseHandler(OnDialogResponse);
			this.BorderWidth = 12;
			
			this.Resizable = false;
			
			// add options to the same Gtk.SizeGroup
			SizeGroup group = new SizeGroup(SizeGroupMode.Horizontal);
			group.AddWidget(lblUseDictionary);
			group.AddWidget(lblSpacer);
			group.AddWidget(lblGetWordsFrom);
			
			// add glade VBox to the dialog
			this.VBox.PackStart( gxml["mainVBox"], true, true, 6);
			
			// set all options based on preferences
			chkFromInternet.Active = preferences.RetrieveFromUrl;
			
			// fill up file list box
			populateDictionaryList();			
			optDictionary.SetHistory((uint)preferences.DictionaryIndex);
			
			if (preferences.DictionaryUrl == UrlWordGenerator.DictionaryComUrl)
				optWebsite.SetHistory(0);
			else if (preferences.DictionaryUrl == UrlWordGenerator.MerriamWebsterUrl)
				optWebsite.SetHistory(1);
			
			spnTimes.Value = preferences.Frequency;
			
			Period period = preferences.Period;
			if (period==Period.Day)
				optInterval.SetHistory(0);
			else if (period==Period.Hour)
				optInterval.SetHistory(1);
			else if (period==Period.Minute)
				optInterval.SetHistory(2);			
			else if (period==Period.Second)
				optInterval.SetHistory(3);
		}
		
		//Constructor
		///<summary />Calls setup() to initialize dialog, then starts the dialog thread.
		public PreferencesDialog (IWordGenerator wordGenerator, Preferences preferences)  {
			this.wordGenerator = wordGenerator;
			this.preferences=preferences;
			
			Setup();
		}
				
		///<summary>
		///Saves the contents of this preferences form to the preferences object.
		///</summary>
		private void SavePreferences()
		{
			preferences.RetrieveFromUrl = chkFromInternet.Active;
			
			switch (optWebsite.History)  {
				case 0:
					preferences.DictionaryUrl = UrlWordGenerator.DictionaryComUrl;
					break;
				case 1:
					preferences.DictionaryUrl = UrlWordGenerator.MerriamWebsterUrl;
					break;
			}
			
			preferences.DictionaryIndex = optDictionary.History;
			preferences.DictionaryPath = getDictionaryPath();
			
			preferences.Frequency = (int)spnTimes.Value;
			switch (optInterval.History)  {
				case 0:
					preferences.Period=Period.Day;
					break;
				case 1:
					preferences.Period=Period.Hour;
					break;
				case 2:
					preferences.Period=Period.Minute;
					break;
				case 3:
					preferences.Period=Period.Second;
					break;
			}	
		}
		
	    private void populateDictionaryList()  {
	    	files = null;
			try  {
	    		files = Directory.GetFiles(programDirectory);
	    	} catch (DirectoryNotFoundException e)  {
	    		Directory.CreateDirectory(programDirectory);
	    		Console.WriteLine("Created program directory");
	    	}
	    	
	    	Menu menu = new Menu();
	    	
	    	int position = 0;
	    	if (files != null)  {    		
	    		foreach (string file in files)  {
	    			string name = file.Substring(programDirectory.Length, 
												 file.Length - programDirectory.Length - 4);
	    			MenuItem menuItem = new MenuItem(name);
	    			menu.Append(menuItem);
	    			position++;
				}
			}
			
			menu.Append(new SeparatorMenuItem());
			position++;
				
			ImageMenuItem import = new ImageMenuItem("Import Dictionary");
			import.Image = new Gtk.Image(Gtk.Stock.Execute, IconSize.Menu);
 			import.Activated += new EventHandler(OnImportDictionaryActivated);
			menu.Append(import);
			
			importPosition = position;

			optDictionary.Menu = menu;
			menu.ShowAll();
			
		}
		
	    private string getDictionaryPath()  {
	    	// There is an "import" option that we don't want to return/index in to.
	    	return (files.Length > optDictionary.History+1) ? files[optDictionary.History] : "";
		}
  
	    #region EventHandlers
	    
	    
		private void OnDialogResponse (object obj, ResponseArgs args)  {
			if (args.ResponseId == ResponseType.Close)  {
				SavePreferences();
				wordGenerator = Strategy.GetGenerator(wordGenerator, preferences);
			}
		}
		
		private void OnBtnEditDictionaryClicked (object o, EventArgs args)  {
			if (optDictionary.History == importPosition)  {
				string errorMessage = "Please select a dictionary to edit.";
				MessageDialog warning = new MessageDialog (this, 
														   DialogFlags.DestroyWithParent, 
														   MessageType.Error,
														   ButtonsType.Close,
														   errorMessage);
				warning.Run();
				warning.Hide();				
			} else {
				string file = getDictionaryPath();
				string dictionaryName = file.Substring(programDirectory.Length, 
													   file.Length - programDirectory.Length - 4);
				Dictionary dictionary = Dictionary.FromFile(file);
			
				DictionaryEditorDialog editor = new DictionaryEditorDialog(dictionary,
																		   dictionaryName,
																		   this);
				editor.Run();
				Console.WriteLine("Dictionary Closed");
				editor.Hide();
			}
		}
		
		private void OnImportDictionaryActivated (object o, EventArgs args)  {
			FileSelection fs = new FileSelection ("Import Dictionary");
			fs.Run();
			string filename = fs.Filename;
			fs.Hide();
			
			// TODO: popup a message box if the dictionary isn't valid
			if (fs.SelectionEntry.Text != "")  {			
				Dictionary dictionary = Dictionary.FromFile(filename);
				dictionary.Save(programDirectory + fs.SelectionEntry.Text);
				populateDictionaryList();
			}
		}
		
		private void OnChkFromInternetToggled (object o, EventArgs args)  {
			if (chkFromInternet.Active)  {
				lblUpdate.Sensitive = spnTimes.Sensitive = optInterval.Sensitive = false;
			} else  {
				lblUpdate.Sensitive = spnTimes.Sensitive = optInterval.Sensitive = true;
			}
		}
		#endregion
		
		#region Properties
		public IWordGenerator WordGenerator  {
			get { return wordGenerator; }
		}
		#endregion
	}

}//end namespace