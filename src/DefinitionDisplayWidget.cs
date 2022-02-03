///<remarks>Author: Michael Quinn
///</remarks>
using System;
using System.Collections;
using Gtk;
using GtkSharp;

namespace org.gnome.Applets.GnomeWotdApplet{

///<remarks>Class representing a Definition display widget, consisting of a 
/// TreeView of Definitions.
///</remarks>
public class DefinitionDisplayWidget : TreeView
{
	protected Gtk.ListStore treeStore;
	
	protected Entry entry;
	
	//Constructor
	///<summary />Creates all the necessary Objects necessary for the
	///  TreeView to display Definitions.  It sets up a column for
	///  the Article and another for the actual Definition
	///</summary>
	public DefinitionDisplayWidget () : base ()
	{		
		treeStore = new ListStore(typeof(string), typeof(string));
		this.Model = treeStore;
		
		// set TreeView properties
		this.HeadersVisible = true;
		this.HeadersClickable = false;
		this.EnableSearch = false;
		this.RulesHint = true;
		
		CellRendererText renderer;
		this.AppendColumn("Article", new CellRendererText(), "text", 0);
		
		renderer = new CellRendererText();
		renderer.Editable = true;
		renderer.Edited += new EditedHandler(DefinitionEditedEvent);
		this.AppendColumn("Title", renderer, "text", 1);
    }
	
	private void DefinitionEditedEvent(object obj, EditedArgs args){
        int index = Int32.Parse(args.Path);
        Console.WriteLine(index);
        
        IList definitions = entry.Definitions;
        ((Definition)(definitions[index])).Text = args.NewText;

        Console.WriteLine("entry changed to: " + args.NewText);
        
        TreeIter iter = new TreeIter();
        treeStore.IterNthChild(out iter, index);
        
        treeStore.SetValue(iter, 1, args.NewText);
    }
        
	///<summary />Refreshes the definitions displayed in the TreeView
	///<param name="entry" />Entry object describing the new word
	public void RefreshDefinitions(Entry entry)  {
		this.entry = entry;
		IList definitions = entry.Definitions;
		treeStore.Clear();
		foreach (Definition def in definitions)  {
			TreeIter iter = treeStore.AppendValues(def.Article.ToString(), def.Text);
		}
	}
}

}