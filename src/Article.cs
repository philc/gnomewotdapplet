/// <remarks>
/// Author: Phil Crosby
/// </remarks>
using System;

namespace org.gnome.Applets.GnomeWotdApplet{

/// <summary>
/// An article of a definition for a word.
/// </summary>
public class Article{
	//Static members
	public static Article NOUN = new Article("n");
	public static Article VERB = new Article("v");
	public static Article ADJ = new Article("adj");
	public static Article ADV = new Article("adv");
	
	//Instance members
	string name = "";
	
	#region Constructors
	protected Article(string name){
		this.name=name;
	}
	#endregion

	/// <summary>
	/// Translates a string into an article
	/// </summary>
	/// <param name="article" />String representation of an article
	/// <returns />Article enumeration of the string.
	public static Article Parse(string article){
		Article result;
		string lower = article.Trim().ToLower();
		if (lower.Equals("n") || lower.Equals("noun"))
			result=Article.NOUN;
		else if (lower.Equals("v") || lower.Equals("verb"))
			result=Article.VERB;
		else if (lower.Equals("adv") || lower.Equals("adverb"))
			result=Article.ADV;
		else if (lower.Equals("adj") || lower.Equals("adjective"))
			result=Article.ADJ;
		else 
			throw new ArgumentException(article + " is not a valid article.");
		return result;
	}
	public override string ToString(){
		return name;
	}	
}

}//end namespace