///<remarks>
///Author: Michael Quinn
///</remarks>
using System;
using System.Collections;
using Gtk;
using GtkSharp;

namespace org.gnome.Applets.GnomeWotdApplet{

	public class WordDisplayWidget : TreeView
	{
		protected Gtk.ListStore treeStore;
		protected CellRendererText renderer;
		
		protected ArrayList entries;
		
		public WordDisplayWidget (ArrayList entries) : base ()
		{
			this.entries = entries;
			
			treeStore = new ListStore(typeof(string));
			this.Model = treeStore;
			
			// set properties
			this.HeadersVisible = true;
			this.HeadersClickable = false;
			this.EnableSearch = false;

			renderer = new CellRendererText ();
			renderer.Editable = true;
			renderer.Edited += new EditedHandler(WordEditedEvent);
			this.AppendColumn("Word", renderer, "text", 0);
									
			refreshWords();
		}
		
		private void refreshWords()  {
			treeStore.Clear();
			entries.Sort();
			foreach (Entry entry in entries)  {
				TreeIter iter = treeStore.AppendValues(entry.Word);
			}
		}
		
		private void WordEditedEvent(object obj, EditedArgs args){
        	int index = Int32.Parse(args.Path);
        	Console.WriteLine("Word # " + index + " has been changed.");
        	
        	((Entry)(entries[index])).Word = args.NewText;

        	Console.WriteLine("Word changed to: " + args.NewText);
        
        	TreeIter iter = new TreeIter();
        	treeStore.IterNthChild(out iter, index);
        
        	treeStore.SetValue(iter, 1, args.NewText);
        	refreshWords();
    	}
		
		public ArrayList Entries {
			get{
				return this.entries;
			}
			set{
				this.entries=value;
				refreshWords();
			}
		}
	}

}