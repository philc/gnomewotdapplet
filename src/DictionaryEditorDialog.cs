/// <remarks>
/// Author: Michael Quinn
/// </remarks>
using System;
using System.Collections;
using Gtk;
using GtkSharp;
using Glade;

namespace org.gnome.Applets.GnomeWotdApplet{

	/// <remarks>
	/// Class representing a Dictionary editor dialog window.  The
	/// dialog consists mainly of two TreeViews which display the currently
	/// loaded Dictionary, and buttons allowing for the manipulation of the
	/// Dictionary.
	/// </remarks>
	public class DictionaryEditorDialog : Dialog
	{
		[Glade.Widget] Gtk.Entry txtDictionaryName;
		
		protected Dictionary dictionary;
		protected ArrayList entries;
		
		protected string dictionaryName;
		protected WordDisplayWidget words;
		protected DefinitionDisplayWidget definitions;
		
		private Glade.XML gxml;
		
		static private string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
	    static private string programDirectory = homeDirectory + "/.gnomewotd/";
	    
		public DictionaryEditorDialog (Dictionary dictionary, string dictionaryName, Window window) :
			base("Dictionary Editor", window, DialogFlags.Modal)
		{
			entries = new ArrayList(dictionary.Entries);
			entries.Sort();
			
			this.dictionary = dictionary;
			this.dictionaryName = dictionaryName;
			
			gxml = new Glade.XML(null, "DictionaryEditorDialog.glade", "mainVBox", null);
			gxml.Autoconnect(this);
			
			this.VBox.Add (gxml ["mainVBox"]);
			SetUpDialog(gxml);
		}
		
		private void SetUpDialog(Glade.XML gxml)  {
			words = new WordDisplayWidget(entries);
			definitions = new DefinitionDisplayWidget();
			
			((Container) gxml ["scrollWords"]).Add(words);
			((Container) gxml ["scrollDefinitions"]).Add(definitions);
			
			words.Show();
			definitions.Show();
			
			words.Selection.Changed += OnSelectionChanged;
			
			this.DefaultHeight = 400;
			
			// add the buttons
			this.AddButton(Gtk.Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Gtk.Stock.Apply, ResponseType.Apply);
			this.AddButton(Gtk.Stock.Ok, ResponseType.Ok);
			
			// change label to reflect open dictionary
			txtDictionaryName.Text = dictionaryName;
			
			this.Response += new ResponseHandler(OnDialogResponse);
			
			Console.WriteLine("Set up window");
		}
		
		private void SaveDictionary ()  {
			Console.WriteLine("Saving dictionary to: " + dictionaryName);
			dictionary.Entries = entries;
			dictionary.Save(programDirectory + dictionaryName + ".xml");
		}
		
		private void ShowErrorMessage(string errorMessage)  {
			MessageDialog warning = new MessageDialog (this, 
													   DialogFlags.DestroyWithParent, 
													   MessageType.Error,
													   ButtonsType.Close,
													   errorMessage);
			warning.Run();
			warning.Hide();			
		}
		
		#region EventHandlers
				
		//<summary>Updates the definitions in the DefinitionDisplayWidget to reflect
		//  the coorsponding selected word in the WordDisplayWidget
		//</summary>
		private void OnSelectionChanged(object o, EventArgs args)  {
			TreeSelection selection = (TreeSelection)o;
			TreeModel model;
			TreePath [] path = selection.GetSelectedRows(out model);
			
			if (path.Length != 0)  {
				int [] row = path[0].Indices;
				int index = row[0];
				definitions.RefreshDefinitions(entries[index] as Entry);
			}
		}
	
		private void OnNewClicked (object o, EventArgs args)  {
			// TODO: add a dialog that asks where to save it? how should we handle this?
			
		} 
				
		private void OnClearClicked (object o, EventArgs args)  {
			string warningMessage = "Are you sure you want to clear the dictionary?";
			MessageDialog warning = new MessageDialog (this, 
													   DialogFlags.DestroyWithParent, 
													   MessageType.Warning,
													   ButtonsType.YesNo,
													   warningMessage);
			
			ResponseType result = (ResponseType) warning.Run();
			warning.Hide();				
			
			if (result == ResponseType.Yes)  {
				entries = new ArrayList();
				dictionary = Dictionary.FromEntries(entries);
				words.Entries = entries;
			} 
		}
		
		private void OnDialogResponse(object o, ResponseArgs args) {
			if (args.ResponseId == ResponseType.Ok && entries.Count > 0)  {
				SaveDictionary();
			} else if ((args.ResponseId == ResponseType.Ok && entries.Count == 0) ||
						args.ResponseId == ResponseType.Apply)  {
				if (entries.Count == 0)  {
					ShowErrorMessage("You must add at least one word to the dictionary.");
				}  else {
					SaveDictionary();
				}
				this.Run();
			} else if (args.ResponseId == ResponseType.Cancel)  {
				this.Destroy();
			}
		}
		#endregion

		#region Properties
		public Dictionary Dictionary  {
			get  { return dictionary; }
		}
		
		#endregion
		
		
	}
}//end namespace