using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System;

public class BackpackItemEvent : MonoBehaviour
{
	public void Awake()
	{
		//取得xml數據檔
		xmlStructLoad = XmlStructLoad.GetInstance();
	}
	
	// Use this for initialization
	void Start()
	{
		getpathPanel = LoadAssetBundles.lobbyAssetbundle.LoadAsset<GameObject>("GetpathPanel");
		CreateBackpackItem(PlayerPrefs.GetString("Backpack_Property"));
	}
	
	void CreateBackpackItem(string property)
	{
		foreach (Transform obj in gameObject.transform)
			Destroy (obj.gameObject);
		
		List<string> itemID = new List<string> ();
		List<string> itemCount = new List<string> ();
		List<string> itemPhoto = new List<string> ();
		
		SqliteDataReader reader;
		if (property.Equals ("All")) {
			reader = LobbyManager.db.SearchFullTable ("PJ_Equipment");
		} else {
			reader = LobbyManager.db.SearchFullTableCondition ("PJ_Equipment", "Backpack_Property", "=", "'" + property + "'");
		}
		if (reader != null) {
			if (reader.HasRows) {
				while (reader.Read()) {
					//取得需要的欄位
					itemID.Add (reader [0].ToString ());
					itemCount.Add (reader [7].ToString ());
					//++++++++++++
				}
				reader.Close ();
			}
		}
		
		//生成背包資訊
		UISprite itemUISprite;
		GameObject item;
		for (int i = 0; i < itemID.Count - 1; i++) {
			if (!itemCount [i].Equals ("0")) {
				item = NGUITools.AddChild (gameObject, backpackItem_Prefab);
				itemUISprite = item.GetComponent<UISprite> ();
				itemUISprite.spriteName = xmlStructLoad.item_type_Configuration.Items.Item.Find (x => x.Id = itemID [i]).Icon;
				itemUISprite.MakePixelPerfect ();
				item.GetComponent<UIButtonMessage> ().target = item.transform.parent.gameObject;
				item.transform.FindChild ("tv_ID").GetComponent<UILabel> ().text = itemID [i];
				item.transform.FindChild ("tv_Count").GetComponent<UILabel> ().text = itemCount [i];
				item.name = "Item_" + (i + 1);
			}
		}
		//++++++++++
	}
}
