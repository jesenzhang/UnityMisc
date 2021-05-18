
using System.Collections.Generic;


namespace TagParser
{
   public class TagParser {
	   
		private char openingCharacter = '<';
		private char closingCharacter = '>';
		private char endTagCharacter = '/';

		private TagObject _tempTagObject; // temporary object to store tag info in
		private Stack<TagObject> _tagStack; // stack that holds all tag objects
		private List<TagObject> _listOfTags; // final list of tag objects

		string _textWithNoTags = ""; // the text without the HTML tags
		public TagParser()
		{
			_tempTagObject = new TagObject(); // temporary object to store tag info in
			_tagStack = new Stack<TagObject>(); // stack that holds all tag objects
			_listOfTags = new List<TagObject>(); // final list of tag objects
		}
		
		public void Reset()
		{
			_tagStack.Clear(); 
			_listOfTags.Clear();
			_textWithNoTags = "";
		}

		public List<TagObject> Tags => _listOfTags;

		public string TextWithNoTags => _textWithNoTags;
		
		// Whenever we encounter an opening character, and the previous
		// character wasn't an escape character (\) create a new TagObject
		// and start counting till the closing character is reached 
		public void ParseText(string text) 
		{
			bool tagInProgress = false; // tells us whether or not we're in the middle of a tag (between < and >)
			bool tagIsClosing = false; // tells us if the tag we're reading right now is closing a group (with </ )
			_tagStack.Clear();
			_listOfTags.Clear();
			int line = 0;
			int actualTextIndex = 0; // this will only be incremented when a "normal" character is read
			for (int i = 0; i < text.Length; i++) {
				char nextChar;
				if (i < text.Length - 1) nextChar = text[i+1];
				else nextChar = '\0';

				if (!tagInProgress) {
					bool currentCharacterIsOpeningTag = (text[i] == openingCharacter);
					bool nextCharacterIsEndingTag = (nextChar == endTagCharacter);

					if (currentCharacterIsOpeningTag) {
						tagInProgress = true;
						if (nextCharacterIsEndingTag) {
							tagIsClosing = true;

							var o =_tagStack.Peek();
							
							if (o.startIndex == actualTextIndex)
							{
								o.endIndex = actualTextIndex;
								o.Value = "";
							}
							else
							{
								o.endIndex = actualTextIndex - 1;
								o.Value = _textWithNoTags.Substring(o.startIndex,o.endIndex-o.startIndex+1);
							}

							o.lineIndex = line;
							//tempTagObject.endIndex = actualTextIndex - 1;
						} else {
							// a new tag is happening
							_tempTagObject = new TagObject();
							_tagStack.Push(_tempTagObject);
							_tagStack.Peek().startIndex = actualTextIndex;
							//tempTagObject.startIndex = actualTextIndex;
						}
					} else {
						_textWithNoTags += text[i];
						actualTextIndex++;
					}
				} else {
					bool currentCharacterIsClosingTag = (text[i] == closingCharacter); 
					if (currentCharacterIsClosingTag) {
						tagInProgress = false;
						if (tagIsClosing) {
							//Debug.Log($"closing tag for {_tempTagObject.content}, which starts at {_tempTagObject.startIndex}");

							TagObject finishedTagObject = _tagStack.Pop();
							_listOfTags.Add(finishedTagObject);
							tagIsClosing = false;
						} else {
							// we're done reading an opening tag
							// so let's put this on the stack
							//tagStack.Push(tempTagObject);
							///Debug.Log($"Pushing the tag {_tempTagObject.content} onto the stack");
						}
					} else if (!tagIsClosing) {
						_tagStack.Peek().content += text[i];
						//tempTagObject.contents += text[i];
					}
				}

				if (text[i] == '\n')
				{
					line++;
				}
			}
			 /*Debug.Log("========");
			 Debug.Log($"Raw text: {_textWithNoTags}");
			 Debug.Log("========");

			 foreach (TagObject tag in _listOfTags) {
			 	tag.WriteInformation();
			 	Debug.Log("========");
			 }*/
		}
	} 


    public class TagObject {
        public string content = "";
        public int startIndex = 0;
        public int endIndex = 0;
        public int lineIndex = 0;
        private string _tag;
        private string _value;
        
        private List<TagProperty> _properties;
        private string[] _splitedContent;
        private bool _parsed = false;
		public void WriteInformation() {
			/*Debug.Log($"Name: {Name}");
			Debug.Log($"Value: {Value}");
			Debug.Log($"Start index: {startIndex}");
			Debug.Log($"End index: {endIndex}");
			Debug.Log("Properties:");
			foreach (TagProperty prop in Properties) {
				prop.WriteInformation();
				Debug.Log("\t========");
			}*/
			return;

		}

		public int[] ArrayOfIndices {
			get {
				int[] outVal = new int[(endIndex - startIndex) + 1];

				for (int i = 0; i <= endIndex - startIndex; i++) {
					outVal[i] = startIndex + i;
				}
				
				return outVal;
			}
		}

		public void Parse()
		{
			if (!_parsed)
			{
				if(_splitedContent==null)
					_splitedContent = content.Split(' ');
				if(_properties==null)
					_properties = new List<TagProperty>();
				_properties.Clear();
				_tag = _splitedContent[0];
				for (int g = 1; g < _splitedContent.Length; g++) {
					_properties.Add(new TagProperty(_splitedContent[g]));
				}
				_parsed = true;
			}
		}

		public string Name {
			get
			{
				Parse();
				return _tag;
			}
		}
		
		public string Value {
			get
			{
				return _value;
			}
			set
            {
	            _value = value;
            }
		}

		public List<TagProperty> Properties {
			get {
				Parse();
				return _properties;
			}
		}
    }

    public class TagProperty {
        public string key = "";
        public string value = "";

		public TagProperty(string input) {
			string[] stuff = input.Split('=');
			if (stuff.Length >= 2)
			{
				key = stuff[0];
				value = stuff[1];
			}
		}

		public void WriteInformation() {
			/*Debug.Log($"\tKey: {key}");
			Debug.Log($"\tValue: {value}");*/
			return;
		}
    }
}