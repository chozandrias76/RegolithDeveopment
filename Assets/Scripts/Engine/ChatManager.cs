using UnityEngine;
using System;
using System.Collections.Generic;

public class ChatManager
{
	PangoWorld	root;
	
	
	List<string>		chatHistory=new List<string>();
	string 				chatLine="";
	public bool			inChat=false;
	float				chatDisplayLines=0;
	float				chatBlinkCounter=0;
	
	
	public void localChat(string str){
		chatHistory.Add(str);
		if(chatDisplayLines<5)
			chatDisplayLines=5;		
	}
	
	public void Update()
	{
		if(inChat){
			chatLine+=Input.inputString;
			chatLine=chatLine.Replace("\r","");
			chatLine=chatLine.Replace("\b","");
			if(chatLine.Length>32)
				chatLine=chatLine.Substring(0,32);
		}
		if(Input.GetKeyDown (KeyCode.Backspace)){
			if(chatLine.Length>0){
				chatLine=chatLine.Substring(0,chatLine.Length-1);
			}
		}
		if(Input.GetKeyDown (KeyCode.Return)){
			if(inChat==false){
				inChat=true;
				if(chatDisplayLines<5)
					chatDisplayLines=5;
			}else{

				if(chatLine.Length>0){
					if(chatLine.StartsWith("#players")){
						root.playerManager.dumpPlayers();
					}else{
					 	root.allChat(chatLine);
					}
					chatLine="";
				}else{
					inChat=false;
					chatLine="";
				}
			}
		}
		
	}
	
	
	public void Render(){

		string chatStr="";
		int cdl=(int)chatDisplayLines;
		int clen=chatHistory.Count;
		int lineCount=0;
		if(cdl>clen){
			chatDisplayLines=cdl=clen;
		}
		if(cdl>0){
			List<string> visChat=chatHistory.GetRange(clen-cdl,cdl);
			foreach(string str in visChat){
				chatStr+=str+"\n";
				lineCount++;
			}
		}
		chatStr+=chatLine;
		if(inChat && (chatBlinkCounter++%60)>30)chatStr+="|";
		float scrollOff=chatDisplayLines-((int)chatDisplayLines);
		if(inChat)
			lineCount++;
		if(lineCount>0){
			int fhite=17;
			GUI.TextArea (new Rect (10,10+ (scrollOff*fhite), 700, (fhite*lineCount)+3 ),chatStr);
		}
		if(inChat==false){
			chatDisplayLines -= 0.01f;
			if(chatDisplayLines<0.0f)
				chatDisplayLines=0.0f;
		}
	}
	
	public ChatManager (PangoWorld _root)
	{
		root=_root;
	}
}


