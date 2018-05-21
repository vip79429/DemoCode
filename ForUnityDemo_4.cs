using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.IO;

public class do_ServerFunction : MonoBehaviour {
    string ServerUpdateType;
    static get_do_Function.do_commit_battle_result_Bai battle_result_Bai;
    static get_do_Function.do_bet_RootObject do_bet_RootObject_data;
    static get_do_Function.do_exchange_RootObject do_exchange_RootObject_data;
    static string Login_Award_ID;

    static string Now_Scene;
    static XmlStructLoad xmlStructLoad;
  
	//更新server回傳資料後的畫面
	void Update () {
        
        switch (ServerUpdateType)
        {
            case "Disconnect":
                NGUITools.AddChild(GameObject.Find("Camera"), messageonebuttonPanel);
                GameObject.Find("MessageOneButtonPanel(Clone)/tv_Message").GetComponent<UILabel>().text = "與伺服器斷線";
                GlobalValue.inUpload = false;
                ServerLoadingPanel.serverOpen = false;
                JFSocketNonblocking.GetInstance().Closed();
                ServerUpdateType = "";
                this.enabled = false;
                break;
            case "do_commit_battle_result_WIN":
                object[] WIN = new object[2];
                WIN[0] = "do_commit_battle_result_WIN";
                WIN[1] = battle_result_Bai;
                Transform gameoverTweenWin = GameObject.Find("GameOverPanel").transform.FindChild("GameOverTween");
                gameoverTweenWin.gameObject.SetActive(true);
                gameoverTweenWin.SendMessage("ReceiveServerCallBack", WIN);
                ServerUpdateType = "";
                break;
            case "do_get_mail_extra_content":
                string attackment = LobbyManager.db_Server.searchOneCondition("extra_content", "mail_content", "id", "=", PlayerPrefs.GetString("Click_Mail_ID"));
                JArray data = JArray.Parse(attackment);
                ArrayList temp = new ArrayList();
                NGUITools.AddChild(GameObject.Find("Camera"), messagegetitemPanel);

                for (int i = 1; i < 5; i++)
                {
                    if (i > data.Count)
                    {
                        continue;
                    }
                    int rt = (int)data[i - 1]["rt"];
                    int rto = (int)data[i - 1]["rto"];
                    int rtv = (int)data[i - 1]["rtv"];
                    switch (rt)
                    {
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_GOLD:
                            Debug.Log("黃金數量:" + rtv.ToString());
                            temp.Add("Icon_Misson_Reward_001");
                            temp.Add(rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_COIN:
                            Debug.Log("硬幣數量:" + rtv.ToString());
                            temp.Add("Icon_Misson_Reward_004");
                            temp.Add(rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_DIAMOND:
                            Debug.Log("鑽石數量:" + rtv.ToString());
                            temp.Add("Icon_Misson_Reward_002");
                            temp.Add(rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_STAMINA:
                            Debug.Log("體力數量:" + rtv.ToString());
                            temp.Add("Icon_Misson_Reward_005");
                            temp.Add(rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_CTRYSTAL:
                            Debug.Log("CTRYSTAL數量:" + rtv.ToString());
                            temp.Add("0");
                            temp.Add(rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_MEDAL:
                            Debug.Log("MEDAL數量:" + rtv.ToString());
                            temp.Add("Icon_Misson_Reward_003");
                            temp.Add(rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_ROLE_EXP:
                            Debug.Log("角色EXP:" + rtv.ToString());
                            temp.Add("Icon_Misson_Reward_006");
                            temp.Add(rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_SKILL_POINT:
                            Debug.Log("技能點數:" + rtv.ToString());
                            temp.Add("0");
                            temp.Add(rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_HERO_EXP:
                            Debug.Log("HERO_EXP:" + rtv.ToString());
                            temp.Add("Icon_Misson_Reward_006");
                            temp.Add(rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_ITEM:
                            Debug.Log("item type:" + rto.ToString());
                            Debug.Log("count:" + rtv.ToString());
                            temp.Add(xmlStructLoad.item_type_Configuration.Items.Item.Find(x => x.Id == rto.ToString()).Icon);
                            temp.Add(rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_HERO:
                            Debug.Log("hero type:" + rto.ToString());
                            Debug.Log("count:" + rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_BET_WEEK_HERO:
                            Debug.Log("week hero type:" + rto.ToString());
                            Debug.Log("count:" + rtv.ToString());
                            temp.Add("0");
                            temp.Add(rtv.ToString());
                            break;
                        case (int)NET_MSG_API.EM_GAME_RESOURCE_TYPE.GRT_BET_DAY_HERO_CHIP:
                            Debug.Log("chip hero type:" + rto.ToString());
                            Debug.Log("count:" + rtv.ToString());
                            temp.Add(xmlStructLoad.item_type_Configuration.Items.Item.Find(x => x.Id == rto.ToString()).Icon);
                            temp.Add(rtv.ToString());
                            break;
                    }
                }
                LobbyManager.db_Server.deleteAccordData("mail_title_list", "id", PlayerPrefs.GetString("Click_Mail_ID"));
                LobbyManager.db_Server.updateInto("mail_unread_count", "count", (int.Parse(LobbyManager.db_Server.searchOneCondition("count", "mail_unread_count", "id", "=", "1")) - 1).ToString(), "id", "1");
                GameObject.Find("MailPanel(Clone)/MailButtonEvent").SendMessage("Start");
                gameObject.SendMessage("role_info_refresh");
                //開啟獲得框
                GameObject.Find("MessageGetItemPanel(Clone)/MessageGetItem_Grid").SendMessage("ReceiveGetItem", temp.ToArray());

                ServerUpdateType = "";
                break;
            case "Error":
                NGUITools.AddChild(GameObject.Find("Camera"), messageonebuttonPanel);
                GameObject.Find("MessageOneButtonPanel(Clone)/tv_Message").GetComponent<UILabel>().text = xmlStructLoad.string_Configuration.Strings.String.Find(x => x.Id == GlobalValue.Now_ret.ToString()).Desc;
                ServerUpdateType = "";
                break;
        }
	}

    string showXml(string ret)
    {
        TextAsset filepath = (TextAsset)Resources.Load("xml/" + "string", typeof(TextAsset));
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(filepath.text);
        XmlNodeList nodeList = xmldoc.SelectSingleNode("configuration/" + "strings").ChildNodes;

        foreach (XmlElement xe in nodeList)
        {
            if (xe.GetAttribute("id") == ret)
            {
                string data = xe.GetAttribute("desc");
                data = data.Replace("\\n", "\n");
                return "ret:" + ret + "\n" + data;
            }
        }
        return "ret:" + ret + "\n" + "沒有對應的string";
    }

    /// <summary>
    /// 英雄交換
    /// <para>id: team id</para>
    /// <para>id2: first hero id </para>
    /// <para>id3: second hero id</para>
    /// </summary>
    public static void do_switch_team_hero(int id, int id2, int id3)
    {
        do_Function.do_switch_team_hero_RootObject do_rootObject = new do_Function.do_switch_team_hero_RootObject();
        do_Function.do_switch_team_hero_Params do_parameArray = new do_Function.do_switch_team_hero_Params();
        do_parameArray.id = id;
        do_parameArray.id2 = id2;
        do_parameArray.id3 = id3;
        do_rootObject.@params = do_parameArray;
        string Json = JsonConvert.SerializeObject(do_rootObject);
        
        JFSocketNonblocking.GetInstance().sendToServer(NET_MSG_API.EM_SERVER_LOGIC_MSG_TYPE.SLMT_ACTION, Json, ((string content) =>
        { //傳送至Server   Server回傳結果
 
            if (content == null || content == "")
            {
                ServerUpdateType = "Disconnect";
                return;
            }

            get_do_Function.do_switch_team_hero_RootObject rootObject = JsonConvert.DeserializeObject<get_do_Function.do_switch_team_hero_RootObject>(content);

            if (rootObject.data.ret == 0)
            {
                //更新畫面
                ServerUpdateType = "do_switch_team_hero";
            }
            else
            {
                ServerUpdateType = "Error";
            }
            GlobalValue.Now_action_type = rootObject.action_type;
            GlobalValue.Now_ret = rootObject.data.ret;
        }));
    }
}
