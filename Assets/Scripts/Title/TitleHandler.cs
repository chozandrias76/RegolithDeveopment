using UnityEngine;
using System.Collections;

public class MenuHandler : MonoBehaviour {
	
	public MenuGUI[] menuItems;
	public int currentMenuItem;
	float keyDelay = 0.1250f;
	 RaycastHit hit=new RaycastHit();
	// Use this for initialization
	void Start () {
		menuItems[currentMenuItem].OnSelected(true);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.anyKeyDown){
		if(Input.GetAxisRaw ("Vertical") > 0.9)
			{
				menuItems[currentMenuItem].OnSelected(false);
				currentMenuItem--;
				if(currentMenuItem < 0.9) currentMenuItem = 0;
				menuItems[currentMenuItem].OnSelected(true);
				//new WaitForSeconds(keyDelay);
			}
			else if(Input.GetAxisRaw("Vertical") < -0.9)
			{
				menuItems[currentMenuItem].OnSelected(false);
				currentMenuItem++;
				if(currentMenuItem >= menuItems.Length) currentMenuItem = menuItems.Length - 1;
				menuItems[currentMenuItem].OnSelected(true);
				//new WaitForSeconds(keyDelay);
			}
			else if(Input.GetKey(KeyCode.Return))
			{
				if(currentMenuItem==0)
				{
					Application.LoadLevel("");
				}
				if(currentMenuItem==1)
				{
					Application.LoadLevel("Test01");
				}
				
			}
			}
		  /*Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		  // Update is called once per frame
		  if (Physics.Raycast(ray, out hit, 100.0f)) {
		   Debug.Log ("Hit:" + hit.transform.name); 
		   if (hit.transform.name == "Start") {
				currentMenuItem = 0;
		   iTween.MoveTo(menuItems[currentMenuItem],iTween.Hash ("z",-1,"time",4));
		   }
			else  if (hit.transform.name != "Start") {
		   iTween.MoveTo(menuItems[0],iTween.Hash ("z",1,"time",4));
		   }
			else if (hit.transform.name == "Debug") {
		   iTween.MoveTo(menuItems[1],iTween.Hash ("z",-1,"time",4));
		   }
			else  if (hit.transform.name != "Debug") {
		   iTween.MoveTo(menuItems[1],iTween.Hash ("z",1,"time",4));
		   }
			else if (hit.transform.name == "Quit") {
		   iTween.MoveTo(menuItems[2],iTween.Hash ("z",-1,"time",4));
		   }
			else  if (hit.transform.name != "Quit") {
		   iTween.MoveTo(menuItems[2],iTween.Hash ("z",1,"time",4));
		   }
		  }*/
	}
 
	
	
}	
