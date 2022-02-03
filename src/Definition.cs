///<remarks>Author: Phil Crosby
///</remarks>
using System;
using System.Xml;

namespace org.gnome.Applets.GnomeWotdApplet{

///<remarks>Class representing one definition, including article and
///definition text.
///</remarks>
public class Definition : ICloneable{
	//Instance members
	//Valid values include adj, adv, v, and n
	protected Article article;
	protected string text;	
		
	//Constructor
	///<summary />Creates a definition from an article and definition text.
	///<param name="article">Article of the definition - adj, adv, n, or v
	///<param name="text">Text for the definition
	public Definition(string article, string text)
			: this(Article.Parse(article), text){
	}
	///<summary />Creates a definition from an article and definition text.
	///<param name="article"/ >Article of the definition - adj, adv, n, or v
	///<param name="text" />Text for the definition
	public Definition(Article article, string text){
		this.article=article;
		this.text=text;
	}
	/// <summary>
	/// Creates a copy of this definition, including the article and the text.
	/// </summary>
	/// <returns />A deep copy of this definition.
	public Object Clone()
	{
		return new Definition(this.Article, text);
	}
	///<summary />Static factory method creating a definition from an XML stream.
	///<param name="reader" />XMLTextReader from which to create the definition.
	public static Definition FromXml(XmlTextReader reader)
	{
		string article="";
		string text="";
		//Read in the article attribute
		reader.MoveToAttribute("article");
		reader.ReadAttributeValue();
		article=reader.Value.Trim();
		
		//Read in definition text
		reader.MoveToElement();
		text = reader.ReadString().Trim();

		if (article.Length<=0 || text.Length<=0)
			throw new XmlException("Error parsing definition XML fragment.");
			
		return new Definition(article,text);
	}
	/// <summary>
	/// Writes this dictionary to an XML stream.
	/// </summary>
	public void WriteXml(XmlWriter writer)
	{
		writer.WriteStartElement("Definition");
		writer.WriteAttributeString("article",this.Article.ToString());
		writer.WriteString(text);
		writer.WriteEndElement();		
	}
	
	///<see cref="Object.ToString()">
	public override string ToString()
	{
		return "(" + article + ") " + text;
	}
	
	//Properties
	///<summary />The article of this definition.
	public Article Article{
		get { return article; }
		set { article=value; }
	}
	
	///<summary />The text of this definition.
	public string Text{
		get { return text; }
		set { text=value; }
	}	
}

}//end namespace